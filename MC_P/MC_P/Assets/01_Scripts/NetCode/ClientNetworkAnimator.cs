using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ClientNetworkAnimator : NetworkAnimator
{
	/// <summary>
	/// Used to determine who can write to this animator. Owner client only.
	/// This imposes state to the server. This is putting trust on your clients. Make sure no security-sensitive features use this animator.
	/// </summary>
	protected override bool OnIsServerAuthoritative()
	{
		return false;
	}

    private void Update()
    {
        if (!NetworkObject.IsOwner)
            return;

        if (Input.GetMouseButtonDown(0)) // 0은 왼쪽 마우스 버튼
        {
            Animator.SetBool("Mining", true);
        }
        // 마우스 버튼이 떼졌을 때
        if (Input.GetMouseButtonUp(0)) // 0은 왼쪽 마우스 버튼
        {
            Animator.SetBool("Mining", false);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            float horizontal = Input.GetAxis("Horizontal");
            Animator.SetFloat("Speed", horizontal);
        }
    }

    private string ToTriggerName(string input)
    {
        // 각 키에 대한 처리를 구현
        switch (input)
        {
            case " ":
                return "space";
            default:
                return input;
        }
    }
}