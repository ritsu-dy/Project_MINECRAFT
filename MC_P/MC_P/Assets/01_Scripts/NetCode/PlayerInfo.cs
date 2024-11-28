using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct PlayerInfo : INetworkSerializable
{
    public string Id;
    public string Name;
    public int Level;

    public PlayerInfo(string id, string name, int level)
    {
        Id = id;
        Name = name;
        Level = level;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Level);
    }
}