using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using CodeImp.DoomBuilder;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using CodeImp.DoomBuilder.Windows;
using TriDelta.OpenType;
using CodeImp.DoomBuilder.Controls;

namespace TriDelta.DrawTextMode {
    internal delegate void ModeChangedEvent(DrawTextMode mode);

    [EditMode(
       DisplayName = "Draw Text Mode",
       SwitchAction = "drawtextmode",
       ButtonImage = "icon.png",
       ButtonOrder = 502,
       ButtonGroup = "000_editing",
       UseByDefault = true,
       SafeStartMode = false,
       Volatile = true
    )]
    public class DrawTextMode : ClassicMode {
        private const float LINE_THICKNESS = 0.8f;
        private const float GRIP_SIZE = 9.0f;


        BuilderPlug plug;

        List<List<DrawnVertex>> shapecache;

        Docker dockerDrawText;
        DrawTextPanel panel;

        private LineLengthLabel labelGuideLength;

        private ControlHandle handleInner;
        private ControlHandle handleOuter;
        private ControlHandle handleCurrent;

        private PixelColor pcLineSnap = General.Colors.Selection;
        private PixelColor pcLineFree = General.Colors.Highlight;
        private PixelColor pcHandleFill = General.Colors.Background;

        private bool snapguidetogrid;
        private bool isCreating;

        internal event ModeChangedEvent ModeChanged; 

        public BuilderPlug Plug {
            get { return plug; }
        }

        public DrawTextMode() {
            plug = BuilderPlug.Me;
            plug.EditMode = this;

            labelGuideLength = new LineLengthLabel(true);

            shapecache = new List<List<DrawnVertex>>();

            panel = new DrawTextPanel(this);
            dockerDrawText = new Docker("drawtextmode", "Draw Text", panel);
        }

        public string Text {
            get { return plug.DisplayText; }
            set {
                if (value != plug.DisplayText) {
                    plug.DisplayText = value;
                    Update();
                }
            }
        }
        public float TextSize {
            get { return plug.Size; }
            set {
                if (value != plug.Size) {
                    plug.Size = value;
                    if (plug.Size < 0)
                        plug.Size = 0;
                    if (plug.Font != null)
                        plug.Font.PointSize = value;
                    Update();
                    if (ModeChanged != null)
                        ModeChanged(this);
                }
            }
        }
        public OpenFont TextFont {
            get { return plug.Font; }
            set {
                if (value != plug.Font) {
                    plug.Font = value;
                    if (value != null) {
                        value.PointSize = plug.Size;
                        value.Quality = plug.CurveQuality;
                    }
                    Update();
                }
            }
        }
        public int Quality {
            get { return plug.CurveQuality; }
            set {
                if (value != plug.CurveQuality) {
                    plug.CurveQuality = value;
                    if (plug.Font != null)
                        plug.Font.Quality = value;
                    Update();
                }
            }
        }
        public PlotMode DrawMode {
            get { return plug.PlotMode; }
            set {
                if (value != plug.PlotMode) {
                    plug.PlotMode = value;
                    Update();
                }
            }
        }
        public TextAlignment Alignment {
            get { return plug.TextAlignment; }
            set {
                if (value != plug.TextAlignment) {
                    plug.TextAlignment = value;
                    Update();
                }
            }
        }

        public float TextSpacing {
            get { return plug.TextSpacing; }
            set {
                if (value != plug.TextSpacing) {
                    plug.TextSpacing = value;
                    Update();
                }
            }
        }

        [BeginAction("incfontsize")]
        public void IncreaseFontSize() {
            if (handleInner != null && handleOuter != null)
                TextSize++;
        }

        [BeginAction("decfontsize")]
        public void DecreaseFontSize() {
            if (handleInner != null && handleOuter != null)
                TextSize--;
        }

        public override void OnEngage() {
            base.OnEngage();
            General.Interface.AddDocker(dockerDrawText);
            renderer.SetPresentation(Presentation.Standard);
        }

        public override void OnDisengage() {
            plug.Save();
            General.Interface.RemoveDocker(dockerDrawText);
            base.OnDisengage();
        }

