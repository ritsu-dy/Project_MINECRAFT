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

    int test=0;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            MessageManager.Instance.SendMessageToAllClient(" all test " + test, MessageName.Message);

        if (Input.GetKeyDown(KeyCode.N))
        {
            MessageManager.Instance.SendMessageToClient(1, " message test " + test);
            test++;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            MessageManager.Instance.SendMessageToClient(0, " message test " + test);
            test++;
        }
    }

    private void CheckApproval(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
    {
        var isVerify = VerifyClient(request);
        ClientManager.Instance.SetText("CheckApproval");
        ulong clientId = request.ClientNetworkId;

        if (!isVerify)
        {
            //���� ���� ���¸� ������
            response.Pending = false;
            response.Reason = "You have been disconnected: Failed validation.";

            // ���� ������ �����ϰ� Ŭ���̾�Ʈ�� ���� �׽�Ʈ �غ�����
            // Ŭ���̾�Ʈ ���� ����
            SendMessageToClient(clientId, response.Reason);
            //MessageManager.Instance.SendMessageToClient(clientId, response.Reason, MessageName.Message);
            NetworkManager.Singleton.DisconnectClient(clientId, response.Reason);
        }
        else
            SendMessageToClient(clientId, "connect Success!!");

        response.Approved = isVerify;
        response.CreatePlayerObject = isVerify;  // �÷��̾� ��ü ���� ���� ����
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
    }

    private void SendMessageToClient(ulong clientId, string message)
    {
        var writer = new FastBufferWriter(256, Allocator.Temp);
        writer.WriteValueSafe(message);
        ClientManager.Instance.SetText("WriteValueSafe");
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(MessageName.Message.ToString(), clientId, writer);
        ClientManager.Instance.SetText("WriteValueSafe end");
    }

    private bool VerifyClient(ConnectionApprovalRequest request)
    {
        string data = System.Text.Encoding.UTF8.GetString(request.Payload);
        var clientId = request.ClientNetworkId;

        Debug.Log("VerifyClient " + data);
        // ������Ʈ�� ���Ե� Ŭ���̾�Ʈ���� Ȯ��
        if (_benList.Contains(clientId))
        {
            Debug.Log("�� ����Ʈ ���� " + clientId);
            return false; // ���� ����
        }

        return true; // ����
    }

    private void OnClientConnected(ulong clientId)
    {
        MessageManager.Instance.Show();

        if (NetworkManager.Singleton.IsHost)
        {
            _hostId = clientId;
            MessageManager.Instance.SetupEvent();
        }


    }

    private void OnDisClientConnected(ulong clientId)
    {
        Debug.Log("NetworkManager.Singleton.DisconnectReason " + NetworkManager.Singleton.DisconnectReason);
        //ȣ��Ʈ ���� ����
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
            Debug.Log("�ٸ� Ŭ���̾�Ʈ ���� ���� " + clientId);
        }
    }

    private void DisconnectedClient(ulong clientId)
    {
        Debug.Log("DisconnectedClient");
        NetworkManager.Singleton.DisconnectClient(clientId, "testreason");
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
