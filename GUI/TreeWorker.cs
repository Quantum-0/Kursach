using CharTrees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class TreeWorker
    {
        public static CharTree LoadTreeFromFile(string fname)
        {
            using (StreamReader sr = new StreamReader(fname))
            {
                sr.ReadLine(); sr.ReadLine();
                CharTree tree = new SyncCharTree();
                tree.ProcessedChars = ulong.Parse(sr.ReadLine());
                tree.ProcessedWords = ulong.Parse(sr.ReadLine());
                tree.DifferentWords = ulong.Parse(sr.ReadLine());
                tree.ProcessingTime = ulong.Parse(sr.ReadLine());
                tree.FilesProcessed = ulong.Parse(sr.ReadLine());
                sr.ReadLine(); sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    tree.CreateBranch( WordCountPair.Parse(sr.ReadLine()) );
                }
                return tree;
            }
        }
        public static void SaveTreeToFile(string fname, CharTree tree)
        {
            using (StreamWriter sw = new StreamWriter(fname))
            {
                sw.WriteLine("<< Char Tree Content File >>");
                sw.WriteLine();
                sw.WriteLine(tree.ProcessedChars);
                sw.WriteLine(tree.ProcessedWords);
                sw.WriteLine(tree.DifferentWords);
                sw.WriteLine(tree.ProcessingTime);
                sw.WriteLine(tree.FilesProcessed);
                sw.WriteLine();
                sw.WriteLine("Data:");
                var data = tree.Export().ToArray();
                for (int i = 0; i < data.Length; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }
        }
    }
}
