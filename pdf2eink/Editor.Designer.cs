namespace pdf2eink
{
    partial class Editor
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
            pictureBox1 = new PictureBoxWithInterpolationMode();
            tableLayoutPanel1 = new TableLayoutPanel();
            trackBar1 = new TrackBar();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            loadToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            attachSourceBookToolStripMenuItem = new ToolStripMenuItem();
            compressToolStripMenuItem = new ToolStripMenuItem();
            createFromTextToolStripMenuItem = new ToolStripMenuItem();
            toolStripButton2 = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            toolStripButton6 = new ToolStripButton();
            toolStripDropDownButton2 = new ToolStripDropDownButton();
            almostWhiteToolStripMenuItem = new ToolStripMenuItem();
            almostBlackToolStripMenuItem = new ToolStripMenuItem();
            almostNoWhiteHorizontalLinesToolStripMenuItem = new ToolStripMenuItem();
            toolStripDropDownButton3 = new ToolStripDropDownButton();
            flyReadToolStripMenuItem = new ToolStripMenuItem();
            mirrorReadToolStripMenuItem = new ToolStripMenuItem();
            inverseColorsToolStripMenuItem = new ToolStripMenuItem();
            thisPageToolStripMenuItem = new ToolStripMenuItem();
            allPagesToolStripMenuItem1 = new ToolStripMenuItem();
            extractTilesToolStripMenuItem = new ToolStripMenuItem();
            toolStripDropDownButton4 = new ToolStripDropDownButton();
            parseToolStripMenuItem = new ToolStripMenuItem();
            showToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            printToPageToolStripMenuItem = new ToolStripMenuItem();
            toolStripDropDownButton5 = new ToolStripDropDownButton();
            singlePageToolStripMenuItem = new ToolStripMenuItem();
            allPagesToolStripMenuItem = new ToolStripMenuItem();
            toolStripButton5 = new ToolStripDropDownButton();
            renderTextToolStripMenuItem = new ToolStripMenuItem();
            fillRectangleToolStripMenuItem = new ToolStripMenuItem();
            toolStripDropDownButton6 = new ToolStripDropDownButton();
            nearestToolStripMenuItem = new ToolStripMenuItem();
            defaultToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabel3 = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            contextMenuStrip1 = new ContextMenuStrip(components);
            showImageToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            pictureBox1.Location = new Point(3, 2);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(939, 351);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            pictureBox1.MouseClick += pictureBox1_MouseClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(trackBar1, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 27);
            tableLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            tableLayoutPanel1.Size = new Size(945, 370);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // trackBar1
            // 
            trackBar1.Dock = DockStyle.Fill;
            trackBar1.Location = new Point(3, 357);
            trackBar1.Margin = new Padding(3, 2, 3, 2);
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(939, 11);
            trackBar1.TabIndex = 1;
            trackBar1.TickStyle = TickStyle.None;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripButton2, toolStripButton1, toolStripButton3, toolStripButton4, toolStripButton6, toolStripDropDownButton2, toolStripDropDownButton3, toolStripDropDownButton4, toolStripDropDownButton5, toolStripButton5, toolStripDropDownButton6 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(945, 27);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { loadToolStripMenuItem, saveAsToolStripMenuItem, attachSourceBookToolStripMenuItem, compressToolStripMenuItem, createFromTextToolStripMenuItem });
            toolStripDropDownButton1.Image = Properties.Resources.book;
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(58, 24);
            toolStripDropDownButton1.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Image = Properties.Resources.book_open;
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new Size(184, 26);
            loadToolStripMenuItem.Text = "load";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(184, 26);
            saveAsToolStripMenuItem.Text = "save as";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // attachSourceBookToolStripMenuItem
            // 
            attachSourceBookToolStripMenuItem.Name = "attachSourceBookToolStripMenuItem";
            attachSourceBookToolStripMenuItem.Size = new Size(184, 26);
            attachSourceBookToolStripMenuItem.Text = "attach source book";
            attachSourceBookToolStripMenuItem.Click += attachSourceBookToolStripMenuItem_Click;
            // 
            // compressToolStripMenuItem
            // 
            compressToolStripMenuItem.Image = Properties.Resources.box_zipper;
            compressToolStripMenuItem.Name = "compressToolStripMenuItem";
            compressToolStripMenuItem.Size = new Size(184, 26);
            compressToolStripMenuItem.Text = "compress";
            compressToolStripMenuItem.Click += compressToolStripMenuItem_Click;
            // 
            // createFromTextToolStripMenuItem
            // 
            createFromTextToolStripMenuItem.Image = Properties.Resources.printer__arrow;
            createFromTextToolStripMenuItem.Name = "createFromTextToolStripMenuItem";
            createFromTextToolStripMenuItem.Size = new Size(184, 26);
            createFromTextToolStripMenuItem.Text = "create from text";
            createFromTextToolStripMenuItem.Click += createFromTextToolStripMenuItem_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(65, 24);
            toolStripButton2.Text = "goto page";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(89, 24);
            toolStripButton1.Text = "insert page";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(92, 24);
            toolStripButton3.Text = "delete page";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(85, 24);
            toolStripButton4.Text = "back page";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripButton6
            // 
            toolStripButton6.Image = Properties.Resources.arrow;
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(83, 24);
            toolStripButton6.Text = "next page";
            toolStripButton6.Click += toolStripButton6_Click;
            // 
            // toolStripDropDownButton2
            // 
            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { almostWhiteToolStripMenuItem, almostBlackToolStripMenuItem, almostNoWhiteHorizontalLinesToolStripMenuItem });
            toolStripDropDownButton2.Image = (Image)resources.GetObject("toolStripDropDownButton2.Image");
            toolStripDropDownButton2.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            toolStripDropDownButton2.Size = new Size(74, 24);
            toolStripDropDownButton2.Text = "search";
            // 
            // almostWhiteToolStripMenuItem
            // 
            almostWhiteToolStripMenuItem.Name = "almostWhiteToolStripMenuItem";
            almostWhiteToolStripMenuItem.Size = new Size(242, 22);
            almostWhiteToolStripMenuItem.Text = "almost white";
            almostWhiteToolStripMenuItem.Click += almostWhiteToolStripMenuItem_Click;
            // 
            // almostBlackToolStripMenuItem
            // 
            almostBlackToolStripMenuItem.Name = "almostBlackToolStripMenuItem";
            almostBlackToolStripMenuItem.Size = new Size(242, 22);
            almostBlackToolStripMenuItem.Text = "almost black";
            // 
            // almostNoWhiteHorizontalLinesToolStripMenuItem
            // 
            almostNoWhiteHorizontalLinesToolStripMenuItem.Name = "almostNoWhiteHorizontalLinesToolStripMenuItem";
            almostNoWhiteHorizontalLinesToolStripMenuItem.Size = new Size(242, 22);
            almostNoWhiteHorizontalLinesToolStripMenuItem.Text = "almost no white horizontal lines";
            // 
            // toolStripDropDownButton3
            // 
            toolStripDropDownButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton3.DropDownItems.AddRange(new ToolStripItem[] { flyReadToolStripMenuItem, mirrorReadToolStripMenuItem, inverseColorsToolStripMenuItem, extractTilesToolStripMenuItem });
            toolStripDropDownButton3.Image = (Image)resources.GetObject("toolStripDropDownButton3.Image");
            toolStripDropDownButton3.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            toolStripDropDownButton3.Size = new Size(49, 24);
            toolStripDropDownButton3.Text = "filters";
            // 
            // flyReadToolStripMenuItem
            // 
            flyReadToolStripMenuItem.Name = "flyReadToolStripMenuItem";
            flyReadToolStripMenuItem.Size = new Size(146, 22);
            flyReadToolStripMenuItem.Text = "fly read";
            flyReadToolStripMenuItem.Click += flyReadToolStripMenuItem_Click;
            // 
            // mirrorReadToolStripMenuItem
            // 
            mirrorReadToolStripMenuItem.Name = "mirrorReadToolStripMenuItem";
            mirrorReadToolStripMenuItem.Size = new Size(146, 22);
            mirrorReadToolStripMenuItem.Text = "mirror read";
            mirrorReadToolStripMenuItem.Click += bustofedonToolStripMenuItem_Click;
            // 
            // inverseColorsToolStripMenuItem
            // 
            inverseColorsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { thisPageToolStripMenuItem, allPagesToolStripMenuItem1 });
            inverseColorsToolStripMenuItem.Name = "inverseColorsToolStripMenuItem";
            inverseColorsToolStripMenuItem.Size = new Size(146, 22);
            inverseColorsToolStripMenuItem.Text = "inverse colors";
            inverseColorsToolStripMenuItem.Click += inverseColorsToolStripMenuItem_Click;
            // 
            // thisPageToolStripMenuItem
            // 
            thisPageToolStripMenuItem.Name = "thisPageToolStripMenuItem";
            thisPageToolStripMenuItem.Size = new Size(122, 22);
            thisPageToolStripMenuItem.Text = "this page";
            thisPageToolStripMenuItem.Click += thisPageToolStripMenuItem_Click;
            // 
            // allPagesToolStripMenuItem1
            // 
            allPagesToolStripMenuItem1.Name = "allPagesToolStripMenuItem1";
            allPagesToolStripMenuItem1.Size = new Size(122, 22);
            allPagesToolStripMenuItem1.Text = "all pages";
            allPagesToolStripMenuItem1.Click += allPagesToolStripMenuItem1_Click;
            // 
            // extractTilesToolStripMenuItem
            // 
            extractTilesToolStripMenuItem.Name = "extractTilesToolStripMenuItem";
            extractTilesToolStripMenuItem.Size = new Size(146, 22);
            extractTilesToolStripMenuItem.Text = "extract tiles";
            extractTilesToolStripMenuItem.Click += extractTilesToolStripMenuItem_Click;
            // 
            // toolStripDropDownButton4
            // 
            toolStripDropDownButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton4.DropDownItems.AddRange(new ToolStripItem[] { parseToolStripMenuItem, showToolStripMenuItem, editToolStripMenuItem, toolStripSeparator1, printToPageToolStripMenuItem });
            toolStripDropDownButton4.Image = (Image)resources.GetObject("toolStripDropDownButton4.Image");
            toolStripDropDownButton4.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton4.Name = "toolStripDropDownButton4";
            toolStripDropDownButton4.Size = new Size(42, 24);
            toolStripDropDownButton4.Text = "TOC";
            // 
            // parseToolStripMenuItem
            // 
            parseToolStripMenuItem.Name = "parseToolStripMenuItem";
            parseToolStripMenuItem.Size = new Size(142, 22);
            parseToolStripMenuItem.Text = "parse";
            parseToolStripMenuItem.Click += parseToolStripMenuItem_Click;
            // 
            // showToolStripMenuItem
            // 
            showToolStripMenuItem.Name = "showToolStripMenuItem";
            showToolStripMenuItem.Size = new Size(142, 22);
            showToolStripMenuItem.Text = "show";
            showToolStripMenuItem.Click += showToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(142, 22);
            editToolStripMenuItem.Text = "edit";
            editToolStripMenuItem.Click += editToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(139, 6);
            // 
            // printToPageToolStripMenuItem
            // 
            printToPageToolStripMenuItem.Name = "printToPageToolStripMenuItem";
            printToPageToolStripMenuItem.Size = new Size(142, 22);
            printToPageToolStripMenuItem.Text = "print to page";
            printToPageToolStripMenuItem.Click += printToPageToolStripMenuItem_Click;
            // 
            // toolStripDropDownButton5
            // 
            toolStripDropDownButton5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton5.DropDownItems.AddRange(new ToolStripItem[] { singlePageToolStripMenuItem, allPagesToolStripMenuItem });
            toolStripDropDownButton5.Image = (Image)resources.GetObject("toolStripDropDownButton5.Image");
            toolStripDropDownButton5.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton5.Name = "toolStripDropDownButton5";
            toolStripDropDownButton5.Size = new Size(52, 24);
            toolStripDropDownButton5.Text = "footer";
            // 
            // singlePageToolStripMenuItem
            // 
            singlePageToolStripMenuItem.Name = "singlePageToolStripMenuItem";
            singlePageToolStripMenuItem.Size = new Size(134, 22);
            singlePageToolStripMenuItem.Text = "single page";
            singlePageToolStripMenuItem.Click += singlePageToolStripMenuItem_Click;
            // 
            // allPagesToolStripMenuItem
            // 
            allPagesToolStripMenuItem.Name = "allPagesToolStripMenuItem";
            allPagesToolStripMenuItem.Size = new Size(134, 22);
            allPagesToolStripMenuItem.Text = "all pages";
            allPagesToolStripMenuItem.Click += allPagesToolStripMenuItem_Click;
            // 
            // toolStripButton5
            // 
            toolStripButton5.DropDownItems.AddRange(new ToolStripItem[] { renderTextToolStripMenuItem, fillRectangleToolStripMenuItem });
            toolStripButton5.Image = Properties.Resources.pencil_button;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(66, 24);
            toolStripButton5.Text = "draw";
            // 
            // renderTextToolStripMenuItem
            // 
            renderTextToolStripMenuItem.Name = "renderTextToolStripMenuItem";
            renderTextToolStripMenuItem.Size = new Size(139, 22);
            renderTextToolStripMenuItem.Text = "render text";
            renderTextToolStripMenuItem.Click += renderTextToolStripMenuItem_Click;
            // 
            // fillRectangleToolStripMenuItem
            // 
            fillRectangleToolStripMenuItem.Name = "fillRectangleToolStripMenuItem";
            fillRectangleToolStripMenuItem.Size = new Size(139, 22);
            fillRectangleToolStripMenuItem.Text = "fill rectangle";
            fillRectangleToolStripMenuItem.Click += fillRectangleToolStripMenuItem_Click;
            // 
            // toolStripDropDownButton6
            // 
            toolStripDropDownButton6.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton6.DropDownItems.AddRange(new ToolStripItem[] { nearestToolStripMenuItem, defaultToolStripMenuItem });
            toolStripDropDownButton6.Image = (Image)resources.GetObject("toolStripDropDownButton6.Image");
            toolStripDropDownButton6.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton6.Name = "toolStripDropDownButton6";
            toolStripDropDownButton6.Size = new Size(122, 24);
            toolStripDropDownButton6.Text = "interpolation mode";
            // 
            // nearestToolStripMenuItem
            // 
            nearestToolStripMenuItem.Name = "nearestToolStripMenuItem";
            nearestToolStripMenuItem.Size = new Size(115, 22);
            nearestToolStripMenuItem.Text = "nearest";
            nearestToolStripMenuItem.Click += nearestToolStripMenuItem_Click;
            // 
            // defaultToolStripMenuItem
            // 
            defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            defaultToolStripMenuItem.Size = new Size(115, 22);
            defaultToolStripMenuItem.Text = "smooth";
            defaultToolStripMenuItem.Click += defaultToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2, toolStripStatusLabel3, toolStripProgressBar1 });
            statusStrip1.Location = new Point(0, 397);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 12, 0);
            statusStrip1.Size = new Size(945, 22);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(0, 17);
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(890, 17);
            toolStripStatusLabel2.Spring = true;
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new Size(42, 17);
            toolStripStatusLabel3.Text = "1 / 100";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 16);
            toolStripProgressBar1.Visible = false;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { showImageToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(139, 26);
            // 
            // showImageToolStripMenuItem
            // 
            showImageToolStripMenuItem.Name = "showImageToolStripMenuItem";
            showImageToolStripMenuItem.Size = new Size(138, 22);
            showImageToolStripMenuItem.Text = "show image";
            showImageToolStripMenuItem.Click += showImageToolStripMenuItem_Click;
            // 
            // Editor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(945, 419);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Editor";
            Text = "Editor";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBoxWithInterpolationMode pictureBox1;
        private TableLayoutPanel tableLayoutPanel1;
        private ToolStrip toolStrip1;
        private TrackBar trackBar1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem loadToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem almostWhiteToolStripMenuItem;
        private ToolStripMenuItem almostBlackToolStripMenuItem;
        private ToolStripMenuItem almostNoWhiteHorizontalLinesToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton3;
        private ToolStripMenuItem flyReadToolStripMenuItem;
        private ToolStripMenuItem attachSourceBookToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton4;
        private ToolStripMenuItem parseToolStripMenuItem;
        private ToolStripMenuItem showToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem mirrorReadToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem showImageToolStripMenuItem;
        private ToolStripButton toolStripButton1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem printToPageToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton5;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripMenuItem singlePageToolStripMenuItem;
        private ToolStripMenuItem allPagesToolStripMenuItem;
        private ToolStripMenuItem inverseColorsToolStripMenuItem;
        private ToolStripMenuItem thisPageToolStripMenuItem;
        private ToolStripMenuItem allPagesToolStripMenuItem1;
        private ToolStripMenuItem extractTilesToolStripMenuItem;
        private ToolStripMenuItem compressToolStripMenuItem;
        private ToolStripDropDownButton toolStripButton5;
        private ToolStripMenuItem renderTextToolStripMenuItem;
        private ToolStripMenuItem fillRectangleToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButton6;
        private ToolStripMenuItem nearestToolStripMenuItem;
        private ToolStripMenuItem defaultToolStripMenuItem;
        private ToolStripButton toolStripButton6;
        private ToolStripMenuItem createFromTextToolStripMenuItem;
    }
}