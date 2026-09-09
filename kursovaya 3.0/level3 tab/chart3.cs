using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static Kursovaya30.Form1;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void ShowLevel3LinksChartWindow()
        {
            using (Form form = new Form())
            {
                form.Text = "III уровень: связи и отклонения";
                form.StartPosition = FormStartPosition.CenterParent;
                form.Size = new Size(1100, 700);
                form.MinimumSize = new Size(850, 520);
                form.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

                Chart independentChart = CreateChart("III уровень: отклонения связей", "Связь", "Максимальное отклонение");
                independentChart.Dock = DockStyle.Fill;
                DrawLinksChartToChart(independentChart, lastLevel3Links);
                form.Controls.Add(independentChart);
                form.ShowDialog(this);
            }
        }

        private void DrawLinksChartToChart(Chart targetChart, List<LinkInfo> links)
        {
            if (targetChart == null)
                return;
            if (links == null)
                links = new List<LinkInfo>();

            PrepareChart(targetChart, "III уровень: отклонения связей", "Связь", "Максимальное отклонение");
            ChartArea area = targetChart.ChartAreas[0];
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.Angle = -90;
            area.AxisX.ScrollBar.Enabled = true;
            area.AxisX.ScaleView.Zoomable = true;
            area.AxisX.ScaleView.Size = Math.Min(12, Math.Max(1, links.Count));
            area.CursorX.IsUserEnabled = true;
            area.CursorX.IsUserSelectionEnabled = true;
            area.CursorX.AutoScroll = true;
            area.AxisY.LabelStyle.Format = "0.####";

            Series s = new Series("Отклонение") { ChartType = SeriesChartType.Column, IsValueShownAsLabel = true };
            s["PointWidth"] = "0.8";
            List<LinkInfo> visible = links.Where(l => l.Статус == "Ослабленная").ToList();
            if (visible.Count == 0)
                visible = links.Take(20).ToList();

            int x = 1;
            foreach (LinkInfo link in visible)
            {
                int p = s.Points.AddXY(x, link.Максимальное_отклонение);
                s.Points[p].AxisLabel = link.Связь;
                s.Points[p].Label = x.ToString(CultureInfo.InvariantCulture);
                s.Points[p].ToolTip = "№ " + x.ToString(CultureInfo.InvariantCulture) + "\n" + link.Связь + "\nОтклонение: " + FormatNumber(link.Максимальное_отклонение);
                x++;
            }

            targetChart.Series.Add(s);
            ApplyPointNumbersToChart(targetChart);
        }
    }
}
