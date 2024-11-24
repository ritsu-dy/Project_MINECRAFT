using Unity.Netcode;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<int> Level = new NetworkVariable<int>();

    private void Start()
    {
        // 서버에서만 값을 설정 가능 (기본 설정)
        if (IsServer)
            Level.Value = 0; // 초기값 설정
    }

    private void SetupEvent()
    {
        Level.OnValueChanged += OnChangeLevel;
    }

    private void Update()
    {
        // 서버에서만 값 변경 가능 (기본 설정)
        if (IsServer && Input.GetKeyDown(KeyCode.Space))
            Level.Value += 10; // 점수 증가
    }

    private void OnChangeLevel(int prevValue, int newValue)
    {

    }
}
