using System.Collections.Generic;
using UnityEngine;

public class ChatManager : SingletonBase<ChatManager>
{
    [SerializeField] private List<FollowUI> _followUi;

    public void Init(Transform target)
    {
        for (int i = 0; i < _followUi.Count; i++)
        {
            _followUi[i].SetTarget(target);
        }
    }

    public void ShowChat(string message)
    {
        _followUi[0].ShowText(message);
    }
}
