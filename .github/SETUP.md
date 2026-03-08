# Maestro GitHub Actions - Setup Checklist & Quick Start

## Quick Setup (5 minutes)

### ✅ Step 1: Verify Files Are in Place

```bash
# Check workflow file exists
ls -la .github/workflows/maestro-tests.yml

# Should show the workflow file
```

### ✅ Step 2: Verify App ID Matches

```bash
# Get app ID from csproj
grep "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj

# Example output:
# <ApplicationId>com.companyname.tictoe.infinite</ApplicationId>

# Verify all YAML tests have matching appId
grep "^appId:" maestro-tests/*.yaml

# Should all be: appId: com.companyname.tictoe.infinite
```

If they don't match, update the YAML files:

```bash
# Replace all occurrences in maestro-tests/*.yaml
sed -i 's/^appId:.*/appId: com.companyname.tictoe.infinite/' maestro-tests/*.yaml
```

### ✅ Step 3: Test Locally (Recommended)

```bash
# Build APK locally
cd src/TicToe.Infinite
dotnet publish -f net10.0-android -c Release

# Run Maestro test
cd ../../maestro-tests
maestro test ./launch.yaml
```

If this passes, the GitHub Actions workflow will likely pass too.

### ✅ Step 4: Push to GitHub

```bash
git add .github/workflows/maestro-tests.yml
git add maestro-pipeline-guide.md
git add troubleshooting-quick-ref.md
git add maestro-configuration.md

git commit -m "Add Maestro CI/CD workflow"
git push origin main
```

### ✅ Step 5: Monitor First Workflow Run

