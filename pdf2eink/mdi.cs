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
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Viewer v = new Viewer();
            v.MdiParent = this;
            v.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Editor v = new Editor();
            v.MdiParent = this;
            v.Show();
        }
    }
}
