using Discord.Commands.Builders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Commands
{
    [DebuggerDisplay("{Name,nq}")]
    public class CommandInfo
    {
        private static readonly System.Reflection.MethodInfo _convertParamsMethod = typeof(CommandInfo).GetTypeInfo().GetDeclaredMethod(nameof(ConvertParamsList));
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<object>, object>> _arrayConverters = new ConcurrentDictionary<Type, Func<IEnumerable<object>, object>>();

        private readonly Func<ICommandContext, object[], IServiceProvider, CommandInfo, Task> _action;

        public ModuleInfo Module { get; }
        public string Name { get; }
        public string Summary { get; }
        public string Remarks { get; }
        public int Priority { get; }
        public bool HasVarArgs { get; }
        public RunMode RunMode { get; }

        public IReadOnlyList<string> Aliases { get; }
        public IReadOnlyList<ParameterInfo> Parameters { get; }
        public IReadOnlyList<PreconditionAttribute> Preconditions { get; }
        public IReadOnlyList<Attribute> Attributes { get; }

        internal CommandInfo(CommandBuilder builder, ModuleInfo module, CommandService service)
        {
            Module = module;

            Name = builder.Name;
            Summary = builder.Summary;
            Remarks = builder.Remarks;

            RunMode = (builder.RunMode == RunMode.Default ? service._defaultRunMode : builder.RunMode);
            Priority = builder.Priority;

            Aliases = module.Aliases
                .Permutate(builder.Aliases, (first, second) =>
                {
                    if (first == "")
                        return second;
                    else if (second == "")
                        return first;
                    else
                        return first + service._separatorChar + second;
                })
                .Select(x => service._caseSensitive ? x : x.ToLowerInvariant())
                .ToImmutableArray();

            Preconditions = builder.Preconditions.ToImmutableArray();
            Attributes = builder.Attributes.ToImmutableArray();

            Parameters = builder.Parameters.Select(x => x.Build(this)).ToImmutableArray();
            HasVarArgs = builder.Parameters.Count > 0 ? builder.Parameters[builder.Parameters.Count - 1].IsMultiple : false;

            _action = builder.Callback;
        }

        public async Task<PreconditionResult> CheckPreconditionsAsync(ICommandContext context, IServiceProvider services = null)
        {
            services = services ?? EmptyServiceProvider.Instance;

            async Task<PreconditionResult> CheckGroups(IEnumerable<PreconditionAttribute> preconditions, string type)
            {
                foreach (IGrouping<string, PreconditionAttribute> preconditionGroup in preconditions.GroupBy(p => p.Group, StringComparer.Ordinal))
                {
                    if (preconditionGroup.Key == null)
                    {
                        foreach (PreconditionAttribute precondition in preconditionGroup)
                        {
                            var result = await precondition.CheckPermissions(context, this, services).ConfigureAwait(false);
                            if (!result.IsSuccess)
                                return result;
                        }
                    }
                    else
                    {
                        var results = new List<PreconditionResult>();
                        foreach (PreconditionAttribute precondition in preconditionGroup)
                            results.Add(await precondition.CheckPermissions(context, this, services).ConfigureAwait(false));

                        if (!results.Any(p => p.IsSuccess))
                            return PreconditionGroupResult.FromError($"{type} precondition group {preconditionGroup.Key} failed.", results);
                    }
                }
                return PreconditionGroupResult.FromSuccess();
            }

            var moduleResult = await CheckGroups(Module.Preconditions, "Module");
            if (!moduleResult.IsSuccess)
                return moduleResult;

            var commandResult = await CheckGroups(Preconditions, "Command");
            if (!commandResult.IsSuccess)
                return commandResult;

            return PreconditionResult.FromSuccess();
        }

        public async Task<ParseResult> ParseAsync(ICommandContext context, int startIndex, SearchResult searchResult, PreconditionResult preconditionResult = null, IServiceProvider services = null)
        {
            services = services ?? EmptyServiceProvider.Instance;

            if (!searchResult.IsSuccess)
                return ParseResult.FromError(searchResult);
            if (preconditionResult != null && !preconditionResult.IsSuccess)
                return ParseResult.FromError(preconditionResult);

            string input = searchResult.Text.Substring(startIndex);
            return await CommandParser.ParseArgs(this, context, services, input, 0).ConfigureAwait(false);
        }

        public Task<IResult> ExecuteAsync(ICommandContext context, ParseResult parseResult, IServiceProvider services)
        {
            if (!parseResult.IsSuccess)
                return Task.FromResult((IResult)ExecuteResult.FromError(parseResult));

            var argList = new object[parseResult.ArgValues.Count];
            for (int i = 0; i < parseResult.ArgValues.Count; i++)
            {
                if (!parseResult.ArgValues[i].IsSuccess)
                    return Task.FromResult((IResult)ExecuteResult.FromError(parseResult.ArgValues[i]));
                argList[i] = parseResult.ArgValues[i].Values.First().Value;
            }

            var paramList = new object[parseResult.ParamValues.Count];
            for (int i = 0; i < parseResult.ParamValues.Count; i++)
            {
                if (!parseResult.ParamValues[i].IsSuccess)
                    return Task.FromResult((IResult)ExecuteResult.FromError(parseResult.ParamValues[i]));
                paramList[i] = parseResult.ParamValues[i].Values.First().Value;
            }

            return ExecuteAsync(context, argList, paramList, services);
        }
        public async Task<IResult> ExecuteAsync(ICommandContext context, IEnumerable<object> argList, IEnumerable<object> paramList, IServiceProvider services)
        {
            services = services ?? EmptyServiceProvider.Instance;

            try
            {
                object[] args = GenerateArgs(argList, paramList);

                for (int position = 0; position < Parameters.Count; position++)
                {
                    var parameter = Parameters[position];
                    object argument = args[position];
                    var result = await parameter.CheckPreconditionsAsync(context, argument, services).ConfigureAwait(false);
                    if (!result.IsSuccess)
                        return ExecuteResult.FromError(result);
                }

                switch (RunMode)
                {
                    case RunMode.Sync: //Always sync
                        return await ExecuteAsyncInternal(context, args, services).ConfigureAwait(false);
                    case RunMode.Async: //Always async
                        var t2 = Task.Run(async () =>
                        {
                            await ExecuteAsyncInternal(context, args, services).ConfigureAwait(false);
                        });
                        break;
                }
                return ExecuteResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return ExecuteResult.FromError(ex);
            }
        }

        private async Task<IResult> ExecuteAsyncInternal(ICommandContext context, object[] args, IServiceProvider services)
        {
            await Module.Service._cmdLogger.DebugAsync($"Executing {GetLogText(context)}").ConfigureAwait(false);
            try
            {
                var task = _action(context, args, services, this);
                if (task is Task<IResult> resultTask)
                {
                    var result = await resultTask.ConfigureAwait(false);
                    if (result is RuntimeResult execResult)
                        return execResult;
                }
                else if (task is Task<ExecuteResult> execTask)
                {
                    return await execTask.ConfigureAwait(false);
                }
                else
                    await task.ConfigureAwait(false);

                return ExecuteResult.FromSuccess();
            }
            catch (Exception ex)
            {
                var originalEx = ex;
                while (ex is TargetInvocationException) //Happens with void-returning commands
                    ex = ex.InnerException;

                var wrappedEx = new CommandException(this, context, ex);
                await Module.Service._cmdLogger.ErrorAsync(wrappedEx).ConfigureAwait(false);
                if (Module.Service._throwOnError)
                {
                    if (ex == originalEx)
                        throw;
                    else
                        ExceptionDispatchInfo.Capture(ex).Throw();
                }

                return ExecuteResult.FromError(CommandError.Exception, ex.Message);
            }
            finally
            {
                await Module.Service._cmdLogger.VerboseAsync($"Executed {GetLogText(context)}").ConfigureAwait(false);
            }
        }

        private object[] GenerateArgs(IEnumerable<object> argList, IEnumerable<object> paramsList)
        {
            int argCount = Parameters.Count;
            var array = new object[Parameters.Count];
            if (HasVarArgs)
                argCount--;

            int i = 0;
            foreach (object arg in argList)
            {
                if (i == argCount)
                    throw new InvalidOperationException("Command was invoked with too many parameters");
                array[i++] = arg;
            }
            if (i < argCount)
                throw new InvalidOperationException("Command was invoked with too few parameters");

            if (HasVarArgs)
            {
                var func = _arrayConverters.GetOrAdd(Parameters[Parameters.Count - 1].Type, t =>
                {
                    var method = _convertParamsMethod.MakeGenericMethod(t);
                    return (Func<IEnumerable<object>, object>)method.CreateDelegate(typeof(Func<IEnumerable<object>, object>));
                });
                array[i] = func(paramsList);
            }

            return array;
        }

        private static T[] ConvertParamsList<T>(IEnumerable<object> paramsList)
            => paramsList.Cast<T>().ToArray();

        internal string GetLogText(ICommandContext context)
        {
            if (context.Guild != null)
                return $"\"{Name}\" for {context.User} in {context.Guild}/{context.Channel}";
            else
                return $"\"{Name}\" for {context.User} in {context.Channel}";
        }
    }
}
