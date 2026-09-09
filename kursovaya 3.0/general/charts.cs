using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Kursovaya30
{
    public partial class Form1
    {
        private ComboBox CreateChartModeCombo()
        {
            ComboBox combo = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            combo.Items.AddRange(new object[]
            {
                "Фазовая траектория α(μ)",
                "Функция μ(t)",
                "Функция α(t)",
                "Сравнение сглаживания μ(t)",
                "Сравнение сглаживания α(t)"
            });
            combo.SelectedIndex = 0;
            return combo;
        }

        private Control CreateChartOptionsPanel(Chart chart, EventHandler redraw, bool mainDefault, bool plusDefault, bool minusDefault, bool forecastDefault)
        {
            var box = new GroupBox
            {
                Text = "Параметры графиков",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(10, 6, 10, 8)
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true
            };

            var controls = new ChartOptionControls
            {
                Main = new CheckBox { Text = "Основная траектория", AutoSize = true, Checked = mainDefault, Margin = new Padding(6, 4, 14, 4) },
                Plus = new CheckBox { Text = "+E", AutoSize = true, Checked = plusDefault, Margin = new Padding(6, 4, 14, 4) },
                Minus = new CheckBox { Text = "-E", AutoSize = true, Checked = minusDefault, Margin = new Padding(6, 4, 14, 4) },
                Forecast = new CheckBox { Text = "Прогноз", AutoSize = true, Checked = forecastDefault, Margin = new Padding(6, 4, 14, 4) }
            };

            EventHandler changed = delegate
            {
                if (redraw != null)
                    redraw(chart, EventArgs.Empty);
            };
            controls.Main.CheckedChanged += changed;
            controls.Plus.CheckedChanged += changed;
            controls.Minus.CheckedChanged += changed;
            controls.Forecast.CheckedChanged += changed;

            Button clear = new Button { Text = "Убрать все", Width = 150, Height = 32, Margin = new Padding(20, 0, 6, 0) };
            clear.Click += delegate
            {
                bool hasChecked = controls.Main.Checked || controls.Plus.Checked || controls.Minus.Checked || controls.Forecast.Checked;
                controls.Main.Checked = !hasChecked;
                controls.Plus.Checked = !hasChecked;
                controls.Minus.Checked = !hasChecked;
                controls.Forecast.Checked = !hasChecked;
            };

            panel.Controls.Add(controls.Main);
            panel.Controls.Add(controls.Plus);
            panel.Controls.Add(controls.Minus);
            panel.Controls.Add(controls.Forecast);
            panel.Controls.Add(clear);
            box.Controls.Add(panel);

            if (chart != null)
                chartOptions[chart] = controls;
            return box;
        }

        private ChartOptionControls GetChartOptions(Chart chart)
        {
            ChartOptionControls options;
            if (chart != null && chartOptions.TryGetValue(chart, out options))
                return options;
            return new ChartOptionControls
            {
                Main = new CheckBox { Checked = true },
                Plus = new CheckBox { Checked = false },
                Minus = new CheckBox { Checked = false },
                Forecast = new CheckBox { Checked = false }
            };
        }

        private void SetChartOptionsEnabled(Chart chart, bool enabled)
        {
            ChartOptionControls options;
            if (chart == null || !chartOptions.TryGetValue(chart, out options) || options == null)
                return;

            if (options.Main != null) options.Main.Enabled = enabled;
            if (options.Plus != null) options.Plus.Enabled = enabled;
            if (options.Minus != null) options.Minus.Enabled = enabled;
            if (options.Forecast != null) options.Forecast.Enabled = enabled;
        }

        private Chart CreateChart(string title, string xTitle, string yTitle)
        {
            Chart chart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            chart.ChartAreas.Add(new ChartArea("Main"));
            chart.Legends.Add(new Legend("Legend"));
            PrepareChart(chart, title, xTitle, yTitle);
            return chart;
        }

        private void PrepareChart(Chart chart, string title, string xTitle, string yTitle)
        {
            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Titles.Add(title);
            if (chart.ChartAreas.Count == 0)
                chart.ChartAreas.Add(new ChartArea("Main"));
            ChartArea area = chart.ChartAreas[0];
            area.AxisX.CustomLabels.Clear();
            area.AxisX.Minimum = double.NaN;
            area.AxisX.Maximum = double.NaN;
            area.AxisX.Interval = 0;
            area.AxisX.LabelStyle.Format = string.Empty;
            area.AxisX.ScaleView.ZoomReset(0);
            area.AxisX.Title = xTitle;
            area.AxisY.Title = yTitle;
            area.AxisX.IsStartedFromZero = false;
            area.AxisY.IsStartedFromZero = false;
            area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            area.AxisY.LabelStyle.Format = "0.####";
            area.AxisX.ScrollBar.Enabled = true;
            area.AxisX.ScaleView.Zoomable = true;
            area.CursorX.IsUserEnabled = true;
            area.CursorX.IsUserSelectionEnabled = true;
        }

        private void DrawPhaseChartBySelectedMode(Chart chart, List<PhaseAnalysis> report, string title, ComboBox combo)
        {
            string mode = combo == null || combo.SelectedItem == null ? "Фазовая траектория α(μ)" : combo.SelectedItem.ToString();
            DrawPhaseChartByMode(chart, report, title, mode);
        }

        private void DrawPhaseChartByMode(Chart chart, List<PhaseAnalysis> report, string title, string mode)
        {
            if (chart == null)
                return;
            if (report == null || report.Count == 0)
            {
                PrepareChart(chart, title, "Цикл", "Значение");
                return;
            }

            ChartOptionControls options = GetChartOptions(chart);
            List<string> labels = report.Select(r => r.Label).ToList();

            if (mode == "Функция μ(t)")
            {
                PrepareChart(chart, title + ": μ(t)", "Цикл наблюдения", "μ");
                PrepareTimeArea(chart.ChartAreas[0], "μ", labels, options.ShowForecast && HasVisibleBaseSeries(options));
                AddPhaseTimeSeriesByOptions(chart, labels,
                    report.Select(r => r.M).ToList(),
                    report.Select(r => r.MPlus).ToList(),
                    report.Select(r => r.MMinus).ToList(),
                    "μ(t)", options);
            }
            else if (mode == "Функция α(t)")
            {
                PrepareChart(chart, title + ": α(t)", "Цикл наблюдения", "α, рад");
                PrepareTimeArea(chart.ChartAreas[0], "α, рад", labels, options.ShowForecast && HasVisibleBaseSeries(options));
                AddPhaseTimeSeriesByOptions(chart, labels,
                    report.Select(r => r.A).ToList(),
                    report.Select(r => r.APlus).ToList(),
                    report.Select(r => r.AMinus).ToList(),
                    "α(t)", options);
            }
            else if (mode == "Сравнение сглаживания μ(t)")
            {
                PrepareChart(chart, title + ": сглаживание μ(t)", "Цикл наблюдения", "μ");
                PrepareTimeArea(chart.ChartAreas[0], "μ", labels, options.ShowForecast);
                DrawSmoothingComparison(chart, labels, report.Select(r => r.M).ToList(), "μ", options);
            }
            else if (mode == "Сравнение сглаживания α(t)")
            {
                PrepareChart(chart, title + ": сглаживание α(t)", "Цикл наблюдения", "α, рад");
                PrepareTimeArea(chart.ChartAreas[0], "α, рад", labels, options.ShowForecast);
                DrawSmoothingComparison(chart, labels, report.Select(r => r.A).ToList(), "α", options);
            }
            else
            {
                PrepareChart(chart, title + ": фазовая траектория α(μ)", "μ", "α, сек");
                DrawPhaseTrajectoryByOptions(chart, report, options);
                return;
            }

            ApplyAxisFromPoints(chart);
            ApplyPointNumbersToChart(chart);
        }

        private bool HasVisibleBaseSeries(ChartOptionControls options)
        {
            return options != null && (options.ShowMain || options.ShowPlus || options.ShowMinus);
        }

        private void DrawPhaseTrajectoryByOptions(Chart chart, List<PhaseAnalysis> report, ChartOptionControls options)
        {
            ChartArea area = chart.ChartAreas[0];
            area.AxisX.Title = "μ";
            area.AxisY.Title = "α, сек";
            area.AxisX.CustomLabels.Clear();
            area.AxisX.LabelStyle.Format = "0.###";
            area.AxisY.LabelStyle.Format = "0.#####";
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.Interval = 0;
            area.AxisX.Minimum = double.NaN;
            area.AxisX.Maximum = double.NaN;
            area.AxisY.Minimum = double.NaN;
            area.AxisY.Maximum = double.NaN;

            int visibleCount = (options.ShowMain ? 1 : 0) + (options.ShowPlus ? 1 : 0) + (options.ShowMinus ? 1 : 0);
            bool showCycleLabels = false;
            bool showForecastText = visibleCount == 1;
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            if (options.ShowMain)
            {
                Series series = CreateLineSeries("M vs A", true, showCycleLabels, Color.RoyalBlue);
                AddPhasePoints(series, report, r => r.M, r => RadToArcSeconds(r.A), ref minX, ref maxX, ref minY, ref maxY);
                chart.Series.Add(series);
            }
            if (options.ShowPlus)
            {
                Series series = CreateLineSeries("M+E vs A+E", true, showCycleLabels, Color.Orange);
                AddPhasePoints(series, report, r => r.MPlus, r => RadToArcSeconds(r.APlus), ref minX, ref maxX, ref minY, ref maxY);
                chart.Series.Add(series);
            }
            if (options.ShowMinus)
            {
                Series series = CreateLineSeries("M-E vs A-E", true, showCycleLabels, Color.OrangeRed);
                AddPhasePoints(series, report, r => r.MMinus, r => RadToArcSeconds(r.AMinus), ref minX, ref maxX, ref minY, ref maxY);
                chart.Series.Add(series);
            }
            if (options.ShowForecast)
            {
                if (options.ShowMain)
                    AddPhaseForecastConnected(chart, "M vs A", report.Select(r => r.M).ToList(), report.Select(r => RadToArcSeconds(r.A)).ToList(), "Прогноз M", Color.RoyalBlue, showForecastText, ref minX, ref maxX, ref minY, ref maxY);
                if (options.ShowPlus)
                    AddPhaseForecastConnected(chart, "M+E vs A+E", report.Select(r => r.MPlus).ToList(), report.Select(r => RadToArcSeconds(r.APlus)).ToList(), "Прогноз M+E", Color.Orange, showForecastText, ref minX, ref maxX, ref minY, ref maxY);
                if (options.ShowMinus)
                    AddPhaseForecastConnected(chart, "M-E vs A-E", report.Select(r => r.MMinus).ToList(), report.Select(r => RadToArcSeconds(r.AMinus)).ToList(), "Прогноз M-E", Color.OrangeRed, showForecastText, ref minX, ref maxX, ref minY, ref maxY);
            }

            ApplyXYScale(area, minX, maxX, minY, maxY);

            DisablePhaseValueLabelsKeepAxisLabels(chart);
            ApplyPhaseXAxisValueLabels(chart, GetPhaseXAxisValuesFromVisibleChartSeries(chart));
            ApplyPointNumbersToChart(chart);
        }

        private void AddPhasePoints(Series series, List<PhaseAnalysis> report, Func<PhaseAnalysis, double> xSelector, Func<PhaseAnalysis, double> ySelector, ref double minX, ref double maxX, ref double minY, ref double maxY)
        {
            for (int i = 0; i < report.Count; i++)
            {
                double x = xSelector(report[i]);
                double y = ySelector(report[i]);
                int p = series.Points.AddXY(x, y);

                string epochLabel = report[i] == null ? string.Empty : Convert.ToString(report[i].Label);
                series.Points[p].AxisLabel = FormatNumber(x);
                series.Points[p].Tag = string.IsNullOrWhiteSpace(epochLabel)
                    ? i.ToString(CultureInfo.InvariantCulture)
                    : epochLabel;
                series.Points[p].Label = string.Empty;
                series.Points[p].ToolTip = series.Name
                    + "\nЦикл: " + Convert.ToString(series.Points[p].Tag)
                    + "\nμ: " + FormatNumber(x)
                    + "\nα: " + FormatNumber(y);
                AddBounds(x, y, ref minX, ref maxX, ref minY, ref maxY);
            }
        }

        private Series CreateLineSeries(string name, bool spline, bool showLabels, Color color)
        {
            Series s = new Series(MakeUniqueSeriesName(null, name))
            {
                ChartType = spline ? SeriesChartType.Spline : SeriesChartType.Line,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6,
                IsValueShownAsLabel = showLabels,
                Color = color,
                BorderColor = color,
                MarkerColor = color
            };
            s.SmartLabelStyle.Enabled = false;
            s.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No;
            s.LabelAngle = 0;
            s["SplineTension"] = "0.12";
            return s;
        }

        private void AddPhaseForecastConnected(Chart chart, string baseSeriesName, List<double> xValues, List<double> yValues, string forecastSeriesName, Color fallbackColor, bool showTextLabel, ref double minX, ref double maxX, ref double minY, ref double maxY)
        {
            if (chart == null || xValues == null || yValues == null || xValues.Count == 0 || yValues.Count == 0)
                return;

            double fx = ForecastBySmoothing(xValues, ReadA());
            double fy = ForecastBySmoothing(yValues, ReadA());
            AddConnectedForecast(chart, baseSeriesName, forecastSeriesName, fx, fy, showTextLabel ? "Прогноз" : string.Empty, true, fallbackColor);
            AddBounds(fx, fy, ref minX, ref maxX, ref minY, ref maxY);
        }

        private void AddConnectedForecast(Chart chart, string baseSeriesName, string forecastSeriesName, double x, double y, string label, bool isVisibleInLegend, Color fallbackColor)
        {
            Series baseSeries = chart.Series.FindByName(baseSeriesName);
            if (baseSeries == null || baseSeries.Points.Count == 0)
                return;

            DataPoint lastPoint = baseSeries.Points[baseSeries.Points.Count - 1];
            Color color = baseSeries.Color.IsEmpty ? fallbackColor : baseSeries.Color;
            string safeForecastName = MakeUniqueSeriesName(chart, forecastSeriesName);
            Series connection = new Series(MakeUniqueSeriesName(chart, "Соединение " + forecastSeriesName))
            {
                ChartType = SeriesChartType.Spline,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dot,
                IsVisibleInLegend = false,
                Color = color,
                BorderColor = color
            };
            connection["SplineTension"] = "0.12";
            double lastX = lastPoint.XValue;
            double lastY = lastPoint.YValues[0];
            connection.Points.AddXY(lastX, lastY);
            connection.Points.AddXY((lastX + x) / 2.0, (lastY + y) / 2.0);
            connection.Points.AddXY(x, y);
            chart.Series.Add(connection);

            bool hasPointLabel = !string.IsNullOrWhiteSpace(label);
            Series forecast = new Series(safeForecastName)
            {
                ChartType = SeriesChartType.Point,
                MarkerStyle = MarkerStyle.Diamond,
                MarkerSize = 11,
                IsValueShownAsLabel = hasPointLabel,
                IsVisibleInLegend = isVisibleInLegend,
                Color = color,
                BorderColor = color,
                MarkerColor = color
            };
            forecast.SmartLabelStyle.Enabled = false;
            forecast.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No;
            forecast.LabelAngle = 0;
            int p = forecast.Points.AddXY(x, y);
            forecast.Points[p].AxisLabel = FormatNumber(x);
            forecast.Points[p].Tag = "П";
            forecast.Points[p].Label = string.Empty;
            forecast.Points[p].ToolTip = forecast.Name + "\nПрогнозная точка\nμ: " + FormatNumber(x) + "\nα: " + FormatNumber(y);
            chart.Series.Add(forecast);
        }

        private void AddPhaseTimeSeriesByOptions(Chart chart, List<string> labels, List<double> mainValues, List<double> plusValues, List<double> minusValues, string name, ChartOptionControls options)
        {
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            int visibleSeriesCount = CountVisibleBaseSeries(options);
            int labelStep = GetSparseValueLabelStep(labels == null ? 0 : labels.Count, visibleSeriesCount);

            if (options.ShowMain)
            {
                Series s = AddOriginalTimeSeries(chart, labels, mainValues, name, false, Color.RoyalBlue, labelStep);
                AddBoundsY(mainValues, ref minY, ref maxY);
                if (options.ShowForecast)
                    AddForecastPointToSeries(s, labels, mainValues, ReadA(), false, ref minY, ref maxY);
            }
            if (options.ShowPlus)
            {
                Series p = AddOriginalTimeSeries(chart, labels, plusValues, name + "+E", false, Color.Orange, labelStep);
                p.BorderDashStyle = ChartDashStyle.Dash;
                AddBoundsY(plusValues, ref minY, ref maxY);
                if (options.ShowForecast)
                    AddForecastPointToSeries(p, labels, plusValues, ReadA(), false, ref minY, ref maxY);
            }
            if (options.ShowMinus)
            {
                Series m = AddOriginalTimeSeries(chart, labels, minusValues, name + "-E", false, Color.OrangeRed, labelStep);
                m.BorderDashStyle = ChartDashStyle.Dash;
                AddBoundsY(minusValues, ref minY, ref maxY);
                if (options.ShowForecast)
                    AddForecastPointToSeries(m, labels, minusValues, ReadA(), false, ref minY, ref maxY);
            }
            ApplyYScale(chart.ChartAreas[0], minY, maxY);
        }

        private int CountVisibleBaseSeries(ChartOptionControls options)
        {
            if (options == null)
                return 0;
            int count = 0;
            if (options.ShowMain) count++;
            if (options.ShowPlus) count++;
            if (options.ShowMinus) count++;
            return count;
        }

        private int GetSparseValueLabelStep(int pointCount, int visibleSeriesCount)
        {
            if (pointCount <= 0)
                return 0;

            int targetLabelsPerSeries;
            if (visibleSeriesCount <= 1)
                targetLabelsPerSeries = pointCount <= 25 ? pointCount : 18;
            else if (visibleSeriesCount == 2)
                targetLabelsPerSeries = 8;
            else
                targetLabelsPerSeries = 6;

            return Math.Max(1, (int)Math.Ceiling(pointCount / (double)Math.Max(1, targetLabelsPerSeries)));
        }

        private bool ShouldShowSparseValueLabel(int index, int pointCount, int step)
        {
            if (step <= 0 || pointCount <= 0)
                return false;
            return index % step == 0 || index == pointCount - 1;
        }
        private class PhaseAxisLabelRow
        {
            public string Name { get; private set; }
            public List<double> Values { get; private set; }

            public PhaseAxisLabelRow(string name, List<double> values)
            {
                Name = name;
                Values = values ?? new List<double>();
            }
        }

        private Series AddOriginalTimeSeries(Chart chart, List<string> labels, List<double> values, string name, bool showLabels, Color color)
        {
            return AddOriginalTimeSeries(chart, labels, values, name, showLabels, color, 1);
        }

        private Series AddOriginalTimeSeries(Chart chart, List<string> labels, List<double> values, string name, bool showLabels, Color color, int labelStep)
        {
            if (labels == null)
                labels = new List<string>();
            if (values == null)
                values = new List<double>();

            Series s = new Series(MakeUniqueSeriesName(chart, name))
            {
                ChartType = SeriesChartType.Spline,
                BorderWidth = 2,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 6,
                IsValueShownAsLabel = showLabels
            };
            if (!color.IsEmpty)
            {
                s.Color = color;
                s.BorderColor = color;
                s.MarkerColor = color;
            }
            s.SmartLabelStyle.Enabled = true;
            s.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            s.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom | LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
            s.LabelAngle = -45;
            s["SplineTension"] = "0.12";
            for (int i = 0; i < values.Count; i++)
            {
                int p = s.Points.AddXY(i, values[i]);
                string epochLabel = i < labels.Count ? labels[i] : i.ToString(CultureInfo.InvariantCulture);
                s.Points[p].AxisLabel = epochLabel;
                s.Points[p].Tag = epochLabel;
                s.Points[p].ToolTip = s.Name + "\nЦикл: " + epochLabel + "\nЗначение: " + FormatNumber(values[i]);
                if (showLabels && ShouldShowSparseValueLabel(i, values.Count, labelStep))
                    s.Points[p].Label = FormatNumber(values[i]);
                else
                    s.Points[p].Label = string.Empty;
            }
            chart.Series.Add(s);
            return s;
        }

        private string MakeUniqueSeriesName(Chart chart, string name)
        {
            if (chart == null || !chart.Series.Any(series => series.Name == name))
                return name;
            int index = 2;
            while (chart.Series.Any(series => series.Name == name + " " + index.ToString(CultureInfo.InvariantCulture)))
                index++;
            return name + " " + index.ToString(CultureInfo.InvariantCulture);
        }

        private void DrawSmoothingComparison(Chart chart, List<string> labels, List<double> values, string valueName, ChartOptionControls options)
        {
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            if (options.ShowMain)
            {
                AddOriginalTimeSeries(chart, labels, values, "Исходная " + valueName, false, Color.RoyalBlue);
                AddBoundsY(values, ref minY, ref maxY);
            }
            double[] alphas = new double[] { 0.1, 0.4, 0.7, 0.9 };
            Color[] colors = new Color[] { Color.Red, Color.Gold, Color.Orange, Color.Green };
            for (int i = 0; i < alphas.Length; i++)
            {
                double alpha = alphas[i];
                List<double> smoothed = ExponentialSmooth(values, alpha);
                string seriesName = "Сглаж. α=" + alpha.ToString("0.0", CultureInfo.InvariantCulture);
                Series s = AddOriginalTimeSeries(chart, labels, smoothed, seriesName, false, colors[i]);
                AddBoundsY(smoothed, ref minY, ref maxY);
                if (options.ShowForecast)
                    AddForecastPointToSeries(s, labels, values, alpha, false, ref minY, ref maxY);
            }
            ApplyYScale(chart.ChartAreas[0], minY, maxY);
        }

        private void AddForecastPointToSeries(Series s, List<string> labels, List<double> values, double alpha, bool verboseLabel, ref double minY, ref double maxY)
        {
            if (s == null || values == null || values.Count == 0)
                return;
            double f = ForecastBySmoothing(values, alpha);
            int p = s.Points.AddXY(values.Count, f);
            s.Points[p].AxisLabel = "Прогноз";
            s.Points[p].Tag = "П";
            s.Points[p].ToolTip = s.Name + "\nПрогноз: " + FormatNumber(f);
            s.Points[p].Label = verboseLabel ? "Прогноз: " + FormatNumber(f) : string.Empty;
            s.Points[p].MarkerStyle = MarkerStyle.Diamond;
            s.Points[p].MarkerSize = 9;
            if (!s.Color.IsEmpty)
            {
                s.Points[p].Color = s.Color;
                s.Points[p].MarkerColor = s.Color;
                s.Points[p].BorderColor = s.Color;
            }
            AddBoundsY(f, ref minY, ref maxY);
        }

        private void DrawLevel3SelectedChart()
        {
            if (chartLevel3Links == null)
                return;

            SetChartOptionsEnabled(chartLevel3Links, true);
            RecalculateLevel3ResponseReportFromSelectedCluster();

            string mode = comboLevel3ChartMode == null || comboLevel3ChartMode.SelectedItem == null
                ? "Отклик: фазовая α(μ)"
                : comboLevel3ChartMode.SelectedItem.ToString();

            string phaseMode = mode.Replace("Отклик: ", string.Empty);
            if (phaseMode == "фазовая α(μ)") phaseMode = "Фазовая траектория α(μ)";
            if (phaseMode == "μ(t)") phaseMode = "Функция μ(t)";
            if (phaseMode == "α(t)") phaseMode = "Функция α(t)";
            if (phaseMode == "сглаживание μ(t)") phaseMode = "Сравнение сглаживания μ(t)";
            if (phaseMode == "сглаживание α(t)") phaseMode = "Сравнение сглаживания α(t)";

            string title = lastLevel3Title;
            if (comboLevel3Cluster != null && comboLevel3Cluster.SelectedItem != null)
                title += ": " + comboLevel3Cluster.SelectedItem.ToString();
            else
                title += ": функция отклика";

            DrawPhaseChartByMode(chartLevel3Links, lastLevel3ResponseReport, title, phaseMode);
        }

        private List<double> GetPhaseXAxisValuesFromVisibleChartSeries(Chart chart)
        {
            List<double> values = new List<double>();
            if (chart == null)
                return values;

            foreach (Series series in chart.Series)
            {
                if (series == null)
                    continue;

                if (series.Name.StartsWith("Соединение", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (DataPoint point in series.Points)
                {
                    double x = point.XValue;
                    if (!double.IsNaN(x) && !double.IsInfinity(x))
                        values.Add(x);
                }
            }

            return values;
        }

        private void ApplyPhaseXAxisValueLabels(Chart chart, IEnumerable<double> sourceXValues)
        {
            if (chart == null || chart.ChartAreas.Count == 0)
                return;

            ApplyPhaseXAxisValueLabelRows(chart, new List<PhaseAxisLabelRow>
            {
                new PhaseAxisLabelRow("μ", sourceXValues == null ? new List<double>() : sourceXValues.ToList())
            });
        }

        private void ApplyPhaseXAxisValueLabelRows(Chart chart, List<PhaseAxisLabelRow> labelRows)
        {
            if (chart == null || chart.ChartAreas.Count == 0)
                return;

            ChartArea area = chart.ChartAreas[0];
            area.AxisX.CustomLabels.Clear();
            area.AxisX.IsStartedFromZero = false;
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.LabelStyle.Format = string.Empty;
            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.Interval = 0;
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.MajorTickMark.Enabled = true;
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisY.LabelStyle.Format = "0.#####";

            if (labelRows == null || labelRows.Count == 0)
                return;

            List<double> allValues = labelRows
                .Where(row => row != null && row.Values != null)
                .SelectMany(row => row.Values)
                .ToList();
            AddPhaseXAxisLabelsForRow(area, allValues);
        }

        private void AddPhaseXAxisLabelsForRow(ChartArea area, IEnumerable<double> sourceXValues)
        {
            if (area == null || sourceXValues == null)
                return;

            List<double> sorted = sourceXValues
                .Where(value => !double.IsNaN(value) && !double.IsInfinity(value))
                .OrderBy(value => value)
                .ToList();

            if (sorted.Count == 0)
                return;

            List<double> visibleValues = new List<double>();
            foreach (double value in sorted)
            {
                if (visibleValues.Count == 0 || Math.Abs(value - visibleValues[visibleValues.Count - 1]) > 0.0000000001)
                    visibleValues.Add(value);
            }

            if (visibleValues.Count == 0)
                return;

            if (visibleValues.Count > 26)
            {
                area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7.0F, FontStyle.Regular);
                area.AxisX.LabelStyle.Angle = -90;
            }
            else if (visibleValues.Count > 16)
            {
                area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8.0F, FontStyle.Regular);
                area.AxisX.LabelStyle.Angle = -75;
            }
            else
            {
                area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9.0F, FontStyle.Regular);
                area.AxisX.LabelStyle.Angle = -45;
            }

            double axisMin = double.IsNaN(area.AxisX.Minimum) ? sorted.First() : area.AxisX.Minimum;
            double axisMax = double.IsNaN(area.AxisX.Maximum) ? sorted.Last() : area.AxisX.Maximum;
            double axisRange = Math.Abs(axisMax - axisMin);

            double labelWidth = axisRange > 0 ? axisRange * 0.006 : 0.001;
            if (visibleValues.Count > 1)
            {
                double minDiff = double.MaxValue;
                for (int i = 1; i < visibleValues.Count; i++)
                    minDiff = Math.Min(minDiff, Math.Abs(visibleValues[i] - visibleValues[i - 1]));
                if (minDiff != double.MaxValue && minDiff > 0)
                    labelWidth = Math.Max(labelWidth, minDiff * 0.45);
            }
            else
            {
                labelWidth = Math.Max(Math.Abs(visibleValues[0]) * 0.001, labelWidth);
            }

            if (labelWidth <= 0 || double.IsNaN(labelWidth) || double.IsInfinity(labelWidth))
                labelWidth = 0.001;

            area.AxisX.CustomLabels.Clear();
            foreach (double value in visibleValues)
            {
                string text = FormatNumber(value);
                area.AxisX.CustomLabels.Add(new CustomLabel(value - labelWidth, value + labelWidth, text, 0, LabelMarkStyle.None));
            }
        }

        private void DisablePhaseValueLabelsKeepAxisLabels(Chart chart)
        {
            if (chart == null)
                return;

            foreach (Series series in chart.Series)
            {
                series.IsValueShownAsLabel = false;
                series.Label = string.Empty;
                series.SmartLabelStyle.Enabled = false;
                series.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No;
                foreach (DataPoint point in series.Points)
                {
                    point.Label = string.Empty;
                }
            }
        }

        private void ApplyPointNumbersToChart(Chart chart)
        {
            if (chart == null)
                return;

            foreach (Series series in chart.Series)
            {
                if (series == null)
                    continue;

                if (series.Name.StartsWith("Соединение", StringComparison.OrdinalIgnoreCase))
                {
                    series.IsValueShownAsLabel = false;
                    foreach (DataPoint point in series.Points)
                        point.Label = string.Empty;
                    continue;
                }

                series.IsValueShownAsLabel = true;
                series.LabelForeColor = Color.Black;
                series.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                series.LabelAngle = 0;
                series.SmartLabelStyle.Enabled = true;
                series.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No;
                series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top | LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;

                for (int i = 0; i < series.Points.Count; i++)
                {
                    DataPoint point = series.Points[i];
                    if (point == null || double.IsNaN(point.XValue) || double.IsInfinity(point.XValue))
                    {
                        if (point != null)
                            point.Label = string.Empty;
                        continue;
                    }

                    string pointNumber = GetChartPointNumber(series, point, i);
                    point.Label = pointNumber;
                    point.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                    point.LabelForeColor = Color.Black;

                    string oldToolTip = point.ToolTip;
                    string prefix = IsForecastPoint(series, point)
                        ? "Прогнозная точка"
                        : "Цикл наблюдения: " + pointNumber;
                    point.ToolTip = string.IsNullOrWhiteSpace(oldToolTip)
                        ? prefix
                        : prefix + "\n" + oldToolTip;
                }
            }
        }

        private string GetChartPointNumber(Series series, DataPoint point, int index)
        {
            if (IsForecastPoint(series, point))
                return "П";

            string tag = point == null || point.Tag == null ? string.Empty : Convert.ToString(point.Tag);
            if (!string.IsNullOrWhiteSpace(tag))
                return tag;

            string axisLabel = point == null ? string.Empty : Convert.ToString(point.AxisLabel);
            if (!string.IsNullOrWhiteSpace(axisLabel) && axisLabel != "Прогноз")
                return axisLabel;

            return index.ToString(CultureInfo.InvariantCulture);
        }

        private bool IsForecastPoint(Series series, DataPoint point)
        {
            if (series != null && series.Name.StartsWith("Прогноз", StringComparison.OrdinalIgnoreCase))
                return true;
            if (point != null && string.Equals(point.AxisLabel, "Прогноз", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private void PrepareTimeArea(ChartArea area, string yTitle, IList<string> labels, bool showForecastLabel)
        {
            if (area == null)
                return;

            int count = labels == null ? 0 : labels.Count;
            area.AxisX.Title = "Цикл наблюдения";
            area.AxisY.Title = yTitle;
            area.AxisX.CustomLabels.Clear();
            area.AxisX.Interval = 1;
            area.AxisX.Minimum = -0.5;
            area.AxisX.Maximum = Math.Max(0.5, showForecastLabel ? count + 0.8 : count - 0.5);
            area.AxisX.LabelStyle.Format = string.Empty;
            area.AxisX.LabelStyle.Enabled = true;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.IsLabelAutoFit = false;
            area.AxisY.LabelStyle.Format = "0.#####";

            for (int i = 0; i < count; i++)
            {
                string label = string.IsNullOrWhiteSpace(labels[i]) ? i.ToString(CultureInfo.InvariantCulture) : labels[i];
                area.AxisX.CustomLabels.Add(new CustomLabel(i - 0.5, i + 0.5, label, 0, LabelMarkStyle.None));
            }

            if (showForecastLabel)
                area.AxisX.CustomLabels.Add(new CustomLabel(count - 0.5, count + 0.5, "Прогноз", 0, LabelMarkStyle.None));
        }

        private void ApplyAxisFromPoints(Chart chart)
        {
            if (chart == null || chart.ChartAreas.Count == 0)
                return;
            ChartArea area = chart.ChartAreas[0];
            area.RecalculateAxesScale();
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.IsLabelAutoFit = false;
        }

        private void AddBounds(double x, double y, ref double minX, ref double maxX, ref double minY, ref double maxY)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                return;
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        private void AddBoundsY(IEnumerable<double> values, ref double minY, ref double maxY)
        {
            if (values == null)
                return;
            foreach (double value in values)
                AddBoundsY(value, ref minY, ref maxY);
        }

        private void AddBoundsY(double y, ref double minY, ref double maxY)
        {
            if (double.IsNaN(y) || double.IsInfinity(y))
                return;
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        private void ApplyYScale(ChartArea area, double minY, double maxY)
        {
            if (area == null)
                return;
            if (minY == double.MaxValue || maxY == double.MinValue)
            {
                area.AxisY.Minimum = double.NaN;
                area.AxisY.Maximum = double.NaN;
                return;
            }
            if (minY < maxY)
            {
                double dy = (maxY - minY) * 0.10;
                if (dy == 0) dy = Math.Max(Math.Abs(minY) * 0.001, 0.001);
                area.AxisY.Minimum = minY - dy;
                area.AxisY.Maximum = maxY + dy;
            }
            else
            {
                double dy = Math.Max(Math.Abs(minY) * 0.001, 0.001);
                area.AxisY.Minimum = minY - dy;
                area.AxisY.Maximum = maxY + dy;
            }
        }

        private void ApplyXYScale(ChartArea area, double minX, double maxX, double minY, double maxY)
        {
            if (area == null)
                return;
            if (minX != double.MaxValue && maxX != double.MinValue)
            {
                double dx = maxX - minX;
                if (Math.Abs(dx) < 0.000001)
                    dx = Math.Max(Math.Abs(minX) * 0.001, 0.001);
                area.AxisX.Minimum = minX - dx * 0.10;
                area.AxisX.Maximum = maxX + dx * 0.10;
            }
            ApplyYScale(area, minY, maxY);
        }

        private void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
        }

        private void StatusCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string colName = grid.Columns[e.ColumnIndex].Name.ToLowerInvariant();
            if (!colName.Contains("status") && !colName.Contains("статус"))
                return;

            string status = Convert.ToString(e.Value).ToLowerInvariant();
            if (status.Contains("без") || status.Contains("не измен"))
            {
                e.CellStyle.BackColor = Color.FromArgb(198, 239, 206);
                e.CellStyle.ForeColor = Color.DarkGreen;
            }
            else if (status.Contains("авар"))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 199, 206);
                e.CellStyle.ForeColor = Color.DarkRed;
            }
        }
    }
}
