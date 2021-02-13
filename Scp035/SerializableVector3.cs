namespace Scp035
{
    using UnityEngine;

    public class SerializableVector3
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Vector3 ToVector3()
            => new Vector3(X, Y, Z);
    }
}