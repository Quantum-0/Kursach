using CharTrees;
using Newtonsoft.Json.Linq;
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
         * 
         * WriteОтчёт <= RefreshAll? ===> куча циферок всяких
         * Append 1+ Dicts
         * Добавить к видам textBoxWords
         * Вывод лога
         * - Выводить после выполнения, на сколько процентов увеличился словарь (кол-во различных слов)
         * - После обработки файла выводить, какой процент слов в файле - новые, а какие встречались ранее
         * Добавить "обработать N статей"
         * Обрабатывать статьи с вики "пачками"
         * Заменить Sort при выводе графика на сортировку выборкой, обрабатывающую первые 100-150 элементов ( O(100n) всё ж лучше чем O(n^2) )
         * Блокировать возможность работы со словарём в меню пока идт обработка
         * Tree Files Merging (Reduce)
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
            //var MidReadingSpeed = (LTProcessedChars != 0 ? LTProcessedChars : MainTree.ProcessedChars) / (1f + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
            var MidReadingSpeed = (LTProcessedChars + MainTree.ProcessedChars) / (1f + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
            var kbps = MidReadingSpeed * 1000 / 1024;
            if (kbps > 512)
                return (kbps / 1024).ToString("F2") + " Мб/с";
            else if (kbps > 10)
                return kbps.ToString("F1") + " Кб/с";
            else if (kbps > 2)
                return kbps.ToString("F2") + " Кб/с";
            else
                return Math.Floor(kbps * 1024) + " б/с";
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
            var MidProcessingSpeed = (float)(LTProcessedWords != 0 ? LTProcessedWords : MainTree.ProcessedWords) / (1 + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
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
                dataGridStatistics[0, 3].Value = "Средняя скорость построения словаря:";
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
        private void RefreshFilesStats(string[] ArticleTitles, int[] Lengths, ProcessingState[] states, int[] percents, float[] ReadingSpeeds, float[] ProcessingSpeeds)
        {
            this.Invoke((Action)(() =>
            {
                dataGridFilesProcessing.RowCount = Lengths.Length;
                for (int i = 0; i < ArticleTitles.Length; i++)
                {
                    dataGridFilesProcessing[0, i].Value = ArticleTitles[i];
                    dataGridFilesProcessing[1, i].Value = states[i];
                    dataGridFilesProcessing[2, i].Value = percents[i] + "%";
                    dataGridFilesProcessing[3, i].Value = Lengths[i] / 1024f + " Кб";
                    dataGridFilesProcessing[4, i].Value = ReadingSpeeds[i] / 1024f * 1000 + " Кб/с";
                    dataGridFilesProcessing[5, i].Value = (ProcessingSpeeds[i] * 1000f) + " Слов/сек";
                }
            }));
        }
        private void RefreshFilesStats()
        {
            this.Invoke((Action)(() => dataGridFilesProcessing.RowCount = 0));
        }
        private void RefreshChart()
        {
            Log("Обновление графика и списка слов");
            this.Invoke((Action)(() =>
            {
                Func<int, int> Xvisualization = x => x;
                Func<double, double> Yvisualization = y => y;// Math.Pow(y, 1.0 / 2.5);
                double Dispersion = 0;
                if (chart.Visible)
                {
                    textBoxWords.Clear();
                    chart.Series[0].Points.Clear();
                    chart.Series[1].Points.Clear();
                    var list = MainTree.Export();
                    list.Sort();
                    var maxX = Math.Min(list.Count, 100);// Convert.ToInt32(Math.Floor(Math.Pow(list.Count, 1d / 2)));
                    var chartdata = new List<WordCountPair>();
                    for (int i = 0; i < maxX; i++)
                    {
                        chartdata.Add(list[Xvisualization(i)]);
                    }

                    /*var FuncConst = 0d;
                    for (int j = 0; j < Math.Min(chartdata.Count, 3); j++)
                        FuncConst += j * Convert.ToDouble(chartdata[j].Count) / 3;
                        */


                    for (int i = 1; i < chartdata.Count; i++)
                    {
                        var y1 = Convert.ToDouble(chartdata[i-1].Count);
                        //var y2 = FuncConst / (i);
                        var y2 = chartdata[0].Count / i;
                        chart.Series[0].Points.AddXY(i, Yvisualization(y1));
                        chart.Series[1].Points.AddXY(i, Yvisualization(y2));
                        textBoxWords.AppendText(i + ")" + chartdata[i-1] + " (" + y2 + ") \r\n");
                        Dispersion += (y1 - y2) * (y1 - y2) / chartdata.Count;
                    }
                }
                textBoxWords.Select(0, 0);
                textBoxWords.ScrollToCaret();
            }));
        }
        private void RefreshAll()
        {
            RefreshStatistics();
            RefreshFilesStats();
            RefreshChart();
        }
        private void Log(string Text)
        {
            this.Invoke((Action)(() =>
            {
                if (textBoxLog.Focused)
                {
                    int pos = textBoxLog.SelectionStart;
                    int len = textBoxLog.SelectionLength;

                    if (textBoxLog.Text != "")
                        textBoxLog.Text += string.Format("\r\n[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text);
                    else
                        textBoxLog.Text += string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text);

                    textBoxLog.Select(pos, len);
                    textBoxLog.ScrollToCaret();
                }
                else
                {
                    if (textBoxLog.Text != "")
                        textBoxLog.AppendText(string.Format("\r\n[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text));
                    else
                        textBoxLog.AppendText(string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text));
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
            RefreshAll();
        }
        private void AppendFile_Click(object sender, EventArgs e)
        {
            openDictDialog.Multiselect = true;
            if (openDictDialog.ShowDialog() != DialogResult.OK)
                return;
            openDictDialog.Multiselect = false;
            foreach (var item in openDictDialog.FileNames)
            {
                var tr = TreeWorker.LoadTreeFromFile(item);
                MainTree.AppendTree(tr);
            }
            if (openDictDialog.FileNames.Length == 1)
                Log("Словарь дополнен из другого словаря");
            else
                Log("Словарь дополнен из других словарей");
            FileChanged = true;
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
            if (string.IsNullOrWhiteSpace(CurrentFile))
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
                Log("Словарь загружен из файла");
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
                Log("Словарь сохранён в файл");
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

        private void отображатьСтатистикуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridStatistics.Visible = отображатьСтатистикуToolStripMenuItem.Checked;
            splitContainer1.Panel1Collapsed = false;
            RefreshControlsLocations();
        }
        private void отображатьГрафикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chart.Visible = отображатьГрафикToolStripMenuItem.Checked;
            splitContainer1.Panel1Collapsed = false;
            RefreshControlsLocations();
            RefreshChart();
        }
        private void отображатьЛогToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxLog.Visible = отображатьЛогToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = false;
            RefreshControlsLocations();
        }
        private void отображатьПрогрессToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridFilesProcessing.Visible = отображатьПрогрессToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = false;
            RefreshControlsLocations();
        }
        private void RefreshControlsLocations()
        {
            // Top
            if (chart.Visible)
            {
                if (dataGridStatistics.Visible)
                {
                    dataGridStatistics.Width = 271;
                    chart.Left = 280;
                    chart.Width = splitContainer1.Panel1.Right - chart.Location.X - 3;
                }
                else
                {
                    chart.Location = new Point(3, 3);
                    chart.Size = new Size(splitContainer1.Panel1.Right - 6, splitContainer1.Panel1.Bottom - 6);
                }
            }
            else
            {
                if (dataGridStatistics.Visible)
                {
                    dataGridStatistics.Size = new Size(splitContainer1.Panel1.Right - 6, splitContainer1.Panel1.Bottom - 6);
                }
                else
                {
                    splitContainer1.Panel1Collapsed = true;
                }
            }
            // Bottom
            if (dataGridFilesProcessing.Visible)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Height = 65;
                    textBoxLog.Top = splitContainer1.Panel2.Bottom - splitContainer1.Panel2.Top - 65 - 3;
                    dataGridFilesProcessing.Top = 3;
                    dataGridFilesProcessing.Height = textBoxLog.Top - dataGridFilesProcessing.Top - 6;
                }
                else
                {
                    dataGridFilesProcessing.Top = 3;
                    dataGridFilesProcessing.Height = splitContainer1.Panel2.Height - 6;
                }
            }
            else
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Top = 3;
                    textBoxLog.Height = splitContainer1.Panel2.Height - 6;
                }
                else
                {
                    splitContainer1.Panel2Collapsed = true;
                }
            }
        }

        private void прерватьВыполнениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbortCalculating = true;
            UpdateStatusStrip(100, "Прерывание обработки..");
            Log("Выполняется прерывание обработки");
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
        private void обработатьТекстовыйФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;

            Task.Run(() => ProcessSingleFile(openFileDialog.FileName));
        }
        private void обработатьСлучайнуюСтатьюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;
            ProcessRandomPage();
        }
        private void запуститьОбработкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*for (int i = 0; i < 250; i++)
            {
                ProcessRandomPage();
            }*/
            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;
            ProcessRandomPages();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ProcessingFiles > 0)
            {
                MessageBox.Show("Невозможно прервать пока выполняется обработка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            if (!FileChanged)
                return;

            var dr = AskForSaving();
            if (dr == DialogResult.No)
                return;

            if (dr == DialogResult.Yes)
                e.Cancel = !SaveFile();
            else
                e.Cancel = true;
        }

        private void ProcessSingleFile(string fname)
        {
            Log("Запрос на обработку файла");
            StartProcessing(new FileInfo[] { new FileInfo(fname) });
        }
        private void ProcessFolder(string dname)
        {
            Log("Запрос на обработку файлов из папки");
            var DI = new DirectoryInfo(dname);
            StartProcessing(DI.EnumerateFiles("*.txt").ToArray());
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
            Log("Начало параллельной обработки файлов");
            CommonSW.Start();
            Parallel.For(0, Files.Length, i =>
            {
                // Инициализация
                ProcessingFiles++;
                States[i] = ProcessingState.Processing;
                Log($"- Начало обработки файла {ShortFNames[i]}");
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
                Log($"- Файл {ShortFNames[i]} обработан");

                // Окончание обработки, объединение с основным деревом
                States[i] = ProcessingState.Merging;
                if (Files.Length != 1)
                    MainTree.AppendTree(LocalTree);
                LocalTree = null;
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
                Log("Параллельная обработка была прервана");
                AbortCalculating = false;
            }
            else
            {
                if (Files.Length == 1)
                    UpdateStatusStrip(100, "Обработка файла выполнена");
                else
                    UpdateStatusStrip(100, String.Format("Обработка {0} файлов выполнена", Files.Length));
                
                Log("Параллельная обработка файлов завершена");
            }
            
            // Обновление интерфейса
            this.Invoke((Action)(() =>
            {
                файлToolStripMenuItem.Enabled = true;
                wikipediaToolStripMenuItem.Enabled = true;
                прерватьВыполнениеToolStripMenuItem.Enabled = false;
                RefreshChart();
            }));
            RefreshStatistics();
            RefreshFilesStats(Files, States, Percents, ReadingSpeed, ProcessingSpeed);

            // Чистка мусора
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
            Finished,
            Downloading,
            Aborted
        }
        private void ProcessRandomPage()
        {
            Task.Run(() =>
            {
                // Инициализация процесса обработки
                Log("Начало обработки случайно статьи с Википедии");
                FileChanged = true;
                ProcessingFiles++;
                UpdateCaption();
                UpdateStatusStrip(0, "Загрузка случайной статьи");
                var ReadingSpeed = new float[1];
                var ProcessingSpeed = new float[1];
                var States = new ProcessingState[1];
                States[0] = ProcessingState.Downloading;
                StringReader Stream;
                var Percents = new int[1];
                var CommonSW = new Stopwatch();
                RefreshFilesStats(new string[] { "???" }, new int[] { 0 }, States, Percents, ReadingSpeed, ProcessingSpeed);
                Log("Скачивание..");

                // Получение текста
                var ArticleTask = WikiGetRandomText();

                // Начало обработки
                var Article = /*await*/ ArticleTask;
                if (Article == null)
                    throw new Exception("Не скачалось :с");
                var TotalSize = Article.Item2.Length;
                States[0] = ProcessingState.Processing;
                RefreshFilesStats(new string[] { Article.Item1 }, new int[] { TotalSize }, States, Percents, ReadingSpeed, ProcessingSpeed);
                Stream = new StringReader(Article.Item2);
                UpdateStatusStrip(0, "Обработка содержимого статьи \"" + Article.Item1 + "\"");
                CommonSW.Start();

                // Инициализация
                long iteration = 0;
                long lastiterationsvalue = 0;
                long lastswvalue = 0;
                int lastwordsvalue = 0;
                //var LocalTree = Files.Length == 1 ? MainTree : new SyncCharTree();
                var LocalTree = MainTree;
                var Word = new StringBuilder();
                int WordsFound = 0;
                Log("Обработка текста скаченной статьи");

                while (iteration < Article.Item2.Length && !AbortCalculating)
                {
                    // Считывание символа
                    var c = Convert.ToChar(Stream.Read());
                    iteration++;

                    // Обновление визуализации данных
                    if (iteration > Article.Item2.Length * (Percents[0] + 5) / 100)
                    {
                        // Обновление локальных данных
                        Percents[0] += 5;
                        ReadingSpeed[0] = (float)(iteration - lastiterationsvalue) / (CommonSW.ElapsedMilliseconds - lastswvalue);
                        ProcessingSpeed[0] = (float)(WordsFound - lastwordsvalue) / (CommonSW.ElapsedMilliseconds - lastswvalue);
                        lastswvalue = CommonSW.ElapsedMilliseconds;
                        lastiterationsvalue = iteration;
                        lastwordsvalue = WordsFound;

                        // Обновление глобальных данных
                        this.LTProcessedChars = LocalTree.ProcessedChars;
                        this.LTProcessedWords += LocalTree.ProcessedWords;

                        this.ReadingSpeed = ReadingSpeed.Sum();
                        this.ProcessingSpeed = ProcessingSpeed.Sum();
                        this.ProcessingTime = CommonSW.ElapsedMilliseconds;
                        UpdateStatusStrip(Percents.Sum() / 1);
                        this.Invoke((Action)(() =>
                        {
                            RefreshStatistics();
                            RefreshFilesStats(new string[] { Article.Item1 }, new int[] { TotalSize }, States, Percents, ReadingSpeed, ProcessingSpeed);
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
                }
                Percents[0] = 100;
                ReadingSpeed[0] = 0;
                ProcessingSpeed[0] = 0;

                // Окончание обработки, объединение с основным деревом
                States[0] = ProcessingState.Merging;
                //if (Files.Length != 1)
                //  MainTree.AppendTree(LocalTree);
                ProcessingFiles--;
                MainTree.FilesProcessed++;
                States[0] = ProcessingState.Finished;
                this.ReadingSpeed = 0;
                this.ProcessingSpeed = 0;
                LTProcessedChars = 0;
                LTProcessedWords = 0;
                CommonSW.Stop();
                ProcessingTime = 0;
                MainTree.ProcessingTime += Convert.ToUInt64(CommonSW.ElapsedMilliseconds);

                // Вывод статуса внизу
                if (AbortCalculating)
                {
                    UpdateStatusStrip(100, "Обработка прервана");
                    Log("Обработка была прервана");
                    AbortCalculating = false;
                }
                else
                {
                    UpdateStatusStrip(100, "Обработка статьи завершена");
                    Log("Обработка текста завершена");
                }

                // Обновление интерфейса
                this.Invoke((Action)(() =>
                {
                    файлToolStripMenuItem.Enabled = true;
                    wikipediaToolStripMenuItem.Enabled = true;
                    прерватьВыполнениеToolStripMenuItem.Enabled = false;
                    RefreshChart();
                }));
                RefreshStatistics();
                RefreshFilesStats(new string[] { Article.Item1 }, new int[] { TotalSize }, States, Percents, ReadingSpeed, ProcessingSpeed);

                // Чистка мусора
                CommonSW = null;
                Percents = null;
                ReadingSpeed = null;
                ProcessingSpeed = null;
                States = null;
                Stream = null;
                GC.Collect(1);
            });
        }
        private void ProcessRandomPages()
        {
            Task.Run(() =>
            {
                Log("Запуск обработки случайных статей");
                do {

                    // Инициализация процесса обработки
                    UpdateStatusStrip(0, "Загрузка списка случайных статей");
                    Log("Загрузка списка статей");
                    var Articles = WikiGetRandomTitles();
                    if (Articles == null)
                    {
                        UpdateStatusStrip(0, "Ошибка загрузки");
                        return;
                    }
                    FileChanged = true;
                    UpdateCaption();
                    var CommonSW = new Stopwatch();

                    // Локальная инициализация
                    var Percents = new int[100];
                    var States = new ProcessingState[100];
                    var Lenghts = new int[100];
                    var LocalSW = new Stopwatch[100];
                    var ReadingSpeed = new float[100];
                    var ProcessingSpeed = new float[100];
                    var ProccessedChars = new int[100];
                    var ProccessedWords = new int[100];
                    ProcessingFiles += 100;
                    RefreshFilesStats(Articles.Item2, Lenghts, States, Percents, ReadingSpeed, ProcessingSpeed);
                    CommonSW.Start();
                    
                    Log("Список статей загружен, запуск параллельного скачивания и обработки текстов");
                    Parallel.For(0, 100, i =>
                    {
                        if (AbortCalculating)
                        {
                            States[i] = ProcessingState.Aborted;
                            return;
                        }

                        Log($"- Скачивание статьи \"{Articles.Item2[i]}\"");
                        var DownloadingID = WikiStartGettingTextById(Articles.Item1[i]);
                        // Загрузка
                        States[i] = ProcessingState.Downloading;
                        RefreshFilesStats(Articles.Item2, Lenghts, States, Percents, ReadingSpeed, ProcessingSpeed);
                        var Text = WikiFinishGettingTextById(DownloadingID);

                        // Инициализация
                        Log($"- Статья \"{Articles.Item2[i]}\" загружена, начало обработки текста");
                        int iteration = 0;
                        int lastiterationsvalue = 0;
                        long lastswvalue = 0;
                        int lastwordsvalue = 0;
                        var Word = new StringBuilder();
                        int WordsFound = 0;
                        Lenghts[i] = Text.Length;
                        LocalSW[i] = new Stopwatch();
                        States[i] = ProcessingState.Processing;
                        RefreshFilesStats(Articles.Item2, Lenghts, States, Percents, ReadingSpeed, ProcessingSpeed);
                        //Refresh();
                        var Stream = new StringReader(Text);
                        LocalSW[i].Start();

                        while (iteration < Text.Length && !AbortCalculating)
                        {
                            // Считывание символа
                            var c = Convert.ToChar(Stream.Read());
                            iteration++;

                            // Обновление визуализации данных
                            if (iteration > Text.Length * (Percents[i] + 10) / 100)
                            {
                                // Обновление локальных данных
                                Percents[i] += 10;
                                ReadingSpeed[i] = (float)(iteration - lastiterationsvalue) / (LocalSW[i].ElapsedMilliseconds - lastswvalue);
                                ProcessingSpeed[i] = (float)(WordsFound - lastwordsvalue) / (LocalSW[i].ElapsedMilliseconds - lastswvalue);
                                lastswvalue = LocalSW[i].ElapsedMilliseconds;
                                lastiterationsvalue = iteration;
                                lastwordsvalue = WordsFound;
                                ProccessedChars[i] = iteration;
                                ProccessedWords[i] = WordsFound;

                                // Обновление глобальных данных
                                /*this.LTProcessedChars = 0;
                                this.LTProcessedWords = 0;
                                for (int j = 0; j < 100; j++)
                                {
                                    this.LTProcessedChars += (ulong)ProccessedChars[j];
                                    this.LTProcessedWords += (ulong)ProccessedWords[j];
                                }*/
                                this.ReadingSpeed = ReadingSpeed.Sum();
                                this.ProcessingSpeed = ProcessingSpeed.Sum();
                                this.ProcessingTime = CommonSW.ElapsedMilliseconds;
                                UpdateStatusStrip(Percents.Sum() / 100);
                                RefreshStatistics();
                                RefreshFilesStats(Articles.Item2, Lenghts, States, Percents, ReadingSpeed, ProcessingSpeed);
                                //Refresh();
                            }

                            // Обработка считанных данных
                            if (char.IsLetter(c))
                            {
                                Word.Append(char.ToUpper(c));
                            }
                            else if (Word.Length != 0)
                            {
                                WordsFound++;
                                lock (MainTree)
                                {
                                    while (true)
                                    {
                                        if (MainTree.AddWord(Word.ToString()))
                                            break;

                                        Thread.Sleep(0);
                                    }
                                }
                                Word.Clear();
                            }
                        }
                        Percents[i] = 100;
                        ReadingSpeed[i] = 0;
                        ProcessingSpeed[i] = 0;

                        // Окончание обработки
                        ProcessingFiles--;
                        MainTree.FilesProcessed++;
                        if (AbortCalculating)
                            States[i] = ProcessingState.Aborted;
                        else
                            States[i] = ProcessingState.Finished;
                        LocalSW[i].Stop();
                        LocalSW[i].Reset();
                        Log($"- Обработка статьи \"{Articles.Item2[i]}\" завершена");
                    });
                    Log("Параллельная обработка статей завершена");
                    this.ReadingSpeed = 0;
                    this.ProcessingSpeed = 0;
                    LTProcessedChars = 0;
                    LTProcessedWords = 0;
                    CommonSW.Stop();
                    ProcessingTime = 0;
                    MainTree.ProcessingTime += Convert.ToUInt64(CommonSW.ElapsedMilliseconds);
                    
                    RefreshChart();
                    RefreshStatistics();
                    RefreshFilesStats(Articles.Item2, Lenghts, States, Percents, ReadingSpeed, ProcessingSpeed);

                    // Чистка мусора
                    CommonSW = null;
                    Percents = null;
                    ReadingSpeed = null;
                    ProcessingSpeed = null;
                    ProccessedWords = null;
                    ProccessedChars = null;
                    States = null;
                    GC.Collect(1);
                }
                while (!AbortCalculating);

                // Вывод статуса внизу
                if (AbortCalculating)
                {
                    UpdateStatusStrip(100, "Обработка прервана");
                    Log("Обработка была прервана");
                    AbortCalculating = false;
                    ProcessingFiles = 0;
                }
                else
                {
                    UpdateStatusStrip(100, "Обработка статей завершена");
                }

                // Обновление интерфейса после выхода из обработки
                this.Invoke((Action)(() =>
                {
                    файлToolStripMenuItem.Enabled = true;
                    wikipediaToolStripMenuItem.Enabled = true;
                    прерватьВыполнениеToolStripMenuItem.Enabled = false;
                }));
            });
        }

        private Tuple<string,string> WikiGetRandomText()
        {
            try
            {
                var g = Downloader.StartDownloadString("https://ru.wikipedia.org/w/api.php?action=query&grnnamespace=0&generator=random&rnlimit=5&format=json");
                var d = Downloader.WaitForDownloaded(g);
                var ee = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                var f = ee.Last.Last.Last.Last.Last.Last.Last.Last.ToString();
                var content = Downloader.WaitForDownloaded(Downloader.StartDownloadString(@"https://ru.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&explaintext=1&titles=" + f));
                var a = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var b = (Newtonsoft.Json.Linq.JObject)a;
                return Tuple.Create(f, b.Children().Last().Last().Last().Last().Last().Last().Last().Last().ToString());
            }
            catch
            {
                return null;
            }
        }
        private Guid WikiStartGettingTextById(string id)
        {
            return Downloader.StartDownloadString("https://ru.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&pageids=" + id + "&exlimit=1&explaintext=1&exsectionformat=plain");
        }
        private string WikiFinishGettingTextById(Guid guid)
        {
            try
            {
                var d = Downloader.WaitForDownloaded(guid);
                var dd = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                var c = dd.Last.Last.Last.Last.Last.Last.Last.Last.ToString();
                return c;
            }
            catch
            {
                return "";
            }
        }
        private Tuple<string[], string[]> WikiGetRandomTitles()
        {
            try
            {
                var d = Downloader.WaitForDownloaded(Downloader.StartDownloadString("https://ru.wikipedia.org/w/api.php?action=query&format=json&list=&generator=random&grnnamespace=0&grnlimit=100"));
                //   action=query&format=json&prop=extracts&titles=A%7CB%7CC&utf8=1&exlimit=1&explaintext=1&exsectionformat=plain
                var ee = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                // [JSON].query.pages.625963
                var f = (JObject)ee.Last.Last.Last.Last;
                var t = f.Properties().Select(u => u.Name).ToArray();
                var t2 = f.Properties().Select(u => u.Last.Last.Last.ToString()).ToArray();

                return Tuple.Create(t, t2);
            }
            catch
            {
                return null;
            }
        }

        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (var item in chart.Series[0].Points)
            {
                item.Color = chart.Series[0].Color;
            }
            
            if (chart.Series[0].Points.Count == 0)
                return;

            var x = (int)Math.Round(chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X)) - 1;
            if (x >= 0 && x < chart.Series[0].Points.Last().XValue)
                chart.Series[0].Points[x].Color = System.Drawing.Color.Red;
        }
    }
}
