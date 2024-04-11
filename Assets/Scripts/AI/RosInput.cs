using UnityEngine;

public class RosInput : MonoBehaviour
{
    public float linear;
    public float angular;
    public TankController tankController;

    private void Update()
    {
        tankController.SetLinerInput(linear);
        tankController.SetAngularInput(angular);
    }
}
