using System.Collections.Generic;
using UnityEngine;

public class UIGroupChat : MonoBehaviour
{
    [SerializeField] private List<ChatCell> _cells;

    private int _index = 0;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void ShowChat(string message, ChatType type, bool isMy)
    {
        _cells[_index].Show(message, type, isMy);
        _index++;
    }
}