# GoTween

A powerful, fluent, and performant tweening library for Godot 4 (C#). GoTween provides an intuitive API for creating smooth animations with advanced features like object pooling, sequences, groups, and virtual tweens.

## Features

-  **Fluent API** - Chainable methods for clean, readable code
-  **Object Pooling** - Automatic pooling for zero-garbage animations
-  **Sequences** - Chain multiple tweens with precise timing control
-  **Groups** - Organize and control tweens by category
-  **Virtual Tweens** - Animate values without target objects
-  **Path Tweening** - Animate along custom curves
-  **Sub-property Support** - Tween individual vector components (e.g., `position:x`)
-  **Rich Helper Methods** - Pre-built methods for common animations

## Installation

1. Copy all `.cs` files into your Godot project under a `GoTweening` folder
2. Add the `GoTween` node to your scene tree (typically in an autoload)
3. Start tweening!

## Quick Start

```csharp
using GoTweening;

// Simple fade out
sprite.GoProperty("modulate:a")
    .To(0f)
    .SetDuration(1f)
    .Elastic()
    .EaseOut()
    .Start();

// Or use a helper method
GoTween.GoFadeOut(sprite, 1f);

// Move with callbacks
node.GoProperty("position")
    .To(new Vector2(100, 200))
    .SetDuration(2f)
    .Bounce()
    .OnComplete(() => GD.Print("Done!"))
    .Start();
```

## Core Concepts

### Basic Tweening

```csharp
// Tween any property
myNode.GoProperty("property_name")
    .To(endValue)
    .SetDuration(1.5f)
    .Start();

// Multiple values (create keyframes)
sprite.GoProperty("position")
    .To(new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100))
    .SetDuration(1f, 0.5f, 1f)  // Duration for each keyframe
    .Start();

// Set starting value explicitly
node.GoProperty("scale")
    .From(Vector2.Zero)
    .To(Vector2.One)
    .SetDuration(1f)
    .Start();
```

### Sub-property Syntax

Tween individual components of vectors, colors, and other composite types:

```csharp
// Tween just the X position
node.GoProperty("position:x").To(100f).SetDuration(1f).Start();

// Supported types:
// - Vector2/3/4: :x, :y, :z, :w
// - Color: :r, :g, :b, :a, :h, :s, :v
// - Rect2: :position, :size, :end
// - Quaternion: :x, :y, :z, :w
```

### Easing and Transitions

```csharp
sprite.GoProperty("position")
    .To(targetPos)
    .SetDuration(1f)
    .Elastic()      // Transition type
    .EaseOut()      // Ease type
    .Start();
```

**Transition Types:**
- `Linear()`, `Sine()`, `Quad()`, `Cubic()`, `Quart()`, `Quint()`
- `Expo()`, `Circ()`, `Elastic()`, `Back()`, `Bounce()`, `Spring()`

**Ease Types:**
- `EaseIn()`, `EaseOut()`, `EaseInOut()`, `EaseOutIn()`

### Callbacks and Events

```csharp
builder.OnComplete(() => GD.Print("Finished!"))
       .OnLoop(loopIndex => GD.Print($"Loop {loopIndex}"))
       .OnUpdate(delta => GD.Print($"Delta: {delta}"))
       .OnStep(stepIndex => GD.Print($"Keyframe {stepIndex}"))
       .Start();
```

### Looping and Delays

```csharp
sprite.GoProperty("rotation")
    .To(Mathf.Tau)
    .SetDuration(2f)
    .SetLoops(5)        // Loop 5 times (0 = infinite)
    .Wait(1f)           // Delay before starting
    .Start();
```

## Helper Methods

### Fade

```csharp
GoTween.GoFadeOut(sprite, 1f);
GoTween.GoFadeIn(sprite, 1f);
GoTween.GoFade(sprite, 0.5f, 1f);  // Fade to specific alpha
```

### Movement

```csharp
// 2D Movement
GoTween.GoMove(node2D, new Vector2(100, 100), 1f);
GoTween.GoMoveLocal(node2D, new Vector2(50, 50), 1f);
GoTween.GoMoveX(node2D, 100f, 1f);
GoTween.GoMoveY(node2D, 100f, 1f);

// 3D Movement
GoTween.GoMove(node3D, new Vector3(10, 5, 10), 1f);
GoTween.GoMoveZ(node3D, 20f, 1f);
```

### Scale

```csharp
GoTween.GoScale(node, new Vector2(2, 2), 1f);
GoTween.GoScaleX(node, 1.5f, 1f);
GoTween.GoScaleY(node, 0.5f, 1f);
```

### Rotation

```csharp
GoTween.Rotate2D(node2D, 360f, 2f);  // Degrees
GoTween.Rotate3D(node3D, new Vector3(0, 180, 0), 2f);
GoTween.RotateX(node3D, 90f, 1f);
```

### Color

```csharp
GoTween.GoModulate(sprite, Colors.Red, 1f);
GoTween.GoSelfModulate(sprite, Colors.Blue, 1f);
GoTween.GoColor(colorRect, Colors.Green, 1f);
```

### Effects

```csharp
// Shake effects
GoTween.GoShake(node2D, intensity: 10f, duration: 0.5f);
GoTween.GoShake(node3D, intensity: 5f, duration: 0.3f);
GoTween.GoShakeRotation(node2D, intensityDegrees: 15f, duration: 0.4f);

// Blink effect
GoTween.GoBlink(sprite, times: 5, duration: 2f);

```

### UI

```csharp
GoTween.GoSize(control, new Vector2(200, 100), 1f);
GoTween.GoAnchorMove(control, new Vector2(50, 50), 1f);
```

## Advanced Features

### Sequences

Create complex animation timelines:

```csharp
GoTween.Sequence()
    .Append(sprite.GoProperty("position").To(new Vector2(100, 0)).SetDuration(1f))
    .Append(sprite.GoProperty("rotation").To(Mathf.Pi).SetDuration(0.5f))
    .Join(sprite.GoProperty("scale").To(Vector2.One * 2).SetDuration(0.5f))  // Runs parallel
    .AppendInterval(0.5f)  // Wait
    .AppendCallback(() => GD.Print("Halfway!"))
    .Append(sprite.GoProperty("modulate:a").To(0f).SetDuration(1f))
    .OnComplete(() => GD.Print("Sequence complete!"))
    .Start();
```

**Sequence Methods:**
- `Append(builder)` - Add to end
- `Prepend(builder)` - Add to start
- `Join(builder)` - Run parallel with previous
- `Insert(time, builder)` - Insert at specific time
- `AppendInterval(duration)` - Add delay
- `AppendCallback(action)` - Add callback after last tween
- `SetLoops(count)` - Loop the last added tween

### Groups

Organize and control multiple tweens:

```csharp
// Add tweens to groups
sprite.GoProperty("position")
    .To(target)
    .SetDuration(1f)
    .AddToGroup("UI")
    .Start();

// Control entire groups
GoTween.GoPauseGroup("UI");
GoTween.GoResumeGroup("UI");
GoTween.GoKillGroup("UI");
GoTween.GoCompleteGroup("UI");  // Jump to end

// Query groups
bool isActive = GoTween.IsGroupActive("UI");
int count = GoTween.GetGroupTweenCount("UI");
var builders = GoTween.GetGroupBuilders("UI");

// Enum-based groups (type-safe)
public enum TweenGroup { UI, Gameplay, Effects }

builder.AddToGroup(TweenGroup.UI).Start();
GoTween.GoKillGroup(TweenGroup.UI);
```

### Virtual Tweens

Animate custom values without target objects:

```csharp
// Built-in types
GoTween.Virtual.Float(0f, 100f, 2f, value => 
{
    GD.Print($"Value: {value}");
});

GoTween.Virtual.Vector2(Vector2.Zero, Vector2.One * 100, 1f, value =>
{
    customObject.Position = value;
});

GoTween.Virtual.Color(Colors.Red, Colors.Blue, 1f, value =>
{
    material.SetShaderParameter("color", value);
});

// Custom interpolation
GoTween.Virtual.Custom(
    from: 0f,
    to: 1f,
    duration: 2f,
    interpolator: (a, b, t) => Mathf.Lerp(a, b, t),
    onUpdate: value => GD.Print(value)
)
.Bounce()
.EaseOut()
.Start();
```

**Virtual Types Available:**
- `Float`, `Int`, `Vector2`, `Vector3`, `Vector4`, `Color`, `Quaternion`, `Custom<T>`

### Path Tweening

Animate along custom curves:

```csharp
var curve = new Curve();
curve.AddPoint(new Vector2(0, 0));
curve.AddPoint(new Vector2(0.5f, 1));
curve.AddPoint(new Vector2(1, 0));

sprite.GoPath("position", curve)
    .From(new Vector2(0, 0))
    .To(new Vector2(100, 100))
    .SetDuration(2f)
    .Start();
```

## Tween Control

### Individual Control

```csharp
var tween = sprite.GoProperty("position").To(target).SetDuration(1f).Start();

// Store and control later
var builder = sprite.GoProperty("scale");
builder.To(Vector2.One * 2).SetDuration(1f);
var tween = builder.Start();

builder.Pause();
builder.Resume();
builder.Cancel();  // Kill and return to pool
builder.Replay();  // Restart with current settings
```

### Global Control

```csharp
// Kill all tweens
GoTween.GoKillAll();

// Pause/Resume all
GoTween.GoPauseAll();
GoTween.GoResumeAll();

// Pause all except specific groups
GoTween.GoPauseAll("UI", "HUD");

// Kill tweens on specific object
myNode.GoKillTweens();
GoTween.GoKillTarget(myNode);

// Query active tweens
int activeCount = GoTween.GetActiveTweenCount();
int targetCount = GoTween.GetTweenCountForTarget(myNode);
bool isTweening = GoTween.IsPropertyTweening(myNode, "position");
```

## Configuration with Lambdas

Many helper methods accept configuration lambdas:

```csharp
GoTween.GoFadeOut(sprite, 1f, builder =>
{
    builder.Elastic()
           .EaseOut()
           .AddToGroup("UI")
           .OnComplete(() => sprite.QueueFree());
});

GoTween.GoMove(node, target, 2f, builder =>
{
    builder.Bounce()
           .SetLoops(3)
           .Wait(0.5f);
});
```

## Complete Example

```csharp
using Godot;
using GoTweening;

public partial class Player : CharacterBody2D
{
    private Sprite2D sprite;
    
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite");
        
        // Entrance animation sequence
        GoTween.Sequence("PlayerIntro")
            .Append(sprite.GoProperty("scale")
                .From(Vector2.Zero)
                .To(Vector2.One)
                .SetDuration(0.5f))
            .Join(sprite.GoProperty("modulate:a")
                .From(0f)
                .To(1f)
                .SetDuration(0.5f))
            .AppendInterval(0.2f)
            .Append(sprite.GoProperty("rotation")
                .To(Mathf.Tau)
                .SetDuration(1f))
            .OnComplete(() => GD.Print("Ready!"))
            .Start();
    }
    
    public void TakeDamage()
    {
        // Flash red and shake
        GoTween.Sequence("DamageEffect")
            .Join(GoTween.GoModulate(sprite, Colors.Red, 0.1f))
            .Join(GoTween.GoShake(this, 5f, 0.3f))
            .Append(GoTween.GoModulate(sprite, Colors.White, 0.1f))
            .Start();
    }
    
    public override void _ExitTree()
    {
        // Clean up all tweens on this object
        this.GoKillTweens();
    }
}
```

## API Reference

### TweenBuilder

| Method | Description |
|--------|-------------|
| `To(params Variant[])` | Set target values (keyframes) |
| `From(Variant)` | Set starting value |
| `SetDuration(params float[])` | Set duration for each keyframe |
| `Wait(float)` | Delay before starting |
| `SetLoops(int)` | Set loop count (0 = infinite) |
| `SetParallel(bool)` | Enable parallel mode for loops |
| `AddToGroup(string)` | Add to named group |
| `OnComplete(params Action[])` | Completion callbacks |
| `OnLoop(Action<int>)` | Loop callback (receives loop index) |
| `OnUpdate(Action<double>)` | Update callback (receives delta) |
| `OnStep(Action<int>)` | Keyframe completion callback |
| `Start()` | Start the tween |
| `Replay()` | Restart with current settings |
| `Pause()` | Pause animation |
| `Resume()` | Resume animation |
| `Cancel()` | Kill and return to pool |

### Transition & Easing

**Transitions:** `Linear()`, `Sine()`, `Quad()`, `Cubic()`, `Quart()`, `Quint()`, `Expo()`, `Circ()`, `Elastic()`, `Back()`, `Bounce()`, `Spring()`

**Easing:** `EaseIn()`, `EaseOut()`, `EaseInOut()`, `EaseOutIn()`

### PathBuilder

| Method | Description |
|--------|-------------|
| `From(Variant)` | Set starting value |
| `To(Variant)` | Set target value |
| `SetDuration(float)` | Set animation duration |
| All TweenBuilder methods | Inherited methods available |

### VirtualBuilder

| Method | Description |
|--------|-------------|
| `From(T)` | Set starting value |
| `To(T)` | Set target value |
| `SetDuration(float)` | Set duration |
| `OnUpdate(Action<T>)` | Value update callback |
| `SetInterpolator(Func<T,T,float,T>)` | Custom interpolation |
| All transitions & easing | Same as TweenBuilder |

### TweenSequence

| Method | Description |
|--------|-------------|
| `Append(IBuilder)` | Add tween to end |
| `Prepend(IBuilder)` | Add tween to start |
| `Join(IBuilder)` | Run parallel with previous |
| `Insert(float, IBuilder)` | Insert at specific time |
| `AppendInterval(float)` | Add delay |
| `AppendCallback(Action)` | Add callback |
| `SetLoops(int)` | Loop last tween |
| `OnComplete(Action)` | Sequence completion callback |
| `Start()` | Execute sequence |

## Troubleshooting

**Tween doesn't start:**
- Ensure you call `.Start()` at the end
- Check that the target object is valid
- Verify duration is > 0

**Tween stops early:**
- Don't free the target object during animation
- Use `.GoKillTweens()` before freeing nodes

**Memory leaks:**
- Always kill tweens on objects that are being freed
- GoTween automatically pools completed tweens

**Groups not working:**
- Ensure group name is set before calling `.Start()`
- Use `GoTween.GetActiveGroups()` to debug

## Contributing

Contributions welcome!

---

**Made for the Godot community**
