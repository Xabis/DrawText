using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;

namespace TriDelta.OpenType {
    //Note: This parser only supports Microsoft "TTF" fonts with Windows encodings.
    //      OpenType fonts are NOT supported.
    //      Collections are NOT supported.
    //      Composite glyphs are NOT supported.
    public class OpenFont {
        // Private storage
        private float m_sfnt_version;
        private int m_search_range;
        private int m_entry_selector;
        private int m_range_shift;
        private int m_curve_quality = 2;
        private float m_optimize_tolerance = 0.500f;
        private int m_device_dpi;
        private float m_point_size;
        private float m_glyph_scale;
        private string m_name;
        private string m_filePath;

        private bool m_font_valid = false;
        private bool m_font_cached = false;

        SectionHead head;
        SectionMaxp maxp;
        SectionHhea hhea;

        Dictionary<string, Section> records;
        Dictionary<int, string> names;
        Dictionary<int, Glyph> glyphs;
        List<HorizontalMetrics> glyphmetrics;
        Dictionary<char, Glyph> glyphmap;

        List<PointF> m_last_controllist;

        //winapi for grabbing screen dpi
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        //-------------------------------------------------------------------------------------
        // Static properties and methods
        //-------------------------------------------------------------------------------------
        private static List<OpenFont> m_fontcache;

        /// <summary>Gets a list of supported fonts installed on the system</summary>
        public static List<OpenFont> Fonts {
            get {
                if (m_fontcache == null) {
                    m_fontcache = new List<OpenFont>();
                    string pathFonts = GetPath(20); //Windows font folder
                    DirectoryInfo folder = new DirectoryInfo(pathFonts);
                    FileInfo[] files = folder.GetFiles();
                    OpenFont font;

                    foreach (FileInfo file in files) {
                        try {
                            if (file.Extension.ToLower() == ".ttf") {
                                font = new OpenFont(file.FullName, 12);

                                //Load times increase dramatically at glyph high counts.
                                //For example, the font "Arial Unicode MS" contains 50377 glyphs.
                                //Some performance has been gained by caching point data for glyphs and only on demand.
                                //The remaining issue is the byte-by-byte reading in the .ReadStream function which incurs
                                //  a serious performance hit. A future enhancement would be reading the entire glyph data
                                //  into a byte array and operating off of that instead of reading from the stream one
                                //  byte at a time.
                                if (font.GlyphCount < 10000)
                                    m_fontcache.Add(new OpenFont(file.FullName, 12));
                            }
                        } catch { } //Dont care about what fonts were skipped
                    }
                }
                return m_fontcache;
            }
        }

        /// <summary>Gets a font based on its font name</summary>
        public static OpenFont GetFont(string name) {
            foreach (OpenFont font in Fonts) {
                if (font.Name == name)
                    return font;
            }
            return null;
        }

        //Retrieves a special window folder based on its index
        private static string GetPath(int folder) {
            StringBuilder lpszPath = new StringBuilder(260);
            SHGetFolderPath(IntPtr.Zero, folder, IntPtr.Zero, 0, lpszPath);
            return lpszPath.ToString();
        }

        //-------------------------------------------------------------------------------------
        // Constructor
        //-------------------------------------------------------------------------------------
        public OpenFont(string FontFile, float pointSize) {
            m_filePath = FontFile;
            m_name = FontFile;
            m_device_dpi = GetDpi();
            m_point_size = pointSize;
            records = new Dictionary<string, Section>();
            names = new Dictionary<int, string>();
            glyphs = new Dictionary<int, Glyph>();
            glyphmetrics = new List<HorizontalMetrics>();
            glyphmap = new Dictionary<char, Glyph>();
            LoadFont(FontFile);
        }

        //determine the system wide dpi setting
        private int GetDpi() {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int dpi;
            if (hdc != IntPtr.Zero) {
                dpi = GetDeviceCaps(hdc, 88); //LOGPIXELSX
                ReleaseDC(IntPtr.Zero, hdc);
                return dpi;
            }
            return 0;
        }

        //-------------------------------------------------------------------------------------
        // Properties
        //-------------------------------------------------------------------------------------
        public Dictionary<int, Glyph> Glyphs {
            get { return glyphs; }
        }

        public int GlyphCount {
            get { return maxp.numGlyphs; }
        }

        public int Quality {
            get { return m_curve_quality; }
            set {
                m_curve_quality = value;
                foreach (KeyValuePair<int, Glyph> pair in glyphs)
                    pair.Value.Update();
            }
        }

        public float Tolerance {
            get { return m_optimize_tolerance; }
            set {
                m_optimize_tolerance = value;
                foreach (KeyValuePair<int, Glyph> pair in glyphs)
                    pair.Value.Update();
            }
        }

        public int UnitsPerEM {
            get { return head.unitsPerEM; }
        }

        public float GlyphScale {
            get { return m_glyph_scale; }
        }

        public float PointSize {
            get { return m_point_size; }
            set {
                m_point_size = value;
                CalculateGlyphScale();
            }
        }

        public string Name {
            get { return m_name; }
        }

        public string FontFamily {
            get {
                if (names.ContainsKey(1))
                    return names[1];
                return "";
            }
        }

        public List<PointF> LastControlList {
            get {
                return m_last_controllist;
            }
        }

        //-------------------------------------------------------------------------------------
        // Methods
        //-------------------------------------------------------------------------------------
        /// <summary>Retrieve the glyph data associated with a unicode character.</summary>
        public Glyph GetGlyph(Char c) {
            if (!m_font_cached)
                CacheData();
            if (glyphmap.ContainsKey(c))
                return glyphmap[c];
            return glyphs[0];
        }

