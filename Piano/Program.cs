using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toub.Sound.Midi;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using UI;

namespace Piano
{
    class Program
    {
        private enum Menu
        {
            PlaySongs,
            Settings,
            Exit
        }
        private enum Songs
        {
            SampleSong1,
            SampleSong2,
            SampleSong3,
            SampleSong4,
            SampleSong5,
            SampleSong6,
            Exit
        }
        private enum Settings
        {
            DisplayNotes = 1,
            Exit
        }

        private enum Sliders
        {
            Velocity
        }
        static void Main(string[] args)
        {
            int choice;
            bool[] setting = new bool[(int)Settings.Exit - (int)Settings.DisplayNotes];
            Slider[] t = { new Slider(0, 127, 100) };
            setting[0] = true;
            do
            {
                choice = UIGeneric.UI("Select an option", typeof(Menu), ConsoleColor.DarkRed, ConsoleColor.White,
                    ConsoleColor.Cyan);
                switch (choice)
                {
                    case (int)Menu.PlaySongs:
                        PlaySongs(setting);
                        break;
                    case (int)Menu.Settings:
                        Setting(ref setting, ref t);
                        break;
                    default:
                        break;
                }

            } while (choice != (int)Menu.Exit);

        }

        private static void Setting(ref bool[] settings, ref Slider[] sliders)
        {
            int cursor = 0;
            Tuple<int, ConsoleKey> choice;
            do
            {
                choice = UIGeneric.UISlidersAndBools("Select an option", ref cursor, typeof(Sliders), typeof(Settings), sliders, settings, ConsoleColor.DarkRed,
                    ConsoleColor.White,
                    ConsoleColor.Cyan);
                if (choice.Item1 < (int)Settings.DisplayNotes)
                {
                    if (choice.Item2 == ConsoleKey.RightArrow) sliders[choice.Item1].Up();
                    else sliders[choice.Item1].Down();
                }
                else if (choice.Item1 != (int)Settings.Exit)
                    settings[choice.Item1 - (int)Settings.DisplayNotes] ^= true;

            } while (choice.Item1 != (int)Settings.Exit);
        }
        private static void PlaySongs(bool[] settings)
        {
            MidiOutCaps myCaps = new MidiOutCaps();
            int handle = 0;
            int deviceNumber = 0;
            midiOutGetDevCaps(0, ref myCaps,
                (uint)Marshal.SizeOf(myCaps));
            midiOutOpen(ref handle, deviceNumber,
                null, 0, 0);
            int choice;
            do
            {
                choice = UIGeneric.UI("Select a song", typeof(Songs), ConsoleColor.DarkRed, ConsoleColor.White,
                    ConsoleColor.Cyan);
                if (choice != (int)Songs.Exit)
                {
                    Note[,] note;
                    string[] arr = PlayTheNotes(choice, out note);
                    if (choice == 5)
                        NoteIntepereter(handle, PianoString(arr, true, 3), settings[(int)Settings.DisplayNotes - (int)Settings.DisplayNotes]);
                    else
                        PlayNote(note, handle, settings[(int)Settings.DisplayNotes - (int)Settings.DisplayNotes]);

                    Console.WriteLine("Press a button to continue... ");
                    Console.ReadKey(true);
                }

            } while (choice != (int)Songs.Exit);

            midiOutClose(handle);
        }
        /// <summary>
        /// Trims the note from input and assigns it to an octave of 11.
        /// The format is {octave}|{notes}|{nothing}
        /// or {octave}|{notes} but this is not preferred
        /// {notes} usually contains 26 notes 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string[] TrimNotes(string[] input)
        {
            // 11 is the amount of octaves, midi can handle 128 notes, 128/12=10,6... octaves
            string[] output = new string[11];
            for (int i = 0; i < output.Length; i++)
                output[i] = "";
            int blocks = 1;
            // loops through each line in the song string array
            foreach (var t in input)
            {
                // checks if it is empty therefore a new cluster of notes
                if (t != "")
                {
                    // if not assigns note number in output to the second split in the format {number}|{notes}|{nothing}
                    // or {number}|{notes} but this is not preferred
                    string[] split = t.Split('|');
                    output[Convert.ToInt32(split[0]) + 1] += split[1];
                    continue;
                }
                // then it pads the rest of the notes to the same length
                for (int j = 0; j < output.Length; j++)
                {
                    if (output[j].Length < blocks * 26)
                    {
                        output[j] += "--------------------------";
                    }
                }
                blocks++;
            }
            // the last padding for the last loop that got missed
            for (int j = 0; j < output.Length; j++)
            {
                if (output[j].Length < blocks * 26)
                {
                    output[j] += "--------------------------";
                }
            }

            return output;
        }
        private static string[] PlayTheNotes(int choice, out Note[,] output)
        {
            string[] arr = File.ReadAllLines(@"..\..\" + Enum.GetName(typeof(Songs), choice) + ".txt");
            string[] trimmed = TrimNotes(arr);
            int[] lengths = new int[11];
            for (int i = 0; i < 11; i++)
            {
                lengths[i] = trimmed[i].Length;
            }
            Note[,] notes = new Note[trimmed.GetLength(0), trimmed[0].Length];
            for (int i = 0; i < notes.GetLength(0); i++)
            {
                for (int j = 0; j < trimmed[0].Length; j++)
                {
                    notes[i, j] = new Note(Convert.ToString(trimmed[i][j]), 100, 20, i - 1);
                }
            }

            Note[,] input = new Note[3, trimmed[0].Length];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < trimmed[0].Length; j++)
                {
                    input[i, j] = notes[i + 4, j];
                }
            }

