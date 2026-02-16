<img width="1536" height="1024" alt="GoTweenLogo" src="https://github.com/user-attachments/assets/f8f844e2-6dee-48c6-8c69-7ff2b621b57b" />

# GoTween

A powerful and easy-to-use tweening library for Godot 4 with C#. GoTween makes it simple to add smooth animations to your game with minimal code.

## Features

- **Fluent API** - Chain methods together for readable, intuitive code
- **Property Tweening** - Animate any property with full easing and transition support
- **Path Tweening** - Create custom animation curves for unique motion paths
- **Virtual Tweening** - Animate values that aren't tied to game objects
- **Sequence System** - Build complex animation timelines with ease
- **Helper Methods** - Quick one-line animations for common tasks
- **Group Management** - Control multiple tweens at once with groups
- **Object Pooling** - Optimized performance with automatic memory management
- **Sub-property Support** - Animate individual components like `position:x` or `color:r`

---

## Quick Start

### Basic Property Tween

```csharp
// Fade out a sprite over 1 second
sprite.GoProperty("modulate:a")
    .To(0f)
    .SetDuration(1f)
    .Start();

// Move a node with an elastic bounce
node.GoProperty("position")
    .To(new Vector2(500, 300))
    .SetDuration(2f)
    .Elastic()
    .EaseOut()
    .Start();
```

### Using Helper Methods

Helper methods are the quickest way to animate common properties. They don't require calling `.Start()` - they start immediately:

```csharp
// Simple fade in - no .Start() needed
GoTween.GoFadeIn(canvasItem, 0.5f);

// Move to a position
GoTween.GoMove(node, new Vector2(100, 100), 1f);

// Scale up
GoTween.GoScale(node, Vector2.One * 2f, 1f);

// Camera shake
GoTween.GoShake(camera, strength: 5f, duration: 0.5f);
```

If you need to configure the tween further, use the config parameter and chain additional methods:

```csharp
// Using config to add callbacks and modify the tween
GoTween.GoFadeIn(canvasItem, 0.5f, tween => 
{
    tween.OnComplete(() => GD.Print("Fade complete!"));
    tween.Elastic().EaseOut();
});
```

---

## Property Tweening

Property tweening is the most common way to animate things in GoTween. You can animate any property on any Godot object.

### Basic Example

```csharp
// Simple position tween
node.GoProperty("position")
    .To(new Vector2(100, 200))
    .SetDuration(1.5f)
    .Start();
```

### Chaining Multiple Values

```csharp
// Move through multiple points
node.GoProperty("position")
    .To(new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100))
    .SetDuration(0.5f, 1f, 0.5f)
    .Start();
```

### Easing and Transitions

```csharp
// Smooth elastic bounce
sprite.GoProperty("scale")
    .To(Vector2.One * 1.5f)
    .SetDuration(0.8f)
    .Elastic()
    .EaseOut()
    .Start();

// Available transitions:
// Linear, Sine, Quad, Cubic, Quart, Quint, 
// Expo, Circ, Elastic, Back, Bounce, Spring

// Available easing:
// EaseIn, EaseOut, EaseInOut, EaseOutIn
```

### Looping

```csharp
// Ping-pong between two values
sprite.GoProperty("rotation_degrees")
    .To(45f)
    .SetDuration(1f)
    .SetLoops(5, TweenBuilder.LoopType.PingPong)
    .Start();

// Loop forever
sprite.GoProperty("position:y")
    .To(100f)
    .SetDuration(2f)
    .LoopInfinitely()
    .AsRelative()  // Move 100 pixels from current position
    .Start();
```

### Callbacks and Events

```csharp
sprite.GoProperty("modulate:a")
    .To(0f)
    .SetDuration(1f)
    .OnUpdate(delta => GD.Print($"Fading... {delta}"))
    .OnComplete(() => GD.Print("Fade complete!"))
    .OnLoop(loopIndex => GD.Print($"Loop {loopIndex}"))
    .Start();
```

### Sub-Properties

GoTween supports animating individual components of vectors and colors:

