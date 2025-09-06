using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace pdf2eink
{
    public partial class SourceBookViewer : Form
    {
        public SourceBookViewer()
        {
            InitializeComponent();

        }

        public void Open(IPagesProvider pp)
        {
            book = pp;
            pictureBox1.Image = pp.GetPage(0);
            Text = pp.SourcePath;
            trackBar1.Maximum = pp.Pages;
            toolStripStatusLabel1.Text = $"{pp.Pages} total pages";
        }

        IPagesProvider book;
        int page = 0;
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                page++;
                pictureBox1.Image = book.GetPage(page);
                if (book is IPagesProviderWithLetters l)
                {
                    richTextBox1.Text = l.GetPageText(page);
                }

            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            page = trackBar1.Value;
            pictureBox1.Image = book.GetPage(page);
            //richTextBox1.Text = book.GetPageText(page);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("page", "Page", 0, book.Pages, decimalPlaces: 0);
            if (!d.ShowDialog())
                return;

            page = d.GetIntegerNumericField("page");
            pictureBox1.Image = book.GetPage(page);
            //richTextBox1.Text = book.GetPageText(page);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            List<TiledPageInfo> pages = new List<TiledPageInfo>();
            List<Word> words = new List<Word>();
            Page ppage = null;
            using (PdfDocument document = PdfDocument.Open(book.SourcePath))
            {
                foreach (Page page in document.GetPages().Skip(page).Take(1))
                {
                    string pageText = page.Text;
                    ppage = page;
                    words = page.GetWords().ToList();
                }
            }

            var bmp = book.GetPage(page).ToMat();
            var bw = BookExporter.Threshold(bmp, new BookExportParams());
            TileProcessor tp = new TileProcessor();
            tp.Init(bw.ToBitmap());
            //tp.GetDebugBitmap().Save("debug1.jpg");
            tp.MakeGroups();
            //  tp.GetDebugBitmap().Save("debug2.jpg");
            tp.SimplifyMarks();
            // tp.GetDebugBitmap().Save("debug3.jpg");

            pages.Add(tp.ExtractTiles());
            var letters = words.SelectMany(z => z.Letters).ToArray();
            /*var bmp = book.GetPage(page);
            var gr = Graphics.FromImage(bmp);

            
            var kx = bmp.Width / (float)ppage.Width;
            var ky = bmp.Height / (float)ppage.Height;
            foreach (var item in words)
            {
                foreach (var litem in item.Letters)
                {
                    var g = litem.GlyphRectangle;

                    gr.DrawRectangle(Pens.Red, new RectangleF(kx * (float)g.Left,
                        bmp.Height - ky * (float)g.Top, kx * (float)g.Width,
                         ky * (float)g.Height));
                }
            }*/
            //bmp.Save("out1.png");
            foreach (var item in pages)
            {
                foreach (var pitem in item.Infos)
                {
                    var cx = pitem.X + pitem.Tile.Bmp.Width / 2;
                    var cy = pitem.Y + pitem.Tile.Bmp.Height / 2;
                    var fr = letters.OrderBy(z => Math.Abs((z.Location.X + z.GlyphRectangle.Width / 2) - cx) + Math.Abs((z.Location.Y + z.GlyphRectangle.Height / 2))).First();

                    pitem.Key = fr.Value;

                }
            }

            var allTilesInfos = pages.SelectMany(z => z.Infos).ToArray();
            var allTiles = pages.SelectMany(z => z.Infos.Select(u => u.Tile)).ToArray();
            var tiles = TileProcessor.DistinctTiles(allTiles.ToArray()).ToList();

            foreach (var item in allTilesInfos)
            {
                item.Tile = tiles.First(z => z.ImageHash == item.Tile.ImageHash);
            }
            TilesViewer tv = new TilesViewer();
            tv.MdiParent = MdiParent;
            tv.Init(pages.ToArray());
            tv.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("startPage", "Start page", 0, book.Pages, decimalPlaces: 0);
            d.AddNumericField("endPage", "End page", 0, book.Pages, decimalPlaces: 0);
            if (!d.ShowDialog())
                return;

            var startPage = d.GetIntegerNumericField("startPage");
            var endPage = d.GetIntegerNumericField("endPage");

            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
                return;       

            XElement elem = new XElement("root");
            var l = book as IPagesProviderWithLetters;
            Dictionary<string, FontInfo> fonts = new();
            XElement fontsElem = new XElement("fonts");
            XElement pages = new XElement("pages");
            elem.Add(fontsElem);
            elem.Add(pages);
            for (int i = startPage; i < endPage; i++)
            {
                XElement page = new XElement("page");
                
                var pageInfo = l.GetPageSize(i);
                page.Add(new XAttribute("w", pageInfo.Width));
                page.Add(new XAttribute("h", pageInfo.Height));
                var letters = l.GetPageLetters(i);
                foreach (var item in letters)
                {
                    XElement letter = new XElement("letter");
                    letter.Add(new XAttribute("letter", item.Letter));
                    letter.Add(new XAttribute("x", item.Bound.X));
                    letter.Add(new XAttribute("y", item.Bound.Y));
                    letter.Add(new XAttribute("w", item.Bound.Width));
                    letter.Add(new XAttribute("h", item.Bound.Height));
                    letter.Add(new XAttribute("locationX", item.Location.X));
                    letter.Add(new XAttribute("locationY", item.Location.Y));
                    
                    fonts.TryAdd(item.Font, item.FontInfo);
                    letter.Add(new XAttribute("fontId", fonts.Keys.ToList().IndexOf(item.Font)));
                    page.Add(letter);
                }
                pages.Add(page);
            }

            int fontIndex = 0;
            foreach (var item in fonts)
            {
                fontsElem.Add(new XElement("font", [new XAttribute("id", fontIndex++),  
                    new XAttribute("bold", item.Value.IsBold),
                    new XAttribute("italic", item.Value.IsItalic),
                    new XAttribute("family", item.Value.Family),
                    new XAttribute("size", item.Value.Size)]));
            }


            File.WriteAllText(sfd.FileName, elem.ToString());

        }

        private void SourceBookViewer_Load(object sender, EventArgs e)
        {

        }
    }
}
