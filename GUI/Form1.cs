﻿using CharTrees;
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
         * ================================== TODO ===================================
         * Tree Files Merging (объединение 2 или более достаточно больших файлов,
         * не помещая их в ОЗУ целиком, используя MergeSort)
         * 
         * лематизация (http://lemmatise.ijs.si/Software/Version3,
         * http://www.solarix.ru/for_developers/api/lemmatization.shtml)
         * 
         * бот (Telegram-bot, хранящий список subscribers (комманды /subscribe & /unsubscribe),
         * периодически высылающий подписчикам статистику обработки данных и изображение графика)
         * 
         * бэкап (каждые N обработанных кб/мб словарь автоматически сохраняется во временный файл,
         * использующийся для восстановления в случае если внезапно всё падает)
         */

        public PrefixTree MainTree;
        public string CurrentFile;
        public bool FileChanged;
        private const string Caption = "Курсач";
        public bool AbortCalculating = false;
        public ulong LTProcessedChars;
        public ulong LTProcessedWords;
        public double Averange;
        public List<string> Words = new List<string>();

        public string GetReadingSpeed()
        {
            // ReadingSpeed - скорость чтения, байт/мс
            // ReadingSpeed * 1000 / 1024 - кб/с
            var kbps = ProcessingEntitiesInfo.ReadingSpeed * 1000 / 1024;
            if (kbps > 512)
                return (kbps / 1024).ToString("F2") + " Мб/с";
            else
                return kbps.ToString("F1") + " Кб/с";
        }
        public string GetMidReadingSpeed()
        {
            //var MidReadingSpeed = (LTProcessedChars != 0 ? LTProcessedChars : MainTree.ProcessedChars) / (1f + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
            var MidReadingSpeed = (LTProcessedChars + MainTree.ProcessedChars) / (1f + MainTree.ProcessingTime + ProcessingEntitiesInfo.ProcessingTime);
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
            var wps = ProcessingEntitiesInfo.ProcessingSpeed * 1000;
            if (wps > 5000)
                return (wps / 1000).ToString("F1") + " тыс.слов/сек";
            else
                return wps + " слов/сек";
        }
        public string GetMidProcessingSpeed()
        {
            var MidProcessingSpeed = (float)(LTProcessedWords != 0 ? LTProcessedWords : MainTree.ProcessedWords) / (1 + MainTree.ProcessingTime + ProcessingEntitiesInfo.ProcessingTime);
            var wps = MidProcessingSpeed * 1000;
            if (wps > 5000)
                return (wps / 1000).ToString("F1") + " тыс.слов/сек";
            else
                return wps + " слов/сек";
        }
        public string GetProcessedFiles()
        {
            return MainTree.FilesProcessed + " завершено, " + ProcessingEntitiesInfo.Count + " в обработке";
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
            else if (kwords < 1500)
                return Math.Round(kwords) + " тыс.слов";
            else
                return (kwords / 1000).ToString("F2") + " млн.слов";
        }
        public string GetDifferentWords()
        {
            var kwords = MainTree.DifferentWords / 1000f;
            if (kwords < 100)
                return kwords.ToString("F2") + " тыс.слов";
            else if (kwords < 700)
                return kwords.ToString("F1") + " тыс.слов";
            else
                return (kwords / 1000).ToString("F2") + " млн.слов";
        }
        public string GetTotalProcessingTime()
        {
            var ticks = Convert.ToInt64(TimeSpan.TicksPerMillisecond * MainTree.ProcessingTime + ProcessingEntitiesInfo.ProcessingTime);
            var time = new TimeSpan(ticks);

            if (time.TotalHours > 1)
            {
                return $"{Math.Floor(time.TotalHours)} ч. {time.Minutes} мин";
            }
            else if (time.TotalSeconds > 1000)
            {
                return $"{Math.Floor(time.TotalMinutes)} мин {time.Seconds} сек";
            }
            else if (time.TotalSeconds > 100)
            {
                return $"{Math.Floor(time.TotalSeconds)} сек";
            }
            else
            {
                return $"{(time.TotalMilliseconds / 1000f).ToString("F1")} сек";
            }
        }
        public string GetAverange()
        {
            return (Averange * 100).ToString("F1") + "%";
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshStatistics(true);
            UpdateCaption();
            MainTree = new SyncPrefixTree();
            var colorslist = new List<Color>();
            colorslist.Add(Color.FromArgb(192, 192, 192, 192));
            colorslist.Add(Color.FromArgb(64, Color.GreenYellow));
            chart.PaletteCustomColors = colorslist.ToArray();
            ProcessingEntity.LogEvent += (s, ea) => Log(ea.Text);
        }

        // Обновление выводимых данных
        private void RefreshStatistics(bool Init = false)
        {
            Invoke((Action)(() =>
            {
                if (Init)
                {
                    dataGridStatistics.RowCount = 11;
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
                    dataGridStatistics[0, 10].Value = "Ср.откл. от з.Ципфа"; // Средне-квадратичное отклонение от закона ципфа
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
                    dataGridStatistics[1, 10].Value = GetAverange();
                }
                else
                {
                    for (int i = 0; i < dataGridStatistics.RowCount; i++)
                    {
                        dataGridStatistics[1, i].Value = "Словарь не открыт";
                    }
                }
            }));
        }
        private void RefreshFilesStats(IEnumerable<ProcessingEntity> ProcessingEntities)
        {
            this.Invoke((Action)(() =>
            {
                var data = ProcessingEntities.ToArray();
                dataGridFilesProcessing.RowCount = data.Length;
                for (int i = 0; i < data.Length; i++)
                {
                    dataGridFilesProcessing[0, i].Value = data[i].Name;
                    dataGridFilesProcessing[1, i].Value = data[i].State;
                    dataGridFilesProcessing[2, i].Value = data[i].Progress + "%";
                    dataGridFilesProcessing[3, i].Value = data[i].Length / 1024f + " Кб";
                    dataGridFilesProcessing[4, i].Value = data[i].ReadingSpeed / 1024f * 1000 + " Кб/с";
                    dataGridFilesProcessing[5, i].Value = (data[i].ProcessingSpeed * 1000f) + " Слов/сек";
                }
            }));
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
                for (int i = 0; i < Lengths.Length; i++)
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
            Log("Обновление графика и списка слов..");
            this.Invoke((Action)(async () =>
            {
                Func<int, int> Xvisualization = x => x;
                Func<double, double> Yvisualization = y => y;// Math.Pow(y, 1.0 / 2.5);
                Averange = 0;
                if (chart.Visible)
                {
                    listBoxWords.Items.Clear();
                    Words.Clear();
                    chart.Series[0].Points.Clear();
                    chart.Series[1].Points.Clear();
                    var list = await Task.Run(() => { return WordCountPair.Sort(MainTree.Export(), 100); });
                    var maxX = Math.Min(list.Count, 100);
                    var chartdata = new List<WordCountPair>();
                    for (int i = 0; i < maxX; i++)
                    {
                        chartdata.Add(list[Xvisualization(i)]);
                        Words.Add(list[Xvisualization(i)].Word);
                    }

                    /*var FuncConst = 0d;
                    for (int j = 0; j < Math.Min(chartdata.Count, 3); j++)
                        FuncConst += j * Convert.ToDouble(chartdata[j].Count) / 3;
                        */


                    for (int i = 1; i < chartdata.Count; i++)
                    {
                        var y1 = Convert.ToDouble(chartdata[i-1].Count);
                        //var y2 = FuncConst / (i);
                        var y2 = (double)chartdata[0].Count / i;
                        chart.Series[0].Points.AddXY(i, Yvisualization(y1));
                        chart.Series[1].Points.AddXY(i, Yvisualization(y2));
                        listBoxWords.Items.Add($"{i}) {chartdata[i - 1]} {Environment.NewLine}");
                        var absdiff = (y1 - y2);
                        var reldiff = absdiff / Math.Max(y1, y2);
                        Averange += reldiff / chartdata.Count;
                    }
                }
            }));
        }
        private void RefreshAll()
        {
            RefreshStatistics();
            RefreshFilesStats(ProcessingEntitiesInfo.ProcessingEntities);
            RefreshChart();
        }
        private void Log(string Text)
        {
            this.Invoke((Action)(() =>
            {
                if (textBoxLog.Lines.Length > 300)
                    textBoxLog.Lines = textBoxLog.Lines.Skip(50).ToArray();

                if (textBoxLog.Focused)
                {
                    int pos = textBoxLog.SelectionStart;
                    int len = textBoxLog.SelectionLength;

                    if (textBoxLog.Text != "")
                        textBoxLog.Text += string.Format("{2}[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text, Environment.NewLine);
                    else
                        textBoxLog.Text += string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text);

                    textBoxLog.Select(pos, len);
                    textBoxLog.ScrollToCaret();
                }
                else
                {
                    if (textBoxLog.Text != "")
                        textBoxLog.AppendText(string.Format("{2}[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text, Environment.NewLine));
                    else
                        textBoxLog.AppendText(string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), Text));
                }
            }));
        }
        private void LockMenu(bool Lock)
        {
            Invoke((Action)(() => {
                файлToolStripMenuItem.Enabled = !Lock;
                wikipediaToolStripMenuItem.Enabled = !Lock;
                прерватьВыполнениеToolStripMenuItem.Enabled = Lock;
                словарьToolStripMenuItem.Enabled = !Lock;
                отображатьГрафикToolStripMenuItem.Enabled = !Lock;
            }));
        }

        // Обработчики нажатий кнопок в вернем меню
        private async void OpenFile_Click(object sender, EventArgs e)
        {
            await OpenFile();
        }
        private async void SaveFile_Click(object sender, EventArgs e)
        {
            await SaveFile();
        }
        private async void SaveAsFile_Click(object sender, EventArgs e)
        {
            await SaveAsFile();
        }
        private async void NewFile_Click(object sender, EventArgs e)
        {
            await NewFile();
            RefreshAll();
        }
        private async void AppendFile_Click(object sender, EventArgs e)
        {
            openDictDialog.Multiselect = true;
            if (openDictDialog.ShowDialog() != DialogResult.OK)
                return;
            openDictDialog.Multiselect = false;
            Log("Чтение файлов..");
            UpdateStatusStrip(0, "Открытие файлов..");
            await Task.Run(() =>
            {
                var trs = new PrefixTree[openDictDialog.FileNames.Length];
                Parallel.For(0, trs.Length, i =>
                {
                    trs[i] = TreeWorker.LoadTreeFromFile(openDictDialog.FileNames[i]);
                });
                Log("Добавление к дереву..");
                UpdateStatusStrip(50);
                for (int i = 0; i < trs.Length; i++)
                {
                    MainTree.AppendTree(trs[i]);
                    UpdateStatusStrip(50 + (int)(50f*i/trs.Length));
                }
            });
            if (openDictDialog.FileNames.Length == 1)
                Log("Словарь дополнен из другого словаря");
            else
                Log("Словарь дополнен из других словарей");
            FileChanged = true;
            RefreshAll();
            UpdateStatusStrip(100, "Выбранные словари добавлены в текущий");
        }

        // UI для открытия/сохранения словарей
        private async Task<bool> NewFile()
        {
            if (!FileChanged)
            {
                MainTree = new SyncPrefixTree();
                CurrentFile = String.Empty;
                FileChanged = false;
                UpdateCaption();
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
                    MainTree = new SyncPrefixTree();
                    CurrentFile = String.Empty;
                    FileChanged = false;
                    UpdateCaption();
                    GC.Collect(2);
                    return true;
                }
                else //res == Yes
                {
                    if (await SaveFile())
                    {
                        MainTree = new SyncPrefixTree();
                        CurrentFile = String.Empty;
                        FileChanged = false;
                        UpdateCaption();
                        GC.Collect(2);
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
        private async Task<bool> OpenFile()
        {
            if (!FileChanged)
            {
                if (openDictDialog.ShowDialog() == DialogResult.OK)
                    return await JustOpen(openDictDialog.FileName);
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
                        return await JustOpen(openDictDialog.FileName);
                    else
                        return false;
                }
                else //res == Yes
                {
                    if (await SaveFile())
                    {
                        if (openDictDialog.ShowDialog() == DialogResult.OK)
                            return await JustOpen(openDictDialog.FileName);
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
        }
        private async Task<bool> SaveAsFile()
        {
            if (saveDictDialog.ShowDialog() == DialogResult.OK)
                return await JustSave(saveDictDialog.FileName);
            else
                return false;
        }
        private async Task<bool> SaveFile()
        {
            if (string.IsNullOrWhiteSpace(CurrentFile))
                return await SaveAsFile();
            else
                return await JustSave(CurrentFile);
        }
        private DialogResult AskForSaving()
        {
            return MessageBox.Show("Сохранить изменения в файле?", "Сохранение изменений", MessageBoxButtons.YesNoCancel);
        }
        private async Task<bool> JustOpen(string fname)
        {
            LockMenu(true);
            try
            {
                Log("Открытие файла..");
                await Task.Run(() => { MainTree = TreeWorker.LoadTreeFromFile(fname); });
                Log("Словарь загружен из файла");
                RefreshAll();
                CurrentFile = fname;
                FileChanged = false;
                GC.Collect(2);
            }
            catch
            {
                MessageBox.Show("Не удалось открыть файл.");
                return false;
            }
            LockMenu(false);
            UpdateCaption();
            UpdateStatusStrip(0, "Открыт словарь " + Path.GetFileName(fname));
            return true;
        }
        private async Task<bool> JustSave(string fname)
        {
            LockMenu(true);
            try
            {
                Log("Сохранение в файл..");
                await Task.Run(() => { TreeWorker.SaveTreeToFile(fname, MainTree); });
                CurrentFile = fname;
                FileChanged = false;
                Log("Словарь сохранён в файл");
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить файл.");
                return false;
            }
            LockMenu(false);
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
        private void отображатьСписокСловToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxWords.Visible = отображатьСписокСловToolStripMenuItem.Checked;
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
            listBoxWords.Width = 200;
            listBoxWords.Left = splitContainer1.Panel2.Right - 200 - 3;
            if (dataGridFilesProcessing.Visible)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Height = 65;
                    textBoxLog.Width = listBoxWords.Visible ? splitContainer1.Panel2.Right - 200 - 9 : splitContainer1.Panel2.Right - 6;
                    textBoxLog.Top = splitContainer1.Panel2.Bottom - splitContainer1.Panel2.Top - 65 - 3;
                    dataGridFilesProcessing.Top = 3;
                    dataGridFilesProcessing.Height = textBoxLog.Top - dataGridFilesProcessing.Top - 6;
                    dataGridFilesProcessing.Width = listBoxWords.Visible ? splitContainer1.Panel2.Right - 200 - 9 : splitContainer1.Panel2.Right - 6;
                    
                }
                else
                {
                    dataGridFilesProcessing.Width = listBoxWords.Visible ? splitContainer1.Panel2.Right - 200 - 9 : splitContainer1.Panel2.Right - 6;
                    dataGridFilesProcessing.Top = 3;
                    dataGridFilesProcessing.Height = splitContainer1.Panel2.Height - 6;
                }
            }
            else
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Width = listBoxWords.Visible ? splitContainer1.Panel2.Right - 200 - 9 : splitContainer1.Panel2.Right - 6;
                    textBoxLog.Top = 3;
                    textBoxLog.Height = splitContainer1.Panel2.Height - 6;
                }
                else
                {
                    if (listBoxWords.Visible)
                    {
                        listBoxWords.Width = splitContainer1.Panel2.Right - 6;
                        listBoxWords.Left = 3;
                    }
                    else
                        splitContainer1.Panel2Collapsed = true;
                }
            }
        }

        private void прерватьВыполнениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbortCalculating = true;
            UpdateStatusStrip(100, "Прерывание обработки..");
            Log("Выполняется прерывание обработки");
            Downloader.AbortTasks();
        }
        private void обработатьПапкуСФайламиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            Task.Run(() => ProcessFolder(folderBrowserDialog.SelectedPath));
        }
        private void обработатьТекстовыйФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Task.Run(() => ProcessSingleFile(openFileDialog.FileName));
        }
        private void обработатьСлучайнуюСтатьюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LockMenu(true);
            ProcessRandomPage();
        }
        private void запуститьОбработкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LockMenu(true);
            ProcessRandomPages();
        }
        private void обработатьNСтатейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new FormInputPagesCount();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LockMenu(true);
                ProcessRandomPages(Convert.ToByte(dialog.numericUpDown1.Value), false);
            }
        }
        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ProcessingEntitiesInfo.Count > 0)
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
                e.Cancel = !await SaveFile();
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

            LockMenu(true);

            // Глобальная инициализация процесса обработки
            var BeforeDiffWords = MainTree.DifferentWords;
            FileChanged = true;
            UpdateCaption();
            var TotalSize = Files.Sum(f => f.Length);
            if (Files.Length > 1)
                UpdateStatusStrip(0, string.Format("Обработка {0} файлов ({1} Мб) из директории: {2}",
                    Files.Length, (TotalSize / 1048576f).ToString("F1"), Files.First().DirectoryName));
            else
                UpdateStatusStrip(0, "Обработка файла: " + Files[0].Name);

            // Локальная инициализация
            var Entities = ProcessingEntity.CreateFiles(Files).ToList();
            var Streams = new StreamReader[Files.Length];
            var LocalTrees = new PrefixTree[Files.Length];

            // Параллельная обработка всех файлов
            Log("Начало параллельной обработки файлов");
            Parallel.For(0, Files.Length, i =>
            {
                // Инициализация
                Entities[i].State = ProcessingState.Processing;
                long iteration = 0;
                long lastiterationsvalue = 0;
                long lastswvalue = 0;
                int lastwordsvalue = 0;
                var LocalTree = Files.Length == 1 ? MainTree : new SyncPrefixTree();
                LocalTrees[i] = LocalTree;
                var Word = new StringBuilder();

                using (Streams[i] = new StreamReader(Files[i].FullName))
                {
                    do
                    {
                        // Считывание символа
                        var c = Convert.ToChar(Streams[i].Read());
                        iteration++;

                        // Обновление визуализации данных
                        if (Streams[i].BaseStream.Position > Streams[i].BaseStream.Length * (Entities[i].Progress + 5) / 100)
                        {
                            // Обновление локальных данных
                            Entities[i].Progress += 5;
                            Entities[i].ReadingSpeed = (float)(iteration - lastiterationsvalue) / (Entities[i].ProcessingTime - lastswvalue);
                            Entities[i].ProcessingSpeed = (float)(Entities[i].ProcessedWords - lastwordsvalue) / (Entities[i].ProcessingTime - lastswvalue);
                            lastswvalue = Entities[i].ProcessingTime;
                            lastiterationsvalue = iteration;
                            lastwordsvalue = Entities[i].ProcessedWords;

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
                            UpdateStatusStrip(ProcessingEntitiesInfo.Progress);
                            RefreshStatistics();
                            RefreshFilesStats(Entities);
                        }

                        // Обработка считанных данных
                        if (char.IsLetter(c))
                        {
                            Word.Append(char.ToUpper(c));
                        }
                        else if (Word.Length != 0)
                        {
                            Entities[i].AddWord();
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
                }
                // Окончание обработки, объединение с основным деревом
                Entities[i].State = ProcessingState.Merging;
                if (Files.Length != 1)
                    MainTree.AppendTree(LocalTree);
                LocalTree = null;
                MainTree.FilesProcessed++;
                Entities[i].State = ProcessingState.Finished;
            });
            LTProcessedChars = 0;
            LTProcessedWords = 0;
            MainTree.ProcessingTime += ProcessingEntitiesInfo.ProcessingTime;

            // Вывод статуса внизу
            if (AbortCalculating && ProcessingEntitiesInfo.Count == 0)
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

            // Подсчёт изменений
            if (BeforeDiffWords != 0)
            {
                var AfterDiffWords = MainTree.DifferentWords;
                Log($"В результате обработки случайной статьи с википедии размер словаря увеличился на {AfterDiffWords - BeforeDiffWords} слов / на {100 * ((float)AfterDiffWords / BeforeDiffWords - 1)}%");
            }

            // Обновление интерфейса
            LockMenu(false);
            RefreshAll();

            // Чистка мусора
            Entities = null;
            Streams = null;
            GC.Collect(1);
        }

        [Obsolete]
        private void ProcessRandomPage()
        {
            Task.Run(() =>
            {
                // Инициализация процесса обработки
                Log("Начало обработки случайно статьи с Википедии");
                var BeforeDiffWords = MainTree.DifferentWords;
                FileChanged = true;
                UpdateCaption();
                UpdateStatusStrip(0, "Загрузка случайной статьи");
                var ReadingSpeed = new float[1];
                var ProcessingSpeed = new float[1];
                var States = new ProcessingState[1];
                States[0] = ProcessingState.Downloading;
                StringReader Stream;
                var Percents = new int[1];
                RefreshFilesStats(new string[] { "???" }, new int[] { 0 }, States, Percents, ReadingSpeed, ProcessingSpeed);
                Log("Скачивание..");

                // Получение текста
                var ArticleTask = WikiGetRandomText();

                // Начало обработки
                var Article = /*await*/ ArticleTask;
                if (Article == null)
                {
                    Log("Ошибка скачивания");
                    UpdateStatusStrip(0, "Ошибка скачивания статьи");
                    LockMenu(false);
                    return;
                }
                var TotalSize = Article.Item2.Length;
                States[0] = ProcessingState.Processing;
                RefreshFilesStats(new string[] { Article.Item1 }, new int[] { TotalSize }, States, Percents, ReadingSpeed, ProcessingSpeed);
                Stream = new StringReader(Article.Item2);
                UpdateStatusStrip(0, "Обработка содержимого статьи \"" + Article.Item1 + "\"");

                // Инициализация
                long iteration = 0;
                long lastiterationsvalue = 0;
                ulong lastswvalue = 0;
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
                        ReadingSpeed[0] = (float)(iteration - lastiterationsvalue) / (ProcessingEntitiesInfo.ProcessingTime - lastswvalue);
                        ProcessingSpeed[0] = (float)(WordsFound - lastwordsvalue) / (ProcessingEntitiesInfo.ProcessingTime - lastswvalue);
                        lastswvalue = ProcessingEntitiesInfo.ProcessingTime;
                        lastiterationsvalue = iteration;
                        lastwordsvalue = WordsFound;

                        // Обновление глобальных данных
                        this.LTProcessedChars = LocalTree.ProcessedChars;
                        this.LTProcessedWords = LocalTree.ProcessedWords;
                        
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
                MainTree.FilesProcessed++;
                States[0] = ProcessingState.Finished;
                LTProcessedChars = 0;
                LTProcessedWords = 0;
                MainTree.ProcessingTime += ProcessingEntitiesInfo.ProcessingTime;

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

                // Подсчёт изменений
                if (BeforeDiffWords != 0)
                {
                    var AfterDiffWords = MainTree.DifferentWords;
                    Log($"В результате обработки случайной статьи с википедии размер словаря увеличился на {AfterDiffWords - BeforeDiffWords} слов / на {100 * ((float)AfterDiffWords / BeforeDiffWords - 1)}%");
                }

                // Обновление интерфейса
                LockMenu(false);
                RefreshChart();
                RefreshStatistics();
                RefreshFilesStats(new string[] { Article.Item1 }, new int[] { TotalSize }, States, Percents, ReadingSpeed, ProcessingSpeed);

                // Чистка мусора
                Percents = null;
                ReadingSpeed = null;
                ProcessingSpeed = null;
                States = null;
                Stream = null;
                GC.Collect(1);
            });
        }
        private void ProcessRandomPages(byte Count = 20, bool Repeat = true)
        {
            Task.Run(() =>
            {
                var BeforeDiffWords = MainTree.DifferentWords;
                Log("Запуск обработки случайных статей");
                do {

                    // Инициализация процесса обработки
                    UpdateStatusStrip(0, "Загрузка списка случайных статей");
                    Log("Загрузка списка статей");
                    var ArticleHeaders = WikiGetRandomTitles(Count);
                    if (ArticleHeaders == null)
                    {
                        Log("Ошибка загрузки, проверьте подключение к интернету");
                        UpdateStatusStrip(0, "Загрузка прервана");
                        LockMenu(false);
                        return;
                    }
                    FileChanged = true;
                    UpdateCaption();

                    // Локальная инициализация
                    var Articles = ProcessingEntity.CreateArticles(ArticleHeaders).ToList();
                    RefreshFilesStats(Articles);
                    
                    Log("Список статей загружен, запуск параллельного скачивания и обработки текстов");
                    Parallel.For(0, Count, i =>
                    {
                        if (AbortCalculating)
                        {
                            Articles[i].State = ProcessingState.Aborted;
                            return;
                        }
                        var DownloadingID = WikiStartGettingTextById(ArticleHeaders[i]);
                        Articles[i].State = ProcessingState.Downloading;
                        RefreshFilesStats(Articles);
                        var Text = WikiFinishGettingTextById(DownloadingID);
                        if (Text == null)
                        {
                            Articles[i].State = ProcessingState.Error;
                            return;
                        }
                        
                        Articles[i].Length = Text.Length;
                        Articles[i].State = ProcessingState.Processing;
                        
                        int iteration = 0;
                        int lastiterationsvalue = 0;
                        long lastswvalue = 0;
                        int lastwordsvalue = 0;
                        var Word = new StringBuilder();
                        RefreshFilesStats(Articles);
                        var Stream = new StringReader(Text);

                        while (iteration < Text.Length && !AbortCalculating)
                        {
                            // Считывание символа
                            var c = Convert.ToChar(Stream.Read());
                            iteration++;

                            // Обновление визуализации данных
                            if (iteration > Text.Length * (Articles[i].Progress + 10) / 100)
                            {
                                // Обновление локальных данных
                                Articles[i].Progress += 10;
                                Articles[i].ReadingSpeed = (float)(iteration - lastiterationsvalue) / (Articles[i].ProcessingTime - lastswvalue);
                                Articles[i].ProcessingSpeed = (float)(Articles[i].ProcessedWords - lastwordsvalue) / (Articles[i].ProcessingTime - lastswvalue);
                                lastswvalue = Articles[i].ProcessingTime;
                                lastiterationsvalue = iteration;
                                lastwordsvalue = Articles[i].ProcessedWords;
                                Articles[i].ProcessedChars = iteration;

                                UpdateStatusStrip(Articles.Sum(a => a.Progress) / Articles.Count);
                                RefreshStatistics();
                                RefreshFilesStats(Articles);
                            }

                            // Обработка считанных данных
                            if (char.IsLetter(c))
                            {
                                Word.Append(char.ToUpper(c));
                            }
                            else if (Word.Length != 0)
                            {
                                Articles[i].AddWord();
                                lock (MainTree)
                                {
                                    while (true)
                                    {
                                        if (MainTree.AddWord(Word.ToString()))
                                            break;

                                        Thread.Sleep(1);
                                    }
                                }
                                Word.Clear();
                            }
                        }

                        // Окончание обработки                        
                        if (AbortCalculating)
                            Articles[i].State = ProcessingState.Aborted;
                        else
                        {
                            MainTree.FilesProcessed++;
                            Articles[i].State = ProcessingState.Finished;
                        }
                    });
                    Log("Параллельная обработка статей завершена");
                    LTProcessedChars = 0;
                    LTProcessedWords = 0;
                    MainTree.ProcessingTime += ProcessingEntitiesInfo.ProcessingTime;

                    RefreshAll();

                    // Чистка мусора
                    Articles = null;
                    GC.Collect(1);
                }
                while (!AbortCalculating && Repeat);

                // Вывод статуса внизу
                if (AbortCalculating)
                {
                    UpdateStatusStrip(100, "Обработка прервана");
                    Log("Обработка была прервана");
                    AbortCalculating = false;
                }
                else
                {
                    UpdateStatusStrip(100, "Обработка статей завершена");
                }

                // Подсчёт изменений
                if (BeforeDiffWords != 0)
                {
                    var AfterDiffWords = MainTree.DifferentWords;
                    Log($"В результате обработки случайной статьи с википедии размер словаря увеличился на {AfterDiffWords - BeforeDiffWords} слов / на {100 * ((float)AfterDiffWords / BeforeDiffWords - 1)}%");
                }

                // Обновление интерфейса после выхода из обработки
                LockMenu(false);
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
        private Guid WikiStartGettingTextById(ArticleHeadersInfo id)
        {
            return Downloader.StartDownloadString("https://ru.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&pageids=" + id.Id + "&exlimit=1&explaintext=1&exsectionformat=plain");
        }
        [Obsolete]
        private Guid WikiStartGettingTextById(IEnumerable<ArticleHeadersInfo> ids)
        {
            /*
             * How many extracts to return. (Multiple extracts can only be returned if exintro is set to true.)
No more than 20 (20 for bots) allowed. Enter max to use the maximum limit.
*/
            var idslist = String.Join("\x7C", ids.ToArray().Select(id => id.Id));
            return Downloader.StartDownloadString("https://ru.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&pageids=" + idslist + "&exlimit=5&explaintext=1&exsectionformat=plain");
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
                return null;
            }
        }
        [Obsolete]
        private string[] WikiFinishGettingTextByIds(Guid guid)
        {
            try
            {
                var d = Downloader.WaitForDownloaded(guid);
                var dd = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                var c = dd.Last.Last.Last.Last.Children();//.Last.Last.Last.Last.ToString();
                var f = c.Select(s => s.Last.Last.Last.ToString());
                return f.ToArray();
            }
            catch
            {
                return null;
            }
        }
        private ArticleHeadersInfo[] WikiGetRandomTitles(byte Count)
        {
            try
            {
                var d = Downloader.WaitForDownloaded(Downloader.StartDownloadString($"https://ru.wikipedia.org/w/api.php?action=query&format=json&list=&generator=random&grnnamespace=0&grnlimit={Count}"));
                //   action=query&format=json&prop=extracts&titles=A%7CB%7CC&utf8=1&exlimit=1&explaintext=1&exsectionformat=plain
                var ee = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(d);
                // [JSON].query.pages.625963
                var f = (JObject)ee.Last.Last.Last.Last;
                var t = f.Properties().Select(u => u.Name).ToArray();
                var t2 = f.Properties().Select(u => u.Last.Last.Last.ToString()).ToArray();

                return ArticleHeadersInfo.CreateArray(t, t2);
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
            {
                if (chart.Titles.Count == 0)
                    chart.Titles.Add("Title");
                chart.Series[0].Points[x].Color = System.Drawing.Color.Red;
                listBoxWords.SelectedIndex = x;
                var diff = Math.Abs(Math.Round(chart.Series[0].Points[x].YValues[0] - chart.Series[1].Points[x].YValues[0]));
                chart.Titles[0].Text = $"Слово: {Words[x]}\n"
                    + $"Количество: {chart.Series[0].Points[x].YValues[0]}\n"
                    + $"Абсолютная отклонение от закона Ципфа: {diff} слов\n"
                    + $"Относительное отклонение {Math.Round(100 * diff / Math.Max(chart.Series[1].Points[x].YValues[0], chart.Series[0].Points[x].YValues[0]), 1)}%";
            }
            else
            {
                chart.Titles.Clear();
                listBoxWords.SelectedIndex = -1;
            }
        }
        private void chart_MouseLeave(object sender, EventArgs e)
        {
            chart_MouseMove(sender, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
        }
    }
}
