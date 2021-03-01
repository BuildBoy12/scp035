namespace Scp035.SerializableClasses
{
    using UnityEngine;
    
    public class Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3 ToVector3()
            => new Vector3(X, Y, Z);
    }
}