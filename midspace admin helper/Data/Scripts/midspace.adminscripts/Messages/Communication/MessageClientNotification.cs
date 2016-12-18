﻿using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game;

namespace midspace.adminscripts.Messages.Communication
{
    [ProtoContract]
    public class MessageClientNotification : MessageBase
    {
        [ProtoMember(1)]
        public string Message;

        [ProtoMember(2)]
        public int DisappearTimeMs;

        [ProtoMember(3)]
        public string Font;

        public override void ProcessClient()
        {
            MyAPIGateway.Utilities.ShowNotification(Message, DisappearTimeMs, Font);
        }

        public override void ProcessServer()
        {
            // never processed on server
        }

        public static void SendMessage(ulong steamId, string message, int disappearTimeMs = 2000, params object[] args)
        {
            SendMessage(steamId, message, MyFontEnum.White, disappearTimeMs, args);
        }

        public static void SendMessage(ulong steamId, string message, string font, int disappearTimeMs = 2000, params object[] args)
        {
            if (args != null && args.Length != 0)
                message = string.Format(message, args);

            ConnectionHelper.SendMessageToPlayer(steamId, new MessageClientNotification
            {
                Message = message,
                DisappearTimeMs = disappearTimeMs,
                Font = font
            });
        }
    }
}