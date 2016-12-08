﻿namespace GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.dataGridStatistics = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.словарьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.закрытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.новыйToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьКакToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.добавитьКТекущемуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.данныеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.обработатьТекстовыйФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.обработатьПапкуСФайламиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wikipediaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.обработатьСлучайнуюСтатьюToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.запуститьОбработкуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.прерватьВыполнениеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.видToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отображатьГрафикToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отображатьПрогрессToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отображатьСтатистикуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отображатьЛогToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDictDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveDictDialog = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridStatistics)).BeginInit();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart
            // 
            this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chart.BackColor = System.Drawing.Color.Gainsboro;
            this.chart.BackHatchStyle = System.Windows.Forms.DataVisualization.Charting.ChartHatchStyle.LightUpwardDiagonal;
            this.chart.BorderlineColor = System.Drawing.Color.Black;
            this.chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Area3DStyle.Inclination = 10;
            chartArea1.Area3DStyle.IsRightAngleAxes = false;
            chartArea1.Area3DStyle.LightStyle = System.Windows.Forms.DataVisualization.Charting.LightStyle.Realistic;
            chartArea1.Area3DStyle.Perspective = 1;
            chartArea1.Area3DStyle.Rotation = 8;
            chartArea1.Area3DStyle.WallWidth = 3;
            chartArea1.Name = "ChartArea1";
            this.chart.ChartAreas.Add(chartArea1);
            this.chart.Location = new System.Drawing.Point(293, 27);
            this.chart.Name = "chart";
            this.chart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chart.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))))};
            series1.BorderColor = System.Drawing.Color.Black;
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineArea;
            series1.Legend = "Legend1";
            series1.Name = "SeriesValues";
            series1.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(98)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            series1.ShadowOffset = 4;
            series2.BorderColor = System.Drawing.Color.Lime;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineArea;
            series2.Name = "SeriesIdeal";
            series2.ShadowColor = System.Drawing.Color.Gainsboro;
            series2.ShadowOffset = 4;
            series2.YValuesPerPoint = 4;
            this.chart.Series.Add(series1);
            this.chart.Series.Add(series2);
            this.chart.Size = new System.Drawing.Size(364, 254);
            this.chart.TabIndex = 0;
            this.chart.Text = "chart1";
            this.chart.Visible = false;
            // 
            // dataGridStatistics
            // 
            this.dataGridStatistics.AllowUserToAddRows = false;
            this.dataGridStatistics.AllowUserToDeleteRows = false;
            this.dataGridStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dataGridStatistics.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridStatistics.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            this.dataGridStatistics.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridStatistics.Location = new System.Drawing.Point(12, 27);
            this.dataGridStatistics.MultiSelect = false;
            this.dataGridStatistics.Name = "dataGridStatistics";
            this.dataGridStatistics.ReadOnly = true;
            this.dataGridStatistics.RowHeadersVisible = false;
            this.dataGridStatistics.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridStatistics.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridStatistics.Size = new System.Drawing.Size(275, 254);
            this.dataGridStatistics.TabIndex = 2;
            // 
            // Column1
            // 
            this.Column1.Frozen = true;
            this.Column1.HeaderText = "Параметр";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 120;
            // 
            // Column2
            // 
            this.Column2.Frozen = true;
            this.Column2.HeaderText = "Значение";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 150;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.словарьToolStripMenuItem,
            this.данныеToolStripMenuItem,
            this.видToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(669, 24);
            this.menuStrip.TabIndex = 6;
            this.menuStrip.Text = "menuStrip";
            // 
            // словарьToolStripMenuItem
            // 
            this.словарьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.закрытьToolStripMenuItem,
            this.новыйToolStripMenuItem,
            this.сохранитьToolStripMenuItem,
            this.сохранитьКакToolStripMenuItem,
            this.добавитьКТекущемуToolStripMenuItem});
            this.словарьToolStripMenuItem.Name = "словарьToolStripMenuItem";
            this.словарьToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.словарьToolStripMenuItem.Text = "Словарь";
            // 
            // закрытьToolStripMenuItem
            // 
            this.закрытьToolStripMenuItem.Name = "закрытьToolStripMenuItem";
            this.закрытьToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.закрытьToolStripMenuItem.Text = "Новый";
            this.закрытьToolStripMenuItem.Click += new System.EventHandler(this.закрытьToolStripMenuItem_Click);
            // 
            // новыйToolStripMenuItem
            // 
            this.новыйToolStripMenuItem.Name = "новыйToolStripMenuItem";
            this.новыйToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.новыйToolStripMenuItem.Text = "Открыть";
            this.новыйToolStripMenuItem.Click += new System.EventHandler(this.OpenFile_Click);
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.сохранитьToolStripMenuItem.Text = "Сохранить";
            this.сохранитьToolStripMenuItem.Click += new System.EventHandler(this.сохранитьToolStripMenuItem_Click);
            // 
            // сохранитьКакToolStripMenuItem
            // 
            this.сохранитьКакToolStripMenuItem.Name = "сохранитьКакToolStripMenuItem";
            this.сохранитьКакToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.сохранитьКакToolStripMenuItem.Text = "Сохранить как";
            this.сохранитьКакToolStripMenuItem.Click += new System.EventHandler(this.сохранитьКакToolStripMenuItem_Click);
            // 
            // добавитьКТекущемуToolStripMenuItem
            // 
            this.добавитьКТекущемуToolStripMenuItem.Name = "добавитьКТекущемуToolStripMenuItem";
            this.добавитьКТекущемуToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.добавитьКТекущемуToolStripMenuItem.Text = "Добавить к текущему";
            // 
            // данныеToolStripMenuItem
            // 
            this.данныеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.wikipediaToolStripMenuItem,
            this.прерватьВыполнениеToolStripMenuItem});
            this.данныеToolStripMenuItem.Name = "данныеToolStripMenuItem";
            this.данныеToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.данныеToolStripMenuItem.Text = "Данные";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.обработатьТекстовыйФайлToolStripMenuItem,
            this.обработатьПапкуСФайламиToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // обработатьТекстовыйФайлToolStripMenuItem
            // 
            this.обработатьТекстовыйФайлToolStripMenuItem.Name = "обработатьТекстовыйФайлToolStripMenuItem";
            this.обработатьТекстовыйФайлToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.обработатьТекстовыйФайлToolStripMenuItem.Text = "Обработать текстовый файл";
            this.обработатьТекстовыйФайлToolStripMenuItem.Click += new System.EventHandler(this.обработатьТекстовыйФайлToolStripMenuItem_Click);
            // 
            // обработатьПапкуСФайламиToolStripMenuItem
            // 
            this.обработатьПапкуСФайламиToolStripMenuItem.Name = "обработатьПапкуСФайламиToolStripMenuItem";
            this.обработатьПапкуСФайламиToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.обработатьПапкуСФайламиToolStripMenuItem.Text = "Обработать папку с файлами";
            // 
            // wikipediaToolStripMenuItem
            // 
            this.wikipediaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.обработатьСлучайнуюСтатьюToolStripMenuItem,
            this.запуститьОбработкуToolStripMenuItem});
            this.wikipediaToolStripMenuItem.Name = "wikipediaToolStripMenuItem";
            this.wikipediaToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.wikipediaToolStripMenuItem.Text = "Wikipedia";
            // 
            // обработатьСлучайнуюСтатьюToolStripMenuItem
            // 
            this.обработатьСлучайнуюСтатьюToolStripMenuItem.Name = "обработатьСлучайнуюСтатьюToolStripMenuItem";
            this.обработатьСлучайнуюСтатьюToolStripMenuItem.Size = new System.Drawing.Size(358, 22);
            this.обработатьСлучайнуюСтатьюToolStripMenuItem.Text = "Обработать случайную статью";
            // 
            // запуститьОбработкуToolStripMenuItem
            // 
            this.запуститьОбработкуToolStripMenuItem.Name = "запуститьОбработкуToolStripMenuItem";
            this.запуститьОбработкуToolStripMenuItem.Size = new System.Drawing.Size(358, 22);
            this.запуститьОбработкуToolStripMenuItem.Text = "Запустить/остановить обработку случайных статей";
            // 
            // прерватьВыполнениеToolStripMenuItem
            // 
            this.прерватьВыполнениеToolStripMenuItem.Enabled = false;
            this.прерватьВыполнениеToolStripMenuItem.Name = "прерватьВыполнениеToolStripMenuItem";
            this.прерватьВыполнениеToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.прерватьВыполнениеToolStripMenuItem.Text = "Прервать выполнение";
            this.прерватьВыполнениеToolStripMenuItem.Click += new System.EventHandler(this.прерватьВыполнениеToolStripMenuItem_Click);
            // 
            // видToolStripMenuItem
            // 
            this.видToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.отображатьГрафикToolStripMenuItem,
            this.отображатьПрогрессToolStripMenuItem,
            this.отображатьСтатистикуToolStripMenuItem,
            this.отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem,
            this.отображатьЛогToolStripMenuItem});
            this.видToolStripMenuItem.Name = "видToolStripMenuItem";
            this.видToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.видToolStripMenuItem.Text = "Вид";
            // 
            // отображатьГрафикToolStripMenuItem
            // 
            this.отображатьГрафикToolStripMenuItem.CheckOnClick = true;
            this.отображатьГрафикToolStripMenuItem.Name = "отображатьГрафикToolStripMenuItem";
            this.отображатьГрафикToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.отображатьГрафикToolStripMenuItem.Text = "Отображать график";
            this.отображатьГрафикToolStripMenuItem.Click += new System.EventHandler(this.отображатьГрафикToolStripMenuItem_Click);
            // 
            // отображатьПрогрессToolStripMenuItem
            // 
            this.отображатьПрогрессToolStripMenuItem.Checked = true;
            this.отображатьПрогрессToolStripMenuItem.CheckOnClick = true;
            this.отображатьПрогрессToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.отображатьПрогрессToolStripMenuItem.Name = "отображатьПрогрессToolStripMenuItem";
            this.отображатьПрогрессToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.отображатьПрогрессToolStripMenuItem.Text = "Отображать прогресс";
            // 
            // отображатьСтатистикуToolStripMenuItem
            // 
            this.отображатьСтатистикуToolStripMenuItem.Checked = true;
            this.отображатьСтатистикуToolStripMenuItem.CheckOnClick = true;
            this.отображатьСтатистикуToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.отображатьСтатистикуToolStripMenuItem.Name = "отображатьСтатистикуToolStripMenuItem";
            this.отображатьСтатистикуToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.отображатьСтатистикуToolStripMenuItem.Text = "Отображать статистику";
            this.отображатьСтатистикуToolStripMenuItem.Click += new System.EventHandler(this.отображатьСтатистикуToolStripMenuItem_Click);
            // 
            // отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem
            // 
            this.отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem.Name = "отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem";
            this.отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem.Text = "Отображать дополнительную информацию о потоках";
            // 
            // отображатьЛогToolStripMenuItem
            // 
            this.отображатьЛогToolStripMenuItem.Name = "отображатьЛогToolStripMenuItem";
            this.отображатьЛогToolStripMenuItem.Size = new System.Drawing.Size(374, 22);
            this.отображатьЛогToolStripMenuItem.Text = "Отображать лог";
            // 
            // openDictDialog
            // 
            this.openDictDialog.Filter = "CharTree Content Files (.ctcf)|*.ctcf|Text Files|*.txt|All Files|*.*";
            // 
            // saveDictDialog
            // 
            this.saveDictDialog.Filter = "CharTree Content Files (.ctcf)|*.ctcf|Text Files|*.txt|All Files|*.*";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 284);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(669, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(196, 16);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(16, 17);
            this.toolStripStatusLabel1.Text = "...";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Текстовый файл|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 306);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.dataGridStatistics);
            this.Controls.Add(this.chart);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Курсач";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridStatistics)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private System.Windows.Forms.DataGridView dataGridStatistics;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem словарьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem новыйToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem добавитьКТекущемуToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem закрытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem данныеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem обработатьТекстовыйФайлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem обработатьПапкуСФайламиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wikipediaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem обработатьСлучайнуюСтатьюToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem запуститьОбработкуToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem видToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отображатьГрафикToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отображатьПрогрессToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отображатьСтатистикуToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.ToolStripMenuItem сохранитьКакToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openDictDialog;
        private System.Windows.Forms.SaveFileDialog saveDictDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripMenuItem прерватьВыполнениеToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripMenuItem отображатьДополнительнуюИнформациюОПотокахToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отображатьЛогToolStripMenuItem;
    }
}
