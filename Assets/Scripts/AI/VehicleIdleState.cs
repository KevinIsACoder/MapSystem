public class VehicleIdleState : VehicleStateBase
{
    public override VehicleState GetName()
    {
        return VehicleState.Idle;
    }

    public override void Init()
    {
    }

    public override void DeInit()
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        if (vehicleAIController.isTargetPositionSet)
        {
            vehicleAIController.SetState(VehicleState.MoveForward);
        }
    }

    public override void Exit(VehicleState name)
    {
    }

    public override void Enter(VehicleState lastStateName)
    {
    }

    public override void Push(int eventId, object args)
    {
    }
}
