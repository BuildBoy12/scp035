namespace Scp035.Commands.SubCommands
{
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using System;

    public class Kill : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("035.kill"))
            {
                response = "Insufficient permission. Required: 035.kill";
                return false;
            }
            
            foreach (var player in API.AllScp035)
                player.Kill();

            response = "Killed all Scp035 users successfully.";
            return false;
        }

        public string Command { get; } = "kill";
        public string[] Aliases { get; } = {"k"};
        public string Description { get; } = "Kills all alive Scp035s.";
    }
}