﻿namespace pdf2eink
{
    public class BookExportParams
    {
        public bool AdaptiveThreshold = false;
        public int MinGray = 200;
        public int Width = 600;        
        public int Height = 440;
        public int StartPage = 0;
        public int EndPage = 20;
        public bool UsePagesLimit = false;
        public int AspectSplitLimit = 120;
        public Action<int, int> Progress;
        public Action Finish;
        public bool SplitWhenAspectGreater = false;
        public bool RenderPageInfo = true;
    }
}
