using System.Drawing;
using System.Windows.Forms;
using Netron.Diagramming.Core;

namespace LiveSequence.Shapes
{
    public class ImageRectangle : ComplexShapeBase
    {
        #region Fields
        /// <summary>
        /// holds the bottom connector
        /// </summary>
        Connector cBottom, cLeft, cRight, cTop;

        readonly bool isRoot;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the friendly name of the entity to be displayed in the UI
        /// </summary>
        /// <value></value>
        public override string EntityName
        {
            get { return "Image Rectangle"; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="s"></param>
        public ImageRectangle(IModel s)
            : base(s)
        {
            Init("", null);
        }
        
        public ImageRectangle(string text, Image icon)
        {
            Init(text, icon);
        }

        public ImageRectangle(string text, Image icon, bool isRoot)
        {
            this.isRoot = isRoot;
            Init(text, icon);
        }
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init(string text, Image icon)
        {
            // calculate the text length to adjust the size of rectangle
            SizeF size = TextRenderer.MeasureText(text, ArtPallet.DefaultFont);
            Rectangle r = new Rectangle(Rectangle.Location, Size.Round(Size.Add(size.ToSize(), new Size(icon.Width, icon.Height + 10))));
            this.Transform(r);

            cTop = new Connector(new Point(Rectangle.Left + Rectangle.Width / 2, Rectangle.Top), Model)
                       {
                           Name = "Top connector",
                           Parent = this
                       };
            Connectors.Add(cTop);

            cRight = new Connector(new Point(Rectangle.Right, Rectangle.Top + Rectangle.Height / 2), Model)
                         {
                             Name = "Right connector",
                             Parent = this
                         };
            Connectors.Add(cRight);

            cBottom = new Connector(new Point(Rectangle.Left + Rectangle.Width / 2, Rectangle.Bottom), Model)
                          {
                              Name = "Bottom connector",
                              Parent = this
                          };
            Connectors.Add(cBottom);

            cLeft = new Connector(new Point(Rectangle.Left, Rectangle.Top + Rectangle.Height / 2), Model)
                        {
                            Name = "Left connector",
                            Parent = this
                        };
            Connectors.Add(cLeft);

            ImageLabelMaterial ilab = new ImageLabelMaterial(text, icon) {Gliding = true, Resizable = false};
            // new Rectangle(Rectangle.X + 10, Rectangle.Y + 10, Rectangle.Width + 50, Rectangle.Height - 30)
            r.Inflate(-10, -5);
            ilab.Transform(r);
            Children.Add(ilab);

            Resizable = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tests whether the mouse hits this bundle
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool Hit(Point p)
        {
            Rectangle r = new Rectangle(p, new Size(5, 5));
            return Rectangle.Contains(r);
        }

        /// <summary>
        /// Paints the bundle on the canvas
        /// </summary>
        /// <param name="g"></param>
        public override void Paint(Graphics g)
        {
            /*
            Matrix or = g.Transform;
            Matrix m = new Matrix();
            m.RotateAt(20, Rectangle.Location);            
            g.MultiplyTransform(m, MatrixOrder.Append);
             */
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //the shadow
            //g.FillRectangle(ArtPallet.RullerFillBrush, Rectangle.X + 5, Rectangle.Y + 5, Rectangle.Width, Rectangle.Height);
            //the actual bundle
            if (isRoot)
            {
                g.FillRectangle(Brush, Rectangle);
            }
            //the edge of the bundle
            if (Hovered || IsSelected)
                g.DrawRectangle(ArtPallet.HighlightPen, Rectangle);
            else
                g.DrawRectangle(ArtPallet.BlackPen, Rectangle);
            //the connectors
            for (int k = 0; k < Connectors.Count; k++)
            {
                Connectors[k].Paint(g);
            }
            foreach (IShapeMaterial material in Children)
            {
                material.Paint(g);
            }



        }




        #endregion




    }
}
