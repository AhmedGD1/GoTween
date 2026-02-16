using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoTweening;

public partial class GoTween
{
    public enum Direction { Left, Right, Up, Down }
    
    private static readonly RandomNumberGenerator RNG = new();

    private static PropertyBuilder Config(GodotObject target, string property, Variant endValue, float duration, Action<PropertyBuilder> config)
    {
        var builder = target.GoProperty(property).To(endValue).SetDuration(duration);
        config?.Invoke(builder);
        builder.Start();

        return builder;
    }

    #region Tween Sequence
    public static TweenSequence Sequence()
    {
        return GetSequence();
    }

    public static TweenSequence Sequence(string group)
    {
        return GetSequence(group);
    }
    
    public static TweenSequence Sequence<TGroup>(TGroup group) where TGroup : Enum
    {
        return GetSequence(group.ToString());
    }
    #endregion

    #region Management
    private static void TogglePause(bool toggle, params string[] excludedGroups)
    {   
        var excludedSet = new HashSet<string>(excludedGroups);

        foreach (var kvp in builderGroups)
        {
            if (excludedSet.Contains(kvp.Key)) 
                continue;
            
            foreach (var builder in kvp.Value)
            {
                if (toggle)
                    builder.Pause();
                else 
                    builder.Resume();
            }
        }
    }
    
    public static void GoPauseAll()
    {
        foreach (var builders in activeBuilders)
            builders.Pause();
    }

    public static void GoResumeAll()
    {
        foreach (var builders in activeBuilders)
            builders.Resume();
    } 

    public static void GoKillAll()
    {
        var builders = activeBuilders.ToList();
        foreach (var builder in builders)
            builder.GoKillSafe();
    }

    public static void GoKillTarget(GodotObject target)
    {
        var builders = activeBuilders.Where(b => b.Target == target).ToList();
        foreach (var builder in builders)
            builder.GoKillSafe();
    }

    public static void GoPauseTarget(GodotObject target)
    {
        foreach (var builder in activeBuilders)
            if (builder.Target == target)
                builder.Pause();
    }

    public static void GoPauseAll(params string[] excludedGroups) => TogglePause(true, excludedGroups);

    public static void GoResumeAll(params string[] excludedGroups) => TogglePause(false, excludedGroups);

    public static int GetActiveTweenCount() => activeBuilders.Count;
    public static int GetTweenCountForTarget(GodotObject target) => activeBuilders.Count(b => b.Target == target);

    public static bool IsPropertyTweening(GodotObject target, string property)
    {
        return activeBuilders.Any(b => b.Target == target && b.Property == property);
    }

    public static IBuilder GetTweenForProperty(GodotObject target, string property)
    {
        return activeBuilders.FirstOrDefault(b => b.Target == target && b.Property == property);
    }
    #endregion

    #region Fade Methods
    public static PropertyBuilder GoFade(CanvasItem item, float endValue, float duration, Action<PropertyBuilder> config = null) =>
        Config(item, "modulate:a", endValue, duration, config);

    public static PropertyBuilder GoFadeOut(CanvasItem item, float duration, Action<PropertyBuilder> config = null) =>   
        GoFade(item, 0f, duration, config);

    public static PropertyBuilder GoFadeIn(CanvasItem item, float duration, Action<PropertyBuilder> config = null) =>
        GoFade(item, 1f, duration, config);
    #endregion

    #region Move Methods
    public static PropertyBuilder GoMove(Node2D node, Vector2 to, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "global_position", to, duration, config);

