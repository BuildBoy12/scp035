namespace Scp035.Configs.SubConfigs
{
    using Exiled.API.Features;

    public class Scp035Modifiers
    {
        public uint AmmoAmount { get; private set; } = 250;
        public bool CanHealBeyondHostHp { get; private set; } = true;
        public bool CanUseMedicalItems { get; private set; } = true;
        public int Health { get; private set; } = 300;
        public float RotateInterval { get; private set; } = 120f;
        public bool SelfInflict { get; private set; } = true;
        public SerializableVector3 Scale { get; private set; } = new SerializableVector3 { X = 1, Y = 1, Z = 1 };

        public Broadcast SpawnBroadcast { get; private set; }
            = new Broadcast(
                "<i>You have picked up <color=\"red\">SCP-035.</color> He has infected your body and is now in control of you.</i>");
    }
}