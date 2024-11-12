using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using static Unity.Netcode.NetworkManager;
using System.Collections;
using Unity.Collections;

public class MigrationManager : SingletonBase<MigrationManager>
{
    private ulong _hostId = 0;

    private HashSet<ulong> _benList = new HashSet<ulong>();

    private void Start()
    {
        SetupEvent();
    }

    private void SetupEvent()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisClientConnected;
        NetworkManager.Singleton.ConnectionApprovalCallback += CheckApproval;
    }

    private void CheckApproval(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
    {
        var isVerify = VerifyClient(request);

        if(!isVerify)
        {
            //연결 승인 상태를 지연용
            response.Pending = false;
            response.Reason = "You have been disconnected: Failed validation.";
            ulong clientId = request.ClientNetworkId;

            // 거절 사유를 저장하고 클라이언트에 전송 테스트 해봐야함
            // 클라이언트 연결 종료
            NetworkManager.Singleton.DisconnectClient(clientId, response.Reason);
        }

        response.Approved = isVerify;
        response.CreatePlayerObject = isVerify;  // 플레이어 객체 생성 여부 설정
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
    }

    private void SendDisconnectReasonToClient(ulong clientId, string reason)
    {
        var writer = new FastBufferWriter(256, Allocator.Temp);
        writer.WriteValueSafe(reason);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("DisconnectReason", clientId, writer);
    }

    private bool VerifyClient(ConnectionApprovalRequest request)
    {
        string data = System.Text.Encoding.UTF8.GetString(request.Payload);
        var clientId = request.ClientNetworkId;

        // 블랙리스트에 포함된 클라이언트인지 확인
        if (_benList.Contains(clientId))
            return false; // 승인 거절

        return true; // 승인
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
            _hostId = clientId;
    }

    private void OnDisClientConnected(ulong clientId)
    {
        //호스트 연결 끊김
        if (NetworkManager.Singleton.LocalClientId == _hostId)
        {
            var newHostId = GetNewHostClient();

            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                var client = NetworkManager.Singleton.ConnectedClientsList[i];

                if(client.ClientId == newHostId)
                {
                    StartCoroutine(ChangeHost());
                    break;
                }
            }
        }
        else
        {
            Debug.Log("다른 클라이언트 연결 끊김 " + clientId);
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
    }

    private IEnumerator ChangeHost()
    {
        NetworkManager.Singleton.Shutdown();
        yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);
        GameManager.Instance.OnClickStartHost();
    }

    private ulong GetNewHostClient()
    {
        return NetworkManager.Singleton.ConnectedClientsList.Min(x => x.ClientId);
    }
}
