namespace MVE.SalExt;

/// <summary>
/// SalStateMachine, 十分简单的状态机实现
/// </summary>
/// <typeparam name="TStateEnum">状态枚举</typeparam>
public class StateMachine<TStateEnum> where TStateEnum : Enum
{
    /// <summary>
    /// 一个状态的方法组记录
    /// </summary>
    public class StateMethodRecord
    {
        public StateMethodRecord(
            Action<TStateEnum>? enter,
            Action<TStateEnum>? exit,
            Action<double>? update,
            Action<double>? physicsUpdate
            )
        {
            OnEnter = enter;
            OnExit = exit;
            OnUpdate = update;
            OnPhysicsUpdate = physicsUpdate;
        }

        public Action<TStateEnum>? OnEnter { get; set; }

        public Action<TStateEnum>? OnExit { get; set; }

        public Action<double>? OnUpdate { get; set; }

        public Action<double>? OnPhysicsUpdate { get; set; }
    }

    protected TStateEnum currentState = default!;
    protected Dictionary<TStateEnum, StateMethodRecord> stateMethodRecords;

    /// <summary>
    /// 目前状态, 调用setter等同于调用<see cref="TravelTo(TStateEnum)"/>
    /// </summary>
    public TStateEnum State
    {
        get => currentState;
        set => TravelTo(value);
    }

    /// <summary>
    /// 是否在Travel到当前状态时重新执行Enter与Exit
    /// </summary>
    public bool AllowTravelToSelf { get; set; }

    /// <summary>
    /// 无参构造函数, 默认状态为<see cref="TStateEnum"/>的0值
    /// </summary>
    /// <param name="allowTravelToSelf">是否允许travel到自身状态, 否则此次切换不会调用对应方法</param>
    public StateMachine(bool allowTravelToSelf = false)
        : this(default!, allowTravelToSelf)
    {
    }

    /// <summary>
    /// 使用指定状态初始化对应状态
    /// </summary>
    /// <param name="state">初始状态</param>
    /// <param name="allowTravelToSelf">是否允许travel到自身状态, 否则此次切换不会调用对应方法</param>
    public StateMachine(TStateEnum state, bool allowTravelToSelf = false)
    {
        currentState = state;
        AllowTravelToSelf = allowTravelToSelf;
        stateMethodRecords = new();
    }

    /// <summary>
    /// 注册一个状态, 已有或失败会抛出异常
    /// </summary>
    /// <param name="state">状态</param>
    /// <param name="methodRecord">该状态的方法组</param>
    public void RegisterState(TStateEnum state, StateMethodRecord methodRecord)
        => stateMethodRecords.Add(state, methodRecord);


    /// <summary>
    /// 注册一个状态, 已有或失败会抛出异常
    /// </summary>
    /// <param name="state">状态</param>
    public void RegisterState(TStateEnum state,
        Action<TStateEnum>? enter = null,
        Action<TStateEnum>? exit = null,
        Action<double>? update = null,
        Action<double>? physicsUpdate = null
        )
        => stateMethodRecords.Add(state, new(enter, exit, update, physicsUpdate));

    /// <summary>
    /// 尝试注册一个状态, 成功返回<see langword="true"/>, 否则返回<see langword="false"/>
    /// </summary>
    /// <param name="state">状态</param>
    /// <param name="methodRecord">该状态的方法组</param>
    /// <returns></returns>
    public bool TryRegisterState(TStateEnum state, StateMethodRecord methodRecord)
        => stateMethodRecords.TryAdd(state, methodRecord);

    public void EnterCurrent()
        => stateMethodRecords[currentState].OnEnter?.Invoke(default!);

    /// <summary>
    /// 注册一个状态, 已有或失败会抛出异常
    /// </summary>
    /// <param name="state">状态</param>
    public void TryRegisterState(TStateEnum state,
        Action<TStateEnum>? enter = null,
        Action<TStateEnum>? exit = null,
        Action<double>? update = null,
        Action<double>? physicsUpdate = null
        )
        => stateMethodRecords.TryAdd(state, new(enter, exit, update, physicsUpdate));

    /// <summary>
    /// 切换到一个状态, 并调用状态对应的方法
    /// </summary>
    /// <param name="targetState"></param>
    public void TravelTo(TStateEnum targetState)
    {
        var current = currentState;
        var target = targetState;
        currentState = targetState;
        if (AllowTravelToSelf || !current.Equals(target))
        {
            var curMethods = stateMethodRecords[current];
            var tarMethods = stateMethodRecords[target];

            curMethods.OnExit?.Invoke(target);
            tarMethods.OnEnter?.Invoke(current);
        }
    }

    #region 额外的一些生命周期等的处理

    public void Update(double delta)
        => stateMethodRecords[currentState].OnUpdate?.Invoke(delta);

    public void PhysicsUpdate(double delta)
    => stateMethodRecords[currentState].OnPhysicsUpdate?.Invoke(delta);

    #endregion
}
