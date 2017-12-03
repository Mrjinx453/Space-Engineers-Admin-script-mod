﻿namespace midspace.adminscripts.Messages.Protection
{
    using midspace.adminscripts.Protection;
    using ProtoBuf;

    [ProtoContract]
    public class MessageSyncProtection : MessageBase
    {
        [ProtoMember(201)]
        public ProtectionConfig Config;

        public override void ProcessClient()
        {
            ProtectionHandler.InitOrUpdateClient(Config);
        }

        public override void ProcessServer()
        {
            Config = ProtectionHandler.Config;
            ConnectionHelper.SendMessageToPlayer(SenderSteamId, this);
        }
    }
}