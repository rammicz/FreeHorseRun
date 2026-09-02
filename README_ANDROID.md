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
- version: `1.0.0` / version code `2`
- product name: `Horse Run` (must match the Play store listing name)
- launcher icon: `Application/Assets/Icons/AppIcon.png`, the same artwork as the store hi-res icon
  `play-store-assets/app-icon-512.png`. Play rejects the app (Misleading Claims: store listing
  mismatch) when the on-device icon or name differs from the listing.
- minimum Android API: `25`
- target Android API: `36`
- ARM64 and IL2CPP
- native debug symbols: symbol table, embedded in the bundle

## Native debug symbols

The build embeds ARM64 symbol tables in the bundle, so Play Console can symbolicate native
crashes and ANRs. Because the game code lives in IL2CPP native libraries, stack traces are
unreadable without them.

Play strips the symbols before distributing the app, so the user download does not grow, but
the uploaded bundle is noticeably larger than one built without symbols.

To go back to a separate archive that has to be uploaded to Play Console by hand, change
`DebugSymbolFormat.IncludeInBundle` to `DebugSymbolFormat.Zip` in `BuildAndroid.cs`.

Java and Kotlin code is not minified, so the bundle carries no R8 mapping file. Play Console
reports this as a warning. Enabling minification would only shrink the few megabytes of DEX
that Unity generates, while risking breakage in code reached through reflection or JNI, so it
stays off.

Keep the upload key and its password outside the repository. Google Play App Signing
should manage the distribution key after the first upload.
