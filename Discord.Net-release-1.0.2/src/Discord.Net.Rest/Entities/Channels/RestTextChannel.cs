﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Model = Discord.API.Channel;

namespace Discord.Rest
{
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public class RestTextChannel : RestGuildChannel, IRestMessageChannel, ITextChannel
    {
        public string Topic { get; private set; }

        public string Mention => MentionUtils.MentionChannel(Id);

        internal RestTextChannel(BaseDiscordClient discord, IGuild guild, ulong id)
            : base(discord, guild, id)
        {
        }
        internal new static RestTextChannel Create(BaseDiscordClient discord, IGuild guild, Model model)
        {
            var entity = new RestTextChannel(discord, guild, model.Id);
            entity.Update(model);
            return entity;
        }
        internal override void Update(Model model)
        {
            base.Update(model);

            Topic = model.Topic.Value;
        }

        public async Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
        {
            var model = await ChannelHelper.ModifyAsync(this, Discord, func, options).ConfigureAwait(false);
            Update(model);
        }

        public Task<RestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
            => ChannelHelper.GetUserAsync(this, Guild, Discord, id, options);
        public IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(RequestOptions options = null)
            => ChannelHelper.GetUsersAsync(this, Guild, Discord, null, null, options);

        public Task<RestMessage> GetMessageAsync(ulong id, RequestOptions options = null)
            => ChannelHelper.GetMessageAsync(this, Discord, id, options);
        public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(int limit = DiscordConfig.MaxMessagesPerBatch, RequestOptions options = null)
            => ChannelHelper.GetMessagesAsync(this, Discord, null, Direction.Before, limit, options);
        public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = DiscordConfig.MaxMessagesPerBatch, RequestOptions options = null)
            => ChannelHelper.GetMessagesAsync(this, Discord, fromMessageId, dir, limit, options);
        public IAsyncEnumerable<IReadOnlyCollection<RestMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = DiscordConfig.MaxMessagesPerBatch, RequestOptions options = null)
            => ChannelHelper.GetMessagesAsync(this, Discord, fromMessage.Id, dir, limit, options);
        public Task<IReadOnlyCollection<RestMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
            => ChannelHelper.GetPinnedMessagesAsync(this, Discord, options);

        public Task<RestUserMessage> SendMessageAsync(string text, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ChannelHelper.SendMessageAsync(this, Discord, text, isTTS, embed, options);
#if FILESYSTEM
        public Task<RestUserMessage> SendFileAsync(string filePath, string text, bool isTTS = false, RequestOptions options = null)
            => ChannelHelper.SendFileAsync(this, Discord, filePath, text, isTTS, options);
#endif
        public Task<RestUserMessage> SendFileAsync(Stream stream, string filename, string text, bool isTTS = false, RequestOptions options = null)
            => ChannelHelper.SendFileAsync(this, Discord, stream, filename, text, isTTS, options);

        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
            => ChannelHelper.DeleteMessagesAsync(this, Discord, messages.Select(x => x.Id), options);
        public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
            => ChannelHelper.DeleteMessagesAsync(this, Discord, messageIds, options);

        public Task TriggerTypingAsync(RequestOptions options = null)
            => ChannelHelper.TriggerTypingAsync(this, Discord, options);
        public IDisposable EnterTypingState(RequestOptions options = null)
            => ChannelHelper.EnterTypingState(this, Discord, options);

        private string DebuggerDisplay => $"{Name} ({Id}, Text)";

        //IMessageChannel
        async Task<IMessage> IMessageChannel.GetMessageAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetMessageAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(int limit, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return GetMessagesAsync(limit, options);
            else
                return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
        }
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(ulong fromMessageId, Direction dir, int limit, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return GetMessagesAsync(fromMessageId, dir, limit, options);
            else
                return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
        }
        IAsyncEnumerable<IReadOnlyCollection<IMessage>> IMessageChannel.GetMessagesAsync(IMessage fromMessage, Direction dir, int limit, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return GetMessagesAsync(fromMessage, dir, limit, options);
            else
                return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
        }
        async Task<IReadOnlyCollection<IMessage>> IMessageChannel.GetPinnedMessagesAsync(RequestOptions options) 
            => await GetPinnedMessagesAsync(options).ConfigureAwait(false);

#if FILESYSTEM
        async Task<IUserMessage> IMessageChannel.SendFileAsync(string filePath, string text, bool isTTS, RequestOptions options)
            => await SendFileAsync(filePath, text, isTTS, options).ConfigureAwait(false);
#endif
        async Task<IUserMessage> IMessageChannel.SendFileAsync(Stream stream, string filename, string text, bool isTTS, RequestOptions options) 
            => await SendFileAsync(stream, filename, text, isTTS, options).ConfigureAwait(false);
        async Task<IUserMessage> IMessageChannel.SendMessageAsync(string text, bool isTTS, Embed embed, RequestOptions options) 
            => await SendMessageAsync(text, isTTS, embed, options).ConfigureAwait(false);
        IDisposable IMessageChannel.EnterTypingState(RequestOptions options) 
            => EnterTypingState(options);

        //IGuildChannel
        async Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetUserAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return GetUsersAsync(options);
            else
                return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
        }

        //IChannel
        async Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetUserAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return GetUsersAsync(options);
            else
                return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
        }
    }
}
