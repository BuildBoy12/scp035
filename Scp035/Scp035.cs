namespace Scp035
{
    using Configs;
    using EventHandlers;
    using Exiled.API.Features;
    using HarmonyLib;
    using MEC;
    using System;

    public class Scp035 : Plugin<Config>
    {
        private static readonly Scp035 InstanceValue = new Scp035();
        private static Harmony _harmony;

        private Scp035()
        {
        }

        public static Scp035 Instance { get; } = InstanceValue;

        public override void OnEnabled()
        {
            SubscribeAll();
            _harmony = new Harmony(nameof(Scp035).ToLowerInvariant());
            _harmony.PatchAll();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            UnSubscribeAll();
            Timing.KillCoroutines(Methods.CoroutineHandles.ToArray());
            Methods.CoroutineHandles.Clear();
            _harmony.UnpatchAll(_harmony.Id);
            base.OnDisabled();
        }

        public override string Author { get; } = "Build";
        public override Version RequiredExiledVersion { get; } = new Version(2, 3, 4);
        public override Version Version { get; } = new Version(2, 0, 2);

        private static void SubscribeAll()
        {
            Exiled.Events.Handlers.Player.Hurting += PlayerHandlers.OnHurting;
            Exiled.Events.Handlers.Player.PickingUpItem += PlayerHandlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.Shooting += PlayerHandlers.OnShooting;
            
            Exiled.Events.Handlers.Server.RoundStarted += ServerHandlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;
        }

        private static void UnSubscribeAll()
        {
            Exiled.Events.Handlers.Player.Hurting -= PlayerHandlers.OnHurting;
            Exiled.Events.Handlers.Player.PickingUpItem -= PlayerHandlers.OnPickingUpItem;
            Exiled.Events.Handlers.Player.Shooting -= PlayerHandlers.OnShooting;
            
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;
        }
    }
}