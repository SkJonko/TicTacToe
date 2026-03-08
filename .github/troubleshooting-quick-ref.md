# Maestro CI/CD Troubleshooting Quick Reference

## Pre-Flight Checklist

- [ ] Verify app ID in `maestro-tests/launch.yaml` matches `TicToe.Infinite.csproj`:
  - Run: `grep -r "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj`
  - Should match: `appId: com.companyname.tictoe.infinite` in YAML files

- [ ] Test local build works:
  ```bash
  cd src/TicToe.Infinite
  dotnet publish -f net10.0-android -c Release
  ```

- [ ] Test Maestro CLI locally:
  ```bash
  maestro test maestro-tests/launch.yaml
  ```

- [ ] Push workflow file to repo:
  ```bash
  git add .github/workflows/maestro-tests.yml
  git push
  ```

---

## Issue: "Workflow not showing up in Actions tab"

**Cause**: Workflow file not pushed to GitHub or on wrong branch

**Solution**:
```bash
# Verify file exists locally
ls -la .github/workflows/maestro-tests.yml

# Push to GitHub
git add .github/workflows/
git commit -m "Add Maestro workflow"
git push origin main
```

Check GitHub repo → **Actions** tab (may take 30 seconds to appear).

---

## Issue: "Workflow fails with YAML syntax error"

**Cause**: Indentation or quotation issues in YAML

**Solution**:

1. Check error in GitHub Actions UI (click workflow → red X icon)
2. Validate YAML locally:
   ```bash
   # Using an online validator
   # Paste .github/workflows/maestro-tests.yml into: https://www.yamllint.com/
   ```
3. Common errors:
   - Tabs instead of spaces (use 2 spaces in YAML)
   - Missing colons after keys
   - Unclosed quotes in strings

---

## Issue: "Java version is not 17"

**Error in logs**: `ERROR: ANDROID_HOME not set` or `Java 11 not compatible`

**Solution**:
```yaml
- name: Setup Java 17
  uses: actions/setup-java@v4
  with:
    java-version: '17'
    distribution: 'temurin'
```

Verify in workflow logs:
```bash
Running setup-java v4
Using Java version: 17
Location: /usr/lib/jvm/temurin-17-jdk-amd64
```

---

## Issue: "Emulator timeout after 10 minutes"

**Error**: `Waiting for emulator to boot` hangs, then timeout

**Cause**: 
- GitHub runner is overloaded
- Hardware acceleration not available
- Not enough memory

**Solution (in order of preference)**:

1. Reduce memory pressure:
   ```yaml
   -memory 1024 \  # Down from 2048
   -cores 1 \      # Down from 2
   ```

2. Increase timeout:
   ```yaml
   timeout-minutes: 15  # Up from 10
   ```

3. Use older API:
   ```yaml
   system-images;android-33;google_apis;x86_64  # Instead of 34
   ```

4. Switch to ARM emulation (slower):
   ```yaml
   system-images;android-34;google_apis;arm64-v8a  # Very slow, last resort
   ```

---

## Issue: "APK not found"

**Error**: 
```
ERROR: APK not found!
```

**Cause**: Build failed silently or path is wrong

**Solution**:

1. Check build output for errors:
   ```bash
   # Add to workflow to see full build log
   - name: List build outputs
     run: find src/TicToe.Infinite/bin -type f -name "*.apk" -ls
   ```

2. Verify TargetFramework is correct:
   ```xml
   <!-- In TicToe.Infinite.csproj -->
   <TargetFrameworks>net10.0-android;...</TargetFrameworks>
   ```

3. Check if build step succeeded:
   ```yaml
   - name: Build MAUI Android APK
     run: |
       cd src/TicToe.Infinite
       dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormat=apk
       echo "Exit code: $?"  # Debug line
   ```

---

## Issue: "APK install fails: signature mismatch"

**Error**:
```
adb: failed to install *.apk: Signature/Permission mismatch
```

**Cause**: Installing over existing app with different signature (debug vs release)

**Solution**:
```bash
# Add to workflow before install step
${ANDROID_HOME}/platform-tools/adb uninstall com.companyname.tictoe.infinite || true
```

Or in workflow:
```yaml
- name: Install APK on emulator
  run: |
    ${ANDROID_HOME}/platform-tools/adb uninstall com.companyname.tictoe.infinite || true
    APK_PATH=$(find ... -name "*.apk")
    ${ANDROID_HOME}/platform-tools/adb install "$APK_PATH"
```

---

## Issue: "Element not found" in Maestro test

**Error**:
```
AssertionError: Element not found: text="Expected text"
```

**Cause**:
1. App hasn't finished loading
2. UI element ID changed
3. Text matching is wrong

**Solution**:

1. Add wait to beginning of test:
   ```yaml
   - launchApp
   - wait:
       seconds: 3  # Give app time to load
   - assertVisible:
       text: "Player X's Turn!"
   ```

2. Check element IDs locally:
   ```bash
   # Connect emulator and run locally
   maestro test maestro-tests/launch.yaml -v  # Verbose mode
   ```

3. Use text instead of IDs if unreliable:
   ```yaml
   # Instead of:
   - tap:
       id: "MainPage_Border_FirstFirst"
   
   # Try:
   - tap:
       text: "1"  # Cell number or visible text
   ```

4. Verify XAML has `AutomationProperties.Id`:
   ```xml
   <!-- In MainPage.xaml -->
   <Border x:Name="FirstFirstBorder" 
           AutomationProperties.Id="MainPage_Border_FirstFirst" />
   ```

