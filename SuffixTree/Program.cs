using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuffixTree
{
    class Program
    {
        static void FileToTree(string path, NotSuffixTree tree)
        {
            var Word = new StringBuilder();
            var WordsFound = 0;
            var Percents = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Обработка файла ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Path.GetFileName(path));
            Console.Write(" (" + new FileInfo(path).Length/1024 + " Kb)");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\nТекущий размер словаря повторений слов: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(tree.DifferentWords);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\nОбработка: [                    ]");
            Console.CursorLeft = 12;
            using (StreamReader sr = new StreamReader(path))
            {
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
                        while (tree.QueueLength > 1000)
                            Thread.Sleep(100);

                        WordsFound++;
                        tree.Increment(Word.ToString());
                        Word.Clear();
                    }
                }
                while (!sr.EndOfStream);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 0;
            Console.CursorTop++;
            Console.WriteLine("Файл обработан, найдено " + WordsFound + " слов");
            Console.WriteLine("--- Загрузка ОЗУ: " + GC.GetTotalMemory(false) / 1024 + " КБ ---");
        }

        static void Main(string[] args)
        {
            var tree = new NotSuffixTree();

            var Files = Directory.EnumerateFiles(@"F:\Books\");
            foreach (var File in Files)
            {
                FileToTree(File, tree);
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
            tree.Dispose();
            Console.ReadKey();
        }
    }
}