        /// <summary>Renders the outline of a string to graphic buffer</summary>
        /// <param name="g">A graphic buffer to draw the outline on</param>
        /// <param name="p">The pen to draw the outline with</param>
        /// <param name="text">Text to render</param>
        /// <param name="origin">The leftmost location for text</param>
        /// <param name="vector">The rightmost location for text</param>
        /// <param name="mode">Determines how the text should be drawn</param>
        /// <param name="alignment">Determines where the text should be drawn</param>
        /// <param name="padding">Extra space to add between characters</param>
        public void RenderString(Graphics g, Pen p, string text, PointF origin, PointF vector, PlotMode mode, TextAlignment alignment, int padding) {
            if (!m_font_cached)
                CacheData();
            if (origin.Equals(vector))
                return;

            List<PointF[]> lines = PlotString(text, origin, vector, mode, alignment, padding);
            if (lines != null) {
                foreach (PointF[] points in lines) {
                    g.DrawLines(p, points);
                }
            }
        }

        /// <summary>Creates a list of points that represent a string</summary>
        /// <param name="text">Text to render</param>
        /// <param name="origin">The leftmost location for text</param>
        /// <param name="vector">The rightmost location for text</param>
        /// <param name="mode">Determines how the text should be drawn</param>
        /// <param name="alignment">Determines where the text should be drawn</param>
        /// <param name="padding">Extra space to add between characters</param>
        public List<PointF[]> PlotString(string text, PointF origin, PointF vector, PlotMode mode, TextAlignment alignment, float padding) {
            if (!m_font_cached)
                CacheData();

            List<PointF[]> lines = new List<PointF[]>();
            List<PointF> controls = new List<PointF>();
            Glyph glyph;
            PointF pos = origin;
            float angle, offset, width;
            double sin, cos;
            float radius = (float)Math.Sqrt((vector.X - origin.X) * (vector.X - origin.X) + (vector.Y - origin.Y) * (vector.Y - origin.Y));
            float rotation = 0;
            float lsb;

            switch (mode) {
                case PlotMode.Normal:
                    if (text.Length > 0) {
                        angle = (float)Math.Atan2(vector.Y - origin.Y, vector.X - origin.X);
                        sin = Math.Sin(angle);
                        cos = Math.Cos(angle);

                        //measure the string width
                        float totalwidth = 0;
                        foreach (char c in text) {
                            glyph = GetGlyph(c);
                            totalwidth += glyph.Width;
                            if (alignment != TextAlignment.Justified)
                                totalwidth += padding;
                        }
                        totalwidth *= m_glyph_scale;

                        if (totalwidth > radius)
                            alignment = TextAlignment.Left;

                        switch (alignment) {
                            case TextAlignment.Center:
                                lsb = ((radius / 2) - (totalwidth / 2));
                                pos.X += (float)(cos * lsb);
                                pos.Y += (float)(sin * lsb);
                                break;
                            case TextAlignment.Justified:
                                if (text.Length > 1)
                                    padding += ((radius - totalwidth) / (text.Length - 1)) / m_glyph_scale;
                                break;
                            case TextAlignment.Right:
                                lsb = radius - totalwidth;
                                pos.X += (float)(cos * lsb);
                                pos.Y += (float)(sin * lsb);
                                break;
                        }

                        foreach (char c in text) {
                            glyph = GetGlyph(c);
                            lines.AddRange(glyph.TranslateOutline(pos, angle + rotation, RotationAnchor.Origin));
                            controls.AddRange(glyph.TranslateOutlineControls(pos, angle + rotation, RotationAnchor.Origin));
                            width = (glyph.Width + padding) * m_glyph_scale;
                            pos.X += (float)(cos * width);
                            pos.Y += (float)(sin * width);
                        }
                    }
                    break;
                case PlotMode.Circle:
                    float justifiedrads, offset2;

                    //precache
                    angle = (float)Math.Atan2(vector.Y - origin.Y, vector.X - origin.X);
                    offset = angle;
                    rotation = -1.57079637f; //(float)((double)-90 * (Math.PI / 180));
                    offset2 = justifiedrads = 0f;

                    if (alignment == TextAlignment.Justified)
                        justifiedrads = (float)((Math.PI / (double)text.Length) * 2);

                    foreach (char c in text) {
                        glyph = GetGlyph(c);
                        if (alignment != TextAlignment.Justified)
                            offset2 = (((glyph.Width + padding) * m_glyph_scale) / radius) / 2;

                        pos.X = origin.X + (float)(Math.Cos(offset - offset2) * radius);
                        pos.Y = origin.Y + (float)(Math.Sin(offset - offset2) * radius);

                        lines.AddRange(glyph.TranslateOutline(pos, offset - offset2 + rotation, RotationAnchor.CenterBaseline));
                        controls.AddRange(glyph.TranslateOutlineControls(pos, offset - offset2 + rotation, RotationAnchor.CenterBaseline));

                        if (alignment == TextAlignment.Justified) {
                            offset -= justifiedrads;
                        } else {
                            offset -= ((glyph.Width + padding) * m_glyph_scale) / radius;
                        }
                    }

                    break;
                default:
                    throw new OpenFontException(this, "Unsupported plotting mode");
            }

            m_last_controllist = controls;
            return lines;
        }

        /// <summary>Returns the font name</summary>
        public override string ToString() {
            return Name;
        }