        public override void OnRedrawDisplay() {
            renderer.RedrawSurface();
            if (renderer.StartPlotter(true)) {
                renderer.PlotLinedefSet(General.Map.Map.Linedefs);
                renderer.PlotVerticesSet(General.Map.Map.Vertices);
                renderer.Finish();
            }
            if (renderer.StartThings(true)) {
                renderer.RenderThingSet(General.Map.Map.Things, 1.0f);
                renderer.Finish();
            }
            Render();
        }

        public void Update() {
            snapguidetogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;

            //only create geometry if both control handles have been set and a font has been selected
            if (handleInner != null && handleOuter != null && plug.Font != null) {
                PointF origin = new PointF(handleInner.Position.x, handleInner.Position.y);
                PointF target = new PointF(handleOuter.Position.x, handleOuter.Position.y);

                //plot the string and convert the shape points into something doombuilder can use
                shapecache.Clear();
                List<PointF[]> lines = plug.Font.PlotString(plug.DisplayText, origin, target, plug.PlotMode, plug.TextAlignment, plug.TextSpacing);
                if (lines != null)
                    shapecache.AddRange(ConvertPoints(lines));
            }

            Render();
        }

        public void Render() {
            if (renderer.StartOverlay(true)) {
                float vsize = ((float)renderer.VertexSize + 1.0f) / renderer.Scale;

                //draw a guide circle when drawing circular text
                if (plug.PlotMode == PlotMode.Circle && handleInner != null && handleOuter != null) {
                    //Calculate the angle and circle starting point
                    Vector2D delta = handleOuter.Position - handleInner.Position;
                    int sides = 32;
                    float length = delta.GetLength();
                    float originRads = (float)Math.Atan2(handleInner.Position.y - handleOuter.Position.y, handleInner.Position.x - handleOuter.Position.x);
                    float pointRads = (float)((Math.PI / (double)sides) * 2);

                    Vector2D first, last, current;

                    first = last = new Vector2D(
                        handleInner.Position.x - (float)(Math.Cos(originRads) * length),
                        handleInner.Position.y - (float)(Math.Sin(originRads) * length)
                    );
                    for (int i = 0; i < sides; i++) {
                        //calculate where the vertex should go, based on the number of segments
                        float rads = originRads + (pointRads * (float)i);
                        current = new Vector2D(
                            handleInner.Position.x - (float)(Math.Cos(rads) * length),
                            handleInner.Position.y - (float)(Math.Sin(rads) * length)
                        );

                        //measure the segment for the total length display
                        renderer.RenderLine(last, current, LINE_THICKNESS, pcLineFree, true);
                        last = current;
                    }
                    renderer.RenderLine(last, first, LINE_THICKNESS, pcLineFree, true);
                }

                //Render the text glyph outlines
                if (shapecache.Count > 0) {
                    Vector2D first, last;
                    foreach (List<DrawnVertex> shape in shapecache) {
                        //draw the preview linedefs
                        first = last = shape[0].pos;
                        for (int i = 1; i < shape.Count; i++) {
                            renderer.RenderLine(last, shape[i].pos, LINE_THICKNESS, pcLineFree, true);
                            last = shape[i].pos;
                        }
                        renderer.RenderLine(last, first, LINE_THICKNESS, pcLineFree, true);

                        //draw the preview vertices
                        foreach(DrawnVertex p in shape)
                            RenderPoint(p.pos, vsize, pcLineSnap);
                    }
                }

                //Render Guide
                if (handleInner != null) {
                    //guide line
                    renderer.RenderLine(handleInner.Position, handleOuter.Position, LINE_THICKNESS, snapguidetogrid ? pcLineSnap : pcLineFree, true);
                    float gripsize = GRIP_SIZE / renderer.Scale;

                    //size text
                    labelGuideLength.Start = handleInner.Position;
                    labelGuideLength.End = handleOuter.Position;
                    renderer.RenderText(labelGuideLength.TextLabel);

                    //control handles
                    RectangleF handleRect = new RectangleF(handleInner.Position.x - gripsize * 0.5f, handleInner.Position.y - gripsize * 0.5f, gripsize, gripsize);
                    renderer.RenderRectangleFilled(handleRect, pcHandleFill, true);
                    renderer.RenderRectangle(handleRect, 2, snapguidetogrid ? pcLineSnap : pcLineFree, true);

                    handleRect = new RectangleF(handleOuter.Position.x - gripsize * 0.5f, handleOuter.Position.y - gripsize * 0.5f, gripsize, gripsize);
                    renderer.RenderRectangleFilled(handleRect, pcHandleFill, true);
                    renderer.RenderRectangle(handleRect, 2, snapguidetogrid ? pcLineSnap : pcLineFree, true);
                }

                renderer.Finish();
            }
            renderer.Present();
        }

