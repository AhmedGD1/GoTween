using Godot;
using System;
using System.Linq;

namespace GoTweening;

public partial class PropertyBuilder : TweenBuilder, IBuilder
{
    public Variant[] Values { get; private set; }
    public float[] Durations { get; private set; }

    public Tween.TransitionType TransitionType { get; private set; }
    public Tween.EaseType EaseType { get; private set; }

    public Action<int> StepCallback { get; private set; }

    private Variant InitialValue { get; set; }

    protected override bool ValidateBuilder()
    {
        if (!base.ValidateBuilder())
            return false;

        if (Values == null || Durations == null)
        {
            GD.PushError("GTween Error: Must specify Values and Durations using To() and SetDuration().");
            return false;
        }

        if (Values.Length == 0 || Values.Length != Durations.Length)
        {
            GD.PushError("GTween Error: Values count must match Durations count and be > 0.");
            return false;
        }

        return true;
    }

    protected override Tween CreateTween()
    {
        var tween = GoTween.CreateNewTween();
        tween.SetTrans(TransitionType).SetEase(EaseType);

        if (Delay > 0f)
            tween.TweenInterval(Delay);
            
        Variant startValue = InitialValue.VariantType != Variant.Type.Nil ? InitialValue : GoTween.GetProperty(Target, Property);
        
        if (IsRelative)
        {
            Variant accumulated = startValue;

            for (int i = 0; i < Values.Length; i++)
            {
                accumulated = VariantMath.Add(accumulated, Values[i]);
                Values[i] = accumulated;
            }
        }

        if (LoopMode == LoopType.PingPong)
        {
            Variant[] pingPongValues = new Variant[Values.Length * 2 - 1];
            float[] pingPongDurations = new float[Values.Length * 2 - 1];
            
            for (int i = 0; i < Values.Length; i++)
            {
                pingPongValues[i] = Values[i];
                pingPongDurations[i] = Durations[i];
            }
            
            for (int i = Values.Length - 2; i >= 0; i--)
            {
                pingPongValues[Values.Length + (Values.Length - 2 - i)] = Values[i];
                pingPongDurations[Values.Length + (Values.Length - 2 - i)] = Durations[i];
            }

            Variant[] finalValues = new Variant[pingPongValues.Length + 1];
            float[] finalDurations = new float[pingPongDurations.Length + 1];
            
            Array.Copy(pingPongValues, finalValues, pingPongValues.Length);
            Array.Copy(pingPongDurations, finalDurations, pingPongDurations.Length);
            
            finalValues[^1] = startValue;
            finalDurations[^1] = Durations[Values.Length - 1];

            Values = finalValues;
            Durations = finalDurations;
        }

        for (int i = 0; i < Values.Length; i++)
        {
            if (!IsInstanceValid(Target))
                return null;
            PropertyTweener prop = tween.TweenProperty(Target, Property, Values[i], Durations[i]);

            prop.From(i == 0 ? startValue : Values[i - 1]);
        }

        if (StepCallback != null)
            tween.StepFinished += step => StepCallback.Invoke((int)step);
        
        return tween;
    }

    public override float GetTotalDuration()
    {
        if (Durations == null || Durations.Length == 0)
            return 0f;
        
        float singleLoopDuration = Durations.Sum();
        
        if (LoopMode == LoopType.PingPong)
            singleLoopDuration *= 2;
        
        if (Loops == 0 || Loops == 1)
            return singleLoopDuration;
        
        return singleLoopDuration * Loops;
    }

    public PropertyBuilder From(Variant value)
    {
        InitialValue = value;
        return this;
    }

    public PropertyBuilder To(params Variant[] value)
    {
        Values = value;
        return this;
    }

    public PropertyBuilder SetDuration(params float[] value)
    {
        Durations = value;
        return this;
    }

    public PropertyBuilder OnStep(Action<int> callback)
    {
        StepCallback = callback;
        return this;
    }

    public new PropertyBuilder SetProcessMode(Tween.TweenProcessMode mode)
    {
        base.SetProcessMode(mode);
        return this;
    }

