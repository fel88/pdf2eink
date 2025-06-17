using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pdf2eink
{
    public partial class TilesViewer : Form
    {
        public TilesViewer()
        {
            InitializeComponent();
        }

        TileInfo[] Tiles;
        TiledPageInfo Page;
        internal void Init(TiledPageInfo page)
        {
            Page = page;
            Tiles = page.Infos;
            UpdateList();
            toolStripStatusLabel1.Text = Tiles.Length + " tiles";
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (TileInfo tile in Tiles)
            {
                listView1.Items.Add(new ListViewItem(new string[] { "tile" }) { Tag = tile });
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

            var baseTiles = Tiles.Select(z => z.Tile).Distinct().ToArray();
            ms.Write(BitConverter.GetBytes(Page.Width));
            ms.Write(BitConverter.GetBytes(Page.Heigth));

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
            ms.Write(BitConverter.GetBytes(Tiles.Length));
            foreach (var item in Tiles)
            {
                ms.Write(BitConverter.GetBytes((int)item.X));
                ms.Write(BitConverter.GetBytes((int)item.Y));
                ms.Write(BitConverter.GetBytes((int)baseTiles.ToList().IndexOf(item.Tile)));
            }
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
            var pageWidth = ms.ReadInt();
            var pageHeigth = ms.ReadInt();
            var tilesQty = ms.ReadInt();

            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < tilesQty; i++)
            {
                Tile tile = new Tile(ms);
                tiles.Add(tile);
            }

            var tileInfosQty = ms.ReadInt();
            List<TileInfo> tileInfos = new List<TileInfo>();
            for (int i = 0; i < tileInfosQty; i++)
            {
                TileInfo t = new TileInfo();
                t.X = ms.ReadInt();
                t.Y = ms.ReadInt();
                var tileIdx = ms.ReadInt();
                t.Tile = tiles[tileIdx];
                tileInfos.Add(t);
            }

            Bitmap bmp = new Bitmap(pageWidth, pageHeigth);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            foreach (var item in tileInfos)
            {
                gr.DrawImage(item.Tile.Bmp, item.X, item.Y);
            }
            //draw result
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            bmp.Save(sfd.FileName);


        }
    }
}
