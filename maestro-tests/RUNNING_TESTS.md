# Running TicToe Infinite Maestro Tests

This guide explains how to run the test suite using the `RunAllTests.ps1` PowerShell script and what to expect.

## Prerequisites

Before running tests, ensure you have:

1. **Maestro CLI installed**
   - Download from: https://maestro.mobile.dev/
   - Verify installation: `maestro --version`

2. **Android Environment**
   - Android SDK installed and configured
   - Android emulator running OR physical Android device connected
   - Device has USB debugging enabled (for physical devices)
   - Verify connection: `adb devices`

3. **Application Built and Installed**
   - TicToe.Infinite app built for Android
   - App installed on the target device/emulator
   - Verify app installation: `adb shell pm list packages | grep tictoe`

4. **PowerShell**
   - Windows PowerShell 5.1 or higher
   - Or PowerShell Core (cross-platform)

## Running All Tests

### Step 1: Navigate to Test Directory

```powershell
cd C:\Users\{{USERNAME}}\source\repos\SkJonko\TicTacToe\maestro-tests
```

### Step 2: Execute the Test Script

```powershell
.\RunAllTests.ps1
```

### Step 3: Wait for Completion

The script will automatically run all `.yaml` test files in sequence. Each test takes approximately 30-60 seconds to complete.

## What Happens During Execution

### Script Behavior

The `RunAllTests.ps1` script performs the following:

1. **Discovery Phase**
   - Scans the current directory for all `.yaml` files
   - Counts total number of tests found

2. **Execution Phase**
   - Runs each test sequentially using: `maestro test .\filename.yaml -p android`
   - For each test, the script:
     - Displays the test name
     - Launches the Maestro test framework
     - Sends commands to the Android device
     - Captures the test output
     - Checks the exit code (0 = success, non-zero = failure)

3. **Results Phase**
   - Displays success/failure status for each test
   - Aggregates results into a summary
   - Lists any failed tests with error details

### Expected Success Output

When a test passes, you'll see:
```
Running test for launch.yaml...
launch.yaml succeeded
```

### Expected Failure Output

When a test fails, you'll see:
```
Running test for gameplay_basic.yaml...
gameplay_basic.yaml failed: [error message from Maestro]
```

## Expected Results

### Summary Report

At the end of execution, the script displays a summary:

```
Summary: 11/11 tests succeeded
```

Or if there are failures:

```
Summary: 9/11 tests succeeded
Failures:
  test1.yaml: Element not found with id: MainPage_Border_FirstFirst
  test2.yaml: App crashed during execution
```

### Test Execution Order

The tests run in the following order (alphabetically as enumerated by PowerShell):

1. `ai_mode_basic.yaml` - AI opponent functionality
2. `diagonal_win.yaml` - Diagonal win detection
3. `edge_case_occupied_cells.yaml` - Occupied cell validation
4. `gameplay_basic.yaml` - Basic turn mechanics
5. `horizontal_win.yaml` - Horizontal win detection
6. `launch.yaml` - Application startup
7. `mark_removal_fifo.yaml` - FIFO mark removal
8. `reset_after_win.yaml` - Reset after winning
9. `reset_game.yaml` - Reset functionality
10. `vertical_win.yaml` - Vertical win detection
11. `win_after_mark_removal.yaml` - Win detection with mark removal

**Note**: The order may vary slightly based on how the filesystem returns the directory listing.

## Interpreting Results

### All Tests Passed (Success Scenario)
```
Summary: 11/11 tests succeeded
```
✅ **Meaning**: All game functionality is working as expected. The application is ready for release or further development.

### Some Tests Failed (Failure Scenario)
```
Summary: 9/11 tests succeeded
Failures:
  gameplay_basic.yaml: Element not found with id: MainPage_Border_FirstFirst
  ai_mode_basic.yaml: App crashed during execution
```
❌ **Meaning**: There are issues that need investigation:
- **Element not found**: UI element IDs may have changed, or the app layout is different
- **App crashed**: There's a runtime error or exception in the application code
- **Assertion failed**: Game logic is not behaving as expected

## Troubleshooting

### Common Issues and Solutions

#### 1. "maestro: command not found"
**Problem**: Maestro CLI is not installed or not in PATH
**Solution**: 
- Install Maestro from https://maestro.mobile.dev/
- Add Maestro to system PATH environment variable
- Restart terminal/PowerShell

#### 2. "No connected devices"
**Problem**: Android emulator not running or device not connected
**Solution**:
- Start Android emulator: Open Android Studio → AVD Manager → Start emulator
- Or connect physical device with USB debugging enabled
- Verify: `adb devices`

#### 3. "App not installed"
**Problem**: TicToe.Infinite app is not installed on the device
**Solution**:
- Build the Android app in Visual Studio
- Deploy to target device
- Verify installation: `adb shell pm list packages | grep tictoe`

#### 4. "Element not found with id: MainPage_Border_..."
**Problem**: UI element IDs don't match or the app UI has changed
**Solution**:
- Verify that the application XAML still has these automation IDs defined
- If UI has changed, update the test YAML files with correct IDs
- Rebuild and redeploy the application

#### 5. "Test timeout"
**Problem**: Test takes longer than expected or hangs
**Solution**:
- Restart the Android device/emulator
- Check device performance and available disk space
- Try running a single test to isolate the issue: `maestro test .\launch.yaml -p android`

#### 6. "App crashes during test execution"
**Problem**: Application crashes when tests run
**Solution**:
- Check application logs: `adb logcat`
- Look for exceptions in Android Studio Device Monitor
- Fix the identified bug in the application code
- Rebuild and redeploy

## Running Individual Tests

To debug a specific failing test:

```powershell
maestro test .\launch.yaml -p android
```

This is useful for:
- Isolating test failures
- Debugging specific scenarios
- Testing new test files in development

## Running Tests with Custom Platform

If you want to test on a specific device or emulator profile:

```powershell
maestro test .\launch.yaml -p android --device-id emulator-5554
```

## Performance Expectations

| Metric | Expected Duration |
|--------|------------------|
| Single test | 30-60 seconds |
| All 11 tests | 6-11 minutes |
| Full suite with summary | 8-13 minutes |

Timing varies based on:
- Device performance
- Network latency (for cloud-based testing)
- App complexity

## Continuous Integration

To use this test suite in CI/CD pipelines:

```powershell
# Run tests and fail if any test fails
.\RunAllTests.ps1
if ($LASTEXITCODE -ne 0) {
    exit 1
}
```

## Viewing Test Logs

Maestro generates detailed logs. To view them:

```powershell
# Check Android device logs during test execution
adb logcat
```

## Next Steps After Testing

- **All passed**: Continue with feature development or release
- **Some failed**: 
  1. Review error messages and logs
  2. Fix application or test issues
  3. Rerun failed tests: `.\RunAllTests.ps1`
- **All failed**: 
  1. Verify Maestro installation
  2. Verify Android environment setup
  3. Verify app is properly installed
  4. Check Android device/emulator functionality

## Support Resources

- Maestro Documentation: https://maestro.mobile.dev/
- Android Developer Guide: https://developer.android.com/
- Maestro GitHub Issues: https://github.com/mobile-dev-inc/maestro/issues