    public new PropertyBuilder AsRelative()
    {
        base.AsRelative();
        return this;
    }

    public new PropertyBuilder AddToGroup(string group)
    {
        base.AddToGroup(group);
        return this;
    }

    public new PropertyBuilder AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        Group = group.ToString();
        return this;
    }

    public new PropertyBuilder Wait(float duration)
    {
        base.Wait(duration);
        return this;
    }

    public new PropertyBuilder SetLoops(int loops, LoopType mode = LoopType.Normal)
    {
        base.SetLoops(loops, mode);
        return this;
    }

    public new PropertyBuilder LoopInfinitely(LoopType mode = LoopType.Normal)
    {
        base.LoopInfinitely(mode);
        return this;
    }

    public new PropertyBuilder SetParallel(bool value = true)
    {
        base.SetParallel(value);
        return this;
    }

    public new PropertyBuilder SetSpeedScale(float scale)
    {
        base.SetSpeedScale(scale);
        return this;
    }

    public new PropertyBuilder SetCustomStep(double delta)
    {
        base.SetCustomStep(delta);
        return this;
    }

    public new PropertyBuilder OnComplete(params Action[] callbacks)
    {
        base.OnComplete(callbacks);
        return this;
    }

    public new PropertyBuilder OnLoop(Action<int> callback)
    {
        base.OnLoop(callback);
        return this;
    }

    public new PropertyBuilder OnUpdate(Action<double> method)
    {
        base.OnUpdate(method);
        return this;
    }

    public override void Reset()
    {
        ResetBase();

        Values = default;
        Durations = default;

        TransitionType = Tween.TransitionType.Linear;
        EaseType = Tween.EaseType.In;

        StepCallback = null;
        InitialValue = default;
    }

    public PropertyBuilder SetTrans(Tween.TransitionType type)
    {
        TransitionType = type;
        return this;
    }

    public PropertyBuilder SetEase(Tween.EaseType type)
    {
        EaseType = type;
        return this;
    }

    #region Transitioning
    public PropertyBuilder Back()
    {
        TransitionType = Tween.TransitionType.Back;
        return this;
    }
    public PropertyBuilder Bounce()
    {
        TransitionType = Tween.TransitionType.Bounce;
        return this;
    }
    public PropertyBuilder Circ()
    {
        TransitionType = Tween.TransitionType.Circ;
        return this;
    }
    public PropertyBuilder Cubic()
    {
        TransitionType = Tween.TransitionType.Cubic;
        return this;
    }
    public PropertyBuilder Elastic()
    {
        TransitionType = Tween.TransitionType.Elastic;
        return this;
    }
    public PropertyBuilder Expo()
    {
        TransitionType = Tween.TransitionType.Expo;
        return this;
    }
    public PropertyBuilder Linear()
    {
        TransitionType = Tween.TransitionType.Linear;
        return this;
    }
    public PropertyBuilder Quad()
    {
        TransitionType = Tween.TransitionType.Quad;
        return this;
    }
    public PropertyBuilder Quart()
    {
        TransitionType = Tween.TransitionType.Quart;
        return this;
    }
    public PropertyBuilder Quint()
    {
        TransitionType = Tween.TransitionType.Quint;
        return this;
    }
    public PropertyBuilder Sine()
    {
        TransitionType = Tween.TransitionType.Sine;
        return this;
    }
    public PropertyBuilder Spring()
    {
        TransitionType = Tween.TransitionType.Spring;
        return this;
    }
    #endregion

    #region Easing
    public PropertyBuilder EaseIn()
    {
        EaseType = Tween.EaseType.In;
        return this;
    }

    public PropertyBuilder EaseOut()
    {
        EaseType = Tween.EaseType.Out;
        return this;
    }

    public PropertyBuilder EaseInOut()
    {
        EaseType = Tween.EaseType.InOut;
        return this;
    }

    public PropertyBuilder EaseOutIn()
    {
        EaseType = Tween.EaseType.OutIn;
        return this;
    }
    #endregion
}