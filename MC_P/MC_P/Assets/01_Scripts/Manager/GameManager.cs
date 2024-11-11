using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    [SerializeField] private TMP_Text _joinText;

    private RelayManager relayManager;
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

    private void Start()
    {
        relayManager = GetComponent<RelayManager>();
    }

    private void OnSetupPlayer(NetworkObject player)
    {
        ChatManager.Instance.Init(player.transform);
    }

    public void OnClickStartHost()
    {
        Debug.Log("OnClickStartHost");
        relayManager.StartHost(ShowJoinCode);
    }

    public void OnClickJoinGame()
    {
        relayManager.ConnectToServerCoroutine(relayManager.JoinCode);
    }

    private void ShowJoinCode(string joinCode)
    {
        _joinText.text = joinCode;
    }
}
