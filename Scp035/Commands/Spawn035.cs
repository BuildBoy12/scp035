namespace Scp035.Commands
{
    using API;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using System;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Spawn035 : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("sp.spawn035"))
            {
                response = "Insufficient permission. Required: sp.spawn035";
                return false;
            }

            if (arguments.Count != 2)
            {
                response = "Syntax: spawn035 <Player> <Role>";
                return false;
            }

            Player player = Player.Get(arguments.At(0));
            if (player == null || !Enum.TryParse(arguments.At(1), true, out RoleType roleType))
            {
                response = "Syntax: spawn035 <Player> <Role>";
                return false;
            }

            player.Role = roleType;
            Scp035Data.Spawn035(player);
            response = $"Spawned {player.Nickname} as a Scp035 with the role {roleType}.";
            return true;
        }

        public string Command => "spawn035";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Spawns the specified user as a Scp035.";
    }
}