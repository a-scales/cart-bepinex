# BecomeCart - A R.E.P.O. Mod

A BepInEx plugin for modifying R.E.P.O. with a focus on cart-related modifications.

## Installation

1. Install BepInEx 5.x into your R.E.P.O. game folder
2. Build this project or download a release
3. Copy the compiled DLL to your `BepInEx/plugins` folder
4. Launch the game

## Cart Boosting Features

This mod allows you to boost carts to extreme speeds:

- **F9**: Detects carts in the scene and gives them a 5x velocity boost
- **F11**: Point at a cart and press this to register it for boosting
- **F12**: Super boost - gives any registered carts a 10x velocity boost

## Cart Physics Modifications

Once you've found a cart (using F9 or F11), you can toggle these special physics modes:

- **1**: Toggle Zero Gravity mode - the cart floats and ignores gravity
- **2**: Toggle Floaty Cart mode - the cart hovers slightly above the ground
- **3**: Toggle Super Grip mode - the cart won't slide around corners
- **4**: Toggle Slippery Cart mode - the cart slides like it's on ice

## Debugging Tools

This mod includes several debugging tools to help you understand the game's structure and behavior:

### Keyboard Shortcuts

- **F5**: Dumps the hierarchy of all active GameObjects in the current scene
- **F6**: Dumps all loaded assemblies to help find game classes
- **F7**: Attempts to find the player object and dumps its components, fields, and methods
- **F8**: Lists all UI canvases and buttons in the scene
- **F9**: Finds and tracks cart-like objects and boosts them (5x normal speed)
- **F10**: Stops tracking cart objects
- **F11**: Registers the cart component under your cursor for patching
- **F12**: SUPER BOOST registered carts (10x normal speed)
- **1-4**: Toggle various cart physics modifications

### How to Use

1. In the game, when you're near a cart, press **F9**
2. The mod will search for objects with "cart" in the name and boost them
3. Once registered, use number keys 1-4 to toggle different physics modes
4. Press F12 any time you want a super speed boost

### Example Effects

- Combine Zero Gravity (1) with a boost (F12) to make your cart fly off into the distance
- Use Super Grip (3) to handle tight corners at high speeds
- Enable Slippery Cart (4) for drift-like handling
- Try Floaty Cart (2) to hover slightly above rough terrain

### How to Debug

1. **Enable the BepInEx console**:
   - Open your BepInEx config file (`BepInEx/config/BepInEx.cfg`)
   - Set `Enabled = true` under `[Logging.Console]`

2. **Find BepInEx logs**:
   - Logs are stored in `BepInEx/LogOutput.log`
   - This file is updated in real-time as you play

3. **Use the debugging classes**:
   - `Plugin.DumpGameObject(gameObject)`: Print a GameObject's hierarchy
   - `Debugging.DumpMethods(object)`: Print all methods of an object
   - `Debugging.DumpFields(object)`: Print all fields of an object
   - `Debugging.FindAllInstances<ComponentType>()`: Find all instances of a component
   - `Debugging.DumpAssemblies()`: List all loaded assemblies

## Creating Custom Modifications

1. First, use the debugging tools to understand the game's structure
2. Examine the `CartPatch.cs` file to see how dynamic patching works
3. Once you've identified cart objects, the mod will try to patch them automatically
4. For custom modifications, edit the `CartUpdate_Prefix` method in `CartPatch.cs`

### Example Workflow

1. Press F9 in-game to find all cart-like objects
2. Drive/move the cart to see which objects are moving in the logs
3. Press F11 while looking at the cart to register it for patching
4. Check logs to see if the patching was successful
5. Edit the CartPatch.cs file to create your custom modifications

## Understanding Harmony Patching

Harmony allows you to:
- Run code before a method executes (Prefix)
- Run code after a method executes (Postfix)
- Replace a method entirely (Transpiler)

Check the `ExampleMods.cs.txt` file for examples of how to implement these different patch types.

## Tips for Creating Custom Mods

1. Start by finding the cart object and understanding its components
2. Look for methods related to movement, speed, or physics
3. Use F11 to register the cart for patching once you confirm it
4. Modify fields or override methods to change the cart's behavior
5. For more complex changes, create custom patches based on ExampleMods.cs.txt 