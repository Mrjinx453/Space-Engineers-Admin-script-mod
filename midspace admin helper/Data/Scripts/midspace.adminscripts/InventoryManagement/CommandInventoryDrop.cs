﻿namespace midspace.adminscripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using midspace.adminscripts.Messages.Sync;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class CommandInventoryDrop : ChatCommand
    {
        private readonly string[] _oreNames;
        private readonly string[] _ingotNames;
        private readonly MyPhysicalItemDefinition[] _physicalItems;
        private readonly string[] _physicalItemNames;

        public CommandInventoryDrop(string[] oreNames, string[] ingotNames, MyPhysicalItemDefinition[] physicalItems)
            : base(ChatCommandSecurity.Admin, "drop", new[] { "/drop" })
        {
            _oreNames = oreNames;
            _ingotNames = ingotNames;
            _physicalItems = physicalItems;

            // Make sure all Public Physical item names are unique, so they can be properly searched for.
            var names = new List<string>();
            foreach (var item in _physicalItems)
            {
                var baseName = item.DisplayNameEnum.HasValue ? item.DisplayNameEnum.Value.GetString() : item.DisplayNameString;
                var uniqueName = baseName;
                var index = 1;
                while (names.Contains(uniqueName, StringComparer.InvariantCultureIgnoreCase))
                {
                    index++;
                    uniqueName = string.Format("{0}{1}", baseName, index);
                }
                names.Add(uniqueName);
            }
            _physicalItemNames = names.ToArray();
        }

        public override void Help(ulong steamId, bool brief)
        {
            MyAPIGateway.Utilities.ShowMessage("/drop <type name|name> <amount>", "Drop a specified item (ore, ingot, or item), <name>, <amount>. ie, \"ore gold 98.23\", \"steel plate 25\"");
        }

        public override bool Invoke(ulong steamId, long playerId, string messageText)
        {
            MyObjectBuilder_Base content = null;
            string[] options;
            decimal amount = 1;

            var match = Regex.Match(messageText, @"/drop\s{1,}(?:(?<Key>.+)\s(?<Value>[+-]?((\d+(\.\d*)?)|(\.\d+)))|(?<Key>.+))", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var itemName = match.Groups["Key"].Value;
                var strAmount = match.Groups["Value"].Value;
                if (!decimal.TryParse(strAmount, out amount))
                    amount = 1;

                if (!Support.FindPhysicalParts(_oreNames, _ingotNames, _physicalItemNames, _physicalItems, itemName, out content, out options) && options.Length > 0)
                {
                    MyAPIGateway.Utilities.ShowMessage("Did you mean", String.Join(", ", options) + " ?");
                    return true;
                }
            }

            if (content != null)
            {
                if (amount < 0)
                    amount = 1;

                if (content.TypeId != typeof(MyObjectBuilder_Ore) && content.TypeId != typeof(MyObjectBuilder_Ingot))
                {
                    // must be whole numbers.
                    amount = Math.Round(amount, 0);
                }

                var worldMatrix = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.WorldMatrix;
                Vector3D position;
                if (MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity.Parent == null)
                    position = worldMatrix.Translation + worldMatrix.Forward * 1.5f + worldMatrix.Up * 1.5f; // Spawn item 1.5m in front of player.
                else
                    position = worldMatrix.Translation + worldMatrix.Forward * 1.5f; // Spawn item 1.5m in front of player in cockpit.

                if (!MyAPIGateway.Multiplayer.MultiplayerActive)
                {
                    var id = new MyDefinitionId(content.TypeId, content.SubtypeName);
                    var definition = MyDefinitionManager.Static.GetDefinition(id);
                    Support.InventoryDrop(position, (MyFixedPoint)amount, definition.Id);
                }
                else
                    ConnectionHelper.SendMessageToServer(new MessageSyncCreateObject()
                    {
                        Type = SyncCreateObjectType.Floating,
                        TypeId = content.TypeId.ToString(),
                        SubtypeName = content.SubtypeName,
                        Amount = amount,
                        Position = position
                    });


                return true;
            }

            MyAPIGateway.Utilities.ShowMessage("Unknown Item", "Could not find the specified name.");
            return true;
        }
    }
}
