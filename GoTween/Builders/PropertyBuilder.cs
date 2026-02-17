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
    public PropertyBuilder Back() => SetTrans(Tween.TransitionType.Back);
    public PropertyBuilder Bounce() => SetTrans(Tween.TransitionType.Bounce);
    public PropertyBuilder Circ() => SetTrans(Tween.TransitionType.Circ);
    public PropertyBuilder Cubic() => SetTrans(Tween.TransitionType.Cubic);
    public PropertyBuilder Elastic() => SetTrans(Tween.TransitionType.Elastic);
    public PropertyBuilder Expo() => SetTrans(Tween.TransitionType.Expo);
    public PropertyBuilder Linear() => SetTrans(Tween.TransitionType.Linear);
    public PropertyBuilder Quad() => SetTrans(Tween.TransitionType.Quad);
    public PropertyBuilder Quart() => SetTrans(Tween.TransitionType.Quart);
    public PropertyBuilder Quint() => SetTrans(Tween.TransitionType.Quint);
    public PropertyBuilder Sine() => SetTrans(Tween.TransitionType.Sine);
    public PropertyBuilder Spring() => SetTrans(Tween.TransitionType.Spring);
    #endregion

    #region Easing
    public PropertyBuilder EaseIn() => SetEase(Tween.EaseType.In);
    public PropertyBuilder EaseOut() => SetEase(Tween.EaseType.Out);
    public PropertyBuilder EaseInOut() => SetEase(Tween.EaseType.InOut);
    public PropertyBuilder EaseOutIn() => SetEase(Tween.EaseType.OutIn);
    #endregion

    #region Combo
    /// <summary>Gentle acceleration and deceleration. The most neutral "feels good" motion.</summary>
    /// <remarks>Sine + EaseInOut</remarks>
    public PropertyBuilder Smooth() => Sine().EaseInOut();

    /// <summary>Starts with momentum and gradually slows to a stop. Good for things floating or sliding into place.</summary>
    /// <remarks>Sine + EaseOut</remarks>
    public PropertyBuilder Drift() => Sine().EaseOut();

    /// <summary>Heavier, more cinematic ease in and out. Good for large objects or dramatic transitions.</summary>
    /// <remarks>Quart + EaseInOut</remarks>
    public PropertyBuilder Glide() => Quart().EaseInOut();


    /// <summary>Physically bounces on arrival like a rubber ball hitting the floor. Good for playful UI or collectibles.</summary>
    /// <remarks>Bounce + EaseOut</remarks>
    public PropertyBuilder Boing() => Bounce().EaseOut();

    /// <summary>Stretches and wobbles in both directions before settling. Good for jelly-like objects or playful emphasis.</summary>
    /// <remarks>Elastic + EaseInOut</remarks>
    public PropertyBuilder Wobble() => Elastic().EaseInOut();


    /// <summary>Instant-feeling start that decelerates sharply. The go-to for responsive UI like buttons and popups.</summary>
    /// <remarks>Expo + EaseOut</remarks>
    public PropertyBuilder Snappy() => Expo().EaseOut();

    /// <summary>Smooth ramp up then snaps to the end. Good for panels or drawers sliding in.</summary>
    /// <remarks>Expo + EaseInOut</remarks>
    public PropertyBuilder FadeSnap() => Expo().EaseInOut();

    /// <summary>Overshoots the target slightly then snaps back. Great for elements appearing on screen.</summary>
    /// <remarks>Back + EaseOut</remarks>
    public PropertyBuilder Pop() => Back().EaseOut();

    /// <summary>Overshoots dramatically and oscillates before settling. Good for hit reactions or emphasis.</summary>
    /// <remarks>Elastic + EaseOut</remarks>
    public PropertyBuilder Punch() => Elastic().EaseOut();


    /// <summary>Decelerates like a vehicle coming to a stop. Good for cameras, heavy objects, or anything with physical weight.</summary>
    /// <remarks>Cubic + EaseOut</remarks>
    public PropertyBuilder Brake() => Cubic().EaseOut();

    /// <summary>Winds up slightly backward before launching forward. Good for characters or objects about to move.</summary>
    /// <remarks>Back + EaseIn</remarks>
    public PropertyBuilder Anticipate() => Back().EaseIn();
    #endregion
}

