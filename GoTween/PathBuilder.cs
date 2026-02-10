using Godot;
using System;

namespace GoTweening;

public partial class PathBuilder : TweenBuilderBase, IBuilder
{
    public Curve Curve { get; set; }
    public Variant InitialValue { get; private set; }
    public Variant FinalValue { get; private set; }
    public float Duration { get; private set; }

    protected override bool ValidateBuilder()
    {
        if (!base.ValidateBuilder())
            return false;

        if (Curve == null)
        {
            GD.PushError("PathBuilder Error: Curve is null.");
            return false;
        }

        if (Duration <= 0f)
        {
            GD.PushError("PathBuilder Error: Duration must be > 0.");
            return false;
        }

        return true;
    }

    protected override Tween CreateTween()
    {
        var tween = GoTween.CreateNewTween();

        if (Delay > 0f)
            tween.TweenInterval(Delay);

        bool initialValueCaptured = InitialValue.VariantType != Variant.Type.Nil;
        Variant capturedInitialValue = InitialValue;
        Variant capturedFinalValue = default;

        Callable method = Callable.From<float>(t => 
        {
            if (!initialValueCaptured)
            {
                capturedInitialValue = GoTween.GetProperty(Target, Property);
                capturedFinalValue = IsRelative ? AddVariants(capturedInitialValue, FinalValue) : FinalValue;
                initialValueCaptured = true;
            }
            
            Variant finalValue = capturedFinalValue.VariantType != Variant.Type.Nil ? capturedFinalValue : 
                                 (IsRelative ? AddVariants(capturedInitialValue, FinalValue) : FinalValue);
            
            GoTween.Interpolate(t, Target, Property, Curve, capturedInitialValue, finalValue);
        });
        
        tween.TweenMethod(method, 0f, 1f, Duration);

        return tween;
    }

    public override float GetTotalDuration()
    {
        return Duration;
    }

    public PathBuilder From(Variant value)
    {
        InitialValue = value;
        return this;
    }

    public PathBuilder To(Variant value)
    {
        FinalValue = value;
        return this;
    }

    public PathBuilder SetDuration(float duration)
    {
        Duration = duration;
        return this;
    }

    public new PathBuilder SetProcessMode(Tween.TweenProcessMode mode)
    {
        base.SetProcessMode(mode);
        return this;
    }

    public new PathBuilder AsRelative()
    {
        base.AsRelative();
        return this;
    }

    public new PathBuilder AddToGroup(string group)
    {
        base.AddToGroup(group);
        return this;
    }

    public new PathBuilder AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        Group = group.ToString();
        return this;
    }

    public new PathBuilder Wait(float duration)
    {
        base.Wait(duration);
        return this;
    }

    public new PathBuilder SetLoops(int loops)
    {
        base.SetLoops(loops);
        return this;
    }

    public new PathBuilder SetParallel(bool value = true)
    {
        base.SetParallel(value);
        return this;
    }

    public new PathBuilder OnComplete(params Action[] callbacks)
    {
        base.OnComplete(callbacks);
        return this;
    }

    public new PathBuilder OnLoop(Action<int> callback)
    {
        base.OnLoop(callback);
        return this;
    }

    public new PathBuilder OnUpdate(Action<double> method)
    {
        base.OnUpdate(method);
        return this;
    }

    public override void Reset()
    {
        ResetBase();

        Curve = null;
        Duration = 0f;

        InitialValue = default;
        FinalValue = default;
    }
}

