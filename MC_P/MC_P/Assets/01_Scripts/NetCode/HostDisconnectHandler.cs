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
            // ȣ��Ʈ�� ������� �˸��� �κ�� �̵�
            ShowHostDisconnectedMessage();
            GoToLobby();
        }
    }

    private void OnServerStopped()
    {
        // ��� Ŭ���̾�Ʈ���� ȣ��Ʈ�� ����Ǿ����� �˸�
        ShowHostDisconnectedMessage();
        GoToLobby();
    }

    private void ShowHostDisconnectedMessage()
    {
        // ����� �������̽��� �޽����� ���
        Debug.Log("Host has disconnected. Returning to the lobby...");
    }

    private void GoToLobby()
    {
        // �κ� ������ �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}