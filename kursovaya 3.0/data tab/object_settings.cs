using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void EnsureSchemeTable()
        {
            using (SQLiteCommand create = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [Схема] ([photo] BLOB, [blocks_count] INTEGER, [E] REAL, [A] REAL)", SQLiteConn))
                create.ExecuteNonQuery();

            EnsureColumn("Схема", "blocks_count", "INTEGER");
            EnsureColumn("Схема", "E", "REAL");
            EnsureColumn("Схема", "A", "REAL");
        }

        private void EnsureColumn(string tableName, string columnName, string type)
        {
            List<string> columns = new List<string>();
            using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA table_info([" + tableName + "])", SQLiteConn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
                while (reader.Read()) columns.Add(reader["name"].ToString());

            if (!columns.Contains(columnName))
            {
                using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE [" + tableName + "] ADD COLUMN [" + columnName + "] " + type, SQLiteConn))
                    cmd.ExecuteNonQuery();
            }
        }

        private void SaveObjectSettingsOnly()
        {
            try
            {
                if (SQLiteConn == null)
                    return;

                EnsureSchemeTable();
                double alpha = ReadA();
                double eps = ReadEpsilon();
                int blockCount = numericBlocks == null ? 0 : (int)numericBlocks.Value;

                using (SQLiteCommand count = new SQLiteCommand("SELECT COUNT(*) FROM [Схема]", SQLiteConn))
                {
                    long c = (long)count.ExecuteScalar();
                    string sql = c == 0
                        ? "INSERT INTO [Схема] ([blocks_count], [E], [A]) VALUES (@b,@e,@a)"
                        : "UPDATE [Схема] SET [blocks_count]=@b, [E]=@e, [A]=@a WHERE rowid=(SELECT rowid FROM [Схема] LIMIT 1)";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, SQLiteConn))
                    {
                        cmd.Parameters.AddWithValue("@b", blockCount);
                        cmd.Parameters.AddWithValue("@e", eps);
                        cmd.Parameters.AddWithValue("@a", alpha);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void LoadObjectSettings()
        {
            try
            {
                EnsureSchemeTable();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT [blocks_count], [E], [A] FROM [Схема] LIMIT 1", SQLiteConn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;

                    if (reader["E"] != DBNull.Value)
                    {
                        double eps = Convert.ToDouble(reader["E"]);
                        textBoxEpsilon.Text = eps.ToString("0.####", CultureInfo.CurrentCulture);
                        SetNumericUpDownValue(numericRigidTolerance, eps);
                    }

                    if (reader["A"] != DBNull.Value)
                    {
                        double alpha = Convert.ToDouble(reader["A"]);
                        textBoxAlpha.Text = alpha.ToString("0.####", CultureInfo.CurrentCulture);
                    }

                    if (reader["blocks_count"] != DBNull.Value && numericBlocks != null)
                    {
                        decimal b = Convert.ToDecimal(reader["blocks_count"]);
                        if (b >= numericBlocks.Minimum && b <= numericBlocks.Maximum)
                            numericBlocks.Value = b;
                    }
                }
            }
            catch { }
        }


    }
}
