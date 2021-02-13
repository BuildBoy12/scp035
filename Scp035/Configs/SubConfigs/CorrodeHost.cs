namespace Scp035.Configs.SubConfigs
{
    public class CorrodeHost
    {
        public bool IsEnabled { get; private set; } = false;
        public int Damage { get; private set; } = 5;
        public float Interval { get; private set; } = 6f;
    }
}