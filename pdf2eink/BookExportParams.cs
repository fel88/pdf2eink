namespace pdf2eink
{
    public class BookExportParams
    {
        public bool AutoDithering = false;
        public bool AdaptiveThreshold = false;
        public int MinGray = 200;
        public int Width = 648;
        public int Height = 480;
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
        public TOC TOC;
        public bool TiledMode = false;
        public bool RectifyLetters = false;        
        public bool DebugLetters = false;
    }
}
