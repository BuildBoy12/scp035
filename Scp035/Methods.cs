namespace Scp035
{
    using Configs;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MEC;
    using Mirror;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = System.Random;

    public static class Methods
    {
        private static readonly Config Config = Scp035.Instance.Config;

        internal static readonly List<CoroutineHandle> CoroutineHandles = new List<CoroutineHandle>();
        internal static readonly List<string> FriendlyFireUsers = new List<string>();
        internal static readonly List<Pickup> ScpPickups = new List<Pickup>();
        internal static bool IsRotating;

        private static readonly Random Random = new Random();

        internal static IEnumerator<float> RunSpawning()
        {
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(Config.ItemSpawning.RotateInterval);
                Log.Debug($"Running {nameof(RunSpawning)} loop.", Config.Debug);
                if (IsRotating)
                    SpawnPickups(Config.ItemSpawning.InfectedItemCount);
            }
        }

        internal static List<Pickup> SpawnPickups(int amount)
        {
            Log.Debug($"Running {nameof(SpawnPickups)}.", Config.Debug);
            RemoveScpPickups();

            List<Pickup> pickups = Config.ItemSpawning.OnlyMimicSpawned ? Pickup.Instances.Where(pickup => !ScpPickups.Contains(pickup)).ToList() : Object.FindObjectsOfType<Pickup>().Where(pickup => !ScpPickups.Contains(pickup)).ToList();
            if (Warhead.IsDetonated)
            {
                pickups.RemoveAll(pickup => Map.FindParentRoom(pickup.gameObject).Type != RoomType.Surface);
            }

            List<Pickup> returnPickups = new List<Pickup>();
            for (int i = 0; i < amount; i++)
            {
                if (pickups.Count == 0)
                    return returnPickups;

                Pickup mimicAs = pickups[Random.Next(pickups.Count)];
                Transform transform = mimicAs.transform;
                Pickup scpPickup = Config.ItemSpawning
                    .PossibleItems[Random.Next(Config.ItemSpawning.PossibleItems.Length)]
                    .Spawn(0, transform.position, transform.rotation);
                Log.Debug($"Spawned Scp035 item with ItemType of {scpPickup.itemId} at {scpPickup.transform.position}");
                ScpPickups.Add(scpPickup);
                returnPickups.Add(scpPickup);

                pickups.Remove(mimicAs);
            }

            return returnPickups;
        }

        internal static IEnumerator<float> CorrodeHost(Player player)
        {
            while (player != null && player.IsAlive)
            {
                yield return Timing.WaitForSeconds(Config.CorrodeHost.Interval);
                Log.Debug($"Running {nameof(CorrodeHost)} loop.", Config.Debug);
                player.Hurt(Config.CorrodeHost.Damage);
            }
        }

        internal static IEnumerator<float> CorrodePlayers()
        {
            if (!Config.CorrodePlayers.IsEnabled)
                yield break;

            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(Config.CorrodePlayers.Interval);
                Log.Debug($"Running {nameof(CorrodePlayers)} loop.", Config.Debug);
                List<Player> scp035List = API.AllScp035.ToList();
                if (scp035List.Count == 0)
                    continue;

                List<Player> players = new List<Player>();
                foreach (var player in Player.List)
                {
                    if (!Config.ScpFriendlyFire && player.IsScp)
                        continue;

                    if (!Config.TutorialFriendlyFire && player.Role == RoleType.Tutorial)
                        continue;

                    if (API.IsScp035(player))
                        continue;

                    if (player.IsAlive)
                        players.Add(player);
                }

                foreach (var player in players)
                {
                    foreach (var scp035 in scp035List)
                    {
                        if (Vector3.Distance(scp035.Position, player.Position) <= Config.CorrodePlayers.Distance)
                        {
                            CorrodePlayer(player);
                        }
                    }
                }
            }
        }

        private static void CorrodePlayer(Player player)
        {
            player.Hurt(Config.CorrodePlayers.Damage, DamageTypes.Nuke);
            List<Player> scp035List = API.AllScp035.ToList();
            if (!Config.CorrodePlayers.LifeSteal || scp035List.IsEmpty())
                return;

            foreach (var scp035 in scp035List)
                HealPlayer(scp035, Config.CorrodePlayers.Damage);
        }

        private static void HealPlayer(Player player, int amount)
        {
            if (player.Health + amount > player.MaxHealth)
                player.Health = player.MaxHealth;
            else
                player.Health += amount;
        }

        private static void RemoveScpPickups()
        {
            foreach (var pickup in ScpPickups)
            {
                if (pickup.InUse)
                    pickup.Locked = true;

                if (pickup != null)
                    NetworkServer.Destroy(pickup.gameObject);
            }

            ScpPickups.Clear();
        }

        internal static void GrantFf(Player player)
        {
            player.IsFriendlyFireEnabled = true;
            FriendlyFireUsers.Add(player.UserId);
        }

        internal static void RemoveFf(Player player)
        {
            player.IsFriendlyFireEnabled = false;
            FriendlyFireUsers.Remove(player.UserId);
        }
    }
}