        //-------------------------------------------------------------------------------------
        // Internal Functions
        //-------------------------------------------------------------------------------------
        private void LoadFont(string FilePath) {
            Stream s = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            try {
                m_sfnt_version = Tools.ReadFixed(s);
                if (m_sfnt_version != 1)
                    throw new OpenFontException(this, "Unsupported font version");

                //read the font header
                int sectioncnt = Tools.ReadUShort(s);
                m_search_range = Tools.ReadUShort(s);
                m_entry_selector = Tools.ReadUShort(s);
                m_range_shift = Tools.ReadUShort(s);

                //cache the font table data
                for (int i = 0; i < sectioncnt; i++) {
                    Section newsection = new Section(
                       Tools.ReadString(s, 4), //tag
                       Tools.ReadInt(s),       //checksum
                       Tools.ReadInt(s),       //offset
                       Tools.ReadInt(s)        //length
                    );
                    records.Add(newsection.Tag, newsection);
                }

                //only accept the font if certain tables exist
                if (!records.ContainsKey("glyf"))
                    throw new OpenFontException(this, "Only vector based fonts is supported.");
                if (!records.ContainsKey("head"))
                    throw new OpenFontException(this, "missing required table: head");
                if (!records.ContainsKey("hhea"))
                    throw new OpenFontException(this, "missing required table: hhea");
                if (!records.ContainsKey("maxp"))
                    throw new OpenFontException(this, "missing required table: maxp");
                if (!records.ContainsKey("hmtx"))
                    throw new OpenFontException(this, "missing required table: hmtx");
                if (!records.ContainsKey("loca"))
                    throw new OpenFontException(this, "missing required table: loca");
                if (!records.ContainsKey("cmap"))
                    throw new OpenFontException(this, "missing required table: cmap");
                if (!records.ContainsKey("name"))
                    throw new OpenFontException(this, "missing required table: name");

                //only read the header information for the first pass. glyph data is lazy loaded upon demand.
                ReadNAME(s, records["name"].Offset);
                ReadHEAD(s, records["head"].Offset);
                ReadHHEA(s, records["hhea"].Offset);
                ReadMAXP(s, records["maxp"].Offset);
                ReadHMTX(s, records["hmtx"].Offset);

                m_font_valid = true;
            } catch (OpenFontException ex) {
                Debug.WriteLine("OFE ERROR: " + ex.Message);
                throw ex;
            } catch (Exception ex) {
                Debug.WriteLine("ERROR: " + ex.Message);
            } finally {
                s.Close();
            }
        }

