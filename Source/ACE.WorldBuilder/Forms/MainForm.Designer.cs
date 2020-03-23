namespace ACE.WorldBuilder
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menu = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLandblocks = new System.Windows.Forms.ToolStripMenuItem();
            this.menuNpcs = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCreatures = new System.Windows.Forms.ToolStripMenuItem();
            this.menuQuests = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCharacters = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItems = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuLandblocks,
            this.menuNpcs,
            this.menuCreatures,
            this.menuQuests,
            this.menuCharacters,
            this.menuSettings,
            this.menuItems,
            this.menuHelp});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(800, 24);
            this.menu.TabIndex = 1;
            this.menu.Text = "menu";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "&File";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.Size = new System.Drawing.Size(180, 22);
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuLandblocks
            // 
            this.menuLandblocks.Name = "menuLandblocks";
            this.menuLandblocks.Size = new System.Drawing.Size(79, 20);
            this.menuLandblocks.Text = "&Landblocks";
            this.menuLandblocks.Click += new System.EventHandler(this.menuLandblocks_Click);
            // 
            // menuNpcs
            // 
            this.menuNpcs.Name = "menuNpcs";
            this.menuNpcs.Size = new System.Drawing.Size(48, 20);
            this.menuNpcs.Text = "&NPCs";
            this.menuNpcs.Click += new System.EventHandler(this.menuNpcs_Click);
            // 
            // menuCreatures
            // 
            this.menuCreatures.Name = "menuCreatures";
            this.menuCreatures.Size = new System.Drawing.Size(69, 20);
            this.menuCreatures.Text = "C&reatures";
            this.menuCreatures.Click += new System.EventHandler(this.menuCreatures_Click);
            // 
            // menuQuests
            // 
            this.menuQuests.Name = "menuQuests";
            this.menuQuests.Size = new System.Drawing.Size(55, 20);
            this.menuQuests.Text = "&Quests";
            this.menuQuests.Click += new System.EventHandler(this.menuQuests_Click);
            // 
            // menuCharacters
            // 
            this.menuCharacters.Name = "menuCharacters";
            this.menuCharacters.Size = new System.Drawing.Size(75, 20);
            this.menuCharacters.Text = "&Characters";
            this.menuCharacters.Click += new System.EventHandler(this.menuCharacters_Click);
            // 
            // menuSettings
            // 
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(96, 20);
            this.menuSettings.Text = "Server &Settings";
            this.menuSettings.Click += new System.EventHandler(this.menuSettings_Click);
            // 
            // menuItems
            // 
            this.menuItems.Name = "menuItems";
            this.menuItems.Size = new System.Drawing.Size(48, 20);
            this.menuItems.Text = "&Items";
            this.menuItems.Click += new System.EventHandler(this.menuItems_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpAbout});
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(44, 20);
            this.menuHelp.Text = "&Help";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Name = "menuHelpAbout";
            this.menuHelpAbout.Size = new System.Drawing.Size(180, 22);
            this.menuHelpAbout.Text = "&About";
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ACE World Builder";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuLandblocks;
        private System.Windows.Forms.ToolStripMenuItem menuNpcs;
        private System.Windows.Forms.ToolStripMenuItem menuCreatures;
        private System.Windows.Forms.ToolStripMenuItem menuQuests;
        private System.Windows.Forms.ToolStripMenuItem menuCharacters;
        private System.Windows.Forms.ToolStripMenuItem menuSettings;
        private System.Windows.Forms.ToolStripMenuItem menuItems;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
    }
}

