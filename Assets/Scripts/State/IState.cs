/// <summary>
/// 状态机接口
/// </summary>
public interface IState<T>
{
    /// <summary>
    /// 状态名称
    /// </summary>
    T GetName();
    /// <summary>
    /// 初始化
    /// </summary>
    void Init();
    /// <summary>
    /// 反初始化
    /// </summary>
    void DeInit();
    /// <summary>
    /// 进入状态
    /// </summary>
    /// <param name="lastStateName">上个状态的名称</param>
    void Enter(T lastStateName);
    /// <summary>
    /// 退出状态
    /// </summary>
    /// <param name="nextStateName">下个状态的名称</param>
    void Exit(T nextStateName);
    void OnUpdate(float deltaTime);
    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="args"></param>
    void Push(int eventId, object args);
}
