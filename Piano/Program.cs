using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toub.Sound.Midi;
using System.Threading;
using System.Threading.Tasks;

namespace Piano
{
    class Program
    {
        static void Main(string[] args)
        {
            string s =
                @"5|--d---------------d-------|
4|dd--a--G-g-f-dfgcc--a--G-g|

5|--------d---------------d-|
4|-f-dfg----a--G-g-f-dfg----|
3|------bb--------------AA--|

5|--------------d-----------|
4|a--G-g-f-dfgdd--a--G-g-f-d|

5|----d---------------d-----|
4|fgcc--a--G-g-f-dfg----a--G|
3|------------------bb------|

5|----------d---------------|
4|-g-f-dfg----a--G-g-f-dfgdd|
3|--------AA----------------|

5|d---------------d---------|
4|--a--G-g-f-dfgcc--a--G-g-f|

5|------d---------------d---|
4|-dfg----a--G-g-f-dfg----a-|
3|----bb--------------AA----|

5|------------d-------------|
4|-G-g-f-dfgdd--a--G-g-f-dfg|

5|--d---------------d-------|
4|cc--a--G-g-f-dfg----a--G-g|
3|----------------bb--------|

5|--------d-----------------|
4|-f-dfg----a--G-g-f-dfgf-ff|
3|------AA------------------|

4|-f-f-d-d--d-ffff-g-G-gfdfg|

5|-------------c----d-d-d-dc|
4|--f-ff-g-G-a---a-------a--|

4|--------a-aa-a-a-g-g----a-|

5|---------d----d-------c---|
4|aa-a-g-a---ag---a-g-f---a-|

5|-----------c--------------|
4|g-f-d-ef-a----------------|

4|--fdfgGgfdGgfdfg--------G-|

5|-c-------------c--C-------|
4|a--aGgfdef-g-G------G-Ggfg|

4|--------------f-e---d---e-|
3|--------f-g-a-------------|

4|--f---g---e---a-------aGgF|

4|feDdC-------D-------------|

4|--fdfgGgfdGgfdeg--------G-|

5|--c-------------c-C-------|
4|a---aGgfdef-g-a-----G-Ggfg|

4|------------f-e---d---e---|
3|------f-g-a---------------|

4|f---g---e---a-------aGgFfe|

4|DdC-------D---------------|
3|--------------------b-----|

4|------f---e-------d-------|

4|f-------------------------|

4|------------------f---e---|
3|------b-------------------|

4|----d-----------d---------|

3|----------------------b---|

4|--------f---e-------d-----|

4|--f-----------------------|

4|--------------------f---e-|
3|--------b-----------------|

4|------d-------d-----------|

5|----d---------------d-----|
4|--dd--a--G-g-f-dfgdd--a--G|

5|----------d---------------|
4|-g-f-dfgCC--a--G-g-f-dfgcc|

5|d---------------d---------|
4|--a--G-g-f-dfgdd--a--G-g-f|

5|------d---------------d---|
4|-dfgdd--a--G-g-f-dfgCC--a-|

5|------------d-------------|
4|-G-g-f-dfgcc--a--G-g-f-dfg|";
            string[] first;
            string[] second;
            string[] third;
            PianoString(out first, out second, out third, s);
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
