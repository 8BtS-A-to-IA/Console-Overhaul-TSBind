using Rewired;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Bind
{
    /// <summary>
    /// MMHOOK hooks for reliably initalizing fields at the correct run-time and for adding functionality to the in-game console.
    /// </summary>
    public class Hooks
    {
        private static bool actionIDBound = false;
        private static bool BACAttemptedLoad = false;

        internal static void InitializeHooks()
        {
            On.RoR2.UI.ConsoleWindow.OnInputFieldValueChanged += ConsoleWindow_OnInputFieldChanged;
            On.RoR2.Console.Awake += Console_Awake;
            On.RoR2.Console.Update += Console_Update;

            //On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };//mutiplayer dbg testing
        }

        internal static void DisableHooks()
        {
            On.RoR2.UI.ConsoleWindow.OnInputFieldValueChanged -= ConsoleWindow_OnInputFieldChanged;
            On.RoR2.Console.Awake -= Console_Awake;
            On.RoR2.Console.Update -= Console_Update;

            //On.RoR2.Networking.GameNetworkManager.OnClientConnect -= (self, user, t) => { };//mutiplayer dbg testing
        }

        /// <summary>
        /// Sets up keybind registry tracking.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="s"></param>
        public static void Console_Awake(On.RoR2.Console.orig_Awake o, RoR2.Console s)
        {
            ConCommandBinder.entryPoint.registeredKeyBinds = new List<string>();//setup registeredkeybind list
            ConCommandBinder.entryPoint.registeredConCommands = new List<List<string>>();
            ConCommandBinder.entryPoint.registeredSimulBinds = new List<string>();//setup registeredSimulKeybind list
            ConCommandBinder.entryPoint.registeredSimulCommands = new List<List<string>>();
            o.Invoke(s);
        }

        /// <summary>
        /// Preforms keybind mismatch checks.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="s"></param>
        public static void Console_Update(On.RoR2.Console.orig_Update o, RoR2.Console s)
        {
            if (!(LocalUserManager.GetFirstLocalUser() is null))
            {
                if (!(LocalUserManager.GetFirstLocalUser().userProfile is null))
                {//check user's profile exists/has been chosen before getting keybinds
                    if (ConCommandBinder.entryPoint.keyboardMap is null)
                    {
                        ConCommandBinder.entryPoint.keyboardMap = LocalUserManager.GetFirstLocalUser().userProfile.keyboardMap;
                    }

                    if (ConCommandBinder.entryPoint.simulKeyboardMap is null && !(ConCommandBinder.entryPoint.keyboardMap is null)) 
                    {
                        ConCommandBinder.entryPoint.simulKeyboardMap = new KeyboardMap(ConCommandBinder.entryPoint.keyboardMap);
                    }
                    KeyboardMap keyboardMap = ConCommandBinder.entryPoint.keyboardMap;//get keybinds
                    KeyboardMap simulKeyboardMap = ConCommandBinder.entryPoint.simulKeyboardMap;

                    #region actionID binding; checks if the actionID 598 is already bound, if it is it repeatedly iterates the local actionID field before checking if it's already bound again.
                    if (!(keyboardMap is null) && !actionIDBound)
                    {//if keybinds initalized
                        if (keyboardMap.ContainsAction(ConCommandBinder.actionID))
                        {//and the current actionID is taken by another mod
                            bool wrn = false;
                            //ElementAssignment keyboardBinding = new ElementAssignment(KeyCode.T, ModifierKeyFlags.None, ConCommandBinder.actionID, Pole.Negative);//not inlined for debugging purposses

                            while (true)
                            {
                                //keyboardMap.CreateElementMap(keyboardBinding);//create keybind to check if the actionID has a defined action
                                if (keyboardMap.GetElementMapsWithAction(ConCommandBinder.actionID).Any(e => e.actionDescriptiveName == ""))
                                {//if the current actionID does not have a defined action
                                    //keyboardMap.DeleteElementMapsWithAction(ConCommandBinder.actionID);//remove the checker keybind (removes all, so this wont work)
                                    if (keyboardMap.ContainsAction(ConCommandBinder.actionID))
                                    {//and the current actionID is taken by another mod
                                        System.Diagnostics.Trace.TraceError("A mod is using Console Overhuals' actionID namespace in keyboardMap: " + ConCommandBinder.actionID);
                                        ConCommandBinder.actionID++;//look for an unused one
                                        if (!wrn)
                                        {//this should only ever run once
                                            System.Diagnostics.Trace.TraceError("Moved to: " + ConCommandBinder.actionID + ". If any errors in mods that have this as a dependency occur, this is likely why.");
                                            System.Diagnostics.Trace.TraceError("If you are a mod developer trying to create a patch for an incombatibilty fix while you wait for the culprit " +
                                                "mod to fix their namespacing, please itterate actionID until it is not used then -1 to semi-confidently get CO's new actionID namespace.");
                                            wrn = true;
                                        }
                                        else
                                        {
                                            System.Diagnostics.Trace.TraceInformation("Moved actionID namespace to: " + ConCommandBinder.actionID);
                                        }
                                    }
                                    else
                                    {//otherwise if there's no defined action for the current actionID and it's not being used
                                        System.Diagnostics.Trace.TraceInformation("Found unused actionID namespace: " + ConCommandBinder.actionID);
                                        keyboardMap.CreateElementMap(new ElementAssignment(KeyCode.Tilde, ModifierKeyFlags.None, ConCommandBinder.actionID, Pole.Negative));
                                        actionIDBound = true;
                                        break;//keep actionID as it currently is.
                                    }
                                }
                                else
                                {//if the actionID has an action; it's a base game actionID which can't be used
                                    System.Diagnostics.Trace.TraceError("The current actionID namespace \"" + ConCommandBinder.actionID + "\" is being used by the game.");
                                    ConCommandBinder.actionID++;//so look for an unused one
                                    System.Diagnostics.Trace.TraceError("Moved to: " + ConCommandBinder.actionID + ". If any errors in mods that have this as a dependency occur, this is likely why.");
                                }
                            }
                        }
                        else
                        {
                            keyboardMap.CreateElementMap(new ElementAssignment(KeyCode.Tilde, ModifierKeyFlags.None, ConCommandBinder.actionID, Pole.Negative));//tilde isn't actually used ever, back comma is
                            actionIDBound = true;
                        }
                    }
                    #endregion


                    o.Invoke(s);//let the console do its thing

                    #region preform keybind mismatch check
                    List<string> registeredKeyBinds = ConCommandBinder.entryPoint.registeredKeyBinds;
                    List<List<string>> registeredConCommands = ConCommandBinder.entryPoint.registeredConCommands;
                    KeyBindMismatchError(registeredKeyBinds, registeredConCommands, keyboardMap);
                    
                    List<string> registeredSimulBinds = ConCommandBinder.entryPoint.registeredSimulBinds;
                    List<List<string>> registeredSimulCommands = ConCommandBinder.entryPoint.registeredSimulCommands;
                    KeyBindMismatchError(registeredSimulBinds, registeredSimulCommands, simulKeyboardMap);
                    #endregion
                }
            }

            if (s)
            {
                Event current = Event.current;
                ConCommandBinder.entryPoint.Bind(s, current);//check if key in registeredKeybinds is being pressed (in Event)
                ConCommandBinder.entryPoint.SimulBind(s, current);

            }
        }

        /// <summary>
        /// If B.A.C. is installed; injects extention code ONCE, otherwise does nothing.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="s"></param>
        /// <param name="input"></param>
        private static void ConsoleWindow_OnInputFieldChanged(On.RoR2.UI.ConsoleWindow.orig_OnInputFieldValueChanged o, ConsoleWindow s, string input) 
        {
            o.Invoke(s, input);

            if (!BACAttemptedLoad)
            {//have this only run once
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.ML.BAC", out BepInEx.PluginInfo plugin))
                {//if B.A.C. is loaded; out the pluginInfo
                    List<object> args = new List<object>
                    {
                        new List<string>() { "CC COBind", "CC COSimulBind" },
                        (Action)(() => ACFillWithKeybinds())
                    };//define arguments

                    plugin.Instance.SendMessage("BACListener", args);//tell B.A.C. to add this extention
                }
                BACAttemptedLoad = true;
            }
        }
        
        /// <summary>
        /// Fills the B.A.C. systems' results list with keybind names that have the characters present in the parameter (if B.A.C. is installed; injects extention code).
        /// </summary>
        internal static void ACFillWithKeybinds()
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.ML.BAC", out BepInEx.PluginInfo pluginInf)) 
            {//if BAC is not loaded anymore somehow
                throw new ConCommandException("B.A.C. install not detected mid run! Was B.A.C. somehow unloaded?");//stop here
            }

            //The custom code that will be run 'inside' of BAC's "ACFillWithCustom" to extend its functionality to allow the console commands "COBind" and "COSimulBind" to have their first parameter be tabbable through.
            Action<object> CustomAction = (keyCodeObj) =>
            {
                KeyCode keyCode = (KeyCode)keyCodeObj;
                bool isbound = false;

                foreach (ActionElementMap actionElementMap in UserProfile.defaultProfile.keyboardMap.GetButtonMaps())
                {//foreach mapped key
                    if (keyCode == actionElementMap.keyCode)
                    {//if the current key is already bound (also allowing support for mods that add new keybinds)
                        isbound = true;//set isbound flag to true
                        break;
                    }
                }

                if (!isbound)
                {//if the current key is not already bound
                    pluginInf.Instance.SendMessage("AutoCompleteResultsAdd", keyCode.ToString());//add to the suggestion list
                }

            };

            //Action object which converts the current custom enumerable BAC is looping through into a string
            Action<object> CustomStringFromList = (customString) =>
            {
                pluginInf.Instance.SendMessage("ACCustomStringFromListReturnPoint", customString.ToString());//Converts the current enumerable into a string and returns it to BAC
            };


            List<object> CustomList = new List<object>();//the generic list of enumerables that CustomAction will itterate through
            Array.ForEach((KeyCode[])Enum.GetValues(typeof(KeyCode)), e => CustomList.Add(e));//for this case; just all keyCodes
            //foreach (KeyCode key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
            //{
            //    test.Add(key);
            //}

            List<object> args = new List<object>
            {
                CustomAction,
                new List<string>() { "CC COBind", "CC COSimulBind" },
                CustomList,
                CustomStringFromList
            };

            pluginInf.Instance.SendMessage("ACFillWithCustom", args);//tell BAC to tether the extention

            //this is assuming the first parameter, and only the first, is what is being looked for
            //if (BAC.entryPoint.CurrentFoundACRI.Equals("CC COBind", StringComparison.InvariantCultureIgnoreCase) || BAC.entryPoint.CurrentFoundACRI.Equals("CC COSimulBind", StringComparison.InvariantCultureIgnoreCase))
            //{//and the special case is the bind CC
            //    foreach (KeyCode keyCode in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
            //    {
            //        if (!keyCode.ToString().ToLowerInvariant().Contains("joystick") && !keyCode.ToString().ToLowerInvariant().Contains("none"))
            //        {
            //            if (BAC.entryPoint.Instruction != "")
            //            {//if there is an instruction
            //                if (keyCode.ToString().ToLowerInvariant().Contains(BAC.entryPoint.Instruction.ToLowerInvariant()))
            //                {//if the current instruction is not empty and the current key contains the text of the instruction
            //                    bool isbound = false;
            //                    foreach (ActionElementMap actionElementMap in UserProfile.defaultProfile.keyboardMap.GetButtonMaps())
            //                    {//foreach mapped key
            //                        if (keyCode == actionElementMap.keyCode)
            //                        {//if the current key is already bound (also allowing support for mods that add new keybinds)
            //                            isbound = true;//set isbound flag to true
            //                            break;
            //                        }
            //                    }

            //                    if (!isbound)
            //                    {//if the current key is not already bound
            //                        BAC.entryPoint.AutoCompleteResults.Add(keyCode.ToString());//add the result to the results list
            //                    }

            //                }

            //                BAC.entryPoint.levenshteinSort(BAC.entryPoint.AutoCompleteResults, BAC.entryPoint.Instruction);//sort by closest matching key
            //            }
            //            else
            //            {
            //                bool isbound = false;
            //                foreach (ActionElementMap actionElementMap in UserProfile.defaultProfile.keyboardMap.GetButtonMaps())
            //                {//foreach mapped key
            //                    if (keyCode == actionElementMap.keyCode)
            //                    {//if the current key is already bound (also allowing support for mods that add new keybinds)
            //                        isbound = true;//set isbound flag to true
            //                        break;
            //                    }
            //                }

            //                if (!isbound)
            //                {//if they current key is not already bound
            //                    BAC.entryPoint.AutoCompleteResults.Add(keyCode.ToString());//add the result to the results list
            //                }

            //            }
            //        }
            //    }

            //}
        }

        /// <summary>
        /// Handles the keybind mismatch error by unregerstering the keybindings and reporting the error to the user.
        /// </summary>
        /// <param name="registeredBinds"></param>
        /// <param name="registeredCommands"></param>
        /// <param name="keyboardMap"></param>
        private static void KeyBindMismatchError(List<string> registeredBinds, List<List<string>> registeredCommands, KeyboardMap keyboardMap)
        {
            int registeredBindsCount = registeredBinds.Count;
            int registeredCommandsCount = registeredCommands.Count;
            int keyBoardMapsimulKeybindCount = keyboardMap.ElementMapsWithAction(ConCommandBinder.actionID).ToList().Count;

            if (((registeredCommandsCount != 0 || registeredBindsCount != 0) && registeredCommandsCount != registeredBindsCount) ||
                keyBoardMapsimulKeybindCount != registeredCommandsCount || keyBoardMapsimulKeybindCount != registeredBindsCount)
            {//if there is a bound command and they're not matched or a bound command but less or more keyboardMap keys
                MonoBehaviour.print("An internal error occured: keybinds mismatch. keybind registry will be dumped!");
                Trace.TraceInformation("ERROR: KEYBINDS MISMATCH! KEYBINDS WILL BE UNREGISTERED!");
                registeredBinds.Clear();//reset binds
                registeredCommands.Clear();
                if (keyBoardMapsimulKeybindCount > 0)
                {//if at least one local keybind from this mod has been bound (via COSimulBind)
                    keyboardMap.DeleteElementMapsWithAction(ConCommandBinder.actionID);//unbind them all
                }
            }

        }

    }
}
