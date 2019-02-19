using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Diagnostics;

// Output the build size or a failure depending on BuildPlayer.

public class BuildScript : MonoBehaviour
{
    [MenuItem("Build/Build Linux")]
    public static void MyBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/World.unity"};
        buildPlayerOptions.locationPathName = "linux-build";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
	    Process proc = new Process();
	    proc.StartInfo.FileName = buildPlayerOptions.locationPathName;
	    proc.Start();
        }

        if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.Log("Build failed");
        }
    }
}
