using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Output the build size or a failure depending on BuildPlayer.

public class BuildScript : MonoBehaviour
{
    [MenuItem("Build/Build&Run Standalone")]
    public static void MyBuild()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenHeight = 1000;
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.runInBackground = true;
        PlayerSettings.resizableWindow = true;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/World.unity" };
        buildPlayerOptions.target = selectBuildTarget();
        buildPlayerOptions.locationPathName = "build/terrain-viewer";
        buildPlayerOptions.options = BuildOptions.None;

        if (buildPlayerOptions.target == BuildTarget.StandaloneWindows64)
        {
            buildPlayerOptions.locationPathName += ".exe";
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            Process proc = new Process();
            string relativePath = buildPlayerOptions.locationPathName + (buildPlayerOptions.target == BuildTarget.StandaloneOSX ? ".app" : "");
            proc.StartInfo.FileName = Path.GetFullPath(relativePath);
            proc.Start();
        }

        if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.Log("Build failed");
        }
    }

    private static BuildTarget selectBuildTarget()
    {
        if (Application.platform == RuntimePlatform.LinuxEditor)
        {
            return BuildTarget.StandaloneLinux64;
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            return BuildTarget.StandaloneWindows64;
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            return BuildTarget.StandaloneOSX;
        }
        return BuildTarget.WebGL;
    }
}
