# BecomeCart - A R.E.P.O. Mod

A BepInEx plugin for R.E.P.O. that allows you to transform into a cart.

## Installation

1. Install BepInEx 5.x into your R.E.P.O. game folder
2. Build this project or download a release
3. Copy the compiled DLL to your `BepInEx/plugins` folder
4. Launch the game

## Become A Cart!

This mod allows you to transform into a cart in the game:

- Press **F3** to swap your player model with a cart
  - Makes you look like a cart while retaining player controls
  - Automatically finds and uses available carts in the scene
  - Switches to third-person view with the camera positioned just above and behind the cart
  - Properly positions the cart model to match player movement
  - Player is made immune to damage while in cart form
  - Improved vehicle-like controls:
    - **Mouse**: Turn the cart left/right
    - **W/S**: Move forward/backward
    - **A/D**: Strafe left/right (sideways movement)

- Press **F4** to restore your normal player model
  - Removes the cart model and restores your regular appearance
  - Switches back to your original first-person view
  - Maintains your position and orientation
  - Restores normal damage vulnerability

- Press **F5** to toggle visibility of your player model while in cart form
  - Useful for debugging to see both your player and the cart simultaneously
  - Helps visualize how the cart movement works
  - Toggle between hidden (normal gameplay) and visible (debug mode)

## How to Use

1. In the game, when you're near a cart, press **F3**
2. The mod will swap your player model with a cart
3. Use mouse to steer, W/S to drive forward/backward, and A/D to strafe left/right
4. Press F4 any time you want to return to normal 