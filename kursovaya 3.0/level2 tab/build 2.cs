using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void RefreshBlockLists()
        {
            if (listFreePoints == null)
                return;

            listFreePoints.Items.Clear();
            listBlockPoints.Items.Clear();

            List<string> all = GetPointColumns().Select(c => c.ColumnName).ToList();
            HashSet<string> busy = new HashSet<string>(blockMap.Values.SelectMany(v => v));
            foreach (string p in all.Where(p => !busy.Contains(p)))
                listFreePoints.Items.Add(p);

            if (comboBlocks.SelectedItem != null && blockMap.ContainsKey(comboBlocks.SelectedItem.ToString()))
            {
                string block = comboBlocks.SelectedItem.ToString();
                foreach (string p in blockMap[block])
                    listBlockPoints.Items.Add(p);
                labelBlockInfo.Text = "Точек в блоке: " + blockMap[block].Count;
            }
            else
            {
                labelBlockInfo.Text = "Точек в блоке: -";
            }
        }
    }
}
