namespace CollecteurDialog
{
    partial class I2BCollecteur
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(I2BCollecteur));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.Start = new System.Windows.Forms.Button();
            this.browse = new System.Windows.Forms.Button();
            this.restar = new System.Windows.Forms.Button();
            this.reduit = new System.Windows.Forms.Button();
            this.exit = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ipAddressControl1 = new IPAddressControlLib.IPAddressControl();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.collecteurPath = new System.Windows.Forms.TextBox();
            this.collecteur = new System.Diagnostics.Process();
            this.consoleOut = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.administrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.demarerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redémarerCollecteurToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "I2BCollecteur";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(2, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(505, 54);
            this.panel1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(246, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(183, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Configuration et  Paramétrage du Collecteur";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Enabled = false;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.textBox1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.textBox1.Location = new System.Drawing.Point(63, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(380, 22);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "I2BCollect  Collecteur de Trames Controle";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::CollecteurDialog.Properties.Resources.CollecteurIcons;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(63, 54);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(425, 61);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(82, 28);
            this.Start.TabIndex = 2;
            this.Start.Text = "Démarer";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // browse
            // 
            this.browse.Location = new System.Drawing.Point(426, 95);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(81, 29);
            this.browse.TabIndex = 3;
            this.browse.Text = "Parcourir...";
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.button2_Click);
            // 
            // restar
            // 
            this.restar.Location = new System.Drawing.Point(426, 130);
            this.restar.Name = "restar";
            this.restar.Size = new System.Drawing.Size(81, 30);
            this.restar.TabIndex = 4;
            this.restar.Text = "Les Traces";
            this.restar.UseVisualStyleBackColor = true;
            this.restar.Click += new System.EventHandler(this.button3_Click);
            // 
            // reduit
            // 
            this.reduit.Location = new System.Drawing.Point(426, 166);
            this.reduit.Name = "reduit";
            this.reduit.Size = new System.Drawing.Size(81, 30);
            this.reduit.TabIndex = 5;
            this.reduit.Text = "Rèduire";
            this.reduit.UseVisualStyleBackColor = true;
            this.reduit.Click += new System.EventHandler(this.button4_Click);
            // 
            // exit
            // 
            this.exit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exit.Location = new System.Drawing.Point(427, 209);
            this.exit.Name = "exit";
            this.exit.Size = new System.Drawing.Size(80, 28);
            this.exit.TabIndex = 6;
            this.exit.Text = "Exit";
            this.exit.UseVisualStyleBackColor = true;
            this.exit.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(248, 6);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(55, 20);
            this.textBox3.TabIndex = 9;
            this.textBox3.Text = "30002";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(213, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Port :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = " Adresse IP";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.ipAddressControl1);
            this.panel2.Controls.Add(this.checkBox1);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.collecteurPath);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.textBox3);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(2, 61);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(417, 69);
            this.panel2.TabIndex = 12;
            // 
            // ipAddressControl1
            // 
            this.ipAddressControl1.AllowInternalTab = false;
            this.ipAddressControl1.AutoHeight = true;
            this.ipAddressControl1.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControl1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControl1.Location = new System.Drawing.Point(78, 6);
            this.ipAddressControl1.MinimumSize = new System.Drawing.Size(87, 20);
            this.ipAddressControl1.Name = "ipAddressControl1";
            this.ipAddressControl1.ReadOnly = false;
            this.ipAddressControl1.Size = new System.Drawing.Size(129, 20);
            this.ipAddressControl1.TabIndex = 16;
            this.ipAddressControl1.Text = "...";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(324, 8);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(58, 17);
            this.checkBox1.TabIndex = 15;
            this.checkBox1.Text = "Debug";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Collecteur";
            // 
            // collecteurPath
            // 
            this.collecteurPath.Location = new System.Drawing.Point(78, 40);
            this.collecteurPath.Name = "collecteurPath";
            this.collecteurPath.ReadOnly = true;
            this.collecteurPath.Size = new System.Drawing.Size(321, 20);
            this.collecteurPath.TabIndex = 12;
            // 
            // collecteur
            // 
            this.collecteur.StartInfo.Domain = "";
            this.collecteur.StartInfo.LoadUserProfile = false;
            this.collecteur.StartInfo.Password = null;
            this.collecteur.StartInfo.StandardErrorEncoding = null;
            this.collecteur.StartInfo.StandardOutputEncoding = null;
            this.collecteur.StartInfo.UserName = "";
            this.collecteur.SynchronizingObject = this;
            // 
            // consoleOut
            // 
            this.consoleOut.BackColor = System.Drawing.SystemColors.WindowText;
            this.consoleOut.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.consoleOut.ForeColor = System.Drawing.SystemColors.Window;
            this.consoleOut.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.consoleOut.Location = new System.Drawing.Point(2, 141);
            this.consoleOut.Multiline = true;
            this.consoleOut.Name = "consoleOut";
            this.consoleOut.ReadOnly = true;
            this.consoleOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleOut.Size = new System.Drawing.Size(416, 96);
            this.consoleOut.TabIndex = 13;
            this.consoleOut.UseSystemPasswordChar = true;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.administrationToolStripMenuItem,
            this.demarerToolStripMenuItem,
            this.redémarerCollecteurToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowCheckMargin = true;
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 92);
            // 
            // administrationToolStripMenuItem
            // 
            this.administrationToolStripMenuItem.Name = "administrationToolStripMenuItem";
            this.administrationToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.administrationToolStripMenuItem.Text = "Ouvrir...";
            this.administrationToolStripMenuItem.Click += new System.EventHandler(this.administrationToolStripMenuItem_Click);
            // 
            // demarerToolStripMenuItem
            // 
            this.demarerToolStripMenuItem.Name = "demarerToolStripMenuItem";
            this.demarerToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.demarerToolStripMenuItem.Text = "Démarer Collecteur";
            this.demarerToolStripMenuItem.Click += new System.EventHandler(this.demarerToolStripMenuItem_Click);
            // 
            // redémarerCollecteurToolStripMenuItem
            // 
            this.redémarerCollecteurToolStripMenuItem.Name = "redémarerCollecteurToolStripMenuItem";
            this.redémarerCollecteurToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.redémarerCollecteurToolStripMenuItem.Text = "Redémarer Collecteur";
            this.redémarerCollecteurToolStripMenuItem.Click += new System.EventHandler(this.redémarerCollecteurToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.button5_Click);
            // 
            // I2BCollecteur
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.CancelButton = this.exit;
            this.ClientSize = new System.Drawing.Size(513, 261);
            this.Controls.Add(this.consoleOut);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.exit);
            this.Controls.Add(this.reduit);
            this.Controls.Add(this.restar);
            this.Controls.Add(this.browse);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.On;
            this.Name = "I2BCollecteur";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "I2BCollecteur Configuration";
            this.Load += new System.EventHandler(this.I2BCollecteur_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.Button restar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button reduit;
        private System.Windows.Forms.Button exit;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox collecteurPath;
        private System.Diagnostics.Process collecteur;
        private System.Windows.Forms.TextBox consoleOut;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem administrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem demarerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redémarerCollecteurToolStripMenuItem;
        private IPAddressControlLib.IPAddressControl ipAddressControl1;
    }
}

