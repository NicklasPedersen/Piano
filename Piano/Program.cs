using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toub.Sound.Midi;
using System.Threading;
using System.IO;

namespace Piano
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] arr = File.ReadAllLines(@"..\..\SampleSong.txt");
            string[] first;
            string[] second;
            string[] third;
            PianoString(out first, out second, out third, arr);
            Note[] firstNotes = new Note[39*26];
            Note[] secondNotes = new Note[39*26];
            Note[] thirdNotes = new Note[39*26];
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = first[i / 26][i%26].ToString();
                firstNotes[i] = new Note(str, 100, 20, 5);
            }
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = second[i / 26][i % 26].ToString();
                secondNotes[i] = new Note(str, 100, 20, 5);
            }
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = third[i / 26][i % 26].ToString();
                thirdNotes[i] = new Note(str, 100, 20, 5);
            }
            Thread t5 = new Thread(new ThreadStart(delegate { PlayNote(firstNotes); }));
            Thread t4 = new Thread(new ThreadStart(delegate { PlayNote(secondNotes); }));
            Thread t3 = new Thread(new ThreadStart(delegate { PlayNote(thirdNotes); }));
            MidiPlayer.OpenMidi();
            t5.Start();
            t4.Start();
            t3.Start();
            Console.Read();
            MidiPlayer.CloseMidi();
        }

        static void PlayNote(Note[] notes)
        {
            int sleep = 130;
            for (int i = 0; i < notes.Length; i++)
            {
                
                int nextTick = Environment.TickCount + sleep;
                if (notes[i].noteChar == '-')
                {
                    Thread.Sleep(Math.Max(nextTick-Environment.TickCount,0));
                }
                else
                {
                    MidiPlayer.Play(new NoteOn(0,(byte)notes[i].noteValue,notes[i].note,100));
                    Thread.Sleep(Math.Max(nextTick - Environment.TickCount, 0));
                    MidiPlayer.Play(new NoteOff(0, (byte)notes[i].noteValue, notes[i].note, 100));
                }
            }
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
            foreach (var VARIABLE in input)
            {
                if (VARIABLE != "")
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
    }

    class Note
    {
        public string note;
        public char noteChar;
        public int noteValue;
        public bool sharp;
        public int hardness;
        public int delay;

        public Note(string note, int hardness, int delay, int noteValue)
        {
            noteChar = note[0];
            this.hardness = hardness;
            this.delay = delay;
            sharp = note.ToUpper() == note;
            this.noteValue = noteValue;
            this.note = note.ToUpper() + (sharp ? "#" : "") + noteValue;
        }

        public Note(string note, int noteValue, bool sharp, int hardness, int delay)
        {
            this.note = note + (sharp ? "#" : "") + noteValue;
            this.noteValue = noteValue;
            this.sharp = sharp;
            this.hardness = hardness;
            this.delay = delay;
        }
    }
}
