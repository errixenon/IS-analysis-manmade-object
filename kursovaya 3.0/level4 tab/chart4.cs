using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void DrawPointChart(List<PointStateRow> rows, string point)
        {
            if (rows == null || rows.Count == 0)
            {
                PrepareChart(chartLevel4, "IV уровень", "Цикл наблюдения", "Z");
                return;
            }

            string mode = comboLevel4ChartMode == null || comboLevel4ChartMode.SelectedItem == null
                ? "Z(t) с границами"
                : comboLevel4ChartMode.SelectedItem.ToString();

            ChartOptionControls options = GetChartOptions(chartLevel4);
            List<IGrouping<string, PointStateRow>> groups = rows.GroupBy(r => r.Точка).ToList();
            PrepareChart(chartLevel4, mode + ": " + point, "Цикл наблюдения (t)", "Z");

            List<string> axisLabels = groups.Count == 0
                ? new List<string>()
                : groups[0].OrderBy(r => ParseEpochOrder(r.Эпоха)).Select(r => r.Эпоха).ToList();
            PrepareTimeArea(chartLevel4.ChartAreas[0], "Z", axisLabels, options.ShowForecast);

            double minY = double.MaxValue;
            double maxY = double.MinValue;
            bool showMainLabels = false;

            if (mode == "Сглаживание Z(t)")
            {
                foreach (var group in groups)
                {
                    List<PointStateRow> pointRows = group.OrderBy(r => ParseEpochOrder(r.Эпоха)).ToList();
                    List<string> labels = pointRows.Select(r => r.Эпоха).ToList();
                    List<double> z = pointRows.Select(r => r.Z).ToList();
                    if (options.ShowMain)
                    {
                        Series original = AddOriginalTimeSeries(chartLevel4, labels, z, "Z " + group.Key, showMainLabels, Color.RoyalBlue);
                        AddBoundsY(z, ref minY, ref maxY);
                        if (options.ShowForecast)
                            AddForecastPointToSeries(original, labels, z, ReadA(), groups.Count == 1, ref minY, ref maxY);
                    }

                    double[] alphas = new double[] { 0.1, 0.4, 0.7, 0.9 };
                    Color[] colors = new Color[] { Color.Red, Color.Gold, Color.Orange, Color.Green };
                    for (int i = 0; i < alphas.Length; i++)
                    {
                        double alpha = alphas[i];
                        List<double> smoothed = ExponentialSmooth(z, alpha);
                        Series smooth = AddOriginalTimeSeries(chartLevel4, labels, smoothed, "Сглаж. α=" + alpha.ToString("0.0", CultureInfo.InvariantCulture) + " " + group.Key, false, colors[i]);
                        AddBoundsY(smoothed, ref minY, ref maxY);
                        if (options.ShowForecast)
                            AddForecastPointToSeries(smooth, labels, z, alpha, false, ref minY, ref maxY);
                    }
                }
            }
            else
            {
                foreach (var group in groups)
                {
                    List<PointStateRow> pointRows = group.OrderBy(r => ParseEpochOrder(r.Эпоха)).ToList();
                    List<string> labels = pointRows.Select(r => r.Эпоха).ToList();
                    List<double> z = pointRows.Select(r => r.Z).ToList();
                    if (options.ShowMain)
                    {
                        Series baseSeries = AddOriginalTimeSeries(chartLevel4, labels, z, group.Key, showMainLabels, Color.RoyalBlue);
                        AddBoundsY(z, ref minY, ref maxY);
                        if (options.ShowForecast)
                            AddForecastPointToSeries(baseSeries, labels, z, ReadA(), groups.Count == 1, ref minY, ref maxY);
                    }
                    if (options.ShowPlus)
                    {
                        List<double> plusValues = pointRows.Select(r => r.ZPlusE).ToList();
                        Series plus = AddOriginalTimeSeries(chartLevel4, labels, plusValues, group.Key + " +E", false, Color.Orange);
                        plus.BorderDashStyle = ChartDashStyle.Dash;
                        AddBoundsY(plusValues, ref minY, ref maxY);
                        if (options.ShowForecast)
                            AddForecastPointToSeries(plus, labels, plusValues, ReadA(), false, ref minY, ref maxY);
                    }
                    if (options.ShowMinus)
                    {
                        List<double> minusValues = pointRows.Select(r => r.ZMinusE).ToList();
                        Series minus = AddOriginalTimeSeries(chartLevel4, labels, minusValues, group.Key + " -E", false, Color.OrangeRed);
                        minus.BorderDashStyle = ChartDashStyle.Dash;
                        AddBoundsY(minusValues, ref minY, ref maxY);
                        if (options.ShowForecast)
                            AddForecastPointToSeries(minus, labels, minusValues, ReadA(), false, ref minY, ref maxY);
                    }
                }
            }

            ApplyYScale(chartLevel4.ChartAreas[0], minY, maxY);
            ApplyAxisFromPoints(chartLevel4);
            ApplyPointNumbersToChart(chartLevel4);
        }

        private int ParseEpochOrder(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : 0;
        }
    }
}
