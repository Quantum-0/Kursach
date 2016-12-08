using CharTrees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
         * Tree.Merge/Append
         * Log
         * Dir processing
         */

        public CharTree MainTree;
        public string CurrentFile;
        public bool FileChanged;
        private const string Caption = "Курсач";
        public float ReadingSpeed;
        //public float MidReadingSpeed;
        public float ProcessingSpeed;
        //public float MidProcessingSpeed;
        //public int ProcessedFiles;
        public long ProcessingTime;
        public byte ProcessingFiles;
        public bool AbortCalculating = false;

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
            var MidReadingSpeed = (MainTree.ProcessedChars) / (1 + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
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
            var MidProcessingSpeed = (MainTree.ProcessedWords) / (1 + MainTree.ProcessingTime + Convert.ToUInt64(ProcessingTime));
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
            var chars = MainTree.ProcessedChars;
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
            var kwords = MainTree.ProcessedWords / 1000f;
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
            //colorslist.Add(Color.FromArgb(192, Color.LightSkyBlue));
            //colorslist.Add(Color.FromArgb(64, 255, 128, 0));
            colorslist.Add(Color.FromArgb(192, 192, 192, 192));
            colorslist.Add(Color.FromArgb(64, Color.GreenYellow));
            chart.PaletteCustomColors = colorslist.ToArray();
        }

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

        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }
        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewFile();
            RefreshStatistics();
        }

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

        private void обработатьТекстовыйФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;            

            файлToolStripMenuItem.Enabled = false;
            wikipediaToolStripMenuItem.Enabled = false;
            прерватьВыполнениеToolStripMenuItem.Enabled = true;

            Task.Run(() => ProcessSingleFile(openFileDialog.FileName));
        }

        private void UpdateStatusStrip(int Progress, string Text = null)
        {
            this.Invoke((Action) delegate { 
                toolStripProgressBar1.Value = Progress;
                if (Text != null)
                    toolStripStatusLabel1.Text = Text;
            });
        }

        private void ProcessSingleFile(string fname)
        {
            FileChanged = true;
            var FI = new FileInfo(fname);
            var sw = new Stopwatch();
            var Word = new StringBuilder();
            var WordsFound = 0;
            var Percents = 0;
            var shortfname = Path.GetFileName(fname);
            UpdateStatusStrip(0, "Обработка файла: " + shortfname);
            long lastswvalue = 0;
            long lastiterationsvalue = 0;
            long lastwordsvalue = 0;
            long iteration = 0;
            using (StreamReader sr = new StreamReader(fname))
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
                        this.Invoke((Action)(() => RefreshStatistics() ));
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
            FileChanged = true;
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
            GC.Collect(0);
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
                    var maxX = Convert.ToInt32(Math.Floor(Math.Pow(list.Count, 1d/2)));
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

        private void прерватьВыполнениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbortCalculating = true;
            UpdateStatusStrip(100, "Прерывание обработки..");
        }
    }
}
