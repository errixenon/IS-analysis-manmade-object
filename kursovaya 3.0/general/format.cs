using System;
using System.Collections.Generic;
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
        private void StyleAllButtons(Control root)
        {
            foreach (Control c in root.Controls)
            {
                Button b = c as Button;
                if (b != null)
                {
                    b.AutoSize = false;
                    b.Height = 34;
                    if (b.Text == ">" || b.Text == "<" || b.Text == "+" || b.Text == "−")
                        b.Width = 54;
                    else if (b.Text == "100%")
                        b.Width = 76;
                    else
                        b.Width = Math.Max(150, b.Width);
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = Color.FromArgb(120, 140, 160);
                    b.BackColor = Color.White;
                    b.ForeColor = Color.FromArgb(35, 45, 55);
                    b.Margin = new Padding(4, 3, 4, 3);
                }
                if (c.HasChildren)
                    StyleAllButtons(c);
            }
        }

        private void ApplyGridNumberFormat(DataGridView grid)
        {
            if (grid.Columns == null)
                return;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.ValueType == typeof(double) || col.ValueType == typeof(float) || col.ValueType == typeof(decimal))
                    col.DefaultCellStyle.Format = "0.####";
            }
        }

        private void ShowError(string text)
        {
            MessageBox.Show(text, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return string.Empty;
            if (Math.Abs(value) < 0.0000000001)
                return "0";
            if (Math.Abs(value - Math.Round(value)) < 0.0000000001)
                return Math.Round(value).ToString(CultureInfo.InvariantCulture);
            double abs = Math.Abs(value);
            if (abs >= 1000)
                return value.ToString("0.###", CultureInfo.InvariantCulture);
            if (abs >= 1)
                return value.ToString("0.#####", CultureInfo.InvariantCulture);
            return value.ToString("0.##########", CultureInfo.InvariantCulture);
        }
    }
}