        private void RenderPoint(DrawnVertex p, float size, PixelColor color) {
            RenderPoint(p.pos, size, color);
        }
        private void RenderPoint(Vector2D p, float size, PixelColor color) {
            renderer.RenderRectangleFilled(new RectangleF(p.x - size, p.y - size, size * 2.0f, size * 2.0f), color, true);
        }

        private List<List<DrawnVertex>> ConvertPoints(List<PointF[]> shapeList) {
            DrawnVertex v;
            List<List<DrawnVertex>> shapes = new List<List<DrawnVertex>>();
            List<DrawnVertex> shape;

            foreach (PointF[] points in shapeList) {
                shape = new List<DrawnVertex>();
                for (int i = 0; i < points.Length; i++) {
                    v = new DrawnVertex();
                    v.stitch = true;
                    v.stitchline = true;
                    v.pos.x = points[i].X;
                    v.pos.y = points[i].Y;
                    shape.Add(v);
                }
                shape.Add(shape[0]); //close

                shapes.Add(shape);
            }
            return shapes;
        }

        protected override void OnEditBegin() {
            base.OnEditBegin();

            if (shapecache.Count == 0) {
                //this is a brand new circle, so setup the control handles and do initial shape creation and rendering
                isCreating = true;
                snapguidetogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;                                  //if control is held then only flip the circle

                //initialize the guide line
                handleInner = new ControlHandle();
                handleOuter = new ControlHandle();
                handleInner.Position = handleOuter.Position = GetCurrentPosition();

                Update();
            } else if (handleInner != null) {
                //if we are already in preview mode, then only allow the control handles to be dragged
                float gripsize = GRIP_SIZE / renderer.Scale;
                if (handleOuter.isHovered(MouseMapPos, gripsize)) {
                    handleCurrent = handleOuter;
                    General.Interface.SetCursor(Cursors.Cross);
                    return;
                } else if (handleInner.isHovered(MouseMapPos, gripsize)) {
                    handleCurrent = handleInner;
                    General.Interface.SetCursor(Cursors.Cross);
                    return;
                }
            }
        }

        protected override void OnEditEnd() {
            base.OnEditEnd();

            if (isCreating) {
                //if the opening edit was cancelled before letting go of the mouse then do no further processing
                isCreating = false;

                handleOuter.Position = GetCurrentPosition();
                if ((handleOuter.Position - handleInner.Position).GetLength() == 0) {
                    handleInner = null;
                    handleOuter = null;
                    return;
                }

                Update();
            } else {
                //since we were just dragging an existing handle, just let go of it
                General.Interface.SetCursor(Cursors.Default);
                handleCurrent = null;
            }
        }

        protected override void OnDragStart(MouseEventArgs e) {
            base.OnDragStart(e);

            //this differs from "EditStart", as this is a PRIMARY mouse click, rather than a SECONDARY one.
            if (handleInner != null) {
                float gripsize = GRIP_SIZE / renderer.Scale;
                if (handleOuter.isHovered(MouseDownMapPos, gripsize)) {
                    handleCurrent = handleOuter;
                    General.Interface.SetCursor(Cursors.Cross);
                    return;
                } else if (handleInner.isHovered(MouseDownMapPos, gripsize)) {
                    handleCurrent = handleInner;
                    General.Interface.SetCursor(Cursors.Cross);
                    return;
                }
            }

            handleCurrent = null;
        }

        protected override void OnDragStop(MouseEventArgs e) {
            base.OnDragStop(e);
            General.Interface.SetCursor(Cursors.Default);
            handleCurrent = null;
        }

