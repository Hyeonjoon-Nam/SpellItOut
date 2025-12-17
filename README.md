# Spell It Out

Unity gesture-based spell-casting prototype.  
Originally developed as a CS388 final project for **Nintendo Switch (Dev Kit)**, this repository is a **public PC/WebGL adaptation** focused on **mouse/touch gesture input**.

## Play (WebGL)
https://hyeonjoon-nam.itch.io/spell-it-out

## Public Version Notes
- ✅ WebGL build deployed to itch.io
- ✅ Runs on Windows (tested)
- ❌ Nintendo Switch SDKs / plugins / platform-specific code are **not included**

## Overview
Explore a procedurally generated dungeon and defeat enemies by **drawing spell gestures**.  
Each room contains a single enemy — defeat it to unlock the next room and continue.

## Features
- 2D stroke capture (mouse / touch)
- Stroke normalization + resampling
- 8-direction quantization (line-to-8dir)
- Template-based gesture matching
- Debug visualization of strokes and matched shapes

## Controls (PC)
- WASD: Move
- Left Mouse Button: Draw gestures / interact with UI

## Build & Run (Unity)
- Unity Version: **6000.0.56f1**

1. Open the project in Unity.
2. Load the main scene.
3. Press Play to test gameplay + gesture recognition in-editor.

## CI / Deployment
Includes a GitHub Actions pipeline that builds **WebGL** and deploys the artifact to itch.io.

## Team
- Nathaniel Thoma
- Hyeonjoon (Joon) Nam
- Haneul Lee
- Sam Friedman

## License
MIT License
