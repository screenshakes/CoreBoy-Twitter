using System.Collections.Generic;
using CoreBoy.memory;

namespace CoreBoy.Twitter
{
    public static class MemoryEditor
    {
        static Mmu mmu;
        static Dictionary<int, int> edits;
        static Dictionary<int, int> previous;

        public static void Initialize(Mmu mmu)
        {
            edits    = new Dictionary<int, int>();
            previous = new Dictionary<int, int>();
            MemoryEditor.mmu = mmu;
        }

        public static void Update()
        {
            if(Initialized)
            foreach(var e in edits)
                mmu.SetByte(e.Key, e.Value);
        }

        public static void Edit((int, int) edit)
        {
            Edit(edit.Item1, edit.Item2);
        }

        public static void Edit(int address, int value)
        {
            var current = mmu.GetByte(address);
            if(!previous.ContainsKey(address) || current != value)
            {
                previous.Add(address, current);
                mmu.SetByte(address, value);
            }
        }

        public static void Add((int, int) edit)
        {
            Add(edit.Item1, edit.Item2);
        }

        public static void Add(int address, int value)
        {
            edits.Add(address, value);
            var current = mmu.GetByte(address);
            if(!previous.ContainsKey(address) || current != value)
                previous.Add(address, current);
        }

        public static void Remove(int address)
        {
            if(edits.ContainsKey(address)) edits.Remove(address);
            if(previous.ContainsKey(address))
            {
                mmu.SetByte(address, previous[address]);
                previous.Remove(address);
            }
        }

        public static bool Initialized => mmu != null;
    }
}