        public override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            float gripsize = GRIP_SIZE / renderer.Scale;
            if (handleCurrent != null) {
                handleCurrent.Position = GetCurrentPosition();
                Update();
            } else if (isCreating) {
                handleOuter.Position = GetCurrentPosition();
                Update();
            } else if (handleOuter != null && handleOuter.isHovered(MouseMapPos, gripsize)) {
                General.Interface.SetCursor(Cursors.Hand);
            } else if (handleInner != null && handleInner.isHovered(MouseMapPos, gripsize)) {
                General.Interface.SetCursor(Cursors.Hand);
            } else {
                General.Interface.SetCursor(Cursors.Default);
            }
        }

        public override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            Update();
        }

        public override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            Update();
            if (handleInner != null && e.KeyCode == Keys.Enter)
                OnAccept();
        }

        public override void OnCancel() {
            base.OnCancel();

            isCreating = false;
            handleInner = null;
            handleOuter = null;
            shapecache.Clear();

            Render();
        }

        public override void OnAccept() {
            Cursor.Current = Cursors.AppStarting;
            General.Settings.FindDefaultDrawSettings();

            // When points have been drawn
            if (shapecache.Count > 0) {
                // Make undo for the draw
                General.Map.UndoRedo.CreateUndo("Draw Text");

                // Make the drawing
                foreach (List<DrawnVertex> shape in shapecache) {
                    //if the user holds down ALT while creating, assume they want a "guide" for positioning other map elements. A guide will not split linedefs.
                    if (!General.Interface.AutoMerge || General.Interface.AltState) {
                        int j = shape.Count;
                        DrawnVertex v;
                        while (j-- > 0) {
                            v = shape[j];
                            v.stitchline = false; //don't split any linedefs

                            //don't complete the shape if ALT mode, to prevent sector creation and background fill
                            if (General.Interface.AltState)
                                v.stitch = false;

                            shape[j] = v;
                        }
                    }

                    // Make the drawing
                    CodeImp.DoomBuilder.Geometry.Tools.DrawLines(shape);
                }

                // Snap to map format accuracy
                General.Map.Map.SnapAllToAccuracy();

                // Clear selection
                General.Map.Map.ClearAllSelected();

                // Update cached values
                General.Map.Map.Update();

                // Update the used textures
                General.Map.Data.UpdateUsedTextures();

                // Map is changed
                General.Map.IsChanged = true;
            }

            // Done
            Cursor.Current = Cursors.Default;

            isCreating = false;
            handleInner = null;
            handleOuter = null;
            shapecache.Clear();

            OnRedrawDisplay();
        }

        private Vector2D GetCurrentPosition() {
            Vector2D vm = MouseMapPos;
            float vrange = 20f / renderer.Scale;

            // Try the nearest vertex
            Vertex nv = General.Map.Map.NearestVertexSquareRange(vm, vrange);
            if (nv != null)
                return nv.Position;

            // Try the nearest linedef
            Linedef nl = General.Map.Map.NearestLinedefRange(vm, vrange);
            if (nl != null) {
                // Snap to grid?
                if (snapguidetogrid) {
                    // Get grid intersection coordinates
                    List<Vector2D> coords = nl.GetGridIntersections();

                    // Find nearest grid intersection
                    bool found = false;
                    float found_distance = float.MaxValue;
                    Vector2D found_coord = new Vector2D();
                    foreach (Vector2D v in coords) {
                        Vector2D delta = vm - v;
                        if (delta.GetLengthSq() < found_distance) {
                            found_distance = delta.GetLengthSq();
                            found_coord = v;
                            found = true;
                        }
                    }

                    if (found)
                        return found_coord;
                } else {
                    return nl.NearestOnLine(vm);
                }
            }

            //Just get the current mouse location instead
            if (snapguidetogrid)
                return General.Map.Grid.SnappedToGrid(vm);
            return vm;
        }
    }

    //holder class for the guide drag handles
    internal class ControlHandle {
        public Vector2D Position;

        public ControlHandle() { }
        public ControlHandle(Vector2D position) {
            this.Position = position;
        }
        public bool isHovered(Vector2D position, float size) {
            return position.x <= this.Position.x + size && position.x >= this.Position.x - size && position.y <= this.Position.y + size && position.y >= this.Position.y - size;
        }
    }
}