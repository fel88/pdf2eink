using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pdf2eink
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        public static string DjVuLibrePath = "C:\\Program Files (x86)\\DjVuLibre";

        private void button1_Click(object sender, EventArgs e)
        {
            DjVuLibrePath = textBox1.Text;
            Close();
        }
    }
}
