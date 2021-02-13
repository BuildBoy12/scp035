namespace Scp035
{
    using Configs;
    using Exiled.API.Features;
    using HarmonyLib;
    using System;
    
    public class Scp035 : Plugin<Config>
    {
        internal EventHandlers EventHandlers { get; private set; }
        internal static Scp035 Singleton { get; private set; }
        private Harmony _harmony;

        public override void OnEnabled()
        {
            Singleton = this;

            EventHandlers = new EventHandlers(Config);
            EventHandlers.SubscribeAll();
            
            _harmony = new Harmony(nameof(Scp035).ToLowerInvariant());
            _harmony.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventHandlers.UnSubscribeAll();
            EventHandlers = null;

            _harmony.UnpatchAll(_harmony.Id);

            Singleton = null;

            base.OnDisabled();
        }

        public override string Author { get; } = "Build";
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 30);
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}