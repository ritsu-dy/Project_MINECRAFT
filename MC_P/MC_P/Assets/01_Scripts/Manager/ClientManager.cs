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
    Chat,
}

// 클라이언트 측 클래스
public class ClientManager : SingletonNet<ClientManager>
{
    [SerializeField] private TMP_InputField _input;
    [SerializeField] RelayManager relayManager;

    private string _joinCode;

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
        // 클라이언트로 메시지를 보내는 부분
        PongRpc(pingCount, "aaaabbbbb");
        Debug.Log("PingRpc");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(int pingCount, string message)
    {
    }

    public void OnClickStartClient()
    {
        relayManager.StartClient();
    }

    private void OnMessage(ulong clientId, FastBufferReader reader)
    {
        try
        {
            reader.ReadValueSafe(out string message);
            Debug.Log(message);
        }
        catch (Exception e)
        {
            Debug.Log("OnMessage exception..... " + e);
            throw;
        }
    }

    private void OnCretedJoinCode(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        Debug.Log(message);
        _joinCode = message;
    }

    private void OnClientJoined(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log("클라이언트 조인 " + message);
    }

    private void OnReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        Debug.Log(message);
    }

    public void SetJoinCode()
    {
        _joinCode = _input.text;
        Debug.Log("code " + _input.text);
    }

    public void OnClickJoinServer()
    {
        if (!string.IsNullOrEmpty(_joinCode))
            relayManager.ConnectToServerCoroutine(_joinCode);
        else
        {
            Debug.Log("not joinCode");
        }
    }
}