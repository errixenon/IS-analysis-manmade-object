using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Kursovaya30
{
    public partial class Form1 : Form
    {
        #region 1. Поля формы и состояние программы

        private readonly Random random = new Random();
        private SQLiteConnection SQLiteConn;
        private DataTable dTable;
        private DataTable savedTable;
        private string currentTableName = string.Empty;

        private Dictionary<string, List<string>> blockMap = new Dictionary<string, List<string>>();
        private bool blocksConfirmed;

        private PictureBox pictureBoxLevel2Scheme;
        private NumericUpDown numericBlocks;
        private ComboBox comboBlocks;
        private ListBox listFreePoints;
        private ListBox listBlockPoints;
        private Label labelBlockInfo;
        private Button btnCreateBlocks;
        private Button btnAutoBlocks;
        private Button btnConfirmBlocks;
        private Button btnMovePoint;
        private Button btnReturnPoint;
        private ComboBox comboCalcBlock;
        private Button btnCalcLevel2;
        private DataGridView gridLevel2;
        private Chart chartLevel2;

        private ComboBox comboLevel3Block;
        private NumericUpDown numericRigidTolerance;
        private Button btnCalcLevel3;
        private Button btnShowLevel3Links;
        private DataGridView gridLevel3Links;
        private DataGridView gridLevel3Clusters;
        private Chart chartLevel3Links;
        private TabControl tabControlLevel3;
        private TabPage tabPageLevel3Distribution;
        private TabPage tabPageLevel3Calculation;
        private ListBox listLevel3AvailableBlockPoints;
        private ListBox listLevel3BlockPoints;
        private Button btnLevel3AddPointToBlock;
        private Button btnLevel3RemovePointFromBlock;
        private NumericUpDown numericLevel3Subblocks;
        private NumericUpDown numericLevel3PointsPerSubblock;
        private Button btnCreateLevel3Subblocks;
        private ComboBox comboLevel3Subblock;
        private ListBox listLevel3FreeSubblockPoints;
        private ListBox listLevel3SelectedSubblockPoints;
        private Button btnMoveLevel3Point;
        private Button btnReturnLevel3Point;
        private Button btnAutoLevel3Subblocks;
        private Button btnConfirmLevel3Subblocks;
        private Button btnCalcLevel3Response;
        private Label labelLevel3SubblockInfo;
        private Label labelLevel3Status;
        private DataGridView gridLevel3Differences;
        private Dictionary<string, List<string>> level3SubblockMap = new Dictionary<string, List<string>>();
        private List<string> level3PreparedBlockPoints = new List<string>();
        private string level3PreparedBlockName = string.Empty;
        private bool level3BlockPrepared;
        private bool level3SubblocksConfirmed;

        private ComboBox comboLevel4Point;
        private CheckedListBox checkedLevel4Points;
        private RadioButton radioLevel4AllPoints;
        private RadioButton radioLevel4OutsideClusters;
        private Button btnCalcLevel4;
        private Button btnSelectAllLevel4;
        private Button btnResetLevel4Selection;
        private DataGridView gridLevel4;
        private Chart chartLevel4;
        private Label labelLevel4Status;
        private ComboBox comboLevel1ChartMode;
        private ComboBox comboLevel2ChartMode;
        private ComboBox comboLevel3ChartMode;
        private ComboBox comboLevel3Cluster;
        private ComboBox comboLevel4ChartMode;
        private Button btnHelp;

        private List<PhaseAnalysis> lastLevel1Report = new List<PhaseAnalysis>();
        private List<PhaseAnalysis> lastLevel2Report = new List<PhaseAnalysis>();
        private List<PhaseAnalysis> lastLevel3ResponseReport = new List<PhaseAnalysis>();
        private List<LinkInfo> lastLevel3Links = new List<LinkInfo>();
        private List<ClusterInfo> lastLevel3Clusters = new List<ClusterInfo>();
        private List<PointStateRow> lastLevel4Rows = new List<PointStateRow>();
        private string lastLevel2Title = string.Empty;
        private string lastLevel3Title = string.Empty;
        private string lastLevel4Point = string.Empty;
        private List<string> lastLevel4SelectedPoints = new List<string>();
        private Dictionary<Chart, ChartOptionControls> chartOptions = new Dictionary<Chart, ChartOptionControls>();

        private Button btnSaveTable;
        private Button btnReplaceScheme;
        private Label labelCurrentDb;
        private byte[] pendingSchemeBytes;
        private bool hasPendingSchemeChange;
        private Panel panelDataSchemeScroll;
        private Label labelDataSchemeZoom;
        private float dataSchemeZoom = 1.0F;

        private sealed class ChartOptionControls
        {
            public CheckBox Main { get; set; }
            public CheckBox Plus { get; set; }
            public CheckBox Minus { get; set; }
            public CheckBox Forecast { get; set; }
            public bool ShowMain => Main == null || Main.Checked;
            public bool ShowPlus => Plus != null && Plus.Checked;
            public bool ShowMinus => Minus != null && Minus.Checked;
            public bool ShowForecast => Forecast != null && Forecast.Checked;
        }

        #endregion

        #region 2. Инициализация интерфейса

        public Form1()
        {
            InitializeComponent();

            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            PrepareForm();
            RebuildDataTab();
            RebuildLevel1Tab();
            BuildLevel2Tab();
            BuildLevel3Tab();
            BuildLevel4Tab();
        }

        private void PrepareForm()
        {
            Text = "Информационная система анализа техногенного объекта";
            MinimumSize = new Size(1280, 780);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 247, 251);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            tabControl1.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            textBoxAlpha.Text = string.IsNullOrWhiteSpace(textBoxAlpha.Text) ? "0,9" : textBoxAlpha.Text;
            textBoxEpsilon.Text = string.IsNullOrWhiteSpace(textBoxEpsilon.Text) ? "0,0021" : textBoxEpsilon.Text;
        }

        private void RebuildDataTab()
        {
            date.Controls.Clear();
            date.BackColor = Color.FromArgb(244, 247, 251);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var panelTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 0, 0, 8)
            };

            btnOpenSQLite.Text = "Открыть SQLite";
            btnOpenSQLite.AutoSize = true;
            btnOpenSQLite.Click -= btnOpenDatabase_Click;
            btnOpenSQLite.Click += btnOpenDatabase_Click;

            comboBox1.Width = 220;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            btnSaveTable = new Button { Text = "Сохранить таблицу", AutoSize = true, Enabled = false };
            btnSaveTable.Click += BtnSaveTable_Click;

            btnReplaceScheme = new Button { Text = "Заменить схему", AutoSize = true, Enabled = false };
            btnReplaceScheme.Click += BtnReplaceScheme_Click;

            btnAddEpoch.Text = "Добавить эпоху";
            btnAddEpoch.AutoSize = true;
            btnAddEpoch.Click -= btnAddEpoch_Click;
            btnAddEpoch.Click += btnAddEpoch_Click;
            btnDeleteEpoch.Text = "Удалить эпоху";
            btnDeleteEpoch.AutoSize = true;
            btnDeleteEpoch.Click -= btnDeleteEpoch_Click;
            btnDeleteEpoch.Click += btnDeleteEpoch_Click;

            btnOpenDatabase.Text = "Проверить параметры";
            btnOpenDatabase.AutoSize = true;
            btnOpenDatabase.Click += delegate
            {
                try
                {
                    ReadA();
                    ReadEpsilon();
                    MessageBox.Show("Параметры корректны. Для записи в БД нажмите «Сохранить таблицу».", "Параметры", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            };

            btnHelp = CreateHelpButton(0);

            labelCurrentDb = new Label
            {
                Text = "База не выбрана",
                AutoSize = true,
                Margin = new Padding(12, 8, 8, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            panelTop.Controls.Add(btnOpenSQLite);
            panelTop.Controls.Add(new Label { Text = "Таблица:", AutoSize = true, Margin = new Padding(12, 8, 4, 0) });
            panelTop.Controls.Add(comboBox1);
            panelTop.Controls.Add(btnAddEpoch);
            panelTop.Controls.Add(btnDeleteEpoch);
            panelTop.Controls.Add(btnSaveTable);
            panelTop.Controls.Add(btnReplaceScheme);
            panelTop.Controls.Add(new Label { Text = "A:", AutoSize = true, Margin = new Padding(12, 8, 4, 0) });
            panelTop.Controls.Add(textBoxAlpha);
            textBoxAlpha.Width = 100;
            panelTop.Controls.Add(new Label { Text = "E:", AutoSize = true, Margin = new Padding(12, 8, 4, 0) });
            panelTop.Controls.Add(textBoxEpsilon);
            textBoxEpsilon.Width = 100;
            panelTop.Controls.Add(btnOpenDatabase);
            panelTop.Controls.Add(btnHelp);
            panelTop.Controls.Add(labelCurrentDb);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 7,
                Panel1MinSize = 80,
                Panel2MinSize = 80
            };
            SetInitialSplitterDistance(split, 0.68);

            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.CellValueChanged += delegate { if (btnSaveTable != null) btnSaveTable.Enabled = true; };
            StyleGrid(dataGridView1);

            pictureBox1.Dock = DockStyle.None;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.BackColor = Color.White;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;

            var schemePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(6)
            };
            schemePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            schemePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var zoomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 4)
            };
            Button btnZoomOut = new Button { Text = "−", Width = 40, Height = 30 };
            Button btnZoomIn = new Button { Text = "+", Width = 40, Height = 30 };
            Button btnZoomReset = new Button { Text = "100%", Width = 70, Height = 30 };
            labelDataSchemeZoom = new Label { Text = "Масштаб: 100%", AutoSize = true, Margin = new Padding(12, 7, 4, 0) };
            btnZoomOut.Click += delegate { ChangeDataSchemeZoom(0.85F); };
            btnZoomIn.Click += delegate { ChangeDataSchemeZoom(1.15F); };
            btnZoomReset.Click += delegate { ResetDataSchemeZoom(); };
            zoomPanel.Controls.Add(new Label { Text = "Схема:", AutoSize = true, Margin = new Padding(0, 7, 8, 0), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
            zoomPanel.Controls.Add(btnZoomOut);
            zoomPanel.Controls.Add(btnZoomIn);
            zoomPanel.Controls.Add(btnZoomReset);
            zoomPanel.Controls.Add(labelDataSchemeZoom);

            panelDataSchemeScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            panelDataSchemeScroll.Controls.Add(pictureBox1);
            panelDataSchemeScroll.Resize += delegate { ResizeDataSchemePicture(); };

            schemePanel.Controls.Add(zoomPanel, 0, 0);
            schemePanel.Controls.Add(panelDataSchemeScroll, 0, 1);

            split.Panel1.Controls.Add(dataGridView1);
            split.Panel2.Controls.Add(schemePanel);

            root.Controls.Add(panelTop, 0, 0);
            root.Controls.Add(split, 0, 1);
            StyleAllButtons(root);
            date.Controls.Add(root);
        }

        private void RebuildLevel1Tab()
        {
            first_level_decomposition.Controls.Clear();
            first_level_decomposition.BackColor = Color.FromArgb(244, 247, 251);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnCalculateLevel1.Text = "Выполнить анализ объекта";
            btnCalculateLevel1.AutoSize = true;

            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            comboLevel1ChartMode = CreateChartModeCombo();
            comboLevel1ChartMode.SelectedIndexChanged += delegate { DrawPhaseChartBySelectedMode(chartLevel1, lastLevel1Report, "I уровень: объект", comboLevel1ChartMode); };
            header.Controls.Add(btnCalculateLevel1);
            header.Controls.Add(new Label
            {
                Text = "График:",
                AutoSize = true,
                Margin = new Padding(16, 8, 4, 0)
            });
            header.Controls.Add(comboLevel1ChartMode);
            header.Controls.Add(CreateHelpButton(1));
            header.Controls.Add(new Label
            {
                Text = "I уровень: состояние объекта в целом",
                AutoSize = true,
                Margin = new Padding(16, 8, 0, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(split, 0.58);
            dataGridViewLevel1.Dock = DockStyle.Fill;
            dataGridViewLevel1.CellFormatting += StatusCellFormatting;
            StyleGrid(dataGridViewLevel1);

            chartLevel1.Dock = DockStyle.Fill;
            PrepareChart(chartLevel1, "Фазовая траектория объекта", "μ", "α");
            Control options = CreateChartOptionsPanel(chartLevel1,
                delegate { DrawPhaseChartBySelectedMode(chartLevel1, lastLevel1Report, "I уровень: объект", comboLevel1ChartMode); },
                true, false, false, false);

            split.Panel1.Controls.Add(dataGridViewLevel1);
            split.Panel2.Controls.Add(chartLevel1);
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(options, 0, 1);
            root.Controls.Add(split, 0, 2);
            StyleAllButtons(root);
            first_level_decomposition.Controls.Add(root);
        }

        private void BuildLevel2Tab()
        {
            second_level_decomposition.Controls.Clear();
            second_level_decomposition.BackColor = Color.FromArgb(244, 247, 251);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, AutoScroll = true };
            numericBlocks = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 12, Value = 3 };
            btnCreateBlocks = new Button { Text = "Создать блоки", AutoSize = true };
            btnAutoBlocks = new Button { Text = "Автораспределение", AutoSize = true };
            btnConfirmBlocks = new Button { Text = "Подтвердить", AutoSize = true };
            comboBlocks = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            labelBlockInfo = new Label { Text = "Точек в блоке: -", AutoSize = true, Margin = new Padding(14, 8, 8, 0), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            btnCreateBlocks.Click += BtnCreateBlocks_Click;
            btnAutoBlocks.Click += BtnAutoBlocks_Click;
            btnConfirmBlocks.Click += BtnConfirmBlocks_Click;
            comboBlocks.SelectedIndexChanged += delegate { RefreshBlockLists(); };

            top.Controls.Add(new Label { Text = "Количество блоков:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            top.Controls.Add(numericBlocks);
            top.Controls.Add(btnCreateBlocks);
            top.Controls.Add(new Label { Text = "Текущий блок:", AutoSize = true, Margin = new Padding(16, 8, 4, 0) });
            top.Controls.Add(comboBlocks);
            top.Controls.Add(labelBlockInfo);
            top.Controls.Add(btnAutoBlocks);
            top.Controls.Add(btnConfirmBlocks);
            top.Controls.Add(CreateHelpButton(2));

            var bottom = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(bottom, 0.46);

            var assignmentSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(assignmentSplit, 0.57);

            var lists = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            lists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            lists.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lists.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            listFreePoints = new ListBox { Dock = DockStyle.Fill };
            listBlockPoints = new ListBox { Dock = DockStyle.Fill };
            btnMovePoint = new Button { Text = ">", Width = 54, Height = 36, Anchor = AnchorStyles.None };
            btnReturnPoint = new Button { Text = "<", Width = 54, Height = 36, Anchor = AnchorStyles.None };
            btnMovePoint.Click += BtnMovePoint_Click;
            btnReturnPoint.Click += BtnReturnPoint_Click;
            listFreePoints.DoubleClick += BtnMovePoint_Click;
            listBlockPoints.DoubleClick += BtnReturnPoint_Click;

            var movePanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            movePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            movePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            movePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            movePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            movePanel.Controls.Add(btnMovePoint, 0, 1);
            movePanel.Controls.Add(btnReturnPoint, 0, 2);

            lists.Controls.Add(new Label { Text = "Свободные точки", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, 0);
            lists.Controls.Add(new Label { Text = "Точки выбранного блока", AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 2, 0);
            lists.Controls.Add(listFreePoints, 0, 1);
            lists.Controls.Add(movePanel, 1, 1);
            lists.Controls.Add(listBlockPoints, 2, 1);

            pictureBoxLevel2Scheme = new PictureBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, SizeMode = PictureBoxSizeMode.Zoom };
            assignmentSplit.Panel1.Controls.Add(lists);
            assignmentSplit.Panel2.Controls.Add(pictureBoxLevel2Scheme);
            bottom.Panel1.Controls.Add(assignmentSplit);

            var calcRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            calcRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            calcRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            calcRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            calcRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            calcRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var calcHeader = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            comboCalcBlock = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            comboLevel2ChartMode = CreateChartModeCombo();
            comboLevel2ChartMode.SelectedIndexChanged += delegate { DrawPhaseChartBySelectedMode(chartLevel2, lastLevel2Report, lastLevel2Title, comboLevel2ChartMode); };
            btnCalcLevel2 = new Button { Text = "Рассчитать блок", AutoSize = true };
            btnCalcLevel2.Click += BtnCalcLevel2_Click;
            comboCalcBlock.SelectedIndexChanged += delegate
            {
                if (blocksConfirmed && comboCalcBlock.SelectedItem != null)
                    BtnCalcLevel2_Click(comboCalcBlock, EventArgs.Empty);
            };
            calcHeader.Controls.Add(new Label { Text = "Блок для расчёта:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            calcHeader.Controls.Add(comboCalcBlock);
            calcHeader.Controls.Add(btnCalcLevel2);
            calcHeader.Controls.Add(new Label { Text = "Вид графика:", AutoSize = true, Margin = new Padding(16, 8, 4, 0) });
            calcHeader.Controls.Add(comboLevel2ChartMode);
            gridLevel2 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells };
            gridLevel2.CellFormatting += StatusCellFormatting;
            StyleGrid(gridLevel2);
            chartLevel2 = CreateChart("Фазовая траектория блока", "μ", "α");
            Control chart2Options = CreateChartOptionsPanel(chartLevel2,
                delegate { DrawPhaseChartBySelectedMode(chartLevel2, lastLevel2Report, lastLevel2Title, comboLevel2ChartMode); },
                true, false, false, false);
            calcRoot.Controls.Add(calcHeader, 0, 0);
            calcRoot.SetColumnSpan(calcHeader, 2);
            calcRoot.Controls.Add(chart2Options, 0, 1);
            calcRoot.SetColumnSpan(chart2Options, 2);
            var resultSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(resultSplit, 0.48);
            resultSplit.Panel1.Controls.Add(gridLevel2);
            resultSplit.Panel2.Controls.Add(chartLevel2);
            calcRoot.Controls.Add(resultSplit, 0, 2);
            calcRoot.SetColumnSpan(resultSplit, 2);
            bottom.Panel2.Controls.Add(calcRoot);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(bottom, 0, 1);
            StyleAllButtons(root);
            second_level_decomposition.Controls.Add(root);
        }

        private void BuildLevel3Tab()
        {
            third_level_decomposition.Controls.Clear();
            third_level_decomposition.BackColor = Color.FromArgb(244, 247, 251);

            tabControlLevel3 = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            tabPageLevel3Distribution = new TabPage("Распределение контрольных точек по подблокам");
            tabPageLevel3Calculation = new TabPage("Расчёты и графики")
            {
                Enabled = false
            };

            tabControlLevel3.TabPages.Add(tabPageLevel3Distribution);
            tabControlLevel3.TabPages.Add(tabPageLevel3Calculation);

            BuildLevel3DistributionPage();
            BuildLevel3CalculationPage();

            third_level_decomposition.Controls.Add(tabControlLevel3);
        }

        private void BuildLevel3DistributionPage()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var title = new Label
            {
                Text = "Подготовка блока и распределение контрольных точек по подблокам",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 6)
            };

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                AutoScroll = true
            };

            comboLevel3Block = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboLevel3Block.SelectedIndexChanged += delegate { LoadSelectedLevel3Block(); };

            numericRigidTolerance = new NumericUpDown
            {
                Width = 95,
                DecimalPlaces = 4,
                Increment = 0.0001M,
                Minimum = 0.0001M,
                Maximum = 10M,
                Value = 0.0007M
            };
            SetNumericUpDownValue(numericRigidTolerance, SafeReadEpsilon(0.0007));

            btnCalcLevel3 = new Button
            {
                Text = "Подтвердить блок и рассчитать таблицы",
                Width = 300,
                Enabled = false
            };
            btnCalcLevel3.Click += BtnCalcLevel3_Click;

            btnShowLevel3Links = new Button
            {
                Text = "График отклонений",
                AutoSize = true,
                Enabled = false
            };
            btnShowLevel3Links.Click += BtnShowLevel3Links_Click;

            top.Controls.Add(new Label
            {
                Text = "Разбиваемый блок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 4, 0)
            });
            top.Controls.Add(comboLevel3Block);
            top.Controls.Add(new Label
            {
                Text = "Допуск жёсткости:",
                AutoSize = true,
                Margin = new Padding(16, 8, 4, 0)
            });
            top.Controls.Add(numericRigidTolerance);
            top.Controls.Add(btnCalcLevel3);
            top.Controls.Add(btnShowLevel3Links);
            top.Controls.Add(CreateHelpButton(3));

            labelLevel3Status = new Label
            {
                Text = "Сначала подтвердите распределение точек по блокам на II уровне.",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(0, 5, 0, 7)
            };

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            var blockGroup = new GroupBox
            {
                Text = "1. Подготовка контрольных точек разбиваемого блока",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            var blockLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            blockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            blockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            blockLayout.Controls.Add(new Label
            {
                Text = "Нераспределённые точки",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 0, 0);
            blockLayout.Controls.Add(new Label
            {
                Text = "Точки выбранного блока",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 2, 0);

            listLevel3AvailableBlockPoints = new ListBox { Dock = DockStyle.Fill };
            listLevel3BlockPoints = new ListBox { Dock = DockStyle.Fill };

            btnLevel3AddPointToBlock = new Button
            {
                Text = ">",
                Width = 44,
                Height = 34,
                Anchor = AnchorStyles.None
            };
            btnLevel3RemovePointFromBlock = new Button
            {
                Text = "<",
                Width = 44,
                Height = 34,
                Anchor = AnchorStyles.None
            };
            btnLevel3AddPointToBlock.Click += BtnLevel3AddPointToBlock_Click;
            btnLevel3RemovePointFromBlock.Click += BtnLevel3RemovePointFromBlock_Click;
            listLevel3AvailableBlockPoints.DoubleClick += BtnLevel3AddPointToBlock_Click;
            listLevel3BlockPoints.DoubleClick += BtnLevel3RemovePointFromBlock_Click;

            var blockButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            blockButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            blockButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            blockButtons.Controls.Add(btnLevel3AddPointToBlock, 0, 1);
            blockButtons.Controls.Add(btnLevel3RemovePointFromBlock, 0, 2);

            blockLayout.Controls.Add(listLevel3AvailableBlockPoints, 0, 1);
            blockLayout.Controls.Add(blockButtons, 1, 1);
            blockLayout.Controls.Add(listLevel3BlockPoints, 2, 1);
            blockGroup.Controls.Add(blockLayout);

            var subblockGroup = new GroupBox
            {
                Text = "2. Распределение контрольных точек по подблокам",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            var subblockLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            subblockLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var settings = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };

            numericLevel3Subblocks = new NumericUpDown
            {
                Width = 65,
                Minimum = 1,
                Maximum = 20,
                Value = 2,
                Enabled = false
            };
            numericLevel3PointsPerSubblock = new NumericUpDown
            {
                Width = 65,
                Minimum = 2,
                Maximum = 100,
                Value = 2,
                Enabled = false
            };
            numericLevel3Subblocks.ValueChanged += delegate
            {
                if (level3SubblockMap.Count > 0)
                    ClearLevel3SubblockDistribution();
                UpdateLevel3SubblockInfo();
            };
            numericLevel3PointsPerSubblock.ValueChanged += delegate
            {
                if (level3SubblockMap.Count > 0)
                    ClearLevel3SubblockDistribution();
                UpdateLevel3SubblockInfo();
            };

            btnCreateLevel3Subblocks = new Button
            {
                Text = "Создать подблоки",
                AutoSize = true,
                Enabled = false
            };
            btnCreateLevel3Subblocks.Click += BtnCreateLevel3Subblocks_Click;

            settings.Controls.Add(new Label
            {
                Text = "Количество подблоков:",
                AutoSize = true,
                Margin = new Padding(0, 8, 4, 0)
            });
            settings.Controls.Add(numericLevel3Subblocks);
            settings.Controls.Add(new Label
            {
                Text = "Точек на подблок:",
                AutoSize = true,
                Margin = new Padding(12, 8, 4, 0)
            });
            settings.Controls.Add(numericLevel3PointsPerSubblock);
            settings.Controls.Add(btnCreateLevel3Subblocks);

            var selectPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };

            comboLevel3Subblock = new ComboBox
            {
                Width = 135,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            comboLevel3Subblock.SelectedIndexChanged += delegate { RefreshLevel3SubblockLists(); };

            labelLevel3SubblockInfo = new Label
            {
                Text = "Нужно: -",
                AutoSize = true,
                Margin = new Padding(12, 8, 4, 0)
            };

            btnAutoLevel3Subblocks = new Button
            {
                Text = "Автораспределение",
                AutoSize = true,
                Enabled = false
            };
            btnAutoLevel3Subblocks.Click += BtnAutoLevel3Subblocks_Click;

            selectPanel.Controls.Add(new Label
            {
                Text = "Выберите подблок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 4, 0)
            });
            selectPanel.Controls.Add(comboLevel3Subblock);
            selectPanel.Controls.Add(labelLevel3SubblockInfo);
            selectPanel.Controls.Add(btnAutoLevel3Subblocks);

            var assignLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            assignLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            assignLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            assignLayout.Controls.Add(new Label
            {
                Text = "Свободные точки",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 0, 0);
            assignLayout.Controls.Add(new Label
            {
                Text = "Точки выбранного подблока",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            }, 2, 0);

            listLevel3FreeSubblockPoints = new ListBox
            {
                Dock = DockStyle.Fill,
                Enabled = false
            };
            listLevel3SelectedSubblockPoints = new ListBox
            {
                Dock = DockStyle.Fill,
                Enabled = false
            };

            btnMoveLevel3Point = new Button
            {
                Text = ">",
                Width = 44,
                Height = 34,
                Anchor = AnchorStyles.None,
                Enabled = false
            };
            btnReturnLevel3Point = new Button
            {
                Text = "<",
                Width = 44,
                Height = 34,
                Anchor = AnchorStyles.None,
                Enabled = false
            };
            btnMoveLevel3Point.Click += BtnMoveLevel3Point_Click;
            btnReturnLevel3Point.Click += BtnReturnLevel3Point_Click;
            listLevel3FreeSubblockPoints.DoubleClick += BtnMoveLevel3Point_Click;
            listLevel3SelectedSubblockPoints.DoubleClick += BtnReturnLevel3Point_Click;

            var moveButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            moveButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            moveButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            moveButtons.Controls.Add(btnMoveLevel3Point, 0, 1);
            moveButtons.Controls.Add(btnReturnLevel3Point, 0, 2);

            assignLayout.Controls.Add(listLevel3FreeSubblockPoints, 0, 1);
            assignLayout.Controls.Add(moveButtons, 1, 1);
            assignLayout.Controls.Add(listLevel3SelectedSubblockPoints, 2, 1);

            btnConfirmLevel3Subblocks = new Button
            {
                Text = "Подтвердить распределение по подблокам",
                AutoSize = true,
                Dock = DockStyle.Right,
                Enabled = false
            };
            btnConfirmLevel3Subblocks.Click += BtnConfirmLevel3Subblocks_Click;

            subblockLayout.Controls.Add(settings, 0, 0);
            subblockLayout.Controls.Add(selectPanel, 0, 1);
            subblockLayout.Controls.Add(assignLayout, 0, 2);
            subblockLayout.Controls.Add(btnConfirmLevel3Subblocks, 0, 3);
            subblockGroup.Controls.Add(subblockLayout);

            var tablesGroup = new GroupBox
            {
                Text = "3. Таблицы разностей координат и контроля жёсткости",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var tablesSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 7,
                Panel1MinSize = 80,
                Panel2MinSize = 80
            };
            SetInitialSplitterDistance(tablesSplit, 0.50);

            var differencesPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            differencesPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            differencesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            differencesPanel.Controls.Add(new Label
            {
                Text = "Таблица разности координат Z контрольных точек",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4)
            }, 0, 0);

            gridLevel3Differences = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            StyleGrid(gridLevel3Differences);
            differencesPanel.Controls.Add(gridLevel3Differences, 0, 1);

            var linksPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            linksPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            linksPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            linksPanel.Controls.Add(new Label
            {
                Text = "Жёсткость связей между контрольными точками",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 4, 0, 4)
            }, 0, 0);

            gridLevel3Links = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            StyleGrid(gridLevel3Links);
            linksPanel.Controls.Add(gridLevel3Links, 0, 1);

            tablesSplit.Panel1.Controls.Add(differencesPanel);
            tablesSplit.Panel2.Controls.Add(linksPanel);
            tablesGroup.Controls.Add(tablesSplit);

            content.Controls.Add(blockGroup, 0, 0);
            content.Controls.Add(subblockGroup, 1, 0);
            content.Controls.Add(tablesGroup, 2, 0);

            root.Controls.Add(title, 0, 0);
            root.Controls.Add(top, 0, 1);
            root.Controls.Add(labelLevel3Status, 0, 2);
            root.Controls.Add(content, 0, 3);

            StyleAllButtons(root);
            tabPageLevel3Distribution.Controls.Add(root);
        }

        private void BuildLevel3CalculationPage()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                AutoScroll = true
            };

            comboLevel3Cluster = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            comboLevel3Cluster.SelectedIndexChanged += delegate
            {
                RecalculateLevel3ResponseReportFromSelectedCluster();
                if (gridLevel3Clusters != null)
                {
                    gridLevel3Clusters.DataSource = lastLevel3ResponseReport;
                    ApplyGridNumberFormat(gridLevel3Clusters);
                }
                DrawLevel3SelectedChart();
            };

            btnCalcLevel3Response = new Button
            {
                Text = "Рассчитать подблок",
                AutoSize = true,
                Enabled = false
            };
            btnCalcLevel3Response.Click += BtnCalcLevel3Response_Click;

            comboLevel3ChartMode = new ComboBox
            {
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboLevel3ChartMode.Items.AddRange(new object[]
            {
                "Отклик: фазовая α(μ)",
                "Отклик: μ(t)",
                "Отклик: α(t)",
                "Отклик: сглаживание μ(t)",
                "Отклик: сглаживание α(t)"
            });
            comboLevel3ChartMode.SelectedIndex = 0;
            comboLevel3ChartMode.SelectedIndexChanged += delegate { DrawLevel3SelectedChart(); };

            header.Controls.Add(new Label
            {
                Text = "Выберите подблок:",
                AutoSize = true,
                Margin = new Padding(0, 8, 4, 0)
            });
            header.Controls.Add(comboLevel3Cluster);
            header.Controls.Add(btnCalcLevel3Response);
            header.Controls.Add(new Label
            {
                Text = "Вид графика:",
                AutoSize = true,
                Margin = new Padding(16, 8, 4, 0)
            });
            header.Controls.Add(comboLevel3ChartMode);
            header.Controls.Add(CreateHelpButton(3));

            chartLevel3Links = CreateChart("Функция отклика подблока", "μ", "α");
            Control options = CreateChartOptionsPanel(
                chartLevel3Links,
                delegate { DrawLevel3SelectedChart(); },
                true,
                false,
                false,
                false);

            gridLevel3Clusters = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            gridLevel3Clusters.CellFormatting += StatusCellFormatting;
            StyleGrid(gridLevel3Clusters);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 7,
                Panel1MinSize = 80,
                Panel2MinSize = 80
            };
            SetInitialSplitterDistance(split, 0.48);
            split.Panel1.Controls.Add(gridLevel3Clusters);
            split.Panel2.Controls.Add(chartLevel3Links);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(options, 0, 1);
            root.Controls.Add(split, 0, 2);

            StyleAllButtons(root);
            tabPageLevel3Calculation.Controls.Add(root);
        }

        private void BuildLevel4Tab()
        {
            fourth_level_decomposition.Controls.Clear();
            fourth_level_decomposition.BackColor = Color.FromArgb(244, 247, 251);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, AutoScroll = true };
            comboLevel4ChartMode = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            comboLevel4ChartMode.Items.AddRange(new object[]
            {
                "Z(t) с границами",
                "Сглаживание Z(t)"
            });
            comboLevel4ChartMode.SelectedIndex = 0;
            comboLevel4ChartMode.SelectedIndexChanged += delegate { DrawPointChart(lastLevel4Rows, lastLevel4Point); };
            radioLevel4AllPoints = new RadioButton { Text = "Все точки", AutoSize = true, Checked = true, Margin = new Padding(4, 7, 8, 0) };
            radioLevel4OutsideClusters = new RadioButton { Text = "Только точки вне кластеров III уровня", AutoSize = true, Margin = new Padding(4, 7, 16, 0) };
            radioLevel4AllPoints.CheckedChanged += delegate { if (radioLevel4AllPoints.Checked) UpdateLevel4PointSelector(true); };
            radioLevel4OutsideClusters.CheckedChanged += delegate { if (radioLevel4OutsideClusters.Checked) UpdateLevel4PointSelector(true); };
            btnCalcLevel4 = new Button { Text = "Подтвердить", AutoSize = true };
            btnCalcLevel4.Click += BtnCalcLevel4_Click;
            btnSelectAllLevel4 = new Button { Text = "Выбрать все", AutoSize = true };
            btnSelectAllLevel4.Click += delegate { SelectAllVisibleLevel4Points(); };
            btnResetLevel4Selection = new Button { Text = "Сбросить все", AutoSize = true };
            btnResetLevel4Selection.Click += delegate { ResetLevel4Selection(); };
            labelLevel4Status = new Label { Text = "Состояние выбранных точек: -", AutoSize = true, Margin = new Padding(16, 8, 0, 0), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            top.Controls.Add(new Label { Text = "Фильтр точек:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
            top.Controls.Add(radioLevel4AllPoints);
            top.Controls.Add(radioLevel4OutsideClusters);
            top.Controls.Add(new Label { Text = "График:", AutoSize = true, Margin = new Padding(16, 8, 4, 0) });
            top.Controls.Add(comboLevel4ChartMode);
            top.Controls.Add(CreateHelpButton(4));
            top.Controls.Add(labelLevel4Status);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(split, 0.22);

            var selectorPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(6) };
            selectorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selectorPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            selectorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selectorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selectorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selectorPanel.Controls.Add(new Label { Text = "Контрольные точки", Dock = DockStyle.Fill, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 6) }, 0, 0);
            checkedLevel4Points = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle };
            checkedLevel4Points.ItemCheck += delegate { BeginInvoke(new Action(delegate { DrawPointChart(lastLevel4Rows, lastLevel4Point); })); };
            selectorPanel.Controls.Add(checkedLevel4Points, 0, 1);
            selectorPanel.Controls.Add(btnSelectAllLevel4, 0, 2);
            selectorPanel.Controls.Add(btnCalcLevel4, 0, 3);
            selectorPanel.Controls.Add(btnResetLevel4Selection, 0, 4);
            split.Panel1.Controls.Add(selectorPanel);

            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterWidth = 7, Panel1MinSize = 80, Panel2MinSize = 80 };
            SetInitialSplitterDistance(right, 0.58);
            var chartRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(6) };
            chartRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            chartRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            chartLevel4 = CreateChart("Z(t) выбранных контрольных точек", "Цикл наблюдения (t)", "Z");
            Control chart4Options = CreateChartOptionsPanel(chartLevel4,
                delegate { DrawPointChart(lastLevel4Rows, lastLevel4Point); },
                true, true, true, true);
            chartRoot.Controls.Add(chart4Options, 0, 0);
            chartRoot.Controls.Add(chartLevel4, 0, 1);
            right.Panel1.Controls.Add(chartRoot);

            gridLevel4 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells };
            gridLevel4.CellFormatting += StatusCellFormatting;
            StyleGrid(gridLevel4);
            right.Panel2.Controls.Add(gridLevel4);
            split.Panel2.Controls.Add(right);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(split, 0, 1);
            StyleAllButtons(root);
            fourth_level_decomposition.Controls.Add(root);
        }



        #endregion

        #region 3. Данные
        private void btnOpenDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                if (!OpenDBFile())
                    return;

                GetTableNames();
                comboBox1.Enabled = true;
                btnReplaceScheme.Enabled = true;
                LoadSchemeToPictureBoxes();
                LoadObjectSettings();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка открытия базы: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;

            currentTableName = comboBox1.SelectedItem.ToString();
            ShowTable(SQL_AllTable());
            UpdateAllPointSelectors();
        }

        private void BtnReplaceScheme_Click(object sender, EventArgs e)
        {
            try
            {
                if (SQLiteConn == null)
                {
                    ShowError("Сначала откройте базу данных.");
                    return;
                }

                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*";
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    pendingSchemeBytes = File.ReadAllBytes(dialog.FileName);
                    hasPendingSchemeChange = true;

                    using (MemoryStream ms = new MemoryStream(pendingSchemeBytes))
                    using (Image temp = Image.FromStream(ms))
                    using (Image preview = new Bitmap(temp))
                    {
                        SetPictureBoxImage(pictureBox1, preview);
                        SetPictureBoxImage(pictureBoxLevel2Scheme, preview);
                    }

                    MessageBox.Show("Новая схема загружена для предпросмотра. Для записи в БД нажмите «Сохранить таблицу».", "Схема", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка замены схемы: " + ex.Message);
            }
        }

        private void BtnSaveTable_Click(object sender, EventArgs e)
        {
            try
            {
                SaveCurrentTable();
                SavePendingSchemeIfNeeded();
                SaveObjectSettingsOnly();
                MessageBox.Show("Изменения сохранены в SQLite.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка сохранения: " + ex.Message);
            }
        }

        private void btnAddEpoch_Click(object sender, EventArgs e)
        {
            try
            {
                if (dTable == null || dTable.Rows.Count == 0)
                    return;

                GenerateNewEpoch();
                dataGridView1.Refresh();
                btnSaveTable.Enabled = true;
                UpdateAllPointSelectors();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка добавления эпохи: " + ex.Message);
            }
        }

        private void btnDeleteEpoch_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    ShowError("Выберите строку для удаления.");
                    return;
                }

                if (dTable.Rows.Count <= 2)
                {
                    ShowError("Должно оставаться не менее двух эпох.");
                    return;
                }

                DataRowView rowView = dataGridView1.CurrentRow.DataBoundItem as DataRowView;
                if (rowView == null)
                    return;

                rowView.Row.Delete();
                dTable.AcceptChanges();
                dataGridView1.Refresh();
                btnSaveTable.Enabled = true;
                UpdateAllPointSelectors();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка удаления: " + ex.Message);
            }
        }

        #endregion

        #region 4. I уровень

        private void btnCalculateLevel1_Click(object sender, EventArgs e)
        {
            try
            {
                List<DataColumn> pointColumns = GetPointColumns();
                if (dTable == null || dTable.Rows.Count == 0 || pointColumns.Count == 0)
                {
                    ShowError("Загрузите таблицу с координатами контрольных точек.");
                    return;
                }

                List<PhaseAnalysis> report = CalculatePhaseForColumns(pointColumns, "Объект");
                lastLevel1Report = report;
                dataGridViewLevel1.DataSource = report;
                ApplyGridNumberFormat(dataGridViewLevel1);
                DrawPhaseChartBySelectedMode(chartLevel1, report, "I уровень: объект", comboLevel1ChartMode);
                tabControl1.SelectedTab = first_level_decomposition;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка I уровня: " + ex.Message);
            }
        }

        private List<PhaseAnalysis> CalculateResponsePhaseForColumns(List<DataColumn> pointColumns, string labelPrefix)
        {
            DataTable source = GetCalculationTable();
            if (source == null || source.Rows.Count == 0 || pointColumns == null)
                return new List<PhaseAnalysis>();
            List<string> pointNames = pointColumns.Select(c => c.ColumnName).Where(n => source.Columns.Contains(n)).ToList();
            if (pointNames.Count < 2)
                return new List<PhaseAnalysis>();

            double eps = ReadEpsilon();
            int sensorCount = pointNames.Count;

            double[] baseValues = new double[sensorCount];
            for (int j = 0; j < sensorCount; j++)
            {
                DataColumn col = source.Columns[pointNames[j]];
                baseValues[j] = ReadCellDouble(source.Rows[0], col);
            }

            double baseM = Norm(baseValues);
            List<PhaseAnalysis> report = new List<PhaseAnalysis>();

            for (int i = 0; i < source.Rows.Count; i++)
            {
                double[] current = new double[sensorCount];
                double[] currentPlus = new double[sensorCount];
                double[] currentMinus = new double[sensorCount];

                for (int j = 0; j < sensorCount; j++)
                {
                    DataColumn col = source.Columns[pointNames[j]];
                    current[j] = ReadCellDouble(source.Rows[i], col);
                    currentPlus[j] = current[j] + eps;
                    currentMinus[j] = current[j] - eps;
                }

                double m = Norm(current);
                double mp = Norm(currentPlus);
                double mm = Norm(currentMinus);
                double a = ResponseAngleByDisplacement(baseValues, current);
                double ap = ResponseAngleByDisplacement(baseValues, currentPlus);
                double am = ResponseAngleByDisplacement(baseValues, currentMinus);

                double r = Math.Abs(mp - mm) / 2.0;
                double l = Math.Abs(m - baseM);

                report.Add(new PhaseAnalysis
                {
                    Label = Convert.ToString(source.Rows[i][0]),
                    M = Round(m),
                    MPlus = Round(mp),
                    MMinus = Round(mm),
                    A = Round(a),
                    APlus = Round(ap),
                    AMinus = Round(am),
                    R = Round(r),
                    L = Round(l),
                    StatusText = GetStateByLimit(l, r)
                });
            }

            return report;
        }

        #endregion

        #region 5. II уровень

        private void BtnCreateBlocks_Click(object sender, EventArgs e)
        {
            blockMap.Clear();
            int count = (int)numericBlocks.Value;
            for (int i = 1; i <= count; i++)
            {
                char blockLetter = (char)('А' + i - 1);
                blockMap["Блок " + blockLetter] = new List<string>();
            }

            comboBlocks.Items.Clear();
            comboCalcBlock.Items.Clear();
            comboLevel3Block.Items.Clear();
            foreach (string key in blockMap.Keys)
            {
                comboBlocks.Items.Add(key);
                comboCalcBlock.Items.Add(key);
                comboLevel3Block.Items.Add(key);
            }

            if (comboBlocks.Items.Count > 0) comboBlocks.SelectedIndex = 0;
            if (comboCalcBlock.Items.Count > 0) comboCalcBlock.SelectedIndex = 0;
            if (comboLevel3Block.Items.Count > 0) comboLevel3Block.SelectedIndex = 0;
            blocksConfirmed = false;
            RefreshBlockLists();
        }

        private void BtnMovePoint_Click(object sender, EventArgs e)
        {
            if (comboBlocks.SelectedItem == null || listFreePoints.SelectedItem == null)
                return;

            string block = comboBlocks.SelectedItem.ToString();
            string point = listFreePoints.SelectedItem.ToString();
            if (!blockMap.ContainsKey(block))
                return;

            blockMap[block].Add(point);
            blocksConfirmed = false;
            RefreshBlockLists();
        }

        private void BtnReturnPoint_Click(object sender, EventArgs e)
        {
            if (comboBlocks.SelectedItem == null || listBlockPoints.SelectedItem == null)
                return;

            string block = comboBlocks.SelectedItem.ToString();
            string point = listBlockPoints.SelectedItem.ToString();
            blockMap[block].Remove(point);
            blocksConfirmed = false;
            RefreshBlockLists();
        }

        private void BtnAutoBlocks_Click(object sender, EventArgs e)
        {
            if (blockMap.Count == 0)
                BtnCreateBlocks_Click(sender, e);

            List<string> points = GetPointColumns().Select(c => c.ColumnName).ToList();
            foreach (string key in blockMap.Keys.ToList())
                blockMap[key].Clear();

            int blockIndex = 0;
            int baseSize = points.Count / blockMap.Count;
            int remainder = points.Count % blockMap.Count;
            List<string> keys = blockMap.Keys.ToList();

            foreach (string block in keys)
            {
                int take = baseSize + (blockIndex < remainder ? 1 : 0);
                foreach (string point in points.Skip(blockIndex * baseSize + Math.Min(blockIndex, remainder)).Take(take))
                    blockMap[block].Add(point);
                blockIndex++;
            }

            blocksConfirmed = false;
            RefreshBlockLists();
        }

        private void BtnConfirmBlocks_Click(object sender, EventArgs e)
        {
            if (blockMap.Count == 0)
            {
                ShowError("Сначала создайте блоки.");
                return;
            }

            if (blockMap.Any(x => x.Value.Count < 2))
            {
                ShowError("Каждый блок должен содержать хотя бы две точки.");
                return;
            }

            int firstBlockCount = blockMap.First().Value.Count;
            if (blockMap.Any(x => x.Value.Count != firstBlockCount))
            {
                ShowError("Во всех блоках должно быть равное количество точек.");
                return;
            }

            blocksConfirmed = true;
            RefreshBlockSelectors();
            MessageBox.Show("Распределение точек подтверждено.", "II уровень", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCalcLevel2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!blocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по блокам.");
                    return;
                }

                if (comboCalcBlock.SelectedItem == null)
                    return;

                string block = comboCalcBlock.SelectedItem.ToString();
                List<DataColumn> cols = ColumnsByNames(blockMap[block]);
                List<PhaseAnalysis> report = CalculatePhaseForColumns(cols, block);
                lastLevel2Report = report;
                lastLevel2Title = "II уровень: " + block;
                gridLevel2.DataSource = report;
                ApplyGridNumberFormat(gridLevel2);
                DrawPhaseChartBySelectedMode(chartLevel2, report, lastLevel2Title, comboLevel2ChartMode);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка II уровня: " + ex.Message);
            }
        }

        #endregion

        #region 6. III уровень

        private void BtnLevel3AddPointToBlock_Click(object sender, EventArgs e)
        {
            if (listLevel3AvailableBlockPoints == null ||
                listLevel3AvailableBlockPoints.SelectedItem == null)
                return;

            string point = listLevel3AvailableBlockPoints.SelectedItem.ToString();
            if (!level3PreparedBlockPoints.Contains(point))
                level3PreparedBlockPoints.Add(point);

            InvalidateLevel3PreparedCalculation();
            RefreshLevel3BlockPreparationLists();
        }

        private void BtnLevel3RemovePointFromBlock_Click(object sender, EventArgs e)
        {
            if (listLevel3BlockPoints == null ||
                listLevel3BlockPoints.SelectedItem == null)
                return;

            string point = listLevel3BlockPoints.SelectedItem.ToString();
            level3PreparedBlockPoints.Remove(point);

            InvalidateLevel3PreparedCalculation();
            RefreshLevel3BlockPreparationLists();
        }

        private void BtnCalcLevel3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!blocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по блокам на II уровне.");
                    return;
                }
                if (comboLevel3Block == null || comboLevel3Block.SelectedItem == null)
                {
                    ShowError("Выберите блок для разбиения.");
                    return;
                }
                if (level3PreparedBlockPoints.Count < 2)
                {
                    ShowError("Для III уровня требуется минимум две контрольные точки.");
                    return;
                }

                level3PreparedBlockName = comboLevel3Block.SelectedItem.ToString();
                blockMap[level3PreparedBlockName] = new List<string>(level3PreparedBlockPoints);

                double tolerance = (double)numericRigidTolerance.Value;
                lastLevel3Links = BuildLinks(level3PreparedBlockPoints, tolerance);
                lastLevel3Clusters = new List<ClusterInfo>();
                lastLevel3ResponseReport = new List<PhaseAnalysis>();
                lastLevel3Title = "III уровень: " + level3PreparedBlockName;
                level3BlockPrepared = true;

                ClearLevel3SubblockDistribution();

                gridLevel3Differences.DataSource = BuildLevel3DifferencesTable(level3PreparedBlockPoints);
                gridLevel3Links.DataSource = lastLevel3Links;
                ApplyGridNumberFormat(gridLevel3Differences);
                ApplyGridNumberFormat(gridLevel3Links);

                PrepareLevel3SubblockControls();
                btnShowLevel3Links.Enabled = lastLevel3Links.Count > 0;

                labelLevel3Status.Text =
                    "Блок " + level3PreparedBlockName + " подтверждён. Точек: " +
                    level3PreparedBlockPoints.Count.ToString(CultureInfo.InvariantCulture) +
                    "; связей проверено: " +
                    lastLevel3Links.Count.ToString(CultureInfo.InvariantCulture) +
                    ". Укажите количество подблоков и точек на каждом подблоке.";

                MessageBox.Show(
                    "Состав блока подтверждён. Таблицы разностей координат и жёсткости связей рассчитаны.",
                    "III уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подготовки III уровня: " + ex.Message);
            }
        }

        private void BtnCreateLevel3Subblocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (!level3BlockPrepared)
                {
                    ShowError("Сначала подтвердите состав разбиваемого блока.");
                    return;
                }

                int count = (int)numericLevel3Subblocks.Value;
                int perSubblock = (int)numericLevel3PointsPerSubblock.Value;
                int required = count * perSubblock;

                if (required != level3PreparedBlockPoints.Count)
                {
                    ShowError(
                        "Количество точек должно распределяться без остатка.\n" +
                        "Сейчас требуется " + required.ToString(CultureInfo.InvariantCulture) +
                        ", а в блоке " +
                        level3PreparedBlockPoints.Count.ToString(CultureInfo.InvariantCulture) +
                        " точек.\nИзмените количество подблоков, число точек на подблоке или состав блока.");
                    return;
                }

                level3SubblockMap.Clear();
                comboLevel3Subblock.Items.Clear();
                for (int i = 1; i <= count; i++)
                {
                    string name = "Подблок " + i.ToString(CultureInfo.InvariantCulture);
                    level3SubblockMap[name] = new List<string>();
                    comboLevel3Subblock.Items.Add(name);
                }

                comboLevel3Subblock.Enabled = true;
                listLevel3FreeSubblockPoints.Enabled = true;
                listLevel3SelectedSubblockPoints.Enabled = true;
                btnMoveLevel3Point.Enabled = true;
                btnReturnLevel3Point.Enabled = true;
                btnAutoLevel3Subblocks.Enabled = true;
                btnConfirmLevel3Subblocks.Enabled = true;
                level3SubblocksConfirmed = false;

                if (comboLevel3Subblock.Items.Count > 0)
                    comboLevel3Subblock.SelectedIndex = 0;

                RefreshLevel3SubblockLists();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка создания подблоков: " + ex.Message);
            }
        }

        private void BtnMoveLevel3Point_Click(object sender, EventArgs e)
        {
            if (comboLevel3Subblock == null ||
                comboLevel3Subblock.SelectedItem == null ||
                listLevel3FreeSubblockPoints == null ||
                listLevel3FreeSubblockPoints.SelectedItem == null)
                return;

            string subblock = comboLevel3Subblock.SelectedItem.ToString();
            string point = listLevel3FreeSubblockPoints.SelectedItem.ToString();
            int required = (int)numericLevel3PointsPerSubblock.Value;

            if (level3SubblockMap[subblock].Count >= required)
            {
                ShowError(subblock + " уже содержит требуемое количество точек: " +
                          required.ToString(CultureInfo.InvariantCulture) + ".");
                return;
            }

            level3SubblockMap[subblock].Add(point);
            level3SubblocksConfirmed = false;
            RefreshLevel3SubblockLists();
        }

        private void BtnReturnLevel3Point_Click(object sender, EventArgs e)
        {
            if (comboLevel3Subblock == null ||
                comboLevel3Subblock.SelectedItem == null ||
                listLevel3SelectedSubblockPoints == null ||
                listLevel3SelectedSubblockPoints.SelectedItem == null)
                return;

            string subblock = comboLevel3Subblock.SelectedItem.ToString();
            string point = listLevel3SelectedSubblockPoints.SelectedItem.ToString();
            level3SubblockMap[subblock].Remove(point);
            level3SubblocksConfirmed = false;
            RefreshLevel3SubblockLists();
        }

        private void BtnAutoLevel3Subblocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (level3SubblockMap.Count == 0)
                {
                    ShowError("Сначала создайте подблоки.");
                    return;
                }

                foreach (List<string> points in level3SubblockMap.Values)
                    points.Clear();

                int perSubblock = (int)numericLevel3PointsPerSubblock.Value;
                int index = 0;

                foreach (string subblock in level3SubblockMap.Keys.OrderBy(ParseLevel3SubblockNumber))
                {
                    for (int i = 0; i < perSubblock; i++)
                    {
                        level3SubblockMap[subblock].Add(level3PreparedBlockPoints[index]);
                        index++;
                    }
                }

                level3SubblocksConfirmed = false;
                RefreshLevel3SubblockLists();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка автораспределения по подблокам: " + ex.Message);
            }
        }

        private void BtnConfirmLevel3Subblocks_Click(object sender, EventArgs e)
        {
            try
            {
                if (level3SubblockMap.Count == 0)
                {
                    ShowError("Сначала создайте подблоки.");
                    return;
                }

                int required = (int)numericLevel3PointsPerSubblock.Value;
                foreach (KeyValuePair<string, List<string>> pair in level3SubblockMap)
                {
                    if (pair.Value.Count != required)
                    {
                        ShowError(
                            pair.Key + " содержит " +
                            pair.Value.Count.ToString(CultureInfo.InvariantCulture) +
                            " точек. Требуется строго " +
                            required.ToString(CultureInfo.InvariantCulture) + ".");
                        return;
                    }
                }

                HashSet<string> assigned = new HashSet<string>(
                    level3SubblockMap.Values.SelectMany(points => points));
                if (assigned.Count != level3PreparedBlockPoints.Count)
                {
                    ShowError("Не все точки блока распределены по подблокам.");
                    return;
                }

                lastLevel3Clusters = new List<ClusterInfo>();
                int clusterId = 1;
                foreach (string subblock in level3SubblockMap.Keys.OrderBy(ParseLevel3SubblockNumber))
                {
                    List<string> points = level3SubblockMap[subblock];
                    lastLevel3Clusters.Add(new ClusterInfo
                    {
                        Cluster = clusterId++,
                        Points = string.Join(", ", points),
                        Quantity = points.Count
                    });
                }

                level3SubblocksConfirmed = true;
                PopulateLevel3ClusterSelector(lastLevel3Clusters);

                comboLevel3Cluster.Enabled = comboLevel3Cluster.Items.Count > 0;
                btnCalcLevel3Response.Enabled = comboLevel3Cluster.Items.Count > 0;
                tabPageLevel3Calculation.Enabled = true;

                if (comboLevel3Cluster.Items.Count > 0)
                    comboLevel3Cluster.SelectedIndex = 0;

                labelLevel3Status.Text =
                    "Распределение подтверждено: " +
                    level3SubblockMap.Count.ToString(CultureInfo.InvariantCulture) +
                    " подблоков по " +
                    required.ToString(CultureInfo.InvariantCulture) +
                    " точек. Вкладка «Расчёты и графики» доступна.";

                UpdateLevel4PointSelector(true);
                tabControlLevel3.SelectedTab = tabPageLevel3Calculation;

                BtnCalcLevel3Response_Click(btnCalcLevel3Response, EventArgs.Empty);

                MessageBox.Show(
                    "Распределение контрольных точек по подблокам подтверждено.",
                    "III уровень",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подтверждения подблоков: " + ex.Message);
            }
        }

        private void BtnCalcLevel3Response_Click(object sender, EventArgs e)
        {
            try
            {
                if (!level3SubblocksConfirmed)
                {
                    ShowError("Сначала подтвердите распределение точек по подблокам.");
                    return;
                }
                if (comboLevel3Cluster == null ||
                    comboLevel3Cluster.SelectedItem == null)
                {
                    ShowError("Выберите подблок.");
                    return;
                }

                RecalculateLevel3ResponseReportFromSelectedCluster();
                gridLevel3Clusters.DataSource = lastLevel3ResponseReport;
                ApplyGridNumberFormat(gridLevel3Clusters);
                DrawLevel3SelectedChart();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка расчёта функции отклика III уровня: " + ex.Message);
            }
        }

        private void BtnShowLevel3Links_Click(object sender, EventArgs e)
        {
            if (lastLevel3Links == null || lastLevel3Links.Count == 0)
            {
                ShowError("Сначала выполните расчёт блока на III уровне.");
                return;
            }

            ShowLevel3LinksChartWindow();
        }

        #endregion

        #region 7. IV уровень

        public class PointStateRow
        {
            public string Эпоха { get; set; }
            public string Точка { get; set; }
            public double Z { get; set; }
            public double ZPlusE { get; set; }
            public double ZMinusE { get; set; }
            public double R { get; set; }
            public double L { get; set; }
            public string Статус { get; set; }
        }

        private void BtnCalcLevel4_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> selectedPoints = GetSelectedLevel4Points();
                if (selectedPoints.Count == 0)
                {
                    ShowError("Выберите хотя бы одну контрольную точку.");
                    return;
                }

                List<PointStateRow> rows = new List<PointStateRow>();
                foreach (string point in selectedPoints)
                    rows.AddRange(BuildPointRows(point));

                lastLevel4Rows = rows;
                lastLevel4SelectedPoints = selectedPoints;
                lastLevel4Point = selectedPoints.Count == 1 ? selectedPoints[0] : "выбранные точки";
                gridLevel4.DataSource = rows;
                ApplyGridNumberFormat(gridLevel4);
                DrawPointChart(rows, lastLevel4Point);

                bool hasAccident = rows.Any(r => r.Статус.Contains("Авар"));
                labelLevel4Status.Text = hasAccident ? "Состояние выбранных точек: есть аварийные отклонения" : "Состояние выбранных точек: без изменений";
                labelLevel4Status.ForeColor = hasAccident ? Color.Firebrick : Color.DarkGreen;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка IV уровня: " + ex.Message);
            }
        }

        #endregion

    }
}
