using Godot;
using System;
using System.Collections.Generic;

namespace GoTweening;

/// <summary>
/// Abstract base class for all tween builders. Manages shared configuration such as
/// loops, delay, speed, callbacks, and lifecycle — including pooling, pausing, and cancellation.
/// </summary>
/// <remarks>
/// Concrete builders such as <see cref="PropertyBuilder"/> and <see cref="PathBuilder"/>
/// extend this class and implement <see cref="CreateTween"/> to define their specific animation logic.
/// </remarks>
public abstract partial class TweenBuilder : RefCounted, IBuilder
{
    /// <summary>Defines how the tween behaves when looping.</summary>
    public enum LoopType
    {
        /// <summary>Restarts from the beginning on each loop.</summary>
        Normal,
        /// <summary>Reverses direction on each loop, animating forward then backward.</summary>
        PingPong,
    }

    /// <summary>Fired when the tween finishes all loops and completes.</summary>
    public event Action Completed;

    /// <summary>The current loop behavior. Defaults to <see cref="LoopType.Normal"/>.</summary>
    public LoopType LoopMode { get; protected set; } = LoopType.Normal;

    /// <summary>The node or object whose property is being animated.</summary>
    public GodotObject Target { get; set; }

    /// <summary>The active Godot <see cref="Tween"/> currently running, if any.</summary>
    public Tween ActiveTween { get; set; }

    /// <summary>The name of the property being animated on the target.</summary>
    public string Property { get; set; }

    /// <summary>The group this tween belongs to, used for batch control.</summary>
    public string Group { get; set; }

    /// <summary>Number of times the tween loops. 0 means infinite, 1 means play once.</summary>
    public int Loops { get; set; } = 1;

    /// <summary>Playback speed multiplier. 2.0 plays twice as fast, 0.5 at half speed.</summary>
    public float SpeedScale { get; set; } = 1f;

    /// <summary>A fixed time delta to manually advance the tween on start. Used for testing or custom playback.</summary>
    public double CustomStep { get; set; }

    /// <summary>Delay in seconds before the tween begins playing.</summary>
    public float Delay { get; protected set; }

    /// <summary>When true, all steps in this tween run simultaneously.</summary>
    public bool Parallel { get; protected set; }

    /// <summary>When true, animation values are treated as offsets rather than absolute targets.</summary>
    public bool IsRelative { get; protected set; }

    /// <summary>Returns true if the tween exists and is currently running.</summary>
    public bool IsActive => ActiveTween != null && ActiveTween.IsRunning();

    /// <summary>Whether the tween processes during idle or physics frames.</summary>
    public Tween.TweenProcessMode ProcessMode { get; protected set; }

    /// <summary>Optional callback invoked every frame while the tween is active. Receives delta time in seconds.</summary>
    public Action<double> UpdateCallback { get; private set; }

    /// <summary>Optional callback invoked at the start of each loop. Receives the zero-based loop index.</summary>
    public Action<int> LoopCallback { get; private set; }

    private List<Action> completedSubs = [];

    /// <summary>Implemented by subclasses to construct and return the configured <see cref="Tween"/>.</summary>
    protected abstract Tween CreateTween();

    /// <summary>Implemented by subclasses to return the total expected duration in seconds.</summary>
    public abstract float GetTotalDuration();

    /// <summary>Implemented by subclasses to reset all builder-specific state for reuse.</summary>
    public abstract void Reset();

    protected bool paused;

    /// <summary>Returns true if this builder is a <see cref="VirtualBuilder{T}"/> instance.</summary>
    protected bool IsVirtual => GetType().IsGenericType && GetType().GetGenericTypeDefinition() == typeof(VirtualBuilder<>);

    /// <summary>Fires the <see cref="Completed"/> event manually.</summary>
    protected void InvokeCompleted()
    {
        Completed?.Invoke();
    }

    /// <summary>
    /// Validates, builds, and starts the tween. Returns the active <see cref="Tween"/> on success.
    /// If validation fails or the tween cannot be created, the builder is returned to the pool
    /// and <c>null</c> is returned.
    /// </summary>
    /// <returns>The running <see cref="Tween"/>, or <c>null</c> if startup failed.</returns>
    public Tween Start()
    {
        if (!ValidateBuilder())
        {
            GoTween.ReturnToPool(this);
            return null;
        }
        
        var tween = CreateTween().SetProcessMode(ProcessMode);
        
        if (tween != null)
        {
            ActiveTween = tween;
            SetupLoops(tween);
            SetupCallbacks(tween);

            if (CustomStep != 0f)
                tween.CustomStep(CustomStep);
        }
        else if (IsVirtual)
        {
            ActiveTween = null;
        }
        else
        {
            GoTween.ReturnToPool(this);
            return null;
        }

        GoTween.AddActiveBuilder(this);
        return tween;
    }

