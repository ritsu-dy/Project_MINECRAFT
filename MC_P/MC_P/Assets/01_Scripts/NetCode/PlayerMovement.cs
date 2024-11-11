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
            return; // 플레이어의 소유자가 아닌 경우 무시
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.position += move; // 위치 업데이트
        ShowText(transform.position);
        //networkTransform.SetState(transform.position); // NetworkTransform의 더티 플래그 설정
    }
}
