using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Android;
using UnityEngine;

public static class BuildAndroid
{
    private const string BundlePath = "Builds/Android/FreeHorseRun.aab";
    private const string PackageName = "com.rammicz.freehorserun";

    [MenuItem("Build/Build Android App Bundle")]
    public static void BuildAppBundle()
    {
        ConfigurePlayerSettings();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;

        var outputPath = Path.GetFullPath(BundlePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException("Missing build directory."));

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Main.unity" },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.StrictMode
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");

        Debug.Log($"Android App Bundle created at {outputPath}");
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "Rammi";
        PlayerSettings.productName = "Free Horse Run";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        ConfigureUploadKey();

        SetToolPathIfPresent(AndroidExternalToolsSettings.sdkRootPath, value => AndroidExternalToolsSettings.sdkRootPath = value, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Android/Sdk");
        SetToolPathIfPresent(AndroidExternalToolsSettings.ndkRootPath, value => AndroidExternalToolsSettings.ndkRootPath = value, "W:/UnityTools/android-ndk-r27c");
        SetToolPathIfPresent(AndroidExternalToolsSettings.jdkRootPath, value => AndroidExternalToolsSettings.jdkRootPath = value, "W:/UnityTools/OpenJDK17");
    }

    private static void SetToolPathIfPresent(string currentPath, Action<string> setPath, string candidatePath)
    {
        if (Directory.Exists(candidatePath) && !string.Equals(currentPath, candidatePath, StringComparison.OrdinalIgnoreCase))
            setPath(candidatePath);
    }

    private static void ConfigureUploadKey()
    {
        var keyStorePath = Environment.GetEnvironmentVariable("FREE_HORSE_RUN_KEYSTORE_PATH");
        var keyStorePassword = Environment.GetEnvironmentVariable("FREE_HORSE_RUN_KEYSTORE_PASSWORD");

        if (string.IsNullOrWhiteSpace(keyStorePath) || string.IsNullOrWhiteSpace(keyStorePassword) || !File.Exists(keyStorePath))
            throw new BuildFailedException("Set FREE_HORSE_RUN_KEYSTORE_PATH and FREE_HORSE_RUN_KEYSTORE_PASSWORD before creating a release bundle.");

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keyStorePath;
        PlayerSettings.Android.keystorePass = keyStorePassword;
        PlayerSettings.Android.keyaliasName = "upload";
        PlayerSettings.Android.keyaliasPass = keyStorePassword;
    }
}
