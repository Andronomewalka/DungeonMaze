using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheMaze
{
    public partial class FormMaze : Form
    {
        GameField gameField;
        public FormMaze()
        {
            InitializeComponent();
            CenterToScreen();
            gameField = new GameField(this);
            gameField.Create();
        }
    }
}
