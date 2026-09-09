using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursovaya30
{
    public partial class Form1
    {
        private void LoadSchemeToPictureBoxes()
        {
            Image img = TryLoadBlobImage();
            SetPictureBoxImage(pictureBox1, img);
            SetPictureBoxImage(pictureBoxLevel2Scheme, img);
        }

        private Image TryLoadBlobImage()
        {
            if (SQLiteConn == null)
                return null;

            EnsureSchemeTable();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT [photo] FROM [Схема] LIMIT 1", SQLiteConn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read() && reader[0] != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])reader[0];
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    using (Image temp = Image.FromStream(ms))
                        return new Bitmap(temp);
                }
            }

            return null;
        }

        private void SetPictureBoxImage(PictureBox box, Image img)
        {
            if (box == null)
                return;

            Image old = box.Image;
            box.Image = img == null ? null : new Bitmap(img);
            if (box == pictureBox1)
                ResizeDataSchemePicture();
            if (old != null)
                old.Dispose();
        }

    }
}
