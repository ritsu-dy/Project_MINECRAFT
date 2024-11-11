using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class MigrationManager : SingletonBase<MigrationManager>
{
    private ulong _hostId = 0;

    private void Start()
    {
        SetupEvent();
    }

    private void SetupEvent()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
            _hostId = clientId;
    }

    private void OnDisClientConnected(ulong clientId)
    {
        //ȣ��Ʈ ���� ����
        if (NetworkManager.Singleton.LocalClientId == _hostId)
        {
            var newHostId = GetNewHostClient();

            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                var client = NetworkManager.Singleton.ConnectedClientsList[i];

                if(client.ClientId == newHostId)
                {
                    SetupNewHost(clientId);
                    break;
                }
            }
        }
        else
        {
            Debug.Log("�ٸ� Ŭ���̾�Ʈ ���� ���� " + clientId);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            DisconnectedClient(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            DisconnectedClient(1);
    }

    private void DisconnectedClient(ulong clientId)
    {
        Debug.Log("DisconnectedClient");
        NetworkManager.Singleton.DisconnectClient(clientId);
        //NetworkManager.Singleton.ConnectionApprovalCallback
        //NetworkManager.Singleton.NetworkConfig.ConnectionData
    }

    private void SetupNewHost(ulong clientId)
    {
        NetworkManager.Singleton.Shutdown();
        GameManager.Instance.OnClickStartHost();
    }

    private ulong GetNewHostClient()
    {
        return NetworkManager.Singleton.ConnectedClientsList.Min(x => x.ClientId);
    }
}
