using Godot;
using System;

namespace GoTweening;

public static class VariantMath
{
    public static Variant Add(Variant a, Variant b)
    {
        return a.VariantType switch
        {
            Variant.Type.Float => (float)a + (float)b,
            Variant.Type.Int => (int)a + (int)b,
            Variant.Type.Vector2 => (Vector2)a + (Vector2)b,
            Variant.Type.Vector3 => (Vector3)a + (Vector3)b,
            Variant.Type.Color => (Color)a + (Color)b,
            Variant.Type.Quaternion => (Quaternion)a * (Quaternion)b,
            _ => throw new ArgumentException($"Unsupported type for relative path tweening: {a.VariantType}")
        };
    }

    public static Variant Lerp(Variant a, Variant b, float t)
    {
        return a.VariantType switch
        {
            Variant.Type.Float => Mathf.Lerp((float)a, (float)b, t),
            Variant.Type.Int => Mathf.Lerp((int)a, (int)b, t),
            Variant.Type.Vector2 => ((Vector2)a).Lerp((Vector2)b, t),
            Variant.Type.Vector3 => ((Vector3)a).Lerp((Vector3)b, t),
            Variant.Type.Quaternion => ((Quaternion)a).Slerp((Quaternion)b, t),
            Variant.Type.Color => ((Color)a).Lerp((Color)b, t),
            _ => throw new ArgumentException($"Unsupported type: {a.VariantType}")
        };
    }
}

