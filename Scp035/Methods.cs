namespace Scp035
{
    using API;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MEC;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = System.Random;

    public partial class EventHandlers
    {
        private static readonly Random Random = new Random();

        private IEnumerator<float> RunSpawning()
        {
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(_config.Scp035Modifiers.RotateInterval);
                if (_isRotating)
                    SpawnPickup();
            }
        }

        private void SpawnPickup()
        {
            RemoveScpPickups();
            if (!_config.Scp035Modifiers.SelfInflict && AvailablePlayers().Count == 0)
                return;

            List<Pickup> pickups = Object.FindObjectsOfType<Pickup>().Where(pickup => !ScpPickups.Contains(pickup)).ToList();
            for (int i = 0; i < _config.ItemSpawning.InfectedItemCount; i++)
            {
                Pickup mimicAs = pickups[Random.Next(pickups.Count)];
                Transform transform = mimicAs.transform;
                Pickup scpPickup = _config.ItemSpawning.PossibleItems[Random.Next(_config.ItemSpawning.PossibleItems.Count)]
                    .Spawn(0, transform.position, transform.rotation);

                ScpPickups.Add(scpPickup);
                pickups.Remove(mimicAs);
            }
        }

        internal void AwakeScp035(Player player, Player toReplace = null)
        {
            Scp035Data.AllScp035.Add(player);
            _isRotating = false;

            if (toReplace != null && player != toReplace)
            {
                List<Inventory.SyncItemInfo> items = new List<Inventory.SyncItemInfo>();
                foreach (var item in toReplace.Inventory.items)
                    items.Add(item);

                Vector3 position = toReplace.Position;
                player.Role = toReplace.Role;
                player.ResetInventory(items);

                toReplace.Role = RoleType.Spectator;
                Timing.CallDelayed(0.5f, () => player.Position = position);
            }

            uint ammo = _config.Scp035Modifiers.AmmoAmount;
            player.Ammo[(int) AmmoType.Nato556] = ammo;
            player.Ammo[(int) AmmoType.Nato762] = ammo;
            player.Ammo[(int) AmmoType.Nato9] = ammo;

            player.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
            player.CustomInfo = "<color=#FF0000>SCP-035</color>";

            Scp096.TurnedPlayers.Add(player);
            Scp173.TurnedPlayers.Add(player);

            player.Health = player.MaxHealth = _config.Scp035Modifiers.Health;
            player.Scale = _config.Scp035Modifiers.Scale.ToVector3();
            player.Broadcast(_config.Scp035Modifiers.SpawnBroadcast);

            if (_config.CorrodeHost.IsEnabled)
                CoroutineHandles.Add(Timing.RunCoroutine(CorrodeHost(player)));
        }

        private void DestroyScp035(Player player)
        {
            Scp035Data.AllScp035.Remove(player);
            if (Scp035Data.AllScp035.Count == 0)
                _isRotating = true;
            
            Scp096.TurnedPlayers.Remove(player);
            Scp173.TurnedPlayers.Remove(player);
            player.CustomInfo = string.Empty;
            player.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;
            player.MaxHealth = player.ReferenceHub.characterClassManager.CurRole.maxHP;
            
            if (_config.ItemSpawning.SpawnAfterDeath)
                SpawnPickup();
        }

        private IEnumerator<float> CorrodeHost(Player player)
        {
            while (player != null && player.IsAlive)
            {
                player.Hurt(_config.CorrodeHost.Damage);
                yield return Timing.WaitForSeconds(_config.CorrodeHost.Interval);
            }
        }

        private IEnumerator<float> CorrodePlayers()
        {
            if (_config.CorrodePlayers.IsEnabled)
                yield break;

            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(_config.CorrodePlayers.Interval);
                if (Scp035Data.AllScp035.Count == 0)
                    continue;

                List<Player> players = new List<Player>();
                foreach (var player in Player.List)
                {
                    if (!_config.ScpFriendlyFire && player.IsScp)
                        continue;

                    if (!_config.TutorialFriendlyFire && player.Role == RoleType.Tutorial)
                        continue;

                    if (player.IsScp035())
                        continue;

                    if (player.IsAlive)
                        players.Add(player);
                }

                foreach (var player in players)
                {
                    foreach (var scp035 in Scp035Data.AllScp035)
                    {
                        if (Vector3.Distance(scp035.Position, player.Position) <= _config.CorrodePlayers.Distance)
                        {
                            CorrodePlayer(player);
                        }
                    }
                }
            }
        }

        private void CorrodePlayer(Player player)
        {
            player.Hurt(_config.CorrodePlayers.Damage, DamageTypes.Nuke);
            if (!_config.CorrodePlayers.LifeSteal || Scp035Data.AllScp035.Count == 0)
                return;

            foreach (var scp035 in Scp035Data.AllScp035)
                HealPlayer(scp035, _config.CorrodePlayers.Damage);
        }

        // Helper methods below this point

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
                if (pickup != null)
                    pickup.Delete();
            }

            ScpPickups.Clear();
        }

        private static void GrantFf(Player player)
        {
            player.IsFriendlyFireEnabled = true;
            FfPlayers.Add(player.UserId);
        }

        private static void RemoveFf(Player player)
        {
            player.IsFriendlyFireEnabled = false;
            FfPlayers.Remove(player.UserId);
        }

        private static void ExitPd(Player player)
        {
            if (!Warhead.IsDetonated)
                player.Position = RoleType.Scp096.GetRandomSpawnPoint();
            else
                player.Kill();
        }

        private static List<Player> AvailablePlayers()
        {
            return Player.Get(Team.RIP).Where(ply => !ply.IsOverwatchEnabled).ToList();
        }
    }
}