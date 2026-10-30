using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class FortressBuildVerification
{
    public static void Build()
    {
        string output = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../ReviewArtifacts/TowerEconomy/Player/RoyalFortress.exe"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/RoyalFortressGameplay.unity" },
            locationPathName = output, target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Verification player build failed: " + report.summary.result);
    }
}
