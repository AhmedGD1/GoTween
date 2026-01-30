using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoTweening;

public partial class GoTween : Node
{
    private const int MaxPoolSize = 1000;

    private static GoTween instance;

    /// <summary>
    /// All active tween builders.
    /// </summary>
    public readonly static HashSet<IBuilder> activeBuilders = new();

    /// <summary>
    /// Groups of tween builders by group name.
    /// </summary>
    public readonly static Dictionary<string, HashSet<IBuilder>> builderGroups = new(128);

    /// <summary>
    /// Pool of tween builders by type.
    /// </summary>
    private readonly static Dictionary<Type, Queue<object>> pool = [];

    /// <summary>
    /// Pool of tween sequences.
    /// </summary>
    private readonly static Queue<TweenSequence> sequencePool = new();

    /// <summary>
    /// Cache for updating builders to avoid modifying the collection during iteration.
    /// </summary>
    private readonly static List<IBuilder> updateCache = new();

    public override void _Ready()
    {
        instance = this;
    }

    /// <summary>
    /// Updates all active tween builders.
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
        updateCache.Clear();
        updateCache.AddRange(activeBuilders);

        for (int i = 0; i < updateCache.Count; i++)
        {
            updateCache[i].Update(delta);
        }
    }

    /// <summary>
    /// Creates a new tween instance.
    /// </summary>
    /// <returns></returns>
    public static Tween CreateNewTween()
    {
        return instance.CreateTween();
    }

    /// <summary>
    /// Gets a tween builder from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="object"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static T GetPool<T>(GodotObject @object = null, string property = null) where T : TweenBuilderBase, IBuilder, new()
    {
        var type = typeof(T);
        
        if (!pool.ContainsKey(type))
            pool[type] = new();

        var typePool = pool[type];
        T builder = typePool.Count == 0 ? new T() : (T)typePool.Dequeue();
        
        builder.Target = @object;
        builder.Property = property;
        
        return builder;
    }

    /// <summary>
    /// Returns a tween builder to the pool.
    /// </summary>
    /// <param name="builder"></param>
    public static void ReturnToPool(IBuilder builder)
    {
        if (builder == null)
            return;

        activeBuilders.Remove(builder);
        RemoveFromGroup(builder);
        builder.Reset();

        var type = builder.GetType();
        if (!pool.ContainsKey(type))
            pool[type] = new Queue<object>();
            
        if (pool[type].Count >= MaxPoolSize)
            return;
        
        pool[type].Enqueue(builder);
    }

    /// <summary>
    /// Adds a builder to the active builders list and its group if applicable.
    /// </summary>
    /// <param name="builder"></param>
    public static void AddActiveBuilder(IBuilder builder)
    {
        if (builder == null)
            return;

        activeBuilders.Add(builder);

        if (!string.IsNullOrEmpty(builder.Group))
        {
            if (!builderGroups.ContainsKey(builder.Group))
                builderGroups[builder.Group] = new();
            builderGroups[builder.Group].Add(builder);
        }
    }

    /// <summary>
    /// Interpolates a property on an object using a curve between two Variant values.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="object"></param>
    /// <param name="property"></param>
    /// <param name="curve"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Interpolate(float t, GodotObject @object, string property, Curve curve, Variant a, Variant b)
    {
        if (!IsInstanceValid(@object) || curve == null) 
            return;
        
        float sample = curve.Sample(t);

        Variant result = a.VariantType switch
        {
            Variant.Type.Float => Mathf.Lerp((float)a, (float)b, sample),
            Variant.Type.Int => Mathf.Lerp((int)a, (int)b, sample),

            Variant.Type.Vector2 => ((Vector2)a).Lerp((Vector2)b, sample),
            Variant.Type.Vector3 => ((Vector3)a).Lerp((Vector3)b, sample),

            Variant.Type.Quaternion => ((Quaternion)a).Slerp((Quaternion)b, sample),

            Variant.Type.Color => ((Color)a).Lerp((Color)b, sample),
            _ => throw new ArgumentException($"Invalid Type For Path Tweening: {a.VariantType}")
        };

        @object.Set(property, result);
    }

    /// <summary>
    /// Returns a tween sequence to the pool.
    /// </summary>
    /// <param name="sequence"></param>
    public static void SequenceToPool(TweenSequence sequence)
    {
        sequence.Reset();
        sequencePool.Enqueue(sequence);
    }

    /// <summary>
    /// Gets a tween sequence from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <param name="groupIfNew"></param>
    /// <returns></returns>
    public static TweenSequence GetSequence(string groupIfNew = null)
    {
        return sequencePool.Count != 0 ? sequencePool.Dequeue() : new(groupIfNew);
    }

    /// <summary>
    /// Removes a builder from its group.
    /// </summary>
    /// <param name="builder"></param>
    public static void RemoveFromGroup(IBuilder builder)
    {
        if (builder == null)
            return;

        if (string.IsNullOrEmpty(builder.Group))
            return;
        
        if (!builderGroups.TryGetValue(builder.Group, out var builders))
            return;
        
        builders.Remove(builder);

        if (builders.Count == 0)
            builderGroups.Remove(builder.Group);
    }

    /// <summary>
    /// Removes a builder from all groups.
    /// </summary>
    /// <param name="builder"></param>
    public static void RemoveFromAllGroups(IBuilder builder)
    {
        if (builder == null)
            return;
        
        foreach (var groupSet in builderGroups.Values)
            groupSet.Remove(builder);
        
        var emptyGroups = builderGroups
            .Where(kvp => kvp.Value.Count == 0)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var emptyGroup in emptyGroups)
            builderGroups.Remove(emptyGroup);
    }

    #region Virtual Class
    /// <summary>
    /// Virtual tweens that don't target a specific object property.
    /// </summary>
    public static class Virtual
    {
        public static VirtualBuilder<float> Float(float from, float to, float duration, Action<float> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<float>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator(Mathf.Lerp)
                   .Start();
            return builder;
        }
        
        public static VirtualBuilder<int> Int(int from, int to, float duration, Action<int> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<int>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => (int)Mathf.Round(Mathf.Lerp(a, b, t)))
                   .Start();
            return builder;
        }
        
        public static VirtualBuilder<Vector2> Vector2(Vector2 from, Vector2 to, float duration, Action<Vector2> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<Vector2>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => a.Lerp(b, t))
                   .Start();
            return builder;
        }
        
        public static VirtualBuilder<Vector3> Vector3(Vector3 from, Vector3 to, float duration, Action<Vector3> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<Vector3>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => a.Lerp(b, t))
                    .Start();
            return builder;
        }
        
        public static VirtualBuilder<Color> Color(Color from, Color to, float duration, Action<Color> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<Color>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => a.Lerp(b, t))
                   .Start();
            return builder;
        }
        
        public static VirtualBuilder<Quaternion> Quaternion(Quaternion from, Quaternion to, float duration, Action<Quaternion> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<Quaternion>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => a.Slerp(b, t))
                   .Start();
            return builder;
        }
        
        public static VirtualBuilder<Vector4> Vector4(Vector4 from, Vector4 to, float duration, Action<Vector4> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<Vector4>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator((a, b, t) => a.Lerp(b, t))
                   .Start();
            return builder;
        }
        
        // Generic custom interpolation for any type
        public static VirtualBuilder<T> Custom<T>(T from, T to, float duration, Func<T, T, float, T> interpolator, Action<T> onUpdate)
        {
            var builder = GetPool<VirtualBuilder<T>>(null, null);
            builder.From(from)
                   .To(to)
                   .SetDuration(duration)
                   .OnUpdate(onUpdate)
                   .SetInterpolator(interpolator)
                   .Start();
            return builder;
        }
    }
    #endregion

    #region Parse Sub-Properties
    /// <summary>
    /// Gets a property value, handling sub-properties (e.g., "position:x").
    /// Godot's Get() doesn't support sub-property syntax.
    /// </summary>
    public static Variant GetProperty(GodotObject target, string property)
    {
        if (!property.Contains(":"))
        {
            return target.Get(property);
        }
        
        var parts = property.Split(":");
        if (parts.Length != 2)
        {
            GD.PushError($"Invalid sub-property syntax: {property}");
            return default;
        }
        
        string mainProperty = parts[0];
        string subProperty = parts[1];
        
        var mainValue = target.Get(mainProperty);
        
        // Handle different types
        return mainValue.VariantType switch
        {
            Variant.Type.Vector2 => GetVector2Component((Vector2)mainValue, subProperty),
            Variant.Type.Vector3 => GetVector3Component((Vector3)mainValue, subProperty),
            Variant.Type.Vector4 => GetVector4Component((Vector4)mainValue, subProperty),
            Variant.Type.Color => GetColorComponent((Color)mainValue, subProperty),
            Variant.Type.Rect2 => GetRect2Component((Rect2)mainValue, subProperty),
            Variant.Type.Quaternion => GetQuaternionComponent((Quaternion)mainValue, subProperty),
            _ => throw new ArgumentException($"Unsupported type for sub-property: {mainValue.VariantType}")
        };
    }
    
    private static Variant GetVector2Component(Vector2 vec, string component)
    {
        return component.ToLower() switch
        {
            "x" => vec.X,
            "y" => vec.Y,
            _ => throw new ArgumentException($"Invalid Vector2 component: {component}")
        };
    }
    
    private static Variant GetVector3Component(Vector3 vec, string component)
    {
        return component.ToLower() switch
        {
            "x" => vec.X,
            "y" => vec.Y,
            "z" => vec.Z,
            _ => throw new ArgumentException($"Invalid Vector3 component: {component}")
        };
    }
    
    private static Variant GetVector4Component(Vector4 vec, string component)
    {
        return component.ToLower() switch
        {
            "x" => vec.X,
            "y" => vec.Y,
            "z" => vec.Z,
            "w" => vec.W,
            _ => throw new ArgumentException($"Invalid Vector4 component: {component}")
        };
    }
    
    private static Variant GetColorComponent(Color color, string component)
    {
        return component.ToLower() switch
        {
            "r" => color.R,
            "g" => color.G,
            "b" => color.B,
            "a" => color.A,
            "h" => color.H,
            "s" => color.S,
            "v" => color.V,
            _ => throw new ArgumentException($"Invalid Color component: {component}")
        };
    }
    
    private static Variant GetRect2Component(Rect2 rect, string component)
    {
        return component.ToLower() switch
        {
            "position" => rect.Position,
            "size" => rect.Size,
            "end" => rect.End,
            _ => throw new ArgumentException($"Invalid Rect2 component: {component}")
        };
    }
    
    private static Variant GetQuaternionComponent(Quaternion quat, string component)
    {
        return component.ToLower() switch
        {
            "x" => quat.X,
            "y" => quat.Y,
            "z" => quat.Z,
            "w" => quat.W,
            _ => throw new ArgumentException($"Invalid Quaternion component: {component}")
        };
    }
    #endregion
}