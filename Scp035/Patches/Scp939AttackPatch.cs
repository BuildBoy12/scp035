namespace Scp035.Patches
{
    using API;
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
            if (player.IsScp035() && !Scp035.Singleton.Config.ScpFriendlyFire)
                player.DisableEffect(EffectType.Amnesia);
        }
    }
}