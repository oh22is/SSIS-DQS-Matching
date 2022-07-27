namespace oh22is.SqlServer.DQS
{
    partial class FrmMatchingUi
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMatchingUi));
            this.label1 = new System.Windows.Forms.Label();
            this.tabProperties = new System.Windows.Forms.TabControl();
            this.tpConnectionManager = new System.Windows.Forms.TabPage();
            this.tvMatchingRules = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.lblInfoMatchingRules = new System.Windows.Forms.Label();
            this.btnNew = new System.Windows.Forms.Button();
            this.cbDQKnowledgeBase = new System.Windows.Forms.ComboBox();
            this.cbDQConnectionManager = new System.Windows.Forms.ComboBox();
            this.lblDQKnowledgeBase = new System.Windows.Forms.Label();
            this.lblDQConnectionManager = new System.Windows.Forms.Label();
            this.lblInfoMapping = new System.Windows.Forms.Label();
            this.lblInfoConnectionManager = new System.Windows.Forms.Label();
            this.tpMapping = new System.Windows.Forms.TabPage();
            this.dgvInputColumns = new System.Windows.Forms.DataGridView();
            this.dgvMapping = new System.Windows.Forms.DataGridView();
            this.tpAdvanced = new System.Windows.Forms.TabPage();
            this.cbResultSet = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMinimalMatchingScore = new System.Windows.Forms.Label();
            this.cbCleanDQSProjects = new System.Windows.Forms.CheckBox();
            this.lblScoreValue = new System.Windows.Forms.Label();
            this.trackBar = new System.Windows.Forms.TrackBar();
            this.cbEncryptConnection = new System.Windows.Forms.CheckBox();
            this.tpInformation = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblCodeplex = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.linkOH22 = new System.Windows.Forms.LinkLabel();
            this.linkCodeplex = new System.Windows.Forms.LinkLabel();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabProperties.SuspendLayout();
            this.tpConnectionManager.SuspendLayout();
            this.tpMapping.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).BeginInit();
            this.tpAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).BeginInit();
            this.tpInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Configure the properties used to match the input data.";
            // 
            // tabProperties
            // 
            this.tabProperties.Controls.Add(this.tpConnectionManager);
            this.tabProperties.Controls.Add(this.tpMapping);
            this.tabProperties.Controls.Add(this.tpAdvanced);
            this.tabProperties.Controls.Add(this.tpInformation);
            this.tabProperties.Location = new System.Drawing.Point(15, 35);
            this.tabProperties.Name = "tabProperties";
            this.tabProperties.SelectedIndex = 0;
            this.tabProperties.Size = new System.Drawing.Size(718, 449);
            this.tabProperties.TabIndex = 1;
            // 
            // tpConnectionManager
            // 
            this.tpConnectionManager.Controls.Add(this.tvMatchingRules);
            this.tpConnectionManager.Controls.Add(this.lblInfoMatchingRules);
            this.tpConnectionManager.Controls.Add(this.btnNew);
            this.tpConnectionManager.Controls.Add(this.cbDQKnowledgeBase);
            this.tpConnectionManager.Controls.Add(this.cbDQConnectionManager);
            this.tpConnectionManager.Controls.Add(this.lblDQKnowledgeBase);
            this.tpConnectionManager.Controls.Add(this.lblDQConnectionManager);
            this.tpConnectionManager.Controls.Add(this.lblInfoMapping);
            this.tpConnectionManager.Controls.Add(this.lblInfoConnectionManager);
            this.tpConnectionManager.Location = new System.Drawing.Point(4, 22);
            this.tpConnectionManager.Name = "tpConnectionManager";
            this.tpConnectionManager.Padding = new System.Windows.Forms.Padding(3);
            this.tpConnectionManager.Size = new System.Drawing.Size(710, 423);
            this.tpConnectionManager.TabIndex = 0;
            this.tpConnectionManager.Text = "Connection Manager";
            this.tpConnectionManager.UseVisualStyleBackColor = true;
            // 
            // tvMatchingRules
            // 
            this.tvMatchingRules.ImageIndex = 0;
            this.tvMatchingRules.ImageList = this.imageList;
            this.tvMatchingRules.Location = new System.Drawing.Point(9, 134);
            this.tvMatchingRules.Name = "tvMatchingRules";
            this.tvMatchingRules.SelectedImageIndex = 0;
            this.tvMatchingRules.Size = new System.Drawing.Size(694, 283);
            this.tvMatchingRules.TabIndex = 11;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "DQS_ICO16.png");
            this.imageList.Images.SetKeyName(1, "domain_16.png");
            this.imageList.Images.SetKeyName(2, "cd.png");
            // 
            // lblInfoMatchingRules
            // 
            this.lblInfoMatchingRules.AutoSize = true;
            this.lblInfoMatchingRules.Location = new System.Drawing.Point(6, 118);
            this.lblInfoMatchingRules.Name = "lblInfoMatchingRules";
            this.lblInfoMatchingRules.Size = new System.Drawing.Size(195, 13);
            this.lblInfoMatchingRules.TabIndex = 10;
            this.lblInfoMatchingRules.Text = "Available Matching Rules and Domains:";
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(609, 50);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(94, 23);
            this.btnNew.TabIndex = 8;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // cbDQKnowledgeBase
            // 
            this.cbDQKnowledgeBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDQKnowledgeBase.FormattingEnabled = true;
            this.cbDQKnowledgeBase.Location = new System.Drawing.Point(182, 79);
            this.cbDQKnowledgeBase.Name = "cbDQKnowledgeBase";
            this.cbDQKnowledgeBase.Size = new System.Drawing.Size(421, 21);
            this.cbDQKnowledgeBase.TabIndex = 7;
            this.cbDQKnowledgeBase.SelectedIndexChanged += new System.EventHandler(this.cbDQKnowledgeBase_SelectedIndexChanged);
            // 
            // cbDQConnectionManager
            // 
            this.cbDQConnectionManager.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDQConnectionManager.FormattingEnabled = true;
            this.cbDQConnectionManager.Location = new System.Drawing.Point(182, 52);
            this.cbDQConnectionManager.Name = "cbDQConnectionManager";
            this.cbDQConnectionManager.Size = new System.Drawing.Size(421, 21);
            this.cbDQConnectionManager.TabIndex = 6;
            this.cbDQConnectionManager.SelectedIndexChanged += new System.EventHandler(this.cbDQConnectionManager_SelectedIndexChanged);
            // 
            // lblDQKnowledgeBase
            // 
            this.lblDQKnowledgeBase.AutoSize = true;
            this.lblDQKnowledgeBase.Location = new System.Drawing.Point(6, 82);
            this.lblDQKnowledgeBase.Name = "lblDQKnowledgeBase";
            this.lblDQKnowledgeBase.Size = new System.Drawing.Size(151, 13);
            this.lblDQKnowledgeBase.TabIndex = 5;
            this.lblDQKnowledgeBase.Text = "Data Quality Knowledge Base:";
            // 
            // lblDQConnectionManager
            // 
            this.lblDQConnectionManager.AutoSize = true;
            this.lblDQConnectionManager.Location = new System.Drawing.Point(6, 55);
            this.lblDQConnectionManager.Name = "lblDQConnectionManager";
            this.lblDQConnectionManager.Size = new System.Drawing.Size(170, 13);
            this.lblDQConnectionManager.TabIndex = 4;
            this.lblDQConnectionManager.Text = "Data Quality Connection Manager:";
            // 
            // lblInfoMapping
            // 
            this.lblInfoMapping.AutoSize = true;
            this.lblInfoMapping.Location = new System.Drawing.Point(6, 33);
            this.lblInfoMapping.Name = "lblInfoMapping";
            this.lblInfoMapping.Size = new System.Drawing.Size(240, 13);
            this.lblInfoMapping.TabIndex = 3;
            this.lblInfoMapping.Text = "Select a data quality matching policy for mapping.";
            // 
            // lblInfoConnectionManager
            // 
            this.lblInfoConnectionManager.AutoSize = true;
            this.lblInfoConnectionManager.Location = new System.Drawing.Point(6, 11);
            this.lblInfoConnectionManager.Name = "lblInfoConnectionManager";
            this.lblInfoConnectionManager.Size = new System.Drawing.Size(461, 13);
            this.lblInfoConnectionManager.TabIndex = 2;
            this.lblInfoConnectionManager.Text = "Select an existing connection manager from the list, or create a new connection b" +
    "y clicking new.";
            // 
            // tpMapping
            // 
            this.tpMapping.Controls.Add(this.dgvInputColumns);
            this.tpMapping.Controls.Add(this.dgvMapping);
            this.tpMapping.Location = new System.Drawing.Point(4, 22);
            this.tpMapping.Name = "tpMapping";
            this.tpMapping.Padding = new System.Windows.Forms.Padding(3);
            this.tpMapping.Size = new System.Drawing.Size(710, 423);
            this.tpMapping.TabIndex = 1;
            this.tpMapping.Text = "Mapping";
            this.tpMapping.UseVisualStyleBackColor = true;
            // 
            // dgvInputColumns
            // 
            this.dgvInputColumns.AllowUserToAddRows = false;
            this.dgvInputColumns.AllowUserToDeleteRows = false;
            this.dgvInputColumns.AllowUserToResizeColumns = false;
            this.dgvInputColumns.AllowUserToResizeRows = false;
            this.dgvInputColumns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInputColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInputColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInputColumns.Location = new System.Drawing.Point(3, 3);
            this.dgvInputColumns.Name = "dgvInputColumns";
            this.dgvInputColumns.RowHeadersVisible = false;
            this.dgvInputColumns.Size = new System.Drawing.Size(704, 191);
            this.dgvInputColumns.TabIndex = 1;
            this.dgvInputColumns.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvInputColumns_CellContentClick);
            // 
            // dgvMapping
            // 
            this.dgvMapping.AllowUserToAddRows = false;
            this.dgvMapping.AllowUserToDeleteRows = false;
            this.dgvMapping.AllowUserToResizeColumns = false;
            this.dgvMapping.AllowUserToResizeRows = false;
            this.dgvMapping.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMapping.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvMapping.Location = new System.Drawing.Point(3, 194);
            this.dgvMapping.Name = "dgvMapping";
            this.dgvMapping.RowHeadersVisible = false;
            this.dgvMapping.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvMapping.Size = new System.Drawing.Size(704, 226);
            this.dgvMapping.TabIndex = 0;
            // 
            // tpAdvanced
            // 
            this.tpAdvanced.BackColor = System.Drawing.SystemColors.Control;
            this.tpAdvanced.Controls.Add(this.cbResultSet);
            this.tpAdvanced.Controls.Add(this.label2);
            this.tpAdvanced.Controls.Add(this.lblMinimalMatchingScore);
            this.tpAdvanced.Controls.Add(this.cbCleanDQSProjects);
            this.tpAdvanced.Controls.Add(this.lblScoreValue);
            this.tpAdvanced.Controls.Add(this.trackBar);
            this.tpAdvanced.Controls.Add(this.cbEncryptConnection);
            this.tpAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tpAdvanced.Name = "tpAdvanced";
            this.tpAdvanced.Size = new System.Drawing.Size(710, 423);
            this.tpAdvanced.TabIndex = 2;
            this.tpAdvanced.Text = "Advanced";
            // 
            // cbResultSet
            // 
            this.cbResultSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResultSet.FormattingEnabled = true;
            this.cbResultSet.Items.AddRange(new object[] {
            "None",
            "Raw",
            "Transitive"});
            this.cbResultSet.Location = new System.Drawing.Point(144, 63);
            this.cbResultSet.Name = "cbResultSet";
            this.cbResultSet.Size = new System.Drawing.Size(143, 21);
            this.cbResultSet.TabIndex = 28;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "Result:";
            // 
            // lblMinimalMatchingScore
            // 
            this.lblMinimalMatchingScore.AutoSize = true;
            this.lblMinimalMatchingScore.Location = new System.Drawing.Point(12, 27);
            this.lblMinimalMatchingScore.Name = "lblMinimalMatchingScore";
            this.lblMinimalMatchingScore.Size = new System.Drawing.Size(123, 13);
            this.lblMinimalMatchingScore.TabIndex = 26;
            this.lblMinimalMatchingScore.Text = "Minimal Matching Score:";
            // 
            // cbCleanDQSProjects
            // 
            this.cbCleanDQSProjects.AutoSize = true;
            this.cbCleanDQSProjects.Checked = true;
            this.cbCleanDQSProjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCleanDQSProjects.Enabled = false;
            this.cbCleanDQSProjects.Location = new System.Drawing.Point(15, 115);
            this.cbCleanDQSProjects.Name = "cbCleanDQSProjects";
            this.cbCleanDQSProjects.Size = new System.Drawing.Size(199, 17);
            this.cbCleanDQSProjects.TabIndex = 25;
            this.cbCleanDQSProjects.Text = "Cleanup DQS project after execution";
            this.cbCleanDQSProjects.UseVisualStyleBackColor = true;
            // 
            // lblScoreValue
            // 
            this.lblScoreValue.AutoSize = true;
            this.lblScoreValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScoreValue.Location = new System.Drawing.Point(141, 27);
            this.lblScoreValue.Name = "lblScoreValue";
            this.lblScoreValue.Size = new System.Drawing.Size(27, 13);
            this.lblScoreValue.TabIndex = 24;
            this.lblScoreValue.Text = "80%";
            this.lblScoreValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trackBar
            // 
            this.trackBar.BackColor = System.Drawing.SystemColors.Control;
            this.trackBar.Location = new System.Drawing.Point(180, 12);
            this.trackBar.Maximum = 100;
            this.trackBar.Minimum = 50;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(403, 45);
            this.trackBar.TabIndex = 23;
            this.trackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar.Value = 80;
            this.trackBar.Scroll += new System.EventHandler(this.trackBar_Scroll_1);
            // 
            // cbEncryptConnection
            // 
            this.cbEncryptConnection.AutoSize = true;
            this.cbEncryptConnection.Location = new System.Drawing.Point(15, 92);
            this.cbEncryptConnection.Name = "cbEncryptConnection";
            this.cbEncryptConnection.Size = new System.Drawing.Size(119, 17);
            this.cbEncryptConnection.TabIndex = 19;
            this.cbEncryptConnection.Text = "Encrypt Connection";
            this.cbEncryptConnection.UseVisualStyleBackColor = true;
            // 
            // tpInformation
            // 
            this.tpInformation.BackColor = System.Drawing.SystemColors.Control;
            this.tpInformation.Controls.Add(this.pictureBox1);
            this.tpInformation.Controls.Add(this.label5);
            this.tpInformation.Controls.Add(this.label10);
            this.tpInformation.Controls.Add(this.label8);
            this.tpInformation.Controls.Add(this.lblCodeplex);
            this.tpInformation.Controls.Add(this.label7);
            this.tpInformation.Controls.Add(this.label6);
            this.tpInformation.Controls.Add(this.linkOH22);
            this.tpInformation.Controls.Add(this.linkCodeplex);
            this.tpInformation.Location = new System.Drawing.Point(4, 22);
            this.tpInformation.Name = "tpInformation";
            this.tpInformation.Padding = new System.Windows.Forms.Padding(3);
            this.tpInformation.Size = new System.Drawing.Size(710, 423);
            this.tpInformation.TabIndex = 3;
            this.tpInformation.Text = "Information";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::oh22is.SqlServer.DQS.Properties.Resources.oh22is_200x50;
            this.pictureBox1.Location = new System.Drawing.Point(504, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 23;
            this.pictureBox1.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.label5.Location = new System.Drawing.Point(6, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(396, 29);
            this.label5.TabIndex = 24;
            this.label5.Text = "SSIS DQS Matching Transformation";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(74, 116);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(22, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "1.2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 116);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Version";
            // 
            // lblCodeplex
            // 
            this.lblCodeplex.AutoSize = true;
            this.lblCodeplex.Location = new System.Drawing.Point(8, 43);
            this.lblCodeplex.Name = "lblCodeplex";
            this.lblCodeplex.Size = new System.Drawing.Size(294, 13);
            this.lblCodeplex.TabIndex = 19;
            this.lblCodeplex.Text = "This project is hosted on Codplex and licensed under MS-PL.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Contributor:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Project:";
            // 
            // linkOH22
            // 
            this.linkOH22.AutoSize = true;
            this.linkOH22.Location = new System.Drawing.Point(74, 91);
            this.linkOH22.Name = "linkOH22";
            this.linkOH22.Size = new System.Drawing.Size(99, 13);
            this.linkOH22.TabIndex = 2;
            this.linkOH22.TabStop = true;
            this.linkOH22.Text = "http://www.oh22.is";
            this.linkOH22.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkOH22_LinkClicked);
            // 
            // linkCodeplex
            // 
            this.linkCodeplex.AutoSize = true;
            this.linkCodeplex.Location = new System.Drawing.Point(74, 66);
            this.linkCodeplex.Name = "linkCodeplex";
            this.linkCodeplex.Size = new System.Drawing.Size(184, 13);
            this.linkCodeplex.TabIndex = 1;
            this.linkCodeplex.TabStop = true;
            this.linkCodeplex.Text = "http://ssisdqsmatching.codeplex.com";
            this.linkCodeplex.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCodeplex_LinkClicked);
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(658, 506);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(577, 506);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(496, 506);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FrmMatchingUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 541);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.tabProperties);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(760, 580);
            this.MinimumSize = new System.Drawing.Size(760, 580);
            this.Name = "FrmMatchingUi";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SSIS DQS Matching Transformation";
            this.Load += new System.EventHandler(this.frmMatchingUI_Load);
            this.tabProperties.ResumeLayout(false);
            this.tpConnectionManager.ResumeLayout(false);
            this.tpConnectionManager.PerformLayout();
            this.tpMapping.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvInputColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).EndInit();
            this.tpAdvanced.ResumeLayout(false);
            this.tpAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar)).EndInit();
            this.tpInformation.ResumeLayout(false);
            this.tpInformation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabProperties;
        private System.Windows.Forms.TabPage tpConnectionManager;
        private System.Windows.Forms.Label lblInfoMapping;
        private System.Windows.Forms.Label lblInfoConnectionManager;
        private System.Windows.Forms.TabPage tpMapping;
        private System.Windows.Forms.TabPage tpAdvanced;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.ComboBox cbDQKnowledgeBase;
        private System.Windows.Forms.ComboBox cbDQConnectionManager;
        private System.Windows.Forms.Label lblDQKnowledgeBase;
        private System.Windows.Forms.Label lblDQConnectionManager;
        private System.Windows.Forms.Label lblInfoMatchingRules;
        private System.Windows.Forms.TreeView tvMatchingRules;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgvMapping;
        private System.Windows.Forms.DataGridView dgvInputColumns;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TabPage tpInformation;
        private System.Windows.Forms.LinkLabel linkOH22;
        private System.Windows.Forms.LinkLabel linkCodeplex;
        private System.Windows.Forms.Label lblMinimalMatchingScore;
        private System.Windows.Forms.CheckBox cbCleanDQSProjects;
        private System.Windows.Forms.Label lblScoreValue;
        private System.Windows.Forms.TrackBar trackBar;
        private System.Windows.Forms.CheckBox cbEncryptConnection;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        internal System.Windows.Forms.Label lblCodeplex;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox cbResultSet;
        private System.Windows.Forms.Label label2;
    }
}

