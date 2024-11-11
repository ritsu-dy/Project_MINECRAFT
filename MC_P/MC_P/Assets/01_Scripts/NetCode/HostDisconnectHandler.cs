using Unity.Netcode;
using UnityEngine;

public class HostDisconnectHandler : MonoBehaviour
{
    void Start()
    {
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // 호스트가 종료됨을 알리고 로비로 이동
            ShowHostDisconnectedMessage();
            GoToLobby();
        }
    }

    private void OnServerStopped()
    {
        // 모든 클라이언트에게 호스트가 종료되었음을 알림
        ShowHostDisconnectedMessage();
        GoToLobby();
    }

    private void ShowHostDisconnectedMessage()
    {
        // 사용자 인터페이스에 메시지를 띄움
        Debug.Log("Host has disconnected. Returning to the lobby...");
    }

    private void GoToLobby()
    {
        // 로비 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}