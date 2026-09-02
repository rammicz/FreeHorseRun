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
        PlayerSettings.productName = "Horse Run";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        ConfigureIcons();
        ConfigureNativeDebugSymbols();
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

    private static void ConfigureIcons()
    {
        // Play rejected the listing because the launcher icon in the bundle did not match the
        // hi-res icon uploaded to the store (Misleading Claims policy: "App store listing mismatch").
        // AppIcon.png is the same artwork as play-store-assets/app-icon-512.png, so every launcher
        // icon kind is filled from it. The adaptive icon uses the artwork as the full-bleed background
        // with a transparent foreground, so the device mask only rounds the corners instead of
        // cropping a zoomed-in foreground layer.
        var icon = LoadIcon("Assets/Icons/AppIcon.png");
        var transparent = LoadIcon("Assets/Icons/AppIconTransparent.png");
        var target = NamedBuildTarget.Android;

        foreach (var kind in new[] { AndroidPlatformIconKind.Legacy, AndroidPlatformIconKind.Round, AndroidPlatformIconKind.Adaptive })
        {
            var icons = PlayerSettings.GetPlatformIcons(target, kind);
            foreach (var platformIcon in icons)
            {
                if (kind == AndroidPlatformIconKind.Adaptive)
                    platformIcon.SetTextures(icon, transparent);
                else
                    platformIcon.SetTexture(icon);
            }

            PlayerSettings.SetPlatformIcons(target, kind, icons);
        }
    }

    private static Texture2D LoadIcon(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath)
               ?? throw new BuildFailedException($"Launcher icon texture missing: {assetPath}");
    }

    private static void ConfigureNativeDebugSymbols()
    {
        // IL2CPP puts the game code in native libraries, so Play Console can only symbolicate
        // crashes and ANRs when the bundle carries the native symbol tables.
        // SymbolTable is what Play needs; Full also embeds DWARF debug info and is much larger.
        // Types are fully qualified because Unity.Android.Types also declares AndroidArchitecture,
        // which would clash with the UnityEditor one used above.
        UserBuildSettings.DebugSymbols.level = Unity.Android.Types.DebugSymbolLevel.SymbolTable;

        // Ship the symbols inside the bundle so Play picks them up on upload. The alternative,
        // DebugSymbolFormat.Zip, writes a separate archive that has to be uploaded by hand.
        // Play strips the symbols before distribution, so this does not grow the user download.
        UserBuildSettings.DebugSymbols.format = Unity.Android.Types.DebugSymbolFormat.IncludeInBundle;
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