            output = input;
            return arr;
        }

        [DllImport("winmm.dll")]
        private static extern long MciSendString(string command,
            StringBuilder returnValue, int returnLength, IntPtr winHandle);

        [DllImport("winmm.dll")]
        private static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern int midiOutGetDevCaps(int uDeviceID,
            ref MidiOutCaps lpMidiOutCaps, uint cbMidiOutCaps);

        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(ref int handle,
            int deviceID, MidiCallBack proc, int instance, int flags);

        [DllImport("winmm.dll")]
        protected static extern int midiOutShortMsg(int handle,
            int message);

        [DllImport("winmm.dll")]
        protected static extern int midiOutClose(int handle);

        private delegate void MidiCallBack(int handle, int msg,
            int instance, int param1, int param2);

        static string WriteNotes(string note)
        {
            string output = "";
            if (!Note.noteNumbers.ContainsKey(note))
            {
                for (int i = 0; i < Note.noteNumbers.Count; i++)
                {
                    output += "  -";
                }
                return output;
            }

            for (int i = 0; i < Note.noteNumbers.Count; i++)
            {
                output += Note.noteNumbers[note] != i ? "  -" : note.PadLeft(3);
            }
            return output;

        }

        static void PlayNote(Note[,] notes, int handle, bool write)
        {
            for (int i = 0; i < notes.GetLength(1); i++)
            {
                int nextTick = Environment.TickCount + 100;
                string writeString = "";
                for (int j = 0; j < 3; j++)
                {
                    writeString += notes[j, i].noteValue + WriteNotes(notes[j, i].theNote) + "| ";
                    if (notes[j, i].noteChar != '-')
                    {

                        midiOutShortMsg(handle, notes[j, i].ToMessage());
                    }
                }
                if (write)
                {
                    Console.WriteLine("Current notes: " + writeString);
                }

                //Thread.Sleep(Math.Max(nextTick - Environment.TickCount, 0));
                Thread.Sleep(100);
            }
        }

        public static void NoteIntepereter(int handle, string[][] notes, bool write)
        {
            bool[][] noteOn = new bool[notes.Length][];
            for (int i = 0; i < noteOn.GetLength(0); i++)
            {
                noteOn[i] = new bool[12];
            }
            for (int i = 0; i < notes[0].GetLength(0); i++)
            {
                for (int j = 0; j < notes[0][i].Length; j++)
                {
                    if (write) Console.Write("Current notes: ");
                    for (int k = 0; k < notes.GetLength(0); k++)
                    {
                        if (write) Console.Write(k + 3 + WriteNotes(ToNote(notes[k][i][j])) + "| ");
                        if (notes[k][i][j] != '-')
                        {
                            int noteNum = Note.noteNumbers[ToNote(notes[k][i][j])];
                            int q = (k + 4) * 12 + noteNum;
                            int msg = (100 << 16) + (q << 8) + (noteOn[k][noteNum] ? 0x80 : 0x90);
                            noteOn[k][noteNum] ^= true;
                            midiOutShortMsg(handle, msg);
                        }
                    }
                    if (write) Console.WriteLine();
                    Thread.Sleep(130 / 2);
                }
            }
        }

