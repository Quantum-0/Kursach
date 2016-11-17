using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    class NotSuffixTree
    {
        Node Root;
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
        }

        public NotSuffixTree()
        {
            Root = new Node('\0');
        }

        public void Increment(string Word)
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
            } // for
            Current.Count++;
        }

        public List<WordCountPair> Export()
        {
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
    }
}
