using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;

namespace SuffixTree
{
    public struct WordCountPair : IComparable
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

    //public class NotSuffixTree
    //{
    //            public void Dispose()
    //    {
    //        // Это в кинуть финализатор, а dispose пусть плавненько выходит из цикла
    //        //Adders.ForEach(a => a.Abort());
    //    }
    //}

    public abstract class Node
    {
        public char Char;
        public long Count;
        public virtual IList<Node> Childs { get; set; }
        //public NodeChildsCollection<Node> Childs { get; set; }

        public Node(Char Char)
        {
            this.Char = Char;
            Count = 0;
        }
    }

    public class SynchronousNode : Node
    {
        public new List<SynchronousNode> Childs { get; set; }

        public SynchronousNode(Char Char) : base(Char)
        {
            Childs = new List<SynchronousNode>();
        }

        public override string ToString()
        {
            return Char + " (" + Childs.Count + ")";
        }
    }

    public class AsynchronousNode : Node
    {
        public new NodeChildsCollection<AsynchronousNode> Childs { get; set; }

        public AsynchronousNode(Char Char) : base(Char)
        {
            //Childs = new ConcurrentQueue<AsynchronousNode>();
            Childs = new NodeChildsCollection<AsynchronousNode>();
        }

        public override string ToString()
        {
            return Char + " (" + Childs.Count + ")";
        }
    }

    public abstract class CharTree
    {
        protected Node Root;
        public long DifferentWords;

        public CharTree()
        {
        }

        public abstract bool AddWord(string Word);

        public List<WordCountPair> Export()
        {
            var Result = new List<WordCountPair>();
            _Export(Result, String.Empty, Root);
            return Result;
        }

        protected void _Export(List<WordCountPair> Result, string Word, Node current)
        {
            dynamic Current;
            if (current is SynchronousNode)
                Current = current as SynchronousNode;
            else if (current is AsynchronousNode)
                Current = current as AsynchronousNode;
            else
                throw new ArgumentException();

            foreach (var Child in Current.Childs)
            {
                if (Child.Count > 0)
                    Result.Add(new WordCountPair() { Word = Word + Child.Char, Count = Child.Count });

                if (Child.Childs.Count > 0)
                    _Export(Result, Word + Child.Char, Child);
            }
        }
    }

    public class SyncCharTree : CharTree
    {

        public SyncCharTree()
        {
            Root = new SynchronousNode('\0');
        }

        public override bool AddWord(string Word)
        {
            SynchronousNode Current = Root as SynchronousNode;
            var NotInitialized = false;

            for (int i = 0; i < Word.Length; i++)
            {
                if (NotInitialized)
                {
                    var New = new SynchronousNode(Word[i]);
                    Current.Childs.Add(New);
                    Current = New;
                }
                else
                {

                    if (!Current.Childs.Exists(n => n.Char == Word[i]))
                    {
                        NotInitialized = true;
                        var New = new SynchronousNode(Word[i]);
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

            return true;
        }
    }

    public class ASyncCharTree : CharTree
    {
        ConcurrentQueue<string> WordsToAdd = new ConcurrentQueue<string>();
        Thread Adder;
        bool ThreadWorks = true;
        public bool Busy { get; private set; } = false;
        public int QueueLength { get { return WordsToAdd.Count; } }

        private static void WorkWithWords(object oTree)
        {
            ASyncCharTree Tree = (ASyncCharTree)oTree;
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
                            Tree._AddWord(Word);
                    }
                    Tree.ThreadWorks = false;
                }
                else
                    Thread.Sleep(10);
            }
        }

        public ASyncCharTree()
        {
            Root = new SynchronousNode('\0');
            Adder = new Thread(WorkWithWords);
            Adder.IsBackground = true;
            Adder.Priority = ThreadPriority.Highest;
            Adder.Start(this);
        }

        public new List<WordCountPair> Export()
        {
            while (ThreadWorks || WordsToAdd.Count != 0)
                Thread.Sleep(10);

            var Result = new List<WordCountPair>();
            _Export(Result, String.Empty, Root as SynchronousNode);
            return Result;
        }

        public override bool AddWord(string Word)
        {
            if (Busy)
            {
                if (WordsToAdd.Count < 100)
                    Busy = false;
            }
            else
            {
                if (WordsToAdd.Count > 1000)
                    Busy = true;
            }

            if (Busy)
                return false;

            WordsToAdd.Enqueue(Word);
            return true;
        }

