using UnityEngine;
public static class InputController
{
    public static void GetMovementInput(out float x, out float z)
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }
}

