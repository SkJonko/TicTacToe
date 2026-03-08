# Maestro Configuration & Best Practices

## Maestro Test Structure for GitHub Actions

### Required App ID Header

Every `.yaml` test file **must** start with the app ID:

```yaml
appId: com.companyname.tictoe.infinite
---

# Rest of test commands start here
- launchApp
```

**Important**: The `---` separator is required. Without it, Maestro won't parse the appId.

### Verify Your App ID

Check that your Maestro tests and CSPROJ match:

```bash
# 1. Get app ID from CSPROJ
grep "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj

# Sample output:
# <ApplicationId>com.companyname.tictoe.infinite</ApplicationId>

# 2. Verify all YAML files have matching appId
grep -h "^appId:" maestro-tests/*.yaml | sort | uniq

# Should output:
# appId: com.companyname.tictoe.infinite
```

---

## Maestro Command Reference for GitHub Actions

### Available Commands in Test YAML

#### **launchApp**
Starts the application on the running emulator.

```yaml
- launchApp
```

**When to use**: At the start of every test. Maestro will wait for app to be ready.

---

#### **tap** / **longPress**
Interacts with UI elements.

```yaml
# Tap by ID (recommended for CI)
- tap:
    id: "button_id"

# Tap by text (more stable across environments)
- tap:
    text: "Reset Game"

# Long press
- longPress:
    duration: 2
    id: "some_button"
```

**Best practice for CI**: Use text-based taps when possible (more stable than IDs).

---

#### **assertVisible** / **assertNotVisible**
Verifies UI elements exist or don't exist.

```yaml
# Assert text is visible
- assertVisible:
    text: "Player X's Turn!"

# Assert by ID
- assertVisible:
    id: "MainPage_Border_FirstFirst"

# Assert NOT visible (good for verifying game state)
- assertNotVisible:
    text: "Game Over"
```

**CI tip**: Assertions are your main validation. Fail fast if they don't pass.

---

#### **wait**
Pauses before next command (essential for CI reliability).

```yaml
# Wait 2 seconds
- wait:
    seconds: 2

# More reliable than:
- sleep: 2000  # (deprecated in newer Maestro versions)
```

**CI requirement**: Add waits between interactions to account for network latency and emulator slowness. Rule of thumb:
- After app launch: 2-3 seconds
- After taps: 1-2 seconds
- Before assertions: 1 second

---

#### **scroll**
Scrolls the screen.

```yaml
- scroll:
    direction: down
    amount: 3  # Number of swipes

- scroll:
    direction: up
    amount: 1
```

**Caution**: Limited use in small game UI like TicTacToe.

---

#### **input**
Sends keyboard text.

```yaml
- input:
    text: "Hello"

- input:
    key: enter
```

**Use case**: Text fields, search boxes (not needed for TicTacToe).

---

### Control Flow

#### **onFlow** (run commands if assertion passes)
```yaml
- tapOn:
    text: "Reset Game"
onFlow:
  - wait:
      seconds: 1
  - assertVisible:
      text: "Player X's Turn!"
```

#### **runFlow** (jump to named flow)
```yaml
- runFlow: setup

---
flow:
  name: setup
  commands:
    - launchApp
    - wait:
        seconds: 2
```

---

## Example: Complete Test File for GitHub Actions

```yaml
# maestro-tests/example_game_flow.yaml
appId: com.companyname.tictoe.infinite
---

# Test: Complete game flow with setup, play, and win check

# SETUP: Launch and verify initial state
- launchApp

- wait:
    seconds: 2

- assertVisible:
    text: "Player X's Turn!"

- assertVisible:
    id: "MainPage_Border_FirstFirst"

# PLAY: Make moves to win
- tap:
    id: "MainPage_Border_FirstFirst"

- wait:
    seconds: 1

- tap:
    id: "MainPage_Border_FirstSecond"

- wait:
    seconds: 1

- tap:
    id: "MainPage_Border_FirstThird"

# VERIFY: Check for win
- wait:
    seconds: 2

- assertVisible:
    text: "Player X wins!"
```

---

## CI/CD Differences: Workflow vs PowerShell Script

### PowerShell Script (Local Testing)

