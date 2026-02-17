using Godot;
using System;

namespace GoTweening;

public class GoVirtual
{
    public static VirtualBuilder<float> Float(float from, float to, float duration, Action<float> onUpdate) =>
        Create(from, to, duration, Mathf.Lerp, onUpdate);
    
    public static VirtualBuilder<int> Int(int from, int to, float duration, Action<int> onUpdate) =>
        Create(from, to, duration, (a, b, t) => (int)Mathf.Round(Mathf.Lerp(a, b, t)), onUpdate);
    
    public static VirtualBuilder<Vector2> Vector2(Vector2 from, Vector2 to, float duration, Action<Vector2> onUpdate) =>
        Create(from, to, duration, (a, b, t) => a.Lerp(b, t), onUpdate);
    
    public static VirtualBuilder<Vector3> Vector3(Vector3 from, Vector3 to, float duration, Action<Vector3> onUpdate) =>
        Create(from, to, duration, (a, b, t) => a.Lerp(b, t), onUpdate);
    
    public static VirtualBuilder<Color> Color(Color from, Color to, float duration, Action<Color> onUpdate) =>
        Create(from, to, duration, (a, b, t) => a.Lerp(b, t), onUpdate);
    
    public static VirtualBuilder<Quaternion> Quaternion(Quaternion from, Quaternion to, float duration, Action<Quaternion> onUpdate) =>
        Create(from, to, duration, (a, b, t) => a.Slerp(b, t), onUpdate);
    
    public static VirtualBuilder<Vector4> Vector4(Vector4 from, Vector4 to, float duration, Action<Vector4> onUpdate) =>
        Create(from, to, duration, (a, b, t) => a.Lerp(b, t), onUpdate);
    
    public static VirtualBuilder<T> Create<T>(T from, T to, float duration, 
        Func<T, T, float, T> interpolator, Action<T> onUpdate)
    {
        var builder = GoTween.GetPool<VirtualBuilder<T>>(null, null);
        builder.From(from)
            .To(to)
            .SetDuration(duration)
            .OnUpdate(onUpdate)
            .SetInterpolator(interpolator)
            .Start();
        return builder;
    }
}
