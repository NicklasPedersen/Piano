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
            PianoString(out string[] five, out string[] four, out string[] three, arr);
            Note[] firstNotes = new Note[39*26];
            Note[] secondNotes = new Note[39*26];
            Note[] thirdNotes = new Note[39*26];
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = five[i / 26][i%26].ToString();
                firstNotes[i] = new Note(str, 100, 20, 6);
            }
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = four[i / 26][i % 26].ToString();
                secondNotes[i] = new Note(str, 100, 20, 5);
            }
            for (int i = 0; i < firstNotes.Length; i++)
            {
                string str = three[i / 26][i % 26].ToString();
                thirdNotes[i] = new Note(str, 100, 20, 4);
            }
            Thread t5 = new Thread(new ThreadStart(delegate { PlayNote(firstNotes); }));
            Thread t4 = new Thread(new ThreadStart(delegate { PlayNote(secondNotes); }));
            Thread t3 = new Thread(new ThreadStart(delegate { PlayNote(thirdNotes); }));
            MidiPlayer.OpenMidi();
            //t5.Start();
            //t4.Start();
            //t3.Start();
            Note[,] input = new Note[3,39*26];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 39*26; j++)
                {
                    input[i, j] = i == 0
                        ? firstNotes[j]
                        : (i == 1 ? secondNotes[j] : thirdNotes[j]);
                }
            }

            PlayNote(input, 130);
            Console.Read();
            MidiPlayer.CloseMidi();
        }

        static void PlayNote(Note[,] notes, int sleep)
        {
            for (int i = 0; i < notes.GetLength(1); i++)
            {
                int nextTick = Environment.TickCount + sleep;
                Console.Write("Current notes: ");
                for (int j = 0; j < 3; j++)
                {
                    Console.Write((notes[j,i].noteChar!='-'? notes[j, i].note :"-") + " ");
                    if (notes[j, i].noteChar != '-')
                    {
                        MidiPlayer.Play(new NoteOn(0, (byte)notes[j,i].noteValue, notes[j,i].note, 100));
                    }
                }
                Console.WriteLine();
                //Thread.Sleep(Math.Max(nextTick - Environment.TickCount, 0));
                Thread.Sleep(130);
                for (int j = 0; j < 3; j++)
                {
                    if (notes[j, i].noteChar != '-')
                    {
                        MidiPlayer.Play(new NoteOff(0, (byte)notes[j, i].noteValue, notes[j, i].note, 100));
                    }
                }
            }
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