        /// <summary>Builds and caches the glyph data</summary>
        public void CacheData() {
            if (m_font_cached)
                return;
            if (!m_font_valid)
                throw new OpenFontException(this, "Unable to cache becuase the font is unsupported");

            Stream s = File.Open(m_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            try {
                ReadGLYF(s, records["loca"], records["glyf"]);
                ReadCMAP(s, records["cmap"].Offset);
            } catch (OpenFontException ex) {
                m_font_valid = false;
                throw ex;
            } catch (Exception ex) {
                m_font_valid = false;
                throw ex;
            } finally {
                s.Close();
            }
            m_font_cached = true;
        }

        private void CalculateGlyphScale() {
            m_glyph_scale = (m_point_size * m_device_dpi / 72) / head.unitsPerEM;
        }

        private void ReadHEAD(Stream s, int offset) {
            s.Position = offset;
            float version = Tools.ReadFixed(s);
            if (version != 1.0f)
                throw new OpenFontException(this, "HEAD table declares an unsupported version");

            head.revision = Tools.ReadFixed(s);
            head.checksumAdjustment = Tools.ReadInt(s);
            head.magicNumber = Tools.ReadInt(s);
            head.flags = Tools.ReadUShort(s);
            head.unitsPerEM = Tools.ReadUShort(s);
            for (int i = 0; i < 8; i++) //consume created & modified dates (64bit each)
                Tools.ReadUShort(s);
            head.xMin = Tools.ReadShort(s);
            head.yMin = Tools.ReadShort(s);
            head.xMax = Tools.ReadShort(s);
            head.yMax = Tools.ReadShort(s);
            head.macStyle = Tools.ReadUShort(s);
            head.lowestRecPPEM = Tools.ReadUShort(s);
            head.fontDirectionHint = Tools.ReadShort(s);
            head.indexToLocFormat = Tools.ReadShort(s);
            head.glyphDataFormat = Tools.ReadShort(s);

            if (head.magicNumber != 0x5F0F3CF5)
                throw new OpenFontException(this, "Invalid Font");
            CalculateGlyphScale();
        }

        private void ReadMAXP(Stream s, int offset) {
            s.Position = offset;
            float version = Tools.ReadFixed(s);
            if (!(version == 1.0f || version == 0.5f))
                throw new OpenFontException(this, "MAXP table declares an unsupported version");

            maxp.numGlyphs = Tools.ReadUShort(s);
            if (version == 1.0f) {
                maxp.maxPoints = Tools.ReadUShort(s);
                maxp.maxContours = Tools.ReadUShort(s);
                maxp.maxCompositePoints = Tools.ReadUShort(s);
                maxp.maxCompositeContours = Tools.ReadUShort(s);
                maxp.maxZones = Tools.ReadUShort(s);
                maxp.maxTwilightPoints = Tools.ReadUShort(s);
                maxp.maxStorage = Tools.ReadUShort(s);
                maxp.maxFunctionDefs = Tools.ReadUShort(s);
                maxp.maxInstructionDefs = Tools.ReadUShort(s);
                maxp.maxStackElements = Tools.ReadUShort(s);
                maxp.maxSizeOfInstructions = Tools.ReadUShort(s);
                maxp.maxComponentElements = Tools.ReadUShort(s);
                maxp.maxComponentDepth = Tools.ReadUShort(s);
            }
        }

        private void ReadHHEA(Stream s, int offset) {
            s.Position = offset;
            float version = Tools.ReadFixed(s);
            if (version != 1.0f)
                throw new OpenFontException(this, "HHEA table declares an unsupported version");

            hhea.Ascender = Tools.ReadShort(s);
            hhea.Descender = Tools.ReadShort(s);
            hhea.LineGap = Tools.ReadShort(s);
            hhea.advanceWidthMax = Tools.ReadUShort(s);
            hhea.minLeftSideBearing = Tools.ReadShort(s);
            hhea.minRightSideBearing = Tools.ReadShort(s);
            hhea.xMaxExtent = Tools.ReadShort(s);
            hhea.caretSlopeRise = Tools.ReadShort(s);
            hhea.caretSlopeRun = Tools.ReadShort(s);
            hhea.caretOffset = Tools.ReadShort(s);
            Tools.ReadShort(s); //reserved
            Tools.ReadShort(s); //reserved
            Tools.ReadShort(s); //reserved
            Tools.ReadShort(s); //reserved
            hhea.metricDataFormat = Tools.ReadShort(s);
            hhea.numberOfHMetrics = Tools.ReadUShort(s);
        }

        private void ReadHMTX(Stream s, int offset) {
            int i;

            s.Position = offset;

            HorizontalMetrics metrics;
            metrics.advanceWidth = 0; //squelch compiler error

            //read the base metric info
            for (i = 0; i < hhea.numberOfHMetrics; i++) {
                metrics.advanceWidth = Tools.ReadUShort(s);
                metrics.lsb = Tools.ReadShort(s);
                glyphmetrics.Add(metrics);
            }

            //add additional lsb data for monospaced fonts, if applicable
            for (i = 0; i < (maxp.numGlyphs - hhea.numberOfHMetrics); i++) {
                metrics.lsb = Tools.ReadShort(s);
                glyphmetrics.Add(metrics);
            }
        }

        private void ReadNAME(Stream s, int offset) {
            s.Position = offset;

            NameRecord record;
            Dictionary<int, SortedList<int, NameRecord>> records = new Dictionary<int, SortedList<int, NameRecord>>();

            //read name table header
            int format = Tools.ReadUShort(s);
            int count = Tools.ReadUShort(s);
            int stringOffset = Tools.ReadUShort(s);

            //read and cache the tables
            for (int i = 0; i < count; i++) {
                record.platformID = Tools.ReadUShort(s);
                record.encodingID = Tools.ReadUShort(s);
                record.languageID = Tools.ReadUShort(s);
                record.nameID = Tools.ReadUShort(s);
                record.length = Tools.ReadUShort(s);
                record.offset = Tools.ReadUShort(s);

                if (record.platformID == 3) {
                    if (!records.ContainsKey(record.nameID))
                        records[record.nameID] = new SortedList<int, NameRecord>();
                    records[record.nameID].Add(record.languageID, record);
                }
            }

            //for each string ID, pull out string for the current language or the first in the sorted pile if not found
            int currentLangId = CultureInfo.CurrentCulture.LCID;
            foreach (KeyValuePair<int, SortedList<int, NameRecord>> outer in records) {
                SortedList<int, NameRecord> inner = outer.Value;
                if (inner.ContainsKey(currentLangId))
                    record = inner[currentLangId];
                else
                    record = inner[inner.Keys[0]];

                s.Position = offset + stringOffset + record.offset;
                string text = Tools.ReadUnicodeString(s, record.length);
                names[outer.Key] = text;
            }

            if (names.ContainsKey(4))
                m_name = names[4];
            else if (names.ContainsKey(1))
                m_name = names[1];
        }

        private void ReadCMAP(Stream s, int offset) {
            s.Position = offset;

            int i;
            int version = Tools.ReadUShort(s);
            if (version != 0f)
                throw new OpenFontException(this, "CMAP table declares an unsupported version");

            //read in the encoding list for the microsoft platform and sort it
            SortedList<int, MappingTable> encodings = new SortedList<int, MappingTable>();
            int numTables = Tools.ReadUShort(s);
            for (i = 0; i < numTables; i++) {
                MappingTable table;
                table.platformID = Tools.ReadUShort(s);
                table.encodingID = Tools.ReadUShort(s);
                table.offset = Tools.ReadInt(s);

                //only interested in the microsoft platform
                if (table.platformID == 3)
                    encodings.Add(table.encodingID, table);
            }

            if (encodings.Count == 0)
                throw new OpenFontException(this, "No suitable font encoding is available.");

            //take the lowest encoding the in the platform and process it
            MappingTable chartable = encodings[encodings.Keys[0]];

            s.Position = offset + chartable.offset;
            int format = Tools.ReadUShort(s);

            switch (format) {
                case 4: //dumb m$ format
                    //read format info
                    int length = Tools.ReadUShort(s);
                    int endoffset = offset + chartable.offset + length;
                    int lang = Tools.ReadUShort(s);
                    int segCountX2 = Tools.ReadUShort(s);
                    int segCount = segCountX2 >> 1; //cause I can!
                    int searchRange = Tools.ReadUShort(s);
                    int entrySelector = Tools.ReadUShort(s);
                    int rangeShift = Tools.ReadUShort(s);
                    bool isSymbol = chartable.encodingID == 0;

                    //read segment data
                    int[] endCount = new int[segCount];
                    int[] startCount = new int[segCount];
                    int[] idDelta = new int[segCount];
                    int[] idRangeOffset = new int[segCount];

                    for (i = 0; i < segCount; i++)
                        endCount[i] = Tools.ReadUShort(s);
                    Tools.ReadUShort(s); //reservedPad
                    for (i = 0; i < segCount; i++)
                        startCount[i] = Tools.ReadUShort(s);
                    for (i = 0; i < segCount; i++)
                        idDelta[i] = Tools.ReadUShort(s);
                    for (i = 0; i < segCount; i++)
                        idRangeOffset[i] = Tools.ReadUShort(s);

                    int bytesLeft = endoffset - (int)s.Position;
                    int[] glyphIdArray = new int[bytesLeft / 2];
                    for (i = 0; i < (bytesLeft / 2); i++)
                        glyphIdArray[i] = Tools.ReadUShort(s);

                    //process the segment data
                    int unicode, gid, delta, shift, shift2;
                    char keycode;
                    shift2 = 0;
                    for (i = 0; i < segCount; i++) {
                        //Do not capture the table terminator character
                        if (startCount[i] == 0xFFFF)
                            break;

                        delta = idDelta[i];
                        shift = idRangeOffset[i];
                        if (shift > 0)
                            shift2 = (shift / 2) - (segCount - i);

                        //process each unicode character in the range
                        for (unicode = startCount[i]; unicode <= endCount[i]; unicode++) {
                            //look up the glyph id for the unicode character
                            if (shift > 0)
                                gid = glyphIdArray[(unicode - startCount[i]) + shift2];
                            else
                                gid = (unicode + delta) % 65536;

                            //convert the unicode if this is a symbol font
                            if (isSymbol)
                                keycode = (char)(unicode & 0x00ff);
                            else
                                keycode = (char)unicode;

                            //bind the glyph to the character code if not already taken
                            if (!glyphmap.ContainsKey(keycode)) {
                                if (glyphs.ContainsKey(gid))
                                    glyphmap.Add(keycode, glyphs[gid]);
                                else
                                    glyphmap.Add(keycode, glyphs[0]);
                            }
                        }
                    }

                    break;
                default:
                    throw new OpenFontException(this, "Unsupported character binding format.");
            }
        }

        private void ReadGLYF(Stream s, Section loca, Section glyf) {
            //read offset data. this also declares the glyph id (index)
            s.Position = loca.Offset;
            int[] offsets = new int[maxp.numGlyphs + 1];
            if (head.indexToLocFormat == 0) {
                for (int i = 0; i <= maxp.numGlyphs; ++i)
                    offsets[i] = Tools.ReadUShort(s) * 2;
            } else {
                for (int i = 0; i <= maxp.numGlyphs; ++i)
                    offsets[i] = Tools.ReadInt(s);
            }

            //read glyph data
            for (int i = 0; i < maxp.numGlyphs; ++i) {
                //Add the glyph shell to the pile. If the glyph data is bad (but not missing) then it will render blank
                Glyph g = new Glyph(this, i, glyphmetrics[i]);
                glyphs.Add(i, g);

                if (offsets[i + 1] > glyf.Length) //skip glyphs whos end exceeds the glyph data
                    continue;
                if (offsets[i + 1] < offsets[i])  //skip glyphs whos end offset is set before its start
                    continue;
                if (offsets[i + 1] == offsets[i]) //skip empty declarations
                    continue;

                s.Position = glyf.Offset + offsets[i];
                g.ReadStream(s);
            }
        }
    }

