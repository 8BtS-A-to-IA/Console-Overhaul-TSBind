![console overhaul](https://github.com/8BitShadow/media-resources/blob/main/console%20overhaul.png?raw=true)
# Toggle/Simul-bind, a command binding tool.
## About
"TSBind" is a 'CC (console command)' mod which adds new commands that allow the user to bind and unbind commands to/from keybinds, a very simple--entirely CC--alternative system to the "SimpleMacros" mod.
<br><br>
TSBind both has its own internal keybind system and interacts directly with 'rewired', the in-built keybind system RoR2 uses, allowing the mod to reliably only bind already unbound keys. The system also has a check in place which will unbind all binds if bindings somehow desync (too many keybinds with too few commands or vice versa).
<br><br>
The mod is intended to be mainly used directly through the command console, however the mod uses CCs intentionally so that outside mods can easily--directly--interface with the mod without having to check if the mod is installed.

## Usage
A user can bind a key by doing `COSimulBind [key] [command]` in the console then press the key when the console is closed and the CC will automatically be sent.<br>
If the command is run multiple times with the same key, all commands defined when binding will run one after the other - all at once.
<br><br>
You can also use `CObind [key] [command]`--similarly to `COSimulBind` when used multiple times--to preform the same action but each press will switch between calling one command then the next in the order of when it was bound, for example running `CObind p 'timescale 0'` then `CObind p 'timescale 1'` will set the timescale of the game to 0 when the <kbd>p</kbd> key is pressed then back to 1 when it is pressed again - looping back to 0 when pressed again.
<br><br>
You can unbind the latest bind of the respective type by calling the command with no second parameter, for example: running `COBind P` will attempt to unbind the latest command bound to the <kbd>p</kbd> key.<br>
Alternatively you can use the `COUnbind` command with at least one argument to unbind a keybind at a specific index of the 'toggle' bind type, 2 arguments to unbind at an index of the specified bind type ('toggle' or 'simul') or 3 arguments (with the first argument being ignored) which will let you unbind the first found keybind defined in the 3rd argument.

## development
### How can I develop for this project?
After cloning the repository and ensuring you have any version of [VS 2017/2019](https://visualstudio.microsoft.com/) installed, you should be able to simply open the `.snl` file to open the project in VS.
<br><br>
Before posting a merge request, please ensure you've:
- Adequately checked for 'top level' bugs
- Provided enough commenting/sudo-code for other contributers to quickly understand the process (if necessary)

For the sake of documenting bugfixes, when posting a merge request, please ensure you detail any changes by:
- Describing what was changed (in the head)
- How the changes where made (in the 'extended description')
  - If the merge request only adds new code and does not edit any pre-existing code, feel free to only fill the head.

### How do I compile and run this?
There are no special steps to building and compiling the code, simply press 'run' in <abbr title="Visual Studio">VS</abbr>.<br>
If you do not have the [export helper](https://github.com/8BtS-A-to-IA/VS.DLL-export-helper) installed; simply press 'Ok' if an error appears saying "A project with an Output Type of Class Library cannot be started directly". Visual Studio will have the `.dll` file you need generated in `bin>Debug` for VS 2017 or `bin>Debug>netstandard2.0` for VS 2019, simply copy the `.dll` file into the BepInEx `plugins` folder and start RoR2.<br>
If you have the exporter helper tool setup correctly; after pressing 'run' in VS, simply start <abbr title="Risk of Rain 2">RoR2</abbr>.

### How can I help without any programming 'know-how'?
Simply install the mod/modpack from [the modpacks main page](https://github.com/8BtS-A-to-IA/Console-Overhaul) and play. If you encounter any issues make sure to log it and provide as much relevant detail as possible in the relevant mods' `issue` page--or the main page if you don't know which mod is the problem--after checking if the same issue has not already been encountered, you can use the formatting guide to help with this.<br>
Don't worry about if you predict the wrong mod as the cause, it's more important to just have the report out there.

## Changelog:
<details>
    <summary>V1.0.0 (unreleased):</summary>
  
  - none yet!
</details>
