namespace Scp035.Configs.SubConfigs
{
    using System.Collections.Generic;

    public class ItemSpawning
    {
        public int InfectedItemCount { get; private set; } = 1;

        public List<ItemType> PossibleItems { get; private set; } = new List<ItemType>
        {
            ItemType.Adrenaline, ItemType.Coin, ItemType.Disarmer, ItemType.Flashlight, ItemType.Medkit,
            ItemType.Painkillers, ItemType.Radio, ItemType.GrenadeFlash, ItemType.GrenadeFrag, ItemType.MicroHID
        };

        public bool SpawnAfterDeath { get; private set; } = false;
    }
}