    /// <summary>A set of stream reading utilities for BigEndian binary data</summary>
    public static class Tools {
        //The binary data for the open type file format is stored in big-endian machine order. 
        //  These functions extract and reverse the binary data.

        public static int ReadUShort(Stream s) {
            return (s.ReadByte() << 8) | s.ReadByte();
        }
        public static int ReadShort(Stream s) {
            return (int)((Int16)ReadUShort(s));
        }
        public static int ReadInt(Stream s) {
            return (s.ReadByte() << 24) | (s.ReadByte() << 16) | (s.ReadByte() << 8) | s.ReadByte();
        }
        public static float ReadFixed(Stream s) {
            int val = ReadInt(s);
            int mant = val & 0xffff;
            return (float)(val >> 16) + (float)(mant / 65536.0);
        }
        public static float Read2Dot14(Stream s) {
            int val = ReadUShort(s);
            int mant = val & 0x3fff;
            return ((float)((val << 16) >> (16 + 14)) + (float)(mant / 16384.0));
        }
        public static string ReadString(Stream s, int length) {
            byte[] buffer = new byte[length];
            s.Read(buffer, 0, length);
            return System.Text.Encoding.ASCII.GetString(buffer);
        }
        public static string ReadUnicodeString(Stream s, int length) {
            byte[] buffer = new byte[length];
            s.Read(buffer, 0, length);
            return System.Text.Encoding.BigEndianUnicode.GetString(buffer);
        }
    }

    public class Section {
        private string m_tag;
        private int m_checksum;
        private int m_offset;
        private int m_length;

        public Section(string tag, int checksum, int offset, int length) {
            m_tag = tag;
            m_checksum = checksum;
            m_offset = offset;
            m_length = length;
        }

        public string Tag {
            get { return m_tag; }
        }
        public int Offset {
            get { return m_offset; }
        }
        public int Length {
            get { return m_length; }
        }
        public int Checksum {
            get { return m_checksum; }
        }
    }

    public struct SectionHead {
        public float revision;
        public int checksumAdjustment;
        public int magicNumber;
        public int flags;
        public int unitsPerEM;
        public int xMin;
        public int yMin;
        public int xMax;
        public int yMax;
        public int macStyle;
        public int lowestRecPPEM;
        public int fontDirectionHint;
        public int indexToLocFormat;
        public int glyphDataFormat;
    }

    public struct SectionMaxp {
        public int numGlyphs;
        public int maxPoints;
        public int maxContours;
        public int maxCompositePoints;
        public int maxCompositeContours;
        public int maxZones;
        public int maxTwilightPoints;
        public int maxStorage;
        public int maxFunctionDefs;
        public int maxInstructionDefs;
        public int maxStackElements;
        public int maxSizeOfInstructions;
        public int maxComponentElements;
        public int maxComponentDepth;
    }

    public struct SectionHhea {
        public int Ascender;
        public int Descender;
        public int LineGap;
        public int advanceWidthMax;
        public int minLeftSideBearing;
        public int minRightSideBearing;
        public int xMaxExtent;
        public int caretSlopeRise;
        public int caretSlopeRun;
        public int caretOffset;
        public int metricDataFormat;
        public int numberOfHMetrics;
    }

    public struct HorizontalMetrics {
        public int advanceWidth;
        public int lsb;
    }

    public struct MappingTable {
        public int platformID;
        public int encodingID;
        public int offset;
    }

    public struct NameRecord {
        public int platformID;
        public int encodingID;
        public int languageID;
        public int nameID;
        public int length;
        public int offset;
    }

    [Flags]
    public enum GlyphFlags : int {
        OnCurve = 1,
        xShort = 2,
        yShort = 4,
        Repeat = 8,
        xSame = 16,
        ySame = 32,
    }

    public enum PlotMode : int {
        Normal = 1,
        Circle = 2,
    }

    public enum TextAlignment : int {
        Left = 1,
        Right = 2,
        Center = 3,
        Justified = 4
    }

    public enum RotationAnchor : int {
        Origin = 1,
        CenterBaseline = 2
    }

    /// <summary>Defines the outline of a glyph character within the font.</summary>
    public class Glyph {
        OpenFont m_font;
        int gid;
        int minX;
        int minY;
        int maxX;
        int maxY;
        int width;
        int lsb;
        int[] instructions;
        List<Shape> shapes;

        public Glyph(OpenFont font, int id, HorizontalMetrics metrics) {
            shapes = new List<Shape>();
            m_font = font;
            gid = id;
            width = metrics.advanceWidth;
            lsb = metrics.lsb;
        }

