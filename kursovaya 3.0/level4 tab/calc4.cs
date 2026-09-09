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
        private List<PointStateRow> BuildPointRows(string point)
        {
            DataTable source = GetCalculationTable();
            if (source == null || !source.Columns.Contains(point))
                return new List<PointStateRow>();

            DataColumn col = source.Columns[point];
            List<DataColumn> singleColumnList = new List<DataColumn> { col };

            List<PhaseAnalysis> phaseResults = CalculatePhaseForColumns(singleColumnList, "Точка");

            List<PointStateRow> rows = new List<PointStateRow>();

            for (int i = 0; i < phaseResults.Count; i++)
            {
                var phase = phaseResults[i];

                rows.Add(new PointStateRow
                {
                    Эпоха = phase.Label,
                    Точка = point,
                    Z = phase.M,
                    ZPlusE = phase.MPlus,
                    ZMinusE = phase.MMinus,
                    R = phase.R,
                    L = phase.L,
                    Статус = phase.StatusText
                });
            }
            return rows;
        }



    }
}
