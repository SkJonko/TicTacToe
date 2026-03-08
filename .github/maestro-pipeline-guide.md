# Maestro CI/CD Pipeline Guide

## Overview

This guide explains the GitHub Actions workflow for running Maestro UI tests on your .NET MAUI Android application. The pipeline is completely free and uses only open-source tools.

## Pipeline Architecture

```
Checkout Code
    ↓
Setup Java & .NET
    ↓
Cache Android SDK (saves ~2-5 min)
    ↓
Install Android SDK Components
    ↓
Create Android Virtual Device (AVD)
    ↓
Start Emulator (5-120 seconds)
    ↓
Build MAUI APK (Release)
    ↓
Install APK on Emulator
    ↓
Install Maestro CLI
    ↓
Run Maestro Tests
    ↓
Collect Logs on Failure
```

## Key Configuration Details

### Java & .NET Setup
- **Java 17**: Required for Android SDK, emulator, and Gradle build system
- **.NET 10**: Update version to match your `TicToe.Infinite.csproj` TargetFrameworks
- Gradle caching enabled for faster builds

### Android SDK Caching

The workflow caches `/usr/local/lib/android/sdk` to save 2-5 minutes per run:

```yaml
cache:
  path: /usr/local/lib/android/sdk
  key: android-sdk-${{ runner.os }}-${{ hashFiles('**/local.properties') }}
```

**Why this matters**: The Android SDK is ~2GB. Downloading on every run = 2-5 min wasted per test run.

### Emulator Configuration

The workflow uses these optimized settings for CI:

