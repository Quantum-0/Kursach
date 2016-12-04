using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuffixTree
{
    class Program
    {
        static void FileToTree(string path, CharTree tree)
        {
            var FI = new FileInfo(path);
            Stopwatch sw = new Stopwatch();
            var Word = new StringBuilder();
            var WordsFound = 0;
            var Percents = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Обработка файла ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Path.GetFileName(path));
            Console.Write(" (" + FI.Length/1024 + " Kb)");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\nТекущий размер словаря повторений слов: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(tree.DifferentWords);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\nОбработка: [                    ]");
            Console.CursorLeft = 12;
            using (StreamReader sr = new StreamReader(path))
            {
                sw.Start();
                do
                {
                    var c = Convert.ToChar(sr.Read());

                    if (sr.BaseStream.Position >= sr.BaseStream.Length * (Percents + 5) / 100)
                    {
                        Percents += 5;
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.Write(" ");
                    }

                    if (char.IsLetter(c))
                    {
                        Word.Append(char.ToUpper(c));
                    }
                    else if (Word.Length != 0)
                    {
                        /*if (tree is ASyncCharTree)
                            while (((ASyncCharTree)tree).QueueLength > 1000)
                                Thread.Sleep(20);
                        if (tree is ASyncMultiThreadCharTree)
                            while (((ASyncMultiThreadCharTree)tree).QueueLength > 1000)
                                Thread.Sleep(20);*/

                        WordsFound++;
                        while (true)
                        {
                            if (tree.AddWord(Word.ToString()))
                                break;

                            //Console.WriteLine("Busy");
                            Thread.Sleep(10);
                        }
                        Word.Clear();
                    }
                }
                while (!sr.EndOfStream);
                sw.Stop();
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 0;
            Console.CursorTop++;
            Console.WriteLine("Файл обработан, найдено " + WordsFound + " слов");
            Console.WriteLine("Средняя скорость обработки: " + (FI.Length / 1024 / (sw.ElapsedMilliseconds / 1000)) + " кб/сек | "  + (WordsFound * 1000 / sw.ElapsedMilliseconds) + " слов/сек");
            Console.WriteLine("--- Загрузка ОЗУ: " + GC.GetTotalMemory(false) / 1024 + " КБ ---");
        }

        static void Main(string[] args)
        {
            var before = GC.GetTotalMemory(false);
            var tree = new ASyncCharTree();
            var tree2 = new ASyncCharTree();
            var tree3 = new ASyncCharTree();
            CharTree[] trees = { tree, tree2, tree3};

            var Files = Directory.EnumerateFiles(@"F:\Books\");
            /*foreach (var File in Files)
            {
                FileToTree(File, tree);
            }*/
            Parallel.For(0, 3, i => FileToTree(Files.ElementAt(i), trees[i]));
            //FileToTree(Files.ElementAt(0), tree);
            var after = GC.GetTotalMemory(true);
            var treesize = after - before;

            var s = tree.Export();
            s.Sort();

            using (StreamWriter sw = new StreamWriter("TestResult.txt"))
            {
                foreach (var item in s)
                {
                    sw.WriteLine(item.Word + " - " + item.Count);
                }
            }
            //tree.Dispose();
            Console.ReadKey();



            /*
             * ASyncCharTree и SyncCharTree по скорости работают примерно одинакого
             * ASyncMultiThreadCharTree работает медленнее, т.к. Root.Childs постоянно в локе одним из потоков
             * Решение:
             * - написать сою коллекцию поддерживающую только Count, Add, Exist, Find
             * ...
             * - не получилось :D не работает, падает, а если и работает то всё-равно медленно
             */
        }
    }
}
