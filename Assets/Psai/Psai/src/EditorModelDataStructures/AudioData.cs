using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using psai.Editor;

//using System.Media;

namespace psai.Editor
{

    [Serializable]
    public class AudioData : ICloneable
    {       
        private string _filePathRelativeToProjectDir = "";

        public int _prebeatLengthInSamplesEnteredManually = 0;
        public int _postbeatLengthInSamplesEnteredManually = 0;
        

        /// <summary>
        /// Holds the audio filepath relative to the Project Dir, using '/' as the directory separator if subdirs are used.
        /// </summary>
        /// <remarks>
        /// Within the XML file, always the '/' is used as the directory separator.
        /// </remarks>
        [XmlElement("Path")]
        public string FilePathRelativeToProjectDir
        {
            get
            {
                return _filePathRelativeToProjectDir;
            }
            set
            {
                string path = value.Replace(Path.DirectorySeparatorChar, '/');
                _filePathRelativeToProjectDir = path;
            }
        }

        /// <summary>
        /// Returns the Path relative to the Project File, using Path.DirectorySeparatorChar instead of '/'.
        /// </summary>
        [XmlIgnoreAttribute()]
        public string FilePathRelativeToProjectDirForCurrentSystem
        {
            get
            {
                return _filePathRelativeToProjectDir.Replace('/', Path.DirectorySeparatorChar);
            }
        }

        public float Bpm
        {
            get;
            set;
        }

        public float PreBeats
        {
            get;
            set;
        }

        public float PostBeats
        {
            get;
            set;
        }

        public bool CalculatePostAndPrebeatLengthBasedOnBeats
        {
            get;
            set;
        }

        
        public int PreBeatLengthInSamples
        {
            get
            {
                if (CalculatePostAndPrebeatLengthBasedOnBeats)
                {
                    return GetPrebeatLengthInSamplesBasedOnBeats();
                }
                else
                    return _prebeatLengthInSamplesEnteredManually;
            }

            set
            {
                _prebeatLengthInSamplesEnteredManually = value;
            }
        }

        public int PostBeatLengthInSamples
        {
            get
            {
                if (CalculatePostAndPrebeatLengthBasedOnBeats)
                {
                    return GetPostbeatLengthInSamplesBasedOnBeats();
                }
                else
                    return _postbeatLengthInSamplesEnteredManually;
            }
            set
            {
                _postbeatLengthInSamplesEnteredManually = value;
            }
        }

        public int TotalLengthInSamples
        {
            get;
            set;
        }
        
        /// <summary>
        /// The Sample Rate in kHz
        /// </summary>
        /// <remarks
        /// Although the sample rate can be read out from the wave header, however to be able to load the Project files
        /// directly by the psai engine, we need to serialize the SampleRate anyway to calculate the length in milliseconds. 
        /// </remarks>
        public int SampleRate
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        public int BitsPerSample
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        public int ChannelCount
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]        
        /// only needed for playback in the Playback Panel
        public Int64 ByteIndexOfWaveformDataWithinAudioFile
        {
            get;
            set;
        }

        [XmlIgnoreAttribute()]
        /// only needed for playback in the Playback Panel
        public int LengthOfWaveformDataInBytes
        {
            get;
            set;
        }
        //////////////////////////////////////////////////////////////////////////

        public AudioData()
        {
            FilePathRelativeToProjectDir = "";
            BitsPerSample = 0;
            PostBeatLengthInSamples = 0;
            PreBeatLengthInSamples = 0;
            SampleRate = 0;
            LengthOfWaveformDataInBytes = 0;
            Bpm = 100;
            PreBeats = 1;
            PostBeats = 1;
            CalculatePostAndPrebeatLengthBasedOnBeats = false;
        }


        public psai.net.AudioData CreatePsaiDotNetVersion()
        {
            psai.net.AudioData netAudioData = new psai.net.AudioData();
            netAudioData.filePathRelativeToProjectDir = this.FilePathRelativeToProjectDir;

            if (CalculatePostAndPrebeatLengthBasedOnBeats)
            {
                netAudioData.sampleCountPreBeat = GetPrebeatLengthInSamplesBasedOnBeats();
                netAudioData.sampleCountPostBeat = GetPostbeatLengthInSamplesBasedOnBeats();
            }
            else
            {
                netAudioData.sampleCountPreBeat = this.PreBeatLengthInSamples;
                netAudioData.sampleCountPostBeat = this.PostBeatLengthInSamples;
            }

            netAudioData.sampleCountTotal = this.TotalLengthInSamples;
            netAudioData.sampleRateHz = this.SampleRate;
            netAudioData.bpm = this.Bpm;

            return netAudioData;
        }


        public ProtoBuf_AudioData CreateProtoBuf()
        {
            ProtoBuf_AudioData pbAudio = new ProtoBuf_AudioData();

            pbAudio.filename = this.FilePathRelativeToProjectDir;

            if (CalculatePostAndPrebeatLengthBasedOnBeats)
            {
                pbAudio.sampleCountPrebeat = GetPrebeatLengthInSamplesBasedOnBeats();
                pbAudio.sampleCountPostbeat = GetPostbeatLengthInSamplesBasedOnBeats();
            }
            else
            {
                pbAudio.sampleCountPrebeat = this.PreBeatLengthInSamples;
                pbAudio.sampleCountPostbeat = this.PostBeatLengthInSamples;
            }

            pbAudio.sampleCountTotal = TotalLengthInSamples;
            pbAudio.sampleRate = this.SampleRate;
            pbAudio.bpm = this.Bpm;
            return pbAudio;
        }

