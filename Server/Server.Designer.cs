namespace Server
{
    partial class Server
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.listBoxClients = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbtennguoidung = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.flowLayoutChat = new System.Windows.Forms.FlowLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnShowEmojiServer = new System.Windows.Forms.Button();
            this.btnDelServer = new System.Windows.Forms.Button();
            this.btnSendImageServer = new System.Windows.Forms.Button();
            this.btnSendServer = new System.Windows.Forms.Button();
            this.txtnhaptinnhanServer = new System.Windows.Forms.TextBox();
            this.btnSendFileServer = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Location = new System.Drawing.Point(4, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(170, 365);
            this.panel1.TabIndex = 6;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Aqua;
            this.panel3.Controls.Add(this.listBoxClients);
            this.panel3.Location = new System.Drawing.Point(3, 39);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(165, 323);
            this.panel3.TabIndex = 1;
            // 
            // listBoxClients
            // 
            this.listBoxClients.BackColor = System.Drawing.Color.LightCyan;
            this.listBoxClients.FormattingEnabled = true;
            this.listBoxClients.Location = new System.Drawing.Point(1, 3);
            this.listBoxClients.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxClients.Name = "listBoxClients";
            this.listBoxClients.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxClients.Size = new System.Drawing.Size(161, 316);
            this.listBoxClients.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Blue;
            this.panel2.Controls.Add(this.lbtennguoidung);
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(165, 32);
            this.panel2.TabIndex = 0;
            // 
            // lbtennguoidung
            // 
            this.lbtennguoidung.AutoSize = true;
            this.lbtennguoidung.Font = new System.Drawing.Font("Times New Roman", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbtennguoidung.ForeColor = System.Drawing.Color.Red;
            this.lbtennguoidung.Location = new System.Drawing.Point(3, 3);
            this.lbtennguoidung.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbtennguoidung.Name = "lbtennguoidung";
            this.lbtennguoidung.Size = new System.Drawing.Size(64, 22);
            this.lbtennguoidung.TabIndex = 0;
            this.lbtennguoidung.Text = "Server";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Aqua;
            this.panel4.Controls.Add(this.flowLayoutChat);
            this.panel4.Location = new System.Drawing.Point(177, 6);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(371, 242);
            this.panel4.TabIndex = 7;
            // 
            // flowLayoutChat
            // 
            this.flowLayoutChat.BackColor = System.Drawing.Color.White;
            this.flowLayoutChat.Location = new System.Drawing.Point(2, 3);
            this.flowLayoutChat.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutChat.Name = "flowLayoutChat";
            this.flowLayoutChat.Size = new System.Drawing.Size(367, 236);
            this.flowLayoutChat.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Cyan;
            this.panel5.Controls.Add(this.btnShowEmojiServer);
            this.panel5.Controls.Add(this.btnDelServer);
            this.panel5.Controls.Add(this.btnSendImageServer);
            this.panel5.Controls.Add(this.btnSendServer);
            this.panel5.Controls.Add(this.txtnhaptinnhanServer);
            this.panel5.Controls.Add(this.btnSendFileServer);
            this.panel5.Location = new System.Drawing.Point(177, 254);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(371, 111);
            this.panel5.TabIndex = 8;
            // 
            // btnShowEmojiServer
            // 
            this.btnShowEmojiServer.Image = ((System.Drawing.Image)(resources.GetObject("btnShowEmojiServer.Image")));
            this.btnShowEmojiServer.Location = new System.Drawing.Point(184, 44);
            this.btnShowEmojiServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnShowEmojiServer.Name = "btnShowEmojiServer";
            this.btnShowEmojiServer.Size = new System.Drawing.Size(56, 44);
            this.btnShowEmojiServer.TabIndex = 2;
            this.btnShowEmojiServer.UseVisualStyleBackColor = true;
            this.btnShowEmojiServer.Click += new System.EventHandler(this.btnShowEmojiServer_Click);
            // 
            // btnDelServer
            // 
            this.btnDelServer.Image = ((System.Drawing.Image)(resources.GetObject("btnDelServer.Image")));
            this.btnDelServer.Location = new System.Drawing.Point(4, 44);
            this.btnDelServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelServer.Name = "btnDelServer";
            this.btnDelServer.Size = new System.Drawing.Size(56, 43);
            this.btnDelServer.TabIndex = 5;
            this.btnDelServer.UseVisualStyleBackColor = true;
            this.btnDelServer.Click += new System.EventHandler(this.btnDelServer_Click);
            // 
            // btnSendImageServer
            // 
            this.btnSendImageServer.Image = ((System.Drawing.Image)(resources.GetObject("btnSendImageServer.Image")));
            this.btnSendImageServer.Location = new System.Drawing.Point(124, 44);
            this.btnSendImageServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnSendImageServer.Name = "btnSendImageServer";
            this.btnSendImageServer.Size = new System.Drawing.Size(56, 44);
            this.btnSendImageServer.TabIndex = 3;
            this.btnSendImageServer.UseVisualStyleBackColor = true;
            this.btnSendImageServer.Click += new System.EventHandler(this.btnSendImageServer_Click);
            // 
            // btnSendServer
            // 
            this.btnSendServer.Image = ((System.Drawing.Image)(resources.GetObject("btnSendServer.Image")));
            this.btnSendServer.Location = new System.Drawing.Point(313, 3);
            this.btnSendServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnSendServer.Name = "btnSendServer";
            this.btnSendServer.Size = new System.Drawing.Size(56, 37);
            this.btnSendServer.TabIndex = 1;
            this.btnSendServer.UseVisualStyleBackColor = true;
            this.btnSendServer.Click += new System.EventHandler(this.btnSendServer_Click);
            // 
            // txtnhaptinnhanServer
            // 
            this.txtnhaptinnhanServer.BackColor = System.Drawing.Color.White;
            this.txtnhaptinnhanServer.Location = new System.Drawing.Point(4, 2);
            this.txtnhaptinnhanServer.Margin = new System.Windows.Forms.Padding(2);
            this.txtnhaptinnhanServer.Multiline = true;
            this.txtnhaptinnhanServer.Name = "txtnhaptinnhanServer";
            this.txtnhaptinnhanServer.Size = new System.Drawing.Size(307, 38);
            this.txtnhaptinnhanServer.TabIndex = 0;
            // 
            // btnSendFileServer
            // 
            this.btnSendFileServer.Image = ((System.Drawing.Image)(resources.GetObject("btnSendFileServer.Image")));
            this.btnSendFileServer.Location = new System.Drawing.Point(64, 44);
            this.btnSendFileServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnSendFileServer.Name = "btnSendFileServer";
            this.btnSendFileServer.Size = new System.Drawing.Size(56, 44);
            this.btnSendFileServer.TabIndex = 4;
            this.btnSendFileServer.UseVisualStyleBackColor = true;
            this.btnSendFileServer.Click += new System.EventHandler(this.btnSendFileServer_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 373);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel5);
            this.Name = "Server";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ListBox listBoxClients;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutChat;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnDelServer;
        private System.Windows.Forms.Button btnSendFileServer;
        private System.Windows.Forms.Button btnSendImageServer;
        private System.Windows.Forms.Button btnShowEmojiServer;
        private System.Windows.Forms.Button btnSendServer;
        private System.Windows.Forms.TextBox txtnhaptinnhanServer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lbtennguoidung;
    }
}

