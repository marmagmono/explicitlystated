using System;

namespace ExplicitlyStated.StateMachine.Impl
{
    public delegate TMachineState TryMatchDelegate<TMachineState, TMachineEvent>(TMachineState s, TMachineEvent e);

    internal readonly struct SimpleDispatchEntry<TEntry> where TEntry : class
    {
        public readonly Type Type;
        public readonly TEntry Entry;

        public SimpleDispatchEntry(Type type, TEntry entry)
        {
            Type = type;
            Entry = entry;
        }
    }

    internal struct SimpleDispatch<TEntry> where TEntry : class
    {
        private const int SizeIncrease = 3;

        private int currentEntryIdx;
        private SimpleDispatchEntry<TEntry>[] dispatchEntries;

        public SimpleDispatch(int numDispatchEntries)
        {
            this.dispatchEntries = new SimpleDispatchEntry<TEntry>[numDispatchEntries];
            this.currentEntryIdx = -1;
        }

        public TEntry FindEntryOrDefault(Type type)
        {
            foreach (var dispatchEntry in this.dispatchEntries)
            {
                if (dispatchEntry.Type == type)
                {
                    return dispatchEntry.Entry;
                }
            }

            return null;
        }

        public void AddEntry(in SimpleDispatchEntry<TEntry> entry)
        {
            ++currentEntryIdx;
            if (currentEntryIdx >= this.dispatchEntries.Length)
            {
                var oldEntries = this.dispatchEntries;
                this.dispatchEntries = new SimpleDispatchEntry<TEntry>[oldEntries.Length + SizeIncrease];
                Array.Copy(oldEntries, 0, this.dispatchEntries, 0, oldEntries.Length);
            }

            this.dispatchEntries[currentEntryIdx] = entry;
        }
    }
}
