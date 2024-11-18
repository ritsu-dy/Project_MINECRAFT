using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Netcode.NetworkManager;

public class RelayManager : SingletonBase<RelayManager>
{
    [SerializeField] private TMP_Text _joinText;
    [SerializeField] private UnityTransport _unityTransport;

    private string _joinCode;
    private bool _isHost = false;
    public string JoinCode { get { return _joinCode; } set { _joinCode = value; } }
    public bool IsHost { get { return _isHost; } set { _isHost = value; } }

    public ulong ClientId;

    private Allocation _allocation;

    public Action OnServerStarted;
    public Action<string> OnCreatedJoinCode;
    public Action<bool> OnServerStopped; //true ������ ��û Ȥ�� ��� Ŭ���̾�Ʈ ������ ������ �� false ������������ ���� �Ǿ��� ��
    public Action<ulong> OnClientConnectedCallback; //StartClient ȣ�� �� ��
    public Action<ulong> OnClientDisconnectCallback;
    public Action OnClientStarted;
    public Action<bool> OnClientStopped;
    public OnSessionOwnerPromotedDelegateHandler OnSessionOwnerPromoted; //���� �����ڰ� ����� �� ȣ��Ǹ�, ���ο� �����ڿ��� ������ �ο��ϰų� ���� �۾��� ����
    public ReanticipateDelegate OnReanticipate; //Ŭ���̾�Ʈ�� ���°� ������ ��ġ���� ���� �� Ŭ���̾�Ʈ�� ���¸� ������ ����ȭ�ϱ� ���� �翹���� ���۵� �� ȣ��


    private void Start()
    {
        StartCoroutine(WaitNetWork());
    }

    private void OnClientConnected(ulong clientId)
    {
        // Ŭ���̾�Ʈ�� ������ ����� �� ȣ��˴ϴ�.
        if (clientId == NetworkManager.Singleton.LocalClientId)
            GameManager.Instance.Player = NetworkManager.Singleton.LocalClient.PlayerObject;

        ShowServerText("OnClientConnected " + clientId);
    }    

    public void ConnectToServerCoroutine(string joinCode)
    {
        StartCoroutine(ConnectToServer(joinCode));
    }

    private IEnumerator WaitNetWork()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        SetupEvent();
        StartCoroutine(SetupService());
    }

    private void SetupEvent()
    {
        AddEvent();

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
    }

    private void AddEvent()
    {
        OnServerStarted += ServerStart;
        OnServerStopped += ServerEnd;

        OnClientConnectedCallback += OnClientConnected;
        OnClientDisconnectCallback += DisClientConnected;

        OnClientStarted += ClientStarted;
        OnClientStopped += ClientStopped;
    }

    private void ShowServerText(string message)
    {
    }

    private void ShowClientText(string message)
    {
        Debug.Log(message);
    }

    private void ServerStart()
    {
        ShowServerText("ServerStart");
    }

    private void ServerEnd(bool isForce)
    {
        ShowServerText("ServerEnd " + isForce);
    }

    private void ClientStarted()
    {
        ShowClientText("ClientStarted");
    }

    private void ClientStopped(bool isForce)
    {
        ShowClientText("ClientStopped " + isForce);
    }

    private void DisClientConnected(ulong clientId)
    {
        ShowClientText("DisClientConnected " + clientId);
    }

    private void ShowCliendId(ulong clientID)
    {
        Debug.Log("Ŭ���̾�Ʈ ���� " + clientID);
    }

    private IEnumerator SetupService()
    {
        AggregateException e = null;

        // UnityServices �ʱ�ȭ
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            yield return UnityServices.InitializeAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("UnityServices �ʱ�ȭ �Ϸ�");
                else
                    e = task.Exception;
            });
        }

        if (e != null)
        {
            Debug.LogError("UnityServices �ʱ�ȭ ����: " + e);
            yield break;
        }

        // ����� ���� (�͸� �α���)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            yield return AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("����� ���� �Ϸ�");
                else
                    e = task.Exception;
            });
        }

        if (e != null)
        {
            Debug.LogError("����� ���� ���� : " + e);
            yield break;
        }
    }

    public void VerifyAllcation()
    {
        // Relay ���� ��� �غ� Ȯ��
        if (RelayService.Instance == null)
            Debug.LogError("Relay ���񽺰� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        else
        {
            Debug.Log("Relay ���񽺰� �ʱ�ȭ �Ϸ�.");
            StartCoroutine(CreateAllocation());
        }
    }

    private IEnumerator CreateAllocation()
    {
        AggregateException e = null;
        bool isWait = true;

        // ���� �Ҵ� ����
        yield return RelayService.Instance.CreateAllocationAsync(maxConnections: 10).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _allocation = task.Result;
                Debug.Log("���� �Ҵ� ���� �Ϸ�");
            }
            else
                e = task.Exception;

            isWait = false;

        });

        yield return new WaitWhile(() => isWait);

        if (e != null)
        {
            Debug.LogError("���� �Ҵ� ���� ����: " + e);
            yield break;
        }

        isWait = true;

        yield return RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId).ContinueWith(joinCodeTask =>
        {
            if (joinCodeTask.IsCompletedSuccessfully)
            {
                JoinCode = joinCodeTask.Result;
                Debug.Log("Join Code ���� �Ϸ�: " + JoinCode);
            }
            else
                e = joinCodeTask.Exception;

            isWait = false;
        });

        yield return new WaitWhile(() => isWait);

        _joinText.text = JoinCode;

        if (e != null)
        {
            Debug.LogError("Join Code ���� ����: " + e);
            yield break;
        }
    }

    public void StartHost(Action<string> callback = null)
    {
        StartCoroutine(StartHostRoutine(callback));
    }

    public IEnumerator StartHostRoutine(Action<string> callback = null)
    {
        if (_allocation == null)
            yield return StartCoroutine(CreateAllocation());

        // UnityTransport�� Relay ���� ������ ����
        RelayServerData relayServerData = new RelayServerData(_allocation, "udp");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        // ȣ��Ʈ ����
        var isHost = NetworkManager.Singleton.StartHost();
        IsHost = isHost;
        ShowServerText("Start Host " + isHost);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator ConnectToServer(string joinCode)
    {
        ShowClientText("server connecte... " + joinCode);

        bool isWait = true;
        AggregateException e = null;
        // Join Code�� ������ ����
        yield return RelayService.Instance.JoinAllocationAsync(joinCode).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                // Relay ���� ������ ����
                RelayServerData relayServerData = new RelayServerData(task.Result, "udp");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                Debug.Log("������ ���������� ����Ǿ����ϴ�.");
            }
            else
            {
                e = task.Exception;
                Debug.LogError("���� ���� ����: " + task.Exception);
            }

            isWait = false;
        });

        yield return new WaitWhile(() => isWait);

        if (e != null)
            ShowClientText("server connecte... failed " + e);
        else
            ShowClientText("server success... " + e);
    }
}
