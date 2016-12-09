using CharTrees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainForm : Form
    {
        /*
         * TODO:
         * Log
         * Tree Files Merging (Reduce)
         * Multi Wiki pages downloading and processing
         */

        public CharTree MainTree;
        public string CurrentFile;
        public bool FileChanged;
        private const string Caption = "Курсач";
        public float ReadingSpeed;
        public float ProcessingSpeed;
        public long ProcessingTime;
        public byte ProcessingFiles;
        public bool AbortCalculating = false;
        public ulong LTProcessedChars;
        public ulong LTProcessedWords;

        public string GetReadingSpeed()
        {
            // ReadingSpeed - скорость чтения, байт/мс
            // ReadingSpeed * 1000 / 1024 - кб/с
            var kbps = ReadingSpeed * 1000 / 1024;
            if (kbps > 512)
                return (kbps / 1024).ToString("F2") + " Мб/с";
            else
                return kbps.ToString("F1") + " Кб/с";
        }
        public string GetMidReadingSpeed()
        {
            var MidReadingSpeed = (LTProcessedChars != 0 ? LTProcessedChars : MainTree.ProcessedChars) / (1f + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
            var kbps = MidReadingSpeed * 1000 / 1024;
            if (kbps > 512)
                return (kbps / 1024).ToString("F2") + " Мб/с";
            else
                return kbps.ToString("F1") + " Кб/с";
        }
        public string GetProcessingSpeed()
        {
            // ProcessingSpeed - слов/мс
            var wps = ProcessingSpeed * 1000;
            if (wps > 5000)
                return (wps / 1000).ToString("F1") + " тыс.слов/сек";
            else
                return wps + " слов/сек";
        }
        public string GetMidProcessingSpeed()
        {
            var MidProcessingSpeed = (LTProcessedWords != 0 ? LTProcessedWords : MainTree.ProcessedWords) / (1 + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
            var wps = MidProcessingSpeed * 1000;
            if (wps > 5000)
                return (wps / 1000).ToString("F1") + " тыс.слов/сек";
            else
                return wps + " слов/сек";
        }
        public string GetProcessedFiles()
        {
            return MainTree.FilesProcessed + " завершено, " + ProcessingFiles + " в обработке";
        }
        public string GetProcessedBytes()
        {
            var chars = MainTree.ProcessedChars + LTProcessedChars;
            if (chars < 1024 * 0.9)
                return chars + " байт";
            else if (chars < 1024 * 1024 * 0.9)
                return (chars / 1024f).ToString("F1") + " Кб";
            else if (chars < 1024 * 1024 * 1024 * 0.5)
                return (chars / 1024f / 1024f).ToString("F1") + " Мб";
            else
                return (chars / 1024f / 1024f / 1024f).ToString("F2") + " Гб";
        }
        public string GetProcessedWords()
        {
            var kwords = (LTProcessedWords != 0? LTProcessedWords : MainTree.ProcessedWords) / 1000f;
            if (kwords < 100)
                return kwords.ToString("F2") + " тыс.слов";
            else if (kwords < 5000)
                return kwords.ToString("F1") + " тыс.слов";
            else
                return (kwords / 1000).ToString("F2") + " млн.слов";
        }
        public string GetDifferentWords()
        {
            var kwords = MainTree.DifferentWords / 1000f;
            if (kwords < 100)
                return kwords.ToString("F2") + " тыс.слов";
            else if (kwords < 5000)
                return kwords.ToString("F1") + " тыс.слов";
            else
                return (kwords / 1000).ToString("F2") + " млн.слов";
        }
        public string GetTotalProcessingTime()
        {
            return ((MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime)) / 1000f).ToString("F1") + " сек";
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshStatistics(true);
            UpdateCaption();
            MainTree = new SyncCharTree();
            var colorslist = new List<Color>();
            colorslist.Add(Color.FromArgb(192, 192, 192, 192));
            colorslist.Add(Color.FromArgb(64, Color.GreenYellow));
            chart.PaletteCustomColors = colorslist.ToArray();
        }

        // Обновление выводимых данных
        private void RefreshStatistics(bool Init = false)
        {
            if (Init)
            {
                dataGridStatistics.RowCount = 10;
                dataGridStatistics[0, 0].Value = "Скорость чтения файла:";
                dataGridStatistics[0, 1].Value = "Средняя скорость чтения файла:";
                dataGridStatistics[0, 2].Value = "Скорость построения словаря:";
                dataGridStatistics[0, 3].Value = "Средняя корость построения словаря:";
                dataGridStatistics[0, 4].Value = "Обработанно данных:";
                dataGridStatistics[0, 5].Value = "Обработанно слов:";
                dataGridStatistics[0, 6].Value = "Загрузка ОЗУ:";
                dataGridStatistics[0, 7].Value = "Объём словаря:";
                dataGridStatistics[0, 8].Value = "Обработано файлов:";
                dataGridStatistics[0, 9].Value = "Общее время обработки:";
            }

            if (MainTree != null)
            {
                dataGridStatistics[1, 0].Value = GetReadingSpeed();
                dataGridStatistics[1, 1].Value = GetMidReadingSpeed();
                dataGridStatistics[1, 2].Value = GetProcessingSpeed();
                dataGridStatistics[1, 3].Value = GetMidProcessingSpeed();
                dataGridStatistics[1, 4].Value = GetProcessedBytes();
                dataGridStatistics[1, 5].Value = GetProcessedWords();
                dataGridStatistics[1, 6].Value = GC.GetTotalMemory(false) / (1024 * 512) / 2f + " Мб";
                dataGridStatistics[1, 7].Value = GetDifferentWords();
                dataGridStatistics[1, 8].Value = GetProcessedFiles();
                dataGridStatistics[1, 9].Value = GetTotalProcessingTime(); 
            }
            else
            {
                for (int i = 0; i < dataGridStatistics.RowCount; i++)
                {
                    dataGridStatistics[1, i].Value = "Словарь не открыт";
                }
            }
        }
        private void RefreshFilesStats(FileInfo[] files, ProcessingState[] states, int[] percents, float[] ReadingSpeeds, float[] ProcessingSpeeds)
        {
            dataGridFilesProcessing.RowCount = files.Length;
            for (int i = 0; i < files.Length; i++)
            {
                dataGridFilesProcessing[0, i].Value = files[i].Name;
                dataGridFilesProcessing[1, i].Value = states[i];
                dataGridFilesProcessing[2, i].Value = percents[i] + "%";
                dataGridFilesProcessing[3, i].Value = files[i].Length / 1024f + " Кб";
                dataGridFilesProcessing[4, i].Value = ReadingSpeeds[i] / 1024f * 1000 + " Кб/с";
                dataGridFilesProcessing[5, i].Value = (ProcessingSpeeds[i] * 1000f) + " Слов/сек";
            }
        }
        private void RefreshChart()
        {
            this.Invoke((Action)(() =>
            {
                Func<int, int> Xvisualization = x => x;
                Func<double, double> Yvisualization = y => Math.Pow(y, 1.0 / 2.5);
                double Dispersion = 0;
                if (chart.Visible)
                {
                    chart.Series[0].Points.Clear();
                    chart.Series[1].Points.Clear();
                    var list = MainTree.Export();
                    list.Sort();
                    var maxX = Convert.ToInt32(Math.Floor(Math.Pow(list.Count, 1d / 2)));
                    var chartdata = new List<WordCountPair>();
                    for (int i = 0; i < maxX; i++)
                    {
                        chartdata.Add(list[Xvisualization(i)]);
                    }

                    var FuncConst = 0d;
                    for (int j = 0; j < Math.Min(chartdata.Count, 50); j++)
                        FuncConst += j * Convert.ToDouble(chartdata[j].Count) / 50;


                    for (int i = 1; i < chartdata.Count; i++)
                    {
                        var y1 = Convert.ToDouble(chartdata[i].Count);
                        var y2 = FuncConst / (i + 1);
                        chart.Series[0].Points.AddXY(i, Yvisualization(y1));
                        chart.Series[1].Points.AddXY(i, Yvisualization(y2));
                        Dispersion += (y1 - y2) * (y1 - y2) / chartdata.Count;
                    }
                }
            }));
        }

        // Обработчики нажатий кнопок в вернем меню
        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }
        private void SaveFile_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }
        private void NewFile_Click(object sender, EventArgs e)
        {
            NewFile();
            RefreshStatistics();
        }

        // UI для открытия/сохранения словарей
        private bool NewFile()
        {
            if (!FileChanged)
            {
                MainTree = new SyncCharTree();
                GC.Collect(2);
                return true;
            }
            else
            {
                DialogResult res = AskForSaving();
                if (res == DialogResult.Cancel)
                    return false;

                if (res == DialogResult.No)
                {
                    MainTree = new SyncCharTree();
                    return true;
                }
                else //res == Yes
                {
                    if (SaveFile())
                    {
                        MainTree = new SyncCharTree();
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
        private bool OpenFile()
        {
            if (!FileChanged)
            {
                if (openDictDialog.ShowDialog() == DialogResult.OK)
                    return JustOpen(openDictDialog.FileName);
                else
                    return false;
            }
            else
            {
                DialogResult res = AskForSaving();
                if (res == DialogResult.Cancel)
                    return false;

                if (res == DialogResult.No)
                {
                    if (openDictDialog.ShowDialog() == DialogResult.OK)
                        return JustOpen(openDictDialog.FileName);
                    else
                        return false;
                }
                else //res == Yes
                {
                    if (SaveFile())
                    {
                        if (openDictDialog.ShowDialog() == DialogResult.OK)
                            return JustOpen(openDictDialog.FileName);
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
        }
        private bool SaveAsFile()
        {
            if (saveDictDialog.ShowDialog() == DialogResult.OK)
                return JustSave(saveDictDialog.FileName);
            else
                return false;
        }
        private bool SaveFile()
        {
            if (CurrentFile == "")
                return SaveAsFile();
            else
                return JustSave(CurrentFile);
        }
        private DialogResult AskForSaving()
        {
            return MessageBox.Show("Сохранить изменения в файле?", "Сохранение изменений", MessageBoxButtons.YesNoCancel);
        }
        private bool JustOpen(string fname)
        {
            try
            {
                MainTree = TreeWorker.LoadTreeFromFile(fname);
                RefreshStatistics();
                CurrentFile = fname;
                FileChanged = false;
                GC.Collect(2);
            }
            catch
            {
                MessageBox.Show("Не удалось открыть файл.");
                return false;
            }
            UpdateCaption();
            UpdateStatusStrip(0, "Открыт словарь " + Path.GetFileName(fname));
            return true;
        }
        private bool JustSave(string fname)
        {
            try
            {
                TreeWorker.SaveTreeToFile(fname, MainTree);
                CurrentFile = fname;
                FileChanged = false;
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить файл.");
                return false;
            }
            UpdateCaption();
            UpdateStatusStrip(0, "Словарь " + Path.GetFileName(fname) + " сохранён");
            return true;
        }
        private void UpdateCaption()
        {
            Invoke((Action)(() =>
            {
                if (!string.IsNullOrWhiteSpace(CurrentFile))
                {
                    if (!FileChanged)
                        Text = Caption + " - " + Path.GetFileName(CurrentFile);
                    else
                        Text = Caption + " - " + Path.GetFileName(CurrentFile) + '*';
                }
                else
                {
                    if (!FileChanged)
                        Text = Caption;
                    else
                        Text = Caption + " - Новый словарь*";
                }
            }));
        }
        private void UpdateStatusStrip(int Progress, string Text = null)
        {
            this.Invoke((Action)delegate {
                toolStripProgressBar1.Value = Progress;
                if (Text != null)
                    toolStripStatusLabel1.Text = Text;
            });
        }

        private void обработатьТекстовыйФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;            

            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;

            Task.Run(() => ProcessSingleFile(openFileDialog.FileName));
        }
        private void отображатьСтатистикуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridStatistics.Visible = отображатьСтатистикуToolStripMenuItem.Checked;
        }
        private void отображатьГрафикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart.Visible = отображатьГрафикToolStripMenuItem.Checked;
            RefreshChart();
        }
        private void прерватьВыполнениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbortCalculating = true;
            UpdateStatusStrip(100, "Прерывание обработки..");
        }
        private void обработатьПапкуСФайламиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;

            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;

            Task.Run(() => ProcessFolder(folderBrowserDialog.SelectedPath));
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ProcessingFiles > 0)
            {
                e.Cancel = true;
                return;
            }


        }

        private void ProcessSingleFile(string fname)
        {
            StartProcessing(new FileInfo[] { new FileInfo(fname) });
            //FileChanged = true;
            //var FI = new FileInfo(fname);
            //var sw = new Stopwatch();
            //var Word = new StringBuilder();
            //var WordsFound = 0;
            //var Percents = 0;
            //var shortfname = Path.GetFileName(fname);
            //UpdateStatusStrip(0, "Обработка файла: " + shortfname);
            //long lastswvalue = 0;
            //long lastiterationsvalue = 0;
            //long lastwordsvalue = 0;
            //long iteration = 0;
            /*using (StreamReader sr = new StreamReader(fname))
            {
                ProcessingFiles++;
                sw.Start();
                do
                {
                    var c = Convert.ToChar(sr.Read());
                    iteration++;

                    if (sr.BaseStream.Position > sr.BaseStream.Length * (Percents + 5) / 100)
                    {
                        Percents += 5;
                        UpdateStatusStrip(Percents, "Обработка файла: " + shortfname + " [" + Percents + "%]");
                        ReadingSpeed = (float)(iteration - lastiterationsvalue) / (sw.ElapsedMilliseconds - lastswvalue);
                        ProcessingSpeed = (float)(WordsFound - lastwordsvalue) / (sw.ElapsedMilliseconds - lastswvalue);
                        ProcessingTime = sw.ElapsedMilliseconds;
                        lastswvalue = sw.ElapsedMilliseconds;
                        lastiterationsvalue = iteration;
                        lastwordsvalue = WordsFound;
                        this.Invoke((Action)(() => RefreshStatistics()));
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
                            if (MainTree.AddWord(Word.ToString()))
                                break;

                            Thread.Yield();
                        }
                        Word.Clear();
                    }
                }
                while (!sr.EndOfStream && !AbortCalculating);
                sw.Stop();
                ReadingSpeed = 0;
                ProcessingSpeed = 0;
                MainTree.ProcessingTime += Convert.ToUInt64(sw.ElapsedMilliseconds);
                ProcessingTime = 0;
            }
            UpdateCaption();
            ProcessingFiles--;
            MainTree.FilesProcessed++;
            if (AbortCalculating && ProcessingFiles == 0)
            {
                UpdateStatusStrip(100, "Обработка прервана");
                AbortCalculating = false;
            }
            else
                UpdateStatusStrip(100, String.Format("Обработка файла \"{0}\" выполнена", Path.GetFileName(fname)));

            this.Invoke((Action)(() =>
            {
                файлToolStripMenuItem.Enabled = true;
                wikipediaToolStripMenuItem.Enabled = true;
                прерватьВыполнениеToolStripMenuItem.Enabled = false;
                RefreshChart();
            }));

            RefreshStatistics();
            GC.Collect(1);*/
        }
        private void ProcessFolder(string dname)
        {
            //FileChanged = true;
            //UpdateCaption();
            var DI = new DirectoryInfo(dname);
            StartProcessing(DI.EnumerateFiles("*.txt").ToArray());
            //var FIs = DI.EnumerateFiles("*.txt").ToArray();
            //var sw = new Stopwatch();
            //var Percents = new int[FIs.Length];
            //var TotalSize = FIs.Sum(fi => fi.Length);
            //var ShortFNames = FIs.Select(fi => fi.Name).ToArray();
            //UpdateStatusStrip(0, string.Format("Обработка {0} файлов ({1} Мб) из директории: {2}",
            //FIs.Length, (TotalSize / 1048576f).ToString("F1"), dname));

            //var ReadingSpeed = new float[FIs.Length];
            //var ProcessingSpeed = new float[FIs.Length];


            //sw.Start();
            //Parallel.For(0, FIs.Length, (i) =>
            /*{
                long iteration = 0;
                long lastiterationsvalue = 0;
                long lastswvalue = 0;
                int lastwordsvalue = 0;
                var LocalTree = new SyncCharTree();
                var Word = new StringBuilder();
                int WordsFound = 0;
                ProcessingFiles++;
                using (StreamReader sr = new StreamReader(FIs[i].FullName))
                {
                    do
                    {
                        var c = Convert.ToChar(sr.Read());
                        iteration++;

                        if (sr.BaseStream.Position > sr.BaseStream.Length * (Percents[i] + 5) / 100)
                        {
                            Percents[i] += 5;
                            UpdateStatusStrip(Percents.Sum() / FIs.Length);
                            ReadingSpeed[i] = (float)(iteration - lastiterationsvalue) / (sw.ElapsedMilliseconds - lastswvalue);
                            ProcessingSpeed[i] = (float)(WordsFound - lastwordsvalue) / (sw.ElapsedMilliseconds - lastswvalue);
                            ProcessingTime = sw.ElapsedMilliseconds;
                            lastswvalue = sw.ElapsedMilliseconds;
                            lastiterationsvalue = iteration;
                            lastwordsvalue = WordsFound;
                            this.ReadingSpeed = ReadingSpeed.Sum();
                            this.ProcessingSpeed = ProcessingSpeed.Sum();
                            this.Invoke((Action)(() => RefreshStatistics()));
                        }

                        if (char.IsLetter(c))
                        {
                            Word.Append(char.ToUpper(c));
                        }
                        else if (Word.Length != 0)
                        {
                            //WordsFound++;
                            while (true)
                            {
                                if (LocalTree.AddWord(Word.ToString()))
                                    break;

                                Thread.Yield();
                            }
                            Word.Clear();
                        }
                    }
                    while (!sr.EndOfStream && !AbortCalculating);
                    MainTree.AppendTree(LocalTree);
                    ProcessingFiles--;
                    MainTree.FilesProcessed++;
                }
            });
            sw.Stop();
            MainTree.ProcessingTime += Convert.ToUInt64(sw.ElapsedMilliseconds);*/
            /*ProcessingTime = 0;

            if (AbortCalculating && ProcessingFiles == 0)
            {
                UpdateStatusStrip(100, "Обработка прервана");
                AbortCalculating = false;
            }
            else
                UpdateStatusStrip(100, String.Format("Обработка файлов в  \"{0}\" выполнена", dname));*/

            //this.Invoke((Action)(() =>
            //{
            //    файлToolStripMenuItem.Enabled = true;
            //    wikipediaToolStripMenuItem.Enabled = true;
            //    прерватьВыполнениеToolStripMenuItem.Enabled = false;
            //    RefreshChart();
            //}));

            //RefreshStatistics();
            //GC.Collect(1);
        }
        private void StartProcessing(FileInfo[] Files)
        {
            // Выход если нет файлов
            if (Files.Length == 0)
                return;

            // Глобальная инициализация процесса обработки
            FileChanged = true;
            UpdateCaption();
            var CommonSW = new Stopwatch();
            var TotalSize = Files.Sum(f => f.Length);
            if (Files.Length > 1)
                UpdateStatusStrip(0, string.Format("Обработка {0} файлов ({1} Мб) из директории: {2}",
                    Files.Length, (TotalSize / 1048576f).ToString("F1"), Files.First().DirectoryName));
            else
                UpdateStatusStrip(0, "Обработка файла: " + Files[0].Name);

            // Локальная инициализация
            var LocalSW = new Stopwatch[Files.Length];
            var ShortFNames = Files.Select(fi => fi.Name).ToArray();
            var Percents = new int[Files.Length];
            var ReadingSpeed = new float[Files.Length];
            var ProcessingSpeed = new float[Files.Length];
            var States = new ProcessingState[Files.Length];
            var Streams = new StreamReader[Files.Length];
            var LocalTrees = new CharTree[Files.Length];

            // Параллельная обработка всех файлов
            CommonSW.Start();
            Parallel.For(0, Files.Length, i =>
            {
                // Инициализация
                ProcessingFiles++;
                States[i] = ProcessingState.Processing;
                long iteration = 0;
                long lastiterationsvalue = 0;
                long lastswvalue = 0;
                int lastwordsvalue = 0;
                var LocalTree = Files.Length == 1 ? MainTree : new SyncCharTree();
                LocalTrees[i] = LocalTree;
                var Word = new StringBuilder();
                int WordsFound = 0;
                LocalSW[i] = new Stopwatch();
                LocalSW[i].Start();

                using (Streams[i] = new StreamReader(Files[i].FullName))
                {
                    do
                    {
                        // Считывание символа
                        var c = Convert.ToChar(Streams[i].Read());
                        iteration++;

                        // Обновление визуализации данных
                        if (Streams[i].BaseStream.Position > Streams[i].BaseStream.Length * (Percents[i] + 5) / 100)
                        {
                            // Обновление локальных данных
                            Percents[i] += 5;
                            ReadingSpeed[i] = (float)(iteration - lastiterationsvalue) / (LocalSW[i].ElapsedMilliseconds - lastswvalue);
                            ProcessingSpeed[i] = (float)(WordsFound - lastwordsvalue) / (LocalSW[i].ElapsedMilliseconds - lastswvalue);
                            lastswvalue = LocalSW[i].ElapsedMilliseconds;
                            lastiterationsvalue = iteration;
                            lastwordsvalue = WordsFound;

                            // Обновление глобальных данных
                            this.LTProcessedChars = 0;
                            this.LTProcessedWords = 0;
                            for (int j = 0; j < LocalTrees.Length; j++)
                            {
                                if (LocalTrees[j] != null)
                                {
                                    this.LTProcessedChars += LocalTrees[j].ProcessedChars;
                                    this.LTProcessedWords += LocalTrees[j].ProcessedWords;
                                }
                            }
                            this.ReadingSpeed = ReadingSpeed.Sum();
                            this.ProcessingSpeed = ProcessingSpeed.Sum();
                            this.ProcessingTime = CommonSW.ElapsedMilliseconds;
                            UpdateStatusStrip(Percents.Sum() / Files.Length);
                            this.Invoke((Action)(() =>
                            {
                                RefreshStatistics();
                                RefreshFilesStats(Files, States, Percents, ReadingSpeed, ProcessingSpeed);
                            }));
                        }

                        // Обработка считанных данных
                        if (char.IsLetter(c))
                        {
                            Word.Append(char.ToUpper(c));
                        }
                        else if (Word.Length != 0)
                        {
                            WordsFound++;
                            while (true)
                            {
                                if (LocalTree.AddWord(Word.ToString()))
                                    break;

                                Thread.Sleep(0);
                            }
                            Word.Clear();
                        }
                    } // Прерывание выполнения при достижении конца потока, либо при прерывании выполнения
                    while (!Streams[i].EndOfStream && !AbortCalculating);
                    Percents[i] = 100;
                    ReadingSpeed[i] = 0;
                    ProcessingSpeed[i] = 0;
                }

                // Окончание обработки, объединение с основным деревом
                States[i] = ProcessingState.Merging;
                if (Files.Length != 1)
                    MainTree.AppendTree(LocalTree);
                ProcessingFiles--;
                MainTree.FilesProcessed++;
                States[i] = ProcessingState.Finished;
                LocalSW[i].Stop();
                LocalSW[i].Reset();
            });
            this.ReadingSpeed = 0;
            this.ProcessingSpeed = 0;
            LTProcessedChars = 0;
            LTProcessedWords = 0;
            CommonSW.Stop();
            ProcessingTime = 0;
            MainTree.ProcessingTime += Convert.ToUInt64(CommonSW.ElapsedMilliseconds);

            // Вывод статуса внизу
            if (AbortCalculating && ProcessingFiles == 0)
            {
                UpdateStatusStrip(100, "Обработка прервана");
                AbortCalculating = false;
            }
            else
            {
                if (Files.Length == 1)
                    UpdateStatusStrip(100, "Обработка файла выполнена");
                else
                    UpdateStatusStrip(100, String.Format("Обработка {0} файлов выполнена", Files.Length));
            }
            
            // Обновление интерфейса и чистка мусора
            this.Invoke((Action)(() =>
            {
                файлToolStripMenuItem.Enabled = true;
                wikipediaToolStripMenuItem.Enabled = true;
                прерватьВыполнениеToolStripMenuItem.Enabled = false;
                RefreshChart();
            }));
            RefreshStatistics();
            RefreshFilesStats(Files, States, Percents, ReadingSpeed, ProcessingSpeed);

            CommonSW = null;
            LocalSW = null;
            ShortFNames = null;
            Percents = null;
            ReadingSpeed = null;
            ProcessingSpeed = null;
            States = null;
            Streams = null;
            GC.Collect(1);
        }
        public enum ProcessingState
        {
            NotStarted,
            Processing,
            Merging,
            Finished
        }
        private void ProcessRandomPage()
        {
            var t = WikiGetRandomText();
            UpdateStatusStrip(0, "Обработка содержимого статьи \"" + t.Item1 + "\"");
            StringBuilder Word = new StringBuilder();
            
            for (int i = 0; i < t.Item2.Length; i++)
            {
                if (char.IsLetter(t.Item2[i]))
                {
                    Word.Append(char.ToUpper(t.Item2[i]));
                }
                else if (Word.Length != 0)
                {
                    while (true)
                    {
                        if (MainTree.AddWord(Word.ToString()))
                            break;

                        Thread.Sleep(0);
                    }
                    Word.Clear();
                }
            }

            UpdateStatusStrip(100, "Обработка статьи \"" + t.Item1 + "\" выполнена");
            RefreshStatistics();
            RefreshChart();
        }

        private void обработатьСлучайнуюСтатьюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                ProcessRandomPage();
            }
        }

        WebClient webClient = new WebClient();
        private Tuple<string,string> WikiGetRandomText()
        {
            try
            {
                var d = webClient.DownloadString("https://ru.wikipedia.org/w/api.php?action=query&grnnamespace=0&generator=random&rnlimit=5&format=json");
                var ee = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                var f = ee.Last.Last.Last.Last.Last.Last.Last.Last.ToString();
                var content = webClient.DownloadString(@"https://ru.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&explaintext=1&titles=" + f);
                var a = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var b = (Newtonsoft.Json.Linq.JObject)a;
                return Tuple.Create(f, b.Children().Last().Last().Last().Last().Last().Last().Last().Last().ToString());
            }
            catch
            {
                return null;
            }
        }
    }
}
