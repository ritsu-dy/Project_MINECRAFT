using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private NetworkTransform networkTransform;

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void ShowText(Vector3 move)
    {
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
    }

    void Update()
    {
        if (!NetworkObject.IsOwner)
        {
            return; // �÷��̾��� �����ڰ� �ƴ� ��� ����
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.position += move; // ��ġ ������Ʈ
        ShowText(transform.position);
        //networkTransform.SetState(transform.position); // NetworkTransform�� ��Ƽ �÷��� ����
    }
}
