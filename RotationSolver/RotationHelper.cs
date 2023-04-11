﻿using Dalamud.Interface.Colors;
using Dalamud.Logging;
using RotationSolver.Updaters;
using System.Diagnostics;
using System.Text;

namespace RotationSolver;

internal static class RotationHelper
{
    public static string[] AllowedAssembly { get; private set; } = new string[0];
    static readonly SortedList<Assembly, string> _authors = new SortedList<Assembly, string>();
    public static async void LoadList()
    {
        using (var client = new HttpClient())
        {
            try
            {
                var bts = await client.GetByteArrayAsync("https://raw.githubusercontent.com/ArchiDog1998/RotationSolver/main/Resources/whitelist.json");
                AllowedAssembly = JsonConvert.DeserializeObject<string[]>(Encoding.Default.GetString(bts));
            }
            catch (Exception ex)
            {
                PluginLog.Log(ex, "Failed to load white List.");
            }
        }
    }
    
    public static bool IsAllowed(this ICustomRotation rotation, out string name)
    {
        if (rotation == null)
        {
            name = "Unknown";
            return false;
        }
        var assembly = rotation.GetType().Assembly;
        name = assembly.GetName().Name;

        return AllowedAssembly.Contains(name + " - " + assembly.GetAuthor());
    }

    public static Vector4 GetColor(this ICustomRotation rotation)
        => !rotation.IsAllowed(out _) ? ImGuiColors.DalamudViolet : rotation.IsBeta() 
        ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudWhite ;

    public static bool IsBeta(this ICustomRotation rotation)
        => rotation.GetType().GetCustomAttribute<BetaRotationAttribute>() != null;

    public static string GetAuthor(this Assembly assembly)
    {
        if (_authors.TryGetValue(assembly, out var author)) return author;
        try
        {
            var name = assembly.GetName().Name;
            return _authors[assembly] = 
                (RotationLoadContext.AssemblyPaths.TryGetValue(name, out var path) 
                ? FileVersionInfo.GetVersionInfo(path)?.CompanyName : name)
                ?? name ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
