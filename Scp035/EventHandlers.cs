namespace Scp035
{
    using API;
    using Configs;
    using Exiled.API.Enums;
    using Exiled.Events.EventArgs;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MEC;
    using System.Collections.Generic;
    using System.Linq;
    using PlayerHandlers = Exiled.Events.Handlers.Player;
    using Scp106Handlers = Exiled.Events.Handlers.Scp106;
    using ServerHandlers = Exiled.Events.Handlers.Server;

    public partial class EventHandlers
    {
        public EventHandlers(Config config) => _config = config;
        private readonly Config _config;

        private static readonly List<CoroutineHandle> CoroutineHandles = new List<CoroutineHandle>();
        private static readonly List<Pickup> ScpPickups = new List<Pickup>();
        private static readonly List<string> FfPlayers = new List<string>();
        private static bool _isRotating;

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsScp035())
                DestroyScp035(ev.Player);
        }

        private void OnContaining(ContainingEventArgs ev)
        {
            if (ev.Player.IsScp035() && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev.Target.IsScp035())
                DestroyScp035(ev.Target);
        }

        private void OnEndingRound(EndingRoundEventArgs ev)
        {
            List<Team> pList = Player.List.Where(x => !x.IsScp035()).Select(x => x.Team).ToList();
            bool scp035Exists = Scp035Data.AllScp035.Count > 0;
            
            if (!pList.Contains(Team.CHI) && !pList.Contains(Team.CDP) && !pList.Contains(Team.MTF) && !pList.Contains(Team.RSC) && (pList.Contains(Team.SCP) && scp035Exists || !pList.Contains(Team.SCP) && scp035Exists) ||
                _config.WinWithTutorials && !pList.Contains(Team.CHI) && !pList.Contains(Team.CDP) && !pList.Contains(Team.MTF) && !pList.Contains(Team.RSC) && pList.Contains(Team.TUT) && scp035Exists)
            {
                ev.LeadingTeam = LeadingTeam.Anomalies;
                ev.IsRoundEnded = true;
            }
            else if (scp035Exists && !pList.Contains(Team.SCP) && (pList.Contains(Team.CDP) || pList.Contains(Team.CHI) || pList.Contains(Team.MTF) || pList.Contains(Team.RSC)))
            {
                ev.IsAllowed = false;
            }
        }

        private static void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player.IsScp035())
                ev.IsAllowed = false;
        }

        private static void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (!ev.Player.IsScp035())
                return;

            ev.IsAllowed = false;
            ExitPd(ev.Player);
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (FfPlayers.Contains(ev.Attacker.UserId))
                RemoveFf(ev.Attacker);

            if (Scp035Data.AllScp035.Count == 0)
                return;

            if (!_config.ScpFriendlyFire &&
                (ev.Attacker.IsScp035() && ev.Target.Team == Team.SCP ||
                 ev.Target.IsScp035() && ev.Attacker.Team == Team.SCP))
            {
                ev.IsAllowed = false;
            }

            if (!_config.TutorialFriendlyFire &&
                (ev.Attacker.IsScp035() && ev.Target.Team == Team.TUT ||
                 ev.Target.IsScp035() && ev.Attacker.Team == Team.TUT))
            {
                ev.IsAllowed = false;
            }
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!ScpPickups.Contains(ev.Pickup))
                return;

            ev.IsAllowed = false;
            var players = AvailablePlayers();
            if (!_config.Scp035Modifiers.SelfInflict && players.Count == 0)
                return;

            ev.Pickup.Delete();
            AwakeScp035(ev.Player, _config.Scp035Modifiers.SelfInflict ? null : players[Random.Next(players.Count)]);
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (var coroutine in CoroutineHandles)
                Timing.KillCoroutines(coroutine);

            CoroutineHandles.Clear();
        }

        private void OnRoundStart()
        {
            _isRotating = true;
            ScpPickups.Clear();
            Scp035Data.AllScp035.Clear();

            CoroutineHandles.Add(Timing.RunCoroutine(CorrodePlayers()));
            CoroutineHandles.Add(Timing.RunCoroutine(RunSpawning()));
        }

        private void OnPocketDimensionEnter(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsScp035() && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private static void OnShooting(ShootingEventArgs ev)
        {
            if (ev.Target == null || Scp035Data.AllScp035.Count == 0)
                return;

            Player target = Player.Get(ev.Target);
            if (target == null)
                return;

            if (ev.Shooter.IsScp035() && target.Team == ev.Shooter.Team ||
                target.IsScp035() && ev.Shooter.Team == target.Team)
            {
                GrantFf(ev.Shooter);
            }

            if ((target.IsScp035() || ev.Shooter.IsScp035()) &&
                (target.Side == Side.ChaosInsurgency && ev.Shooter.Side == Side.ChaosInsurgency ||
                 target.Side == Side.Mtf && ev.Shooter.Side == Side.Mtf))
            {
                GrantFf(ev.Shooter);
            }
        }

        private void OnLeft(DestroyingEventArgs ev)
        {
            if (ev.Player.IsScp035())
                DestroyScp035(ev.Player);
        }

        private void OnInsertingGeneratorTablet(InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.Player.IsScp035() && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private static void OnFailingEscapePocketDimension(FailingEscapePocketDimensionEventArgs ev)
        {
            if (!ev.Player.IsScp035())
                return;

            ev.IsAllowed = false;
            ExitPd(ev.Player);
        }

        private void OnUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (!ev.Player.IsScp035())
                return;

            int maxHp = ev.Player.ReferenceHub.characterClassManager.CurRole.maxHP;
            if (!_config.Scp035Modifiers.CanHealBeyondHostHp &&
                ev.Player.Health > maxHp &&
                (ev.Item.IsMedical() || ev.Item == ItemType.SCP207))
            {
                if (ev.Item == ItemType.SCP207)
                    ev.Player.Health = UnityEngine.Mathf.Max(maxHp, ev.Player.Health - 30);
                else
                    ev.Player.Health = maxHp;
            }
        }

        private void OnUsingMedicalItem(UsingMedicalItemEventArgs ev)
        {
            if (!ev.Player.IsScp035())
                return;

            if (ev.Item.IsMedical() && (!_config.Scp035Modifiers.CanHealBeyondHostHp &&
                                        ev.Player.Health >=
                                        ev.Player.ReferenceHub.characterClassManager.CurRole.maxHP ||
                                        !_config.Scp035Modifiers.CanUseMedicalItems))
                ev.IsAllowed = false;
        }

        internal void SubscribeAll()
        {
            PlayerHandlers.ChangingRole += OnChangingRole;
            PlayerHandlers.Destroying += OnLeft;
            PlayerHandlers.Died += OnDied;
            PlayerHandlers.EnteringPocketDimension += OnPocketDimensionEnter;
            PlayerHandlers.Escaping += OnEscaping;
            PlayerHandlers.EscapingPocketDimension += OnEscapingPocketDimension;
            PlayerHandlers.FailingEscapePocketDimension += OnFailingEscapePocketDimension;
            PlayerHandlers.Hurting += OnHurting;
            PlayerHandlers.InsertingGeneratorTablet += OnInsertingGeneratorTablet;
            PlayerHandlers.MedicalItemUsed += OnUsedMedicalItem;
            PlayerHandlers.PickingUpItem += OnPickingUpItem;
            PlayerHandlers.Shooting += OnShooting;
            PlayerHandlers.UsingMedicalItem += OnUsingMedicalItem;

            Scp106Handlers.Containing += OnContaining;

            ServerHandlers.EndingRound += OnEndingRound;
            ServerHandlers.RoundEnded += OnRoundEnded;
            ServerHandlers.RoundStarted += OnRoundStart;
        }

        internal void UnSubscribeAll()
        {
            PlayerHandlers.ChangingRole -= OnChangingRole;
            PlayerHandlers.Destroying -= OnLeft;
            PlayerHandlers.Died -= OnDied;
            PlayerHandlers.EnteringPocketDimension -= OnPocketDimensionEnter;
            PlayerHandlers.Escaping -= OnEscaping;
            PlayerHandlers.EscapingPocketDimension -= OnEscapingPocketDimension;
            PlayerHandlers.FailingEscapePocketDimension -= OnFailingEscapePocketDimension;
            PlayerHandlers.Hurting -= OnHurting;
            PlayerHandlers.InsertingGeneratorTablet -= OnInsertingGeneratorTablet;
            PlayerHandlers.MedicalItemUsed -= OnUsedMedicalItem;
            PlayerHandlers.PickingUpItem -= OnPickingUpItem;
            PlayerHandlers.Shooting -= OnShooting;
            PlayerHandlers.UsingMedicalItem -= OnUsingMedicalItem;

            Scp106Handlers.Containing -= OnContaining;

            ServerHandlers.EndingRound -= OnEndingRound;
            ServerHandlers.RoundEnded -= OnRoundEnded;
            ServerHandlers.RoundStarted -= OnRoundStart;
        }
    }
}