using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace pdf2eink
{
    public partial class TilesViewer : Form
    {
        public TilesViewer()
        {
            InitializeComponent();
        }

        TiledPageInfo[] Pages;

        internal void Init(TiledPageInfo[] pages)
        {
            Pages = pages;
            var baseTiles = pages.SelectMany(z => z.Infos.Select(u => u.Tile)).Distinct().ToArray();
            UpdateList();
            toolStripStatusLabel1.Text = $"{pages.Length} pages, {pages.Sum(z => z.Infos.Length)} tiles, {baseTiles.Length} base tiles";
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            for (int i = 0; i < Pages.Length; i++)
            {
                TiledPageInfo? page = Pages[i];
                foreach (var tile in page.Infos)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { "page " + i + "_" + "tile", tile.Key }) { Tag = tile });
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var t = listView1.SelectedItems[0].Tag as TileInfo;
            pictureBox1.Image = t.Tile.Bmp;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Tiled pages (*.tcb)|*.tcb";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            MemoryStream ms = new MemoryStream();

            var baseTiles = Pages.SelectMany(z => z.Infos.Select(u => u.Tile)).Distinct().ToArray();

            ms.Write(BitConverter.GetBytes(0));//stub for the start offset to pages section

            ms.Write(BitConverter.GetBytes(baseTiles.Length));
            foreach (var item in baseTiles)
            {
                ms.Write(BitConverter.GetBytes((int)item.Bmp.Width));
                ms.Write(BitConverter.GetBytes((int)item.Bmp.Height));

                List<byte> bits = new List<byte>();
                for (int i = 0; i < item.Bmp.Width; i++)
                {
                    for (int j = 0; j < item.Bmp.Height; j++)
                    {
                        var px = item.Bmp.GetPixel(i, j);
                        bits.Add((byte)(px.R == 0 ? 0 : 1));
                    }
                }

                while (bits.Count % 8 != 0)
                    bits.Add(0);

                for (int i = 0; i < bits.Count; i += 8)
                {
                    byte b = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        b ^= (byte)(bits[i + j] << j);
                    }

                    ms.WriteByte(b);
                }
            }

            while (ms.Length % 1024 != 0)
                ms.WriteByte(0);

            var pagesSectionOffset = (int)(ms.Length/1024);

            ms.Write(BitConverter.GetBytes(Pages.Length));
            foreach (var item in Pages)
            {
                ms.Write(BitConverter.GetBytes(item.Width));
                ms.Write(BitConverter.GetBytes(item.Heigth));

                ms.Write(BitConverter.GetBytes(item.Infos.Length));
                foreach (var pitem in item.Infos)
                {
                    ms.Write(BitConverter.GetBytes((int)pitem.X));
                    ms.Write(BitConverter.GetBytes((int)pitem.Y));
                    ms.Write(BitConverter.GetBytes((int)baseTiles.ToList().IndexOf(pitem.Tile)));
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            ms.Write(BitConverter.GetBytes(pagesSectionOffset));
            ms.Seek(0, SeekOrigin.Begin);

            File.WriteAllBytes(sfd.FileName, ms.ToArray());
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Tiled pages (*.tcb)|*.tcb";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var bts = File.ReadAllBytes(ofd.FileName);
            using MemoryStream ms = new MemoryStream(bts);
            TiledCBook book = new TiledCBook(ms);


            Viewer v = new Viewer();
            v.MdiParent = MdiParent;
            v.Init(book);
            v.Show();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var t = listView1.SelectedItems[0].Tag as TileInfo;
            var d = AutoDialog.DialogHelpers.StartDialog();

            d.AddStringField("key", "Key", t.Key);
            if (!d.ShowDialog())
                return;

            var k = d.GetStringField("key");
            t.Key = k;

            UpdateList();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var grp = Pages.SelectMany(z => z.Infos).GroupBy(z => z.Key).ToArray();


            for (int i = 0; i < Pages.Length; i++)
            {
                TiledPageInfo? page = Pages[i];
                foreach (var tile in page.Infos)
                {
                    //tile.k
                }
            }
        }
    }

}
