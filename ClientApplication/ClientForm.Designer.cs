namespace ClientApplication
{
    partial class ClientForm
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
            menuStrip1 = new MenuStrip();
            файлToolStripMenuItem = new ToolStripMenuItem();
            открытьToolStripMenuItem = new ToolStripMenuItem();
            сохранитьПреобразованноеToolStripMenuItem = new ToolStripMenuItem();
            sendButton = new Button();
            openFileDialog = new OpenFileDialog();
            imagePanel = new Panel();
            filteredPanel = new Panel();
            label1 = new Label();
            label2 = new Label();
            sigmaTextBox = new TextBox();
            radiusTextBox = new TextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { файлToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1213, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { открытьToolStripMenuItem, сохранитьПреобразованноеToolStripMenuItem });
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new Size(62, 24);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // открытьToolStripMenuItem
            // 
            открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            открытьToolStripMenuItem.Size = new Size(275, 26);
            открытьToolStripMenuItem.Text = "Открыть";
            открытьToolStripMenuItem.Click += открытьToolStripMenuItem_Click;
            // 
            // сохранитьПреобразованноеToolStripMenuItem
            // 
            сохранитьПреобразованноеToolStripMenuItem.Name = "сохранитьПреобразованноеToolStripMenuItem";
            сохранитьПреобразованноеToolStripMenuItem.Size = new Size(275, 26);
            сохранитьПреобразованноеToolStripMenuItem.Text = "Сохранить обработанное";
            // 
            // sendButton
            // 
            sendButton.Font = new Font("Segoe UI", 19.8000011F, FontStyle.Bold);
            sendButton.Location = new Point(565, 257);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(80, 69);
            sendButton.TabIndex = 3;
            sendButton.Text = "→";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += sendButton_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog";
            openFileDialog.Filter = "Jpg картинки|*.jpg|Все файлы|*.*";
            openFileDialog.Multiselect = true;
            // 
            // imagePanel
            // 
            imagePanel.AutoScroll = true;
            imagePanel.BackColor = SystemColors.ActiveBorder;
            imagePanel.Location = new Point(12, 76);
            imagePanel.Name = "imagePanel";
            imagePanel.Size = new Size(550, 486);
            imagePanel.TabIndex = 4;
            // 
            // filteredPanel
            // 
            filteredPanel.AutoScroll = true;
            filteredPanel.BackColor = SystemColors.ActiveBorder;
            filteredPanel.Location = new Point(651, 76);
            filteredPanel.Name = "filteredPanel";
            filteredPanel.Size = new Size(550, 486);
            filteredPanel.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 41);
            label1.Name = "label1";
            label1.Size = new Size(57, 20);
            label1.TabIndex = 6;
            label1.Text = "Сигма:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(254, 41);
            label2.Name = "label2";
            label2.Size = new Size(63, 20);
            label2.TabIndex = 7;
            label2.Text = "Радиус:";
            label2.Click += label2_Click;
            // 
            // sigmaTextBox
            // 
            sigmaTextBox.Location = new Point(75, 41);
            sigmaTextBox.Name = "sigmaTextBox";
            sigmaTextBox.Size = new Size(173, 27);
            sigmaTextBox.TabIndex = 8;
            // 
            // radiusTextBox
            // 
            radiusTextBox.Location = new Point(323, 41);
            radiusTextBox.Name = "radiusTextBox";
            radiusTextBox.Size = new Size(210, 27);
            radiusTextBox.TabIndex = 9;
            // 
            // ClientForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1213, 582);
            Controls.Add(radiusTextBox);
            Controls.Add(sigmaTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(filteredPanel);
            Controls.Add(imagePanel);
            Controls.Add(sendButton);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "ClientForm";
            Text = "Клиентская сторона";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem открытьToolStripMenuItem;
        private ToolStripMenuItem сохранитьПреобразованноеToolStripMenuItem;
        private Button sendButton;
        private OpenFileDialog openFileDialog;
        private Panel imagePanel;
        private Panel filteredPanel;
        private Label label1;
        private Label label2;
        private TextBox sigmaTextBox;
        private TextBox radiusTextBox;
    }
}