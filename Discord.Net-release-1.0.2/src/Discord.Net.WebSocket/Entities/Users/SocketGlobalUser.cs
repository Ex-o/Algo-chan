﻿using System.Diagnostics;
using System.Linq;
using Model = Discord.API.User;
using PresenceModel = Discord.API.Presence;

namespace Discord.WebSocket
{
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    internal class SocketGlobalUser : SocketUser
    {
        public override bool IsBot { get; internal set; }
        public override string Username { get; internal set; }
        public override ushort DiscriminatorValue { get; internal set; }
        public override string AvatarId { get; internal set; }
        public SocketDMChannel DMChannel { get; internal set; }
        internal override SocketPresence Presence { get; set; }

        public override bool IsWebhook => false;
        internal override SocketGlobalUser GlobalUser => this;

        private readonly object _lockObj = new object();
        private ushort _references;

        private SocketGlobalUser(DiscordSocketClient discord, ulong id)
            : base(discord, id)
        {
        }
        internal static SocketGlobalUser Create(DiscordSocketClient discord, ClientState state, Model model)
        {
            var entity = new SocketGlobalUser(discord, model.Id);
            entity.Update(state, model);
            return entity;
        }

        internal void AddRef()
        {
            checked
            {
                lock (_lockObj)
                    _references++;
            }
        }
        internal void RemoveRef(DiscordSocketClient discord)
        {
            lock (_lockObj)
            {
                if (--_references <= 0)
                    discord.RemoveUser(Id);
            }
        }
        
        internal void Update(ClientState state, PresenceModel model)
        {
            Presence = SocketPresence.Create(model);
            DMChannel = state.DMChannels.FirstOrDefault(x => x.Recipient.Id == Id);
        }
        
        internal new SocketGlobalUser Clone() => MemberwiseClone() as SocketGlobalUser;
    }
}
