using Godot;
using System;

namespace GoTweening;

public partial class VirtualBuilder<T> : TweenBuilderBase, IBuilder
{
    public T StartValue { get; private set; }
    public T EndValue { get; private set; }
    public float Duration { get; private set; }
    public Action<T> OnUpdateValue { get; private set; }
    
    private Func<T, T, float, T> interpolator;
    
    public Tween.TransitionType TransitionType { get; private set; } = Tween.TransitionType.Linear;
    public Tween.EaseType EaseType { get; private set; } = Tween.EaseType.InOut;
    
    private T currentValue;
    private float elapsed;
    private int currentLoop;

    private bool isComplete;
    private bool delayComplete;

    public VirtualBuilder<T> From(T start)
    {
        StartValue = start;
        return this;
    }
    
    public VirtualBuilder<T> To(T end)
    {
        EndValue = end;
        return this;
    }
    
    public VirtualBuilder<T> SetDuration(float duration)
    {
        Duration = duration;
        return this;
    }
    
    public VirtualBuilder<T> OnUpdate(Action<T> callback)
    {
        OnUpdateValue = callback;
        return this;
    }
    
    public VirtualBuilder<T> SetInterpolator(Func<T, T, float, T> lerp)
    {
        interpolator = lerp;
        return this;
    }
    
    protected override bool ValidateBuilder()
    {
        if (OnUpdateValue == null)
        {
            GD.PushError("VirtualBuilder: OnUpdate callback is required");
            return false;
        }
        
        if (Duration <= 0f)
        {
            GD.PushError("VirtualBuilder: Duration must be > 0");
            return false;
        }
        
        if (interpolator == null)
        {
            GD.PushError("VirtualBuilder: Interpolator not set");
            return false;
        }
        
        return true;
    }
    
    protected override Tween CreateTween()
    {
        currentValue = StartValue;
        elapsed = 0f;
        currentLoop = 0;
        isComplete = false;

        delayComplete = Delay <= 0f;
        
        return null;
    }
    
    public override void Update(double delta)
    {
        if (paused || isComplete)
            return;
        
        base.Update(delta);
        
        elapsed += (float)delta;

        if (!delayComplete)
        {
            if (elapsed >= Delay)
            {
                delayComplete = true;
                elapsed -= Delay;
            }
            return;
        }

        if (Duration <= 0f) return;

        float normalizedTime = Mathf.Clamp(elapsed / Duration, 0f, 1f);
        
        float easedTime = ApplyEasing(normalizedTime);
        
        currentValue = interpolator(StartValue, EndValue, easedTime);
        
        OnUpdateValue?.Invoke(currentValue);
        
        if (elapsed >= Duration)
        {
            HandleLoopCompletion();
        }
    }
    
    private void HandleLoopCompletion()
    {
        currentLoop++;
        
        if (Loops == 0 || currentLoop < Loops)
        {
            elapsed = 0f;
            delayComplete = Delay <= 0f;
            LoopCallback?.Invoke(currentLoop);
        }
        else
        {
            isComplete = true;
            InvokeCompleted();
            GoTween.ReturnToPool(this);
        }
    }

    public void Restart()
    {
        elapsed = 0f;
        currentLoop = 0;
        isComplete = false;
    }
        
    private float ApplyEasing(float t)
    {
        float curved = ApplyTransition(t);
        
        return EaseType switch
        {
            Tween.EaseType.In => curved,
            Tween.EaseType.Out => 1f - ApplyTransition(1f - t),
            Tween.EaseType.InOut => t < 0.5f 
                ? ApplyTransition(t * 2f) * 0.5f 
                : 1f - ApplyTransition((1f - t) * 2f) * 0.5f,
            Tween.EaseType.OutIn => t < 0.5f
                ? (1f - ApplyTransition(1f - t * 2f)) * 0.5f
                : (ApplyTransition((t - 0.5f) * 2f) * 0.5f) + 0.5f,
            _ => curved
        };
    }
    
