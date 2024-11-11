using TMPro;
using UnityEngine;

public class InputChatting : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; // TMP_InputField ����
    [SerializeField] private TMP_Text outputText;

    private bool isInputActive = false; // ��ǲ �ʵ� Ȱ��ȭ ����

    private void Start()
    {
        inputField.gameObject.SetActive(false);
    }

    private void Update()
    {
        // ���� Ű�� ������ ��
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                // ��ǲ �ʵ带 Ȱ��ȭ�ϰ� ��Ŀ���� �ش�.
                inputField.gameObject.SetActive(true);
                inputField.Select();
                inputField.ActivateInputField();
                isInputActive = true;
            }
            else
            {
                // ���͸� ������ �� ��ǲ �ʵ忡 �Էµ� �ؽ�Ʈ�� ��� �ؽ�Ʈ�� �����ϰ�
                // ��ǲ �ʵ带 ��Ȱ��ȭ�Ѵ�.
                ClientManager.Instance.SendChatRpc(inputField.text);
                inputField.gameObject.SetActive(false);
                isInputActive = false;
            }
        }
    }
}