```csharp
// Just move on the X axis
node.GoProperty("position:x").To(500f).SetDuration(1f).Start();

// Animate only the red channel
sprite.GoProperty("modulate:r").To(0f).SetDuration(0.5f).Start();

// Supported sub-properties:
// Vector2/3/4: x, y, z, w
// Color: r, g, b, a, h, s, v
// Rect2: position, size, end
// Quaternion: x, y, z, w
```

---

## Path Tweening

<img width="299" height="304" alt="curve" src="https://github.com/user-attachments/assets/b5c4497d-f9a1-4f0b-81c7-c6c0b631c226" />

Path tweening lets you create custom motion using Godot's `Curve` resource. This is perfect for creating unique, non-linear animations that standard easing functions can't achieve.

### What is Path Tweening?

Instead of interpolating directly between two values, path tweening samples a curve to control how the value changes over time. Think of it like this: regular tweening moves from A to B in a predictable way, but path tweening lets you define exactly how that journey happens, even allowing overshoots, dips, or any custom motion pattern you can draw in a curve.

### Basic Path Tween

```csharp
// Create a curve in the editor or in code
var curve = new Curve();
curve.AddPoint(new Vector2(0, 0));
curve.AddPoint(new Vector2(0.5f, 1.2f));  // Overshoot in the middle
curve.AddPoint(new Vector2(1, 1));        // End at target

// Animate using the curve
node.GoPath("position", curve)
    .From(Vector2.Zero)
    .To(new Vector2(500, 300))
    .SetDuration(2f)
    .Start();
```

### Why Use Path Tweening?

Path tweening is great when you need:
- **Custom easing curves** that standard easing functions can't provide
- **Artist-controlled motion** by designing curves in the Godot editor
- **Overshoot effects** without using elastic easing
- **Complex timing** for game feel polish

### Example: Custom Jump Arc

```csharp
// Create a jump curve that goes up quickly, hangs, then falls
var jumpCurve = new Curve();
jumpCurve.AddPoint(new Vector2(0, 0), 0, 2);      // Start
jumpCurve.AddPoint(new Vector2(0.3f, 1f), 0, 0);  // Peak
jumpCurve.AddPoint(new Vector2(1f, 0f), -2, 0);   // Land

player.GoPath("position:y", jumpCurve)
    .From(player.Position.Y)
    .To(player.Position.Y - 200)  // Jump up 200 pixels
    .SetDuration(1f)
    .Start();
```

### Using Editor Curves

```csharp
// Export a curve in your script
[Export] public Curve MovementCurve;

// Use it in your tween
public void AnimateWithCurve()
{
    node.GoPath("position", MovementCurve)
        .To(targetPosition)
        .SetDuration(2f)
        .Start();
}
```

---

## Virtual Tweening

Virtual tweening lets you animate values that aren't attached to game objects. This is incredibly useful for custom logic, counters, camera effects, and more.

### What is Virtual Tweening?

Virtual tweens don't target object properties. Instead, they give you raw interpolated values in a callback, and you decide what to do with them. Think of it as a timer with easing built in - you get smooth values from A to B over time, and you control what happens with those values.

This is different from property tweening because:
- You're not limited to animating object properties
- You can use the values for custom calculations
- You can control multiple things from a single tween
- It's perfect for non-visual animations like audio volume, custom shaders, or game logic

### Basic Virtual Tween

```csharp
// Animate a score counter
Virtual.Int(0, 1000, 2f, value => 
{
    scoreLabel.Text = value.ToString();
});

// Animate a float value
Virtual.Float(0f, 100f, 1.5f, value =>
{
    customShader.SetShaderParameter("intensity", value);
});
```

Note that virtual tweens start automatically - you don't need to call `.Start()`. However, they return a builder if you want to configure them further:

```csharp
// Store the builder if you need more control
var builder = Virtual.Float(0f, 1f, 2f, value => 
{
    // Your update logic
});

// Now you can configure it
builder.Elastic()
    .EaseOut()
    .OnComplete(() => GD.Print("Done!"));
```

### Supported Types

Virtual tweening supports all common types:

```csharp
// Float
Virtual.Float(0f, 1f, 1f, val => { /* your code */ });

// Int
Virtual.Int(0, 100, 2f, val => { /* your code */ });

// Vector2
Virtual.Vector2(Vector2.Zero, new Vector2(10, 10), 1f, val => { /* your code */ });

// Vector3
Virtual.Vector3(Vector3.Zero, new Vector3(5, 5, 5), 1f, val => { /* your code */ });

// Vector4
Virtual.Vector4(Vector4.Zero, Vector4.One, 1f, val => { /* your code */ });

// Color
Virtual.Color(Colors.Red, Colors.Blue, 1f, val => { /* your code */ });

// Quaternion
Virtual.Quaternion(Quaternion.Identity, rotation, 1f, val => { /* your code */ });
```

### Real-World Examples

#### Counting Animation

```csharp
// Smoothly count up a score
Virtual.Int(currentScore, newScore, 1.5f, value => 
{
    scoreLabel.Text = $"${value:N0}";
})
.Cubic()
.EaseOut()
.OnComplete(() => ShowScoreEffects());
```

#### Camera Shake with GoShake

GoTween provides a built-in helper method for camera shake:

```csharp
// Simple camera shake
GoTween.GoShake(camera, strength: 10f, duration: 0.5f);

// Camera shake with custom falloff curve
var falloffCurve = new Curve();
falloffCurve.AddPoint(new Vector2(0, 1));
falloffCurve.AddPoint(new Vector2(1, 0));

GoTween.GoShake(camera, strength: 15f, duration: 1f, falloffCurve);
```

If you need custom shake behavior, you can build it with virtual tweening:

```csharp
// Custom shake with specific control
Virtual.Float(shakeIntensity, 0f, duration, intensity =>
{
    var offset = new Vector2(
        GD.Randf() * intensity - intensity / 2f,
        GD.Randf() * intensity - intensity / 2f
    );
    camera.Offset = offset;
})
.Quad()
.EaseOut()
.OnComplete(() => camera.Offset = Vector2.Zero);
```

#### Health Bar Smoothing

```csharp
// Smooth health bar animation
Virtual.Float(healthBar.Value, newHealthValue, 0.3f, value =>
{
    healthBar.Value = value;
    
    // Flash red when taking damage
    if (value < currentHealth)
        healthBar.Modulate = Colors.Red.Lerp(Colors.White, value / maxHealth);
})
.Quad()
.EaseOut();
```

#### Procedural Animation

```csharp
// Create a breathing effect
Virtual.Float(0f, Mathf.Pi * 2f, 3f, angle =>
{
    var breatheScale = 1f + Mathf.Sin(angle) * 0.05f;
    character.Scale = Vector2.One * breatheScale;
})
.Linear()
.SetLoops(0); // 0 = infinite loops
```

### Why Use Virtual Tweening?

- **Custom logic** - Animate anything you want, not just object properties
- **UI counters** - Smoothly count up scores, health, currencies
- **Shader parameters** - Animate visual effects frame-by-frame
- **Camera effects** - Screen shake, zoom, or custom camera motion
- **Procedural animation** - Create animation patterns programmatically
- **Non-visual values** - Audio volume ramps, AI behavior transitions, etc.

### Virtual Tween Options

Virtual tweens support all the same configuration options as property tweens:

```csharp
Virtual.Float(0f, 100f, 2f, value => { /* your code */ })
    .Elastic()              // Transition type
    .EaseOut()              // Easing type
    .Wait(0.5f)             // Delay before starting
    .SetLoops(3)            // Repeat count (0 = infinite)
    .AddToGroup("effects")  // Group management
    .OnComplete(() => { })  // Completion callback
    .OnLoop(i => { })       // Loop callback
    .OnUpdate(dt => { });   // Per-frame callback (receives delta time)
```

---

## Sequences

Sequences let you chain multiple tweens together, creating complex animation timelines. They're perfect for cutscenes, UI transitions, or any multi-step animation.

### Creating a Sequence

```csharp
// Create a sequence
var seq = GoTween.Sequence();

// Add tweens one after another
seq.Append(sprite.GoProperty("position")
       .To(new Vector2(100, 0))
       .SetDuration(0.5f))
   .Append(sprite.GoProperty("position")
       .To(new Vector2(100, 100))
       .SetDuration(0.5f))
   .Append(sprite.GoProperty("position")
       .To(new Vector2(0, 100))
       .SetDuration(0.5f))
   .OnComplete(() => GD.Print("Square complete!"))
   .Start();
```

### Parallel Tweens with Join