        /// <summary>Gets the parent font.</summary>
        public OpenFont Font {
            get { return m_font; }
        }
        /// <summary>Gets the bezier quality level.</summary>
        public int Quality {
            get { return Font.Quality; }
        }
        /// <summary>Gets the optimization tolerance.</summary>
        public float Tolerance {
            get { return Font.Tolerance; }
        }
        /// <summary>Gets the global id associated with this glyph.</summary>
        public int ID {
            get { return gid; }
        }
        /// <summary>Gets the minimum X boundary for the entire glyph across all shapes.</summary>
        public int MinX {
            get { return minX; }
        }
        /// <summary>Gets the minimum Y boundary for the entire glyph across all shapes.</summary>
        public int MinY {
            get { return minY; }
        }
        /// <summary>Gets the maximum X boundary for the entire glyph across all shapes.</summary>
        public int MaxX {
            get { return maxX; }
        }
        /// <summary>Gets the maximum Y boundary for the entire glyph across all shapes.</summary>
        public int MaxY {
            get { return maxY; }
        }
        /// <summary>Gets the width of the glyph</summary>
        public int Width {
            get { return width; }
        }
        /// <summary>Gets the left side bearing of the glyph</summary>
        public int LSB {
            get { return lsb; }
        }
        /// <summary>Returns an enumerator that iterates through the glyph shape collection.</summary>
        public List<Shape>.Enumerator GetEnumerator() {
            return shapes.GetEnumerator();
        }
        /// <summary>Gets the shape at the specified index.</summary>
        public Shape this[int index] {
            get { return shapes[index]; }
        }
        /// <summary>Gets the hint instructions defined for this glyph.</summary>
        public int[] InstructionList {
            get { return instructions; }
        }
        /// <summary>Updates cached shape data when the bezier quality changes.</summary>
        public void Update() {
            foreach (Shape shape in shapes) {
                if (shape != null)
                    shape.ClearShape();
            }
        }

        /// <summary>Reads the binary glyph data from the stream.</summary>
        public void ReadStream(Stream s) {
            try {
                int contours = Tools.ReadShort(s);
                minX = Tools.ReadShort(s);
                minY = Tools.ReadShort(s);
                maxX = Tools.ReadShort(s);
                maxY = Tools.ReadShort(s);

                if (contours >= 0)
                    ReadSimpleGlyph(s, contours);
                //else //composite glyphs are negative
                //   Debug.WriteLine("-- SKIPPING COMPOSITE GLYPH " + gid);
            } catch (Exception ex) {
                Debug.WriteLine("error reading glyph " + gid + ": " + ex.Message);
            }
        }

        // Processes contour data for simple glyphs
        private void ReadSimpleGlyph(Stream s, int contours) {
            int i, j, totalPoints, last_pos, len;
            PointF[] points, shapepoints;
            GlyphFlags[] flags, shapeflags;
            Shape shape;
            int[] endPoints;

            //For each contour, read the end point. 
            //shapes = new Shape[contours];
            endPoints = new int[contours];
            for (i = 0; i < contours; ++i) {
                endPoints[i] = Tools.ReadUShort(s);
                if (i != 0 && endPoints[i] < endPoints[i - 1]) {
                    Debug.WriteLine("WARNING: bad contour detected for glyph " + gid);
                    return;
                }
            }

            //determine how many coordinate points are defined by this glyph
            if (contours == 0) {
                totalPoints = 0;
                points = new PointF[1];
            } else {
                totalPoints = endPoints[contours - 1] + 1;
                points = new PointF[totalPoints];
            }

            //read the instruction set
            int instructionLength = Tools.ReadUShort(s);
            instructions = new int[instructionLength];
            for (i = 0; i < instructionLength; ++i)
                instructions[i] = s.ReadByte();

            //read the flags for each coordinate point in the glyph
            flags = new GlyphFlags[totalPoints];
            for (i = 0; i < totalPoints; i++) {
                flags[i] = (GlyphFlags)s.ReadByte();
                //if the flag is suppose to be repeated then process it now
                if ((flags[i] & GlyphFlags.Repeat) > 0) {
                    int cnt = s.ReadByte();
                    if (i + cnt >= totalPoints) {
                        Debug.WriteLine("WARNING: Incorrect flag count for glyph " + gid);
                        cnt = totalPoints - i - 1;
                    }
                    for (j = 0; j < cnt; ++j)
                        flags[i + j + 1] = flags[i];
                    i += cnt;
                }
            }

            //extract the X coordinate data
            last_pos = 0;
            for (i = 0; i < totalPoints; ++i) {
                if ((flags[i] & GlyphFlags.xShort) > 0) {
                    int off = s.ReadByte();
                    if ((flags[i] & GlyphFlags.xSame) == 0)
                        off = -off;
                    points[i].X = last_pos + off;
                } else if ((flags[i] & GlyphFlags.xSame) > 0) {
                    points[i].X = last_pos;
                } else {
                    points[i].X = last_pos + Tools.ReadShort(s);
                }
                last_pos = (int)points[i].X;
            }

            //extract the Y coordinate data
            last_pos = 0;
            for (i = 0; i < totalPoints; ++i) {
                if ((flags[i] & GlyphFlags.yShort) > 0) {
                    int off = s.ReadByte();
                    if ((flags[i] & GlyphFlags.ySame) == 0)
                        off = -off;
                    points[i].Y = last_pos + off;
                } else if ((flags[i] & GlyphFlags.ySame) > 0) {
                    points[i].Y = last_pos;
                } else {
                    points[i].Y = last_pos + Tools.ReadShort(s);
                }
                last_pos = (int)points[i].Y;
            }

            //isolate the flag & coordinate data for each contour and forward it to the shape processing class
            last_pos = 0;
            for (i = 0; i < contours; ++i) {
                len = endPoints[i] - last_pos + 1;
                if (len > 1) { //only add contours with more than one point
                    shapepoints = new PointF[len];
                    shapeflags = new GlyphFlags[len];
                    Array.Copy(points, last_pos, shapepoints, 0, len);
                    Array.Copy(flags, last_pos, shapeflags, 0, len);
                    shape = new Shape(this, shapepoints, shapeflags);
                    shapes.Add(shape);
                }
                last_pos = endPoints[i] + 1;
            }
        }

