using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ChatType
{
    Message,
    Image,
}

public class ChatCell : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _emoticon;

    [SerializeField] private TMP_Text _message;
    [SerializeField] private TMP_Text _name;

    [SerializeField] private VerticalLayoutGroup _cellLayout;
    [SerializeField] private LayoutElement _textLayout;

    public void Show(string message, ChatType type, bool isMy)
    {
        gameObject.SetActive(true);

        if(isMy)
        {
            _cellLayout.childAlignment = TextAnchor.UpperLeft;
            _icon.rectTransform.localScale = Vector3.one;
        }
        else
        {
            _cellLayout.childAlignment = TextAnchor.UpperRight;
            _icon.rectTransform.localScale = new Vector3(-1,1,1);
        }

        if(type == ChatType.Message)
        {
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
            _emoticon.gameObject.SetActive(true);
            _message.gameObject.SetActive(false);
        }
    }
}