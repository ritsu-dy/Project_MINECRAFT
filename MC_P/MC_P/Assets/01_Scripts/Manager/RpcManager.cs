using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public enum MessageName
{
    Message,
    Start,
    Chat_Message,
    Chat_Emoticon,
    Chat_File,
}

public class RpcManager : SingletonNet<RpcManager>
{
    [Rpc(SendTo.Server)]
    public void SendPlayerInfoRpc(PlayerInfo info)
    {
        Debug.Log("SendPlayerInfo");
        ReceivePlayInfoRpc(info);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReceivePlayInfoRpc(PlayerInfo info)
    {
        Debug.Log("PlayerInfo " + info.Name);
    }

    [Rpc(SendTo.Server)]
    public void SendChatRpc(string message)
    {
        Debug.Log("SendChat");
        ReceiveChatRpc(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ReceiveChatRpc(string message)
    {
        Debug.Log("ReceiveChat");
        ChatManager.Instance.ShowChat(message);
    }

    [Rpc(SendTo.Server)]
    public void PingRpc(int pingCount)
    {
        RelayManager.Instance.ShowClientText("pingCount " + pingCount);
        PongRpc(pingCount, "aaaabbbbb");
        Debug.Log("PingRpc");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(int pingCount, string message)
    {
        RelayManager.Instance.ShowClientText($"Received pong from server for ping {pingCount} and message {message}");
        Debug.Log($"Received pong from server for ping {pingCount} and message {message}");
    }

    int count = 0;
    private void Update()
    {
        if (NetworkManager.Singleton.IsServer && Input.GetKeyDown(KeyCode.Alpha1))
        {
            PingRpc(count++);
        }

        if (NetworkManager.Singleton.IsClient && Input.GetKeyDown(KeyCode.Alpha2))
        {
            PongRpc(count++, "asdasdad");
        }
    }
}