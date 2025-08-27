namespace WinFormsApp1
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
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            Convert = new Button();
            print = new Button();
            EncodingList = new ComboBox();
            PrintALL = new Button();
            tableLayoutPanel4 = new TableLayoutPanel();
            Printers = new ComboBox();
            Сonnection = new Button();
            Status = new CheckBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5.20953941F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 89.5833359F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5.27777767F));
            tableLayoutPanel1.Size = new Size(553, 739);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(textBox1, 0, 0);
            tableLayoutPanel2.Controls.Add(textBox2, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 41);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 47.826088F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 52.173912F));
            tableLayoutPanel2.Size = new Size(547, 655);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(3, 3);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(541, 307);
            textBox1.TabIndex = 0;
            // 
            // textBox2
            // 
            textBox2.Dock = DockStyle.Fill;
            textBox2.Location = new Point(3, 316);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(541, 336);
            textBox2.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 4;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22.2222214F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.54296F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15.72212F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.4314442F));
            tableLayoutPanel3.Controls.Add(Convert, 0, 0);
            tableLayoutPanel3.Controls.Add(print, 2, 0);
            tableLayoutPanel3.Controls.Add(EncodingList, 1, 0);
            tableLayoutPanel3.Controls.Add(PrintALL, 3, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(547, 32);
            tableLayoutPanel3.TabIndex = 1;
            tableLayoutPanel3.Paint += tableLayoutPanel3_Paint;
            // 
            // Convert
            // 
            Convert.Dock = DockStyle.Fill;
            Convert.Location = new Point(3, 3);
            Convert.Name = "Convert";
            Convert.Size = new Size(115, 26);
            Convert.TabIndex = 0;
            Convert.Text = "Конвертировать";
            Convert.UseVisualStyleBackColor = true;
            Convert.Click += Convert_Click;
            // 
            // print
            // 
            print.Dock = DockStyle.Fill;
            print.Location = new Point(395, 3);
            print.Name = "print";
            print.Size = new Size(80, 26);
            print.TabIndex = 1;
            print.Text = "Печать";
            print.UseVisualStyleBackColor = true;
            print.Click += print_Click;
            // 
            // EncodingList
            // 
            EncodingList.Dock = DockStyle.Fill;
            EncodingList.FormattingEnabled = true;
            EncodingList.Location = new Point(124, 3);
            EncodingList.Name = "EncodingList";
            EncodingList.Size = new Size(265, 23);
            EncodingList.TabIndex = 2;
            // 
            // PrintALL
            // 
            PrintALL.Dock = DockStyle.Fill;
            PrintALL.Location = new Point(481, 3);
            PrintALL.Name = "PrintALL";
            PrintALL.Size = new Size(63, 26);
            PrintALL.TabIndex = 3;
            PrintALL.Text = "Все";
            PrintALL.UseVisualStyleBackColor = true;
            PrintALL.Click += PrintALL_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64.45087F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35.5491333F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 73F));
            tableLayoutPanel4.Controls.Add(Printers, 0, 0);
            tableLayoutPanel4.Controls.Add(Сonnection, 1, 0);
            tableLayoutPanel4.Controls.Add(Status, 2, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 702);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(547, 34);
            tableLayoutPanel4.TabIndex = 2;
            // 
            // Printers
            // 
            Printers.Dock = DockStyle.Fill;
            Printers.FormattingEnabled = true;
            Printers.Location = new Point(3, 3);
            Printers.Name = "Printers";
            Printers.Size = new Size(299, 23);
            Printers.TabIndex = 0;
            // 
            // Сonnection
            // 
            Сonnection.Dock = DockStyle.Fill;
            Сonnection.Location = new Point(308, 3);
            Сonnection.Name = "Сonnection";
            Сonnection.Size = new Size(162, 28);
            Сonnection.TabIndex = 1;
            Сonnection.Text = "Подключение";
            Сonnection.UseVisualStyleBackColor = true;
            Сonnection.Click += Сonnection_Click;
            // 
            // Status
            // 
            Status.AutoSize = true;
            Status.Dock = DockStyle.Fill;
            Status.Enabled = false;
            Status.Location = new Point(476, 3);
            Status.Name = "Status";
            Status.Size = new Size(68, 28);
            Status.TabIndex = 2;
            Status.Text = "Статус";
            Status.UseVisualStyleBackColor = true;
            Status.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(553, 739);
            Controls.Add(tableLayoutPanel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TextBox textBox1;
        private TextBox textBox2;
        private TableLayoutPanel tableLayoutPanel3;
        private Button Convert;
        private Button print;
        private ComboBox EncodingList;
        private TableLayoutPanel tableLayoutPanel4;
        private ComboBox Printers;
        private Button Сonnection;
        private CheckBox Status;
        private Button PrintALL;
    }
}
