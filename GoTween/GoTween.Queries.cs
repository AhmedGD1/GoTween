using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoTweening;

public partial class GoTween
{
    public enum Direction { Left, Right, Up, Down }
    
    private static readonly RandomNumberGenerator RNG = new();

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

    #region Fade Methods
    public static TweenBuilder GoFade(CanvasItem item, float endValue, float duration, Action<TweenBuilder> config = null) =>
        Config(item, "modulate:a", endValue, duration, config);

    public static TweenBuilder GoFadeOut(CanvasItem item, float duration, Action<TweenBuilder> config = null) =>   
        GoFade(item, 0f, duration, config);

    public static TweenBuilder GoFadeIn(CanvasItem item, float duration, Action<TweenBuilder> config = null) =>
        GoFade(item, 1f, duration, config);
    #endregion

    #region Move Methods
    public static TweenBuilder GoMove(Node2D node, Vector2 to, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "global_position", to, duration, config);

    public static TweenBuilder GoMove(Node3D node, Vector3 to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position", to, duration, config);

    public static TweenBuilder GoMoveLocal(Node2D node, Vector2 to, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "position", to, duration, config);

    public static TweenBuilder GoMoveLocal(Node3D node, Vector3 to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position", to, duration, config);

    public static TweenBuilder GoMoveX(Node2D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position:x", to, duration, config);

    public static TweenBuilder GoMoveY(Node2D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position:y", to, duration, config);

    public static TweenBuilder GoMoveX(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position:x", to, duration, config);
    
    public static TweenBuilder GoMoveY(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position:y", to, duration, config);

    public static TweenBuilder GoMoveZ(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "global_position:z", to, duration, config);

    public static TweenBuilder GoMoveLocalX(Node2D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position:x", to, duration, config);

    public static TweenBuilder GoMoveLocalY(Node2D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position:y", to, duration, config);

    public static TweenBuilder GoMoveLocalX(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position:x", to, duration, config);
    
    public static TweenBuilder GoMoveLocalY(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position:y", to, duration, config);

    public static TweenBuilder GoMoveLocalZ(Node3D node, float to, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "position:z", to, duration, config);
    #endregion

    #region Scale Methods
    public static TweenBuilder GoScale(Node2D node, Vector2 value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static TweenBuilder GoScale(Control node, Vector2 value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static TweenBuilder GoScale(Node3D node, Vector3 value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale", value, duration, config);

    public static TweenBuilder GoScaleX(Node2D node, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale:x", value, duration, config);

    public static TweenBuilder GoScaleX(Control control, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(control, "scale:x", value, duration, config);

    public static TweenBuilder GoScaleX(Node3D node, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale:x", value, duration, config);
    
    public static TweenBuilder GoScaleY(Node2D node, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale:y", value, duration, config);
    
    public static TweenBuilder GoScaleY(Control control, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(control, "scale:y", value, duration, config);

    public static TweenBuilder GoScaleY(Node3D node, float value, float duration, Action<TweenBuilder> config = null) => 
        Config(node, "scale:y", value, duration, config);

    public static TweenBuilder GoScaleZ(Node3D node, float value, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "scale:z", value, duration, config);
    #endregion

    #region Rotation Methods
    public static TweenBuilder Rotate2D(Node node, float degrees, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "rotation_degrees", degrees, duration, config);

    public static TweenBuilder Rotate3D(Node3D node, Vector3 euler, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "rotation_degrees", euler, duration, config);

    public static TweenBuilder RotateX(Node3D node, float degrees, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "rotation_degrees:x", degrees, duration, config);

    public static TweenBuilder RotateY(Node3D node, float degrees, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "rotation_degrees:y", degrees, duration, config);

    public static TweenBuilder RotateZ(Node3D node, float degrees, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "rotation_degrees:z", degrees, duration, config);
    #endregion

    #region Color Methods
    public static TweenBuilder GoModulate(CanvasItem item, Color color, float duration, Action<TweenBuilder> config = null) =>
        Config(item, "modulate", color, duration, config);
    
    public static TweenBuilder GoSelfModulate(CanvasItem item, Color color, float duration, Action<TweenBuilder> config = null) =>
        Config(item, "self_modulate", color, duration, config);

    public static TweenBuilder GoColor(ColorRect colorRect, Color color, float duration, Action<TweenBuilder> config = null) =>
        Config(colorRect, "color", color, duration, config);
    #endregion

    #region UI Methods
    public static TweenBuilder GoAnchorMove(Control target, Vector2 endValue, float duration, Action<TweenBuilder> config = null) =>
        Config(target, "position", endValue, duration, config);

    public static TweenBuilder GoSize(Control target, Vector2 endValue, float duration, Action<TweenBuilder> config = null) =>
        Config(target, "size", endValue, duration, config);
    #endregion

    #region Skew Methods
    public static TweenBuilder GoSkew(Control node, float rad, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "skew", rad, duration, config);
    
    public static TweenBuilder GoSkew(Node2D node, float rad, float duration, Action<TweenBuilder> config = null) =>
        Config(node, "skew", rad, duration, config);
    #endregion

    #region Audio Methods
    public static TweenBuilder GoVolume(AudioStreamPlayer player, float db, float duration, Action<TweenBuilder> config = null) =>
        Config(player, "volume_db", db, duration, config);

    public static TweenBuilder GoPitch(AudioStreamPlayer player, float scale, float duration, Action<TweenBuilder> config = null) =>
        Config(player, "pitch_scale", scale, duration, config);
    #endregion

    #region Camera Methods
    public static TweenBuilder GoZoom(Camera2D camera, float zoom, float duration, Action<TweenBuilder> config = null) =>
        Config(camera, "zoom", new Vector2(zoom, zoom), duration, config);

    public static TweenBuilder GoFOV(Camera3D camera, float degrees, float duration, Action<TweenBuilder> config = null) =>
        Config(camera, "fov", degrees, duration, config);
    #endregion

    #region UI Patterns
    public static TweenBuilder GoSlideIn(Control control, Direction from, float duration)
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

    public static TweenBuilder GoTypewriter(Label label, float duration, Action<TweenBuilder> config = null)
    {
        var fullText = label.Text;
        label.VisibleCharacters = 0;

        return Config(label, "visible_characters", fullText, duration, config);
    }
    #endregion

    #region Shake Effects
    
    /// <summary>
    /// Shakes a Node2D with decreasing intensity over duration.
    /// </summary>
    public static VirtualBuilder<float> GoShake(Node2D node, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("Shake2D: Node is invalid");
            return null;
        }
        
        var originalPos = node.Position;
        
        var builder = Virtual.Custom(
            0f,           // from
            1f,           // to (progress 0->1)
            duration,
            (a, b, t) => t, // Linear interpolation for progress
            progress =>
            {
                if (!IsInstanceValid(node)) return;
                
                // Decay: shake gets weaker as progress approaches 1
                float currentIntensity = intensity * (1f - progress);
                
                // Random offset
                float offsetX = (RNG.Randf() * 2f - 1f) * currentIntensity;
                float offsetY = (RNG.Randf() * 2f - 1f) * currentIntensity;
                
                node.Position = originalPos + new Vector2(offsetX, offsetY);
            }
        );
        
        builder.OnComplete(() =>
        {
            if (IsInstanceValid(node))
                node.Position = originalPos; // Reset to original
        });
        
        config?.Invoke(builder);
        builder.Replay();
        return builder;
    }
    
    /// <summary>
    /// Shakes a Node3D with decreasing intensity over duration.
    /// </summary>
    public static VirtualBuilder<float> GoShake(Node3D node, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("Shake3D: Node is invalid");
            return null;
        }
        
        var originalPos = node.Position;
        
        var builder = Virtual.Custom(
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
    
    /// <summary>
    /// Shakes a Control node (UI).
    /// </summary>
    public static VirtualBuilder<float> GoShake(Control control, float intensity, float duration, 
        Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(control))
        {
            GD.PushError("ShakeUI: Control is invalid");
            return null;
        }
        
        var originalPos = control.Position;
        
        var builder = Virtual.Custom(
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
    
    /// <summary>
    /// Rotation shake (useful for impacts).
    /// </summary>
    public static VirtualBuilder<float> GoShakeRotation(Node2D node, float intensityDegrees, 
        float duration, Action<VirtualBuilder<float>> config = null)
    {
        if (!IsInstanceValid(node))
        {
            GD.PushError("ShakeRotation: Node is invalid");
            return null;
        }
        
        var originalRot = node.Rotation;
        
        var builder = Virtual.Custom(
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

    private static TweenBuilder Config(GodotObject target, string property, Variant endValue, float duration, Action<TweenBuilder> config)
    {
        var builder = target.GoProperty(property).To(endValue).SetDuration(duration);
        config?.Invoke(builder);
        builder.Start();

        return builder;
    }

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
}

