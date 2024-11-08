using UnityEngine;
using UnityEngine.InputSystem;

public class Tsetinput : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInput playerInput2;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnClickFirst(playerInput, true);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            OnClickFirst(playerInput, false);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnClickFirst(playerInput2, true);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            OnClickFirst(playerInput2, false);

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            var input = playerInput.actions;
            playerInput.actions = playerInput2.actions;
            playerInput2.actions = input;
        }
    }

    public void OnClickFirst(PlayerInput input, bool enable)
    {
        input.enabled = enable;
    }
}
