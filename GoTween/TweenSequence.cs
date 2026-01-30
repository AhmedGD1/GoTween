using System;
using System.Collections.Generic;
using Godot;

namespace GoTweening;

public partial class TweenSequence : RefCounted
{
    private readonly List<SequenceStep> steps = new();
    private float currentTime;
    private float totalDuration;
    private bool isSequential = true;

    private Action onComplete;
    private string group;

    public class SequenceStep
    {
        public IBuilder Builder { get; set; }
        public float StartTime { get; set; }
        public float Duration { get; set; }

        public SequenceStep(IBuilder builder, float startTime, float duration)
        {
            Builder = builder;
            StartTime = startTime;
            Duration = duration;
        }
    }

    public TweenSequence(string group = null)
    {
        this.group = group;
    }

    public TweenSequence Prepend(IBuilder builder)
    {
        float duration = GetBuilderDuration(builder);
        
        foreach (var step in steps)
            step.StartTime += duration;
        
        steps.Insert(0, new SequenceStep(builder, 0f, duration));
        currentTime += duration;
        totalDuration += duration;
        
        return this;
    }

    public TweenSequence Append(IBuilder builder)
    {
        if (builder == null)
        {
            GD.PushError("TweenSequence.Append: builder is null");
            return this;
        }

        builder.ActiveTween?.Kill();

        float duration = GetBuilderDuration(builder);

        steps.Add(new SequenceStep(builder, currentTime, duration));

        currentTime += duration;
        totalDuration = Mathf.Max(totalDuration, currentTime);
        isSequential = true;
        
        return this;   
    }

    public TweenSequence Join(IBuilder builder)
    {
        if (builder == null)
        {
            GD.PushError("TweenSequence.Join: builder is null");
            return this;
        }

        if (steps.Count == 0)
        {
            GD.PushWarning("TweenSequence.Join: No previous step to join with, using Append instead");
            return Append(builder);
        }

        builder.ActiveTween?.Kill();

        float duration = GetBuilderDuration(builder);
        float startTime = isSequential ? steps[^1].StartTime : currentTime;

        steps.Add(new SequenceStep(builder, startTime, duration));

        totalDuration = Mathf.Max(totalDuration, startTime + duration);
        currentTime = startTime + duration;
        isSequential = false;

        return this;
    }

    public TweenSequence AppendCallback(Action callback)
    {
        if (steps.Count == 0)
        {
            GD.PushError("TweenSequence.AppendCallback: No previous step to attach with");
            return null;
        }

        var builder = steps[^1].Builder;
        builder.Completed += callback;
        builder.AddSub(callback);

        return this;
    }

    public TweenSequence AppendInterval(float duration)
    {
        if (duration <= 0f)
        {
            GD.PushWarning("TweenSequence.AppendInterval: duration must be > 0");
            return this;
        }

        currentTime += duration;
        totalDuration = Mathf.Max(totalDuration, currentTime);
        isSequential = true;

        return this;
    }

    public TweenSequence Insert(float atTime, IBuilder builder)
    {
        if (builder == null)
        {
            GD.PushError("TweenSequence.Insert: builder is null");
            return this;
        }

        if (atTime < 0f)
        {
            GD.PushWarning("TweenSequence.Insert: time cannot be negative, clamping to 0");
            atTime = 0f;
        }

        builder.ActiveTween?.Kill();

        float duration = GetBuilderDuration(builder);
        steps.Add(new SequenceStep(builder, atTime, duration));
        totalDuration = Mathf.Max(totalDuration, atTime + duration);

        return this;
    }

    public TweenSequence SetLoops(int loops)
    {
        if (steps.Count == 0)
        {
            GD.PushError("TweenSequence.SetLoops: No previous step to attach with");
            return null;
        }

        if (loops <= 1)
        {
            GD.PushError("Invalid Loop count: Loop Count must be greater than one to take effect");
            return null;
        }

        var builder = steps[^1].Builder;
        builder.Loops = loops;

        return this;
    }

    public TweenSequence OnComplete(Action callback)
    {
        onComplete = callback;
        return this;
    }

    public void Start()
    {
        if (steps.Count == 0)
        {
            GD.PushWarning("TweenSequence.Start: No steps in sequence");
            onComplete?.Invoke();
            return;
        }

        int completedCount = 0;
        int totalSteps = steps.Count;

        foreach (var step in steps)
        {
            var builder = step.Builder;

            if (builder is not TweenBuilderBase tweenBuilder)
            {
                GD.PushError($"TweenSequence: Builder is not a TweenBuilderBase: {builder.GetType().Name}");
                continue;
            }

            if (!string.IsNullOrEmpty(group))
                tweenBuilder.AddToGroup(group);

            float delay = step.StartTime;
            tweenBuilder.Wait(delay);

            tweenBuilder.OnComplete(() =>
            {
                completedCount++;
                if (completedCount >= totalSteps)
                {
                    onComplete?.Invoke();
                    GoTween.SequenceToPool(this);
                }
            });

            tweenBuilder.Start();
        }
    }

    public float GetDuration()
    {
        return totalDuration;
    }

    public float GetBuilderDuration(IBuilder builder)
    {
        return builder.GetTotalDuration();
    }

    public void Reset()
    {
        steps.Clear();
        currentTime = 0f;
        totalDuration = 0f;
        isSequential = true;
        onComplete = null;
        group = null;
    }
}

