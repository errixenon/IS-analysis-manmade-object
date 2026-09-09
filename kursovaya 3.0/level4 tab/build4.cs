using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        private List<string> GetSelectedLevel4Points()
        {
            List<string> result = new List<string>();
            if (checkedLevel4Points != null)
            {
                foreach (object item in checkedLevel4Points.CheckedItems)
                    result.Add(Convert.ToString(item));
            }
            if (result.Count == 0 && comboLevel4Point != null && comboLevel4Point.SelectedItem != null)
                result.Add(comboLevel4Point.SelectedItem.ToString());
            return result.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        }

        private void ResetLevel4Selection()
        {
            if (checkedLevel4Points == null)
                return;
            for (int i = 0; i < checkedLevel4Points.Items.Count; i++)
                checkedLevel4Points.SetItemChecked(i, false);
            lastLevel4Rows = new List<PointStateRow>();
            lastLevel4SelectedPoints.Clear();
            lastLevel4Point = string.Empty;
            if (gridLevel4 != null)
                gridLevel4.DataSource = null;
            if (chartLevel4 != null)
                PrepareChart(chartLevel4, "Z(t) выбранных контрольных точек", "Цикл наблюдения (t)", "Z");
            if (labelLevel4Status != null)
            {
                labelLevel4Status.Text = "Состояние выбранных точек: -";
                labelLevel4Status.ForeColor = Color.Black;
            }
        }

        private void UpdateLevel4PointSelector(bool keepSelection)
        {
            List<string> points = GetLevel4SelectablePoints();
            HashSet<string> selected = new HashSet<string>();
            if (keepSelection)
            {
                selected = new HashSet<string>(GetSelectedLevel4Points());
                if (selected.Count == 0 && lastLevel4SelectedPoints.Count > 0)
                    selected = new HashSet<string>(lastLevel4SelectedPoints);
            }

            if (comboLevel4Point != null)
            {
                comboLevel4Point.Items.Clear();
                foreach (string point in points)
                    comboLevel4Point.Items.Add(point);
                if (comboLevel4Point.Items.Count > 0)
                    comboLevel4Point.SelectedIndex = 0;
            }

            if (checkedLevel4Points != null)
            {
                checkedLevel4Points.Items.Clear();
                foreach (string point in points)
                    checkedLevel4Points.Items.Add(point, selected.Contains(point));
            }

            if (btnSelectAllLevel4 != null)
                btnSelectAllLevel4.Enabled = points.Count > 0;
            if (btnResetLevel4Selection != null)
                btnResetLevel4Selection.Enabled = points.Count > 0;

            if (labelLevel4Status != null && (lastLevel4Rows == null || lastLevel4Rows.Count == 0))
            {
                string filterText = radioLevel4OutsideClusters != null && radioLevel4OutsideClusters.Checked
                    ? "точек вне кластеров III уровня"
                    : "контрольных точек";
                labelLevel4Status.Text = "Доступно " + filterText + ": " + points.Count.ToString(CultureInfo.InvariantCulture);
                labelLevel4Status.ForeColor = Color.Black;
            }
        }

        private List<string> GetLevel4SelectablePoints()
        {
            List<string> allPoints = GetPointColumns().Select(c => c.ColumnName).ToList();
            if (radioLevel4OutsideClusters == null || !radioLevel4OutsideClusters.Checked)
                return allPoints;

            HashSet<string> clustered = new HashSet<string>();
            if (lastLevel3Clusters != null)
            {
                foreach (ClusterInfo cluster in lastLevel3Clusters.Where(c => c.Quantity >= 2))
                {
                    foreach (string point in SplitPointNames(cluster.Points))
                        clustered.Add(point);
                }
            }

            return allPoints.Where(point => !clustered.Contains(point)).ToList();
        }

        private void SelectAllVisibleLevel4Points()
        {
            if (checkedLevel4Points == null)
                return;
            for (int i = 0; i < checkedLevel4Points.Items.Count; i++)
                checkedLevel4Points.SetItemChecked(i, true);
        }
    }
}
