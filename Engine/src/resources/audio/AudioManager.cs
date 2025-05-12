using System;
using System.IO;
using System.Collections.Generic;

using OpenTK.Audio.OpenAL;

using Toast.Engine.Entities;
using Toast.Engine.Attributes;

namespace Toast.Engine.Resources.Audio;

/// <summary>
/// Class that should manage playing, stopping, and updating all manners of audio files.
/// </summary>
public static class AudioManager
{
    // Constant paths to engine-important audio files
    private const string PATH_AUDIO_SUCCESS = "resources/audio/engine/success.wav";
    private const string PATH_AUDIO_WARNING = "resources/audio/engine/warning.wav";
    private const string PATH_AUDIO_ERROR = "resources/audio/engine/error.wav";

    // Our list of actively playing sound files
    private static List<AudioFile> playingFiles = new List<AudioFile>();

    /// <summary>
    /// Plays a sound from the console.
    /// </summary>
    [ConsoleCommand("playsound", "Plays a sound from a specified path (should be something like \"resources/audio/engine/error.mp3\".)")]
    public static void PlaySound(List<object> args)
    {
        // The amount of arguments (-1 cause the first one is always the command itself)
        int argCount = args.Count - 1;

        string filepath = args[1].ToString().ToLower(); // Get the filepath
        float volume = 1.0f; // Default volume
        bool repeats = false; // Default repeat status

        // If we have enough arguments for it...
        if (argCount >= 2)
        {
            // Get the volume
            if (!float.TryParse(args[2].ToString().Replace(".", ","), out volume))
            {
                Log.Error("Second argument is an invalid float!");
                return;
            }
        }

        // If we have enough arguments for it...
        if (argCount >= 3)
        {
            // Do we repeat?
            if (!bool.TryParse((string)args[3], out repeats))
            {
                Log.Error("Third argument is an invalid bool!");
                return;
            }
        }

        // Call the regular play sound method with our parsed arguments
        PlaySound(filepath, volume, repeats);
    }

    /// <summary>
    /// Plays a sound effect from a specified path.
    /// </summary>
    /// <param name="filepath">The path to the specific sound we wish to play.</param>
    /// <param name="volume">Determines the volume of which this sound should play at. (Scale of 0.0f - 1.0f)</param>
    /// <param name="repeats">Determines whether or not this sound should repeat (loop) or not.</param>
    public static AudioFile PlaySound(string filepath, float volume = 1.0f, bool repeats = false)
    {
        try
        {
            // The file we're about to create
            AudioFile file = null;

            // If we're using the OpenAL backend OR settings is null...
            if (EngineManager.settings == null
              || EngineManager.settings.AudioBackend == AudioBackend.OpenAL)
            {
                // Create a new file from the OpenAL backend
                file = new AudioFile();

                // Set basic variables
                file.Filepath = filepath;
                file.Volume = volume;
                file.Repeats = repeats;

                // Add the newly made and playing audio to the list of playing files
                playingFiles.Add(file);

                // Start actually playing it!
                file.Buffer = AL.GenBuffer();
                file.Source = AL.GenSource();

                // Load the wave file
                file.Data = OAL.LoadWave(filepath, out file.NumChannels, out file.BitsPerSample, out file.SampleRate);

                // Buffer the data to AL
                AL.BufferData(file.Buffer, (ALFormat)OAL.GetSoundFormat(file), file.Data, file.Data.Length, file.SampleRate);
            }

            // Return the file!
            return file;
        }
        catch (Exception exc)
        {
            // If the file / directory isn't found...
            if (exc is FileNotFoundException || exc is DirectoryNotFoundException)
            {
                // Log the warning!
                // If the warning sound itself is missing, it will cause an infinite recursion... Whoopsies!
                Log.Warning($"Couldn't find file at \"{filepath}\"!");
                return null;
            }

            // Log an error with the unmanaged exception!
            // If the error sound is missing, it means that this will cause an infinite recursion... Whoopsies!
            Log.Error("Exception caught playing sound!", exc);
            return null;
        }
    }