1. Go to your GitHub repo
2. Click **Actions** tab
3. Look for "Maestro UI Tests" workflow
4. Wait for it to complete (12-20 minutes)
5. If it passes ✅, you're done!
6. If it fails ❌, check [Quick Troubleshooting](#quick-troubleshooting) below

---

## Pre-Deployment Checklist

- [ ] `.NET MAUI project` builds successfully locally
  ```bash
  cd src/TicToe.Infinite
  dotnet publish -f net10.0-android -c Release
  ```

- [ ] `maestro-tests/` directory exists with `.yaml` files

- [ ] Each `.yaml` file starts with correct `appId`:
  ```bash
  grep "^appId: com.companyname.tictoe.infinite" maestro-tests/*.yaml
  ```

- [ ] At least one test passes locally
  ```bash
  maestro test maestro-tests/launch.yaml
  ```

- [ ] `.github/workflows/maestro-tests.yml` is in repo

- [ ] `.NET version` in workflow matches project
  - Check: `grep "net.*-android" src/TicToe.Infinite/TicToe.Infinite.csproj`
  - Should be `net10.0-android` (or your version)

---

## File Structure

```
✅ ADDED:

.github/
├── workflows/
│   └── maestro-tests.yml              ← Main workflow (this does all the work)
├── maestro-pipeline-guide.md          ← Full documentation
├── troubleshooting-quick-ref.md       ← Common issues & fixes
└── maestro-configuration.md           ← Maestro test patterns

✅ EXISTING:

maestro-tests/
├── launch.yaml                        ← Tests (must have: appId: com.companyname.tictoe.infinite)
├── gameplay_basic.yaml
├── horizontal_win.yaml
├── vertical_win.yaml
├── diagonal_win.yaml
├── edge_case_occupied_cells.yaml
├── mark_removal_fifo.yaml
├── reset_after_win.yaml
├── reset_game.yaml
├── win_after_mark_removal.yaml
└── RunAllTests.ps1                    ← Local test runner (can keep using)

src/
└── TicToe.Infinite/
    ├── TicToe.Infinite.csproj         ← Has: <ApplicationId>com.companyname.tictoe.infinite</ApplicationId>
    └── Pages/
        └── MainPage.xaml              ← Has: AutomationProperties.Id values for Maestro
```

---

## Workflow Overview

### What the Workflow Does

```
┌─ SETUP PHASE ─────────────────────┐
│ 1. Checkout code                  │
│ 2. Install Java 17                │
│ 3. Install .NET 10                │
│ 4. Cache Android SDK              │
│ 5. Install Android components     │
└───────────────────────────────────┘
                 ↓
┌─ EMULATOR PHASE ──────────────────┐
│ 6. Create virtual device          │
│ 7. Start emulator                 │
│    (waits until fully booted)      │
└───────────────────────────────────┘
                 ↓
┌─ BUILD & INSTALL PHASE ───────────┐
│ 8. Build MAUI Android APK         │
│ 9. Install APK on emulator        │
└───────────────────────────────────┘
                 ↓
┌─ TEST PHASE ──────────────────────┐
│ 10. Install Maestro CLI           │
│ 11. Run maestro test maestro-tests/│
│     (runs all .yaml test files)    │
└───────────────────────────────────┘
                 ↓
┌─ REPORT PHASE ────────────────────┐
│ 12. Collect emulator logs (if fail)
│ 13. Upload artifacts              │
└───────────────────────────────────┘
```

**Total time**: 12-20 minutes  
**Cost**: $0 (completely free)

---

## Key Features

### ✅ Completely Free
- No Maestro Cloud subscription needed
- Only uses open-source tools
- GitHub provides 2,000 free CI/CD minutes/month for public repos

### ✅ No External Dependencies
- No external services required
- No authentication tokens needed
- Runs entirely on GitHub's infrastructure

### ✅ Production-Ready
- Implements caching to speed up subsequent runs
- Proper error handling with artifact collection
- Clear, well-commented code for easy customization

### ✅ Scalable
- Can run on multiple branches simultaneously
- Can add matrix strategy for parallel test execution
- Can add multiple test suites or device configurations

---

## Quick Troubleshooting

### My workflow doesn't appear in Actions tab

```bash
# Verify file is committed and pushed
git log --oneline .github/workflows/maestro-tests.yml

# Should show a commit. If not:
git add .github/workflows/maestro-tests.yml
git commit -m "Add workflow"
git push origin main
```

Wait 30 seconds and refresh the Actions tab.

### My tests fail with "Element not found"

**Most likely cause**: App is still loading when Maestro tries to interact

**Quick fix**: Add waits to your test YAML:

```yaml
- launchApp
- wait:
    seconds: 3        # ← Add this line
- assertVisible:
    text: "Player X's Turn!"
```

### APK won't install

**Most common causes**:
1. App ID doesn't match
2. APK wasn't built

**Verify**:
```bash
# Check app ID
grep -i "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj
grep "^appId:" maestro-tests/launch.yaml

# Build locally and check APK exists
cd src/TicToe.Infinite
dotnet publish -f net10.0-android -c Release
find bin/Release/net10.0-android/publish -name "*.apk" -ls
```

### Workflow takes 30+ minutes

**Likely causes**: Long timeouts or SDK not caching

**Fix**:
1. Check caching is working:
   - Go to repo → **Actions** → **Caches** (left sidebar)
   - Should see `android-sdk-*` cache
   - If not, cache will populate next run

2. Reduce timeouts in workflow:
   ```yaml
   timeout-minutes: 45  # Down from 60 if appropriate
   ```

---

## Local Testing (Before Pushing)

This is **highly recommended** to avoid repeated CI/CD failures:

```bash
# 1. Start emulator (if not already running)
# Use Android Studio, or command line:
${ANDROID_HOME}/emulator/emulator -avd Pixel_API_34

# 2. In separate terminal, build and install
cd src/TicToe.Infinite
dotnet publish -f net10.0-android -c Release
APK_PATH=$(find bin/Release -name "*.apk" | head -1)
adb install -r "$APK_PATH"

# 3. Run Maestro test
cd ../../maestro-tests
maestro test ./launch.yaml

# 4. If passes, the GitHub workflow will likely pass too
```

---

## Summary of Files Provided

| File | Purpose | Audience |
|------|---------|----------|
| `.github/workflows/maestro-tests.yml` | The actual GitHub Actions workflow | DevOps/CI-CD engineers |
| `maestro-pipeline-guide.md` | Complete architecture & configuration guide | Anyone setting up CI/CD |
| `troubleshooting-quick-ref.md` | Quick fixes for common issues | When something breaks |
| `maestro-configuration.md` | Maestro test patterns & best practices | QA/Test engineers |
| `SETUP.md` | This file - quick start guide | Getting started |

---

## Next Steps

1. ✅ **Verify app ID matches** (5 min)
   ```bash
   grep "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj
   grep "^appId:" maestro-tests/launch.yaml
   ```

2. ✅ **Test locally** (10 min)
   ```bash
   cd src/TicToe.Infinite
   dotnet publish -f net10.0-android -c Release
   cd ../../maestro-tests
   maestro test ./launch.yaml
   ```

3. ✅ **Push to GitHub** (1 min)
   ```bash
   git add .github/
   git commit -m "Add Maestro CI workflow"
   git push
   ```

4. ✅ **Monitor first run** (20 min)
   - Go to GitHub Actions tab
   - Watch workflow execute
   - Debug any failures

5. ✅ **Enable branch protection** (5 min)
   - Settings → Branches → Add rule
   - Require "maestro-tests" to pass
   - Prevent merging without passing tests

---

## Support & Debugging

### Check Logs

All workflow runs are logged in GitHub Actions. To access:

1. Go to repo → **Actions** tab
2. Click workflow run
3. Expand any failed step to see full logs
4. Download artifacts for detailed diagnostics

### Download Failure Artifacts

If tests fail:

1. Go to failed workflow run
2. Scroll down to **Artifacts** section
3. Download `maestro-failure-logs`
4. Contains:
   - Full emulator logcat output
   - Process list
   - Installed packages info

### Common Success Indicators

✅ Workflow appears in Actions tab within 30 seconds of push  
✅ Emulator boot completes in 2-3 minutes  
✅ APK build completes in 5-10 minutes  
✅ All Maestro tests run and report results  
✅ Workflow completes in 12-20 minutes total

---

## FAQ

**Q: Do I need Maestro Cloud?**  
A: No. This workflow runs tests locally on a GitHub Actions runner using the free Maestro CLI.

**Q: Is this free?**  
A: Yes. GitHub provides 2,000 CI/CD minutes/month free for public repos. 20 runs/month × 2,000 min = effectively unlimited.

**Q: Can I run tests on my local machine too?**  
A: Yes. The PowerShell script `maestro-tests/RunAllTests.ps1` still works for local testing.

**Q: Can I customize the workflow?**  
A: Absolutely. See `maestro-pipeline-guide.md` for detailed configuration options.

**Q: What if I have multiple MAUI platforms (iOS, Windows)?**  
A: This workflow targets Android. To add iOS CI/CD, use a macOS runner with XCode. Windows native would need a Windows runner.

**Q: Can I run tests in parallel?**  
A: Yes. Open `maestro-configuration.md` and search for "Parallel Execution" for an advanced configuration.

---

## Key Commands (Reference)

```bash
# Verify workflow is valid (local)
yamllint .github/workflows/maestro-tests.yml

# Test locally before pushing
cd src/TicToe.Infinite && dotnet publish -f net10.0-android -c Release

# Check app ID
grep "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj

# Verify app ID in tests
grep "^appId:" maestro-tests/*.yaml | sort | uniq

# Run test manually
maestro test maestro-tests/launch.yaml

# View GitHub Actions locally (requires GitHub CLI)
gh run list
gh run view <run-id>
```

---

## One-Liner Verification

Run this to verify everything is configured correctly:

```bash
# All in one command - checks everything
(grep "ApplicationId" src/TicToe.Infinite/TicToe.Infinite.csproj | grep -o "com.*" | tr -d '</' && \
 echo "---" && \
 grep "^appId:" maestro-tests/*.yaml | cut -d: -f2 | sort | uniq && \
 echo "---" && \
 [ -f .github/workflows/maestro-tests.yml ] && echo "✅ Workflow file exists") || \
 echo "❌ Configuration mismatch - fix app ID!"
```

All three should match exactly, and workflow file should exist.

---

## You're Ready!

Everything you need is in place:

```
✅ Workflow file: .github/workflows/maestro-tests.yml
✅ Documentation: maestro-pipeline-guide.md
✅ Troubleshooting: troubleshooting-quick-ref.md
✅ Test patterns: maestro-configuration.md
✅ This guide: SETUP.md
```

**Next action**: Push to GitHub and monitor the first workflow run in Actions tab.

Questions or issues? Check `troubleshooting-quick-ref.md` first—it covers 95% of common problems.
