using Godot;
using System;
using System.Collections.Generic;

namespace GoTweening;

public abstract partial class TweenBuilder : RefCounted, IBuilder
{
    public enum LoopType
    {
        Normal,
        PingPong,
    }

    public event Action Completed;

    public LoopType LoopMode { get; protected set; } = LoopType.Normal;

    public GodotObject Target { get; set; }
    public Tween ActiveTween { get; set; }

    public string Property { get; set; }
    public string Group { get; set; }
    public int Loops { get; set; } = 1;

    public float SpeedScale { get; set; } = 1f;
    public double CustomStep { get; set; }

    public float Delay { get; protected set; }
    
    public bool Parallel { get; protected set; }
    public bool IsRelative { get; protected set; }

    public bool IsActive => ActiveTween != null && ActiveTween.IsRunning();

    public Tween.TweenProcessMode ProcessMode { get; protected set; }

    public Action<double> UpdateCallback { get; private set; }
    public Action<int> LoopCallback { get; private set; }

    private List<Action> completedSubs = [];

    protected abstract Tween CreateTween();
    public abstract float GetTotalDuration();
    public abstract void Reset();

    protected bool paused;

    protected bool IsVirtual => GetType().IsGenericType && GetType().GetGenericTypeDefinition() == typeof(VirtualBuilder<>);

    protected void InvokeCompleted()
    {
        Completed?.Invoke();
    }

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

    private void SetupCallbacks(Tween tween)
    {
        tween.Finished += () =>
        {
            Completed?.Invoke();
            GoTween.ReturnToPool(this);

            CancelCompletedSubs();
        };
    }

    public TweenBuilder SetProcessMode(Tween.TweenProcessMode mode)
    {
        ProcessMode = mode;
        return this;
    }

    public TweenBuilder AddToGroup(string group)
    {
        Group = group;
        return this;
    }

    public TweenBuilder AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        Group = group.ToString();
        return this;
    }

    public TweenBuilder Wait(float duration)
    {
        Delay = duration;
        return this;
    }

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

    public TweenBuilder SetSpeedScale(float scale)
    {
        SpeedScale = scale;
        return this;
    }

    public TweenBuilder SetCustomStep(double delta)
    {
        CustomStep = delta;
        return this;
    }

    public TweenBuilder LoopInfinitely(LoopType mode = LoopType.Normal)
    {
        Loops = 0;
        LoopMode = mode;
        return this;
    }

    public TweenBuilder SetParallel(bool value = true)
    {
        Parallel = value;
        return this;
    }

    public TweenBuilder AsRelative()
    {
        IsRelative = true;
        return this;
    }

    public void Pause()
    {
        ActiveTween?.Pause();
        paused = true;
    }

    public void Resume()
    {
        ActiveTween?.Play();
        paused = false;
    }

    public TweenBuilder OnComplete(params Action[] callbacks)
    {
        foreach (var callback in callbacks)
        {
            completedSubs.Add(callback);
            Completed += callback;
        }
        return this;
    }

    public TweenBuilder OnLoop(Action<int> callback)
    {
        LoopCallback = callback;
        return this;
    }

    public TweenBuilder OnUpdate(Action<double> method)
    {
        UpdateCallback = method;
        return this;
    }

    public virtual void Update(double dt)
    {
        if (paused)
            return;
        UpdateCallback?.Invoke(dt);
    }

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
    /// Use Cancel Subs to remove all OnComplete() Connected Actions;
    /// </summary>
    /// <param name="cancelCompletedSubs"></param>
    /// <returns></returns>
    public Tween Replay(bool cancelCompletedSubs = false)
    {
        ActiveTween?.Kill();
        GoTween.RemoveFromAllGroups(this);

        if (cancelCompletedSubs)
            CancelCompletedSubs();
        return Start();
    }

    private void CancelCompletedSubs()
    {
        foreach (var sub in completedSubs)
            Completed -= sub;

        completedSubs = [];
    }

    public void AddSub(Action callback)
    {
        completedSubs.Add(callback);
    }

    public void Cancel()
    {
        ActiveTween?.Kill();
        GoTween.ReturnToPool(this);
    }
}

