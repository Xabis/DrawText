using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TriDelta.OpenType;
using System.Diagnostics;

namespace TriDelta.DrawTextMode {
    public partial class DrawTextPanel : UserControl {
        private DrawTextMode m_mode;

        public DrawTextPanel(DrawTextMode editmode) {
            m_mode = editmode;
            InitializeComponent();

            if (OpenFont.Fonts.Count > 0) {
                foreach (OpenFont font in OpenFont.Fonts)
                    lstFontList.Items.Add(font);
            }

            lstFontList.SelectedItem = m_mode.Plug.Font;
            txtDisplayText.Text = m_mode.Plug.DisplayText;
            udTextSize.Value = (decimal)m_mode.Plug.Size;
            udQuality.Value = (decimal)m_mode.Plug.CurveQuality;
            udSpacing.Value = (decimal)m_mode.Plug.TextSpacing;
            udTolerance.Value = (decimal)m_mode.Plug.Tolerance;
            chkDebugMode.Checked = m_mode.Plug.DebugMode;

            switch (m_mode.Plug.PlotMode) {
                case PlotMode.Normal:
                    rModeLine.Checked = true;
                    break;
                case PlotMode.Circle:
                    rModeCircle.Checked = true;
                    break;
            }
            switch (m_mode.Plug.TextAlignment) {
                case TextAlignment.Left:
                    rAlignLeft.Checked = true;
                    break;
                case TextAlignment.Center:
                    rAlignCenter.Checked = true;
                    break;
                case TextAlignment.Right:
                    rAlignRight.Checked = true;
                    break;
                case TextAlignment.Justified:
                    rAlignJustified.Checked = true;
                    break;
            }

            m_mode.ModeChanged += m_mode_ModeChanged;
        }

        private void m_mode_ModeChanged(DrawTextMode mode) {
            udTextSize.Value = (decimal)mode.TextSize;
        }

        private void udTextSize_ValueChanged(object sender, EventArgs e) {
            m_mode.TextSize = (float)udTextSize.Value;
        }
        private void txtDisplayText_TextChanged(object sender, EventArgs e) {
            m_mode.Text = txtDisplayText.Text;
        }
        private void lstFontList_SelectedIndexChanged(object sender, EventArgs e) {
            m_mode.TextFont = (OpenFont)lstFontList.SelectedItem;
        }
        private void udQuality_ValueChanged(object sender, EventArgs e) {
            m_mode.Quality = (int)udQuality.Value;
        }
        private void rModeLine_CheckedChanged(object sender, EventArgs e) {
            m_mode.DrawMode = PlotMode.Normal;
        }
        private void rModeCircle_CheckedChanged(object sender, EventArgs e) {
            m_mode.DrawMode = PlotMode.Circle;
        }
        private void rAlignLeft_CheckedChanged(object sender, EventArgs e) {
            m_mode.Alignment = TextAlignment.Left;
        }
        private void rAlignCenter_CheckedChanged(object sender, EventArgs e) {
            m_mode.Alignment = TextAlignment.Center;
        }
        private void rAlignRight_CheckedChanged(object sender, EventArgs e) {
            m_mode.Alignment = TextAlignment.Right;
        }
        private void rAlignJustified_CheckedChanged(object sender, EventArgs e) {
            m_mode.Alignment = TextAlignment.Justified;
        }
        private void udSpacing_ValueChanged(object sender, EventArgs e) {
            m_mode.TextSpacing = (float)udSpacing.Value;
        }
        private void cmdConfirm_Click(object sender, EventArgs e) {
            m_mode.OnAccept();
        }
        private void cmdCancel_Click(object sender, EventArgs e) {
            m_mode.OnCancel();
        }

        private void udTolerance_ValueChanged(object sender, EventArgs e) {
            m_mode.Tolerance = (float)udTolerance.Value;
        }

        private void chkDebugMode_CheckedChanged(object sender, EventArgs e) {
            m_mode.DebugMode = chkDebugMode.Checked;
        }
    }
}
