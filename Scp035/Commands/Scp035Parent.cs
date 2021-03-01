namespace Scp035.Commands
{
    using CommandSystem;
    using NorthwoodLib.Pools;
    using SubCommands;
    using System;
    using System.Collections.Generic;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Scp035Parent : ParentCommand
    {
        public Scp035Parent() => LoadGeneratedCommands();

        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new Kill());
            RegisterCommand(new List());
            RegisterCommand(new Spawn());
            RegisterCommand(new SpawnItems());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> commands = ListPool<string>.Shared.Rent();
            foreach (var command in AllCommands)
                commands.Add(command.Command + (command.Aliases.Length > 0
                    ? $" | Aliases: {string.Join(", ", command.Aliases)}"
                    : string.Empty));

            response = $"Please enter a valid subcommand! Available:\n{string.Join(Environment.NewLine, commands)}";
            return false;
        }

        public override string Command { get; } = "035";
        public override string[] Aliases { get; } = Array.Empty<string>();
        public override string Description { get; } = "Parent command for Scp035";
    }
}