        public int GetMillisecondsFromSampleCount(int sampleCount)
        {
            return (int)((long)sampleCount * 1000 / SampleRate);
        }

        public int GetSampleCountFromMilliseconds(int durationMs)
        {
            int sampleCount = (int) (SampleRate * durationMs / 1000);
            return sampleCount;
        }

        public int GetLengthInSamplesBasedOnBeats(float bpm, float beats)
        {
            int lengthOfOneBeatInMs = (int)(60000 / bpm);
            return GetSampleCountFromMilliseconds((int)(lengthOfOneBeatInMs * beats));
        }

        public int GetPostbeatLengthInSamplesBasedOnBeats()
        {
            return GetLengthInSamplesBasedOnBeats(Bpm, PostBeats);
        }

        public int GetPrebeatLengthInSamplesBasedOnBeats()
        {
            return GetLengthInSamplesBasedOnBeats(Bpm, PreBeats);
        }

        // int32 is sufficient for approx. 745 minutes of audio at a sample rate of 48000 Hz
        public static int CalculateTotalLengthInSamples(int lengthOfWaveformDataInBytes, int bitsPerSample, int channelCount)   
        {
            if (lengthOfWaveformDataInBytes > 0 && bitsPerSample > 0 && channelCount > 0)
            {
                // 4 bytes per sample (16 bit stereo)
                // 2 bytes per sample (16 bit sound mono)
                // 1 byte per sample (8 bit sound mono)
                int sampleCount = (lengthOfWaveformDataInBytes / (bitsPerSample / 8)) / channelCount;

                return sampleCount;
            }
            else
            {
                return 0;
            }
        }

