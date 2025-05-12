using System.IO;

using OpenTK.Audio.OpenAL;

namespace Toast.Engine.Resources.Audio;

/// <summary>
/// The OpenAL backend.
/// </summary>
public static class OAL
{
    /// <summary>
    /// Loads a wave file from the specified file stream.
    /// </summary>
    /// <param name="stream">The wave file we wish to load.</param>
    /// <param name="channels">The amount of channels of this wave file.</param>
    /// <param name="bits">The bits of this wave file.</param>
    /// <param name="rate">The sample rate of this wave file.</param>
    /// <returns>A byte array correspondent to the wave file we've just loaded.</returns>
    public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
    {
        // If the stream is invalid...
        if (stream == null)
        {
            // Log an error!
            Log.Error("Provided stream does not exist!");

            // Return!
            channels = bits = rate = 0;
            return null;
        }

        // To read the file...
        using (BinaryReader reader = new BinaryReader(stream))
        {
            // Get the signature of this file
            string signature = new string(reader.ReadChars(4));

            // If this file doesn't have the RIFF signature...
            if (signature != "RIFF")
            {
                // We've got an error!
                Log.Error("Provided file is not a valid wave file! Cause: signature does not match \"RIFF\".");
                
                // Return!
                channels = bits = rate = 0;
                return null;
            }

            // Get the size of the RIFF chunk
            int riffChunkSize = reader.ReadInt32();

            // Get the format of this file
            string format = new string(reader.ReadChars(4));

            // If the format isn't WAVE...
            if (format != "WAVE")
            {
                // We've got an error!
                Log.Error("Provided file is not a valid wave file! Cause: format does not match \"WAVE\".");

                // Return!
                channels = bits = rate = 0;
                return null;
            }

            // Get the format signature
            string format_signature = new string(reader.ReadChars(4));

            // If the format's signature is invalid...
            if (format_signature != "fmt ")
            {
                // We've got an error!
                Log.Error("Provided file is not a valid wave file! Cause: format signature does not match \"fmt \"");

                // Return!
                channels = bits = rate = 0;
                return null;
            }

            // Read information about the file!
            int formatChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();

            // Get the data signature
            string dataSignature = new string(reader.ReadChars(4));

            // If we have an invalid data signature...
            if (dataSignature != "data")
            {
                // We've got an error!
                Log.Error("Provided file is not a valid wave file! Cause: data signature does not match \"data\"");

                // Return!
                channels = bits = rate = 0;
                return null;
            }

            // Get the size of the data chunk
            int dataChunkSize = reader.ReadInt32();

            // Set our out values
            channels = numChannels;
            bits = bitsPerSample;
            rate = sampleRate;

            // Return the rest of the file!
            return reader.ReadBytes((int)reader.BaseStream.Length);
        }
    }

    /// <summary>
    /// Loads a wave file from the specified filepath.
    /// </summary>
    /// <param name="filepath">The path to the wave file we wish to load.</param>
    /// <param name="channels"></param>
    /// <param name="bits"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    public static byte[] LoadWave(string filepath, out int channels, out int bits, out int rate)
    {
        // If the file doesn't exist...
        if (!File.Exists(filepath))
        {
            // Log an error!
            Log.Error($"Provided file: \"{filepath}\" does not exist!");

            // Return!
            channels = bits = rate = 0;
            return null;
        }

        // Load the wave file by calling File.Open!
        return LoadWave(File.Open(filepath, FileMode.Open), out channels, out bits, out rate);
    }

    /// <summary>
    /// Gets the format of a provided sound.
    /// </summary>
    /// <param name="channels">The number of channels in this file.</param>
    /// <param name="bits">The bits per sample of this file.</param>
    /// <returns>The format of this sound file.</returns>
    public static ALFormat? GetSoundFormat(int channels, int bits)
    {
        switch (channels)
        {
            case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
            default: Log.Error("The specified sound format is not supported!"); return null;
        }
    }

    /// <summary>
    /// Gets the sound format of the provided <see cref="AudioFile"/>.
    /// </summary>
    /// <param name="file">The audio file we wish to get the format of.</param>
    /// <returns>The format of this <see cref="AudioFile"/>.</returns>
    public static ALFormat? GetSoundFormat(AudioFile file)
    {
        return GetSoundFormat(file.NumChannels, file.BitsPerSample);
    }
}