using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GoTweening;

public partial class GoTween
{
    public static bool IsGroupActive(string group)
    {
        return builderGroups.ContainsKey(group) && builderGroups[group].Count > 0;
    }

    public static int GetGroupTweenCount(string group)
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return 0;
        return builders.Count;
    }

    public static void GoKillGroup(string group)
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return;

        var builderList = builders.ToList();
        
        foreach (var builder in builderList)
            builder.GoKillSafe();
    }

    public static void GoKillGroup(string group, params IBuilder[] excludedBuilders)
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return;

        var excludedSet = new HashSet<IBuilder>(excludedBuilders);
        var builderList = builders.ToList();

        foreach (var builder in builderList)
        {
            if (!excludedSet.Contains(builder))
                builder.GoKillSafe();
        }
    }

    public static void GoCompleteGroup(string group)
    {
        GoForEach(group, b => b.ActiveTween?.CustomStep(9999));
    }

    public static void GoPauseGroup(string group)
    {
        GoForEach(group, b => b.Pause());
    }

    public static void GoResumeGroup(string group)
    {
        GoForEach(group, b => b.Resume());
    }

    public static void GoForEach(string group, Action<IBuilder> config)
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return;

        var builderList = builders.ToList();
        
        foreach (var builder in builderList)
            config(builder);
    }

    public static IEnumerable<IBuilder> GetGroupBuilders(string group)
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return Enumerable.Empty<IBuilder>();
        
        return builders.ToList();
    }

    public static IEnumerable<T> GetGroupBuilders<T>(string group) where T : IBuilder
    {
        if (!builderGroups.TryGetValue(group, out var builders))
            return Enumerable.Empty<T>();
        
        return builders.OfType<T>().ToList();
    }

    public static IEnumerable<IBuilder> GetBuildersForTarget(GodotObject target)
    {
        return activeBuilders.Where(b => b.Target == target).ToList();
    }

    public static IEnumerable<string> GetActiveGroups()
    {
        return builderGroups.Keys.ToList();
    }

    #region Enum Based Group Management
    public static void GoKillGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        GoKillGroup(group.ToString());
    }

    public static void GoPauseGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        GoPauseGroup(group.ToString());
    }

    public static void GoResumeGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        GoResumeGroup(group.ToString());
    }

    public static void GoCompleteGroup<TGroup>(TGroup group) where TGroup : Enum
    {
        GoCompleteGroup(group.ToString());
    }

    public static bool IsGroupActive<TGroup>(TGroup group) where TGroup : Enum
    {
        return IsGroupActive(group.ToString());
    }

    public static int GetGroupTweenCount<TGroup>(TGroup group) where TGroup : Enum
    {
        return GetGroupTweenCount(group.ToString());
    }

    public static IEnumerable<IBuilder> GetGroupBuilders<TGroup>(TGroup group) where TGroup : Enum
    {
        return GetGroupBuilders(group.ToString());
    }

    public static void GoForEach<TGroup>(TGroup group, Action<IBuilder> config) where TGroup : Enum
    {
        GoForEach(group.ToString(), config);
    }
    #endregion
}