```csharp
// Run tweens at the same time
var seq = GoTween.Sequence();

seq.Append(sprite.GoProperty("position")
       .To(new Vector2(500, 300))
       .SetDuration(1f))
   .Join(sprite.GoProperty("rotation_degrees")  // Runs at same time as position
       .To(360f)
       .SetDuration(1f))
   .Join(sprite.GoProperty("scale")             // Also runs at same time
       .To(Vector2.One * 2f)
       .SetDuration(1f))
   .Start();
```

### Advanced Sequence Control

```csharp
var seq = GoTween.Sequence("ui_animations");

// Prepend - add to the beginning
seq.Prepend(sprite.GoProperty("modulate:a")
       .To(0f)
       .SetDuration(0.2f));

// Insert - add at specific time
seq.Insert(0.5f, sprite.GoProperty("scale")
       .To(Vector2.One * 1.5f)
       .SetDuration(0.3f));

// Add intervals (pauses)
seq.AppendInterval(0.5f);

// Add callbacks between tweens
seq.AppendCallback(() => PlaySound());

// Control loops on individual steps
seq.Append(sprite.GoProperty("rotation")
       .To(360f)
       .SetDuration(1f))
   .SetLoops(3);

seq.Start();
```

### Example: UI Panel Slide In

```csharp
public void ShowPanel(Control panel)
{
    panel.Visible = true;
    panel.Modulate = new Color(1, 1, 1, 0);
    panel.Position = new Vector2(-300, panel.Position.Y);
    
    var seq = GoTween.Sequence();
    
    seq.Append(panel.GoProperty("position:x")
           .To(0f)
           .SetDuration(0.3f)
           .Back()
           .EaseOut())
       .Join(panel.GoProperty("modulate:a")
           .To(1f)
           .SetDuration(0.3f))
       .AppendInterval(0.1f)
       .AppendCallback(() => PlayPopSound())
       .Start();
}
```

---

## Group Management

Groups let you control multiple tweens together. This is really useful for managing UI transitions, pausing game animations, or organizing your tweens by purpose.

### Creating Groups

```csharp
// Add tweens to a group
sprite.GoProperty("position")
    .To(targetPos)
    .SetDuration(1f)
    .AddToGroup("ui_animations")
    .Start();

label.GoProperty("modulate:a")
    .To(1f)
    .SetDuration(0.5f)
    .AddToGroup("ui_animations")
    .Start();

// Or use sequences with groups
var seq = GoTween.Sequence("gameplay");
```

### Controlling Groups

```csharp
// Pause all tweens in a group
GoTween.GoPauseGroup("ui_animations");

// Resume
GoTween.GoResumeGroup("ui_animations");

// Kill (stop and clean up)
GoTween.GoKillGroup("ui_animations");

// Complete instantly
GoTween.GoCompleteGroup("ui_animations");

// Check if group is active
if (GoTween.IsGroupActive("gameplay"))
{
    // Group has active tweens
}

// Get count
int count = GoTween.GetGroupTweenCount("ui_animations");
```

### Enum-Based Groups

```csharp
// Define your groups as an enum for type safety
public enum TweenGroup
{
    UI,
    Gameplay,
    VFX,
    Audio
}

// Use enum groups
sprite.GoProperty("scale")
    .To(Vector2.One * 2f)
    .SetDuration(1f)
    .AddToGroup(TweenGroup.VFX)
    .Start();

// Control with enum
GoTween.GoPauseGroup(TweenGroup.Gameplay);
GoTween.GoKillGroup(TweenGroup.VFX);
```

### Excluding from Pause

```csharp
// Pause everything except UI
GoTween.GoPauseAll("ui_animations");

// Resume everything except excluded groups
GoTween.GoResumeAll("ui_animations", "menu");
```

---

## Helper Methods

GoTween includes a comprehensive set of helper methods for common animations. These methods are designed to be quick and simple - they start immediately without needing `.Start()`.

### Movement Helpers

