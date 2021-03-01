namespace Scp035.EventHandlers
{
    using Components;
    using Configs;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Mirror;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class PlayerHandlers
    {
        private static readonly Config Config = Scp035.Instance.Config;

        private static readonly Random Random = new Random();

        internal static void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!Methods.ScpPickups.Contains(ev.Pickup))
                return;

            Log.Debug($"{ev.Player.Nickname} attempted to pickup a Scp035 object.", Config.Debug);
            ev.IsAllowed = false;
            if (API.IsScp035(ev.Player))
            {
                Log.Debug($"Pickup failed because {ev.Player.Nickname} is already a Scp035.", Config.Debug);
                return;
            }
            
            List<Player> players = Player.Get(Team.RIP).Where(ply => !ply.IsOverwatchEnabled).ToList();
            if (!Config.Scp035Modifiers.SelfInflict && players.Count == 0)
            {
                Log.Debug("There were no spectators to spawn Scp035 as, cancelling pickup.", Config.Debug);
                return;
            }

            NetworkServer.Destroy(ev.Pickup.gameObject);
            ev.Player.GameObject.AddComponent<Scp035Component>().AwakeFunc(Config.Scp035Modifiers.SelfInflict ? ev.Player : players[Random.Next(players.Count)]);
        }
        
        internal static void OnHurting(HurtingEventArgs ev)
        {
            if (Methods.FriendlyFireUsers.Contains(ev.Attacker.UserId))
                Methods.RemoveFf(ev.Attacker);

            if (!API.IsScp035(ev.Target) && !API.IsScp035(ev.Attacker))
                return;

            if (!Config.ScpFriendlyFire && (ev.Target.Team == Team.SCP || ev.Attacker.Team == Team.SCP))
            {
                ev.IsAllowed = false;
                return;
            }

            if (!Config.TutorialFriendlyFire && (ev.Target.Team == Team.TUT || ev.Attacker.Team == Team.TUT))
            {
                ev.IsAllowed = false;
                return;
            }

            if (ev.Attacker.Side == ev.Target.Side)
                Methods.GrantFf(ev.Attacker);
        }

        internal static void OnShooting(ShootingEventArgs ev)
        {
            if (Methods.FriendlyFireUsers.Contains(ev.Shooter.UserId))
                Methods.RemoveFf(ev.Shooter);

            if (ev.Target == null)
                return;

            Player target = Player.Get(ev.Target);
            if (target == null)
                return;

            if (!API.IsScp035(target) && !API.IsScp035(ev.Shooter))
                return;

            if (target.Side == ev.Shooter.Side)
            {
                Methods.GrantFf(ev.Shooter);
            }
        }
    }
}