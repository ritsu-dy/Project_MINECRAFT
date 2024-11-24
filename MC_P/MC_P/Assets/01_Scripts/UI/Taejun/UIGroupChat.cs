using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupChat : MonoBehaviour
{
    [SerializeField] private UIEmoticon _uiEmoticon;

    [SerializeField] private GameObject _sendObj;

    [SerializeField] private Sprite _emoticonSprite;
    [SerializeField] private Sprite _messageSprite;

    [SerializeField] private Transform _cellParent;
    [SerializeField] private List<ChatCell> _cells;

    [SerializeField] private TMP_InputField inputField; // TMP_InputField 연결

    [SerializeField] private Image _emoticonButton;

    private int _index = 0;
    
    private bool isInputActive = false; // 인풋 필드 활성화 여부

    private ChatType _chatType;

    private void Start()
    {
    }

    public void Show()
    {
        if (gameObject.activeInHierarchy)
        {
            Close();
            return;
        }

        _uiEmoticon.Close();
        gameObject.SetActive(true);
        SetActiveSendObj(false);
        SetChatType(ChatType.Message);

        MessageManager.Instance.OnChatMessageEvent += ReceiveMessage;
        MessageManager.Instance.OnChatMessageEvent += ReceiveEmotidon;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public Sprite GetEmoticon(string key)
    {
        return _uiEmoticon.GetEmoticon(key);
    }

    public void OnChangeMessage()
    {
        Debug.Log(inputField.text);

        _sendObj.SetActive(inputField.text.Length > 0);
    }

    private void SetAciveRedDot()
    {

    }

    private void SetActiveSendObj(bool isActive)
    {
        _sendObj.SetActive(isActive);
    }

    public void SendMessage()
    {
        switch (_chatType)
        {
            case ChatType.Message:
                MessageManager.Instance.SendMessageToAllClient(inputField.text, MessageName.Chat_Message);
                break;
            case ChatType.Emoticon:
                MessageManager.Instance.SendMessageToAllClient(_uiEmoticon.GetEmoticonKey(), MessageName.Chat_Emoticon);
                break;
            default:
                MessageManager.Instance.SendMessageToAllClient(inputField.text, MessageName.Chat_Message);
                break;
        }

        SetActiveSendObj(false);
    }

    private void ReceiveEmotidon(ulong clientId, string message)
    {
        var isMy = NetworkManager.Singleton.LocalClientId == clientId;
        InputChat(message, ChatType.Emoticon, isMy);
    }

    private void ReceiveMessage(ulong clientId, string message)
    {
        var isMy = NetworkManager.Singleton.LocalClientId == clientId;
        InputChat(message, ChatType.Message, isMy);
    }

    public void InputChat(string message, ChatType type, bool isMy)
    {
        _cells[_index].Show(message, type, isMy);
        _index++;

        if (_index == _cells.Count)
            Refresh();
    }

    private void Refresh()
    {
        var child = _cellParent.GetChild(0);

        child.SetAsLastSibling();
        _index = 0;
    }

    private void Update()
    {
        // 엔터 키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputActive)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
            else
            {
                isInputActive = false;
                SendMessage();
            }
        }
    }

    private void SetChatType(ChatType type)
    {
        if (type == ChatType.Message)
        {
            _emoticonButton.sprite = _messageSprite;
            _chatType = ChatType.Message;
        }
        else
        {
            _emoticonButton.sprite = _emoticonSprite;
            _chatType = ChatType.Emoticon;
        }
    }

    public void OnClickInput()
    {

    }

    public void OnClickInit()
    {
        _uiEmoticon.Close();
        inputField.DeactivateInputField();
        isInputActive = false;
    }

    public void OnClickEmoticon()
    {
        SetChatType(_chatType);

        if (_chatType == ChatType.Emoticon)
            _uiEmoticon.Close();
        else
            _uiEmoticon.Show();
    }

    public void OnSelect()
    {
        Debug.Log("OnSelect");
        isInputActive = true;
    }

    public void OnDeSelect()
    {
        Debug.Log("OnDeSelect");
        isInputActive = false;
    }
}