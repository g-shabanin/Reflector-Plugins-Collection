using System.Drawing;
using System.Drawing.Drawing2D;
using Netron.Diagramming.Core;

namespace LiveSequence.Shapes
{
    public class ImageLabelMaterial : ShapeMaterialBase
    {

        /// <summary>
        /// The distance between the icon and the text.
        /// </summary>
        public const int constTextShift = 2;

        #region Fields
        /// <summary>
        /// the Text field
        /// </summary>
        private Bitmap mIcon;
        /// <summary>
        /// the Text field
        /// </summary>
        private string mText = string.Empty;

        private Rectangle textRectangle = Rectangle.Empty;
        private readonly StringFormat stringFormat = StringFormat.GenericTypographic;
        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the Text
        /// </summary>
        public string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
            }
        }

        /// <summary>
        /// Gets or sets the Text
        /// </summary>
        public Bitmap Icon
        {
            get
            {
                return mIcon;
            }
            set
            {
                mIcon = value;
            }
        }
        #endregion

        #region Constructor
        public ImageLabelMaterial(string text)
        {
            mText = text;
            stringFormat.Trimming = StringTrimming.EllipsisWord;
            stringFormat.FormatFlags = StringFormatFlags.LineLimit;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="IconLabelMaterial"/> class.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="text">The text.</param>
        public ImageLabelMaterial(string text, Image icon)
            : this(text)
        {
            mIcon = GetBitmap(icon);
        }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <returns></returns>
        protected static Bitmap GetBitmap(Image icon)
        {
            if (icon == null)
                return null;

            try
            {
                //first try if it's defined in this assembly somewhere                
                return new Bitmap(icon);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLabelMaterial"/> class.
        /// </summary>
        public ImageLabelMaterial()
        {
        }

        #endregion

        #region Methods
        public override void Transform(Rectangle rectangle)
        {
            //textRectangle = new Rectangle(rectangle.X + (mIcon == null ? 0 : mIcon.Width) + constTextShift, rectangle.Y, rectangle.Width - (mIcon == null ? 0 : mIcon.Width) - constTextShift, rectangle.Height);
            textRectangle = new Rectangle(
                rectangle.X + constTextShift, 
                rectangle.Y + (mIcon == null ? 0 : mIcon.Height) + constTextShift, 
                rectangle.Width + constTextShift,
                rectangle.Height + (mIcon == null ? 0 : mIcon.Height) + constTextShift
                );
            base.Transform(rectangle);

        }
        /// <summary>
        /// Paints the entity using the given graphics object
        /// </summary>
        /// <param name="g"></param>
        public override void Paint(Graphics g)
        {
            if (!Visible)
                return;
            GraphicsContainer cto = g.BeginContainer();
            g.SetClip(Shape.Rectangle);
            if (mIcon != null)
            {
                g.DrawImage(mIcon, new Rectangle(new Point(Rectangle.X, Rectangle.Y), mIcon.Size));
            }
            g.DrawString(mText, ArtPallet.DefaultFont, Brushes.Black, textRectangle, stringFormat);
            g.EndContainer(cto);
        }
        #endregion

    }
}
