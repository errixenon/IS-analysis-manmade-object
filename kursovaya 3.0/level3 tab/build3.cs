using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        public class ClusterInfo
        {
            public int Cluster { get; set; }
            public string Points { get; set; }
            public int Quantity { get; set; } = 0;
        }

        private void LoadSelectedLevel3Block()
        {
            level3PreparedBlockPoints.Clear();
            level3PreparedBlockName = string.Empty;
            level3BlockPrepared = false;
            level3SubblocksConfirmed = false;
            level3SubblockMap.Clear();
            lastLevel3Links = new List<LinkInfo>();
            lastLevel3Clusters = new List<ClusterInfo>();
            lastLevel3ResponseReport = new List<PhaseAnalysis>();

            if (gridLevel3Differences != null)
                gridLevel3Differences.DataSource = null;
            if (gridLevel3Links != null)
                gridLevel3Links.DataSource = null;
            if (gridLevel3Clusters != null)
                gridLevel3Clusters.DataSource = null;

            if (comboLevel3Subblock != null)
            {
                comboLevel3Subblock.Items.Clear();
                comboLevel3Subblock.Enabled = false;
            }
            if (comboLevel3Cluster != null)
            {
                comboLevel3Cluster.Items.Clear();
                comboLevel3Cluster.Enabled = false;
            }
            if (listLevel3FreeSubblockPoints != null)
            {
                listLevel3FreeSubblockPoints.Items.Clear();
                listLevel3FreeSubblockPoints.Enabled = false;
            }
            if (listLevel3SelectedSubblockPoints != null)
            {
                listLevel3SelectedSubblockPoints.Items.Clear();
                listLevel3SelectedSubblockPoints.Enabled = false;
            }

            if (numericLevel3Subblocks != null)
                numericLevel3Subblocks.Enabled = false;
            if (numericLevel3PointsPerSubblock != null)
                numericLevel3PointsPerSubblock.Enabled = false;
            if (btnCreateLevel3Subblocks != null)
                btnCreateLevel3Subblocks.Enabled = false;
            if (btnMoveLevel3Point != null)
                btnMoveLevel3Point.Enabled = false;
            if (btnReturnLevel3Point != null)
                btnReturnLevel3Point.Enabled = false;
            if (btnAutoLevel3Subblocks != null)
                btnAutoLevel3Subblocks.Enabled = false;
            if (btnConfirmLevel3Subblocks != null)
                btnConfirmLevel3Subblocks.Enabled = false;
            if (btnShowLevel3Links != null)
                btnShowLevel3Links.Enabled = false;
            if (btnCalcLevel3Response != null)
                btnCalcLevel3Response.Enabled = false;
            if (tabPageLevel3Calculation != null)
                tabPageLevel3Calculation.Enabled = false;

            if (comboLevel3Block == null || comboLevel3Block.SelectedItem == null)
            {
                if (btnCalcLevel3 != null)
                    btnCalcLevel3.Enabled = false;
                RefreshLevel3BlockPreparationLists();
                return;
            }

            level3PreparedBlockName = comboLevel3Block.SelectedItem.ToString();
            if (blockMap.ContainsKey(level3PreparedBlockName))
                level3PreparedBlockPoints = new List<string>(blockMap[level3PreparedBlockName]);

            RefreshLevel3BlockPreparationLists();
            if (btnCalcLevel3 != null)
                btnCalcLevel3.Enabled = blocksConfirmed && level3PreparedBlockPoints.Count >= 2;

            if (labelLevel3Status != null)
            {
                labelLevel3Status.Text =
                    "Блок " + level3PreparedBlockName + ": " +
                    level3PreparedBlockPoints.Count.ToString(CultureInfo.InvariantCulture) +
                    " контрольных точек. При необходимости добавьте нераспределённые точки или удалите лишние.";
            }
        }

        private void RefreshLevel3BlockPreparationLists()
        {
            if (listLevel3BlockPoints == null || listLevel3AvailableBlockPoints == null)
                return;

            listLevel3BlockPoints.Items.Clear();
            foreach (string point in level3PreparedBlockPoints)
                listLevel3BlockPoints.Items.Add(point);

            listLevel3AvailableBlockPoints.Items.Clear();
            List<string> allPoints = GetPointColumns().Select(c => c.ColumnName).ToList();
            HashSet<string> assignedToOtherBlocks = new HashSet<string>(
                blockMap
                    .Where(pair => pair.Key != level3PreparedBlockName)
                    .SelectMany(pair => pair.Value));

            foreach (string point in allPoints)
            {
                if (!level3PreparedBlockPoints.Contains(point) &&
                    !assignedToOtherBlocks.Contains(point))
                {
                    listLevel3AvailableBlockPoints.Items.Add(point);
                }
            }
        }

        private void InvalidateLevel3PreparedCalculation()
        {
            level3BlockPrepared = false;
            level3SubblocksConfirmed = false;
            level3SubblockMap.Clear();
            lastLevel3Links = new List<LinkInfo>();
            lastLevel3Clusters = new List<ClusterInfo>();
            lastLevel3ResponseReport = new List<PhaseAnalysis>();

            if (gridLevel3Differences != null)
                gridLevel3Differences.DataSource = null;
            if (gridLevel3Links != null)
                gridLevel3Links.DataSource = null;
            if (gridLevel3Clusters != null)
                gridLevel3Clusters.DataSource = null;

            if (comboLevel3Subblock != null)
            {
                comboLevel3Subblock.Items.Clear();
                comboLevel3Subblock.Enabled = false;
            }
            if (comboLevel3Cluster != null)
            {
                comboLevel3Cluster.Items.Clear();
                comboLevel3Cluster.Enabled = false;
            }

            if (numericLevel3Subblocks != null)
                numericLevel3Subblocks.Enabled = false;
            if (numericLevel3PointsPerSubblock != null)
                numericLevel3PointsPerSubblock.Enabled = false;
            if (btnCreateLevel3Subblocks != null)
                btnCreateLevel3Subblocks.Enabled = false;
            if (btnMoveLevel3Point != null)
                btnMoveLevel3Point.Enabled = false;
            if (btnReturnLevel3Point != null)
                btnReturnLevel3Point.Enabled = false;
            if (btnAutoLevel3Subblocks != null)
                btnAutoLevel3Subblocks.Enabled = false;
            if (btnConfirmLevel3Subblocks != null)
                btnConfirmLevel3Subblocks.Enabled = false;
            if (btnShowLevel3Links != null)
                btnShowLevel3Links.Enabled = false;
            if (btnCalcLevel3Response != null)
                btnCalcLevel3Response.Enabled = false;
            if (tabPageLevel3Calculation != null)
                tabPageLevel3Calculation.Enabled = false;

            if (btnCalcLevel3 != null)
                btnCalcLevel3.Enabled = blocksConfirmed && level3PreparedBlockPoints.Count >= 2;

            if (labelLevel3SubblockInfo != null)
                labelLevel3SubblockInfo.Text = "Нужно: -";
        }

        private void PrepareLevel3SubblockControls()
        {
            int pointCount = level3PreparedBlockPoints.Count;
            int maxSubblocks = Math.Max(1, pointCount / 2);

            numericLevel3Subblocks.Maximum = maxSubblocks;
            numericLevel3Subblocks.Value = Math.Min(1, maxSubblocks);
            numericLevel3Subblocks.Enabled = true;

            numericLevel3PointsPerSubblock.Maximum = Math.Max(2, pointCount);
            int initialSubblocks = (int)numericLevel3Subblocks.Value;
            int initialPoints = Math.Max(2, pointCount / initialSubblocks);
            numericLevel3PointsPerSubblock.Value =
                Math.Min((int)numericLevel3PointsPerSubblock.Maximum, initialPoints);
            numericLevel3PointsPerSubblock.Enabled = true;

            btnCreateLevel3Subblocks.Enabled = true;
            UpdateLevel3SubblockInfo();
        }

        private void ClearLevel3SubblockDistribution()
        {
            level3SubblockMap.Clear();
            level3SubblocksConfirmed = false;
            lastLevel3Clusters = new List<ClusterInfo>();
            lastLevel3ResponseReport = new List<PhaseAnalysis>();

            if (comboLevel3Subblock != null)
            {
                comboLevel3Subblock.Items.Clear();
                comboLevel3Subblock.Enabled = false;
            }
            if (listLevel3FreeSubblockPoints != null)
            {
                listLevel3FreeSubblockPoints.Items.Clear();
                listLevel3FreeSubblockPoints.Enabled = false;
            }
            if (listLevel3SelectedSubblockPoints != null)
            {
                listLevel3SelectedSubblockPoints.Items.Clear();
                listLevel3SelectedSubblockPoints.Enabled = false;
            }
            if (btnMoveLevel3Point != null)
                btnMoveLevel3Point.Enabled = false;
            if (btnReturnLevel3Point != null)
                btnReturnLevel3Point.Enabled = false;
            if (btnAutoLevel3Subblocks != null)
                btnAutoLevel3Subblocks.Enabled = false;
            if (btnConfirmLevel3Subblocks != null)
                btnConfirmLevel3Subblocks.Enabled = false;

            if (comboLevel3Cluster != null)
            {
                comboLevel3Cluster.Items.Clear();
                comboLevel3Cluster.Enabled = false;
            }
            if (btnCalcLevel3Response != null)
                btnCalcLevel3Response.Enabled = false;
            if (gridLevel3Clusters != null)
                gridLevel3Clusters.DataSource = null;
            if (tabPageLevel3Calculation != null)
                tabPageLevel3Calculation.Enabled = false;
        }

        private void PopulateLevel3ClusterSelector(List<ClusterInfo> clusters)
        {
            if (comboLevel3Cluster == null)
                return;

            comboLevel3Cluster.Items.Clear();

            if (level3SubblocksConfirmed && level3SubblockMap.Count > 0)
            {
                foreach (string subblock in level3SubblockMap.Keys.OrderBy(ParseLevel3SubblockNumber))
                    comboLevel3Cluster.Items.Add(subblock);
            }
            else if (clusters != null && clusters.Count > 0)
            {
                foreach (ClusterInfo cluster in clusters.Where(c => c.Quantity >= 2))
                {
                    comboLevel3Cluster.Items.Add(
                        "Кластер " +
                        cluster.Cluster.ToString(CultureInfo.InvariantCulture) +
                        " (" + cluster.Points + ")");
                }
            }

            if (comboLevel3Cluster.Items.Count == 0 &&
                comboLevel3Block != null &&
                comboLevel3Block.SelectedItem != null)
            {
                comboLevel3Cluster.Items.Add("Весь блок");
            }

            if (comboLevel3Cluster.Items.Count > 0)
                comboLevel3Cluster.SelectedIndex = 0;
        }

        private void RecalculateLevel3ResponseReportFromSelectedCluster()
        {
            try
            {
                List<string> responsePoints = GetSelectedLevel3ResponsePoints();
                lastLevel3ResponseReport = responsePoints.Count == 0
                    ? new List<PhaseAnalysis>()
                    : CalculateResponsePhaseForColumns(ColumnsByNames(responsePoints), "Отклик");
            }
            catch
            {
                lastLevel3ResponseReport = new List<PhaseAnalysis>();
            }
        }

        private List<string> GetSelectedLevel3ResponsePoints()
        {
            if (level3SubblocksConfirmed &&
                comboLevel3Cluster != null &&
                comboLevel3Cluster.SelectedItem != null)
            {
                string selectedSubblock = comboLevel3Cluster.SelectedItem.ToString();
                if (level3SubblockMap.ContainsKey(selectedSubblock))
                    return new List<string>(level3SubblockMap[selectedSubblock]);
            }

            if (lastLevel3Clusters != null &&
                lastLevel3Clusters.Count > 0 &&
                comboLevel3Cluster != null &&
                comboLevel3Cluster.SelectedItem != null)
            {
                string selectedText = comboLevel3Cluster.SelectedItem.ToString();
                int clusterNumber;
                if (TryParseClusterNumber(selectedText, out clusterNumber))
                {
                    ClusterInfo selectedCluster =
                        lastLevel3Clusters.FirstOrDefault(c => c.Cluster == clusterNumber);
                    if (selectedCluster != null &&
                        selectedCluster.Quantity >= 2 &&
                        !string.IsNullOrWhiteSpace(selectedCluster.Points))
                    {
                        return SplitPointNames(selectedCluster.Points);
                    }
                }
            }

            if (comboLevel3Block != null &&
                comboLevel3Block.SelectedItem != null &&
                blockMap.ContainsKey(comboLevel3Block.SelectedItem.ToString()))
            {
                return SelectResponsePoints(
                    blockMap[comboLevel3Block.SelectedItem.ToString()],
                    lastLevel3Clusters);
            }

            return new List<string>();
        }

        private bool TryParseClusterNumber(string text, out int clusterNumber)
        {
            clusterNumber = 0;
            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("Кластер "))
                return false;

            string numberText = new string(text.Skip("Кластер ".Length).TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(numberText, out clusterNumber);
        }

        private List<string> SelectResponsePoints(List<string> points, List<ClusterInfo> clusters)
        {
            if (clusters != null && clusters.Count > 0)
            {
                ClusterInfo selected = clusters.Where(c => c.Quantity >= 2).OrderByDescending(c => c.Quantity).FirstOrDefault();
                if (selected != null && !string.IsNullOrWhiteSpace(selected.Points))
                    return SplitPointNames(selected.Points);
            }
            return points == null ? new List<string>() : points.ToList();
        }

        private void UpdateLevel3SubblockInfo()
        {
            if (labelLevel3SubblockInfo == null ||
                numericLevel3Subblocks == null ||
                numericLevel3PointsPerSubblock == null)
                return;

            int subblocks = (int)numericLevel3Subblocks.Value;
            int perSubblock = (int)numericLevel3PointsPerSubblock.Value;
            int required = subblocks * perSubblock;

            labelLevel3SubblockInfo.Text =
                "Нужно: " + required.ToString(CultureInfo.InvariantCulture) +
                " из " + level3PreparedBlockPoints.Count.ToString(CultureInfo.InvariantCulture);
        }

        private void RefreshLevel3SubblockLists()
        {
            if (listLevel3FreeSubblockPoints == null ||
                listLevel3SelectedSubblockPoints == null)
                return;

            listLevel3SelectedSubblockPoints.Items.Clear();
            if (comboLevel3Subblock != null &&
                comboLevel3Subblock.SelectedItem != null)
            {
                string selected = comboLevel3Subblock.SelectedItem.ToString();
                if (level3SubblockMap.ContainsKey(selected))
                {
                    foreach (string point in level3SubblockMap[selected])
                        listLevel3SelectedSubblockPoints.Items.Add(point);
                }
            }

            listLevel3FreeSubblockPoints.Items.Clear();
            HashSet<string> assigned = new HashSet<string>(
                level3SubblockMap.Values.SelectMany(points => points));

            foreach (string point in level3PreparedBlockPoints)
            {
                if (!assigned.Contains(point))
                    listLevel3FreeSubblockPoints.Items.Add(point);
            }
        }

        private int ParseLevel3SubblockNumber(string name)
        {
            string number = new string((name ?? string.Empty).Where(char.IsDigit).ToArray());
            int value;
            return int.TryParse(number, out value) ? value : int.MaxValue;
        }

        
    }
}
