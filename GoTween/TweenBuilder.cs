using Godot;
using System;
using System.Linq;

namespace GoTweening;

public partial class TweenBuilder : TweenBuilderBase, IBuilder
{
    public Variant[] Values { get; private set; }
    public float[] Durations { get; private set; }

    public Tween.TransitionType TransitionType { get; private set; }
    public Tween.EaseType EaseType { get; private set; }

    public Action<int> StepCallback { get; private set; }

    private Variant from;

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
        if (IsRelative && (Values.Length > 1 || Durations.Length > 1))
        {
            GD.PushError("Invalid As Relative usage !, To Use Relative, To() & SetDuration() Must Inculde only one 1 value");
            return null;
        }

        var tween = GoTween.CreateNewTween();
        tween.SetTrans(TransitionType).SetEase(EaseType);

        Variant startValue = from.VariantType != Variant.Type.Nil ? from : GoTween.GetProperty(Target, Property);
        if (Delay > 0f)
            tween.TweenInterval(Delay);

        for (int i = 0; i < Values.Length; i++)
        {
            if (!IsInstanceValid(Target))
                return null;

            if (IsRelative)
                Values[i] = AddVariants(startValue, Values[i]);
            PropertyTweener prop = tween.TweenProperty(Target, Property, Values[i], Durations[i]);

            if (i == 0)
                prop.From(startValue);
            else
                prop.From(Values[i - 1]);
        }

        if (StepCallback != null)
        {
            tween.StepFinished += step => StepCallback.Invoke((int)step);
        }

        return tween;
    }

    public override float GetTotalDuration()
    {
        if (Durations == null || Durations.Length == 0)
            return 0f;
        return Durations.Sum();
    }

    public TweenBuilder From(Variant value)
    {
        from = value;
        return this;
    }

    public TweenBuilder To(params Variant[] value)
    {
        Values = value;
        return this;
    }

    public TweenBuilder SetDuration(params float[] value)
    {
        Durations = value;
        return this;
    }

    public TweenBuilder OnStep(Action<int> callback)
    {
        StepCallback = callback;
        return this;
    }

    public new TweenBuilder SetProcessMode(Tween.TweenProcessMode mode)
    {
        base.SetProcessMode(mode);
        return this;
    }

    public new TweenBuilder AsRelative()
    {
        base.AsRelative();
        return this;
    }

    public new TweenBuilder AddToGroup(string group)
    {
        base.AddToGroup(group);
        return this;
    }

    public new TweenBuilder AddToGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        Group = group.ToString();
        return this;
    }

    public new TweenBuilder Wait(float duration)
    {
        base.Wait(duration);
        return this;
    }

    public new TweenBuilder SetLoops(int loops)
    {
        base.SetLoops(loops);
        return this;
    }

    public new TweenBuilder SetParallel(bool value = true)
    {
        base.SetParallel(value);
        return this;
    }

    public new TweenBuilder OnComplete(params Action[] callbacks)
    {
        base.OnComplete(callbacks);
        return this;
    }

    public new TweenBuilder OnLoop(Action<int> callback)
    {
        base.OnLoop(callback);
        return this;
    }

    public new TweenBuilder OnUpdate(Action<double> method)
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
        from = default;
    }

    public TweenBuilder SetTrans(Tween.TransitionType type)
    {
        TransitionType = type;
        return this;
    }

    public TweenBuilder SetEase(Tween.EaseType type)
    {
        EaseType = type;
        return this;
    }

    #region Transitioning
    public TweenBuilder Back()
    {
        TransitionType = Tween.TransitionType.Back;
        return this;
    }
    public TweenBuilder Bounce()
    {
        TransitionType = Tween.TransitionType.Bounce;
        return this;
    }
    public TweenBuilder Circ()
    {
        TransitionType = Tween.TransitionType.Circ;
        return this;
    }
    public TweenBuilder Cubic()
    {
        TransitionType = Tween.TransitionType.Cubic;
        return this;
    }
    public TweenBuilder Elastic()
    {
        TransitionType = Tween.TransitionType.Elastic;
        return this;
    }
    public TweenBuilder Expo()
    {
        TransitionType = Tween.TransitionType.Expo;
        return this;
    }
    public TweenBuilder Linear()
    {
        TransitionType = Tween.TransitionType.Linear;
        return this;
    }
    public TweenBuilder Quad()
    {
        TransitionType = Tween.TransitionType.Quad;
        return this;
    }
    public TweenBuilder Quart()
    {
        TransitionType = Tween.TransitionType.Quart;
        return this;
    }
    public TweenBuilder Quint()
    {
        TransitionType = Tween.TransitionType.Quint;
        return this;
    }
    public TweenBuilder Sine()
    {
        TransitionType = Tween.TransitionType.Sine;
        return this;
    }
    public TweenBuilder Spring()
    {
        TransitionType = Tween.TransitionType.Spring;
        return this;
    }
    #endregion

    #region Easing
    public TweenBuilder EaseIn()
    {
        EaseType = Tween.EaseType.In;
        return this;
    }

    public TweenBuilder EaseOut()
    {
        EaseType = Tween.EaseType.Out;
        return this;
    }

    public TweenBuilder EaseInOut()
    {
        EaseType = Tween.EaseType.InOut;
        return this;
    }

    public TweenBuilder EaseOutIn()
    {
        EaseType = Tween.EaseType.OutIn;
        return this;
    }
    #endregion
}