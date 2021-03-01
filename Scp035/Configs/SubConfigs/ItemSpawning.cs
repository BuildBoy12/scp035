namespace Scp035.Configs.SubConfigs
{
    public class ItemSpawning
    {
        public int InfectedItemCount { get; private set; } = 1;
        public float RotateInterval { get; private set; } = 30f;
        public bool OnlyMimicSpawned { get; private set; } = true;

        public ItemType[] PossibleItems { get; private set; } =
        {
            ItemType.Adrenaline, ItemType.Coin, ItemType.Disarmer, ItemType.Flashlight, ItemType.Medkit,
            ItemType.Painkillers, ItemType.Radio, ItemType.GrenadeFlash, ItemType.GrenadeFrag, ItemType.MicroHID
        };

        public bool SpawnAfterDeath { get; private set; } = false;
    }
}