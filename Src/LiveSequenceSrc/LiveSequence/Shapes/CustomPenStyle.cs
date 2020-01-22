using System.Drawing;
using System.Drawing.Drawing2D;
using Netron.Diagramming.Core;

namespace LiveSequence.Shapes
{
    /// <summary>
    /// Extended <see cref="PenStyle"/> with extra properties for the design of connections.
    /// <remarks>Although the .Net 2.0 framework allows you to set line caps the result is, humbly said, rather poor. The
    /// caps are not drawn proportionally and custom caps cannot be filled resulting in a 'not implemented' exception.
    /// So, the only working solution (read: hack) I found is to draw the caps with a secondary pen whose size is bigger than the base pen with which the
    /// line is drawn. Unfortunately this hack cannot solely be implemented in this class, you need to fix it in the painting routines.
    /// Hopefully a solution will be around later on.
    /// </remarks>
    /// </summary>
    public sealed class CustomPenStyle : PenStyle
    {
        public static CustomLineCap LabelPen
        {
            get
            {
                Point[] ps = new Point[3] { new Point(-2, 0), new Point(0, 4), new Point(2, 0) };
                GraphicsPath gpath = new GraphicsPath();
                gpath.AddPolygon(ps);
                gpath.CloseAllFigures();
                return new CustomLineCap(null, gpath);
            }
        }
    }
}