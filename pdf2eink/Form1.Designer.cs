namespace pdf2eink
{
    partial class Form1
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
            button1 = new Button();
            progressBar1 = new ProgressBar();
            checkBox1 = new CheckBox();
            button2 = new Button();
            pictureBox1 = new PictureBox();
            numericUpDown1 = new NumericUpDown();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            label2 = new Label();
            numericUpDown6 = new NumericUpDown();
            numericUpDown5 = new NumericUpDown();
            checkBox5 = new CheckBox();
            numericUpDown4 = new NumericUpDown();
            numericUpDown3 = new NumericUpDown();
            checkBox4 = new CheckBox();
            label1 = new Label();
            numericUpDown2 = new NumericUpDown();
            checkBox3 = new CheckBox();
            cbPreviewOnly = new CheckBox();
            cbRenderPageInfo = new CheckBox();
            checkBox2 = new CheckBox();
            checkBox6 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(9, 11);
            button1.Margin = new Padding(3, 2, 3, 2);
            button1.Name = "button1";
            button1.Size = new Size(103, 22);
            button1.TabIndex = 0;
            button1.Text = "Export";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(9, 38);
            progressBar1.Margin = new Padding(3, 2, 3, 2);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(270, 22);
            progressBar1.TabIndex = 1;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new Point(118, 15);
            checkBox1.Margin = new Padding(3, 2, 3, 2);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(105, 19);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "internal format";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // button2
            // 
            button2.Location = new Point(33, 64);
            button2.Margin = new Padding(3, 2, 3, 2);
            button2.Name = "button2";
            button2.Size = new Size(101, 22);
            button2.TabIndex = 3;
            button2.Text = "show preview";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(3, 162);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(484, 211);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(129, 65);
            numericUpDown1.Margin = new Padding(3, 2, 3, 2);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(42, 23);
            numericUpDown1.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 160F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(490, 375);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // panel1
            // 
            panel1.Controls.Add(checkBox6);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(numericUpDown6);
            panel1.Controls.Add(numericUpDown5);
            panel1.Controls.Add(checkBox5);
            panel1.Controls.Add(numericUpDown4);
            panel1.Controls.Add(numericUpDown3);
            panel1.Controls.Add(checkBox4);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(numericUpDown2);
            panel1.Controls.Add(checkBox3);
            panel1.Controls.Add(cbPreviewOnly);
            panel1.Controls.Add(cbRenderPageInfo);
            panel1.Controls.Add(checkBox2);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(numericUpDown1);
            panel1.Controls.Add(progressBar1);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(checkBox1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(484, 154);
            panel1.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(336, 119);
            label2.Name = "label2";
            label2.Size = new Size(24, 15);
            label2.TabIndex = 18;
            label2.Text = "dpi";
            // 
            // numericUpDown6
            // 
            numericUpDown6.Location = new Point(399, 117);
            numericUpDown6.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            numericUpDown6.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDown6.Name = "numericUpDown6";
            numericUpDown6.Size = new Size(76, 23);
            numericUpDown6.TabIndex = 17;
            numericUpDown6.Value = new decimal(new int[] { 300, 0, 0, 0 });
            numericUpDown6.ValueChanged += numericUpDown6_ValueChanged;
            // 
            // numericUpDown5
            // 
            numericUpDown5.Enabled = false;
            numericUpDown5.Location = new Point(188, 118);
            numericUpDown5.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDown5.Name = "numericUpDown5";
            numericUpDown5.Size = new Size(76, 23);
            numericUpDown5.TabIndex = 16;
            numericUpDown5.Value = new decimal(new int[] { 120, 0, 0, 0 });
            numericUpDown5.ValueChanged += numericUpDown5_ValueChanged;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Location = new Point(9, 122);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new Size(178, 19);
            checkBox5.TabIndex = 15;
            checkBox5.Text = "split when aspect greater (%)";
            checkBox5.UseVisualStyleBackColor = true;
            checkBox5.CheckedChanged += checkBox5_CheckedChanged;
            // 
            // numericUpDown4
            // 
            numericUpDown4.Enabled = false;
            numericUpDown4.Location = new Point(177, 94);
            numericUpDown4.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numericUpDown4.Name = "numericUpDown4";
            numericUpDown4.Size = new Size(76, 23);
            numericUpDown4.TabIndex = 14;
            numericUpDown4.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDown4.ValueChanged += numericUpDown4_ValueChanged;
            // 
            // numericUpDown3
            // 
            numericUpDown3.Enabled = false;
            numericUpDown3.Location = new Point(95, 93);
            numericUpDown3.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new Size(76, 23);
            numericUpDown3.TabIndex = 13;
            numericUpDown3.ValueChanged += numericUpDown3_ValueChanged;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Location = new Point(9, 95);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(84, 19);
            checkBox4.TabIndex = 12;
            checkBox4.Text = "pages limit";
            checkBox4.UseVisualStyleBackColor = true;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(336, 90);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 11;
            label1.Text = "gray level";
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(399, 88);
            numericUpDown2.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(76, 23);
            numericUpDown2.TabIndex = 10;
            numericUpDown2.Value = new decimal(new int[] { 200, 0, 0, 0 });
            numericUpDown2.ValueChanged += numericUpDown2_ValueChanged;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Checked = true;
            checkBox3.CheckState = CheckState.Checked;
            checkBox3.Location = new Point(300, 41);
            checkBox3.Margin = new Padding(3, 2, 3, 2);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(95, 19);
            checkBox3.TabIndex = 9;
            checkBox3.Text = "wear leveling";
            checkBox3.UseVisualStyleBackColor = true;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            // 
            // cbPreviewOnly
            // 
            cbPreviewOnly.AutoSize = true;
            cbPreviewOnly.Location = new Point(348, 11);
            cbPreviewOnly.Name = "cbPreviewOnly";
            cbPreviewOnly.Size = new Size(93, 19);
            cbPreviewOnly.TabIndex = 8;
            cbPreviewOnly.Text = "preview only";
            cbPreviewOnly.UseVisualStyleBackColor = true;
            cbPreviewOnly.CheckedChanged += cbPreviewOnly_CheckedChanged;
            // 
            // cbRenderPageInfo
            // 
            cbRenderPageInfo.AutoSize = true;
            cbRenderPageInfo.Checked = true;
            cbRenderPageInfo.CheckState = CheckState.Checked;
            cbRenderPageInfo.Location = new Point(229, 14);
            cbRenderPageInfo.Margin = new Padding(3, 2, 3, 2);
            cbRenderPageInfo.Name = "cbRenderPageInfo";
            cbRenderPageInfo.Size = new Size(113, 19);
            cbRenderPageInfo.TabIndex = 7;
            cbRenderPageInfo.Text = "render page info";
            cbRenderPageInfo.UseVisualStyleBackColor = true;
            cbRenderPageInfo.CheckedChanged += cbRenderPageInfo_CheckedChanged;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(188, 69);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(95, 19);
            checkBox2.TabIndex = 6;
            checkBox2.Text = "use dithering";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Location = new Point(348, 63);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new Size(124, 19);
            checkBox6.TabIndex = 19;
            checkBox6.Text = "adaptive threshold";
            checkBox6.UseVisualStyleBackColor = true;
            checkBox6.CheckedChanged += checkBox6_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(490, 375);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Export";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown6).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown5).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private ProgressBar progressBar1;
        private CheckBox checkBox1;
        private Button button2;
        private PictureBox pictureBox1;
        private NumericUpDown numericUpDown1;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private CheckBox checkBox2;
        private CheckBox cbRenderPageInfo;
        private CheckBox cbPreviewOnly;
        private CheckBox checkBox3;
        private Label label1;
        private NumericUpDown numericUpDown2;
        private NumericUpDown numericUpDown4;
        private NumericUpDown numericUpDown3;
        private CheckBox checkBox4;
        private NumericUpDown numericUpDown5;
        private CheckBox checkBox5;
        private Label label2;
        private NumericUpDown numericUpDown6;
        private CheckBox checkBox6;
    }
}