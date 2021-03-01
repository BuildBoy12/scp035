namespace Scp035.Configs
{
    using Exiled.API.Interfaces;
    using SubConfigs;

    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; private set; }
        public bool CorrodeTrail { get; private set; }
        public int CorrodeTrailInterval { get; private set; } = 5;
        public bool ScpFriendlyFire { get; private set; }
        public bool TutorialFriendlyFire { get; private set; } = true;
        public CorrodeHost CorrodeHost { get; private set; } = new CorrodeHost();
        public CorrodePlayers CorrodePlayers { get; private set; } = new CorrodePlayers();
        public ItemSpawning ItemSpawning { get; private set; } = new ItemSpawning();
        public Scp035Modifiers Scp035Modifiers { get; private set; } = new Scp035Modifiers();
    }
}