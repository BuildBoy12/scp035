namespace Scp035.Patches
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.CallCmdShoot))]
    internal static class Scp939AttackPatch
    {
        private static void Postfix(GameObject target)
        {
            Player player = Player.Get(target);
            if (API.IsScp035(player) && !Scp035.Instance.Config.ScpFriendlyFire)
                player.DisableEffect(EffectType.Amnesia);
        }
    }
}