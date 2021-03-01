namespace Scp035
{
    using Components;
    using Exiled.API.Features;
    using System.Collections.Generic;
    using System.Linq;

    public static class API
    {
        public static IEnumerable<Player> AllScp035 => Player.List.Where(player => player.SessionVariables.ContainsKey("IsScp035"));
        
        public static bool IsScp035(Player player) => player.SessionVariables.ContainsKey("IsScp035");

        public static void Spawn035(Player player, Player toReplace = null) => player.GameObject.AddComponent<Scp035Component>().AwakeFunc(toReplace);

        public static List<Pickup> SpawnItems(int amount) => Methods.SpawnPickups(amount);
    }
}