        public static string ToNote(char noteTrim)
        {
            return noteTrim.ToString() != noteTrim.ToString().ToUpper()
                ? noteTrim.ToString().ToUpper()
                : noteTrim.ToString().ToUpper() + "#";
        }
        public static string[][] PianoString(string[] input, bool isEnd, int offset)
        {

            string[][] notes = new string[3][];
            if (!isEnd)
            {
                PianoString(out notes[0], out notes[1], out notes[2], input);
                return notes;
            }

            int blocks = 1 + input.Count(t => t == "");
            for (int i = 0; i < 3; i++)
            {
                notes[i] = new string[blocks];
                for (int j = 0; j < blocks; j++)
                {
                    notes[i][j] = "----------------------------------------------------";
                }
            }
            int index = 0;
            foreach (string variable in input)
            {
                if (variable != "")
                {
                    notes[Convert.ToInt32(variable.Substring(0, 1)) - offset][index] = variable.Substring(2, 26 * 2);
                }
                else
                {
                    index++;
                }
            }

            return notes;
        }
        public static void PianoString(out string[] five, out string[] four, out string[] three, string input)
        {
            string[] s = input.Split('\n');
            int blocks = 1;
            foreach (var t in s)
                if (t == "\r")
                    blocks++;
            five = new string[blocks];
            four = new string[blocks];
            three = new string[blocks];
            for (int i = 0; i < blocks; i++)
            {
                five[i] = "--------------------------";
            }
            for (int i = 0; i < blocks; i++)
            {
                four[i] = "--------------------------";
            }
            for (int i = 0; i < blocks; i++)
            {
                three[i] = "--------------------------";
            }

            int index = 0;
            foreach (var VARIABLE in s)
            {
                if (VARIABLE != "\r")
                {
                    switch (VARIABLE.Substring(0, 1))
                    {
                        case "5":
                            five[index] = VARIABLE.Substring(2, 26);
                            break;
                        case "4":
                            four[index] = VARIABLE.Substring(2, 26);
                            break;
                        case "3":
                            three[index] = VARIABLE.Substring(2, 26);
                            break;
                    }
                }
                else
                {
                    index++;
                }
            }
        }
        public static void PianoString(out string[] five, out string[] four, out string[] three, string[] input)
        {
            int blocks = 1;
            foreach (var t in input)
                if (t == "")
                    blocks++;

            five = new string[blocks];
            four = new string[blocks];
            three = new string[blocks];
            for (int i = 0; i < blocks; i++)
            {
                five[i] = "--------------------------";
            }
            for (int i = 0; i < blocks; i++)
            {
                four[i] = "--------------------------";
            }
            for (int i = 0; i < blocks; i++)
            {
                three[i] = "--------------------------";
            }

            int index = 0;
            foreach (string variable in input)
            {
                if (variable != "")
                {
                    switch (variable.Substring(0, 1))
                    {
                        case "5":
                            five[index] = variable.Substring(2, 26);
                            break;
                        case "4":
                            four[index] = variable.Substring(2, 26);
                            break;
                        case "3":
                            three[index] = variable.Substring(2, 26);
                            break;
                    }
                }
                else
                {
                    index++;
                }
            }
        }
    }
    class Note
    {
        public static readonly Dictionary<string, int> noteNumbers = new Dictionary<string, int>
        {
            {"C", 0},
            {"C#", 1},
            {"D", 2},
            {"D#", 3},
            {"E", 4},
            {"F", 5},
            {"F#", 6},
            {"G", 7},
            {"G#", 8},
            {"A", 9},
            {"A#", 10},
            {"B", 11},
        };
        public string note;
        public char noteChar;
        public int noteValue;
        public bool sharp;
        public int hardness;
        public int delay;
        public string theNote;
        public bool end;

        public Note(string note, int hardness, int delay, int noteValue)
        {
            noteChar = note[0];
            this.hardness = hardness;
            this.delay = delay;
            sharp = note.ToUpper() == note;
            this.noteValue = noteValue;
            this.note = note.ToUpper() + (sharp ? "#" : "") + noteValue;
            theNote = note.ToUpper() + (sharp ? "#" : "");
            end = false;

        }

        public Note(string note, int noteValue, bool sharp, int hardness, int delay)
        {
            this.note = note + (sharp ? "#" : "") + noteValue;
            this.noteValue = noteValue;
            this.sharp = sharp;
            this.hardness = hardness;
            this.delay = delay;
            theNote = note.ToUpper() + (sharp ? "#" : "");
        }

        public int ToMessage()
        {
            return (hardness << 16) + (NoteToInt() << 8) + (end ? 0x80 : 0x90);
        }

        public int NoteToInt()
        {
            return (noteValue + 1) * 12 + noteNumbers[theNote];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MidiOutCaps
    {
        public ushort wMid;
        public ushort wPid;
        public ushort vDriverVersion;

        [MarshalAs(UnmanagedType.ByValTStr,
            SizeConst = 32)]
        public string szPname;

        public ushort wTechnology;
        public ushort wVoices;
        public ushort wNotes;
        public ushort wChannelMask;
        public ushort dwSupport;
    }
}