    /// <summary>
    /// Plays a sound from a specified entity, utilizing its position and velocity to determine 3D values.
    /// </summary>
    /// <param name="source">The source of this audio.</param>
    /// <param name="filepath">The path to the specific sound we wish to play.</param>
    /// <param name="volume">Determines the volume of which this sound should play at. (Scale of 0.0f - 1.0f)</param>
    /// <param name="repeats">Determines whether or not this sound should repeat (loop) or not.</param>
    public static AudioFile PlaySound(Entity source, string filepath, float volume = 1.0f, bool repeats = false)
    {
        // Play the sound as per usual
        AudioFile file = PlaySound(filepath, volume, repeats);

        // Set 3D properties of the sound from the provided entity
        

        // Return the file!
        return file;
    }

    /// <summary>
    /// Method to update all actively playing audio files.
    /// </summary>
    public static void Update()
    {
        // Check every actively playing file...
        for (int i = 0; i < playingFiles.Count; i++)
        {
            // Get the current file
            AudioFile file = playingFiles[i];

            // If we encountered a null file...
            if (file == null)
            {
                playingFiles.Remove(file); // Remove it from the list!
                continue; // Continue to the next file
            }
        }
    }

    /// <summary>
    /// Stop a specified <see cref="AudioFile"/>.
    /// </summary>
    /// <param name="file">The <see cref="AudioFile"/> we wish to stop the sound of.</param>
    public static void StopSound(AudioFile file)
    {
        playingFiles.Remove(file); // Remove the file from our list
    }

    /// <summary>
    /// Method to stop all currently playing sounds.
    /// </summary>
    [ConsoleCommand("stopsounds", "Stops all actively playing sounds.")]
    public static void StopAllSounds()
    {
        // For every playing file...
        for (int i = 0; i < playingFiles.Count; i++)
        {
            // Get the current file
            AudioFile file = playingFiles[i];

            // Dispose of the file
            file.Dispose();
        }

        // Clear the list of playing files
        playingFiles.Clear();
    }

    /// <summary>
    /// Returns the AudioManager's list of currently playing <see cref="AudioFile"/>'s.
    /// </summary>
    public static List<AudioFile> GetPlayingFiles()
    {
        return playingFiles;
    }

    /// <summary>
    /// Checks whether or not the argument file is in our list of playing files,<br/>
    /// therefore is playing.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="playingFiles"/> contains the given file, <see langword="false"/> otherwise.</returns>
    public static bool FileIsPlaying(AudioFile file)
    {
        return playingFiles.Contains(file);
    }

    /// <summary>
    /// Displays the list of actively playing files.
    /// </summary>
    [ConsoleCommand("displaysounds", "Displays all currently playing audio files.")]
    public static void DisplayPlayingFiles()
    {
        // Header for the information about to be displayed
        Log.Info("Actively playing files:");

        // For every file...
        foreach (AudioFile file in playingFiles)
        {

        }
    }

    /// <summary>
    /// Plays the engine's success sound.
    /// </summary>
    public static void PlaySuccess()
    {
        PlaySound(PATH_AUDIO_SUCCESS);
    }

    /// <summary>
    /// Plays the engine's warning sound.
    /// </summary>
    public static void PlayWarning()
    {
        PlaySound(PATH_AUDIO_WARNING);
    }

    /// <summary>
    /// Plays the engine's error sound.
    /// </summary>
    public static void PlayError()
    {
        PlaySound(PATH_AUDIO_ERROR);
    }
}

/// <summary>
/// The different audio backends we can use in the engine.
/// </summary>
public enum AudioBackend
{
    /// <summary>
    /// The OpenAL backend is a very modular backend, letting you easily create 3D audio<br/>
    /// at the cost of not having channels built-in.
    /// </summary>
    OpenAL,
}