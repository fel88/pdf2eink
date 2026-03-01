using CefSharp.OffScreen;
using Microsoft.Web.WebView2.Core;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using UglyToad.PdfPig.Content;
using static pdf2eink.Editor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace pdf2eink
{
    public partial class HtmlRenderer : Form
    {
        public HtmlRenderer()
        {
            InitializeComponent();
            Load += HtmlRenderer_Load;
        }

        private async void HtmlRenderer_Load(object? sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            // After initialization, you can set the HTML
            LoadHtmlContent();
        }
        private void LoadHtmlContent()
        {
            if (webView21 != null && webView21.CoreWebView2 != null)
            {
                string htmlContent = @"
            <html>
            <head>
                <title>Dynamic HTML</title>
                <style>
                    body { font-family: sans-serif; background-color: #ffffff; }
                    h1 { color: blue; }
                </style>
            </head>
            <body>
                <h1>Hello from WebView2!</h1>
                <p>This content was set from a C# string in a WinForms application.</p>
            </body>
            </html>";

                webView21.CoreWebView2.NavigateToString(htmlContent);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {

        }
        async Task<Bitmap> capture()
        {
            // Use a MemoryStream to hold the image data temporarily
            using (MemoryStream imageStream = new MemoryStream())
            {
                // Capture the preview into the stream in PNG format
                await webView21.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, imageStream);
                                
                imageStream.Position = 0; // Rewind again
                Bitmap bitmap = new Bitmap(imageStream);
                
                return bitmap; ;
            }
        }
        private async void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 != null)
            {
                // Use a MemoryStream to hold the image data temporarily
                using (MemoryStream imageStream = new MemoryStream())
                {
                    var b = await capture();
                                        
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "capture.png");
                    b.Save(filePath, ImageFormat.Png);
                    

                    MessageBox.Show($"Screenshot saved to {filePath}");

                }
            }
            else
            {
                MessageBox.Show("WebView2 is not initialized yet.");
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var filePath = ofd.FileName;
            string htmlContent;
            Encoding encoding = Encoding.UTF8; // Default to UTF-8 as per HTML standard

            // 1. Read a small portion (e.g., first 1024 bytes) to find the charset declaration
            using (var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024))
            {
                char[] buffer = new char[1024];
                reader.Read(buffer, 0, buffer.Length);
                string startOfFile = new string(buffer);

                // 2. Use regex to find the charset value
                var charsetMatch = Regex.Match(startOfFile, @"<meta\s+[^>]*?charset\s*=\s*['""]?([^'""\s>]+)['""]?", RegexOptions.IgnoreCase);
                if (charsetMatch.Success)
                {
                    string charset = charsetMatch.Groups[1].Value;
                    try
                    {
                        encoding = Encoding.GetEncoding(charset); // Get the specific encoding
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine($"Unknown or unsupported encoding: {charset}. Falling back to default.");
                    }
                }
            }

            // 3. Read the entire file using the determined encoding
            htmlContent = File.ReadAllText(filePath, encoding);
            webView21.CoreWebView2.NavigateToString(htmlContent);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var diffx = 640 - webView21.Width;
            Width += diffx;
            var diffy = 448 - webView21.Height;
            Height += diffy;
        }
        private async Task ScrollWebViewPage(bool scrollDown)
        {
            // JavaScript to get the height of the visible part of the WebView2 (viewport height)
            var viewportHeightJson = await webView21.CoreWebView2.ExecuteScriptAsync("window.innerHeight");

            // Convert the JSON result to an integer
            if (int.TryParse(viewportHeightJson, out int viewportHeight))
            {
                // Calculate the scroll amount: one page height (minus a small margin if needed)
                int scrollAmount = scrollDown ? viewportHeight : -viewportHeight;

                // JavaScript to get the current scroll position
                var scrollYJson = await webView21.CoreWebView2.ExecuteScriptAsync("window.scrollY");
                if (int.TryParse(scrollYJson, out int currentScrollY))
                {
                    // Calculate the new scroll position
                    int newScrollY = currentScrollY + scrollAmount;

                    // Execute JavaScript to scroll to the new position
                    // window.scroll(xpos, ypos) is used to scroll to a specific position
                    await webView21.CoreWebView2.ExecuteScriptAsync($"window.scroll(0, {newScrollY});");
                }
            }
        }

        private async void toolStripButton4_Click(object sender, EventArgs e)
        {
            await ScrollWebViewPage(true);
        }


        private async void toolStripButton5_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();

            MemoryStream ms = new MemoryStream();
            Editor.CreateEmptyBook(ms);
            var book = new CbBook(ms);


            d.AddBoolField("pagesLimit", "pagesLimit", true);
            d.AddIntegerNumericField("maxPages", "Max pages", 20);



            if (!d.ShowDialog())
                return;

            int? pagesLimit = null;
            if (d.GetBoolField("pagesLimit"))
            {
                pagesLimit = d.GetIntegerNumericField("maxPages");
            }

            await RenderBookFromHtml(book,

                pagesLimit
                );
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CB/TCB (*.cb, *.tcb)|*.cb;*.tcb|CB files (*.cb)|*.cb|Tiled book (*.tcb)|*.tcb";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            book.SaveAs(sfd.FileName);

        }
        int pageNo = 0;
        public async Task RenderBookFromHtml(CbBook book,


          int? maxPages,
          bool onlySpacesBreak = true)
        {

            //toolStripProgressBar1.Maximum = strings.Count();

            //toolStripProgressBar1.Visible = true;
            await Task.Run(async () =>
            {

                Graphics gr = null;
                BookExportParams bep = new BookExportParams();

                RectangleF layoutRectangle = new RectangleF(0, 0, book.Width, book.Height - bep.PageInfoHeight);
                Bitmap bmp = null;
                var insertPage = () =>
                {
                    book.InsertPage(book.pages);
                    bmp = book.GetPage(pageNo);

                    gr = Graphics.FromImage(bmp);
                    //gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    //fillRectangle(0, 0, book.Width, book.Height);
                    gr.FillRectangle(Brushes.White, 0, 0, book.Width, book.Height);
                };
                var finalizePage = async () =>
                {
                    var hh = book.Height - bep.PageInfoHeight - 1;

                    gr.FillRectangle(Brushes.White, 0, hh, book.Width, bep.PageInfoHeight + 1);

                    gr.DrawLine(Pens.Black, 0, hh, book.Width, hh);

                    var str = $"{pageNo} / {book.pages}";
                    /*for (int z = 0; z < str.Length; z++)
                    {
                        gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
                     Brushes.Black, 0, 5 + z * 10);
                    }*/
                    string fontName = "Consolas";
                    fontName = "Courier New";
                    var ms = gr.MeasureString("99999 / 99999", new Font(fontName, 7));

                    int xx = (pageNo * 15) % (int)(book.Width - ms.Width - 1);
                    gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    gr.DrawString(str.ToString(), new Font(fontName, 7), Brushes.Black, xx, hh - 1);
                    using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                    {
                        var buf = BookExportContext.GetBuffer(clone);
                        for (int i = 0; i < buf.Length; i++)
                        {
                            buf[i] = (byte)~buf[i];
                        }
                        book.UpdatePage(buf, pageNo);
                    }

                    pageNo++;

                };

                insertPage();

                bool lastPageFinalized = false;
                int lineIndex = 0;
                for (int i = 0; i < maxPages; i++)
                {


                    //statusStrip1.Invoke(() =>
                    //{
                    //    toolStripProgressBar1.Value = strings.Count - q.Count;
                    //});

                    Bitmap b = null;
                    await webView21.Invoke(async () => {
                        b = await capture(); 
                        await ScrollWebViewPage(true);
                    });
                    
                   
                    gr.DrawImageUnscaled(b, 0, 0);
                    finalizePage();

                    lastPageFinalized = true;

                    insertPage();


                }



                //statusStrip1.Invoke(() =>
                //{
                //    toolStripProgressBar1.Visible = false;
                //});
            });


        }
    }

}
