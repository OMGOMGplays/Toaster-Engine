using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;

using Toast.Engine.Attributes;

namespace Toast.Engine.Resources.Console;

/// <summary>
/// The console manager handles all console commands within the engine.<br/>
/// It can find, call and manage console commands to some extent.
/// </summary>
public static class ConsoleManager
{
    // The default path to the saved commands
    private const string PATH_COMMANDS = "resources/commands.json";

    // All of the found commands
    private static List<ConsoleCommand> commands = new();

    private static JsonSerializer serializer = new()
    {
        Formatting = Formatting.Indented,
    };

    /// <summary>
    /// Adds a <see cref="ConsoleCommand"/> to this console manager's list of commands.
    /// </summary>
    public static void AddCommand(ConsoleCommand command)
    {
        // Check every command...
        for (int i = 0; i < commands.Count; i++)
        {
            // If we already feature this command (found from the alias)...
            if (commands[i].alias == command.alias)
            {
                // If we have an onCall command...
                if (command.onCall != InvalidCommand)
                {
                    // Apply the onCall!
                    commands[i].onCall = command.onCall;
                    return;
                }
                else if (command.onArgsCall != InvalidCommand) // Otherwise, if we have an argument call...
                {
                    // Apply the onArgsCall!
                    commands[i].onArgsCall = command.onArgsCall;
                    return;
                }
            }
        }

        // Simply add the argument command to the list
        commands.Add(command);
    }

    /// <summary>
    /// Searches through this console manager's list of commands and returns one fitting with the argument <paramref name="commandAlias"/>.
    /// </summary>
    public static ConsoleCommand GetCommand(string commandAlias)
    {
        // Check every command...
        foreach (ConsoleCommand command in commands)
        {
            // If this command's alias fits the argument alias...
            if (command.alias == commandAlias.ToLower())
            {
                // Return it!
                return command;
            }
        }

        // We couldn't find one, log a warning and return null
        Log.Warning($"Couldn't find command with the alias of \"{commandAlias}\"!");
        return null;
    }

    /// <summary>
    /// Searches through this console manager's list of commands to try and find a console command with the alias from <paramref name="commandAlias"/>.
    /// </summary>
    /// <param name="commandAlias">The alias of the command we wish to find.</param>
    /// <param name="command">The resulting command.</param>
    /// <returns><see langword="true"/> if we found a command, <see langword="false"/> otherwise.</returns>
    public static bool TryGetCommand(string commandAlias, out ConsoleCommand command)
    {
        return (command = GetCommand(commandAlias)) != null;
    }

    /// <summary>
    /// Try to load our list of commands from our saved path.
    /// </summary>
    public static bool LoadCommands()
    {
        // If we don't have a commands file...
        if (!File.Exists(PATH_COMMANDS))
        {
            // Get outta dodge!
            return false;
        }

        try
        {
            // Open the file...
            using (StreamReader sr = new StreamReader(PATH_COMMANDS))
            {
                // Read the JSON data...
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    // If we did find our list of commands...
                    if ((commands = serializer.Deserialize<List<ConsoleCommand>>(reader)) != null)
                    {
                        // Successful command loading!
                        return true;
                    }
                }
            }
        }
        catch (Exception exc) // Unexpected exception has been caught!
        {
            Log.Error("Unmanaged exception caught!", exc);
            return false;
        }

