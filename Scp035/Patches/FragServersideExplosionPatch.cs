namespace Scp035.Patches
{
    using API;
    using Exiled.API.Features;
    using GameCore;
    using Grenades;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(FragGrenade), nameof(FragGrenade.ServersideExplosion))]
    internal static class FragGrenadeServersideExplosionPatch
    {
        private static void Postfix(FragGrenade __instance)
        {
            Player thrower = Player.Get(__instance.thrower.gameObject);
            foreach (var scp035 in Scp035Data.AllScp035)
            {
                if (thrower != null && scp035 != null && thrower.Id != scp035.Id && thrower.Team == scp035.Team)
                {
                    Vector3 position = __instance.transform.position;
                    PlayerStats component = scp035.ReferenceHub.playerStats;
                    float amount =
                        __instance.damageOverDistance.Evaluate(Vector3.Distance(position,
                            component.transform.position)) *
                        (component.ccm.IsHuman()
                            ? ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f)
                            : ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f));
                    if (amount > __instance.absoluteDamageFalloff)
                        component.HurtPlayer(
                            new PlayerStats.HitInfo(amount,
                                (__instance.thrower != null)
                                    ? __instance.thrower.hub.LoggedNameFromRefHub()
                                    : "(UNKNOWN)",
                                DamageTypes.Grenade, __instance.thrower.hub.queryProcessor.PlayerId),
                            scp035.GameObject);
                }
            }
        }
    }
}