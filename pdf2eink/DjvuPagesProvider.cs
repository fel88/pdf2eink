using System.Text;
using System.Diagnostics;

namespace pdf2eink
{
    public class DjvuPagesProvider : IPagesProvider, IDisposable
    {
        string bookName;
        public DjvuPagesProvider(string fileName)
        {
            bookName = fileName;
            Pages = GetPagesCount(bookName);
        }


        public int Pages { get; set; }

        public Bitmap GetPage(int index)
        {
            Process compiler = new Process();
            compiler.StartInfo.FileName = Path.Combine(Settings.DjVuLibrePath, "ddjvu.exe");

            compiler.StartInfo.Arguments = $"-page={index} {bookName}";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.CreateNoWindow = true;

            int lcid = GetSystemDefaultLCID();
            var ci = System.Globalization.CultureInfo.GetCultureInfo(lcid);
            var page = ci.TextInfo.OEMCodePage;

            var enc = CodePagesEncodingProvider.Instance.GetEncoding(page);
            compiler.StartInfo.StandardOutputEncoding = enc;
            compiler.StartInfo.RedirectStandardOutput = true;
            //compiler.OutputDataReceived += Compiler_OutputDataReceived;
            compiler.Start();
          
            var txt = compiler.StandardOutput.ReadToEnd();
            compiler.WaitForExit();

            return FromPPM(txt);
        }

        public int GetPagesCount(string book)
        {
            Process compiler = new Process();
            compiler.StartInfo.FileName = Path.Combine(Settings.DjVuLibrePath, "djvused.exe");            

            compiler.StartInfo.Arguments = $"-e n {book}";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.CreateNoWindow = true;

            int lcid = GetSystemDefaultLCID();
            var ci = System.Globalization.CultureInfo.GetCultureInfo(lcid);
            var page = ci.TextInfo.OEMCodePage;

            var enc = CodePagesEncodingProvider.Instance.GetEncoding(page);
            compiler.StartInfo.StandardOutputEncoding = enc;
            compiler.StartInfo.RedirectStandardOutput = true;
            
            compiler.Start();

            var txt = compiler.StandardOutput.ReadToEnd();
            compiler.WaitForExit();

            return int.Parse(txt);
        }



        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern int GetSystemDefaultLCID();

        Bitmap FromPPM(string text)
        {
            var ind1 = text.IndexOf('\n');

            var ln1 = text.Substring(0, ind1);
            int bytesPerPixel = 1;
            if (ln1.StartsWith("P6"))
            {
                bytesPerPixel = 3;
            }

            text = text.Substring(ind1 + 1);
            var ind2 = text.IndexOf('\n');
            var ln2 = text.Substring(0, ind2);

            text = text.Substring(ind2 + 1);
            var ind3 = text.IndexOf('\n');
            var ln3 = text.Substring(0, ind3);

            text = text.Substring(ind3 + 1);

            int lcid = GetSystemDefaultLCID();
            var ci = System.Globalization.CultureInfo.GetCultureInfo(lcid);
            var page = ci.TextInfo.OEMCodePage;

            var enc = CodePagesEncodingProvider.Instance.GetEncoding(page);
            var bts = enc.GetBytes(text);


            var sizes = ln2.Split(' ');
            int w = int.Parse(sizes[0]);
            int h = int.Parse(sizes[1]);
            Bitmap b = new Bitmap(w, h);
            var max = int.Parse(ln3);
            int x = 0;
            int y = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (bytesPerPixel == 3)
                    {
                        byte[] bb = new byte[3];
                        for (int k = 0; k < 3; k++)
                        {
                            var v = (byte)(bts[(j * w + i) * bytesPerPixel + k]);
                            bb[k] = v;
                        }
                        b.SetPixel(i, j, Color.FromArgb(bb[0], bb[1], bb[2]));
                    }
                    else
                    {
                        var v = (byte)(bts[(j * w + i) * bytesPerPixel]);
                        b.SetPixel(i, j, Color.FromArgb(v, v, v));
                    }

                    /*if (v < max / 2)
                    {
                        b.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        b.SetPixel(i, j, Color.White);
                    }*/
                }
            }

            return b;
        }
        public void Dispose()
        {
        }
    }

}