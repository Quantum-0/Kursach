using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SuffixTree
{
    struct WordCountPair : IComparable
    {
        public string Word;
        public long Count;

        public int CompareTo(object obj)
        {
            if (obj is WordCountPair)
                return -Count.CompareTo(((WordCountPair)obj).Count);
            else
                throw new InvalidOperationException();
        }

        public override string ToString()
        {
            return Word + " - " + Count;
        }
    }

    class NotSuffixTree : IDisposable
    {
        Node Root;
        public long DifferentWords;
        ConcurrentQueue<string> WordsToAdd = new ConcurrentQueue<string>();
        List<Thread> Adders = new List<Thread>();
        bool ThreadWorks = true;
        public int QueueLength { get { return WordsToAdd.Count; } }

        private static void WorkWithWords(object oTree)
        {
            NotSuffixTree Tree = (NotSuffixTree)oTree;
            while (Tree != null)
            {
                if (Tree.WordsToAdd.Count > 0)
                {
                    Tree.ThreadWorks = true;
                    while (Tree.WordsToAdd.Count > 0)
                    {
                        string Word;
                        while (!Tree.WordsToAdd.TryDequeue(out Word)) ;
                        if (Word != null)
                            Tree._Increment(Word);
                    }
                    Tree.ThreadWorks = false;
                }
                else
                    Thread.Sleep(50);
            }
        }

        class Node
        {
            public char Char;
            public long Count;
            public List<Node> Childs;

            public Node(Char Char)
            {
                this.Char = Char;
                Count = 0;
                Childs = new List<Node>();
            }

            public override string ToString()
            {
                return Char + " (" + Childs.Count + ")";
            }
        }

        public NotSuffixTree()
        {
            Root = new Node('\0');

            //for (int i = 0; i < Environment.ProcessorCount - 1; i++)
                Adders.Add(new Thread(WorkWithWords));

            Adders.ForEach(a => a.Start(this));
        }

        public void Increment(string Word)
        {
            WordsToAdd.Enqueue(Word);
        }

        private void _Increment(string Word)
        {
            Node Current = Root;
            var NotInitialized = false;

            for (int i = 0; i < Word.Length; i++)
            {
                if (NotInitialized)
                {
                    var New = new Node(Word[i]);
                    Current.Childs.Add(New);
                    Current = New;
                }
                else
                {

                    if (!Current.Childs.Exists(n => n.Char == Word[i]))
                    {
                        NotInitialized = true;
                        var New = new Node(Word[i]);
                        Current.Childs.Add(New);
                        Current = New;
                    }
                    else
                    {
                        Current = Current.Childs.Find(n => n.Char == Word[i]);
                    }
                }
            }
            if (Current.Count == 0)
                DifferentWords++;
            Current.Count++;
        }

        public List<WordCountPair> Export()
        {
            while (ThreadWorks || !WordsToAdd.IsEmpty)
                Thread.Sleep(10);

            var Result = new List<WordCountPair>();
            _Export(Result, String.Empty, Root);
            return Result;
        }

        private void _Export(List<WordCountPair> Result, string Word, Node Current)
        {
            foreach (var Child in Current.Childs)
            {
                if (Child.Count > 0)
                    Result.Add(new WordCountPair() { Word = Word + Child.Char, Count = Child.Count });

                if (Child.Childs.Count > 0)
                    _Export(Result, Word + Child.Char, Child);
            }
        }

        public void Dispose()
        {
            Adders.ForEach(a => a.Abort());
        }
    }
}