```csharp
// Move to a position (2D)
GoTween.GoMove(node2D, new Vector2(100, 100), 1f);
GoTween.GoMoveLocal(node2D, new Vector2(50, 50), 1f);

// Move individual axes
GoTween.GoMoveX(node2D, 200f, 1f);
GoTween.GoMoveY(node2D, 150f, 1f);
GoTween.GoMoveLocalX(node2D, 100f, 1f);
GoTween.GoMoveLocalY(node2D, 75f, 1f);

// Move to a position (3D)
GoTween.GoMove(node3D, new Vector3(10, 5, 10), 1f);
GoTween.GoMoveX(node3D, 15f, 1f);
GoTween.GoMoveY(node3D, 10f, 1f);
GoTween.GoMoveZ(node3D, 20f, 1f);
```

### Fading Helpers

```csharp
// Fade to a specific alpha
GoTween.GoFade(canvasItem, 0.5f, 1f);

// Fade completely out
GoTween.GoFadeOut(canvasItem, 1f);

// Fade completely in
GoTween.GoFadeIn(canvasItem, 1f);
```

### Scale Helpers

```csharp
// Scale uniformly
GoTween.GoScale(node2D, Vector2.One * 2f, 1f);
GoTween.GoScale(node3D, Vector3.One * 1.5f, 1f);

// Scale individual axes
GoTween.GoScaleX(node2D, 2f, 1f);
GoTween.GoScaleY(node2D, 0.5f, 1f);
GoTween.GoScaleZ(node3D, 3f, 1f);
```

### Rotation Helpers

```csharp
// Rotate 2D
GoTween.GoRotate(node2D, 180f, 1f);

// Rotate 3D (euler angles)
GoTween.GoRotate(node3D, new Vector3(45, 90, 0), 1f);
GoTween.GoRotateX(node3D, 90f, 1f);
GoTween.GoRotateY(node3D, 180f, 1f);
GoTween.GoRotateZ(node3D, 45f, 1f);
```

### Color Helpers

```csharp
GoTween.GoColor(colorRect, Colors.Red, 0.5f);

GoTween.GoModulate(canvasItem, Colors.Red, 1f, t => t.OnComplete(GD.Print("RED"));
GoTween.GoSelfModulate(canvasItem, Colors.Red, 1f, t => t.OnComplete(GD.Print("RED"));
```

### Configuring Helper Methods

If you need more control, use the optional config parameter:

```csharp
// Add callbacks and modify the tween
GoTween.GoMove(node, targetPos, 1f, tween => 
{
    tween.Elastic().EaseOut();
    tween.OnComplete(() => GD.Print("Arrived!"));
    tween.AddToGroup("movement");
});

// Chain multiple configurations
GoTween.GoFadeIn(sprite, 0.5f, tween => 
{
    tween.Wait(0.2f);                    // Delay
    tween.Bounce().EaseOut();            // Easing
    tween.OnComplete(() => sprite.QueueFree());
});
```

### Special Effect Helpers

#### Shake Effects

```csharp
// Shake camera or node
GoTween.GoShake(camera, strength: 10f, duration: 0.5f);

// Shake UI elements
GoTween.GoShakeUI(uiPanel, strength: 5f, duration: 0.3f);
```

#### Punch Effects

Punch effects give you that juicy impact feel - a quick jolt that springs back with elastic bounce.

```csharp
// Punch scale - makes things pop
GoTween.GoPunchScale(node2D, new Vector2(0.5f, 0.5f), 0.5f);
GoTween.GoPunchScale(control, new Vector2(0.3f, 0.3f), 0.4f);
GoTween.GoPunchScale(node3D, new Vector3(0.2f, 0.2f, 0.2f), 0.3f);

// Punch position - knock back effect
GoTween.GoPunchPosition(node2D, new Vector2(20, 0), 0.3f);
GoTween.GoPunchPosition(node3D, new Vector3(0, 10, 0), 0.4f);

// Punch rotation - shake and settle
GoTween.GoPunchRotation(node2D, 45f, 0.3f);
GoTween.GoPunchRotation(node3D, new Vector3(0, 0, 30), 0.4f);
```

Punch effects can also be configured:

```csharp
GoTween.GoPunchScale(button, new Vector2(0.2f, 0.2f), 0.3f, seq =>
{
    seq.OnComplete(() => PlayClickSound());
});
```

#### Blink Effect

```csharp
// Blink 5 times over 1 second
GoTween.GoBlink(sprite, times: 5, duration: 1f);
```

### Why Use Helper Methods?

Helper methods are designed for speed and convenience:

