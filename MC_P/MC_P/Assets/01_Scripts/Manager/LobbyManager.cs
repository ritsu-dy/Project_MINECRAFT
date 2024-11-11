using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class LobbyManager : SingletonBase<LobbyManager>
{
    private Lobby currentLobby;
    private const float heartbeatInterval = 15f;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }

    // 1. �κ� ����
    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Casual") }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Lobby created with ID: " + currentLobby.Id);

            StartCoroutine(HeartbeatLobby());
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // 2. �κ� ����
    public async Task JoinLobby(string lobbyCode)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("Joined lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // 3. �κ� ������
    public async Task LeaveLobby()
    {
        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                currentLobby = null;
                Debug.Log("Left the lobby.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    // 4. �κ� ���� - ������ ���� ���� �ð� �������� ����
    private IEnumerator HeartbeatLobby()
    {
        while (currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    // 5. ���� ���� - Netcode ������ ����
    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
