// (Note: This precondition is obsolete, it is recommended to use the RequireOwnerAttribute that is bundled with Discord.Commands)

using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// Inherit from PreconditionAttribute
public class RequireOwnerAttribute : PreconditionAttribute
{
    // Override the CheckPermissions method
    public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        // Get the ID of the bot's owner
        var ownerId = (await services.GetService<DiscordSocketClient>().GetApplicationInfoAsync()).Owner.Id;
        // If this command was executed by that user, return a success
        if (context.User.Id == ownerId)
            return PreconditionResult.FromSuccess();
        // Since it wasn't, fail
        else
            return PreconditionResult.FromError("You must be the owner of the bot to run this command.");
    }
}
