using System;

[Serializable]
public sealed class PlayerTransformSaveData
{
    public bool HasData;

    public SerializableVector3 Position;
    public SerializableQuaternion Rotation;
}