    /// <summary>
    /// Validates that the builder is in a runnable state. Virtual builders always pass.
    /// Returns false and pushes an error if the target is null or has been freed.
    /// </summary>
    protected virtual bool ValidateBuilder()
    {
        if (IsVirtual)
            return true;
            
        if (!IsInstanceValid(Target))
        {
            GD.PushError($"{GetType().Name} Error: Target is invalid or has been freed.");
            return false;
        }
        return true;
    }

    /// <summary>Applies loop count, parallel mode, speed scale, and the loop callback to the tween.</summary>
    private void SetupLoops(Tween tween)
    {
        tween.SetLoops(Loops).SetParallel(Parallel).SetSpeedScale(SpeedScale);

        if (LoopCallback != null)
        {
            if (Loops == 1)
                GD.PushWarning($"{GetType().Name}: OnLoop called but no loops set (loops = 1)");
            else
                tween.LoopFinished += current => LoopCallback.Invoke((int)current);
        }
    }

    /// <summary>
    /// Wires up the tween's finished signal to fire <see cref="Completed"/>,
    /// return the builder to the pool, and clean up completion subscribers.
    /// </summary>
    private void SetupCallbacks(Tween tween)
    {
        tween.Finished += () =>
        {
            Completed?.Invoke();
            GoTween.ReturnToPool(this);

            CancelCompletedSubs();
        };
    }

    /// <summary>Sets whether the tween processes during idle or physics frames.</summary>
    /// <param name="mode">The process mode to use.</param>
    public TweenBuilder SetProcessMode(Tween.TweenProcessMode mode)
    {
        ProcessMode = mode;
        return this;
    }

    /// <summary>Adds this tween to a named group for batch control.</summary>
    /// <param name="group">The group name.</param>
    public TweenBuilder AddToGroup(string group)
    {
        Group = group;
        return this;
    }

