namespace Scp035.Commands.SubCommands
{
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;
    using System;

    public class Spawn : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("035.spawn"))
            {
                response = "Insufficient permission. Required: 035.spawn";
                return false;
            }

            Player player = Player.Get((sender as PlayerCommandSender)?.ReferenceHub);
            if (arguments.Count > 0)
            {
                if (!(Player.Get(arguments.At(0)) is Player ply))
                {
                    response = "Could not find the referenced user.";
                    return false;
                }

                player = ply;
            }

            if (API.IsScp035(player))
            {
                response = $"{player.Nickname} is already a Scp035!";
                return false;
            }

            if (!player.IsAlive || player.IsScp)
                player.Role = RoleType.ClassD;

            API.Spawn035(player);
            response = $"Spawned {player.Nickname} as a Scp035.";
            return true;
        }

        public string Command { get; } = "spawn";
        public string[] Aliases { get; } = {"s"};
        public string Description { get; } = "Spawns a user as an instance of Scp035.";
    }
}