using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupChat : MonoBehaviour
{
    [SerializeField] private RectTransform _contentRect;
    [SerializeField] private RectTransform _emoticonRect;
    [SerializeField] private RectTransform _inputRect;

    [SerializeField] private UIEmoticon _uiEmoticon;
    [SerializeField] private GameObject _sendObj;

    [SerializeField] private Sprite _emoticonSprite;
    [SerializeField] private Sprite _messageSprite;

    [SerializeField] private Transform _cellParent;
    [SerializeField] private List<ChatCell> _cells;

    [SerializeField] private TMP_InputField inputField; // TMP_InputField 연결

    [SerializeField] private Image _emoticonButton;

    private int _index = 0;
    
    private ChatType _chatType;

    private void Start()
    {
        _uiEmoticon.OnSelected += SetActiveSendObj;
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
        _emoticonButton.sprite = _emoticonSprite;
        _chatType = ChatType.Message;

        MessageManager.Instance.OnChatMessageEvent += ReceiveMessage;
        MessageManager.Instance.OnChatEmoticonEvent += ReceiveEmotidon;
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
        inputField.text = "";
    }

    private void ReceiveEmotidon(ulong clientId, string message)
    {
        InputChat(clientId, message, ChatType.Emoticon);
    }

    private void ReceiveMessage(ulong clientId, string message)
    {
        InputChat(clientId, message, ChatType.Message);
    }

    public void InputChat(ulong clientId, string message, ChatType type)
    {
        _cells[_index].Show(clientId, message, type);

        //float viewSize = _inputRect.rect.height;

        //if (_uiEmoticon.gameObject.activeInHierarchy)
        //    viewSize += _emoticonRect.rect.height;
        
        //if (_contentRect.rect.height < viewSize)
        //{
        //    var heigth = _cells[_index].GetHeight();

        //    _contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, _contentRect.anchoredPosition.y + heigth);
        //}

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
            if (string.IsNullOrEmpty(inputField.text))
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
            else
            {
                SendMessage();
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
    }

    private void SwapChatType()
    {
        if (_chatType == ChatType.Message)
        {
            _emoticonButton.sprite = _messageSprite;
            _chatType = ChatType.Emoticon;
        }
        else
        {
            _emoticonButton.sprite = _emoticonSprite;
            _chatType = ChatType.Message;
        }
    }

    public void OnClickInput()
    {
        SendMessage();
    }

    public void OnClickInit()
    {
        if(_uiEmoticon.gameObject.activeInHierarchy)
            _uiEmoticon.Close();

        _emoticonButton.sprite = _emoticonSprite;
        _chatType = ChatType.Message;
    }

    public void OnClickEmoticon()
    {
        SwapChatType();

        if (_chatType == ChatType.Emoticon)
            _uiEmoticon.Show();
        else
            _uiEmoticon.Close();
    }

    public void OnSelect()
    {
        Debug.Log("OnSelect");
    }

    public void OnDeSelect()
    {
        Debug.Log("OnDeSelect");
    }
}