        /// <summary>Rasterizes the glyph outline to the graphic buffer.</summary>
        public void Render(Graphics buffer, Pen pen, Point origin, float angle, RotationAnchor anchor) {
            foreach (Shape shape in shapes)
                shape.RenderShape(buffer, pen, origin, angle, anchor);
        }

        /// <summary>Translates the glyph outline based on the current point size and origin.</summary>
        public List<PointF[]> TranslateOutline(PointF origin, float angle, RotationAnchor anchor) {
            List<PointF[]> points = new List<PointF[]>();

            foreach (Shape shape in shapes)
                points.Add(shape.TranslateShape(origin, angle, anchor));
            return points;
        }

        /// <summary>DEBUG HELPER. Translates the glyph control points so that they can be rendered out for visual inspection.</summary>
        public List<PointF> TranslateOutlineControls(PointF origin, float angle, RotationAnchor anchor) {
            List<PointF> points = new List<PointF>();

            foreach (Shape shape in shapes)
                points.AddRange(shape.TranslateShapeControls(origin, angle, anchor));
            return points;
        }
    }

    /// <summary>Defines the coordinate data for a single glyph contour.</summary>
    public class Shape {
        private Glyph m_glyph;
        private List<PointF> m_shape;
        private List<PointF> m_controlpoints; //debug purposes
        private List<Segment> m_segments;
        private bool m_shapecached;
        private bool m_segmentcached;

        private PointF[] m_defpoints;
        private GlyphFlags[] m_defflags;

        private static float lastangle = 0;
        private static double cos = 0, sin = 0;

        public Shape(Glyph glyph, PointF[] points, GlyphFlags[] flags) {
            m_glyph = glyph;
            m_shape = new List<PointF>();
            m_controlpoints = new List<PointF>();
            m_segments = new List<Segment>();
            m_defpoints = points;
            m_defflags = flags;
            m_segmentcached = false;
            m_shapecached = false;
        }

        /// <summary>Builds the point cache for the shape</summary>
        public void BuildCache() {
            if (!m_segmentcached)
                CreateSegments(m_defpoints, m_defflags);
            if (!m_shapecached)
                CreateShape(m_glyph.Quality, m_glyph.Tolerance);
        }

        /// <summary>Build the segment list from the font data.</summary>
        private void CreateSegments(PointF[] definitionList, GlyphFlags[] flagList) {
            if (m_segmentcached)
                return;

            m_segmentcached = true;
            if (definitionList.Length < 2)
                return;
            if (definitionList.Length != flagList.Length)
                throw new OpenFontException(m_glyph.Font, "Unable to create segment becuase the flag does not match the definition list");

            //convert definition data to a list for easier manipulation
            List<PointF> deflist = new List<PointF>(definitionList);
            List<GlyphFlags> flaglist = new List<GlyphFlags>(flagList);

            //build the line/curve segments
            bool thisOnCurve = (flaglist[0] & GlyphFlags.OnCurve) > 0;
            bool lastOnCurve = true;
            if (!thisOnCurve) {
                //if the first point is a control point, then lets assume the last point is on-curve and close the spline in reverse
                deflist.Insert(0, deflist[deflist.Count - 1]);
                flaglist.Insert(0, flaglist[flaglist.Count - 1]);
                thisOnCurve = true;
            } else {
                //close the spline
                deflist.Add(deflist[0]);
                flaglist.Add(flaglist[0]);
            }

            //iterate over each coordinate point defined in the contour
            for (int i = 1; i < deflist.Count; ++i) {
                thisOnCurve = (flaglist[i] & GlyphFlags.OnCurve) > 0;

                if (lastOnCurve && thisOnCurve) {
                    //straight line with no control points
                    m_segments.Add(new Segment(
                       deflist[i - 1],
                       deflist[i]
                    ));
                } else if (!lastOnCurve) {
                    //this is a control point that is part of a bezier spline.
                    if (!thisOnCurve) {
                        //since this control point is consecutive with another control point, the
                        //  on-curve point must be interpolated since ttf only uses quadradic curves
                        PointF newpoint = new PointF(
                           (deflist[i - 1].X + deflist[i].X) / 2,
                           (deflist[i - 1].Y + deflist[i].Y) / 2
                        );
                        deflist.Insert(i, newpoint);
                        flaglist.Insert(i, GlyphFlags.OnCurve);
                        thisOnCurve = true;
                    }

                    //add the bezier curve segment to the list
                    m_segments.Add(new Segment(
                       deflist[i - 2],
                       deflist[i],
                       deflist[i - 1]
                    ));
                }
                lastOnCurve = thisOnCurve;
            }
        }

        /// <summary>Forces the shape cache to be rebuilt upon next use.</summary>
        public void ClearShape() {
            if (m_shapecached) {
                m_shape.Clear();
                m_controlpoints.Clear();
                m_shapecached = false;
            }
        }

