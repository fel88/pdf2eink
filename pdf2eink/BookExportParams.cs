namespace pdf2eink
{
    public class BookExportParams
    {
        public bool AdaptiveThreshold = false;
        public int MinGray = 200;
        public int Width = 600;
        public int Height = 448;
        public int StartPage = 0;
        public int EndPage = 20;
        public bool Rotate90;
        public bool FlyRead;
        public bool UsePagesLimit = false;
        public int AspectSplitLimit = 120;
        public Action<int, int> Progress;
        public Action Finish;
        public bool SplitWhenAspectGreater = false;
        public bool RenderPageInfo = true;
        public int PageInfoHeight = 8;
    }
}
