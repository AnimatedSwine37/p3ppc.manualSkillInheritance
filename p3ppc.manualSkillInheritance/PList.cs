using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.manualSkillInheritance
{
    public unsafe class PList<T> : IList<T>
    {
        //private static AddDelegate _add;

        public static void Initialise(IStartupScanner startupScanner, IReloadedHooks hooks)
        {
            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 48 89 CE 49 63 D8", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find ListAddElement, won't be able to add stuff to native lists");
                    return;
                }
                Utils.LogDebug($"Found ListAddElement at 0x{result.Offset + Utils.BaseAddress:X}");

                //_add = hooks.CreateWrapper<AddDelegate>(result.Offset + Utils.BaseAddress, out _);
            });
        }

        private InternalList<T>* _list;

        public int Count => _list->Length;

        public bool IsReadOnly => throw new NotImplementedException();

        public T this[int index]
        {
            get
            {
                var entry = GetEntryAt(index);
                if (entry == null) return default(T);
                return entry->Value;
            }
            set
            {
                var entry = GetEntryAt(index);
                if (entry != null)
                    entry->Value = value;
            }
        }

        public PList(nuint listAddress)
        {
            _list = (InternalList<T>*)listAddress;
        }

        private ListEntry<T>* GetEntryAt(int index)
        {
            var entry = _list->FirstEntry;
            int i = 0;
            while (entry != null)
            {
                if (i == index)
                    return entry;
                i++;
                entry = entry->NextEntry;
            }
            return null;

        }
        
        public int IndexOf(T item)
        {
            var entry = _list->FirstEntry;
            int i = 0;
            while (entry != null)
            {
                if (entry->Value != null && entry->Value!.Equals(item))
                    return i;
                i++;
                entry = entry->NextEntry;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
            //var entry = _list->FirstEntry;
            //while (entry != null)
            //{
            //    yield return entry->Value;
            //    entry = entry->NextEntry;
            //}
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private struct InternalList<T>
        {
            fixed byte unk[12];
            internal int Length;
            internal ListEntry<T>* FirstEntry;
        }

        private struct ListEntry<T>
        {
            fixed byte unk[24];
            internal ListEntry<T>* NextEntry;
            fixed byte unk2[8];
            internal T Value;
        }

        //[Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
        //private delegate int AddDelegate(InternalList<T>* list, uint maxLength, int value);
    }
}
