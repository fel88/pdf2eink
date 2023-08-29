namespace pdf2eink
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            linkLabel1 = new LinkLabel();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(85, 15);
            label1.TabIndex = 0;
            label1.Text = "DjVu libre path";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(121, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(357, 23);
            textBox1.TabIndex = 1;
            textBox1.Text = "C:\\Program Files (x86)\\DjVuLibre";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(121, 32);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(341, 15);
            linkLabel1.TabIndex = 2;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "http://sourceforge.net/projects/djvu/files/DjVuLibre_Windows/";
            // 
            // button1
            // 
            button1.Location = new Point(202, 74);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 3;
            button1.Text = "Ok";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(488, 109);
            Controls.Add(button1);
            Controls.Add(linkLabel1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "Settings";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private LinkLabel linkLabel1;
        private Button button1;
    }
}