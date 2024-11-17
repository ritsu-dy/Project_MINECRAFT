using Unity.Netcode;
using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private TMP_Text _text;
    private Dictionary<ulong, NetworkObject> _obj = new Dictionary<ulong, NetworkObject>();

    private void Start()
    {
        _text = GameObject.Find("checkevent").GetComponent<TMP_Text>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("OnClientConnected ServerClientId " + NetworkManager.ServerClientId);
        Debug.Log("OnClientConnected " + NetworkManager.LocalClientId);
        Debug.Log("OnClientConnected LocalClientId " + NetworkManager.Singleton.LocalClientId);
        
        if (!IsServer) // 서버에서만 처리
            return;

        Debug.Log("OnClientConnected " + clientId);

        CheckOwner(clientId);
    }

    public void CheckOwner(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            //PlayerInput.Instantiate()

            Debug.Log(clientId + " IsOwner " + client.PlayerObject.IsOwner);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}
