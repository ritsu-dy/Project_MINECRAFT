using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MessageManager : SingletonNet<MessageManager>
{
    [SerializeField] private UIGroupChat _uiGroupChat;

    public Action<ulong, string> OnChatEvent;

    private void Start()
    {
        StartCoroutine(WaitNetWork());
    }

    private IEnumerator WaitNetWork()
    {
        yield return new WaitUntil(() => (NetworkManager.Singleton != null && NetworkManager.Singleton.CustomMessagingManager != null));
        SetupEvent();
    }

    public void SetupEvent()
    {
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.Message.ToString(), OnMessage);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName.Chat.ToString(), OnChat);
    }

    public void SendMessageToAllClient(string message, MessageName messageName = MessageName.Message)
    {
        using FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp, 256);
        writer.WriteValueSafe(message);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(messageName.ToString(), writer, NetworkDelivery.ReliableFragmentedSequenced);
    }

    public void SendMessageToClient(ulong clientId, string message, MessageName messageName = MessageName.Message)
    {
        using FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp, 256);
        writer.WriteValueSafe(message);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageName.ToString(), clientId, writer);
    }

    public void OnChat(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log(message);

        var isMy = NetworkManager.Singleton.LocalClientId == clientId;

        _uiGroupChat.ShowChat(message, ChatType.Message, isMy);
        OnChatEvent?.Invoke(clientId, message);
    }

    private void OnMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string message);
        Debug.Log(message);
    }
}
