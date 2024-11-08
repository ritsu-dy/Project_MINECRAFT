using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;

    private void Update()
    {
        if (!IsOwner) return; // ������ Ŭ���̾�Ʈ������ ����

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime;

        transform.Translate(movement);
    }
}
