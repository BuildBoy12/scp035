namespace Scp035.Commands.SubCommands
{
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using NorthwoodLib.Pools;
    using System;
    using System.Text;

    public class SpawnItems : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("035.spawnitem"))
            {
                response = "Insufficient permission. Required: 035.spawnitem";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Syntax: 035 item <Amount>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int amount))
            {
                response = $"Could not parse \"{arguments.At(0)}\" as a number.";
                return false;
            }

            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent().AppendLine("Spawned Items:");
            foreach (Pickup item in API.SpawnItems(amount))
            {
                stringBuilder.AppendLine($"ItemType: {item.itemId} - Position: {item.transform.position}");
            }

            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder);
            return true;
        }

        public string Command { get; } = "spawnitems";
        public string[] Aliases { get; } = {"i", "item", "items"};
        public string Description { get; } = "Spawns the specified amount of Scp035 instanced items.";
    }
}