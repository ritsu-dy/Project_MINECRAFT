using Unity.Netcode;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<int> Level = new NetworkVariable<int>();

    private void Start()
    {
        // ���������� ���� ���� ���� (�⺻ ����)
        if (IsServer)
            Level.Value = 0; // �ʱⰪ ����
    }

    private void SetupEvent()
    {
        Level.OnValueChanged += OnChangeLevel;
    }

    private void Update()
    {
        // ���������� �� ���� ���� (�⺻ ����)
        if (IsServer && Input.GetKeyDown(KeyCode.Space))
            Level.Value += 10; // ���� ����
    }

    private void OnChangeLevel(int prevValue, int newValue)
    {

    }
}