        private void _AddWord(string Word)
        {
            SynchronousNode Current = Root as SynchronousNode;
            var NotInitialized = false;

            for (int i = 0; i < Word.Length; i++)
            {
                if (NotInitialized)
                {
                    var New = new SynchronousNode(Word[i]);
                    Current.Childs.Add(New);
                    Current = New;
                }
                else
                {
                    if (!Current.Childs.Exists(n => n.Char == Word[i]))
                    {
                        NotInitialized = true;
                        var New = new SynchronousNode(Word[i]);
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
    }

    public class ASyncMultiThreadCharTree : CharTree
    {
        ConcurrentQueue<string> WordsToAdd = new ConcurrentQueue<string>();
        List<Thread> Adders = new List<Thread>();
        bool ThreadWorks = true;
        public bool Busy { get; private set; } = false;
        public int QueueLength { get { return WordsToAdd.Count; } }

        private static void WorkWithWords(object oTree)
        {
            ASyncMultiThreadCharTree Tree = (ASyncMultiThreadCharTree)oTree;
            while (Tree != null)
            {
                if (Tree.WordsToAdd.Count > 0)
                {
                    Tree.ThreadWorks = true;
                    while (Tree.WordsToAdd.Count > 0)
                    {
                        string Word;
                        while (!Tree.WordsToAdd.TryDequeue(out Word));
                        if (Word != null)
                            Tree._AddWord(Word);
                    }
                    Tree.ThreadWorks = false;
                }
                else
                    Thread.Sleep(50);
            }
        }

        public ASyncMultiThreadCharTree(int ThreadsCount = 0)
        {
            Root = new AsynchronousNode('\0');

            if (ThreadsCount == 0)
                ThreadsCount = Environment.ProcessorCount;

            for (int i = 0; i < ThreadsCount; i++)
                Adders.Add(new Thread(WorkWithWords));

            foreach (var Adder in Adders)
            {
                Adder.IsBackground = true;
                Adder.Priority = ThreadPriority.Highest;
                Adder.Start(this);
            }
        }

        public new List<WordCountPair> Export()
        {
            while (ThreadWorks || !WordsToAdd.IsEmpty)
                Thread.Sleep(10);

            var Result = new List<WordCountPair>();
            _Export(Result, String.Empty, Root);
            return Result;
        }

        public override bool AddWord(string Word)
        {
            if (Busy)
            {
                if (WordsToAdd.Count < 100)
                    Busy = false;
            }
            else
            {
                if (WordsToAdd.Count > 1000)
                    Busy = true;
            }

            if (Busy)
                return false;

            WordsToAdd.Enqueue(Word);
            return true;
        }

        private void _AddWord(string Word)
        {
            AsynchronousNode Current = Root as AsynchronousNode;
            var NotInitialized = false;

            for (int i = 0; i < Word.Length; i++)
            {
                if (NotInitialized)
                {
                    var New = new AsynchronousNode(Word[i]);
                    //Current.Childs.Enqueue(New);
                    Current.Childs.Add(New);
                    Current = New;
                }
                else
                {

                    if (!Current.Childs.Exists(n => n?.Char == Word[i]))
                    {
                        NotInitialized = true;
                        var New = new AsynchronousNode(Word[i]);
                        //Current.Childs.Enqueue(New);
                        Current.Childs.Add(New);
                        Current = New;
                    }
                    else
                    {
                        Current = Current.Childs.Find(n => n?.Char == Word[i]);
                    }
                }
            }
            if (Current.Count == 0)
                DifferentWords++;
            Current.Count++;
        }
    }

    public static class ConcurentQueueExtension
    {
        public static AsynchronousNode Find(this ConcurrentQueue<AsynchronousNode> Collection, Predicate<AsynchronousNode> Pred)
        {
            //AsynchronousNode Node;
            //lock (Collection)
            /*{
                for (int i = 0; i < Collection.Count; i++)
                {
                    while (!Collection.TryDequeue(out Node)) ;
                    Collection.Enqueue(Node);
                    if (Pred(Node))
                        return Node;
                }
            }*/
            
            return Collection.ToList().Find(Pred);
        }

        public static bool Exists(this ConcurrentQueue<AsynchronousNode> Collection, Predicate<AsynchronousNode> Pred)
        {
            return Collection.Find(Pred) != null;
        }
    }
}
