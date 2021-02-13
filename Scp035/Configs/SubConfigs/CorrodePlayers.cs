namespace Scp035.Configs.SubConfigs
{
    public class CorrodePlayers
    {
        public bool IsEnabled { get; private set; } = false;
        public int Damage { get; private set; } = 10;
        public float Distance { get; private set; } = 1.5f;
        public bool LifeSteal { get; private set; } = true;
        public float Interval { get; private set; } = 1f;
    }
}