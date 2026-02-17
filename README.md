# GoTween

A fluent, high-performance tweening library for **Godot 4 + C#**.

GoTween wraps Godot's native `Tween` system with a clean, chainable API while adding virtual tweens, curve-driven path animation, sequence orchestration, object pooling, and group management — all with zero boilerplate.

---

## Table of Contents

- [Setup](#setup)
- [PropertyBuilder](#propertybuilder)
  - [Basic Usage](#basic-usage)
  - [Multi-Step Tweens](#multi-step-tweens)
  - [Sub-Properties](#sub-properties)
  - [Relative Tweens](#relative-tweens)
  - [Transitions & Easing](#transitions--easing)
  - [Loops](#loops)
  - [Callbacks](#callbacks)
  - [Runtime Control](#runtime-control)
- [Shortcut Methods](#shortcut-methods)
  - [Movement](#movement)
  - [Scale](#scale)
  - [Rotation](#rotation)
  - [Fade & Colour](#fade--colour)
  - [UI Helpers](#ui-helpers)
  - [Audio](#audio)
  - [Camera](#camera)
- [Built-In Effects](#built-in-effects)
  - [Shake](#shake)
  - [Punch](#punch)
  - [Blink](#blink)
- [PathBuilder](#pathbuilder)
- [VirtualBuilder\<T\>](#virtualbuildert)
- [TweenSequence](#tweensequence)
- [Group Management](#group-management)
- [Common Patterns](#common-patterns)
- [API Quick Reference](#api-quick-reference)

---

## Setup

Add `GoTween` as an **AutoLoad** singleton in your project settings:

```
Project Settings → AutoLoad → GoTween → res://GoTween/GoTween.cs
```

Once active, all static methods (`GoTween.GoMove`, `GoTween.GoFade`, etc.) and extension methods (`.GoProperty()`, `.GoPath()`) are available from any script.

---

## PropertyBuilder

`PropertyBuilder` animates any exported property on a `GodotObject` using Godot's `TweenProperty` under the hood. Get one via the `.GoProperty()` extension method.

### Basic Usage

```csharp
myNode.GoProperty("position")
    .To(new Vector2(400, 300))
    .SetDuration(1.5f)
    .Sine().EaseOut()
    .Start();
```

### Multi-Step Tweens

Pass multiple values and matching durations to chain steps in a single builder.

```csharp
mySprite.GoProperty("scale")
    .To(new Vector2(1.2f, 1.2f), new Vector2(0.8f, 0.8f), Vector2.One)
    .SetDuration(0.3f, 0.3f, 0.4f)
    .Elastic().EaseOut()
    .Start();
```

Use `OnStep(int step)` to fire a callback after each individual step.

```csharp
myNode.GoProperty("position")
    .To(pointA, pointB, pointC)
    .SetDuration(0.5f, 0.5f, 0.5f)
    .OnStep(step => GD.Print($"Reached point {step}"))
    .Start();
```

### Sub-Properties

GoTween supports Godot's `property:component` syntax to tween a single axis or channel without affecting the rest of the value.

```csharp
// Only move the X axis
myNode.GoProperty("global_position:x")
    .To(500f).SetDuration(1f).Quad().EaseInOut().Start();

// Only fade the alpha channel
mySprite.GoProperty("modulate:a")
    .To(0f).SetDuration(0.5f).Start();
```

| Type | Valid Components |
|------|-----------------|
| `Vector2` | `x`, `y` |
| `Vector3` | `x`, `y`, `z` |
| `Vector4` | `x`, `y`, `z`, `w` |
| `Color` | `r`, `g`, `b`, `a`, `h`, `s`, `v` |
| `Rect2` | `position`, `size`, `end` |
| `Quaternion` | `x`, `y`, `z`, `w` |

### Relative Tweens

Call `.AsRelative()` to treat the target value as an offset from the current value.

```csharp
// Move 200 units to the right of wherever the node currently is
myNode.GoProperty("position")
    .To(new Vector2(200, 0))
    .SetDuration(0.8f)
    .AsRelative()
    .Start();
```

### Transitions & Easing

Chain a transition type and an ease type together.

```csharp
myNode.GoProperty("position").To(target).SetDuration(1f)
    .Elastic().EaseOut()
    .Start();
```

**Transition methods:** `Linear()` `Sine()` `Quad()` `Cubic()` `Quart()` `Quint()` `Expo()` `Circ()` `Elastic()` `Back()` `Bounce()` `Spring()`

**Ease methods:** `EaseIn()` `EaseOut()` `EaseInOut()` `EaseOutIn()`

Or set them explicitly with raw Godot enums:

```csharp
.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out)
```

### Loops

```csharp
// Repeat 3 times
myNode.GoProperty("position").To(target).SetDuration(1f)
    .SetLoops(3).Start();

// PingPong — goes there and back each loop
myNode.GoProperty("scale").To(bigScale).SetDuration(0.5f)
    .SetLoops(4, LoopType.PingPong).Start();

// Loop forever
myNode.GoProperty("rotation_degrees").To(360f).SetDuration(2f)
    .LoopInfinitely().Start();

// Infinite PingPong
myNode.GoProperty("position").To(patrolB).SetDuration(2f)
    .LoopInfinitely(LoopType.PingPong).Start();
```

### Callbacks

```csharp
myNode.GoProperty("position").To(target).SetDuration(1f)
    .OnComplete(() => GD.Print("Done!"))
    .OnLoop(loopIndex => GD.Print($"Loop #{loopIndex}"))
    .OnUpdate(delta => GD.Print($"dt={delta:F4}"))
    .Start();
```

Multiple `OnComplete` callbacks are supported and each one is properly cleaned up when the tween finishes.

### Runtime Control

```csharp
var builder = myNode.GoProperty("position").To(target).SetDuration(2f);
builder.Start();

builder.Pause();                    // freeze
builder.Resume();                   // continue
builder.Cancel();                   // kill and return to pool
builder.Replay();                   // kill current run and restart
builder.Replay(cancelSubs: true);   // restart and clear OnComplete callbacks

float progress   = builder.GetProgress();       // 0..1
float elapsed    = builder.GetElapsedTime();    // seconds
float remaining  = builder.GetRemainingTime();  // seconds
float total      = builder.GetTotalDuration();  // seconds
```

---

## Shortcut Methods

Pre-built static helpers for the most common animation tasks. All accept an optional `Action<PropertyBuilder> config` lambda for customisation.

### Movement

```csharp
// Global position (2D or 3D)
GoTween.GoMove(enemy, new Vector2(500, 200), 1.5f, b => b.Quad().EaseOut());
GoTween.GoMove(node3D, new Vector3(0, 0, 10), 2f);

// Local position
GoTween.GoMoveLocal(myNode, new Vector2(100, 0), 0.5f);

// Single axis
GoTween.GoMoveX(myNode, 400f, 1f);
GoTween.GoMoveY(myNode, 300f, 1f);
GoTween.GoMoveZ(node3D, -5f, 1f);

// Local single axis
GoTween.GoMoveLocalX(myNode, 100f, 0.5f);

// Speed-based — duration is calculated from distance / speed
GoTween.GoMoveAtSpeed(bullet, targetPos, 800f);
```

### Scale

```csharp
GoTween.GoScale(mySprite, new Vector2(1.5f, 1.5f), 0.3f);
GoTween.GoScale(node3D, new Vector3(2f, 2f, 2f), 0.5f);
GoTween.GoScaleX(myNode, 2f, 0.3f);
GoTween.GoScaleY(myNode, 0.5f, 0.3f);
```

### Rotation

```csharp
// 2D rotation in degrees
GoTween.GoRotate(wheel, 360f, 2f, b => b.Linear().LoopInfinitely());

// 3D Euler rotation
GoTween.GoRotate(node3D, new Vector3(0, 90, 0), 1f);

// Single 3D axis
GoTween.GoRotateY(node3D, 180f, 1f);
```

### Fade & Colour

```csharp
GoTween.GoFadeIn(myPanel, 0.5f);
GoTween.GoFadeOut(myPanel, 0.5f, b => b.OnComplete(() => myPanel.QueueFree()));
GoTween.GoFade(mySprite, 0.5f, 0.3f);   // fade to 50% alpha

GoTween.GoModulate(mySprite, Colors.Red, 0.2f);
GoTween.GoSelfModulate(mySprite, new Color(1, 1, 1, 0.5f), 0.3f);
GoTween.GoColor(colorRect, Colors.Blue, 1f);
```

### UI Helpers

```csharp
// Slide in from a screen edge
GoTween.GoSlideIn(hudPanel, GoTween.Direction.Up, 0.4f);
// Direction options: Left, Right, Up, Down

// Typewriter text reveal
GoTween.GoTypewriter(dialogueLabel, 2f);

// Animate Control size
GoTween.GoSize(myPanel, new Vector2(300, 200), 0.5f);

// Animate skew
GoTween.GoSkew(myNode2D, 0.2f, 0.3f);

// Animate a ProgressBar or HSlider
GoTween.GoRange(healthBar, newHealth, 0.5f);
```

### Audio

```csharp
// Fade out music
GoTween.GoVolume(musicPlayer, -80f, 2f, b => b.OnComplete(() => musicPlayer.Stop()));

// Tween pitch
GoTween.GoPitch(sfxPlayer, 1.5f, 0.1f);
```

### Camera

```csharp
// 2D zoom (applies the same value to both X and Y)
GoTween.GoZoom(camera2D, 2f, 0.5f);

// 3D field of view
GoTween.GoFOV(camera3D, 90f, 0.3f, b => b.Quad().EaseOut());
```

---

## Built-In Effects

### Shake

Decaying positional shake that auto-restores the original position on completion. Works on `Node2D`, `Node3D`, and `Control`.

```csharp
// Position shake
GoTween.GoShake(camera2D, intensity: 20f, duration: 0.5f);
GoTween.GoShake(node3D, intensity: 5f, duration: 0.3f);
GoTween.GoShake(uiPanel, intensity: 8f, duration: 0.4f);

// Rotational shake
GoTween.GoShakeRotation(sprite, intensityDegrees: 15f, duration: 0.4f);
```

### Punch

Quickly animates a property outward then springs back using an `Elastic` ease. Great for button feedback and hit reactions.

```csharp
// Scale punch — ideal for button press feedback
GoTween.GoPunchScale(myButton, new Vector2(0.2f, 0.2f), 0.4f);
GoTween.GoPunchScale(node3D, new Vector3(0.3f, 0.3f, 0.3f), 0.4f);

// Position punch — hit impact feedback
GoTween.GoPunchPosition(hitEnemy, new Vector2(10f, 0f), 0.3f);
GoTween.GoPunchPosition(node3D, new Vector3(0.2f, 0, 0), 0.3f);

// Rotation punch
GoTween.GoPunchRotation(sprite2D, punchDegrees: 15f, duration: 0.4f);
GoTween.GoPunchRotation(node3D, new Vector3(0, 15f, 0), 0.4f);
```

### Blink

Toggles a `CanvasItem`'s visibility N times over a duration, then leaves it visible.

```csharp
// Blink 6 times over 1.2 seconds (e.g. invincibility frames)
GoTween.GoBlink(invincibilitySprite, times: 6, duration: 1.2f);
```

---

## PathBuilder

<img width="299" height="304" alt="curve" src="https://github.com/user-attachments/assets/bd262188-ad3b-423f-8e5e-d37532a26108" />

`PathBuilder` uses a Godot `Curve` resource to drive interpolation instead of a built-in easing function. Edit the curve visually in the Inspector for full control over the animation shape.

```csharp
[Export] public Curve BounceCurve;

myNode.GoPath("position", BounceCurve)
    .From(new Vector2(0, 0))
    .To(new Vector2(0, -200))
    .SetDuration(1.5f)
    .SetLoops(3, LoopType.PingPong)
    .Start();
```

The curve is sampled `0 → 1` over the duration; its output is used as the blend factor between `From` and `To`. Supports all `VariantMath` types: `float`, `int`, `Vector2`, `Vector3`, `Color`, `Quaternion`.

---

## VirtualBuilder\<T\>

`VirtualBuilder<T>` runs entirely in C# without touching Godot's `Tween` engine. It calls your `OnUpdate` callback every frame with the interpolated value — perfect for animating anything that isn't a Godot property (shader parameters, game logic values, procedural offsets, etc.).

### GoVirtual Factory

```csharp
GoVirtual.Float(from, to, duration, onUpdate);
GoVirtual.Int(from, to, duration, onUpdate);       // rounds to nearest int
GoVirtual.Vector2(from, to, duration, onUpdate);
GoVirtual.Vector3(from, to, duration, onUpdate);
GoVirtual.Color(from, to, duration, onUpdate);
GoVirtual.Quaternion(from, to, duration, onUpdate); // uses Slerp
GoVirtual.Vector4(from, to, duration, onUpdate);
GoVirtual.Create<T>(from, to, duration, interpolator, onUpdate); // any type
```

### Examples

```csharp
// Tween a shader dissolve parameter
GoVirtual.Float(0f, 1f, 1.5f,
    v => material.SetShaderParameter("dissolve", v))
    .Sine().EaseInOut();

// Animated score counter
GoVirtual.Int(oldScore, newScore, 0.8f,
    v => scoreLabel.Text = v.ToString());

// Custom type with your own interpolator
GoVirtual.Create(
    from: new MyStruct { x = 0, y = 0 },
    to:   new MyStruct { x = 100, y = 50 },
    duration: 1f,
    interpolator: (a, b, t) => new MyStruct {
        x = Mathf.Lerp(a.x, b.x, t),
        y = Mathf.Lerp(a.y, b.y, t)
    },
    onUpdate: val => ApplyMyStruct(val)
);
```

`VirtualBuilder<T>` supports the full fluent API: all transition types, easing methods, `Wait`, `SetLoops`, `LoopInfinitely`, `OnComplete`, `OnLoop`, `AddToGroup`, etc.

---

## TweenSequence

`TweenSequence` composes multiple builders into a timeline. Get one from the pool with `GoTween.Sequence()` and start it with `.Start()`.

### Append — sequential

```csharp
GoTween.Sequence()
    .Append(GoTween.GoMove(myNode, targetPos, 0.5f, t => t.Sine().EaseInOut()))
    .Append(GoTween.GoScale(myNode, Vector2.One * 2f, 0.5f))
    .Append(myNode.GoProperty("modulate:a").To(0f).SetDuration(0.4f))
    .OnComplete(() => myNode.QueueFree())
    .Start();
```

### Join — parallel

`Join` plays a tween at the same start time as the previous step.

```csharp
GoTween.Sequence()
    .Append(myNode.GoProperty("position").To(targetPos).SetDuration(1f))
    .Join(myNode.GoProperty("modulate:a").To(0f).SetDuration(1f))
    .Start();
```

### Insert — at a specific timestamp

```csharp
GoTween.Sequence()
    .Append(intro)             // 0 – 1 s
    .AppendInterval(0.5f)      // pause
    .Insert(0.8f, flashBurst)  // always starts at t = 0.8 s
    .Start();
```

### Prepend

Inserts a builder at the very beginning, shifting all existing steps forward.

```csharp
seq.Prepend(flashBuilder);
```

### AppendCallback

Fires a callback when the previous step completes.

```csharp
GoTween.Sequence()
    .Append(moveBuilder)
    .AppendCallback(() => GD.Print("Arrived!"))
    .Append(fadeBuilder)
    .Start();
```

### Groups in sequences

Pass a group name to `GoTween.Sequence()` and all tweens inside will inherit it.

```csharp
GoTween.Sequence("cutscene")
    .Append(cameraMove)
    .Append(dialogue)
    .Start();

// Skip the cutscene
GoTween.GoKillGroup("cutscene");
```

---

## Group Management

Tag any builder with a group string or enum to control many tweens at once.

```csharp
// String group
GoTween.GoMove(enemy, target, 1f, b => b.AddToGroup("enemies"));

// Enum group (type-safe)
public enum TweenGroup { UI, Enemies, Cutscene }

myButton.GoProperty("scale").To(bigScale).SetDuration(0.3f)
    .AddToGroup(TweenGroup.UI)
    .Start();
```

### Group operations

```csharp
GoTween.GoPauseGroup("ui");
GoTween.GoResumeGroup("ui");
GoTween.GoKillGroup("ui");
GoTween.GoCompleteGroup("ui");          // instantly finish all tweens in group
GoTween.GoKillGroup("ui", excluded);   // kill all except specific builders
GoTween.GoForEach("ui", b => b.SetSpeedScale(2f));

bool active = GoTween.IsGroupActive("ui");
int count   = GoTween.GetGroupTweenCount("ui");
var builders = GoTween.GetGroupBuilders<PropertyBuilder>("ui");
```

All group methods accept both `string` and `Enum` overloads.

### Global operations

```csharp
GoTween.GoPauseAll();
GoTween.GoResumeAll();
GoTween.GoKillAll();

// Pause/resume all except specific groups
GoTween.GoPauseAll("ui", "hud");
GoTween.GoResumeAll("ui", "hud");

// Target-based
GoTween.GoKillTarget(myNode);
GoTween.GoPauseTarget(myNode);
myNode.GoKillAll();   // extension method — same effect

// Queries
int total     = GoTween.GetActiveTweenCount();
bool tweening = GoTween.IsPropertyTweening(myNode, "position");
var builder   = GoTween.GetTweenForProperty(myNode, "position");
```

---

## Common Patterns

### Popup (scale + fade in)

```csharp
public void ShowPopup(Control popup)
{
    popup.Modulate = new Color(1, 1, 1, 0);
    popup.Scale = Vector2.Zero;
    popup.Visible = true;

    GoTween.Sequence()
        .Append(GoTween.GoScale(popup, Vector2.One, 0.35f, t => t.Back().EaseOut()))
        .Join(GoTween.GoFadeIn(popup, 0.25f, t => t.Quad().EaseOut()))
        .Start();
}
```

### Hit flash

```csharp
public void PlayHitFlash(CanvasItem target)
{
    GoTween.Sequence()
        .Append(GoTween.GoModulate(target, Colors.Red, 0.05f))
        .Append(GoTween.GoModulate(target, Colors.White, 0.15f))
        .Start();
}
```

### Camera trauma shake

```csharp
public void CameraShake(float damage)
{
    float intensity = Mathf.Clamp(damage * 0.5f, 3f, 25f);
    GoTween.GoShake(mainCamera2D, intensity, 0.4f);
}
```

### Patrol loop (infinite PingPong)

```csharp
GoTween.GoMove(enemy, patrolPointB, 2f,
    b => b.Sine().EaseInOut()
         .LoopInfinitely(TweenBuilder.LoopType.PingPong));
```

### Safe cleanup when a node is freed

```csharp
public override void _ExitTree()
{
    this.GoKillAll();
}
```

### Inline config lambda

```csharp
mySprite.GoProperty("modulate", b => b
    .To(Colors.Red)
    .SetDuration(0.5f)
    .Sine().EaseInOut()
    .OnComplete(() => GD.Print("done")));
```

---

## API Quick Reference

### Builder methods (all builder types)

| Method | Description |
|--------|-------------|
| `.To(value[s])` | Target value, or multiple values for multi-step |
| `.From(value)` | Override the starting value |
| `.SetDuration(dur[s])` | Duration in seconds (one per step) |
| `.Wait(seconds)` | Delay before the tween begins |
| `.SetLoops(n, mode)` | Repeat N times — `Normal` or `PingPong` |
| `.LoopInfinitely(mode)` | Loop forever |
| `.AsRelative()` | Treat `To` value as an offset |
| `.SetSpeedScale(scale)` | Multiplier on playback speed |
| `.SetProcessMode(mode)` | `Idle` / `Physics` / `Always` |
| `.AddToGroup(name)` | Tag for group management |
| `.OnComplete(cb)` | Fire when done |
| `.OnLoop(cb)` | Fire each loop with loop index |
| `.OnUpdate(cb)` | Fire every frame with delta |
| `.Start()` | Begin the tween, returns `Tween` |
| `.Pause()` / `.Resume()` | Freeze / unfreeze |
| `.Cancel()` | Kill and return to pool |
| `.Replay()` | Kill current run and restart |
| `.GetProgress()` | `0..1` progress float |
| `.GetElapsedTime()` | Seconds elapsed |
| `.GetRemainingTime()` | Seconds remaining |
| `.GetTotalDuration()` | Total expected duration in seconds |

### TweenSequence methods

| Method | Description |
|--------|-------------|
| `.Append(builder)` | Add a step after all existing steps |
| `.Join(builder)` | Play at the same time as the previous step |
| `.Insert(time, builder)` | Place a step at an exact timestamp |
| `.Prepend(builder)` | Insert at the beginning, shift everything else |
| `.AppendCallback(action)` | Fire a callback when the previous step ends |
| `.AppendInterval(seconds)` | Add a gap with no animation |
| `.OnComplete(action)` | Fire when the entire sequence ends |
| `.Start()` | Begin the sequence |

---

> **Pooling note:** All builders and sequences are automatically returned to an internal pool when they complete or are cancelled. You never need to manage the pool manually.
