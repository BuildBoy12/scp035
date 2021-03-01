namespace Scp035.Components
{
    using Configs;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using System.Collections.Generic;
    using UnityEngine;

    public class Scp035Component : MonoBehaviour
    {
        private static Config _config;
        private static Player _player;

        internal void AwakeFunc(Player toReplace)
        {
            _player = Player.Get(gameObject);
            if (_player == null)
            {
                Destroy();
                return;
            }

            _config = Scp035.Instance.Config;
            SubscribeEvents();
            
            Methods.IsRotating = false;

            if (toReplace != null && _player != toReplace)
            {
                List<Inventory.SyncItemInfo> items = new List<Inventory.SyncItemInfo>(toReplace.Inventory.items);

                Vector3 position = toReplace.Position;
                _player.Role = toReplace.Role;
                _player.ResetInventory(items);

                toReplace.Role = RoleType.Spectator;
                Timing.CallDelayed(0.5f, () => _player.Position = position);
            }

            uint ammo = _config.Scp035Modifiers.AmmoAmount;
            _player.Ammo[(int) AmmoType.Nato556] = ammo;
            _player.Ammo[(int) AmmoType.Nato762] = ammo;
            _player.Ammo[(int) AmmoType.Nato9] = ammo;

            _player.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
            _player.CustomInfo = "<color=#FF0000>SCP-035</color>";

            _player.SessionVariables.Add("IsScp035", true);

            Scp096.TurnedPlayers.Add(_player);
            Scp173.TurnedPlayers.Add(_player);

            _player.Health = _player.MaxHealth = _config.Scp035Modifiers.Health;

            Vector3 scale = _config.Scp035Modifiers.Scale.ToVector3();
            if (_player.Scale != scale)
                _player.Scale = scale;

            if (_config.Scp035Modifiers.SpawnBroadcast.Show)
                _player.Broadcast(_config.Scp035Modifiers.SpawnBroadcast);

            if (_config.CorrodeHost.IsEnabled)
                Methods.CoroutineHandles.Add(Timing.RunCoroutine(Methods.CorrodeHost(_player)));
        }

        private void Destroy()
        {
            _player.SessionVariables.Remove("IsScp035");
            if (API.AllScp035.IsEmpty())
                Methods.IsRotating = true;

            Scp096.TurnedPlayers.Remove(_player);
            Scp173.TurnedPlayers.Remove(_player);

            _player.CustomInfo = string.Empty;
            _player.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;

            _player.MaxHealth = _player.ReferenceHub.characterClassManager.CurRole.maxHP;

            if (_config.ItemSpawning.SpawnAfterDeath)
                Methods.SpawnPickups(_config.ItemSpawning.InfectedItemCount);

            UnSubscribeEvents();
            Destroy(this);
        }

        private void CheckDestroy(Player player)
        {
            if (player == _player)
                Destroy();
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            CheckDestroy(ev.Player);
        }

        private void OnDestroying(DestroyingEventArgs ev)
        {
            CheckDestroy(ev.Player);
        }

        private void OnDied(DiedEventArgs ev)
        {
            CheckDestroy(ev.Target);
        }

        private static void OnContaining(ContainingEventArgs ev)
        {
            if (ev.Player == _player && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private static void OnPocketDimensionEnter(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player == _player && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private static void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player == _player)
                ev.IsAllowed = false;
        }

        private static void OnInsertingGeneratorTablet(InsertingGeneratorTabletEventArgs ev)
        {
            if (_player == ev.Player && !_config.ScpFriendlyFire)
                ev.IsAllowed = false;
        }

        private static void OnMedicalItemUsed(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player != _player)
                return;

            int maxHp = ev.Player.ReferenceHub.characterClassManager.CurRole.maxHP;
            if (!_config.Scp035Modifiers.CanHealBeyondHostHp &&
                ev.Player.Health > maxHp &&
                (ev.Item.IsMedical() || ev.Item == ItemType.SCP207))
            {
                if (ev.Item == ItemType.SCP207)
                    ev.Player.Health = Mathf.Max(maxHp, ev.Player.Health - 30);
                else
                    ev.Player.Health = maxHp;
            }
        }

        private static void OnUsingMedicalItem(UsingMedicalItemEventArgs ev)
        {
            if (ev.Player != _player)
                return;

            if (ev.Item.IsMedical() && (!_config.Scp035Modifiers.CanHealBeyondHostHp &&
                                        ev.Player.Health >=
                                        ev.Player.ReferenceHub.characterClassManager.CurRole.maxHP ||
                                        !_config.Scp035Modifiers.CanUseMedicalItems))
                ev.IsAllowed = false;
        }

        private void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Scp106.Containing += OnContaining;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += OnPocketDimensionEnter;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.Escaping += OnEscaping;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += OnInsertingGeneratorTablet;
            Exiled.Events.Handlers.Player.MedicalItemUsed += OnMedicalItemUsed;
            Exiled.Events.Handlers.Player.UsingMedicalItem += OnUsingMedicalItem;
        }

        private void UnSubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Scp106.Containing -= OnContaining;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= OnPocketDimensionEnter;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.Escaping -= OnEscaping;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= OnInsertingGeneratorTablet;
            Exiled.Events.Handlers.Player.MedicalItemUsed -= OnMedicalItemUsed;
            Exiled.Events.Handlers.Player.UsingMedicalItem -= OnUsingMedicalItem;
        }
    }
}