using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public enum MessageType
{
    OnMessage,
    OnStart,
}

// 클라이언트 측 클래스
public class ClientManager : SingletonNet<ClientManager>
{
    [SerializeField] private TMP_InputField _input;
    [SerializeField] TMP_Text _message;
    [SerializeField] RelayManager relayManager;

    private string _joinCode;

    private void Start()
    {
        StartCoroutine(WaitNetWork());
    }

    private IEnumerator WaitNetWork()
    {
        yield return new WaitUntil(() => (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null));
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageType.OnMessage.ToString(), OnMessage);
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

    private void Update()
    {
        // 서버에서만 P 키를 눌렀을 때 클라이언트로 메시지 전송
        if (NetworkManager.Singleton.IsServer && Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 서버에서 클라이언트로 PingRpc 호출
            PingRpc(123);
        }

        // 클라이언트에서만 O 키를 눌렀을 때 서버로 메시지 전송
        if (NetworkManager.Singleton.IsClient && Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 클라이언트에서 서버로 PingRpc 호출
            PongRpc(123, "Asdasd");
        }
    }

    private void SendMessageAllClient(MessageType type, string message)
    {
        // 클라이언트에게 JoinCode 전송
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            SendMessageToClient(client.Key, type, message);
        }
    }

    private void SendMessageToClient(ulong clientId, MessageType type, string message)
    {
        // 문자열을 UTF-8 인코딩으로 변환하여 바이트 배열로 만듭니다.
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        int messageLength = messageBytes.Length;

        // 메시지 길이와 문자열을 저장할 버퍼 크기를 계산합니다.
        using FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp); // + sizeof(int)은 문자열 길이 공간을 위한 것

        // 먼저 문자열의 길이를 쓴 다음 바이트를 씁니다.
        if (writer.TryBeginWrite(messageLength))
        {
            // 문자열의 바이트 배열을 버퍼에 씁니다.
            writer.WriteValue(messageBytes);
            // 메시지를 클라이언트에게 전송합니다.
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(type.ToString(), clientId, writer);
        }
        else
        {
           Debug.LogError("SendMessageToClient type " + type);
        }
    }

    public void OnClickStartClient()
    {
        relayManager.StartClient();
    }

    private void OnMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        _message.text = "OnMessage " + message;
        Debug.Log(message);
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