    private float ApplyTransition(float t)
    {
        return TransitionType switch
        {
            Tween.TransitionType.Sine => Mathf.Sin(t * Mathf.Pi * 0.5f),
            Tween.TransitionType.Quad => t * t,
            Tween.TransitionType.Cubic => t * t * t,
            Tween.TransitionType.Quart => t * t * t * t,
            Tween.TransitionType.Quint => t * t * t * t * t,
            Tween.TransitionType.Expo => t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f)),
            Tween.TransitionType.Circ => 1f - Mathf.Sqrt(1f - t * t),
            Tween.TransitionType.Elastic => ApplyElastic(t),
            Tween.TransitionType.Back => ApplyBack(t),
            Tween.TransitionType.Bounce => ApplyBounce(t),
            Tween.TransitionType.Spring => ApplySpring(t),
            _ => t
        };
    }
    
    private float ApplyElastic(float t)
    {
        if (t == 0f || t == 1f) return t;
        return -Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1.1f) * 5f * Mathf.Pi);
    }
    
    private float ApplyBack(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1f) * t - s);
    }
    
    private float ApplyBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        
        if (t < 1f / d1)
            return n1 * t * t;
        
        if (t < 2f / d1)
        {
            t -= 1.5f / d1;
            return n1 * t * t + 0.75f;
        }
        
        if (t < 2.5f / d1)
        {
            t -= 2.25f / d1;
            return n1 * t * t + 0.9375f;
        }
        
        t -= 2.625f / d1;
        return n1 * t * t + 0.984375f;
    }
    
    private float ApplySpring(float t)
    {
        return 1f - (Mathf.Cos(t * Mathf.Pi * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f));
    }
    
    public override void Reset()
    {
        ResetBase();
        
        StartValue = default;
        EndValue = default;
        Duration = 0f;
        OnUpdateValue = null;
        interpolator = null;
        
        TransitionType = Tween.TransitionType.Linear;
        EaseType = Tween.EaseType.InOut;
        
        currentValue = default;
        elapsed = 0f;
        currentLoop = 0;
        isComplete = false;
        delayComplete = false;
    }
    
    public override float GetTotalDuration()
    {
        return (Duration + Delay) * Mathf.Max(1, Loops);
    }
    
    #region Fluent API - Transitions
    public VirtualBuilder<T> Linear()
    {
        TransitionType = Tween.TransitionType.Linear;
        return this;
    }
    
    public VirtualBuilder<T> Sine()
    {
        TransitionType = Tween.TransitionType.Sine;
        return this;
    }
    
    public VirtualBuilder<T> Quad()
    {
        TransitionType = Tween.TransitionType.Quad;
        return this;
    }
    
    public VirtualBuilder<T> Cubic()
    {
        TransitionType = Tween.TransitionType.Cubic;
        return this;
    }
    
    public VirtualBuilder<T> Quart()
    {
        TransitionType = Tween.TransitionType.Quart;
        return this;
    }
    
    public VirtualBuilder<T> Quint()
    {
        TransitionType = Tween.TransitionType.Quint;
        return this;
    }
    
    public VirtualBuilder<T> Expo()
    {
        TransitionType = Tween.TransitionType.Expo;
        return this;
    }
    
    public VirtualBuilder<T> Circ()
    {
        TransitionType = Tween.TransitionType.Circ;
        return this;
    }
    
    public VirtualBuilder<T> Elastic()
    {
        TransitionType = Tween.TransitionType.Elastic;
        return this;
    }
    
    public VirtualBuilder<T> Back()
    {
        TransitionType = Tween.TransitionType.Back;
        return this;
    }
    
    public VirtualBuilder<T> Bounce()
    {
        TransitionType = Tween.TransitionType.Bounce;
        return this;
    }
    
    public VirtualBuilder<T> Spring()
    {
        TransitionType = Tween.TransitionType.Spring;
        return this;
    }
    #endregion
    
    #region Fluent API - Easing
    public VirtualBuilder<T> EaseIn()
    {
        EaseType = Tween.EaseType.In;
        return this;
    }
    
    public VirtualBuilder<T> EaseOut()
    {
        EaseType = Tween.EaseType.Out;
        return this;
    }
    
    public VirtualBuilder<T> EaseInOut()
    {
        EaseType = Tween.EaseType.InOut;
        return this;
    }
    
    public VirtualBuilder<T> EaseOutIn()
    {
        EaseType = Tween.EaseType.OutIn;
        return this;
    }
    #endregion
    
    #region Fluent API - Base Method Overrides
    public new VirtualBuilder<T> AddToGroup(string group)
    {
        base.AddToGroup(group);
        return this;
    }
    
    public new VirtualBuilder<T> AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        base.AddToGroup(group.ToString());
        return this;
    }
    
    public new VirtualBuilder<T> Wait(float duration)
    {
        base.Wait(duration);
        return this;
    }
    
    public new VirtualBuilder<T> SetLoops(int loops)
    {
        base.SetLoops(loops);
        return this;
    }
    
    public new VirtualBuilder<T> SetParallel(bool value = true)
    {
        base.SetParallel(value);
        return this;
    }
    
    public new VirtualBuilder<T> OnComplete(params Action[] callbacks)
    {
        base.OnComplete(callbacks);
        return this;
    }
    
    public new VirtualBuilder<T> OnLoop(Action<int> callback)
    {
        base.OnLoop(callback);
        return this;
    }
    
    public new VirtualBuilder<T> OnUpdate(Action<double> method)
    {
        base.OnUpdate(method);
        return this;
    }
    #endregion
}

