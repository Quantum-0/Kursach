using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Runtime.Serialization;

namespace CharTrees
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

        internal static WordCountPair Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException();

            if (!str.Contains(" - "))
                throw new ArgumentException();

            var arr = str.Split('-');

            if (arr.Length != 2)
                throw new ArgumentException();

            return new WordCountPair() { Word = arr[0].Substring(0, arr[0].Length - 1), Count = int.Parse(arr[1]) };
        }

        static public List<WordCountPair> Sort(IEnumerable<WordCountPair> WCPs, int Count = -1)
        {
            if (Count == -1)
                Count = WCPs.Count();

            var On2 = /*WCPs.Count() * */Count;
            var Onlogn = /*WCPs.Count() * */Math.Log(WCPs.Count());
            if (On2 > Onlogn)
            {
                var Result = WCPs.ToList();
                Result.Sort();
                return Result.Take(Count).ToList();
            }
            else // SelectionSort
            {
                WordCountPair[] Result = WCPs.ToArray();
                WordCountPair Temp;
                int BiggestIndex, i, j;
                for (i = 0; i < Count; i++)
                {
                    BiggestIndex = i;
                    for (j = i; j < Result.Length; j++)
                    {
                        if (Result[j].Count > Result[BiggestIndex].Count)
                            BiggestIndex = j;
                    }
                    Temp = Result[i];
                    Result[i] = Result[BiggestIndex];
                    Result[BiggestIndex] = Temp;
                }
                return Result.Take(Count).ToList();
            }
        }
    }

    public abstract class Node
    {
        public char Char;
        public long Count;
        public virtual IList<Node> Childs { get; set; }

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
    
    public abstract class PrefixTree
    {
        protected object Sync = new object();
        public Node Root;
        public ulong DifferentWords;
        public ulong ProcessedWords;
        public ulong ProcessedChars;
        public ulong ProcessingTime;
        public ulong FilesProcessed;

        public PrefixTree()
        {
        }

        public abstract void AppendTree(PrefixTree Tree);

        public abstract void CreateBranch(WordCountPair wcp);

        public abstract bool AddWord(string Word);

        public List<WordCountPair> Export()
        {
            lock (Sync)
            {
                var Result = new List<WordCountPair>();
                _Export(Result, String.Empty, Root);
                return Result;
            }
        }

        protected void _Export(List<WordCountPair> Result, string Word, Node current)
        {
            dynamic Current;
            if (current is SynchronousNode)
                Current = current as SynchronousNode;
            //else if (current is AsynchronousNode)
            //    Current = current as AsynchronousNode;
            else
                throw new ArgumentException();

            lock (Sync)
            {
                foreach (var Child in Current.Childs)
                {
                    if (Child.Count > 0)
                        Result.Add(new WordCountPair() { Word = Word + Child.Char, Count = Child.Count });

                    if (Child.Childs.Count > 0)
                        _Export(Result, Word + Child.Char, Child);
                }
            }
        }
    }

    public class SyncPrefixTree : PrefixTree
    {
        public SyncPrefixTree()
        {
            Root = new SynchronousNode('\0');
        }

        public override void CreateBranch(WordCountPair wcp)
        {
            SynchronousNode Current = Root as SynchronousNode;
            var NotInitialized = false;

            for (int i = 0; i < wcp.Word.Length; i++)
            {
                if (NotInitialized)
                {
                    var New = new SynchronousNode(wcp.Word[i]);
                    Current.Childs.Add(New);
                    Current = New;
                }
                else
                {

                    if (!Current.Childs.Exists(n => n.Char == wcp.Word[i]))
                    {
                        NotInitialized = true;
                        var New = new SynchronousNode(wcp.Word[i]);
                        Current.Childs.Add(New);
                        Current = New;
                    }
                    else
                    {
                        Current = Current.Childs.Find(n => n.Char == wcp.Word[i]);
                    }
                }
            }
            if (Current.Count == 0)
                DifferentWords++;
            Current.Count += wcp.Count;
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
                    lock (Sync)
                        Current.Childs.Add(New);
                    Current = New;
                }
                else
                {
                    bool Condition;
                    lock (Sync)
                        Condition = !Current.Childs.Exists(n => n.Char == Word[i]);
                    if (Condition)
                    {
                        NotInitialized = true;
                        var New = new SynchronousNode(Word[i]);
                        lock (Sync)
                            Current.Childs.Add(New);
                        Current = New;
                    }
                    else
                    {
                        lock (Sync)
                            Current = Current.Childs.Find(n => n.Char == Word[i]);
                    }
                }
            }

            if (Current.Count == 0)
                DifferentWords++;
            Current.Count++;
            ProcessedWords++;
            ProcessedChars += Convert.ToUInt64(Word.Length * 2);

            return true;
        }

        public override void AppendTree(PrefixTree Tree)
        {
            lock (Sync)
            {
                this.FilesProcessed += Tree.FilesProcessed;
                this.ProcessedChars += Tree.ProcessedChars;
                this.ProcessedWords += Tree.ProcessedWords;
                this.ProcessingTime += Tree.ProcessingTime;
                _Append(Tree.Root);
            }
        }

        private void _Append(Node current, string Word = "")
        {
            dynamic Current;
            if (current is SynchronousNode)
                Current = current as SynchronousNode;

            else
                throw new ArgumentException();

            foreach (var Child in Current.Childs)
            {
                if (Child.Count > 0)
                    CreateBranch(new WordCountPair() { Word = Word + Child.Char, Count = Child.Count });

                if (Child.Childs.Count > 0)
                    _Append(Child, Word + Child.Char);
            }
        }
    }

    public class AsyncPrefixTree : PrefixTree
    {
        ConcurrentQueue<string> WordsToAdd = new ConcurrentQueue<string>();
        Thread Adder;
        bool ThreadWorks = true;
        public bool Busy { get; private set; } = false;
        public int QueueLength { get { return WordsToAdd.Count; } }

        public override void CreateBranch(WordCountPair wcp)
        {
            throw new NotImplementedException();
        }

        private static void WorkWithWords(object oTree)
        {
            AsyncPrefixTree Tree = (AsyncPrefixTree)oTree;
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

        public AsyncPrefixTree()
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

        public override void AppendTree(PrefixTree Tree)
        {
            throw new NotImplementedException();
        }
    }

    //public class ASyncMultiThreadCharTree : CharTree
    //{
    //    ConcurrentQueue<string> WordsToAdd = new ConcurrentQueue<string>();
    //    List<Thread> Adders = new List<Thread>();
    //    bool ThreadWorks = true;
    //    public bool Busy { get; private set; } = false;
    //    public int QueueLength { get { return WordsToAdd.Count; } }

    //    private static void WorkWithWords(object oTree)
    //    {
    //        ASyncMultiThreadCharTree Tree = (ASyncMultiThreadCharTree)oTree;
    //        while (Tree != null)
    //        {
    //            if (Tree.WordsToAdd.Count > 0)
    //            {
    //                Tree.ThreadWorks = true;
    //                while (Tree.WordsToAdd.Count > 0)
    //                {
    //                    string Word;
    //                    while (!Tree.WordsToAdd.TryDequeue(out Word));
    //                    if (Word != null)
    //                        Tree._AddWord(Word);
    //                }
    //                Tree.ThreadWorks = false;
    //            }
    //            else
    //                Thread.Sleep(50);
    //        }
    //    }

    //    public ASyncMultiThreadCharTree(int ThreadsCount = 0)
    //    {
    //        Root = new AsynchronousNode('\0');

    //        if (ThreadsCount == 0)
    //            ThreadsCount = Environment.ProcessorCount;

    //        for (int i = 0; i < ThreadsCount; i++)
    //            Adders.Add(new Thread(WorkWithWords));

    //        foreach (var Adder in Adders)
    //        {
    //            Adder.IsBackground = true;
    //            Adder.Priority = ThreadPriority.Highest;
    //            Adder.Start(this);
    //        }
    //    }

    //    public new List<WordCountPair> Export()
    //    {
    //        while (ThreadWorks || !WordsToAdd.IsEmpty)
    //            Thread.Sleep(10);

    //        var Result = new List<WordCountPair>();
    //        _Export(Result, String.Empty, Root);
    //        return Result;
    //    }

    //    public override bool AddWord(string Word)
    //    {
    //        if (Busy)
    //        {
    //            if (WordsToAdd.Count < 100)
    //                Busy = false;
    //        }
    //        else
    //        {
    //            if (WordsToAdd.Count > 1000)
    //                Busy = true;
    //        }

    //        if (Busy)
    //            return false;

    //        WordsToAdd.Enqueue(Word);
    //        return true;
    //    }

    //    private void _AddWord(string Word)
    //    {
    //        AsynchronousNode Current = Root as AsynchronousNode;
    //        var NotInitialized = false;

    //        for (int i = 0; i < Word.Length; i++)
    //        {
    //            if (NotInitialized)
    //            {
    //                var New = new AsynchronousNode(Word[i]);
    //                //Current.Childs.Enqueue(New);
    //                Current.Childs.Add(New);
    //                Current = New;
    //            }
    //            else
    //            {

    //                if (!Current.Childs.Exists(n => n?.Char == Word[i]))
    //                {
    //                    NotInitialized = true;
    //                    var New = new AsynchronousNode(Word[i]);
    //                    //Current.Childs.Enqueue(New);
    //                    Current.Childs.Add(New);
    //                    Current = New;
    //                }
    //                else
    //                {
    //                    Current = Current.Childs.Find(n => n?.Char == Word[i]);
    //                }
    //            }
    //        }
    //        if (Current.Count == 0)
    //            DifferentWords++;
    //        Current.Count++;
    //    }
    //}

    //public static class ConcurentQueueExtension
    //{
    //    public static AsynchronousNode Find(this ConcurrentQueue<AsynchronousNode> Collection, Predicate<AsynchronousNode> Pred)
    //    {
    //        //AsynchronousNode Node;
    //        //lock (Collection)
    //        /*{
    //            for (int i = 0; i < Collection.Count; i++)
    //            {
    //                while (!Collection.TryDequeue(out Node)) ;
    //                Collection.Enqueue(Node);
    //                if (Pred(Node))
    //                    return Node;
    //            }
    //        }*/
            
    //        return Collection.ToList().Find(Pred);
    //    }

    //    public static bool Exists(this ConcurrentQueue<AsynchronousNode> Collection, Predicate<AsynchronousNode> Pred)
    //    {
    //        return Collection.Find(Pred) != null;
    //    }
    //}
}
