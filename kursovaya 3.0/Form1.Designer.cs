namespace Kursovaya30
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.date = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnDeleteEpoch = new System.Windows.Forms.Button();
            this.btnAddEpoch = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBoxEpsilon = new System.Windows.Forms.TextBox();
            this.textBoxAlpha = new System.Windows.Forms.TextBox();
            this.btnOpenSQLite = new System.Windows.Forms.Button();
            this.btnOpenDatabase = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.first_level_decomposition = new System.Windows.Forms.TabPage();
            this.chartLevel1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnCalculateLevel1 = new System.Windows.Forms.Button();
            this.dataGridViewLevel1 = new System.Windows.Forms.DataGridView();
            this.second_level_decomposition = new System.Windows.Forms.TabPage();
            this.third_level_decomposition = new System.Windows.Forms.TabPage();
            this.fourth_level_decomposition = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.date.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.first_level_decomposition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartLevel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLevel1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.date);
            this.tabControl1.Controls.Add(this.first_level_decomposition);
            this.tabControl1.Controls.Add(this.second_level_decomposition);
            this.tabControl1.Controls.Add(this.third_level_decomposition);
            this.tabControl1.Controls.Add(this.fourth_level_decomposition);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1900, 971);
            this.tabControl1.TabIndex = 0;
            // 
            // date
            // 
            this.date.Controls.Add(this.pictureBox1);
            this.date.Controls.Add(this.btnDeleteEpoch);
            this.date.Controls.Add(this.btnAddEpoch);
            this.date.Controls.Add(this.dataGridView1);
            this.date.Controls.Add(this.comboBox1);
            this.date.Controls.Add(this.textBoxEpsilon);
            this.date.Controls.Add(this.textBoxAlpha);
            this.date.Controls.Add(this.btnOpenSQLite);
            this.date.Controls.Add(this.btnOpenDatabase);
            this.date.Controls.Add(this.label4);
            this.date.Controls.Add(this.label3);
            this.date.Controls.Add(this.label2);
            this.date.Controls.Add(this.label1);
            this.date.Controls.Add(this.splitter1);
            this.date.Location = new System.Drawing.Point(4, 25);
            this.date.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.date.Name = "date";
            this.date.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.date.Size = new System.Drawing.Size(804, 538);
            this.date.TabIndex = 0;
            this.date.Text = "Данные";
            this.date.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pictureBox1.Location = new System.Drawing.Point(446, 189);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(355, 347);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // btnDeleteEpoch
            // 
            this.btnDeleteEpoch.Location = new System.Drawing.Point(247, 188);
            this.btnDeleteEpoch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDeleteEpoch.Name = "btnDeleteEpoch";
            this.btnDeleteEpoch.Size = new System.Drawing.Size(77, 44);
            this.btnDeleteEpoch.TabIndex = 13;
            this.btnDeleteEpoch.Text = "Удалить строку";
            this.btnDeleteEpoch.UseVisualStyleBackColor = true;
            this.btnDeleteEpoch.Click += new System.EventHandler(this.btnDeleteEpoch_Click);
            // 
            // btnAddEpoch
            // 
            this.btnAddEpoch.Location = new System.Drawing.Point(99, 188);
            this.btnAddEpoch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnAddEpoch.Name = "btnAddEpoch";
            this.btnAddEpoch.Size = new System.Drawing.Size(143, 44);
            this.btnAddEpoch.TabIndex = 12;
            this.btnAddEpoch.Text = "Добавить строку";
            this.btnAddEpoch.UseVisualStyleBackColor = true;
            this.btnAddEpoch.Click += new System.EventHandler(this.btnAddEpoch_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(446, 2);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(355, 534);
            this.dataGridView1.TabIndex = 10;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(99, 148);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(225, 24);
            this.comboBox1.TabIndex = 9;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // textBoxEpsilon
            // 
            this.textBoxEpsilon.Location = new System.Drawing.Point(99, 377);
            this.textBoxEpsilon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxEpsilon.Name = "textBoxEpsilon";
            this.textBoxEpsilon.Size = new System.Drawing.Size(225, 22);
            this.textBoxEpsilon.TabIndex = 8;
            // 
            // textBoxAlpha
            // 
            this.textBoxAlpha.Location = new System.Drawing.Point(99, 311);
            this.textBoxAlpha.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxAlpha.Name = "textBoxAlpha";
            this.textBoxAlpha.Size = new System.Drawing.Size(225, 22);
            this.textBoxAlpha.TabIndex = 7;
            // 
            // btnOpenSQLite
            // 
            this.btnOpenSQLite.Location = new System.Drawing.Point(79, 69);
            this.btnOpenSQLite.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOpenSQLite.Name = "btnOpenSQLite";
            this.btnOpenSQLite.Size = new System.Drawing.Size(275, 46);
            this.btnOpenSQLite.TabIndex = 5;
            this.btnOpenSQLite.Text = "Загрузить базу данных";
            this.btnOpenSQLite.UseVisualStyleBackColor = true;
            this.btnOpenSQLite.Click += new System.EventHandler(this.btnOpenDatabase_Click);
            // 
            // btnOpenDatabase
            // 
            this.btnOpenDatabase.Location = new System.Drawing.Point(79, 418);
            this.btnOpenDatabase.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOpenDatabase.Name = "btnOpenDatabase";
            this.btnOpenDatabase.Size = new System.Drawing.Size(275, 73);
            this.btnOpenDatabase.TabIndex = 4;
            this.btnOpenDatabase.Text = "Применить";
            this.btnOpenDatabase.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(95, 357);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Точность";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 292);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(322, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Коэффициент экспоненциального сглаживания";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(95, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Таблица";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Курсовая работа";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(3, 2);
            this.splitter1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(443, 534);
            this.splitter1.TabIndex = 11;
            this.splitter1.TabStop = false;
            // 
            // first_level_decomposition
            // 
            this.first_level_decomposition.Controls.Add(this.chartLevel1);
            this.first_level_decomposition.Controls.Add(this.btnCalculateLevel1);
            this.first_level_decomposition.Controls.Add(this.dataGridViewLevel1);
            this.first_level_decomposition.Location = new System.Drawing.Point(4, 25);
            this.first_level_decomposition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.first_level_decomposition.Name = "first_level_decomposition";
            this.first_level_decomposition.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.first_level_decomposition.Size = new System.Drawing.Size(1892, 942);
            this.first_level_decomposition.TabIndex = 1;
            this.first_level_decomposition.Text = "I уровень";
            this.first_level_decomposition.UseVisualStyleBackColor = true;
            // 
            // chartLevel1
            // 
            chartArea1.Name = "ChartArea1";
            this.chartLevel1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartLevel1.Legends.Add(legend1);
            this.chartLevel1.Location = new System.Drawing.Point(1146, 63);
            this.chartLevel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chartLevel1.Name = "chartLevel1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.Blue;
            series1.Legend = "Legend1";
            series1.Name = "SeriesM";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            series2.Legend = "Legend1";
            series2.Name = "SeriesMPlus";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Color = System.Drawing.Color.Lime;
            series3.Legend = "Legend1";
            series3.Name = "SeriesMMinus";
            this.chartLevel1.Series.Add(series1);
            this.chartLevel1.Series.Add(series2);
            this.chartLevel1.Series.Add(series3);
            this.chartLevel1.Size = new System.Drawing.Size(693, 521);
            this.chartLevel1.TabIndex = 2;
            this.chartLevel1.Text = "chart1";
            // 
            // btnCalculateLevel1
            // 
            this.btnCalculateLevel1.Location = new System.Drawing.Point(9, 25);
            this.btnCalculateLevel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCalculateLevel1.Name = "btnCalculateLevel1";
            this.btnCalculateLevel1.Size = new System.Drawing.Size(215, 34);
            this.btnCalculateLevel1.TabIndex = 1;
            this.btnCalculateLevel1.Text = "Рассчитать I уровень";
            this.btnCalculateLevel1.UseVisualStyleBackColor = true;
            this.btnCalculateLevel1.Click += new System.EventHandler(this.btnCalculateLevel1_Click);
            // 
            // dataGridViewLevel1
            // 
            this.dataGridViewLevel1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLevel1.Location = new System.Drawing.Point(9, 63);
            this.dataGridViewLevel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridViewLevel1.Name = "dataGridViewLevel1";
            this.dataGridViewLevel1.RowHeadersWidth = 51;
            this.dataGridViewLevel1.RowTemplate.Height = 24;
            this.dataGridViewLevel1.Size = new System.Drawing.Size(1131, 521);
            this.dataGridViewLevel1.TabIndex = 0;
            // 
            // second_level_decomposition
            // 
            this.second_level_decomposition.Location = new System.Drawing.Point(4, 25);
            this.second_level_decomposition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.second_level_decomposition.Name = "second_level_decomposition";
            this.second_level_decomposition.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.second_level_decomposition.Size = new System.Drawing.Size(804, 538);
            this.second_level_decomposition.TabIndex = 2;
            this.second_level_decomposition.Text = "II уровень";
            this.second_level_decomposition.UseVisualStyleBackColor = true;
            // 
            // third_level_decomposition
            // 
            this.third_level_decomposition.Location = new System.Drawing.Point(4, 25);
            this.third_level_decomposition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.third_level_decomposition.Name = "third_level_decomposition";
            this.third_level_decomposition.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.third_level_decomposition.Size = new System.Drawing.Size(804, 538);
            this.third_level_decomposition.TabIndex = 3;
            this.third_level_decomposition.Text = "III уровень";
            this.third_level_decomposition.UseVisualStyleBackColor = true;
            // 
            // fourth_level_decomposition
            // 
            this.fourth_level_decomposition.Location = new System.Drawing.Point(4, 25);
            this.fourth_level_decomposition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fourth_level_decomposition.Name = "fourth_level_decomposition";
            this.fourth_level_decomposition.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fourth_level_decomposition.Size = new System.Drawing.Size(804, 538);
            this.fourth_level_decomposition.TabIndex = 4;
            this.fourth_level_decomposition.Text = "IV уровень";
            this.fourth_level_decomposition.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1900, 971);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(1918, 1018);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.date.ResumeLayout(false);
            this.date.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.first_level_decomposition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartLevel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLevel1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage date;
        private System.Windows.Forms.TabPage first_level_decomposition;
        private System.Windows.Forms.TabPage second_level_decomposition;
        private System.Windows.Forms.TabPage third_level_decomposition;
        private System.Windows.Forms.Button btnOpenDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage fourth_level_decomposition;
        private System.Windows.Forms.Button btnOpenSQLite;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBoxEpsilon;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Button btnAddEpoch;
        private System.Windows.Forms.Button btnDeleteEpoch;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridViewLevel1;
        private System.Windows.Forms.TextBox textBoxAlpha;
        private System.Windows.Forms.Button btnCalculateLevel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartLevel1;
    }
}