| Setting | Value | Reason |
|---------|-------|--------|
| **API Level** | 34 (Android 14) | Modern API, widely supported |
| **Architecture** | x86_64 | Faster emulation than ARM on CI runners |
| **Device** | Nexus 5X | Standard screen size (5"), represents common devices |
| **Cores** | 2 | Prevents exhausting runner CPU |
| **Memory** | 2048 MB | Balanced for reliability without OOM |
| **Acceleration** | KVM | Hardware acceleration on Linux |

### Boot Detection Strategy

The workflow uses a robust boot check instead of arbitrary delays:

```bash
until [[ -n $(${ANDROID_HOME}/platform-tools/adb shell getprop sys.boot_completed 2>&1 | grep "^1") ]]; do
  echo "Booting..." && sleep 5
done
```

**Why**: The `sys.boot_completed` property is more reliable than sleeping for 120 seconds:
- Boot usually takes 60-120s on CI runners
- This method doesn't wait longer than necessary
- Fails fast if emulator is unresponsive

### APK Build Configuration

```bash
dotnet publish -f net10.0-android -c Release \
  -p:AndroidPackageFormat=apk \
  -p:TrimPackageSize=true
```

- **Release config**: Creates smaller, optimized APK
- **AndroidPackageFormat=apk**: Generate APK (not AAB for app stores)
- **TrimPackageSize**: Removes unused code (optional but speeds up install)
- Output path: `bin/Release/net10.0-android/publish/*.apk`

### Maestro Test Execution

```bash
maestro test maestro-tests/ -p android
```

- `maestro-tests/`: Directory containing all YAML test files
- `-p android`: Run on already-connected Android emulator
- Maestro automatically discovers `.yaml` files and runs them sequentially
- Each test must have `appId: com.companyname.tictoe.infinite` at the top

## Timeout Strategy

| Step | Timeout | Notes |
|------|---------|-------|
| Emulator boot | 10 min | Usually 2-3 min; 10 min is belt-and-suspenders |
| APK build | 15 min | Depends on project size; adjust if needed |
| Maestro tests | 30 min | Each test ~30-60s; depends on test complexity |
| **Total job** | 60 min | Sum of all steps with overhead |

Adjust timeouts based on your specific test suite. The total workflow typically completes in **12-20 minutes**.

## Debugging Failing Tests

### 1. **Check Maestro Logs**

Enable verbose logging in the workflow:

```yaml
env:
  MAESTRO_LOG_LEVEL=debug
```

Or locally:

```bash
export MAESTRO_LOG_LEVEL=debug
maestro test maestro-tests/
```

### 2. **Run Locally First**

Before pushing, verify tests work on your machine:

```bash
# Start emulator locally (Android Studio or command line)
${ANDROID_HOME}/emulator/emulator -avd Pixel_API_34

# In separate terminal, build and test
cd src/TicToe.Infinite
dotnet publish -f net10.0-android -c Release
adb install -r bin/Release/net10.0-android/publish/*.apk

# From maestro-tests directory
cd ../../maestro-tests
maestro test ./launch.yaml -p android
```

### 3. **Common Failure Scenarios**

**Problem**: "App not installed" error

```
Reason: APK path changed or app ID doesn't match
Solution: 
1. Verify app ID in launch.yaml matches build output
2. Check APK path: find src/TicToe.Infinite/bin -name "*.apk"
3. Verify adb install succeeded: adb shell pm list packages | grep tictoe
```

**Problem**: "Element not found" in Maestro test

```
Reason: App hasn't finished loading or UI element IDs changed
Solution:
1. Add wait commands in test YAML: wait: { seconds: 2 }
2. Use text matching instead of IDs if elements are missing
3. Run test locally with -v flag: maestro test test.yaml -p android -v
4. Check app.xaml.cs to verify AutomationProperties.Id values match test IDs
```

**Problem**: Emulator timeout (won't boot)

```
Reason: CI runner under heavy load, HW acceleration unavailable
Solution:
1. Increase timeout from 10 to 15 minutes (emergency)
2. Reduce emulator memory from 2048 to 1024 MB
3. Switch to API 33 if API 34 is unavailable
4. Check runner status: sometimes GitHub actions runners are congested
```

**Problem**: APK build fails

```
Reason: Missing dependencies, SDK version mismatch
Solution:
1. Verify you can build locally: dotnet publish -f net10.0-android -c Release
2. Check .csproj TargetFramework version matches workflow (net10.0-android)
3. Look for NuGet restore errors in logs
4. Increase APK build timeout
```

### 4. **Download Failure Artifacts**

In GitHub Actions, when a test fails:

1. Go to the failed workflow run
2. Scroll to "Artifacts" section at bottom
3. Download `maestro-failure-logs`
4. Examine `maestro-failure-report.txt` and `emulator.log`

The report includes:
- Full Android logcat output
- Running process list
- Installed app packages

## Performance Optimization Tips

### **Tip 1: SDK Caching**

If SDK cache is NOT working, try clearing it:

```yaml
- uses: actions/cache@v4
  with:
    path: /usr/local/lib/android/sdk
    key: android-sdk-${{ runner.os }}-v2  # Increment version to bust cache
```

### **Tip 2: Parallel Test Execution**

Currently tests run sequentially. For faster feedback on large suites:

```yaml
strategy:
  matrix:
    test: [launch, gameplay_basic, horizontal_win, vertical_win, diagonal_win]
steps:
  - name: Run test
    run: maestro test maestro-tests/${{ matrix.test }}.yaml -p android
```

**Trade-off**: Requires multiple emulators (more resources), but can reduce runtime from 10 min to 3 min.

### **Tip 3: Reduce APK Size**

Enable code trimming and minimize resources:

```xml
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishReadyToRun>false</PublishReadyToRun>  <!-- Can speed up build -->
  <AndroidEnableProfileGuidedOptimization>true</AndroidEnableProfileGuidedOptimization>
</PropertyGroup>
```

### **Tip 4: Pre-warm Dependencies**

Add `dotnet restore` step to leverage NuGet cache:

```yaml
- name: Restore NuGet packages
  run: dotnet restore src/
```

## Maestro Test File Format Reference

Each test YAML must start with the app ID:

```yaml
# maestro-tests/my_test.yaml

appId: com.companyname.tictoe.infinite
---

# Commands:
- launchApp

- tap:
    id: "button_id"  # Or use: text: "Button Text"

- assertVisible:
    text: "Expected text on screen"

- assertNotVisible:
    text: "Text that shouldn't be there"

- wait:
    seconds: 2

- scroll:
    direction: down
```

Full docs: https://maestro.mobile/api/commands

## Cost Analysis

| Component | Cost | Details |
|-----------|------|---------|
| **GitHub Actions** | Free (public repo) | 2,000 min/month free on public repos |
| **Ubuntu Runners** | Free | Shared infrastructure |
| **Android SDK** | Free | Open source; no account needed |
| **Maestro CLI** | Free | Open source; no account needed |
| **Maestro Cloud** | ❌ Not used | This workflow runs locally only |
| **Total** | **$0** | Completely free for public repos |

For private repos, GitHub provides 3,000 min/month free per user account; usage beyond that is ~$0.24/min.

## Continuous Integration Best Practices

1. **Run on all PRs**: Catches regressions before merge
2. **Run on push**: Validates main branch health
3. **Fail fast**: Stop on first test failure (current behavior)
4. **Artifact retention**: Keep logs 90 days for post-mortems
5. **Status checks**: Require workflow to pass before merge

To enable status checks:

1. Go to repo **Settings** → **Branches**
2. Under branch protection rule, enable "Require status checks to pass"
3. Select "maestro-tests" workflow

## Troubleshooting the Workflow Itself

### Workflow not triggering?

Verify `.github/workflows/maestro-tests.yml` is on the correct branch in GitHub:

```bash
git add .github/workflows/maestro-tests.yml
git commit -m "Add Maestro CI workflow"
git push origin main
```

### Workflow file syntax errors?

GitHub validates workflows on push. Check:
- Go to **Actions** tab
- Look for red X next to workflow name
- Click workflow to see syntax error

Or validate locally:

```bash
# Using yamllint
yamllint .github/workflows/maestro-tests.yml
```

## Next Steps

1. **Push this workflow** to your repository
2. **Test locally first** to ensure tests pass
3. **Monitor the first workflow run** in GitHub Actions UI
4. **Adjust timeouts** based on actual runtimes
5. **Set up branch protection** to require workflow to pass

## Additional Resources

- [Maestro Mobile Testing Docs](https://maestro.mobile/)
- [Android Emulator CI Setup](https://developer.android.com/studio/run/emulator-acceleration#accel-vm)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [MAUI Build Configuration](https://learn.microsoft.com/en-us/dotnet/maui/deployment/android)