        private float DistanceFromLine(PointF start, PointF end, PointF point) {
            float A = point.X - start.X,
                  B = point.Y - start.Y,
                  C = end.X - start.X,
                  D = end.Y - start.Y,

                  dot = A * C + B * D,
                  len_sq = C * C + D * D,
                  param = -1,
                  xx, yy;

            if (len_sq != 0)
                param = dot / len_sq;

            if (param < 0) {
                xx = start.X;
                yy = start.Y;
            } else if (param > 1) {
                xx = end.X;
                yy = end.Y;
            } else {
                xx = start.X + param * C;
                yy = start.Y + param * D;
            }

            var dx = point.X - xx;
            var dy = point.Y - yy;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private bool PointOnLine(PointF start, PointF end, PointF point, float tolerance = 0.001f) {
            if (DistanceFromLine(start, end, point) < tolerance)
                return true;
            return false;
        }

        /// <summary>Translate the bezier curves into coordinate data.</summary>
        private void CreateShape(int Quality, float tolerance) {
            if (m_shapecached)
                return;
            m_shapecached = true;

            PointF lastpoint = PointF.Empty;
            foreach (Segment segment in m_segments) {
                if (lastpoint == PointF.Empty || !PointOnLine(lastpoint, segment.End, segment.Start, tolerance)) {
                    m_shape.Add(segment.Start);
                    lastpoint = segment.Start;
                    if (Quality > 0 && segment.HasControl) {
                        if (!PointOnLine(segment.Start, segment.End, segment.Control, tolerance)) {
                            //Calculate the bezier tween points
                            float qbase = 1f / (Quality + 1);
                            for (int i = 1; i <= Quality; i++) {
                                float t = (qbase * i);
                                PointF newpoint = new PointF(
                                   ((1 - t) * (1 - t)) * segment.Start.X + 2 * t * (1 - t) * segment.Control.X + t * t * segment.End.X,
                                   ((1 - t) * (1 - t)) * segment.Start.Y + 2 * t * (1 - t) * segment.Control.Y + t * t * segment.End.Y
                                );
                                m_shape.Add(newpoint);
                                lastpoint = newpoint;
                            }
                            m_controlpoints.Add(segment.Control);
                        }
                    }
                }
            }
            m_shape.Add(m_shape[0]); //close it
        }

        /// <summary>Draws a visual representation of the underlying control data for debugging purposes.</summary>
        /// <param name="g">Graphic buffer to draw to</param>
        /// <param name="origin">Coordinate origin of the glyph</param>
        /// <param name="pointSize">Point size of the glyph</param>
        public void RenderGuide(Graphics g, PointF origin, float angle, RotationAnchor anchor) {
            BuildCache();
            if (m_segments.Count == 0)
                return;

            PointF pt;
            float scale = m_glyph.Font.GlyphScale;
            Pen guideLines = new Pen(Color.Blue);
            Pen guideControlPoints = new Pen(Color.Red);

            foreach (Segment segment in m_segments) {
                g.DrawLine(guideLines, TranslatePoint(segment.Start, origin, scale, angle, anchor), TranslatePoint(segment.End, origin, scale, angle, anchor));
                if (segment.HasControl) {
                    pt = TranslatePoint(segment.Control, origin, scale, angle, anchor);
                    g.DrawRectangle(guideControlPoints, pt.X - 1, pt.Y - 1, 2, 2);
                }
            }
        }

        /// <summary>Rasterizes the contour to the buffer</summary>
        /// <param name="g">Graphic buffer to draw to</param>
        /// <param name="p">Pen of the outline</param>
        /// <param name="origin">Coordinate origin of the glyph</param>
        /// <param name="pointSize">Point size of the glyph</param>
        public void RenderShape(Graphics g, Pen p, PointF origin, float angle, RotationAnchor anchor) {
            BuildCache();
            if (m_shape.Count == 0)
                return;
            g.DrawLines(p, TranslateShape(origin, angle, anchor));
        }

        /// <summary>Translates the glyph shape based on the current point size, origin, and angle.</summary>
        public PointF[] TranslateShape(PointF origin, float angle, RotationAnchor anchor) {
            BuildCache();
            if (m_shape.Count == 0)
                return null;

            float scale = m_glyph.Font.GlyphScale;
            PointF[] points = new PointF[m_shape.Count];
            for (int i = 0; i < m_shape.Count; i++)
                points[i] = TranslatePoint(m_shape[i], origin, scale, angle, anchor);
            return points;
        }

        /// <summary>DEBUG HELPER. Translates the glyph control points based on the current point size, origin, and angle.</summary>
        public PointF[] TranslateShapeControls(PointF origin, float angle, RotationAnchor anchor) {
            if (m_controlpoints.Count == 0)
                return new PointF[0];

            float scale = m_glyph.Font.GlyphScale;
            PointF[] points = new PointF[m_controlpoints.Count];
            for (int i = 0; i < m_controlpoints.Count; i++) {
                points[i] = TranslatePoint(m_controlpoints[i], origin, scale, angle, anchor);
            }
            return points;
        }

        // helper function to perform the basic point transformation
        private PointF TranslatePoint(PointF point, PointF origin, float scale, float angle, RotationAnchor anchor) {
            float ox = origin.X;
            float oy = origin.Y;

            float w = ((m_glyph.MinX - m_glyph.LSB) + (m_glyph.Width / 2)) * scale;
            //int w = (int)((m_glyph.Width / 2) * scale);

            float x = ox + (point.X * scale);
            if (anchor == RotationAnchor.CenterBaseline)
                x -= w;
            float y = oy + (point.Y * scale);

            if (angle != 0) {
                //cache the sin/cos globally if the angle has changed.
                if (angle != lastangle) {
                    sin = Math.Sin(angle);
                    cos = Math.Cos(angle);
                    lastangle = angle;
                }

                //apply the transformation to the point
                return new PointF(
                   (float)(((x - ox) * cos) - ((y - oy) * sin) + ox),
                   (float)(((x - ox) * sin) + ((y - oy) * cos) + oy)
                );
            } else {
                //no rotation, so just return the scaled point
                return new PointF(x, y);
            }
        }
    }

    /// <summary>Defines a line or bezier curve segment within a glyph shape.</summary>
    public class Segment {
        private PointF m_start;
        private PointF m_end;
        private PointF m_control;
        private bool hasControl;

        public Segment(PointF start, PointF end) {
            m_start = start;
            m_end = end;
            hasControl = false;
        }
        public Segment(PointF start, PointF end, PointF control) {
            m_start = start;
            m_end = end;
            m_control = control;
            hasControl = true;
        }

        public PointF Start { get { return m_start; } }
        public PointF End { get { return m_end; } }
        public PointF Control { get { return m_control; } }
        public bool HasControl { get { return hasControl; } }
    }

    /// <summary>Represents errors that occur during font processing.</summary>
    public class OpenFontException : Exception {
        private OpenFont owner;
        public OpenFontException() : base() { }
        public OpenFontException(OpenFont Owner, string message) : base(message) { owner = Owner; }
        public OpenFontException(OpenFont Owner, string message, Exception innerException) : base(message, innerException) { owner = Owner; }
        public OpenFont Owner { get { return owner; } }
    }
}