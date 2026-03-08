# Maestro UI Test Suite - TicToe Infinite

This directory contains automated UI tests for the TicToe Infinite application using Maestro. The tests validate core game functionality, win conditions, edge cases, and special features.

## Test Overview

### Test Files and Coverage

#### 1. **launch.yaml** - Application Launch
- **Purpose**: Validates that the application launches successfully and the main game screen is ready
- **Coverage**:
  - App starts without errors
  - Main game screen is visible
  - Turn indicator shows "Player X's Turn!"
  - Reset Game button is visible
  - AI toggle button is visible
  - All 9 board cells are rendered and empty
  - No win or game-over messages are displayed initially

#### 2. **gameplay_basic.yaml** - Basic Turn Alternation
- **Purpose**: Validates core gameplay mechanics and player turn alternation
- **Coverage**:
  - Players can alternate turns correctly
  - Marks are placed in the correct cells
  - Turn indicator updates after each valid move
  - Tapping occupied cells is properly ignored
  - Game state remains consistent when invalid moves are attempted
  - Players can continue playing after an invalid move attempt

#### 3. **horizontal_win.yaml** - Horizontal Win Condition
- **Purpose**: Validates win detection for horizontal lines
- **Coverage**:
  - Three consecutive marks in a horizontal row (top, middle, or bottom) result in a win
  - Win message displays the correct winner
  - Game properly detects and honors the win condition
  - Example: X wins with marks at positions (0,0), (0,1), (0,2)

#### 4. **vertical_win.yaml** - Vertical Win Condition
- **Purpose**: Validates win detection for vertical lines
- **Coverage**:
  - Three consecutive marks in a vertical column (left, middle, or right) result in a win
  - Win message displays the correct winner
  - Game properly detects and honors the win condition
  - Turn indicator is no longer displayed when game is won
  - Example: X wins with marks at positions (0,0), (1,0), (2,0)

#### 5. **diagonal_win.yaml** - Diagonal Win Conditions
- **Purpose**: Validates win detection for both diagonal directions
- **Coverage**:
  - **Main Diagonal**: Three marks from top-left to bottom-right result in a win
    - Positions: (0,0), (1,1), (2,2)
  - **Anti-Diagonal**: Three marks from top-right to bottom-left result in a win
    - Positions: (0,2), (1,1), (2,0)
  - Both conditions are tested in a single test file
  - Win messages correctly identify the winner
  - Game resets between diagonal tests

#### 6. **reset_game.yaml** - Reset Functionality
- **Purpose**: Validates that the Reset Game button correctly clears the board
- **Coverage**:
  - Reset button clears all marks from the board
  - Game state returns to initial state after reset
  - Turn indicator resets to "Player X's Turn!"
  - Players can continue playing normally after reset
  - Multiple moves are possible post-reset

#### 7. **reset_after_win.yaml** - Reset After Win
- **Purpose**: Validates that reset functionality works correctly after a player wins
- **Coverage**:
  - Reset button works when a player has won the game
  - Win message disappears after reset
  - Turn indicator resets to "Player X's Turn!"
  - Game can be played again after winning and resetting
  - Previously won game state is completely cleared

#### 8. **edge_case_occupied_cells.yaml** - Occupied Cell Handling
- **Purpose**: Validates that clicking occupied cells is properly ignored
- **Coverage**:
  - Attempts to place a mark on an opponent's cell are ignored
  - Attempts to place a mark on your own cell are ignored
  - Game turn does not change when tapping an occupied cell
  - Multiple consecutive invalid attempts are handled properly
  - Valid moves continue to work after invalid attempts
  - Game state remains consistent throughout invalid attempts

#### 9. **ai_mode_basic.yaml** - AI Opponent Mode
- **Purpose**: Validates AI toggle and AI gameplay functionality
- **Coverage**:
  - AI toggle button exists and is labeled "Enable AI (O)"
  - AI mode can be enabled
  - When AI is enabled, the computer automatically plays as Player O
  - After the player (X) makes a move, the AI responds automatically
  - Turn indicator returns to "Player X's Turn!" after AI move
  - AI mode can be disabled
  - When AI is disabled, human vs. human mode resumes
  - Game behavior changes based on AI mode state

#### 10. **mark_removal_fifo.yaml** - Three-Mark Limit & FIFO Rule
- **Purpose**: Validates the FIFO (First-In-First-Out) mark removal rule
- **Coverage**:
  - Each player can have at most 3 active marks on the board simultaneously
  - When a 4th mark is placed, the oldest mark is automatically removed
  - The removal follows FIFO (First-In-First-Out) principle
  - The oldest mark "breathes" as a visual warning before removal
  - Game continues properly after mark removal
  - Players can continue placing marks even after reaching the 3-mark limit
  - The 4th, 5th, and subsequent moves all trigger FIFO removal correctly

#### 11. **win_after_mark_removal.yaml** - Win Detection with FIFO Removal
- **Purpose**: Validates that win detection works correctly even after marks have been removed by the FIFO rule
- **Coverage**:
  - Win conditions are properly detected with remaining marks after FIFO removal
  - Complex game scenarios with multiple mark removals are handled correctly
  - Removing old marks doesn't prevent new wins from being detected
  - The game accurately tracks which marks constitute a winning line
  - Example scenario: Vertical line is formed after previous marks have been removed due to FIFO

## Test Execution Environment

- **Platform**: Android
- **Test Framework**: Maestro
- **Test Format**: YAML-based declarative tests
- **Application ID**: com.companyname.tictoe.infinite

## Key Testing Areas

### Core Gameplay
- ✅ Application initialization and launch
- ✅ Turn-based gameplay mechanics
- ✅ Player alternation
- ✅ Board state management

### Win Conditions
- ✅ Horizontal wins (3 rows)
- ✅ Vertical wins (3 columns)
- ✅ Diagonal wins (main and anti-diagonal)

### Game Features
- ✅ Reset/New Game functionality
- ✅ AI opponent mode
- ✅ Mark limit with FIFO removal
- ✅ Occupied cell validation

### Edge Cases & Robustness
- ✅ Invalid move handling (occupied cells)
- ✅ Game state consistency after errors
- ✅ Win detection after mark removal
- ✅ Reset after winning

## Test Dependencies

Tests assume:
- The application is properly installed on the Android device/emulator
- The correct app ID (`com.companyname.tictoe.infinite`) is used
- UI elements have the correct automation IDs (as defined in the XAML code)
- The Maestro CLI is properly installed and configured

## Notes

- Tests are independent and can be run in any order
- Each test file resets the application state at the beginning
- Tests use element IDs for reliable element identification
- Some tests (like `diagonal_win.yaml`) include multiple test scenarios within a single file
