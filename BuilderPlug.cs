using System;
using CodeImp.DoomBuilder.Plugins;
using System.Drawing;
using System.Reflection;
using System.IO;
using CodeImp.DoomBuilder;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using TriDelta.OpenType;
using CodeImp.DoomBuilder.Editing;

namespace TriDelta.DrawTextMode {
    public delegate bool EditModeChange(EditMode oldmode, EditMode newmode);

    public class BuilderPlug : Plug {
        private static BuilderPlug me;
        public static BuilderPlug Me { get { return me; } }

        private PlotMode m_mode;
        private TextAlignment m_align;
        private OpenFont m_font;
        private bool m_dirty = false;
        private float m_size;
        private int m_curveQuality;
        private float m_tolerance;
        private DrawTextMode m_editmode;
        private string m_text;
        private float m_spacing;
        private bool m_debugmode;

        private List<OpenFont> m_fontcache;

        public override string Name {
            get { return "Draw Text"; }
        }

        public override void OnInitialize() {
            base.OnInitialize();
            m_fontcache = new List<OpenFont>();
            Load();
            me = this;
        }

        public override void Dispose() {
            base.Dispose();
        }

        public Image GetResourceImage(string key) {
            Stream s = this.GetResourceStream(key);
            if (s != null)
                return Image.FromStream(s);
            return null;
        }

        public PlotMode PlotMode {
            get { return m_mode; }
            set { m_mode = value; m_dirty = true; }
        }

        public TextAlignment TextAlignment {
            get { return m_align; }
            set { m_align = value; m_dirty = true; }
        }

        public OpenFont Font {
            get { return m_font; }
            set { m_font = value; m_dirty = true; }
        }

        public float Size {
            get { return m_size; }
            set { m_size = value; m_dirty = true; }
        }

        public int CurveQuality {
            get { return m_curveQuality; }
            set { m_curveQuality = value; m_dirty = true; }
        }

        public float Tolerance {
            get { return m_tolerance; }
            set { m_tolerance = value; m_dirty = true; }
        }

        public DrawTextMode EditMode {
            get { return m_editmode; }
            set { m_editmode = value; }
        }

        public bool DebugMode {
            get { return m_debugmode; }
            set { m_debugmode = value; m_dirty = true; }
        }

        public string DisplayText {
            get { return m_text; }
            set { m_text = value; m_dirty = true; }
        }

        public float TextSpacing {
            get { return m_spacing; }
            set { m_spacing = value; m_dirty = true; }
        }

        public void Load() {
            m_mode = (PlotMode)General.Settings.ReadPluginSetting("plotmode", 1);
            m_align = (TextAlignment)General.Settings.ReadPluginSetting("textalignment", 1);
            m_size = General.Settings.ReadPluginSetting("textsize", 10f);
            m_curveQuality = General.Settings.ReadPluginSetting("curvequality", 1);
            m_tolerance = General.Settings.ReadPluginSetting("tolerance", 0.5f);
            m_font = OpenFont.GetFont(General.Settings.ReadPluginSetting("textfont", "Arial"));
            if (m_font != null) {
                m_font.PointSize = m_size;
                m_font.Quality = m_curveQuality;
            }
            m_text = General.Settings.ReadPluginSetting("defaulttext", "Open the dock panel to edit!");
            m_spacing = General.Settings.ReadPluginSetting("textspacing", 0f);
            m_debugmode = General.Settings.ReadPluginSetting("debugmode", false);
        }

        public void Save() {
            if (!m_dirty)
                return;
            General.Settings.WritePluginSetting("plotmode", (int)m_mode);
            General.Settings.WritePluginSetting("textalignment", (int)m_align);
            General.Settings.WritePluginSetting("textsize", m_size);
            General.Settings.WritePluginSetting("textfont", m_font == null ? "Arial" : m_font.Name);
            General.Settings.WritePluginSetting("curvequality", m_curveQuality);
            General.Settings.WritePluginSetting("tolerance", m_tolerance);
            General.Settings.WritePluginSetting("defaulttext", m_text);
            General.Settings.WritePluginSetting("textspacing", m_spacing);
            General.Settings.WritePluginSetting("debugmode", m_debugmode);
        }
    }
}