        // Some other error happened
        return false;
    }

    /// <summary>
    /// Saves our commands to the default commands dictionary file.
    /// </summary>
    public static void SaveCommands()
    {
        // Create a new file at the default path
        FileStream file = File.Open(PATH_COMMANDS, FileMode.Create);
        file.Close();

        // Open the file...
        using (StreamWriter sw = new StreamWriter(PATH_COMMANDS))
        {
            // Create a new JSON writer...
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                // For every command...
                foreach (ConsoleCommand command in commands)
                {
                    // Set their command's alias to the name of their connected method
                    command.onCallAlias = command.onCall.Method.Name;
                    command.onArgsCallAlias = command.onArgsCall.Method.Name;
                }

                // Serialize the JSON data to the file!
                serializer.Serialize(writer, commands);
            }
        }
    }

    /// <summary>
    /// Registers every single console command that features <see cref="ConsoleCommandAttribute"/>.
    /// </summary>
    public static void RegisterCommands()
    {
        // Get the list of methods using the console command attribute
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttribute<ConsoleCommandAttribute>() != null);

        // For every method...
        foreach (MethodInfo method in methods)
        {
            // Get the attribute
            ConsoleCommandAttribute attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();

            // Get the action and argumented method
            Action onCall;
            Action<List<object>> onArgsCall = InvalidCommand;

            // If we couldn't find an argument-less function...
            if ((onCall = (Action)Delegate.CreateDelegate(typeof(Action), method, false)) == null)
            {
                // Set the argumented call correctly and the regular call to InvalidCommand
                onArgsCall = (Action<List<object>>)Delegate.CreateDelegate(typeof(Action<List<object>>), method);
                onCall = InvalidCommand;
            }

            // Make a new console command
            ConsoleCommand command = new ConsoleCommand
            {
                alias = attribute.Alias, // The alias of the console command
                description = attribute.Description, // The description of the console command
                conditions = attribute.Conditions, // Determines whether or not this command requires cheats to be called

                onCall = onCall, // Regular call method
                onArgsCall = onArgsCall, // Argumented call method
            };

            // Add the command
            AddCommand(command);
        }

        // Log our success!
        Log.Success("Successfully registered all console commands!");
    }

    /// <summary>
    /// Gets the total list of console commands.
    /// </summary>
    public static List<ConsoleCommand> GetConsoleCommands()
    {
        return commands;
    }

    /// <summary>
    /// Log every available command to the console, and their description
    /// </summary>
    [ConsoleCommand("help", "Displays information about a command, or the list of available commands.")]
    public static void DisplayCommands()
    {
        // Display a header / introduction to what we just did
        Log.Info("List of available commands and their status:");

        // For every command...
        foreach (ConsoleCommand command in commands)
        {
            // Display its information!
            Log.Info($"\t{command.alias} - {command.description} {(command.enabled ? "" : "(*DISABLED*)")}");
        }
    }

    /// <summary>
    /// Log the information about a specific command.
    /// </summary>
    [ConsoleCommand("help", "Displays information about a command, or the list of available commands.")]
    public static void DisplayCommands(List<object> args)
    {
        // Amount of arguments
        int argCount = args.Count - 1;

        // If we have more than the allowed amount of arguments...
        if (argCount > 1)
        {
            // Log the error!
            Log.Error("Error displaying command, more than one argument given! Please specify at most one argument, defining the alias of the keybind.");
            return;
        }

        // If we have found command...
        if (TryGetCommand((string)args[1], out ConsoleCommand command))
        {
            // Display its info!
            Log.Info($"\t{args[1]} - {command.description} {(command.enabled ? "" : "(*DISABLED*)")}");
        }
    }

    /// <summary>
    /// Toggles a command through the console.
    /// </summary>
    [ConsoleCommand("togglecommand", "Disables or enables a specific console command.")]
    public static void ToggleCommand(List<object> args)
    {
        // Find the command
        ConsoleCommand command = GetCommand((string)args[1]);

        // If we did actually find a command...
        if (command != null)
        {
            // If its alias is our own...
            if (command.alias == "togglecommand")
            {
                // We can't toggle its status!
                Log.Warning("Can't toggle the toggle command, that'd be problematic!");
                return;
            }

            // Toggle its state!
            command.enabled = !command.enabled;
        }
    }

    /// <summary>
    /// Try to call the command from our inputs.
    /// </summary>
    public static void TryCommand(string input)
    {
        // Log the input
        Log.Info(input);

        // Make sure the input isn't actually empty
        if (input != string.Empty)
        {
            // All of the collective arguments
            // Null by default, allows for commands without arguments to be called too
            object[] args = null;

            // If our input has spaces...
            if (input.Contains(" "))
            {
                // We should split our arguments on these spaces!
                args = input.Split(" ");
            }

            // Find our command, either from the first variable of our args list, or directly from our input
            ConsoleCommand command = GetCommand(args != null ? (string)args[0] : input);

            // Make sure our command actually is found...
            if (command == null)
            {
                // If not, return!!!
                return;
            }

            // If this command is disabled...
            if (!command.enabled)
            {
                // Log such to the console and get outta here!
                Log.Warning($"Found command \"{command.alias}\", but the command is disabled, therefore we cannot call it!");
                return;
            }

            // Check against which conditions this command has
            switch (command.conditions)
            {
                // No special conditions / default:
                case CommandConditions.None:
                default:
                    break; // Continue as intended

                // Requires cheats to be enabled:
                case CommandConditions.Cheats:
                    // If we don't have cheats enabled...
                    if (!EngineManager.cheatsEnabled)
                    {
                        // Log this revelating information to the console, then skedaddle!
                        Log.Warning($"Command \"{command.alias}\" requires cheats, but cheats are disabled!");
                        return;
                    }

                    // Otherwise, continue as intended
                    break;
            }

            // If we have arguments...
            if (args != null)
            {
                // Call the argumented version of this command's function!
                command.onArgsCall?.Invoke([.. args]);
            }
            else // Otherwise!
            {
                // Call the command's regular function!
                command.onCall?.Invoke();
            }
        }
    }

    /// <summary>
    /// An un-argumented invalid command.
    /// </summary>
    public static void InvalidCommand()
    {
        Log.Warning("This command only supports arguments! Please try to run the command again, with fitting arguments!");
    }

    /// <summary>
    /// An argumented invalid command.
    /// </summary>
    public static void InvalidCommand(List<object> args)
    {
        Log.Warning("This command does not support arguments! Please try to run the command again, without any arguments!");
    }

    /// <summary>
    /// Removes a <see cref="ConsoleCommand"/> from this console manager's list of commands.
    /// </summary>
    public static void RemoveCommand(ConsoleCommand command)
    {
        // Simpy remove the argument command from the list
        commands.Remove(command);
    }
}