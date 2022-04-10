using BepInEx;
using R2API.Utils;
using Rewired;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bind
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.ML.ConCommandBinder", "Console Overhaul: Bind", "1.0")]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [BepInDependency("com.bepis.BAC", BepInDependency.DependencyFlags.SoftDependency)]

    public class ConCommandBinder : BaseUnityPlugin
    {
        /// <summary>
        /// Entry point used to access non-static methods.
        /// </summary>
        public static ConCommandBinder entryPoint = new ConCommandBinder();
        internal static ushort actionID = 598;
        internal List<string> registeredKeyBinds;
        internal List<List<string>> registeredConCommands;
        internal List<string> registeredSimulBinds;
        internal List<List<string>> registeredSimulCommands;
        internal KeyboardMap keyboardMap;
        internal KeyboardMap simulKeyboardMap;

        /// <summary>
        /// Index possition of the current toggle command being itterated through.
        /// </summary>
        private int ToggleIndex = 0;

        private ConCommandBinder()
        {}

        public void Awake()
        {
            CommandHelper.AddToConsoleWhenReady();
            Hooks.InitializeHooks();

        }

        public void OnDisable()
        {
            Hooks.DisableHooks();//drop hooks
            //KeyboardMap keyboardMap = LocalUserManager.GetFirstLocalUser().userProfile.keyboardMap;//(readability)
            if (keyboardMap.ElementMapsWithAction(actionID).Cast<ActionElementMap>().ToArray().Length > 0)
            {
                keyboardMap.DeleteElementMapsWithAction(actionID);
            }
        }

        /// <summary>
        /// Backend checker for "COBind" to cause the bound console command to be activated when the bound key is pressed.
        /// </summary>
        /// <param name="console"></param>
        /// <param name="current"></param>
        public void Bind(RoR2.Console console, Event current)
        {
            if (current is null) { return; }
            //try
            //{
            //    CC.print(current.character.ToString(), 5);//check if the key currently being press is not null nor the event.
            //}
            //catch { return; }

            if (current.isKey && registeredConCommands.Count != 0 && registeredKeyBinds.Count != 0 && !ConsoleWindow.instance)
            {//if the current even is a key press, there is at least one registered CC keybind and the console is not open
                CC.print("got a registered keybind and command", 4);
                for (int num = 0; num < registeredKeyBinds.Count; num++)
                {//foreach registered keyBind
                    CC.print("at i: " + num, 5);
                    if (Enum.TryParse<KeyCode>(registeredKeyBinds[num], out KeyCode keyCode))
                    {//if the bound key is a valid keybind
                        CC.print("got the keyBind: " + registeredKeyBinds[num], 4);
                        if (current.keyCode == keyCode)
                        {//and the key is currently being pressed (otherwise do nothing until the key is pressed)
                            if (registeredConCommands[num].Count > 1)
                            {//if there are multiple commands in the current commands list then we're working with toggle
                                console.SubmitCmd(null, registeredConCommands[num][ToggleIndex], true);//activate the console command
                                CC.print("Detected key code in toggle: " + current.keyCode, 5);
                                ToggleIndex++;
                                if (ToggleIndex > registeredConCommands[num].Count - 1)
                                {//
                                    ToggleIndex = 0;//cycle back so that this can be run infinetly ("walk around the world")
                                }
                                return;
                            }

                            //otherwise it's simul
                            foreach (string command in registeredConCommands[num])
                            {//foreach command bound to this keybind
                                console.SubmitCmd(null, command, true);//activate the console command
                                CC.print("Detected key code: " + current.keyCode, 5);
                            }
                        }
                    }
                    else
                    {
                        MonoBehaviour.print("Failed to get the key to bind.");
                    }
                }
            }
        }

        /// <summary>
        /// Backend checker for "COSimulBind" to cause the bound console command to be activated when the bound key is pressed.
        /// </summary>
        /// <param name="console"></param>
        /// <param name="current"></param>
        public void SimulBind(RoR2.Console console, Event current)
        {
            if (current is null) { return; }

            if (current.isKey && registeredSimulCommands.Count != 0 && registeredSimulBinds.Count != 0 && !ConsoleWindow.instance)
            {//if the current even is a key press, there is at least one registered CC keybind and the console is not open
                CC.print("got a registered keybind and command", 4);
                for (int num = 0; num < registeredSimulBinds.Count; num++)
                {//foreach registered keyBind
                    CC.print("at i: " + num, 5);
                    if (Enum.TryParse<KeyCode>(registeredSimulBinds[num], out KeyCode keyCode))
                    {//if the bound key is a valid keybind
                        CC.print("got the keyBind: " + registeredSimulBinds[num], 4);
                        if (current.keyCode == keyCode)
                        {//and the key is currently being pressed (otherwise do nothing until the key is pressed)
                            foreach (string command in registeredSimulCommands[num])
                            {//foreach command bound to this keybind
                                console.SubmitCmd(null, command, true);//activate the console command
                                CC.print("Detected key code: " + current.keyCode, 5);
                            }
                        }
                    }
                    else
                    {
                        MonoBehaviour.print("Failed to get the key to bind.");
                    }
                }
            }
        }

    }
}
