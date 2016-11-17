using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuffixTree
{
    class Program
    {
        static void Main(string[] args)
        {
            var tree = new NotSuffixTree();
            tree.Increment("A");
            tree.Increment("Apple");
            tree.Increment("Banana");
            tree.Increment("Bananas");
            tree.Increment("Banana");
            tree.Increment("Fruit");

            using (StreamReader sr = new StreamReader("F:\\test.txt", Encoding.Unicode))
            {
                var Input = sr.ReadToEnd();
                var Words = Input.Split(new char[] { ' ', ',', '.', ':', '?', '!', '\n' });
                foreach (var Word in Words)
                {
                    tree.Increment(Word);
                }
            }

            var s = tree.Export();
            s.Sort();

            using (StreamWriter sw = new StreamWriter("TestResult.txt"))
            {
                foreach (var item in s)
                {
                    sw.WriteLine(item.Word + " - " + item.Count);
                }
            }
        }
    }
}