```powershell
# maestro-tests/RunAllTests.ps1
$yamlFiles = Get-ChildItem -Path . -Filter *.yaml
foreach ($file in $yamlFiles) {
    $output = & maestro test .\$($file.Name) -p android 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "$($file.Name) succeeded"
    } else {
        Write-Host "$($file.Name) failed"
    }
}
```

**Pros**: 
- Works on Windows
- Easy to customize per-machine

**Cons**: 
- Requires Maestro installed locally
- Emulator must be started manually
- No log artifacts

### GitHub Actions Workflow

```yaml
- name: Run Maestro Tests
  run: maestro test maestro-tests/ -p android
```

**Pros**:
- Fully automated (self-contained)
- Runs on every push/PR
- Logs stored as artifacts
- Cross-platform (Linux, Windows, macOS runners available)
- Free for public repos

**Cons**:
- 10-20 min per run (vs instant feedback locally)
- Slower emulator (shared resources)

| Aspect | PowerShell | GitHub Actions |
|--------|-----------|-----------------|
| **Setup** | Manual emulator start | Automated (Workflow does it) |
| **Speed** | Fast (local machine) | Slower (shared runner) |
| **Logs** | Console only | Artifacts (90 days storage) |
| **Triggers** | Manual only | Auto on push/PR |
| **Cost** | Free | Free (public repos) |
| **Scaling** | Single machine | Many parallel workflows |

---

## Maestro Execution Modes

### Running in GitHub Actions (Automated)

```bash
# Workflow command (finds all .yaml files)
maestro test maestro-tests/ -p android
```

Maestro will:
1. Discover all `.yaml` files in directory
2. Execute them sequentially
3. Exit with code 0 if all pass
4. Exit with code 1 if any fail
5. Print results to stdout (captured by GitHub Actions)

### Running Locally (Development)

```bash
# Single test
maestro test maestro-tests/launch.yaml -p android

# All tests
maestro test maestro-tests/ -p android

# Verbose output
maestro test maestro-tests/ -p android -v

# Debug mode
export MAESTRO_LOG_LEVEL=debug
maestro test maestro-tests/launch.yaml -p android
```

### Running Without Emulator

If testing against a physical device connected via `adb`:

```bash
maestro test maestro-tests/ -d  # -d for device instead of -p android
```

---

## Performance: Reducing Test Execution Time

### Original Test Suite
```
launch.yaml                  (10s)
gameplay_basic.yaml          (15s)
horizontal_win.yaml          (12s)
vertical_win.yaml            (12s)
diagonal_win.yaml            (15s)
edge_case_occupied_cells.yaml (20s)
---
Total Sequential:            ~84 seconds
```

### Option 1: Parallel Execution (Advanced)

Use GitHub Actions matrix strategy:

```yaml
strategy:
  matrix:
    test:
      - launch
      - gameplay_basic
      - horizontal_win
      - vertical_win
      - diagonal_win
steps:
  - name: Run test ${{ matrix.test }}
    run: maestro test maestro-tests/${{ matrix.test }}.yaml -p android
```

**Result**: Tests run in parallel (multiple emulators), ~20 seconds total  
**Trade-off**: More runner resources needed

### Option 2: Smoke Tests

Create a `smoke-tests.yaml` with just critical paths:

```yaml
appId: com.companyname.tictoe.infinite
---

# Quick smoke test (30 seconds instead of 84)
- launchApp
- wait: { seconds: 2 }
- assertVisible: { text: "Player X's Turn!" }

- tap: { id: "MainPage_Border_FirstFirst" }
- wait: { seconds: 1 }
- tap: { id: "MainPage_Border_FirstSecond" }
- wait: { seconds: 1 }
- tap: { id: "MainPage_Border_FirstThird" }

- wait: { seconds: 2 }
- assertVisible: { text: "Player X wins!" }
```

Use in workflow for quick feedback on every PR.

---

## Debugging: Using Maestro Locally

### Check if Emulator is Accessible

```bash
# List connected devices
adb devices

# Should show:
# List of attached devices
# localhost:5555         device
# emulator-5554          device
```

### Run Test with Maximum Verbosity

```bash
export MAESTRO_LOG_LEVEL=debug
maestro test maestro-tests/launch.yaml -p android -v
```

Output will include:
- Every tap/wait/assertion
- Timing of each step
- Device state at each checkpoint

