using EVP;
using UnityEngine;

public class VehicleAIController : MonoBehaviour
{
    [Range(-1, 1)]
    public float steerInput;
    [Range(-1, 1)]
    public float throttleInput;
    [Range(0, 1)]
    public float brakeInput;
    [Range(0, 1)]
    public float handbrakeInput;

    public Vector3 targetPosition;
    public bool isTargetPositionSet;
    public bool isAvoiding;
    public bool isArrived;

    private VehicleController target;
    private StateController<VehicleState> StateController;

    void Start()
    {
        target = GetComponent<VehicleController>();

        StateController = new StateController<VehicleState>();
        StateController.RegisterState(new VehicleIdleState().SetVehicleAIController(this));
        StateController.RegisterState(new VehicleMoveForwardState().SetVehicleAIController(this));
        StateController.RegisterState(new VehicleMoveBackwardState().SetVehicleAIController(this));
        StateController.RegisterState(new VehicleTurnLeftState().SetVehicleAIController(this));
        StateController.RegisterState(new VehicleTurnRightState().SetVehicleAIController(this));
        StateController.ActiveState(VehicleState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return;
        }

        StateController.OnUpdate(Time.deltaTime);

        target.steerInput = steerInput;
        target.throttleInput = throttleInput;
        target.brakeInput = brakeInput;
        target.handbrakeInput = handbrakeInput;

        if (isTargetPositionSet)
        {
            targetPosition.y = transform.position.y;
            var distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < 5)
            {
                isArrived = true;
            }
            else
            {
                isArrived = false;
            }
        }
    }

    public void SetState(VehicleState state)
    {
        StateController.ActiveState(state);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        isTargetPositionSet = true;
        isArrived = false;
    }

    public void ResetTargetPosition()
    {
        isTargetPositionSet = false;
        isArrived = false;
    }

    [ContextMenu("Startup")]
    public void Startup()
    {
        SetTargetPosition(targetPosition);
    }
}
