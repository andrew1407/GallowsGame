using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

// Player builds for the menu (Build/...) and for batch mode:
//   Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
// Every build lands in dist/<Platform>/ (ignored by git)
public static class BuildScript
{
    private const string _outputRoot = "dist";

    private static readonly (string Platform, BuildTarget Target, string Location)[] _targets = {
        ("Windows", BuildTarget.StandaloneWindows64, "GallowsGame.exe"),
        ("Linux", BuildTarget.StandaloneLinux64, "GallowsGame.x86_64"),
        ("macOS", BuildTarget.StandaloneOSX, "GallowsGame.app"),
        ("WebGL", BuildTarget.WebGL, string.Empty),
    };

    [MenuItem("Build/Windows")]
    public static void BuildWindows() => build("Windows");

    [MenuItem("Build/Linux")]
    public static void BuildLinux() => build("Linux");

    [MenuItem("Build/macOS")]
    public static void BuildMacOS() => build("macOS");

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        ConfigureWebGraphics();
        build("WebGL");
    }

    // WebGPU first, WebGL 2 second: the player starts with the first API the browser supports,
    // so browsers without WebGPU (or without a WebGPU adapter) fall back to WebGL 2
    [MenuItem("Build/Configure WebGPU with WebGL 2 Fallback")]
    public static void ConfigureWebGraphics()
    {
        GraphicsDeviceType[] apis = { GraphicsDeviceType.WebGPU, GraphicsDeviceType.OpenGLES3 };
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, apis);
        AssetDatabase.SaveAssets();
        string applied = string.Join(", ", PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL));
        Debug.Log($"WebGL graphics APIs: {applied}");
    }

    // Builds every platform whose module is installed with this editor and skips the rest
    [MenuItem("Build/All Installed Platforms")]
    public static void BuildAll()
    {
        foreach (var target in _targets)
        {
            if (BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target.Target), target.Target))
                build(target.Platform);
            else
                Debug.LogWarning($"Skipping {target.Platform}: its build support module isn't installed");
        }
    }

    private static void build(string platform)
    {
        var target = _targets.First(t => t.Platform == platform);
        string location = Path.Combine(_outputRoot, platform, target.Location);
        var options = new BuildPlayerOptions() {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = location,
            target = target.Target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target.Target),
        };

        BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
        string sizeMb = (summary.totalSize / 1024f / 1024f).ToString("0.0");
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"{platform} build succeeded: {location} ({sizeMb} MB, {summary.totalTime:mm\\:ss})");
            return;
        }
        // An exception makes batch mode exit with a non-zero code
        throw new Exception($"{platform} build {summary.result.ToString().ToLower()} with {summary.totalErrors} error(s)");
    }
}
