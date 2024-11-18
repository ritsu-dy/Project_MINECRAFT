using Unity.Netcode;
using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private Dictionary<ulong, NetworkObject> _obj = new Dictionary<ulong, NetworkObject>();

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) // 서버에서만 처리
            return;

        CheckOwner(clientId);
    }

    public void CheckOwner(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
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
