# Spell It Out

Gesture-based spell casting prototype built in Unity.  
Originally developed as a CS388 final project, this public version focuses on **touch and mouse-based gesture recognition**.

## Overview
Spell It Out is a small dungeon-style prototype where players defeat enemies by drawing spell gestures.

At the start of the game, the player can freely move using WASD and explore a **procedurally generated map**.
Each room contains a single enemy, and the player **must defeat the enemy to unlock the next room**.
 
Clearing rooms by defeating their enemies completes the game.

## Gameplay
- Explore a PCG-generated dungeon using keyboard movement
- Enter rooms containing enemies
- Draw spell gestures to attack enemies
- Progress to the next room only after the current enemy is defeated

## Features
- 2D stroke capture using mouse or touch input
- Stroke normalization and resampling
- 8-direction quantization (line-to-8dir)
- Template-based gesture matching
- Debug visualization of input strokes and matched shapes

## Controls
**PC (Mouse / Keyboard)**
- WASD: Move
- Left Mouse Button: Draw gestures and interact with UI

## Build & Run
1. Open the project in Unity.
2. Load the main scene.
3. Press Play to test gesture recognition in-editor.

> This repository does **not** include Nintendo Switch SDKs or plugins.

## Team
- Nathaniel Thoma  
- Hyeonjoon (Joon) Nam  
- Haneul Lee  
- Sam Friedman  

## License
MIT License