        // checks if the file exists, and if it does reads in the wave header and updates all
        // related member variables based on the header information.
        public bool DoUpdateMembersBasedOnWaveHeader(string fullPathToAudioFile, out string errorMessage)
        {
            bool successResult = false;

            if (fullPathToAudioFile != null && fullPathToAudioFile.Length > 0)
            {
                int channelCount;
                int sampleRate;
                int bitsPerSample;
                int lengthOfWaveformBlockInBytes;
                Int64 byteIndexOfWaveformDataWithinAudioFile;

                string normalizedPathToAudioFile = fullPathToAudioFile.Replace('/', Path.DirectorySeparatorChar);
                normalizedPathToAudioFile = normalizedPathToAudioFile.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                if (File.Exists(normalizedPathToAudioFile))
                {
                    // Freshly copied / changed files were already being locked for a short time, maybe by the Anti Virus software.
                    // That's why we do some sort of busy waiting to retry opening the files.
                    Stream stream = null;
                    int numberOfTries = 0;
                    while (stream == null && numberOfTries < 100)
                    {
                        try
                        {
                            stream = File.Open(normalizedPathToAudioFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        catch (System.IO.IOException ex)
                        {
                            errorMessage = ex.ToString() + "   numberOfTries=" + numberOfTries;
                            System.Threading.Thread.Sleep(50);
                        }
                        numberOfTries++;
                    }

                    if (stream != null)
                    {
                        if (AudioData.ReadWaveHeader(stream, out channelCount, out sampleRate, out bitsPerSample, out lengthOfWaveformBlockInBytes, out byteIndexOfWaveformDataWithinAudioFile) == true)
                        {
                            ChannelCount = channelCount;
                            SampleRate = sampleRate;
                            LengthOfWaveformDataInBytes = lengthOfWaveformBlockInBytes;                            
                            BitsPerSample = bitsPerSample;
                            ByteIndexOfWaveformDataWithinAudioFile = byteIndexOfWaveformDataWithinAudioFile;
                            TotalLengthInSamples = CalculateTotalLengthInSamples(lengthOfWaveformBlockInBytes, bitsPerSample, channelCount);

                            errorMessage = "";
                            successResult = true;
                        }
                        else
                        {
                            errorMessage = "ERROR: file '" + normalizedPathToAudioFile + "' contains an unsupported format. Please uncheck the 'read out file header' checkbox and enter the format values (samplerate, bits, length in samples) manually.";
                        }

                        stream.Close();
                        return successResult;
                    }
                    else
                    {
                        errorMessage = "ERROR: audio file '" + normalizedPathToAudioFile + "' could not be opened. ";
                        return false;
                    }
                }
            }

            errorMessage = "ERROR: audio file '" + fullPathToAudioFile + "' could not be found. Please make sure that all audio files reside within a subfolder of your project directory";
            return false;
        }



        /// <summary>
        /// Looks for a RIFF WAVE chunk like 'fmt ' or 'data' and returns true if position was found.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static bool SeekChunkInWaveHeader(ref BinaryReader reader, string chunk)
        {
            string lastFourChars = "";

            if (chunk.Length != 4)
            {
                return false;
            }
            else
            {
                System.Collections.Generic.Queue<byte> byteQueue = new Queue<byte>(4);



                try
                {
                    while (reader.BaseStream.CanRead)
                    {
                        byte currentByte = 0;

                        do
                        {
                            currentByte = reader.ReadByte();
                            byteQueue.Enqueue(currentByte);

                            if (byteQueue.Count > 4)
                                byteQueue.Dequeue();
                        }
                        while (currentByte != chunk[3]);    // check the last index of the chunk string, then look back if the rest matches, too

                        lastFourChars = ASCIIEncoding.ASCII.GetString(byteQueue.ToArray());

                        if (lastFourChars.Equals(chunk))
                        {
                            return true;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return false;
        }


        // returns true if successful, false otherwise
        public static bool ReadWaveHeader(Stream stream, out int outChannelCount, out int outSampleRate, out int outBitsPerSample, out int outLengthOfWaveformDatablockInBytes, out Int64 outBytePositionOfWaveformData)
        {
            outChannelCount = 0;
            outBitsPerSample = 0;
            outSampleRate = 0;
            outLengthOfWaveformDatablockInBytes = 0;
            outBytePositionOfWaveformData = 0;

            BinaryReader reader = new BinaryReader(stream);

            // RIFF header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
            {
                // "Specified stream is not a wave file.";
                reader.Close();
                return false;
            }

            reader.ReadInt32();  // int riff_chunk_size

            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
            {
                // "Specified stream is not a wave file."
                reader.Close();
                return false;
            }

            // keep looking for the Format Chunk
               
            try
            {
                if (SeekChunkInWaveHeader(ref reader, "fmt ") == false)
                {
                    Console.WriteLine(".wave file corrupt! format-chunk not found.");
                    reader.Close();
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                // we most likely reached the end. Cannot parse!
                // TODO: This may happen if the data chunk lies before the fmt chunk, which is allowed but hopefully never happens! Implement?
                Console.WriteLine(ex.ToString());
                reader.Close();
                return false;
            }

            Int64 fmt_chunk_pos = reader.BaseStream.Position;

            int fmt_chunk_size = reader.ReadInt32();   // int format_chunk_size
            reader.ReadInt16();   // int audio_format
            int num_channels = reader.ReadInt16();
            int sample_rate = reader.ReadInt32();
            reader.ReadInt32();     // int byte_rate
            reader.ReadInt16();     // int block_align
            int bits_per_sample = reader.ReadInt16();

            //Int64 debugPosition = reader.BaseStream.Position;

            Int64 seekPosition = fmt_chunk_pos + fmt_chunk_size + 4;

            reader.BaseStream.Seek(seekPosition, SeekOrigin.Begin);               

            if (!SeekChunkInWaveHeader(ref reader, "data"))
            {
                // we missed the data chunk. Last chance: It is located before the "fmt " chunk, which is very unusual, but not totally prohibited.
                // So we start looking again from the beginning.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                if (!SeekChunkInWaveHeader(ref reader, "data"))
                {
                    Console.WriteLine("wave file corrupt! no 'data' chunk found!");

                    reader.Close();
                    return false;
                }
            }
             
            //////////////////////////////////////////////////////////////////////////
            // found the data chunk:            

            int dataBlockLengthInBytes = stream.ReadByte() + stream.ReadByte() * 256 + stream.ReadByte() * 65536 + stream.ReadByte() * 16777216;

            outLengthOfWaveformDatablockInBytes = dataBlockLengthInBytes;
            outBytePositionOfWaveformData = stream.Position;

            outChannelCount = num_channels;
            outBitsPerSample = bits_per_sample;
            outSampleRate = sample_rate;

            reader.Close();
            return true;

        }


        // Loads a wave/riff audio file.
        public static byte[] LoadWaveformDataToByteArray(string fullFilePath, long byteIndexOfWaveformDataWithinAudioFile, int lengthOfWaveformDataInBytes)
        {
            Stream stream = null;
            int numberOfTries = 0;
            while (stream == null && numberOfTries < 100)
            {
               
                // TODO: do we need this?
                string normalizedPathToAudioFile = fullFilePath.Replace('/', Path.DirectorySeparatorChar);
                normalizedPathToAudioFile = normalizedPathToAudioFile.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                try
                {
                    stream = File.Open(normalizedPathToAudioFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (System.IO.IOException ex)
                {
                    string errorMessage = ex.ToString() + "   numberOfTries=" + numberOfTries;
                    Console.WriteLine(errorMessage);
                    System.Threading.Thread.Sleep(50);
                }
                numberOfTries++;
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            byte[] resultArray = null;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header                
                try
                {
                    reader.BaseStream.Position = byteIndexOfWaveformDataWithinAudioFile;
                    resultArray = reader.ReadBytes(lengthOfWaveformDataInBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception reading Audio Data! e=" + e.ToString() + "  " + e.Message, psai.net.LogLevel.errors);
                    //MainForm.Instance.WriteLineToLogWindow();
                }
            }
            stream.Close();
            return resultArray;
        }




        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
