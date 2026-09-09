using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void ChangeDataSchemeZoom(float factor)
        {
            if (factor <= 0)
                return;
            dataSchemeZoom *= factor;
            if (dataSchemeZoom < 0.25F) dataSchemeZoom = 0.25F;
            if (dataSchemeZoom > 5.0F) dataSchemeZoom = 5.0F;
            ResizeDataSchemePicture();
        }

        private void ResetDataSchemeZoom()
        {
            dataSchemeZoom = 1.0F;
            ResizeDataSchemePicture();
        }

        private void ResizeDataSchemePicture()
        {
            if (pictureBox1 == null || panelDataSchemeScroll == null)
                return;

            Image img = pictureBox1.Image;
            if (img == null)
            {
                pictureBox1.Size = panelDataSchemeScroll.ClientSize;
                pictureBox1.Location = Point.Empty;
                if (labelDataSchemeZoom != null)
                    labelDataSchemeZoom.Text = "Масштаб: 100%";
                return;
            }

            int clientWidth = Math.Max(1, panelDataSchemeScroll.ClientSize.Width - 4);
            int clientHeight = Math.Max(1, panelDataSchemeScroll.ClientSize.Height - 4);
            double baseScale = Math.Min(clientWidth / (double)img.Width, clientHeight / (double)img.Height);
            if (double.IsNaN(baseScale) || double.IsInfinity(baseScale) || baseScale <= 0)
                baseScale = 1.0;

            int width = Math.Max(20, (int)Math.Round(img.Width * baseScale * dataSchemeZoom));
            int height = Math.Max(20, (int)Math.Round(img.Height * baseScale * dataSchemeZoom));
            pictureBox1.Size = new Size(width, height);
            pictureBox1.Location = new Point(width < clientWidth ? (clientWidth - width) / 2 : 0, height < clientHeight ? (clientHeight - height) / 2 : 0);

            if (labelDataSchemeZoom != null)
                labelDataSchemeZoom.Text = "Масштаб: " + Math.Round(dataSchemeZoom * 100).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private void SetInitialSplitterDistance(SplitContainer split, double ratio)
        {
            if (split == null)
                return;

            split.HandleCreated += delegate
            {
                BeginInvoke(new Action(delegate
                {
                    try
                    {
                        SafeSetSplitterDistance(split, ratio);
                    }
                    catch
                    { }
                }));
            };

            split.SizeChanged += delegate
            {
                try
                {
                    SafeSetSplitterDistance(split, ratio);
                }
                catch
                { }
            };
        }

        private void SafeSetSplitterDistance(SplitContainer split, double ratio)
        {
            if (split == null)
                return;

            if (ratio <= 0 || ratio >= 1)
                ratio = 0.5;

            int totalSize = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
            if (totalSize <= 0)
                return;

            int min1 = Math.Max(0, split.Panel1MinSize);
            int min2 = Math.Max(0, split.Panel2MinSize);
            int splitterSize = Math.Max(1, split.SplitterWidth);

            int maxDistance = totalSize - min2 - splitterSize;
            int minDistance = min1;

            if (maxDistance <= minDistance)
                return;

            int distance = (int)(totalSize * ratio);
            distance = Math.Max(minDistance, Math.Min(maxDistance, distance));

            if (distance >= minDistance && distance <= maxDistance)
            {
                try
                {
                    split.SplitterDistance = distance;
                }
                catch
                { }
            }
        }

        private List<DataColumn> GetPointColumns()
        {
            if (dTable == null)
                return new List<DataColumn>();

            return dTable.Columns.Cast<DataColumn>()
                .Skip(1)
                .Where(c => !IsServiceColumn(c.ColumnName))
                .Where(c => c.DataType == typeof(double) || LooksNumericColumn(c))
                .ToList();
        }

        private void RefreshBlockSelectors()
        {
            comboCalcBlock.Items.Clear();
            comboLevel3Block.Items.Clear();
            foreach (string block in blockMap.Keys)
            {
                comboCalcBlock.Items.Add(block);
                comboLevel3Block.Items.Add(block);
            }
            if (comboCalcBlock.Items.Count > 0) comboCalcBlock.SelectedIndex = 0;
            if (comboLevel3Block.Items.Count > 0) comboLevel3Block.SelectedIndex = 0;
        }

        private DataTable GetCalculationTable()
        {
            return dTable;
        }

        private double ReadA()
        {
            double value;
            if (!TryParseUserDouble(textBoxAlpha.Text, out value) || value <= 0 || value >= 1)
                throw new InvalidOperationException("Коэффициент A должен быть в интервале (0;1).");
            return value;
        }

        private double ReadEpsilon()
        {
            double value;
            if (!TryParseUserDouble(textBoxEpsilon.Text, out value) || value <= 0)
                throw new InvalidOperationException("Точность E должна быть больше 0.");
            return value;
        }

        private bool TryParseUserDouble(string text, out double value)
        {
            text = (text ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryReadDouble(object raw, out double value)
        {
            value = 0;
            if (raw == null || raw == DBNull.Value)
                return false;
            if (raw is double) { value = (double)raw; return true; }
            if (raw is float) { value = (float)raw; return true; }
            if (raw is decimal) { value = (double)(decimal)raw; return true; }
            if (raw is int) { value = (int)raw; return true; }
            if (raw is long) { value = (long)raw; return true; }
            string text = Convert.ToString(raw, CultureInfo.InvariantCulture).Replace(',', '.');
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private double ReadCellDouble(DataRow row, DataColumn column)
        {
            double value;
            return TryReadDouble(row[column], out value) ? value : 0.0;
        }

        private List<DataColumn> ColumnsByNames(List<string> names)
        {
            return names.Where(n => dTable.Columns.Contains(n)).Select(n => dTable.Columns[n]).ToList();
        }

        private void UpdateAllPointSelectors()
        {
            UpdateLevel4PointSelector(true);

            if (pictureBoxLevel2Scheme != null && pictureBox1.Image != null)
                SetPictureBoxImage(pictureBoxLevel2Scheme, pictureBox1.Image);

            RefreshBlockLists();
        }

        private List<string> SplitPointNames(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();
            return text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }

        private double SafeReadEpsilon(double fallback)
        {
            try
            {
                double value;
                return TryParseUserDouble(textBoxEpsilon.Text, out value) && value > 0 ? value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private void SetNumericUpDownValue(NumericUpDown numeric, double value)
        {
            if (numeric == null || double.IsNaN(value) || double.IsInfinity(value))
                return;
            decimal v = (decimal)value;
            if (v < numeric.Minimum) v = numeric.Minimum;
            if (v > numeric.Maximum) v = numeric.Maximum;
            numeric.Value = Math.Round(v, numeric.DecimalPlaces);
        }
    }
}
