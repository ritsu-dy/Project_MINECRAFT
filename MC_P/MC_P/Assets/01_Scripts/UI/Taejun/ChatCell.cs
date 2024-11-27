using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ChatType
{
    Message,
    Emoticon,
}

public class ChatCell : MonoBehaviour
{
    [SerializeField] private RectTransform _rect;

    [SerializeField] private GameObject _infoObj;
    [SerializeField] private GameObject _messageObj;
    [SerializeField] private GameObject _systemObj;

    [SerializeField] private Image _icon;
    [SerializeField] private Image _emoticon;
    [SerializeField] private Image _messageBg;

    [SerializeField] private TMP_Text _message;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _systemMessage;

    [SerializeField] private VerticalLayoutGroup _cellLayout;
    [SerializeField] private LayoutElement _textLayout;

    private string _name;

    public void Init(string name)
    {
        _name = name;
    }

    public float GetHeight()
    {
        return _rect.rect.height;
    }

    public void Show(ulong clientId, string message, ChatType type)
    {
        var isMy = NetworkManager.Singleton.LocalClientId == clientId;

        if (isMy)
        {
            _cellLayout.childAlignment = TextAnchor.UpperLeft;
            _infoObj.SetActive(false);
        }
        else
        {
            _infoObj.SetActive(true);
            _cellLayout.childAlignment = TextAnchor.UpperRight;
            _icon.rectTransform.localScale = new Vector3(-1,1,1);
        }

        if(type == ChatType.Message)
        {
            _messageBg.enabled = true;
            _message.gameObject.SetActive(true);
            _emoticon.gameObject.SetActive(false);

            _message.text = message;

            if (_message.preferredWidth > 300)
            {
                _textLayout.enabled = true;
                _textLayout.preferredWidth = 300;
            }
            else
                _textLayout.enabled = false;
        }
        else
        {
            _messageBg.enabled = false;
            _emoticon.gameObject.SetActive(true);
            _emoticon.sprite = MessageManager.Instance.GetEmoticon(message);
            _message.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }
}