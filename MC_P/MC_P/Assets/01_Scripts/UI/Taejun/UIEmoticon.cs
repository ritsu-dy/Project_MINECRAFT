using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;

public class UIEmoticon : MonoBehaviour
{
    [SerializeField] private GameObject _previewObj;

    [SerializeField] private List<Image> _tabImages;
    [SerializeField] private List<Image> _emoticonImages;

    [SerializeField] private Image _previewImage;

    [SerializeField] private SerializedDictionary<int, List<Sprite>> _emoticonSprites;
    [SerializeField] private SerializedDictionary<int, List<Sprite>> _tabSprites;

    private int _tabIndex = 0;
    private string _key;

    public void Show()
    {
        _previewObj.SetActive(false);

        for (int i = 0; i < _tabImages.Count; i++)
        {
            _tabImages[i].sprite = _tabSprites[i][0];
        }

        ShowEmoticon(_tabIndex);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _previewObj.SetActive(false);
    }

    public void OnClickRecent()
    {

    }

    public void OnClickTab(int index)
    {
        if (_tabIndex == index)
            return;

        _tabImages[_tabIndex].sprite = _tabSprites[_tabIndex][1];
        _tabIndex = index;
        ShowEmoticon(index);
    }

    public void OnClickEmoticon(int index)
    {
        if(_emoticonSprites.TryGetValue(_tabIndex, out var list))
        {
            if(list.Count > index)
                ShowPreview(list[index]);

            _key = _tabIndex + "_"+ index;
        }
    }

    public void OnClickClosePreview()
    {
        _previewObj.SetActive(false);
    }

    private void ShowEmoticon(int index)
    {
        for (int i = 0; i < _emoticonImages.Count; i++)
        {
            if (_emoticonImages[i].gameObject.activeInHierarchy)
                _emoticonImages[i].gameObject.SetActive(false);
        }

        if (_tabSprites.ContainsKey(_tabIndex))
        {
            _tabImages[_tabIndex].sprite = _tabSprites[_tabIndex][1];
            _tabImages[_tabIndex].gameObject.SetActive(true);
        }

        if (_emoticonSprites.TryGetValue(_tabIndex, out var list))
        {
            if (list.Count > index)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    _emoticonImages[i].sprite = list[i];
                    _emoticonImages[i].gameObject.SetActive(true);
                }
            }
        }
    }

    private void ShowPreview(Sprite sprite)
    {
        _previewObj.SetActive(true);
        _previewImage.sprite = sprite;
    }

    public string GetEmoticonKey()
    {
        return _key;
    }

    public Sprite GetEmoticon(string key)
    {
        var values = key.Split("_");

        int tabIndex = int.Parse(values[0]);
        int emoticonIndex = int.Parse(values[0]);

        return _emoticonSprites[tabIndex][emoticonIndex];
    }
}

//[Serializable]
//public class EmoticonSpriteData : SerializableDictionary<int, EmoticonSpriteList> { }

//[Serializable]
//public class EmoticonSpriteList
//{
//    [SerializeField] private List<Sprite> _sprites;

//    public List<Sprite> Sprites { get { return _sprites; } }
//}
