using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuffixTree
{
    public class NodeChildsCollection<T> : ICollection<T>, IEnumerable<T>
    {
        private int size = 0;
        private bool busy = false;
        private T[] items;

        public NodeChildsCollection()
        {
            items = new T[4];
        }

        public int Count
        {
            get
            {
                return size;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            if (busy)
            {
                SpinWait spin = new SpinWait();
                while (busy)
                    spin.SpinOnce();
            }

            busy = true;

            if (size == items.Length - 1)
            {
                Array.Resize(ref items, items.Length + 4);
            }
            items[size++] = item;

            busy = false;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            busy = true;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            for (int i = 0; i < size; i++)
            {
                if (c.Equals(items[i], item))
                {
                    busy = false;
                    return true;
                }
            }
            busy = false;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Exists(Predicate<T> match)
        {
            return FindIndex(match) != -1;
        }
        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, size, match);
        }

        public T Find(Predicate<T> match)
        {
            for (int i = 0; i < size; i++)
            {
                if (match(items[i]))
                {
                    return items[i];
                }
            }
            return default(T);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            busy = true;
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(items[i]))
                {
                    busy = false;
                    return i;
                }
            }
            busy = false;
            return -1;
        }
    }
}