    /// <summary>Adds this tween to a group using an enum value as the group name.</summary>
    /// <param name="group">The enum value whose name is used as the group identifier.</param>
    public TweenBuilder AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        Group = group.ToString();
        return this;
    }

    /// <summary>Adds a delay in seconds before the tween starts.</summary>
    /// <param name="duration">Delay duration in seconds.</param>
    public TweenBuilder Wait(float duration)
    {
        Delay = duration;
        return this;
    }

    /// <summary>Sets the number of times the tween repeats and how it loops.</summary>
    /// <param name="loops">Number of times to loop. Must be 1 or greater. Use <see cref="LoopInfinitely"/> for infinite loops.</param>
    /// <param name="mode">Normal replays from start. PingPong reverses direction each loop.</param>
    /// <returns><c>null</c> if loops is less than 1, otherwise the builder for chaining.</returns>
    public TweenBuilder SetLoops(int loops, LoopType mode = LoopType.Normal)
    {
        if (loops < 1)
        {
            GD.PushError("GoTween: Loops must be greater than 1, if you want infinite loops, call: LoopInifinitely() method");
            return null;
        }

        LoopType appliedMode = mode;

        Loops = loops;
        LoopMode = appliedMode;
        return this;
    }

    /// <summary>Scales the playback speed of the tween. 2.0 plays twice as fast, 0.5 plays at half speed.</summary>
    /// <param name="scale">Speed multiplier.</param>
    public TweenBuilder SetSpeedScale(float scale)
    {
        SpeedScale = scale;
        return this;
    }

    /// <summary>Manually advances the tween by a fixed delta on start. Useful for frame-accurate testing or custom playback.</summary>
    /// <param name="delta">The time delta in seconds to advance by.</param>
    public TweenBuilder SetCustomStep(double delta)
    {
        CustomStep = delta;
        return this;
    }

    /// <summary>Loops the tween indefinitely until manually stopped.</summary>
    /// <param name="mode">Normal replays from start. PingPong reverses direction each loop.</param>
    public TweenBuilder LoopInfinitely(LoopType mode = LoopType.Normal)
    {
        Loops = 0;
        LoopMode = mode;
        return this;
    }

    /// <summary>When enabled, all steps in this tween run simultaneously instead of in sequence.</summary>
    /// <param name="value">Pass <c>false</c> to revert to sequential mode.</param>
    public TweenBuilder SetParallel(bool value = true)
    {
        Parallel = value;
        return this;
    }

    /// <summary>Treats animation values as offsets from the current property value rather than absolute targets.</summary>
    public TweenBuilder AsRelative()
    {
        IsRelative = true;
        return this;
    }

    /// <summary>Pauses the tween at its current position. Resume with <see cref="Resume"/>.</summary>
    public void Pause()
    {
        ActiveTween?.Pause();
        paused = true;
    }

    /// <summary>Resumes a paused tween from where it left off.</summary>
    public void Resume()
    {
        ActiveTween?.Play();
        paused = false;
    }

    /// <summary>Registers one or more callbacks to invoke when the tween completes.</summary>
    /// <param name="callbacks">One or more actions to call on completion.</param>
    public TweenBuilder OnComplete(params Action[] callbacks)
    {
        foreach (var callback in callbacks)
        {
            completedSubs.Add(callback);
            Completed += callback;
        }
        return this;
    }

    /// <summary>Registers a callback invoked at the start of each loop iteration.</summary>
    /// <param name="callback">Receives the zero-based loop index.</param>
    public TweenBuilder OnLoop(Action<int> callback)
    {
        LoopCallback = callback;
        return this;
    }

    /// <summary>Registers a callback invoked every frame while the tween is running.</summary>
    /// <param name="method">Receives the elapsed delta time in seconds.</param>
    public TweenBuilder OnUpdate(Action<double> method)
    {
        UpdateCallback = method;
        return this;
    }

    /// <summary>
    /// Called every frame by the tween system. Invokes <see cref="UpdateCallback"/> unless paused.
    /// </summary>
    /// <param name="dt">Delta time in seconds since the last frame.</param>
    public virtual void Update(double dt)
    {
        if (paused)
            return;
        UpdateCallback?.Invoke(dt);
    }

    /// <summary>
    /// Resets all shared base state to defaults. Called by subclass <see cref="Reset"/> implementations.
    /// </summary>
    protected void ResetBase()
    {
        LoopMode = LoopType.Normal;

        Target = null;
        ActiveTween = null;
        Property = null;
        Group = null;

        Delay = 0f;
        Loops = 1;
        Parallel = false;
        paused = false;

        LoopCallback = null;
        UpdateCallback = null;

        CancelCompletedSubs();
    }

    /// <summary>
    /// Kills the active tween and restarts it from the beginning.
    /// </summary>
    /// <param name="cancelCompletedSubs">
    /// If <c>true</c>, all callbacks registered via <see cref="OnComplete"/> are unsubscribed before replaying.
    /// </param>
    /// <returns>The new running <see cref="Tween"/>.</returns>
    public Tween Replay(bool cancelCompletedSubs = false)
    {
        ActiveTween?.Kill();
        GoTween.RemoveFromAllGroups(this);

        if (cancelCompletedSubs)
            CancelCompletedSubs();
        return Start();
    }

    /// <summary>Unsubscribes all callbacks registered via <see cref="OnComplete"/> and clears the subscription list.</summary>
    private void CancelCompletedSubs()
    {
        foreach (var sub in completedSubs)
            Completed -= sub;

        completedSubs = [];
    }

    /// <summary>
    /// Registers a callback to the completion subscription list without going through <see cref="OnComplete"/>.
    /// Used internally for managed subscriptions that need to be tracked separately.
    /// </summary>
    /// <param name="callback">The action to add to the subscription list.</param>
    public void AddSub(Action callback)
    {
        completedSubs.Add(callback);
    }

    /// <summary>Kills the active tween and returns the builder to the pool.</summary>
    public void Cancel()
    {
        ActiveTween?.Kill();
        GoTween.ReturnToPool(this);
    }

    /// <summary>
    /// Returns a normalized value between 0 and 1 representing how far through the tween's total duration it is.
    /// Returns 0 if the tween is invalid or not running.
    /// </summary>
    public virtual float GetProgress()
    {
        if (!IsInstanceValid(ActiveTween) || !ActiveTween.IsValid())
            return 0f;
        
        float elapsed = (float)ActiveTween.GetTotalElapsedTime();
        float total = GetTotalDuration();

        return total > 0f ? Mathf.Clamp(elapsed / total, 0f, 1f) : 0f;
    }

    /// <summary>
    /// Returns the total time in seconds the tween has been running.
    /// Returns 0 if no active tween exists.
    /// </summary>
    public virtual float GetElapsedTime()
    {
        return ActiveTween != null ? (float)ActiveTween.GetTotalElapsedTime() : 0f;
    }

    /// <summary>
    /// Returns the estimated time in seconds remaining until the tween completes.
    /// Returns 0 if the tween is invalid or has already finished.
    /// </summary>
    public virtual float GetRemainingTime()
    {
        if (ActiveTween == null || !ActiveTween.IsValid())
            return 0f;
        
        float elapsed = (float)ActiveTween.GetTotalElapsedTime();
        float total = GetTotalDuration();
        
        return Mathf.Max(0f, total - elapsed);
    }
}

