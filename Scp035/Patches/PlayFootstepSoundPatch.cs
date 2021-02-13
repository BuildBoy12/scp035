namespace Scp035.Patches
{
    using API;
    using Exiled.API.Features;
    using HarmonyLib;

    [HarmonyPatch(typeof(FootstepSync), nameof(FootstepSync.PlayFootstepSound))]
    internal static class PlayFootstepSoundPatch
    {
        private static int _count;

        private static void Prefix(FootstepSync __instance)
        {
            if (!Scp035.Singleton.Config.CorrodeTrail)
                return;

            Player player = Player.Get(__instance.gameObject);
            _count++;

            foreach (var scp035 in Scp035Data.AllScp035)
            {
                if (player.Id == scp035?.Id && _count >= Scp035.Singleton.Config.CorrodeTrailInterval)
                {
                    player.ReferenceHub.characterClassManager.RpcPlaceBlood(player.Position, 1, 2f);
                    _count = 0;
                }
            }
        }
    }
}