1. **No .Start() needed** - They begin immediately
2. **Sensible defaults** - Common use cases work out of the box
3. **Less typing** - `GoMove()` vs setting up a full property tween
4. **Still configurable** - Use the config parameter when you need more control

Compare the approaches:

```csharp
// Using GoProperty (more verbose)
node.GoProperty("position")
    .To(new Vector2(100, 100))
    .SetDuration(1f)
    .Elastic()
    .EaseOut()
    .Start();

// Using helper (cleaner)
GoTween.GoMove(node, new Vector2(100, 100), 1f, t => t.Elastic().EaseOut());
```

Both work perfectly - use helpers for common cases, use GoProperty when you need maximum control.

---

## Advanced Features

### Relative Tweening

```csharp
// Move 100 pixels from current position
node.GoProperty("position")
    .To(new Vector2(100, 0))
    .SetDuration(1f)
    .AsRelative()  // Adds to current value instead of replacing
    .Start();
```

### Speed Control

```csharp
// Make tween play at 2x speed
tween.SetSpeedScale(2f);

// Slow motion at 0.5x speed
tween.SetSpeedScale(0.5f);
```

### Process Modes

```csharp
// Use physics process
node.GoProperty("position")
    .To(target)
    .SetDuration(1f)
    .SetProcessMode(Tween.TweenProcessMode.Physics)
    .Start();
```

### Pause and Resume

```csharp
var builder = node.GoProperty("rotation")
    .To(360f)
    .SetDuration(2f);

var tween = builder.Start();

// Later...
builder.Pause();
builder.Resume();
```

### Query Active Tweens

```csharp
// Check if a property is being tweened
if (GoTween.IsPropertyTweening(node, "position"))
{
    GD.Print("Position is animating!");
}

// Get active tween count
int count = GoTween.GetActiveTweenCount();

// Get tweens for a specific target
int nodeCount = GoTween.GetTweenCountForTarget(node);

// Get all builders in a group
foreach (var builder in GoTween.GetGroupBuilders("ui"))
{
    // Do something with each builder
}
```

### Killing Tweens

```csharp
// Kill all tweens on an object
node.GoKillAll();

// Kill specific target's tweens
GoTween.GoKillTarget(sprite);

// Kill all active tweens
GoTween.GoKillAll();

// Kill specific group
GoTween.GoKillGroup("menu");

// Kill all except specific builders
GoTween.GoKillGroup("effects", exceptBuilder1, exceptBuilder2);
```

### Replaying Tweens

```csharp
var builder = node.GoProperty("scale")
    .To(Vector2.One * 2f)
    .SetDuration(1f)
    .OnComplete(() => GD.Print("Done!"));

builder.Start();

// Replay later (keeps callbacks)
builder.Replay();

// Replay without callbacks
builder.Replay(cancelCompletedSubs: true);
```

---

## API Reference

### PropertyBuilder Methods

```csharp
.To(params Variant[] values)              // Target value(s)
.From(Variant value)                      // Starting value
.SetDuration(params float[] durations)    // Duration(s) in seconds
.AsRelative()                             // Relative to current value
.Wait(float delay)                        // Delay before starting

// Transitions
.Linear() .Sine() .Quad() .Cubic() .Quart() .Quint()
.Expo() .Circ() .Elastic() .Back() .Bounce() .Spring()

// Easing
.EaseIn() .EaseOut() .EaseInOut() .EaseOutIn()

// Looping
.SetLoops(int count, LoopType mode)
.LoopInfinitely(LoopType mode)

// Callbacks
.OnComplete(params Action[] callbacks)
.OnLoop(Action<int> callback)
.OnUpdate(Action<double> callback)
.OnStep(Action<int> callback)

// Control
.AddToGroup(string group)
.SetSpeedScale(float scale)
.SetProcessMode(Tween.TweenProcessMode mode)
.Start()                                  // Begin the tween
```

### PathBuilder Methods

```csharp
.From(Variant value)
.To(Variant value)
.SetDuration(float duration)
// ... plus all PropertyBuilder control methods
```

### VirtualBuilder<T> Methods

```csharp
.From(T value)
.To(T value)
.SetDuration(float duration)
.OnUpdate(Action<T> callback)             // Receives interpolated value
// ... plus transitions, easing, and control methods
```

