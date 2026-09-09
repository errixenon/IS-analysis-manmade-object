using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        public class LinkInfo
        {
            public string Связь { get; set; }
            public double Эталонное_расстояние { get; set; }
            public double Максимальное_отклонение { get; set; }
            public string Статус { get; set; }
        }



        private DataTable BuildLevel3DifferencesTable(List<string> points)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Эпоха", typeof(string));

            var pairs = new List<Tuple<string, string>>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    pairs.Add(Tuple.Create(points[i], points[j]));
                    result.Columns.Add(
                        "ΔZ(" + points[i] + "-" + points[j] + ")",
                        typeof(double));
                }
            }

            DataTable source = GetCalculationTable();
            if (source == null)
                return result;

            foreach (DataRow sourceRow in source.Rows)
            {
                DataRow row = result.NewRow();
                row["Эпоха"] = Convert.ToString(sourceRow[0]);

                foreach (Tuple<string, string> pair in pairs)
                {
                    if (!source.Columns.Contains(pair.Item1) ||
                        !source.Columns.Contains(pair.Item2))
                        continue;

                    double valueA = ReadCellDouble(sourceRow, source.Columns[pair.Item1]);
                    double valueB = ReadCellDouble(sourceRow, source.Columns[pair.Item2]);
                    row["ΔZ(" + pair.Item1 + "-" + pair.Item2 + ")"] =
                        Round(valueA - valueB);
                }

                result.Rows.Add(row);
            }

            return result;
        }

        private List<LinkInfo> BuildLinks(List<string> points, double tolerance)
        {
            DataTable source = GetCalculationTable();
            points = points.Where(p => source.Columns.Contains(p)).ToList();
            List<LinkInfo> links = new List<LinkInfo>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    DataColumn a = source.Columns[points[i]];
                    DataColumn b = source.Columns[points[j]];
                    double baseDistance = Math.Abs(ReadCellDouble(source.Rows[0], a) - ReadCellDouble(source.Rows[0], b));
                    double maxDev = 0;
                    foreach (DataRow row in source.Rows)
                    {
                        double dist = Math.Abs(ReadCellDouble(row, a) - ReadCellDouble(row, b));
                        maxDev = Math.Max(maxDev, Math.Abs(dist - baseDistance));
                    }

                    links.Add(new LinkInfo
                    {
                        Связь = points[i] + " - " + points[j],
                        Эталонное_расстояние = Round(baseDistance),
                        Максимальное_отклонение = Round(maxDev),
                        Статус = maxDev <= tolerance ? "Жёсткая" : "Ослабленная"
                    });
                }
            }
            return links.OrderByDescending(x => x.Максимальное_отклонение).ToList();
        }

    }
}
