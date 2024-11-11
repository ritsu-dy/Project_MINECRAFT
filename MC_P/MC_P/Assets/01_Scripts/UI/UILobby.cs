using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_InputField lobbyCodeInput;

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button startGameButton;

    private void Start()
    {
        createLobbyButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.CreateLobby(lobbyNameInput.text, 4);
        });

        joinLobbyButton.onClick.AddListener(async () =>
        {
            await LobbyManager.Instance.JoinLobby(lobbyCodeInput.text);
        });

        startGameButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartGame();
        });
    }
}
