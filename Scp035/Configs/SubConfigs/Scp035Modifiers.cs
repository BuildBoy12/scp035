namespace Scp035.Configs.SubConfigs
{
    using Exiled.API.Features;
    using SerializableClasses;
    
    public class Scp035Modifiers
    {
        public uint AmmoAmount { get; private set; } = 250;
        public bool CanHealBeyondHostHp { get; private set; } = true;
        public bool CanUseMedicalItems { get; private set; } = true;
        public int Health { get; private set; } = 300;
        public bool SelfInflict { get; private set; } = false;
        public Vector Scale { get; private set; } = new Vector {X = 1, Y = 1, Z = 1};

        public Broadcast SpawnBroadcast { get; private set; }
            = new Broadcast("<i>You have picked up <color=\"red\">SCP-035.</color> He has infected your body and is now in control of you.</i>");
    }
}