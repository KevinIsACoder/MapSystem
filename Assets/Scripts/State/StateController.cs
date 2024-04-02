using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用状态机
/// </summary>
/// <typeparam name="T">索引类型</typeparam>
public class StateController<T>
{
    private Dictionary<T, IState<T>> m_dic = new Dictionary<T, IState<T>>();
    private IState<T> m_currentState;
    public void Init()
    {
        foreach (var pair in m_dic)
        {
            pair.Value.Init();
        }
    }
    public void DeInit()
    {
        if (m_currentState != null)
        {
            m_currentState.Exit(default(T));
            m_currentState = null;
        }

        foreach (var pair in m_dic)
        {
            pair.Value.DeInit();
        }
        m_dic.Clear();
    }
    public void OnUpdate(float deltaTime)
    {
        if (m_currentState != null)
        {
            m_currentState.OnUpdate(deltaTime);
        }
    }
    public void RegisterState(IState<T> state)
    {
        state.Init();
        if (m_dic.ContainsKey(state.GetName()))
        {
            Debug.LogError($"{this.GetType()}.RegisterState state:{state} is already have");
        }
        else
        {
            m_dic[state.GetName()] = state;
        }
    }
    public void UnRegisterState(T name)
    {
        if (m_dic.TryGetValue(name, out var state))
        {
            state.DeInit();
            m_dic.Remove(name);
        }
        else
        {
            Debug.Log($"{this.GetType()}.UnRegisterState[{name}] is dont in dic.");
        }
    }
    public void UnRegisterState(IState<T> state)
    {
        UnRegisterState(state.GetName());
    }

    public void ActiveState(T stateName)
    {
        IState<T> nextState = null;
        if (!m_dic.TryGetValue(stateName, out nextState))
        {
            var content = $"StateController Error :NextStateNotFound :{stateName}";
            Debug.LogError(content);
            return;
        }

        if (m_currentState != null)
        {
            m_currentState.Exit(nextState.GetName());
            nextState.Enter(m_currentState.GetName());
        }
        else
        {
            nextState.Enter(default(T));
        }

        m_currentState = nextState;
    }
    public void Push(int id, object args = null)
    {
        if (m_currentState != null)
        {
            m_currentState.Push(id, args);
        }
    }
}
