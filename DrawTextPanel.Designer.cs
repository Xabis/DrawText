namespace TriDelta.DrawTextMode
{
   partial class DrawTextPanel
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
            this.label1 = new System.Windows.Forms.Label();
            this.txtDisplayText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.udTextSize = new TriDelta.DrawTextMode.VariableNumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tlConfig = new System.Windows.Forms.TableLayoutPanel();
            this.lstFontList = new System.Windows.Forms.ComboBox();
            this.udQuality = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.rModeLine = new System.Windows.Forms.RadioButton();
            this.rModeCircle = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.rAlignLeft = new System.Windows.Forms.RadioButton();
            this.rAlignCenter = new System.Windows.Forms.RadioButton();
            this.rAlignRight = new System.Windows.Forms.RadioButton();
            this.rAlignJustified = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.udSpacing = new TriDelta.DrawTextMode.VariableNumericUpDown();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdConfirm = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.udTextSize)).BeginInit();
            this.tlConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udQuality)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udSpacing)).BeginInit();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text";
            // 
            // txtDisplayText
            // 
            this.txtDisplayText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDisplayText.Location = new System.Drawing.Point(62, 30);
            this.txtDisplayText.Name = "txtDisplayText";
            this.txtDisplayText.Size = new System.Drawing.Size(181, 20);
            this.txtDisplayText.TabIndex = 1;
            this.txtDisplayText.TextChanged += new System.EventHandler(this.txtDisplayText_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Size";
            // 
            // udTextSize
            // 
            this.udTextSize.DecimalPlaces = 1;
            this.udTextSize.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udTextSize.IncrementAlt = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.udTextSize.IncrementCtrl = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udTextSize.IncrementShift = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTextSize.Location = new System.Drawing.Point(62, 116);
            this.udTextSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udTextSize.Name = "udTextSize";
            this.udTextSize.Size = new System.Drawing.Size(84, 20);
            this.udTextSize.TabIndex = 2;
            this.udTextSize.Value = new decimal(new int[] {
            72,
            0,
            0,
            0});
            this.udTextSize.ValueChanged += new System.EventHandler(this.udTextSize_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Quality";
            // 
            // tlConfig
            // 
            this.tlConfig.AutoSize = true;
            this.tlConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlConfig.ColumnCount = 2;
            this.tlConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlConfig.Controls.Add(this.label1, 0, 1);
            this.tlConfig.Controls.Add(this.label3, 0, 5);
            this.tlConfig.Controls.Add(this.txtDisplayText, 1, 1);
            this.tlConfig.Controls.Add(this.lstFontList, 1, 0);
            this.tlConfig.Controls.Add(this.udTextSize, 1, 4);
            this.tlConfig.Controls.Add(this.label2, 0, 4);
            this.tlConfig.Controls.Add(this.udQuality, 1, 5);
            this.tlConfig.Controls.Add(this.label4, 0, 0);
            this.tlConfig.Controls.Add(this.label5, 0, 2);
            this.tlConfig.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tlConfig.Controls.Add(this.label6, 0, 3);
            this.tlConfig.Controls.Add(this.flowLayoutPanel2, 1, 3);
            this.tlConfig.Controls.Add(this.label7, 0, 6);
            this.tlConfig.Controls.Add(this.udSpacing, 1, 6);
            this.tlConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlConfig.Location = new System.Drawing.Point(0, 0);
            this.tlConfig.Name = "tlConfig";
            this.tlConfig.RowCount = 7;
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlConfig.Size = new System.Drawing.Size(246, 191);
            this.tlConfig.TabIndex = 5;
            // 
            // lstFontList
            // 
            this.lstFontList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFontList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstFontList.Location = new System.Drawing.Point(62, 3);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(181, 21);
            this.lstFontList.TabIndex = 0;
            this.lstFontList.SelectedIndexChanged += new System.EventHandler(this.lstFontList_SelectedIndexChanged);
            // 
            // udQuality
            // 
            this.udQuality.Location = new System.Drawing.Point(62, 142);
            this.udQuality.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udQuality.Name = "udQuality";
            this.udQuality.Size = new System.Drawing.Size(84, 20);
            this.udQuality.TabIndex = 3;
            this.udQuality.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udQuality.ValueChanged += new System.EventHandler(this.udQuality_ValueChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Font";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Mode";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.rModeLine);
            this.flowLayoutPanel1.Controls.Add(this.rModeCircle);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(62, 53);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(181, 30);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // rModeLine
            // 
            this.rModeLine.Image = global::TriDelta.DrawText.Properties.Resources.icon_line;
            this.rModeLine.Location = new System.Drawing.Point(3, 3);
            this.rModeLine.Name = "rModeLine";
            this.rModeLine.Size = new System.Drawing.Size(37, 24);
            this.rModeLine.TabIndex = 1;
            this.rModeLine.TabStop = true;
            this.rModeLine.UseVisualStyleBackColor = true;
            this.rModeLine.CheckedChanged += new System.EventHandler(this.rModeLine_CheckedChanged);
            // 
            // rModeCircle
            // 
            this.rModeCircle.Image = global::TriDelta.DrawText.Properties.Resources.icon_circle;
            this.rModeCircle.Location = new System.Drawing.Point(46, 3);
            this.rModeCircle.Name = "rModeCircle";
            this.rModeCircle.Size = new System.Drawing.Size(37, 24);
            this.rModeCircle.TabIndex = 2;
            this.rModeCircle.TabStop = true;
            this.rModeCircle.UseVisualStyleBackColor = true;
            this.rModeCircle.CheckedChanged += new System.EventHandler(this.rModeCircle_CheckedChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Alignment";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.rAlignLeft);
            this.flowLayoutPanel2.Controls.Add(this.rAlignCenter);
            this.flowLayoutPanel2.Controls.Add(this.rAlignRight);
            this.flowLayoutPanel2.Controls.Add(this.rAlignJustified);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(62, 83);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(181, 30);
            this.flowLayoutPanel2.TabIndex = 10;
            // 
            // rAlignLeft
            // 
            this.rAlignLeft.Image = global::TriDelta.DrawText.Properties.Resources.icon_alignLeft;
            this.rAlignLeft.Location = new System.Drawing.Point(3, 3);
            this.rAlignLeft.Name = "rAlignLeft";
            this.rAlignLeft.Size = new System.Drawing.Size(37, 24);
            this.rAlignLeft.TabIndex = 3;
            this.rAlignLeft.TabStop = true;
            this.rAlignLeft.UseVisualStyleBackColor = true;
            this.rAlignLeft.CheckedChanged += new System.EventHandler(this.rAlignLeft_CheckedChanged);
            // 
            // rAlignCenter
            // 
            this.rAlignCenter.Image = global::TriDelta.DrawText.Properties.Resources.icon_alignCenter;
            this.rAlignCenter.Location = new System.Drawing.Point(46, 3);
            this.rAlignCenter.Name = "rAlignCenter";
            this.rAlignCenter.Size = new System.Drawing.Size(37, 24);
            this.rAlignCenter.TabIndex = 4;
            this.rAlignCenter.TabStop = true;
            this.rAlignCenter.UseVisualStyleBackColor = true;
            this.rAlignCenter.CheckedChanged += new System.EventHandler(this.rAlignCenter_CheckedChanged);
            // 
            // rAlignRight
            // 
            this.rAlignRight.Image = global::TriDelta.DrawText.Properties.Resources.icon_alignRight;
            this.rAlignRight.Location = new System.Drawing.Point(89, 3);
            this.rAlignRight.Name = "rAlignRight";
            this.rAlignRight.Size = new System.Drawing.Size(37, 24);
            this.rAlignRight.TabIndex = 5;
            this.rAlignRight.TabStop = true;
            this.rAlignRight.UseVisualStyleBackColor = true;
            this.rAlignRight.CheckedChanged += new System.EventHandler(this.rAlignRight_CheckedChanged);
            // 
            // rAlignJustified
            // 
            this.rAlignJustified.Image = global::TriDelta.DrawText.Properties.Resources.icon_alignJustified;
            this.rAlignJustified.Location = new System.Drawing.Point(132, 3);
            this.rAlignJustified.Name = "rAlignJustified";
            this.rAlignJustified.Size = new System.Drawing.Size(37, 24);
            this.rAlignJustified.TabIndex = 6;
            this.rAlignJustified.TabStop = true;
            this.rAlignJustified.UseVisualStyleBackColor = true;
            this.rAlignJustified.CheckedChanged += new System.EventHandler(this.rAlignJustified_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Spacing";
            // 
            // udSpacing
            // 
            this.udSpacing.DecimalPlaces = 1;
            this.udSpacing.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udSpacing.IncrementAlt = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.udSpacing.IncrementCtrl = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udSpacing.IncrementShift = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udSpacing.Location = new System.Drawing.Point(62, 168);
            this.udSpacing.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udSpacing.Name = "udSpacing";
            this.udSpacing.Size = new System.Drawing.Size(84, 20);
            this.udSpacing.TabIndex = 12;
            this.udSpacing.ValueChanged += new System.EventHandler(this.udSpacing_ValueChanged);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.cmdConfirm);
            this.flowLayoutPanel3.Controls.Add(this.cmdCancel);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 191);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(246, 29);
            this.flowLayoutPanel3.TabIndex = 13;
            // 
            // cmdConfirm
            // 
            this.cmdConfirm.AutoSize = true;
            this.cmdConfirm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdConfirm.Image = global::TriDelta.DrawText.Properties.Resources.icon_confirm;
            this.cmdConfirm.Location = new System.Drawing.Point(3, 3);
            this.cmdConfirm.Name = "cmdConfirm";
            this.cmdConfirm.Size = new System.Drawing.Size(68, 23);
            this.cmdConfirm.TabIndex = 0;
            this.cmdConfirm.Text = "Confirm";
            this.cmdConfirm.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdConfirm.UseVisualStyleBackColor = true;
            this.cmdConfirm.Click += new System.EventHandler(this.cmdConfirm_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.Image = global::TriDelta.DrawText.Properties.Resources.icon_cancel;
            this.cmdCancel.Location = new System.Drawing.Point(77, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(66, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // DrawTextPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel3);
            this.Controls.Add(this.tlConfig);
            this.Name = "DrawTextPanel";
            this.Size = new System.Drawing.Size(246, 247);
            ((System.ComponentModel.ISupportInitialize)(this.udTextSize)).EndInit();
            this.tlConfig.ResumeLayout(false);
            this.tlConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udQuality)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udSpacing)).EndInit();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox txtDisplayText;
      private System.Windows.Forms.Label label2;
      private VariableNumericUpDown udTextSize;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.TableLayoutPanel tlConfig;
      private System.Windows.Forms.NumericUpDown udQuality;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
      private System.Windows.Forms.ComboBox lstFontList;
      private System.Windows.Forms.RadioButton rModeLine;
      private System.Windows.Forms.RadioButton rModeCircle;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
      private System.Windows.Forms.RadioButton rAlignLeft;
      private System.Windows.Forms.RadioButton rAlignCenter;
      private System.Windows.Forms.RadioButton rAlignRight;
      private System.Windows.Forms.RadioButton rAlignJustified;
      private System.Windows.Forms.Label label7;
      private VariableNumericUpDown udSpacing;
      private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
      private System.Windows.Forms.Button cmdConfirm;
      private System.Windows.Forms.Button cmdCancel;
   }
}
