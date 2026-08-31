# Android release

The Unity project is in `Application/` and is built with Unity `6000.3.23f1`.

## Release requirements

- Unity Personal or a valid Unity licence
- Android SDK with platform 36, command-line tools and CMake 3.22.1
- Android NDK r27c
- OpenJDK 17.0.18
- a local upload keystore (never commit it)

## Build

Set these variables before building:

```powershell
$env:FREE_HORSE_RUN_KEYSTORE_PATH = 'path-to-upload-key.jks'
$env:FREE_HORSE_RUN_KEYSTORE_PASSWORD = 'upload-key-password'
```

In Unity, run **Build > Build Android App Bundle**. The release bundle is written to
`Application/Builds/Android/FreeHorseRun.aab`.

The build configuration is in `Application/Assets/Editor/BuildAndroid.cs`:

- application ID: `com.rammicz.freehorserun`
- version: `1.0.0` / version code `1`
- minimum Android API: `25`
- target Android API: `36`
- ARM64 and IL2CPP

Keep the upload key and its password outside the repository. Google Play App Signing
should manage the distribution key after the first upload.
