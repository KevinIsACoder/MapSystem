using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    public TankController tankController;

    private void Update()
    {
        var vertical = Input.GetAxis("Vertical") * 0.1f;
        var horizontal = Input.GetAxis("Horizontal") * 0.1f;
        tankController.SetVerticleInput(vertical);
        tankController.SetHorizontalInput(horizontal);
    }
}
