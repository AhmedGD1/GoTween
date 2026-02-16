using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoTweening;

public partial class GoTween : Node
{
    private const int MaxPoolSize = 1000;

    private static GoTween instance;

    public readonly static HashSet<IBuilder> activeBuilders = new();

    public readonly static Dictionary<string, HashSet<IBuilder>> builderGroups = new(128);
    private readonly static Dictionary<Type, Stack<object>> pool = new();

    private readonly static Stack<TweenSequence> sequencePool = new();

    private readonly static List<IBuilder> updateCache = new();

    public override void _Ready()
    {
        instance = this;
    }

    public override void _Process(double delta)
    {
        updateCache.Clear();
        updateCache.AddRange(activeBuilders);

        for (int i = 0; i < updateCache.Count; i++)
            updateCache[i].Update(delta);
    }

    public static Tween CreateNewTween()
    {
        return instance.CreateTween();
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
        Variant result = VariantMath.Lerp(a, b, sample);
        @object.Set(property, result);
    }

    #region Data Pool
    public static T GetPool<T>(GodotObject @object = null, string property = null) where T : TweenBuilder, IBuilder, new()
    {
        var type = typeof(T);
        
        if (!pool.ContainsKey(type))
            pool[type] = new();

        var typePool = pool[type];
        T builder = typePool.Count == 0 ? new T() : (T)typePool.Pop();
        
        builder.Target = @object;
        builder.Property = property;
        
        return builder;
    }

    public static void ReturnToPool(IBuilder builder)
    {
        if (builder == null)
            return;

        activeBuilders.Remove(builder);
        RemoveFromGroup(builder);
        builder.Reset();

        var type = builder.GetType();
        if (!pool.ContainsKey(type))
            pool[type] = new Stack<object>();
        
        var typePool = pool[type];
        if (typePool.Count >= MaxPoolSize)
            return;
        
        pool[type].Push(builder);
    }

    public static void AddActiveBuilder(IBuilder builder)
    {
        if (builder == null)
            return;

        activeBuilders.Add(builder);

        if (!string.IsNullOrEmpty(builder.Group))
        {
            if (!builderGroups.ContainsKey(builder.Group))
                builderGroups[builder.Group] = new HashSet<IBuilder>();
            builderGroups[builder.Group].Add(builder);
        }
    }

    public static void SequenceToPool(TweenSequence sequence)
    {
        sequence.Reset();
        sequencePool.Push(sequence);
    }

    /// <summary>
    /// Gets a tween sequence from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <param name="groupIfNew"></param>
    /// <returns></returns>
    public static TweenSequence GetSequence(string groupIfNew = null)
    {
        return sequencePool.Count != 0 ? sequencePool.Pop() : new(groupIfNew);
    }
    #endregion

    #region Groups
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
    #endregion

    #region Parse Sub-Properties
    /// <summary>
    /// Gets a property value, handling sub-properties (e.g., "position:x").
    /// Godot's Get() doesn't support sub-property syntax.
    /// </summary>
    public static Variant GetProperty(GodotObject target, string property)
    {
        if (!property.Contains(':'))
        {
            return target.Get(property);
        }
        
        var parts = property.Split(':');
        if (parts.Length != 2)
        {
            GD.PushError($"Invalid sub-property syntax: {property}");
            return default;
        }
        
        string mainProperty = parts[0];
        string subProperty = parts[1];
        
        var mainValue = target.Get(mainProperty);
        
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