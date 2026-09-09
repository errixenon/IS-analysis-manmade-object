using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void MainForm_Load(object sender, EventArgs e)
        {
            SQLiteConn = new SQLiteConnection();
            dTable = new DataTable();
            savedTable = new DataTable();
        }

        private bool OpenDBFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "SQLite (*.sqlite;*.db)|*.sqlite;*.db";

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return false;

            if (SQLiteConn != null)
                SQLiteConn.Dispose();

            SQLiteConn = new SQLiteConnection("Data Source=" + openFileDialog.FileName + ";Version=3;");
            SQLiteConn.Open();
            labelCurrentDb.Text = Path.GetFileName(openFileDialog.FileName);
            pendingSchemeBytes = null;
            hasPendingSchemeChange = false;
            return true;
        }

        private void GetTableNames()
        {
            string SQLQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT IN ('Схема', 'sqlite_sequence', 'photo') ORDER BY name;";
            SQLiteCommand command = new SQLiteCommand(SQLQuery, SQLiteConn);
            SQLiteDataReader reader = command.ExecuteReader();

            comboBox1.Items.Clear();
            while (reader.Read())
                comboBox1.Items.Add(reader[0].ToString());

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void SaveCurrentTable()
        {
            if (SQLiteConn == null || dTable == null || string.IsNullOrWhiteSpace(currentTableName))
                return;

            dataGridView1.EndEdit();
            using (SQLiteTransaction tr = SQLiteConn.BeginTransaction())
            {
                using (SQLiteCommand del = new SQLiteCommand("DELETE FROM [" + currentTableName + "]", SQLiteConn, tr))
                    del.ExecuteNonQuery();

                List<string> cols = dTable.Columns.Cast<DataColumn>().Select(c => "[" + c.ColumnName + "]").ToList();
                List<string> pars = dTable.Columns.Cast<DataColumn>().Select((c, i) => "@p" + i).ToList();
                string sql = "INSERT INTO [" + currentTableName + "] (" + string.Join(",", cols) + ") VALUES (" + string.Join(",", pars) + ")";

                foreach (DataRow row in dTable.Rows)
                {
                    using (SQLiteCommand ins = new SQLiteCommand(sql, SQLiteConn, tr))
                    {
                        for (int i = 0; i < dTable.Columns.Count; i++)
                            ins.Parameters.AddWithValue(pars[i], row[i] == null ? DBNull.Value : row[i]);
                        ins.ExecuteNonQuery();
                    }
                }

                tr.Commit();
            }
            savedTable = dTable.Copy();
        }

        private void SavePendingSchemeIfNeeded()
        {
            if (SQLiteConn == null || !hasPendingSchemeChange || pendingSchemeBytes == null)
                return;

            EnsureSchemeTable();
            using (SQLiteCommand count = new SQLiteCommand("SELECT COUNT(*) FROM [Схема]", SQLiteConn))
            {
                long c = (long)count.ExecuteScalar();
                string sql = c == 0
                    ? "INSERT INTO [Схема] ([photo]) VALUES (@p)"
                    : "UPDATE [Схема] SET [photo]=@p WHERE rowid=(SELECT rowid FROM [Схема] LIMIT 1)";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, SQLiteConn))
                {
                    cmd.Parameters.AddWithValue("@p", pendingSchemeBytes);
                    cmd.ExecuteNonQuery();
                }
            }

            pendingSchemeBytes = null;
            hasPendingSchemeChange = false;
        }

        
    }
}