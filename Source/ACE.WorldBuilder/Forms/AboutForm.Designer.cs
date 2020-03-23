namespace ACE.WorldBuilder.Forms
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.DoClose = new System.Windows.Forms.Button();
            this.about = new System.Windows.Forms.Label();
            this.image = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.image)).BeginInit();
            this.SuspendLayout();
            // 
            // DoClose
            // 
            this.DoClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DoClose.Location = new System.Drawing.Point(13, 380);
            this.DoClose.Name = "DoClose";
            this.DoClose.Size = new System.Drawing.Size(641, 30);
            this.DoClose.TabIndex = 0;
            this.DoClose.Text = "Close";
            this.DoClose.UseVisualStyleBackColor = true;
            this.DoClose.Click += new System.EventHandler(this.DoClose_Click);
            // 
            // about
            // 
            this.about.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.about.Location = new System.Drawing.Point(13, 228);
            this.about.Name = "about";
            this.about.Size = new System.Drawing.Size(641, 149);
            this.about.TabIndex = 1;
            this.about.Text = "Something something Danger Zone.";
            // 
            // image
            // 
            this.image.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.image.Image = ((System.Drawing.Image)(resources.GetObject("image.Image")));
            this.image.Location = new System.Drawing.Point(13, 12);
            this.image.Name = "image";
            this.image.Size = new System.Drawing.Size(641, 213);
            this.image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.image.TabIndex = 2;
            this.image.TabStop = false;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 422);
            this.ControlBox = false;
            this.Controls.Add(this.image);
            this.Controls.Add(this.about);
            this.Controls.Add(this.DoClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ACE World Builder Help";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.image)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button DoClose;
        private System.Windows.Forms.Label about;
        private System.Windows.Forms.PictureBox image;
    }
}