using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    [SerializeField] private TMP_InputField _joinField;
    [SerializeField] private TMP_Text _statusText;

    public void OnClickEnterServer()
    {
        RelayManager.Instance.ConnectToServerCoroutine(_joinField.text, Close);
    }

    public void OnClickCreateServer()
    {
        RelayManager.Instance.StartHost(Close);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
