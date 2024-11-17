using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MessageManager : SingletonNet<MessageManager>
{
    [SerializeField] private UIGroupChat _uiGroupChat;

    public Action<ulong, string> OnChatEvent;

    public void Show()
    {
        _uiGroupChat.Show();
    }

    public void SetupEvent()
    {
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
        ClientManager.Instance.SetText("onchat:");
        reader.ReadValue(out string message);
        Debug.Log(message);

        var isMy = NetworkManager.Singleton.LocalClientId == clientId;
        ClientManager.Instance.SetText("onchat: ShowChat");

        _uiGroupChat.ShowChat(message, ChatType.Message, isMy);
        ClientManager.Instance.SetText("onchat: ShowChat end");
        OnChatEvent?.Invoke(clientId, message);
        ClientManager.Instance.SetText("onchat: ShowChat OnChatEvent");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
            _uiGroupChat.ShowChat("qqqqqq", ChatType.Message, true);
        if (Input.GetKeyDown(KeyCode.K))
            _uiGroupChat.ShowChat("asdasdasd", ChatType.Message, false);
    }
    private void OnMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValue(out string message);
        Debug.Log(message);
    }
}
