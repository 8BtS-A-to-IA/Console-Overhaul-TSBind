using Rewired;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Bind
{
    /// <summary>
    /// Bind specific 'Console Commands'.
    /// </summary>
    class CC
    {
        private static bool debug = false;
        private static uint verbos = 1u;

        [ConCommand(commandName = "CODBGGetKeybindingDebugInfo", flags = ConVarFlags.None, helpText = "Prints debugging information on every bound key in the keyboardMap of the first local user.")]
        private static void CCGetKeybindingDebugInfo(ConCommandArgs args)
        {//remove commenting of whatever information you'll need
            KeyboardMap keyboardMap = LocalUserManager.GetFirstLocalUser().userProfile.keyboardMap;
            MonoBehaviour.print("Game bound keys (according to first local user) are:");
            foreach (ActionElementMap actionElementMap in keyboardMap.AllMaps)
            {//foreach mapped key
                MonoBehaviour.print("");
                MonoBehaviour.print("Key: " + actionElementMap.keyCode.ToString());
                MonoBehaviour.print("action ID: " + actionElementMap.actionId);
                //MonoBehaviour.print("axis contr: " + actionElementMap.axisContribution);
                //MonoBehaviour.print("axis range: " + actionElementMap.axisRange);
                //MonoBehaviour.print("axis type: " + actionElementMap.axisType);
                //MonoBehaviour.print("element ID name: " + actionElementMap.elementIdentifierName);
                //MonoBehaviour.print("element index: " + actionElementMap.elementIndex);
                //MonoBehaviour.print("element type: " + actionElementMap.elementType);
                //MonoBehaviour.print("enabled? " + actionElementMap.enabled);
                //MonoBehaviour.print("ID: " + actionElementMap.id);
                //MonoBehaviour.print("keyboard key code: " + actionElementMap.keyboardKeyCode.ToString());
                //MonoBehaviour.print("key code: " + actionElementMap.keyCode.ToString());

            }

        }

        [ConCommand(commandName = "COBind", flags = ConVarFlags.None, helpText = "Binds a console command to an unbound keyboard key. If there is no second parameter; the keybind will be unbound. " +
            "Multiple keybinds can be bound to the same key creating a toggle effect, but unbindig will unbind all commands bound to the key. COBind(key, command, [command argument 1], [...])")]
        public static void CCCOBind(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            KeyboardMap keyboardMap = ConCommandBinder.entryPoint.keyboardMap;
            List<string> keyBinds = ConCommandBinder.entryPoint.registeredKeyBinds;
            List<List<string>> CCs = ConCommandBinder.entryPoint.registeredConCommands;
            AddBind(args, keyboardMap, keyBinds, CCs);
        }

        [ConCommand(commandName = "COUnbind", flags = ConVarFlags.None, helpText = "Unbinds either a specified keybind, or a keybind that exists at a specific index, or the last keybind if index is -1" +
            ", for the specified list type (toggle or simul). If a keybind is defined, index is ignored. COUnbind(index, [list = toggle], [keybind = none])")]
        public static void CCCOUnbind(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            if (int.TryParse(args.userArgs[0], out int index))                                  //T
            {                                                                                   //
                KeyboardMap keyboardMap = ConCommandBinder.entryPoint.keyboardMap;              //
                List<string> keyBinds = ConCommandBinder.entryPoint.registeredKeyBinds;         //
                List<List<string>> CCs = ConCommandBinder.entryPoint.registeredConCommands;     //
                                                                                                //
                if (args.userArgs.Count() >= 2)
                {                                                                                           //
                    if (args.userArgs[1].Equals("simul", StringComparison.InvariantCultureIgnoreCase))      //
                    {                                                                           //
                        keyboardMap = ConCommandBinder.entryPoint.simulKeyboardMap;             //
                        keyBinds = ConCommandBinder.entryPoint.registeredSimulBinds;            //
                        CCs = ConCommandBinder.entryPoint.registeredSimulCommands;              //
                    }                                                                           //
                }                                                                               //
                                                                                                //
                if (args.userArgs.Count() >= 3)                                                 //
                {                                                                               //
                    if (Enum.TryParse(args.userArgs[3], out KeyCode keyCode))                   //T if the key is a valid KeyCode
                    {                                                                           //
                        Unbind(keyboardMap, keyBinds, CCs, keyCode);                            //use keyCode
                    }                                                                           //
                    else
                    {

                    }
                }                                                                               //
                else                                                                            //
                {                                                                               //
                    Unbind(keyboardMap, keyBinds, CCs, index);                                  //use indexing
                }                                                                               //
            }
            else 
            {

            }//
        }

        [ConCommand(commandName = "COSimulBind", flags = ConVarFlags.None, helpText = "Like COBind but when binding multiple commands to one key the commands wont be toggled " +
            "between eachother, instead they'll all run at once in the input order.")]
        public static void CCCOSimulBind(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            KeyboardMap keyboardMap = ConCommandBinder.entryPoint.simulKeyboardMap;
            List<string> keyBinds = ConCommandBinder.entryPoint.registeredSimulBinds;
            List<List<string>> CCs = ConCommandBinder.entryPoint.registeredSimulCommands;
            AddBind(args, keyboardMap, keyBinds, CCs);
        }

        [ConCommand(commandName = "CODBGListBoundKeybinds", flags = ConVarFlags.None, helpText = "Lists all keys that have been bound.")]
        private static void CCCOListBoundKeybinds(ConCommandArgs args)
        {
            MonoBehaviour.print("Game bound keys are:");
            foreach (KeyCode keyCode in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
            {//foreach key
                foreach (ActionElementMap actionElementMap in UserProfile.defaultProfile.keyboardMap.GetButtonMaps())
                {//foreach mapped key
                    if (keyCode == actionElementMap.keyCode)
                    {//if the current key is already bound (also allowing support for mods that add new keybinds)
                        MonoBehaviour.print(keyCode.ToString());
                    }
                }
            }

            MonoBehaviour.print("Console Overhaul bound keys are:");
            foreach (string key in ConCommandBinder.entryPoint.registeredKeyBinds)
            {
                MonoBehaviour.print(key.ToString());

            }
        }

        [ConCommand(commandName = "CODBGToggleBindDebugging", flags = ConVarFlags.None, helpText = "Toggles debugging for all CO-Bind Console Commands according to the verbos level. " +
            "Verbos 0 (or no paramater input) causes toggle, otherwise only changes verbos level. " +
            "Verbos level 0 = never show, 1 = is always show, 2 = errors only, 3 = quick debug, 4 = detailed debug 5 = overly detailed debug (WARNING CAUSES LAG)")]
        public static void CCCOToggleDebugging(ConCommandArgs args)
        {
            ushort num = 0;
            if (args.userArgs.Count == 1)
            {
                if (!ushort.TryParse(args.userArgs[0], out num))
                {
                    MonoBehaviour.print("COToggleDebugging: Paramater 1 must be an unsigned short.");
                    return;
                }
            }
            if (debug && num != 0)
            {
                verbos = (uint)num;
                print("verbos is now: " + verbos, 1);
            }
            if (num == 0)
            {
                debug = !debug;
                print("debugging activated.", 0);
                if (!debug)
                {
                    MonoBehaviour.print("debugging deactivated.");
                }
            }
            if (!debug && num != 0)
            {
                MonoBehaviour.print("Verbos can not be set while debugging is disabled.");
            }
        }


        /// <summary>
        /// Simply provides some real-time debugging message management through the toggleDebugging and Verbos command. Outputs to both bepinex's and the in game console.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="verbosReq"></param>
        /// <param name="callerName"></param>
        public static void print(string message, ushort verbosReq, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            if (debug && verbos >= (uint)verbosReq)
            {
                MonoBehaviour.print(callerName + ": " + message);
            }
        }

        /// <summary>
        /// Adds a new bind to the keybind:CC and keyboardMap list if the input arguments are valid.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="keyboardMap"></param>
        /// <param name="keyBinds"></param>
        /// <param name="CCs"></param>
        public static void AddBind(ConCommandArgs args, KeyboardMap keyboardMap, List<string> keyBinds, List<List<string>> CCs) 
        {
            if (Enum.TryParse(args.userArgs[0], out KeyCode keyCode))                                                                                               //T if the key is a valid KeyCode
            {                                                                                                                                                       //|
                if (args.userArgs.Count != 1)                                                                                                                       //|-T if there is more than 1 argument (i.e. there is a key and a CC)
                {                                                                                                                                                   //| |
                    # region if the key is already bound then only add the new command, otherwise add both the keybind and the command binidng with its new command
                    if (!keyboardMap.ContainsKeyboardKey(keyCode, ModifierKeyFlags.None))                                                                           //|-|--T if the key is not already bound (no need to create multiple keybinds, all we need is multiple commands)
                    {                                                                                                                                               //| |  |
                        CC.print("User is binding the command: " + args.userArgs[1] + " to the key: " + args.userArgs[0], 4);                                       //| |  |
                        ElementAssignment keyboardBinding = new ElementAssignment(keyCode, ModifierKeyFlags.None, ConCommandBinder.actionID, Pole.Negative);        //| |  |    (not inlined for debugging purposes)
                        keyboardMap.CreateElementMap(keyboardBinding);                                                                                              //|-|--|---- create a new keybind of the input key with the sudo-action of the current actionID
                        keyBinds.Add(args.userArgs[0]);                                                                                                             //|-|--|---- commit the key to the list (keyBinds and CCs are 1:1) 
                        CCs.Add(new List<string>());                                                                                                                //|-|--|---- add a new list for the new CCs to be commited to
                    }                                                                                                                                               //|-|--\e
                                                                                                                                                                    //| |
                    CCs[CCs.Count - 1].Add(args.userArgs[1]);                                                                                                       //|-|--- commit the new keybound CC to the CCs list
                    foreach (string text in args.userArgs)                                                                                                          //|-|--T foreach argument
                    {                                                                                                                                               //| |  |
                        if (text != args.userArgs[1] && text != args.userArgs[0])                                                                                   //|-|--|---T if the current argument is not the first or second argument (not the key being bound nor the new command)
                        {                                                                                                                                           //| |  |   |
                            int upperIndex = CCs.Count - 1;                                                                                                         //|-|--|---|----- get the index of the just added command binding
                            int lowerIndex = CCs[upperIndex].Count - 1;                                                                                             //|-|--|---|----- get the index of the just added command
                            CCs[upperIndex][lowerIndex] += " " + text;                                                                                              //|-|--|---|----- add the new commands arguments to the command
                        }                                                                                                                                           //|-|--|---\e
                    }                                                                                                                                               //|-|--\L
                    CC.print("Done binding.", 4);                                                                                                                   //| |
                    return;                                                                                                                                         //<<<<<< RETURN
                    #endregion
                }                                                                                                                                                   //|-\e
                                                                                                                                                                    //|
                Unbind(keyboardMap, keyBinds, CCs, keyCode);                                                                                                       //|-- (implied) otherwise, if there is one argument, unbind the latest added keybind
            }                                                                                                                                                       //|
            else                                                                                                                                                    //\c otherwise if the key is invalid
            {                                                                                                                                                       //|
                MonoBehaviour.print("Please enter a valid key.");                                                                                                   //|-- Tell the user the input key is invalid.
            }                                                                                                                                                       //\e

        }

        /// <summary>
        /// Unbinds the keybind at the specified index, otherwise the last keybind if index is -1.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="keyboardMap"></param>
        /// <param name="keyBinds"></param>
        /// <param name="CCs"></param>
        /// <param name="KeyBindIndex"></param>
        public static void Unbind(KeyboardMap keyboardMap, List<string> keyBinds, List<List<string>> CCs, int KeyBindIndex)
        {
            CC.print("Unbinding request at index: " + KeyBindIndex, 4);
            int keyBindsCount = keyBinds.Count;
            if (keyBindsCount != 0)
            {
                if (KeyBindIndex >= keyBindsCount)
                {
                    throw new ConCommandException("Can not unbind keybind at index '" + KeyBindIndex
                        + "' as no such keybind exist. Highest current keybind is: " + keyBindsCount);
                }

                if (KeyBindIndex <= -1) { Unbind(keyboardMap, keyBinds, CCs, (KeyCode)Enum.Parse(typeof(KeyCode), keyBinds.Last())); return; }          //T\e If the input target index is -1; unbind the last keybind

                bool syncCheck = false;                                                                                                                 //
                syncCheck = keyBinds.Remove(keyBinds[KeyBindIndex]);                                                                                    //|-- (if previous was sucessful, otherwise skip,) attempt to remove the keybind from the local list at the "KeyBindIndex" index and override "syncCheck"
                switch (syncCheck)                                                                                                                      //|-T if "syncCheck" indicated:
                {                                                                                                                                       //| |
                    case true:                                                                                                                          //|-\c BOTH sucessfull
                        CC.print("Unbound key. Unbinding CC with index: " + KeyBindIndex, 4);                                                           //| |
                        CCs.Remove(CCs[KeyBindIndex]);                                                                                                  //|-|--- attempt to remove the CC from the list at the "KeyBindIndex" index, to resync the two local lists
                        MonoBehaviour.print("Done unbinding.");                                                                                         //| |     (if this fails then a desync somehow happened and may, or may not, now be fixed)
                        return;                                                                                                                         //<<<<<<< RETURN
                    case false:                                                                                                                         //|-\c EITHER failure
                        throw new ConCommandException("Failed to unbind key, either the key did not exist in the keybinds list/keyboardMap!");          //|-|--- Throw ConCommand error
                }                                                                                                                                       //|-\e
            }

            MonoBehaviour.print("Could not find the key to unbind, no unbinding process has occured. Maybe the key is already unbound?");
        }

        /// <summary>
        /// Unbinds the specified key from the keyBinds list and keyboardMap.
        /// </summary>
        /// <param name="keyboardMap"></param>
        /// <param name="keyBinds"></param>
        /// <param name="CCs"></param>
        /// <param name="keyCode"></param>
        public static void Unbind(KeyboardMap keyboardMap, List<string> keyBinds, List<List<string>> CCs, KeyCode keyCode = KeyCode.None)
        {
            CC.print("Unbinding request for key: " + keyCode, 4);

            ActionElementMap elementMap = keyboardMap.ElementMapsWithAction(ConCommandBinder.actionID).ToList().Find(x => x.keyCode == keyCode);        //- get the KeyboardMap keybind that has the current keyCode
            int KeyBindIndex = keyBinds.IndexOf(keyCode.ToString());                                                                                    //- get the index of the key in the keyBinds list, the index in keyBinds is always the same as the CCs list
            if (!(elementMap is null))                                                                                                                  //T if there is at least one keybind (in the KeyboardMap)
            {                                                                                                                                           //|
                bool syncCheck = keyboardMap.DeleteElementMap(elementMap.id);                                                                           //|-- attempt to delete the keybind from the map and store if successful in a "syncCheck" var
                syncCheck = syncCheck ? keyBinds.Remove(keyCode.ToString()) : false;                                                                    //|-- (if previous was sucessful, otherwise skip,) attempt to remove the keybind from the local list at the "KeyBindIndex" index and override "syncCheck"
                switch (syncCheck)                                                                                                                      //|-T if "syncCheck" indicated:
                {                                                                                                                                       //| |
                    case true:                                                                                                                          //|-\c BOTH sucessfull
                        CC.print("Unbound key. Unbinding CC with index: " + KeyBindIndex, 4);                                                           //| |
                        CCs.Remove(CCs[KeyBindIndex]);                                                                                                  //|-|--- attempt to remove the CC from the list at the "KeyBindIndex" index, to resync the two local lists
                        MonoBehaviour.print("Done unbinding.");                                                                                         //| |     (if this fails then a desync somehow happened and may, or may not, now be fixed)
                        return;                                                                                                                         //<<<<<<< RETURN
                    case false:                                                                                                                         //|-\c EITHER failure
                        throw new ConCommandException("Failed to unbind key, either the key did not exist in the keybinds list/keyboardMap!");          //|-|--- Throw ConCommand error
                }                                                                                                                                       //|-\e
            }                                                                                                                                           //\e
            MonoBehaviour.print("Could not find the key to unbind, no unbinding process has occured. Maybe the key is already unbound?");
        }

    }
}
