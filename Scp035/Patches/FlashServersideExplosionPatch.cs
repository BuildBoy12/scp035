namespace Scp035.Patches
{
    using Exiled.API.Features;
    using Grenades;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(FlashGrenade), nameof(FlashGrenade.ServersideExplosion))]
    internal static class FlashGrenadeServersideExplosionPatch
    {
        private static void Postfix(FlashGrenade __instance)
        {
            Player thrower = Player.Get(__instance.thrower.gameObject);
            foreach (var scp035 in API.AllScp035)
            {
                if (thrower != null && scp035 != null && thrower.Id != scp035.Id && thrower.Team == scp035.Team)
                {
                    GameObject gameObject = scp035.GameObject;
                    Vector3 position = __instance.transform.position;
                    ReferenceHub hub = scp035.ReferenceHub;
                    CustomPlayerEffects.Flashed effect =
                        hub.playerEffectsController.GetEffect<CustomPlayerEffects.Flashed>();
                    CustomPlayerEffects.Deafened effect2 =
                        hub.playerEffectsController.GetEffect<CustomPlayerEffects.Deafened>();
                    if (effect != null && __instance.thrower != null && Flashable(thrower.ReferenceHub,
                        scp035.ReferenceHub,
                        position, __instance._ignoredLayers))
                    {
                        float num = __instance.powerOverDistance.Evaluate(
                            Vector3.Distance(gameObject.transform.position, position) / ((position.y > 900f)
                                ? __instance.distanceMultiplierSurface
                                : __instance.distanceMultiplierFacility)) * __instance.powerOverDot.Evaluate(
                            Vector3.Dot(hub.PlayerCameraReference.forward,
                                (hub.PlayerCameraReference.position - position).normalized));
                        byte b = (byte) Mathf.Clamp(Mathf.RoundToInt(num * 10f * __instance.maximumDuration), 1,
                            255);
                        if (b >= effect.Intensity && num > 0f)
                        {
                            hub.playerEffectsController.ChangeEffectIntensity<CustomPlayerEffects.Flashed>(b);
                            if (effect2 != null)
                            {
                                hub.playerEffectsController.EnableEffect(effect2, num * __instance.maximumDuration,
                                    true);
                            }
                        }
                    }
                }
            }
        }

        private static bool Flashable(ReferenceHub throwerPlayerHub, ReferenceHub targetPlayerHub,
            Vector3 sourcePosition, int ignoreMask)
        {
            return targetPlayerHub != throwerPlayerHub && !Physics.Linecast(sourcePosition,
                targetPlayerHub.PlayerCameraReference.position, ignoreMask);
        }
    }
}