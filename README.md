# Spell It Out

Gesture-based spell casting prototype built in Unity.  
Originally developed as a CS388 final project for **Nintendo Switch (Dev Kit)**, this repository is a **public PC/Web version** rebuilt for **touch & mouse-based gesture recognition**.

## Play (WebGL)
https://hyeonjoon-nam.itch.io/spell-it-out

## What’s in this repo (Public Version)
- ✅ WebGL build deployed to itch.io
- ✅ Runs on Windows (tested)
- ❌ Nintendo Switch SDKs / plugins / platform-specific code are **not included**

## Overview
Spell It Out is a small dungeon-style prototype where you defeat enemies by **drawing spell gestures**.

- Explore a procedurally generated dungeon
- Each room contains a single enemy
- Defeat the enemy to unlock the next room
- Clear rooms to finish the run

## Gameplay
- Move through a PCG-generated dungeon using keyboard movement
- Enter rooms containing enemies
- Draw spell gestures to attack enemies
- Progress only after the current enemy is defeated

## Features
- 2D stroke capture using mouse or touch input
- Stroke normalization and resampling
- 8-direction quantization (line-to-8dir)
- Template-based gesture matching
- Debug visualization of input strokes and matched shapes

## Controls
**PC (Mouse / Keyboard)**
- WASD: Move
- Left Mouse Button: Draw gestures / interact with UI

## Build & Run (Unity)
- Unity Version: **6000.0.56f1**

1. Open the project in Unity.
2. Load the main scene.
3. Press Play to test gameplay + gesture recognition in-editor.

## CI / Deployment
This repo includes a GitHub Actions pipeline that builds **WebGL** and deploys the artifact to itch.io.

## Team
- Nathaniel Thoma  
- Hyeonjoon (Joon) Nam  
- Haneul Lee  
- Sam Friedman  

## License
MIT License