### Inspect App State During Test

In a separate terminal while test runs:

```bash
# See current screen
adb shell screencap /tmp/screen.png
adb pull /tmp/screen.png

# Read UI hierarchy
adb shell uiautomator dump /tmp/hierarchy.xml
adb pull /tmp/hierarchy.xml

# Check app logs
adb logcat | grep -i tictoe
```

---

## Common Maestro YAML Mistakes in CI

### ❌ **Mistake 1: Missing appId**
```yaml
# WRONG - No appId header
---
- launchApp
```

**Fix**: Add appId at top
```yaml
appId: com.companyname.tictoe.infinite
---
- launchApp
```

### ❌ **Mistake 2: No waits between actions**
```yaml
# WRONG - App might not be ready
- launchApp
- assertVisible:
    text: "Player X's Turn!"
```

**Fix**: Add wait
```yaml
- launchApp
- wait:
    seconds: 2
- assertVisible:
    text: "Player X's Turn!"
```

### ❌ **Mistake 3: Over-reliance on element IDs**
```yaml
# RISKY - ID might change if UI refactored
- tap:
    id: "MainPage_Border_FirstFirst"
```

**Better**: Mix text and IDs
```yaml
# More stable
- tap:
    text: "Top-left cell"
    # OR
- tap:
    id: "MainPage_Border_FirstFirst"
```

### ❌ **Mistake 4: Inconsistent app ID**
```yaml
# launch.yaml
appId: com.companyname.tictoe.infinite

# But gameplay.yaml
appId: com.mycompany.tictoe
```

**Fix**: Use script to validate:
```bash
grep "^appId:" maestro-tests/*.yaml | cut -d: -f2 | sort | uniq -c
# All should have same ID
```

---

## Maestro Tips for MAUI Apps

### 1. **Access MAUI AutomationProperties**

In your XAML:

```xml
<Button 
    Text="Reset Game"
    AutomationProperties.Id="btn_reset_game" />
```

In Maestro test:

```yaml
- tap:
    id: "btn_reset_game"
```

### 2. **Handle Platform-Specific IDs**

XAML IDs are consistent across platforms, so the same test works on iOS/Android.

### 3. **Testing with Shell Routes**

If your app uses Shell navigation:

```csharp
// AppShell.xaml.cs
Routing.RegisterRoute("gamedetail", typeof(GameDetailPage));
```

Your Maestro tests should still work since Maestro interacts with rendered UI:

```yaml
- tap:
    text: "View Game"  # Button text
# Shell navigation happens automatically
- wait:
    seconds: 1
- assertVisible:
    text: "Game Details"
```

---

## Integration with CI/CD Pipeline

### GitHub Actions Order of Operations

1. ✅ Checkout code
2. ✅ Install dependencies (Java, .NET)
3. ✅ Cache Android SDK
4. ✅ Start emulator (5-120 seconds)
5. ✅ Build APK (5-10 minutes)
6. ✅ Install APK
7. ✅ Install Maestro CLI (30 seconds)
8. ✅ **Run Maestro tests** (5-30 minutes, depending on test suite)
9. ✅ Collect logs on failure

**Total time**: 12-20 minutes per run

### Branch Protection Rule (Best Practice)

To prevent merging without passing tests:

1. Go to repo **Settings** → **Branches**
2. Add branch protection rule for `main`
3. Check "Require status checks to pass before merging"
4. Select "maestro-tests" workflow
5. Check "Include administrators"

Now all merges require passing Maestro tests.

---

## Next Steps After Setup

1. **Push workflow to main branch**
   ```bash
   git add .github/workflows/maestro-tests.yml
   git commit -m "Add Maestro CI/CD workflow"
   git push
   ```

2. **Monitor first run** in GitHub Actions UI

3. **Adjust timeouts** based on actual execution time

4. **Add branch protection** to require tests pass

5. **Share results** with your team (link to Actions tab)

---

## References

- [Maestro Mobile Testing Docs](https://maestro.mobile/)
- [Maestro Command Reference](https://maestro.mobile/api/commands)
- [GitHub Actions Best Practices](https://docs.github.com/en/actions/guides/security-hardening-for-github-actions)
- [MAUI Automation & Testing](https://learn.microsoft.com/en-us/dotnet/maui/testing/overview)
