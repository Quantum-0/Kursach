#define CONTRACTS_FULL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
                        WordsFound++;
                        while (true)
                        {
                            if (tree.AddWord(Word.ToString()))
                                break;

                            Thread.Sleep(1);
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

        static void ParallelFilesToTrees(IEnumerable<string> paths, IEnumerable<CharTree> trees)
        {
            Contract.Assert(paths.Count() == trees.Count());

            var MemBefore = GC.GetTotalMemory(false);

            Stopwatch sw = new Stopwatch();
            List<FileInfo> FIs = new List<FileInfo>(paths.Count());
            Console.WriteLine("Обработка файлов:");
            foreach (var path in paths)
            {
                var FI = new FileInfo(path);
                FIs.Add(FI);
                Console.WriteLine("> " + FI.Name + " (" + (FI.Length / (1024 * 512) / 2f) + " МБ)");
            }
            StreamReader[] srs = new StreamReader[paths.Count()];
            Stopwatch[] localsws = new Stopwatch[paths.Count()];
            for (int i = 0; i < srs.Length; i++)
            {
                srs[i] = new StreamReader(paths.ElementAt(i));
                localsws[i] = new Stopwatch();
            }
            Console.WriteLine();

            sw.Start();
            long WordsFound = 0;
            bool BlockOutput = false;
            Parallel.For(0, paths.Count(), i =>
            {
                localsws[i].Start();
                var LocalWordsFound = 0;
                StringBuilder Word = new StringBuilder();
                do
                {
                    var c = Convert.ToChar(srs[i].Read());

                    /*if (srs[i].BaseStream.Position >= srs[i].BaseStream.Length * (Percents + 5) / 100)
                    {
                        Percents += 5;
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.Write(" ");
                    }*/

                    if (char.IsLetter(c))
                    {
                        Word.Append(char.ToUpper(c));
                    }
                    else if (Word.Length != 0)
                    {
                        WordsFound++;
                        LocalWordsFound++;
                        while (true)
                        {
                            if (trees.ElementAt(i).AddWord(Word.ToString()))
                                break;

                            Thread.Sleep(1);
                        }
                        Word.Clear();
                    }
                }
                while (!srs[i].EndOfStream);
                localsws[i].Stop();

                SpinWait.SpinUntil(() => !BlockOutput);
                BlockOutput = true;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> Файл ");
                Console.ForegroundColor = ConsoleColor.Green;
                if (FIs[i].Name.Length > 46)
                    Console.Write(FIs[i].Name.Substring(0, 44) + "...");
                else
                    Console.Write(FIs[i].Name);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" обработан за ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(localsws[i].ElapsedMilliseconds / 10 / 100f);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" секунд");
                Console.Write(">> Обработано ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(LocalWordsFound);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" слов");
                Console.Write(">> Средняя скорость чтения: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(FIs[i].Length * 1000 / (1024 * 64) / (localsws[i].ElapsedMilliseconds) / 16f);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" Мб/сек");
                Console.Write(">> Средняя скорость обработки: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(LocalWordsFound * 1000 / localsws[i].ElapsedMilliseconds);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" слов/сек\n");

                BlockOutput = false;
            });
            sw.Stop();
            Console.WriteLine("\n--- Файлы обработаны ---\n");
            Console.Write("> Всего слов найдено: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(WordsFound);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("> Всего данных обработано: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(FIs.Sum(fi => fi.Length) / (1024 * 64) / 16f);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" МБ");
            Console.Write("> Общее время выполнения: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(sw.ElapsedMilliseconds / 100 / 10f);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" сек");
            Console.Write("> Средняя скорость чтения: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(FIs.Sum(fi => fi.Length) / (1024 * 64) / (sw.ElapsedMilliseconds / 1000) / 16f);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" Мб/сек");
            Console.Write("> Средняя скорость обработки: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(WordsFound * 1000 / sw.ElapsedMilliseconds);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" слов/сек");
            var MemAfter = GC.GetTotalMemory(true);
            Console.Write("> Использовано памяти: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write((MemAfter - MemBefore) / (1024 * 64) / 16f);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" Мб");
        }

        static void Main(string[] args)
        {
            var tree = new ASyncCharTree();

            var Files = Directory.EnumerateFiles(@"F:\Books\");
            var Trees = new List<CharTree>();
            for (int i = 0; i < Files.Count(); i++)
            {
                Trees.Add(new SyncCharTree());
            }
            ParallelFilesToTrees(Files, Trees);

            var s = tree.Export();
            s.Sort();

            using (StreamWriter sw = new StreamWriter("TestResult.txt"))
            {
                foreach (var item in s)
                {
                    sw.WriteLine(item.Word + " - " + item.Count);
                }
            }
            Console.ReadKey();



            /*
             * ASyncCharTree и SyncCharTree по скорости работают примерно одинакого
             * ASyncMultiThreadCharTree работает медленнее, т.к. Root.Childs постоянно в локе одним из потоков
             * Решение:
             * - написать сою коллекцию поддерживающую только Count, Add, Exist, Find
             * ...
             * - не получилось :D не работает, падает, а если и работает то всё-равно медленно
             */

            /*
             * MapReduce:
             * Map - File -> Tree
             * Reduce - Tree + Tree -> Tree
             */
        }
    }
}
