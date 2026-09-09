using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void ShowTable(string SQLQuery)
        {
            dTable = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(SQLQuery, SQLiteConn);
            adapter.Fill(dTable);
            NormalizeDataTableTypes();
            dataGridView1.DataSource = dTable;
            savedTable = dTable.Copy();
            ApplyGridNumberFormat(dataGridView1);
            btnSaveTable.Enabled = true;
            UpdateAllPointSelectors();
        }

        private string SQL_AllTable()
        {
            return "SELECT * FROM [" + comboBox1.SelectedItem + "] ORDER BY 1";
        }

        private void NormalizeDataTableTypes()
        {
            DataTable result = new DataTable();

            foreach (DataColumn col in dTable.Columns)
            {
                if (IsServiceColumn(col.ColumnName))
                    result.Columns.Add(col.ColumnName, typeof(string));
                else if (LooksNumericColumn(col))
                    result.Columns.Add(col.ColumnName, typeof(double));
                else
                    result.Columns.Add(col.ColumnName, typeof(string));
            }

            foreach (DataRow sourceRow in dTable.Rows)
            {
                DataRow newRow = result.NewRow();
                for (int i = 0; i < dTable.Columns.Count; i++)
                {
                    if (result.Columns[i].DataType == typeof(double))
                    {
                        double value;
                        newRow[i] = TryReadDouble(sourceRow[i], out value) ? value : 0.0;
                    }
                    else
                    {
                        newRow[i] = sourceRow[i] == DBNull.Value ? string.Empty : sourceRow[i].ToString();
                    }
                }
                result.Rows.Add(newRow);
            }

            dTable = result;
        }

        private bool LooksNumericColumn(DataColumn col)
        {
            string name = col.ColumnName.ToLowerInvariant();
            if (name.Contains("photo") || name.Contains("image") || name.Contains("path"))
                return false;

            foreach (DataRow row in dTable.Rows)
            {
                double value;
                if (TryReadDouble(row[col], out value))
                    return true;
            }

            return false;
        }

        private bool IsServiceColumn(string name)
        {
            string n = name.ToLowerInvariant();
            return n == "id" || n.Contains("photo") || n.Contains("image") || n.Contains("path") || n.Contains("схема");
        }

        private void GenerateNewEpoch()
        {
            DataRow newRow = dTable.NewRow();
            List<DataColumn> pointColumns = GetPointColumns();

            for (int i = 0; i < dTable.Columns.Count; i++)
            {
                DataColumn col = dTable.Columns[i];
                if (i == 0)
                {
                    newRow[col] = GetNextEpochLabel();
                }
                else if (pointColumns.Contains(col))
                {
                    newRow[col] = ForecastColumn(col);
                }
                else
                {
                    newRow[col] = dTable.Rows.Count > 0 ? dTable.Rows[dTable.Rows.Count - 1][col] : string.Empty;
                }
            }

            dTable.Rows.Add(newRow);
        }

        private string GetNextEpochLabel()
        {
            int max = -1;
            foreach (DataRow row in dTable.Rows)
            {
                int value;
                if (int.TryParse(Convert.ToString(row[0]), out value) && value > max)
                    max = value;
            }
            return (max + 1).ToString();
        }

        private double ForecastColumn(DataColumn column)
        {
            List<double> values = new List<double>();
            foreach (DataRow row in dTable.Rows)
            {
                double value;
                if (TryReadDouble(row[column], out value))
                    values.Add(value);
            }

            if (values.Count == 0)
                return 0;
            if (values.Count == 1)
                return values[0];

            double last = values[values.Count - 1];
            double sumSquaredDifferences = 0.0;
            for (int k = 0; k < values.Count - 1; k++)
            {
                double diff = values[k + 1] - values[k];
                sumSquaredDifferences += diff * diff;
            }

            double d = Math.Sqrt(sumSquaredDifferences / (values.Count - 1));
            double randomDelta = -d + random.NextDouble() * (2.0 * d);
            return Math.Round(last + randomDelta, 4);
        }
    }
}
