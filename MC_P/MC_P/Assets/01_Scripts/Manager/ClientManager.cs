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
    [SerializeField] TMP_Text _message;
    [SerializeField] TMP_Text _messageText;
    [SerializeField] RelayManager relayManager;

    private string _joinCode;

    private void Start()
    {
        StartCoroutine(WaitNetWork());
    }

    private IEnumerator WaitNetWork()
    {
        yield return new WaitUntil(() => (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null));
        ClientManager.Instance.SetText("RegisterNamedMessageHandler init");
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.Message.ToString(), OnMessage);
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
        // 클라이언트로 메시지를 보내는 부분
        PongRpc(pingCount, "aaaabbbbb");
        Debug.Log("PingRpc");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PongRpc(int pingCount, string message)
    {
        RelayManager.Instance.RpcText.text = "PongRpc " + message;
        Debug.Log($"Received pong from server for ping {pingCount} and message {message}");
    }

    public void OnClickStartClient()
    {
        relayManager.StartClient();
    }

    public void SetText(string str)
    {
        _messageText.text = "Client " + str;
    }

    private void OnMessage(ulong clientId, FastBufferReader reader)
    {
        _message.text = "reader length " + reader.Length + " position " + reader.Position + reader.Position + " is " + reader.IsInitialized;
        
        //if (NetworkManager.Singleton.LocalClientId == clientId)
        //    return;

        try
        {
            reader.ReadValue(out string message);
            SetText("readvalue..... " + message);
            Debug.Log(message);
        }
        catch (Exception e)
        {
            SetText("OnMessage exception..... " + e);
            throw;
        }

        //if (reader.TryBeginRead(1024))
        //{
        //    reader.ReadValue(out string message);
        //    SetText("readvalue..... " + message);
        //    Debug.Log(message);
        //}
        //else
        //    SetText("OnMessage TryBeginRead.....false");
    }

    private void OnCretedJoinCode(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        _message.text = "receive join code " + message;
        Debug.Log(message);
        _joinCode = message;
    }

    private void OnClientJoined(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        _message.text = "client joined " + message;
        Debug.Log("클라이언트 조인 " + message);
    }

    private void OnReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        _message.text = message;
        Debug.Log(message);
    }

    public void SetJoinCode()
    {
        _joinCode = _input.text;
        _message.text = "code " + _input.text;
        Debug.Log("code " + _input.text);

    }

    public void OnClickJoinServer()
    {
        if (!string.IsNullOrEmpty(_joinCode))
            relayManager.ConnectToServerCoroutine(_joinCode);
        else
        {
            _message.text = "not joinCode";
            Debug.Log("not joinCode");
        }
    }
}