namespace Scp035.Commands.SubCommands
{
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using System;
    using System.Linq;

    public class List : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("035.list"))
            {
                response = "Insufficient permission. Required: 035.list";
                return false;
            }
            
            response = $"Alive Scp035 Instances: {string.Join(", ", API.AllScp035.Select(player => player.Nickname))}";
            return true;
        }

        public string Command { get; } = "list";
        public string[] Aliases { get; } = {"l"};
        public string Description { get; } = "Lists all active Scp035 instances.";
    }
}