---

## Issue: "Maestro CLI not found"

**Error**:
```
maestro: command not found
```

**Cause**: Installation failed or PATH not updated

**Solution**:
```yaml
- name: Install Maestro CLI
  run: |
    curl -Ls "https://get.maestro.mobile/install.sh" | bash
    # Debug: verify installation
    ls -la $HOME/.maestro/bin/
    echo "PATH=$PATH"
    $HOME/.maestro/bin/maestro --version
```

Or check if download URL changed:
```bash
# Test locally
curl -Ls "https://get.maestro.mobile/install.sh" | bash
maestro --version
```

---

## Issue: "All tests pass locally but fail in GitHub Actions"

**Cause**: Environmental differences (emulator speed, timing, device config)

**Debug steps**:

1. Add generous waits in Maestro YAML:
   ```yaml
   - wait:
       seconds: 2
   - tap:
       id: "button"
   - wait:
       seconds: 2
   ```

2. Use text assertions (more stable than IDs):
   ```yaml
   - assertVisible:
       text: "Player X's Turn!"  # More reliable than ID
   ```

3. Download failure logs from Actions:
   - Go to failed workflow run
   - Scroll to Artifacts
   - Download `maestro-failure-logs`
   - Check `maestro-failure-report.txt`

4. Add extra diagnostics:
   ```yaml
   - name: Debug app state
     if: failure()
     run: |
       ${ANDROID_HOME}/platform-tools/adb shell dumpsys activity activities | grep -A 5 "tictoe\|Focus"
       ${ANDROID_HOME}/platform-tools/adb logcat -d "*:E" -o /tmp/errors.log
   ```

---

## Issue: "Runs slow (takes 20+ minutes)"

**Cause**: Caching not working, large APK, many tests

**Optimization**:

1. **Clear cache and rebuild** (fresh cache):
   ```bash
   # Delete the cache in GitHub
   # Go to repo → Actions → left sidebar → Caches
   # Click delete on "android-sdk-*" caches
   # Next run will populate fresh cache
   ```

2. **Reduce APK size**:
   ```xml
   <PropertyGroup>
     <PublishTrimmed>true</PublishTrimmed>
     <TrimPackageSize>true</TrimPackageSize>
   </PropertyGroup>
   ```

3. **Run tests in parallel** (advanced):
   ```yaml
   strategy:
     matrix:
       test: [launch, gameplay_basic, horizontal_win]
   ```

---

## Issue: "Out of memory" errors

**Error**: `Emulator exited unexpectedly` or `SIGKILL`

**Solution**:
```yaml
- name: Start Android Emulator
  run: |
    ${ANDROID_HOME}/emulator/emulator \
      -avd Pixel_API_34 \
      -memory 1024 \      # Reduce from 2048
      -cores 1 \          # Reduce from 2
      ...
```

---

## Getting Logs for Support

If you need help debugging, collect:

```bash
# 1. Workflow run URL
# Example: https://github.com/your-org/TicTacToe/actions/runs/12345

# 2. Specific error from logs (copy full error message)

# 3. Local reproduction
maestro test maestro-tests/ -p android
maestro --version
adb devices
${ANDROID_HOME}/emulator/emulator -help | grep memory

# 4. Upload artifacts
# (GitHub automatically saves them, downloadable from Actions UI)
```

---

## Key Files to Check

```
.github/
├── workflows/
│   └── maestro-tests.yml          ← The GitHub Actions workflow
├── maestro-pipeline-guide.md      ← Full documentation
└── troubleshooting-quick-ref.md   ← This file

maestro-tests/
├── launch.yaml                    ← All tests must have: appId: com.companyname.tictoe.infinite
├── gameplay_basic.yaml
└── *.yaml

src/
└── TicToe.Infinite/
    ├── TicToe.Infinite.csproj     ← Check: TargetFrameworks & ApplicationId
    ├── Properties/
    │   └── launchSettings.json
    └── Pages/
        └── MainPage.xaml          ← Check: AutomationProperties.Id values
```

---

## One-Minute Emergency Fixes

| Problem | Quick Fix |
|---------|-----------|
| Workflow not found | `git push .github/workflows/maestro-tests.yml` |
| YAML error | Check colons, quotes, indentation (2 spaces) |
| Emulator hangs | Increase timeout from 10 to 15 minutes |
| Element not found | Add `wait: { seconds: 3 }` before assertions |
| APK not found | Verify `.csproj` has `net10.0-android` TargetFramework |
| App won't install | Run: `adb uninstall com.companyname.tictoe.infinite` first |
| Maestro not found | Check install script URL, run `ls ~/.maestro/bin/` |
| Tests pass locally, fail in CI | Add larger waits: `wait: { seconds: 3 }` to YAML |

---

## When to Escalate

If still stuck:

1. **Check Maestro issues**: https://github.com/mobile-dev-inc/maestro/issues
2. **Check Android emulator CI setup**: https://developer.android.com/studio/run/emulator-acceleration
3. **Review GitHub Actions docs**: https://docs.github.com/en/actions/troubleshooting
4. **Download and share failure artifacts** with anyone helping you debug

---

## Success Indicators

✅ Workflow appears in **Actions** tab  
✅ Workflow runs on each push/PR  
✅ Emulator boots in 2-5 minutes  
✅ APK installs successfully  
✅ All tests pass in 5-10 minutes  
✅ Status checks prevent merge on failure  
✅ Artifacts collected on failure (helpful for debugging)
