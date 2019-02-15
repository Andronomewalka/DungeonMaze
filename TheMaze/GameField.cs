using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TheMaze
{
    enum Obj { wall, space, background};
    public class GameField
    {
        FormMaze parent;
        BinaryField binaryField;
        Bitmap bitmapField; // по мере заполнения поля добавляем тайлы в битмап, после чего записываем его в PictureBox
        PictureBox mazePicture; // графическое представление игрового поля
        bool[,] bfield; // булевое поле, полученное из BinaryField
        int[,] Field; // пиксельное поле 
        int imgWidth = Properties.Resources.space.Width;
        int imgHeight = Properties.Resources.space.Height;
        Point bitmapCoord; // координаты для ориентования в битмапе


        public GameField(FormMaze parent)
        {
            this.parent = parent;
            binaryField = new BinaryField();
            binaryField.GenerateMaze();
            bfield = binaryField.Field;
            Field = new int[parent.ClientSize.Height, parent.ClientSize.Width];
            bitmapField = new Bitmap(parent.ClientSize.Width, parent.ClientSize.Height);
            bitmapField.SetResolution(72, 72);
            bitmapCoord = new Point();
            mazePicture = new PictureBox() { Size = bitmapField.Size };
            mazePicture.Parent = parent;
            mazePicture.Click += MazePicture_Click;
        }

        private void MazePicture_Click(object sender, EventArgs e)
        {
            MouseEventArgs em = e as MouseEventArgs;
            int x = em.X;
            int y = em.Y;

            if (Field[y, x] == (int)Obj.background)
                MessageBox.Show("Background");
            else if (Field[y, x] == (int)Obj.wall)
                MessageBox.Show("Wall");
            else if (Field[y, x] == (int)Obj.space)
                MessageBox.Show("Space");
        }

        // накладываем текстуры на булевое поле(0 - стена, 1 - земля)
        public void Create()
        {
            int height = binaryField.Height;
            int width = binaryField.Width;

            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    Image sprite = Properties.Resources.background;
                    Obj objType = Obj.wall;

                    if (bfield[i, k] == true)
                    {
                        sprite = Properties.Resources.space;
                        objType = Obj.space;
                    }

                    else if (
                        (i - 1 < 0 || k - 1 < 0 ? false : bfield[i - 1, k - 1]) == false
                        && (i - 1 < 0 ? false : bfield[i - 1, k]) == false
                        && (i - 1 < 0 || k + 1 >= width ? false : bfield[i - 1, k + 1]) == false
                        && (k + 1 >= width ? false : bfield[i, k + 1]) == false
                        && (i + 1 >= height || k + 1 >= width ? false : bfield[i + 1, k + 1]) == false
                        && (i + 1 >= height ? false : bfield[i + 1, k]) == false
                        && (i + 1 >= height || k - 1 < 0 ? false : bfield[i + 1, k - 1]) == false
                        && (k - 1 < 0 ? false : bfield[i, k - 1]) == false)
                    {
                        sprite = Properties.Resources.background;
                        objType = Obj.background;
                    }

                    else if (bfield[i, k] == false)
                    {
                        sprite = Properties.Resources.wall;
                        objType = Obj.wall;
                    }

                    using (Graphics g = Graphics.FromImage(bitmapField))
                        g.DrawImage(sprite, bitmapCoord.X, bitmapCoord.Y);

                    FillField(bitmapCoord.X, bitmapCoord.Y, bitmapCoord.X + imgWidth, bitmapCoord.Y + imgHeight, objType);
                    bitmapCoord.X += imgWidth;
                }
                bitmapCoord = new Point(0, bitmapCoord.Y + imgHeight);
            }
            mazePicture.Image = bitmapField;
        }

        private void FillField(int fromX, int fromY, int toX, int toY, Obj obj)
        {
            for (int i = fromY; i <= toY; i++)
            {
                for (int k = fromX; k < toX; k++)
                {
                    if (i == parent.ClientSize.Height || k == parent.ClientSize.Width)
                        return;
                    Field[i, k] = (int)obj;
                }
            }
        }
    }
}