### TweenSequence Methods

```csharp
.Append(IBuilder builder)                 // Add to end
.Join(IBuilder builder)                   // Add in parallel
.Prepend(IBuilder builder)                // Add to beginning
.Insert(float time, IBuilder builder)     // Add at specific time
.AppendInterval(float duration)           // Add pause
.AppendCallback(Action callback)          // Add callback
.SetLoops(int loops)                      // Set loop count on last step
.OnComplete(Action callback)              // Sequence complete callback
.Start()                                  // Begin sequence
```

### Helper Methods

Helper methods start automatically - no need to call `.Start()`:

```csharp
// Fading
GoTween.GoFade(item, alpha, duration)
GoTween.GoFadeIn(item, duration)
GoTween.GoFadeOut(item, duration)

// Moving
GoTween.GoMove(node, target, duration)
GoTween.GoMoveLocal(node, target, duration)
GoTween.GoMoveX/Y/Z(node, value, duration)

// Scaling
GoTween.GoScale(node, scale, duration)
GoTween.GoScaleX/Y/Z(node, value, duration)

// Rotating
GoTween.GoRotate(node, degrees, duration)
GoTween.GoRotateX/Y/Z(node, degrees, duration)

// Color
GoTween.GoColor(item, color, duration)
GoTween.GoColorAlpha(item, alpha, duration)

// Effects - these return TweenSequence or VirtualBuilder
GoTween.GoShake(node, strength, duration)
GoTween.GoPunchScale(node, amount, duration)
GoTween.GoPunchPosition(node, offset, duration)
GoTween.GoPunchRotation(node, degrees, duration)
GoTween.GoBlink(item, times, duration)

// All helpers support optional config parameter:
GoTween.GoMove(node, target, duration, tween => 
{
    tween.Elastic().EaseOut();
    tween.OnComplete(() => { });
});

// Control
GoTween.GoPauseAll()
GoTween.GoResumeAll()
GoTween.GoKillAll()
GoTween.GoPauseGroup(group)
GoTween.GoKillGroup(group)
```

---

## Tips and Best Practices

### Memory Management

GoTween uses object pooling automatically. Builders are returned to the pool when tweens complete or are killed, so you don't need to worry about creating garbage.

```csharp
// This is efficient - the builder returns to the pool automatically
node.GoProperty("position").To(target).SetDuration(1f).Start();

// Helper methods also use pooling
GoTween.GoMove(node, target, 1f);

// Don't hold onto builders unnecessarily
// Bad - keeping references you don't need
this.myBuilder = node.GoProperty("position").To(target).SetDuration(1f);

// Good - just start it and let it complete
node.GoProperty("position").To(target).SetDuration(1f).Start();

// Good - store it only if you need to control it later
var builder = node.GoProperty("position").To(target).SetDuration(1f);
builder.Start();
// Now you can pause/resume/kill the builder
```

### Performance

- Use groups to manage many tweens efficiently
- Use `Virtual` tweens for non-property animations (they're lighter)

## Examples

### Hit Effect

```csharp
public void OnHit()
{
    // Flash red
    sprite.GoProperty("modulate")
        .To(Colors.Red, Colors.White)
        .SetDuration(0.1f, 0.1f)
        .Start();
    
    // Punch the scale
    GoTween.GoPunchScale(sprite, new Vector2(0.2f, 0.2f), 0.2f);
    
    // Shake briefly
    GoTween.GoShake(sprite, strength: 3f, duration: 0.15f);
}
```

### Simple Button Feedback

```csharp
public void OnButtonPressed(Button button)
{
    // Quick punch effect - no extra config needed
    GoTween.GoPunchScale(button, new Vector2(0.1f, 0.1f), 0.2f);
}

public void OnButtonHover(Button button)
{
    // Scale up smoothly
    GoTween.GoScale(button, Vector2.One * 1.1f, 0.2f, t => t.Back().EaseOut());
}

public void OnButtonUnhover(Button button)
{
    // Scale back down
    GoTween.GoScale(button, Vector2.One, 0.2f, t => t.Back().EaseOut());
}
```

---

## Contributing

Found a bug? Have a feature request? Feel free to open an issue or submit a pull request!

---
