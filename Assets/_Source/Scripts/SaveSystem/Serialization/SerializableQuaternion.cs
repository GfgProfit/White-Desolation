using System;
using UnityEngine;

[Serializable]
public struct SerializableQuaternion
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public SerializableQuaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public SerializableQuaternion(Quaternion quaternion)
    {
        X = quaternion.x;
        Y = quaternion.y;
        Z = quaternion.z;
        W = quaternion.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(X, Y, Z, W);
    }
}