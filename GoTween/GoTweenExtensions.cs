using Godot;
using System;
using System.Collections.Generic;

namespace GoTweening;

public static class GoTweenExtensions
{
    public static TweenBuilder GoProperty(this GodotObject @object, string property)
    {
        return GoTween.GetPool<TweenBuilder>(@object, property);
    }

    public static Tween GoProperty(this GodotObject @object, string property, Action<TweenBuilder> config)
    {
        var builder = GoTween.GetPool<TweenBuilder>(@object, property);
        config(builder);
        return builder.Start();
    }

    public static PathBuilder GoPath(this GodotObject @object, string property, Curve curve)
    {
        var builder = GoTween.GetPool<PathBuilder>(@object, property);
        builder.Curve = curve;

        return builder;
    }

    public static Tween GoPath(this GodotObject @object, string property, Curve curve, Action<PathBuilder> config)
    {
        var builder = GoTween.GetPool<PathBuilder>(@object, property);
        builder.Curve = curve;

        config(builder);
        return builder.Start();
    }

    public static void GoKillTweens(this GodotObject @object)
    {
        var buildersToKill = new List<IBuilder>();

        foreach (var builder in GoTween.activeBuilders)
        {
            if (builder.Target == @object)
            {
                buildersToKill.Add(builder);
            }
        }
        
        foreach (var builder in buildersToKill)
        {
            builder.ActiveTween?.Kill();
            GoTween.ReturnToPool(builder);
        }
    }

    public static void GoKillSafe(this IBuilder builder)
    {
        builder.ActiveTween?.Kill();
        GoTween.ReturnToPool(builder);
    }
}

public interface IBuilder
{
    event Action Completed;

    GodotObject Target { get; set; }
    Tween ActiveTween { get; set; }
    string Property { get; set; }
    string Group { get; set; }
    int Loops { get; set; }

    void Pause();
    void Resume();
    void Cancel();

    void Reset();
    void Update(double delta);
    void AddSub(Action callback);

    Tween Start();
    Tween Replay();
    
    float GetTotalDuration();
}