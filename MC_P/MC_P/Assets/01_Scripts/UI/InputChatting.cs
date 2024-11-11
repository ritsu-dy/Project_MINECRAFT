using TMPro;
using UnityEngine;

public class InputChatting : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; // TMP_InputField 연결
    [SerializeField] private TMP_Text outputText;

    private bool isInputActive = false; // 인풋 필드 활성화 여부

    private void Start()
    {
        inputField.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 엔터 키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                // 인풋 필드를 활성화하고 포커스를 준다.
                inputField.gameObject.SetActive(true);
                inputField.Select();
                inputField.ActivateInputField();
                isInputActive = true;
            }
            else
            {
                // 엔터를 눌렀을 때 인풋 필드에 입력된 텍스트를 출력 텍스트로 복사하고
                // 인풋 필드를 비활성화한다.
                ClientManager.Instance.SendChatRpc(inputField.text);
                inputField.gameObject.SetActive(false);
                isInputActive = false;
            }
        }
    }
}
