using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    [SerializeField] private UIGroupChat _uiGroupChat;

    private NetworkObject _player;

    public NetworkObject Player 
    { 
        get { return _player; } 
        set 
        { 
            _player = value;
            OnSetupPlayer(_player);
        }
    }

    private void OnSetupPlayer(NetworkObject player)
    {
        ChatManager.Instance.Init(player.transform);
    }

    public void OnClickChat()
    {
        _uiGroupChat.Show();
    }
}