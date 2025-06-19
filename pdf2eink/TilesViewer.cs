using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
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
            pictureBox1.InterpolationMode = InterpolationMode.NearestNeighbor;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        TiledPageInfo[] Pages;

        internal void Init(TiledPageInfo[] pages)
        {
            Pages = pages;
            var baseTiles = pages.SelectMany(z => z.Infos.Select(u => u.Tile)).Distinct().ToArray();
            UpdateList();
            toolStripStatusLabel1.Text = $"{pages.Length} pages, {pages.Sum(z => z.Infos.Length)} tiles, {baseTiles.Length} base tiles";
        }

        int currentPage = 0;
        public void UpdateList()
        {
            listView1.Items.Clear();
            //for (int i = 0; i < Pages.Length; i++)
            {
                TiledPageInfo? page = Pages[currentPage];
                foreach (var tile in page.Infos)
                {
                    listView1.Items.Add(new ListViewItem(new string[] { "page " + currentPage + "_" + "tile", tile.Key }) { Tag = tile });
                }
            }
        }
        TileStatisticInfo[] Tiles;
        public void UpdateBaseList()
        {
            listView1.Items.Clear();

            foreach (var tile in Tiles)
            {
                listView1.Items.Add(new ListViewItem(new string[] { "tile", tile.Tile.Name,tile.Qty.ToString() }) { Tag = tile });
            }

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            if (listView1.SelectedItems[0].Tag is TileInfo t)
            {
                pictureBox1.Image = t.Tile.Bmp;
            }
            if (listView1.SelectedItems[0].Tag is Tile tt)
            {
                pictureBox1.Image = tt.Bmp;
            }
            if (listView1.SelectedItems[0].Tag is TileStatisticInfo ttt)
            {
                pictureBox1.Image = ttt.Tile.Bmp;
                toolStripStatusLabel2.Text = ttt.Tile.Bmp.Width + "x" + ttt.Tile.Bmp.Height;
            }
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
                item.WriteTo(ms);                
            }

            while (ms.Length % 1024 != 0)
                ms.WriteByte(0);

            var pagesSectionOffset = (int)(ms.Length / 1024);
            var list = baseTiles.ToList();
            ms.Write(BitConverter.GetBytes(Pages.Length));
            var bitsOfpattern = (int)Math.Ceiling(Math.Log2(list.Count));
            foreach (var item in Pages)
            {
                ms.Write(BitConverter.GetBytes(item.Width));
                ms.Write(BitConverter.GetBytes(item.Heigth));

                ms.Write(BitConverter.GetBytes(item.Infos.Length));
                foreach (var pitem in item.Infos)
                {
                    var bits = new BitArray(BitConverter.GetBytes((ushort)pitem.X));
                    var bits2 = new BitArray(BitConverter.GetBytes((ushort)pitem.Y));
                    var bits3 = new BitArray(BitConverter.GetBytes((uint)list.IndexOf(pitem.Tile)));
                    List<byte> ar = new List<byte>();
                    for (int i = 0; i < 10; i++)
                    {
                        ar.Add((byte)(bits[i] ? 1 : 0));
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        ar.Add((byte)(bits2[i] ? 1 : 0));
                    }
                    for (int i = 0; i < bitsOfpattern; i++)
                    {
                        ar.Add((byte)(bits3[i] ? 1 : 0));
                    }
                    while (ar.Count % 8 != 0)
                    {
                        ar.Add(0);
                    }
                    for (int j = 0; j < ar.Count; j += 8)
                    {
                        byte b = 0;
                        for (int k = 0; k < 8; k++)
                        {
                            b |= (byte)(ar[j + k] << k);
                        }
                        ms.WriteByte(b);
                    }
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

        public class TileStatisticInfo
        {
            public Tile Tile;
            public int Qty;

            internal int DistTo(TileStatisticInfo deq)
            {
                int diff = 0;
                for (int i = 0; i < Tile.ImageHash.Length; i++)
                {
                    if (Tile.ImageHash[i] != deq.Tile.ImageHash[i])
                        diff++;
                }
                return diff;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            var tiles = Pages.SelectMany(z => z.Infos.Select(u => u.Tile)).Distinct();
            Tiles = tiles.Select(z => new TileStatisticInfo() { Tile = z }).ToArray();
            foreach (var item in Tiles)
            {
                item.Qty = Pages.Sum(z => z.Infos.Count(u => u.Tile == item.Tile));
            }
            List<TileStatisticInfo> ordered = new List<TileStatisticInfo>();
            ordered.Add(Tiles[0]);
            List<TileStatisticInfo> left = new List<TileStatisticInfo>(Tiles);
            left.Remove(Tiles[0]);
            while (left.Any())
            {
                var deq = ordered.Last();
                var ww = left.Where(z => z.Tile.ImageHash.Length == deq.Tile.ImageHash.Length).ToArray();
                if (!ww.Any())
                {
                    ordered.Add(left[0]);
                    left.RemoveAt(0);
                    continue;
                }
                var bb = ww.Select(z => (z.DistTo(deq), z)).ToArray();
                var b = bb.OrderBy(z => z.Item1).First().z;
                ordered.Add(b);
                left.Remove(b); 
            }
            Tiles = ordered.ToArray();
            UpdateBaseList();
        }

        private void baseTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateBaseList();
        }

        private void infosToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
    public class PictureBoxWithInterpolationMode : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }
}
