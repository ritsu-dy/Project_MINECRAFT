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
    public Action<bool> OnServerStopped; //true 서버의 요청 혹은 모든 클라이언트 연결이 끊겼을 때 false 비정상적으로 중지 되었을 때
    public Action<ulong> OnClientConnectedCallback; //StartClient 호출 된 후
    public Action<ulong> OnClientDisconnectCallback;
    public Action OnClientStarted;
    public Action<bool> OnClientStopped;
    public OnSessionOwnerPromotedDelegateHandler OnSessionOwnerPromoted; //세션 소유자가 변경될 때 호출되며, 새로운 소유자에게 권한을 부여하거나 관련 작업을 수행
    public ReanticipateDelegate OnReanticipate; //클라이언트의 상태가 서버와 일치하지 않을 때 클라이언트의 상태를 서버와 동기화하기 위해 재예측이 시작될 때 호출


    private void Start()
    {
        StartCoroutine(WaitNetWork());
    }

    private void OnClientConnected(ulong clientId)
    {
        // 클라이언트가 서버에 연결될 때 호출됩니다.
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
        Debug.Log("클라이언트 연결 " + clientID);
    }

    private IEnumerator SetupService()
    {
        AggregateException e = null;

        // UnityServices 초기화
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            yield return UnityServices.InitializeAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("UnityServices 초기화 완료");
                else
                    e = task.Exception;
            });
        }

        if (e != null)
        {
            Debug.LogError("UnityServices 초기화 실패: " + e);
            yield break;
        }

        // 사용자 인증 (익명 로그인)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            yield return AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("사용자 인증 완료");
                else
                    e = task.Exception;
            });
        }

        if (e != null)
        {
            Debug.LogError("사용자 인증 실패 : " + e);
            yield break;
        }
    }

    public void VerifyAllcation()
    {
        // Relay 서비스 사용 준비 확인
        if (RelayService.Instance == null)
            Debug.LogError("Relay 서비스가 초기화되지 않았습니다.");
        else
        {
            Debug.Log("Relay 서비스가 초기화 완료.");
            StartCoroutine(CreateAllocation());
        }
    }

    private IEnumerator CreateAllocation()
    {
        AggregateException e = null;
        bool isWait = true;

        // 서버 할당 생성
        yield return RelayService.Instance.CreateAllocationAsync(maxConnections: 10).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _allocation = task.Result;
                Debug.Log("서버 할당 생성 완료");
            }
            else
                e = task.Exception;

            isWait = false;

        });

        yield return new WaitWhile(() => isWait);

        if (e != null)
        {
            Debug.LogError("서버 할당 생성 실패: " + e);
            yield break;
        }

        isWait = true;

        yield return RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId).ContinueWith(joinCodeTask =>
        {
            if (joinCodeTask.IsCompletedSuccessfully)
            {
                JoinCode = joinCodeTask.Result;
                Debug.Log("Join Code 생성 완료: " + JoinCode);
            }
            else
                e = joinCodeTask.Exception;

            isWait = false;
        });

        yield return new WaitWhile(() => isWait);

        _joinText.text = JoinCode;

        if (e != null)
        {
            Debug.LogError("Join Code 생성 실패: " + e);
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

        // UnityTransport에 Relay 서버 데이터 설정
        RelayServerData relayServerData = new RelayServerData(_allocation, "udp");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        // 호스트 시작
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
        // Join Code로 서버에 연결
        yield return RelayService.Instance.JoinAllocationAsync(joinCode).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                // Relay 서버 데이터 설정
                RelayServerData relayServerData = new RelayServerData(task.Result, "udp");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                Debug.Log("서버에 성공적으로 연결되었습니다.");
            }
            else
            {
                e = task.Exception;
                Debug.LogError("서버 연결 실패: " + task.Exception);
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