    public static PropertyBuilder GoMove(Node3D node, Vector3 to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position", to, duration, config);

    public static PropertyBuilder GoMoveLocal(Node2D node, Vector2 to, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "position", to, duration, config);

    public static PropertyBuilder GoMoveLocal(Node3D node, Vector3 to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position", to, duration, config);

    public static PropertyBuilder GoMoveX(Node2D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position:x", to, duration, config);

    public static PropertyBuilder GoMoveY(Node2D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position:y", to, duration, config);

    public static PropertyBuilder GoMoveX(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position:x", to, duration, config);
    
    public static PropertyBuilder GoMoveY(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position:y", to, duration, config);

    public static PropertyBuilder GoMoveZ(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "global_position:z", to, duration, config);

    public static PropertyBuilder GoMoveLocalX(Node2D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position:x", to, duration, config);

    public static PropertyBuilder GoMoveLocalY(Node2D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position:y", to, duration, config);

    public static PropertyBuilder GoMoveLocalX(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position:x", to, duration, config);
    
    public static PropertyBuilder GoMoveLocalY(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position:y", to, duration, config);

    public static PropertyBuilder GoMoveLocalZ(Node3D node, float to, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "position:z", to, duration, config);
    #endregion

    #region Scale Methods
    public static PropertyBuilder GoScale(Node2D node, Vector2 value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static PropertyBuilder GoScale(Control node, Vector2 value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static PropertyBuilder GoScale(Node3D node, Vector3 value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static PropertyBuilder GoScaleX(Node2D node, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale:x", value, duration, config);

    public static PropertyBuilder GoScaleX(Control control, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(control, "scale:x", value, duration, config);

    public static PropertyBuilder GoScaleX(Node3D node, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale:x", value, duration, config);
    
    public static PropertyBuilder GoScaleY(Node2D node, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale:y", value, duration, config);
    
    public static PropertyBuilder GoScaleY(Control control, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(control, "scale:y", value, duration, config);

    public static PropertyBuilder GoScaleY(Node3D node, float value, float duration, Action<PropertyBuilder> config = null) => 
        Config(node, "scale:y", value, duration, config);

    public static PropertyBuilder GoScaleZ(Node3D node, float value, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "scale:z", value, duration, config);
    #endregion

    #region Rotation Methods
    public static PropertyBuilder GoRotate(Node node, float degrees, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "rotation_degrees", degrees, duration, config);

    public static PropertyBuilder GoRotate(Node3D node, Vector3 euler, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "rotation_degrees", euler, duration, config);

    public static PropertyBuilder GoRotateX(Node3D node, float degrees, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "rotation_degrees:x", degrees, duration, config);

    public static PropertyBuilder GoRotateY(Node3D node, float degrees, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "rotation_degrees:y", degrees, duration, config);

    public static PropertyBuilder GoRotateZ(Node3D node, float degrees, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "rotation_degrees:z", degrees, duration, config);
    #endregion

    #region Color Methods
    public static PropertyBuilder GoModulate(CanvasItem item, Color color, float duration, Action<PropertyBuilder> config = null) =>
        Config(item, "modulate", color, duration, config);
    
    public static PropertyBuilder GoSelfModulate(CanvasItem item, Color color, float duration, Action<PropertyBuilder> config = null) =>
        Config(item, "self_modulate", color, duration, config);

    public static PropertyBuilder GoColor(ColorRect colorRect, Color color, float duration, Action<PropertyBuilder> config = null) =>
        Config(colorRect, "color", color, duration, config);
    #endregion

    #region UI Methods
    public static PropertyBuilder GoAnchorMove(Control target, Vector2 endValue, float duration, Action<PropertyBuilder> config = null) =>
        Config(target, "position", endValue, duration, config);

    public static PropertyBuilder GoSize(Control target, Vector2 endValue, float duration, Action<PropertyBuilder> config = null) =>
        Config(target, "size", endValue, duration, config);
    #endregion

    #region Skew Methods
    public static PropertyBuilder GoSkew(Control node, float rad, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "skew", rad, duration, config);
    
    public static PropertyBuilder GoSkew(Node2D node, float rad, float duration, Action<PropertyBuilder> config = null) =>
        Config(node, "skew", rad, duration, config);
    #endregion

    #region Audio Methods
    public static PropertyBuilder GoVolume(AudioStreamPlayer player, float db, float duration, Action<PropertyBuilder> config = null) =>
        Config(player, "volume_db", db, duration, config);

    public static PropertyBuilder GoPitch(AudioStreamPlayer player, float scale, float duration, Action<PropertyBuilder> config = null) =>
        Config(player, "pitch_scale", scale, duration, config);
    #endregion

    #region Camera Methods
    public static PropertyBuilder GoZoom(Camera2D camera, float zoom, float duration, Action<PropertyBuilder> config = null) =>
        Config(camera, "zoom", new Vector2(zoom, zoom), duration, config);

    public static PropertyBuilder GoFOV(Camera3D camera, float degrees, float duration, Action<PropertyBuilder> config = null) =>
        Config(camera, "fov", degrees, duration, config);
    #endregion

    #region Range Methods
    public static PropertyBuilder GoRange(Godot.Range range, float value, float duration, Action<PropertyBuilder> config = null) =>
        Config(range, "value", value, duration, config);
    #endregion

    #region UI Patterns
    public static PropertyBuilder GoSlideIn(Control control, Direction from, float duration)
    {
        var screenSize = control.GetViewportRect().Size;
        var startPos = from switch
        {
            Direction.Left => new Vector2(-control.Size.X, control.Position.Y),
            Direction.Right => new Vector2(screenSize.X, control.Position.Y),
            Direction.Up => new Vector2(control.Position.X, -control.Size.Y),
            Direction.Down => new Vector2(control.Position.X, screenSize.Y),
            _ => control.Position
        };
        
        return Config(control, "position", control.Position, duration, b => b.From(startPos));
    }

    public static PropertyBuilder GoTypewriter(Label label, float duration, Action<PropertyBuilder> config = null)
    {
        var fullText = label.Text;
        label.VisibleCharacters = 0;

        return Config(label, "visible_characters", fullText, duration, config);
    }
    #endregion

    #region Shake Effects
    
    public static VirtualBuilder<float> GoShake(Node2D node, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("Shake2D: Node is invalid");
            return null;
        }
        
        var originalPos = node.Position;
        
        var builder = Virtual.Create(
            0f,
            1f,
            duration,
            (a, b, t) => t,
            progress =>
            {
                if (!IsInstanceValid(node)) return;
                
                float currentIntensity = intensity * (1f - progress);
                
                float offsetX = (RNG.Randf() * 2f - 1f) * currentIntensity;
                float offsetY = (RNG.Randf() * 2f - 1f) * currentIntensity;
                
                node.Position = originalPos + new Vector2(offsetX, offsetY);
            }
        );
        
        builder.OnComplete(() =>
        {
            if (IsInstanceValid(node))
                node.Position = originalPos;
        });
        
        config?.Invoke(builder);
        builder.Replay();
        return builder;
    }
    
    public static VirtualBuilder<float> GoShake(Node3D node, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("Shake3D: Node is invalid");
            return null;
        }
        
        var originalPos = node.Position;
        
        var builder = Virtual.Create(
            0f, 1f, duration,
            (a, b, t) => t,
            progress =>
            {
                if (!IsInstanceValid(node)) return;
                
                float currentIntensity = intensity * (1f - progress);
                
                float offsetX = (RNG.Randf() * 2f - 1f) * currentIntensity;
                float offsetY = (RNG.Randf() * 2f - 1f) * currentIntensity;
                float offsetZ = (RNG.Randf() * 2f - 1f) * currentIntensity;
                
                node.Position = originalPos + new Vector3(offsetX, offsetY, offsetZ);
            }
        );
        
        builder.OnComplete(() =>
        {
            if (IsInstanceValid(node))
                node.Position = originalPos;
        });
        
        config?.Invoke(builder);
        builder.Replay();
        return builder;
    }
    
    public static VirtualBuilder<float> GoShake(Control control, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(control))
        {
            GD.PushError("ShakeUI: Control is invalid");
            return null;
        }
        
        var originalPos = control.Position;
        
        var builder = Virtual.Create(
            0f, 1f, duration,
            (a, b, t) => t,
            progress =>
            {
                if (!IsInstanceValid(control)) return;
                
                float currentIntensity = intensity * (1f - progress);
                
                float offsetX = (RNG.Randf() * 2f - 1f) * currentIntensity;
                float offsetY = (RNG.Randf() * 2f - 1f) * currentIntensity;
                
                control.Position = originalPos + new Vector2(offsetX, offsetY);
            }
        );
        
        builder.OnComplete(() =>
        {
            if (IsInstanceValid(control))
                control.Position = originalPos;
        });
        
        config?.Invoke(builder);
        builder.Replay();
        return builder;
    }
    
    public static VirtualBuilder<float> GoShakeRotation(Node2D node, float intensityDegrees, 
        float duration, Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("ShakeRotation: Node is invalid");
            return null;
        }
        
        var originalRot = node.Rotation;
        
        var builder = Virtual.Create(
            0f, 1f, duration,
            (a, b, t) => t,
            progress =>
            {
                if (!IsInstanceValid(node)) return;
                
                float currentIntensity = intensityDegrees * (1f - progress);
                float offset = (RNG.Randf() * 2f - 1f) * Mathf.DegToRad(currentIntensity);
                
                node.Rotation = originalRot + offset;
            }
        );
        
        builder.OnComplete(() =>
        {
            if (IsInstanceValid(node))
                node.Rotation = originalRot;
        });
        
        config?.Invoke(builder);
        builder.Replay();
        return builder;
    }
    #endregion

    #region Punch Effects
    public static TweenSequence GoPunchScale(Node2D node, Vector2 punchAmount, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchScale: Node is invalid");
            return null;
        }

        var originalScale = node.Scale;
        var punchScale = originalScale + punchAmount;
        var seq = Sequence();

        config?.Invoke(seq);
        
        seq
            .Append(node.GoProperty("scale")
                .To(punchScale)
                .SetDuration(duration * 0.3f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("scale")
                .To(originalScale)
                .SetDuration(duration * 0.7f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.Scale = originalScale;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchScale(Node3D node, Vector3 punchAmount, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchScale: Node is invalid");
            return null;
        }

        var originalScale = node.Scale;
        var punchScale = originalScale + punchAmount;
        var seq = Sequence();

        config?.Invoke(seq);
        
        seq
            .Append(node.GoProperty("scale")
                .To(punchScale)
                .SetDuration(duration * 0.3f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("scale")
                .To(originalScale)
                .SetDuration(duration * 0.7f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.Scale = originalScale;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchScale(Control control, Vector2 punchAmount, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(control))
        {
            GD.PushError("GoPunchScale: control is invalid");
            return null;
        }

        var originalScale = control.Scale;
        var punchScale = originalScale + punchAmount;
        var seq = Sequence();

        config?.Invoke(seq);
        
        seq
            .Append(control.GoProperty("scale")
                .To(punchScale)
                .SetDuration(duration * 0.3f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(control.GoProperty("scale")
                .To(originalScale)
                .SetDuration(duration * 0.7f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(control))
                    control.Scale = originalScale;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchPosition(Node2D node, Vector2 punchOffset, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchPosition: Node is invalid");
            return null;
        }

        var originalPos = node.Position;
        var seq = Sequence();
        
        config?.Invoke(seq);

        seq
            .Append(node.GoProperty("position")
                .To(originalPos + punchOffset)
                .SetDuration(duration * 0.2f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("position")
                .To(originalPos)
                .SetDuration(duration * 0.8f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.Position = originalPos;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchPosition(Node3D node, Vector3 punchOffset, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchPosition: Node is invalid");
            return null;
        }

        var originalPos = node.Position;
        var seq = Sequence();
        
        config?.Invoke(seq);

        seq
            .Append(node.GoProperty("position")
                .To(originalPos + punchOffset)
                .SetDuration(duration * 0.2f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("position")
                .To(originalPos)
                .SetDuration(duration * 0.8f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.Position = originalPos;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchRotation(Node2D node, float punchDegrees, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchRotation: Node is invalid");
            return null;
        }

        var originalRot = node.RotationDegrees;
        var seq = Sequence();

        config?.Invoke(seq);

        seq
            .Append(node.GoProperty("rotation_degrees")
                .To(originalRot + punchDegrees)
                .SetDuration(duration * 0.2f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("rotation_degrees")
                .To(originalRot)
                .SetDuration(duration * 0.8f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.RotationDegrees = originalRot;
            }).Start();
        return seq;
    }

    public static TweenSequence GoPunchRotation(Node3D node, Vector3 punchDegrees, float duration, Action<TweenSequence> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("GoPunchRotation: Node is invalid");
            return null;
        }

        var originalRot = node.RotationDegrees;
        var seq = Sequence();

        config?.Invoke(seq);

        seq
            .Append(node.GoProperty("rotation_degrees")
                .To(originalRot + punchDegrees)
                .SetDuration(duration * 0.2f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out))
            .Append(node.GoProperty("rotation_degrees")
                .To(originalRot)
                .SetDuration(duration * 0.8f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out))
            .OnComplete(() =>
            {
                if (IsInstanceValid(node))
                    node.RotationDegrees = originalRot;
            }).Start();
        return seq;
    }
    #endregion

    #region Blink
    public static VirtualBuilder<int> GoBlink(CanvasItem item, int times, float duration)
    {
        return Virtual.Int(0, times, duration, current =>
        {
            if (IsInstanceValid(item))
                item.Visible = current % 2 == 0;
        }).OnComplete(() => {
            if (IsInstanceValid(item))
                item.Visible = true;
        });
    }
    #endregion
}

