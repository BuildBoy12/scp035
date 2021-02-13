namespace Scp035.API
{
    using Exiled.API.Features;
    using System.Collections.Generic;

    public static class Scp035Data
    {
        public static List<Player> AllScp035 { get; } = new List<Player>();

        public static void Spawn035(Player player)
        {
            Scp035.Singleton.EventHandlers.AwakeScp035(player);
        }

        public static bool IsScp035(this Player player)
        {
            return AllScp035.Contains(player);
        }
    }
}