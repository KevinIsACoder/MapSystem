public abstract class VehicleStateBase : IState<VehicleState>
{
    public abstract VehicleState GetName();
    public abstract void Init();
    public abstract void DeInit();
    public abstract void Exit(VehicleState name);
    public abstract void Enter(VehicleState lastStateName);
    public abstract void Push(int eventId, object args);

    public abstract void OnUpdate(float deltaTime);

    public VehicleAIController vehicleAIController;

    public VehicleStateBase SetVehicleAIController(VehicleAIController vehicleAIController)
    {
        this.vehicleAIController = vehicleAIController;
        return this;
    }
}
