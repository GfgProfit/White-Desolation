using System;

[Serializable]
public sealed class PlayerTransformSaveData
{
    public bool HasData;
    public bool HasCameraData;

    public SerializableVector3 Position;
    public SerializableQuaternion Rotation;
    public SerializableQuaternion CameraLocalRotation;
}
