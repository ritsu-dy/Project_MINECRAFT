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

// Ŭ���̾�Ʈ �� Ŭ����
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
        // Ŭ���̾�Ʈ�� �޽����� ������ �κ�
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
        // ���������� P Ű�� ������ �� Ŭ���̾�Ʈ�� �޽��� ����
        if (NetworkManager.Singleton.IsServer && Input.GetKeyDown(KeyCode.Alpha1))
        {
            // �������� Ŭ���̾�Ʈ�� PingRpc ȣ��
            PingRpc(123);
        }

        // Ŭ���̾�Ʈ������ O Ű�� ������ �� ������ �޽��� ����
        if (NetworkManager.Singleton.IsClient && Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Ŭ���̾�Ʈ���� ������ PingRpc ȣ��
            PongRpc(123, "Asdasd");
        }
    }

    private void SendMessageAllClient(MessageType type, string message)
    {
        // Ŭ���̾�Ʈ���� JoinCode ����
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            SendMessageToClient(client.Key, type, message);
        }
    }

    private void SendMessageToClient(ulong clientId, MessageType type, string message)
    {
        // ���ڿ��� UTF-8 ���ڵ����� ��ȯ�Ͽ� ����Ʈ �迭�� ����ϴ�.
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        int messageLength = messageBytes.Length;

        // �޽��� ���̿� ���ڿ��� ������ ���� ũ�⸦ ����մϴ�.
        using FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp); // + sizeof(int)�� ���ڿ� ���� ������ ���� ��

        // ���� ���ڿ��� ���̸� �� ���� ����Ʈ�� ���ϴ�.
        if (writer.TryBeginWrite(messageLength))
        {
            // ���ڿ��� ����Ʈ �迭�� ���ۿ� ���ϴ�.
            writer.WriteValue(messageBytes);
            // �޽����� Ŭ���̾�Ʈ���� �����մϴ�.
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
        Debug.Log("Ŭ���̾�Ʈ ���� " + message);
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