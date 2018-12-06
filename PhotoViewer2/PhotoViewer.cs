using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Cincinnatus
{
    public partial class PhotoViewer : Form
    {
        /// <summary>
        /// gets or sets the image to display
        /// </summary>
        private Image Image = null;

        /// <summary>
        /// Top-left corner of image on display
        /// </summary>
        private PointF ImageOrigin = PointF.Empty;
        /// <summary>
        /// Scale by which to enlarge image on display
        /// </summary>
        private float ImageScale = 1.0f;

        /// <summary>
        /// The interpolation mode to use for drawing image.
        /// </summary>
        private InterpolationMode InterpolationMode = InterpolationMode.NearestNeighbor;

        /// <summary>
        /// The pixel offset mode to use for drawing the image
        /// </summary>
        private PixelOffsetMode PixelOffsetMode = PixelOffsetMode.Half;

        public PhotoViewer()
        {
            InitializeComponent();

            // generate context menu
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            // open image button
            contextMenuStrip.Items.Add(new ToolStripMenuItem("Open", null, (o, e) => PromptOpenImage()));

            // create the drop-down for the InterpolationMode values and link them to context menu
            ToolStripMenuItem interpolationModes = new ToolStripMenuItem("Interpolation", null);
            contextMenuStrip.Items.Add(interpolationModes);

            foreach (InterpolationMode mode in Enum.GetValues(typeof(InterpolationMode)))
                if (mode != InterpolationMode.Invalid && mode != InterpolationMode.Default)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(Utility.TransformCammelCase(mode.ToString()), null);
                    item.Checked = InterpolationMode == mode;
                    item.Click += (o, e) =>
                    {
                        InterpolationMode = mode;

                        foreach (ToolStripMenuItem other in interpolationModes.DropDownItems)
                            other.Checked = false;
                        item.Checked = true;

                        Invalidate();
                    };

                    interpolationModes.DropDownItems.Add(item);
                }  

            // create drop-down for background color values
            ToolStripMenuItem colorModes = new ToolStripMenuItem("Background", null);
            contextMenuStrip.Items.Add(colorModes);

            foreach (KeyValuePair<Color, string> entry in new List<KeyValuePair<Color, string>>()
                {
                    new KeyValuePair<Color, string>(Color.Black, "Black"),
                    new KeyValuePair<Color, string>(Color.White, "White"),
                })
            {
                ToolStripMenuItem item = new ToolStripMenuItem(entry.Value, null);
                item.Checked = BackColor == entry.Key;
                item.Click += (o, e) =>
                {
                    BackColor = entry.Key;

                    foreach (ToolStripMenuItem other in colorModes.DropDownItems)
                        other.Checked = false;
                    item.Checked = true;
                };

                colorModes.DropDownItems.Add(item);
            }
            
            // allow user to reset view (e.g. zoom out too far and lose image)
            contextMenuStrip.Items.Add(new ToolStripMenuItem("Reset View", null, (o, e) => ResetViewFit()));

            // allow user to set zoom to actual image size
            contextMenuStrip.Items.Add(new ToolStripMenuItem("Actual Size", null, (o, e) => ResetViewTrueSize()));

            // launch another instance of the viewer (not tied to this one)
            contextMenuStrip.Items.Add(new ToolStripMenuItem("New Window", null, (o, e) => Process.Start(new ProcessStartInfo(Application.ExecutablePath))));

            ContextMenuStrip = contextMenuStrip;

            // initialize null image
            SetImage(null, null);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Image != null)
            {
                float imageWidth = Image.Width * ImageScale;
                float imageHeight = Image.Height * ImageScale;

                e.Graphics.InterpolationMode = InterpolationMode;
                e.Graphics.PixelOffsetMode = PixelOffsetMode;

                e.Graphics.DrawImage(Image, new RectangleF(ImageOrigin.X, ImageOrigin.Y, imageWidth, imageHeight));
            }
        }

        /// <summary>
        /// Time to pause between movement frames
        /// </summary>
        private const int DragInterval = 13;
        protected async override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // if there's not an image, there's no need to scroll
            if (Image == null) return;

            PointF origStart = ImageOrigin;
            Point start = e.Location, last = e.Location;
            while (MouseButtons == MouseButtons.Left)
            {
                await Task.Delay(DragInterval);

                // ignore frames where the mouse didn't move
                Point now = PointToClient(MousePosition);
                if (now == last) continue;
                last = now;

                // set new origin and redraw canvas
                ImageOrigin = new PointF(origStart.X + now.X - start.X, origStart.Y + now.Y - start.Y);
                Invalidate();
            }
        }

        /// <summary>
        /// Multiplier applied when zooming in one tick
        /// </summary>
        private const float ZoomMod = 1.25f;
        /// <summary>
        /// Limit for how far viewer will zoom
        /// </summary>
        private const float MaxZoom = 1000f, MinZoom = 0.0001f;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // if there's not an image, there's no need to zoom
            if (Image == null) return;

            // vector equations to determine how to reposition origin

            // act = orig1 + pos * scale1
            // act = orig2 + pos * scale2
            // 0 = orig1 - orig2 + pos * (scale1 - scale2)
            // orig2 = orig1 + pos * (scale1 - scale2)

            // pos = (act - orig1) / scale1

            // orig2 = orig1 + (scale1 - scale2) * (act - orig1) / scale1
            // orig2 = orig1 + (1 - scale2 / scale1) * (act - orig1)

            // scale after operation
            float newScale = (e.Delta > 0 ? ImageScale * ZoomMod : ImageScale / ZoomMod).Clamp(MinZoom, MaxZoom);

            // pre-compute this for efficiency
            float scaleFactor = (1f - newScale / ImageScale);

            ImageOrigin = new PointF(
                ImageOrigin.X + scaleFactor * (e.Location.X - ImageOrigin.X),
                ImageOrigin.Y + scaleFactor * (e.Location.Y - ImageOrigin.Y));

            ImageScale = newScale;

            // redraw canvas
            Invalidate();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            ResetViewFit();
        }

        /// <summary>
        /// Centers the display image and zooms the image to fit in the display window.
        /// Additionally redraws the display.
        /// </summary>
        private void ResetViewFit()
        {
            // can't center display if image is null. but we will redraw
            if (Image != null)
            {
                float imgWidth = Image.Width, imgHeight = Image.Height;

                // remove half a pixel from virtual size if not using a pixel-perfect offset mode
                if (PixelOffsetMode != PixelOffsetMode.HighQuality && PixelOffsetMode != PixelOffsetMode.Half)
                {
                    imgWidth -= 0.5f;
                    imgHeight -= 0.5f;
                }

                ImageScale = Math.Min(
                    ClientRectangle.Width / imgWidth,
                    ClientRectangle.Height / imgHeight);
                ImageOrigin = new PointF(
                    (ClientRectangle.Width - imgWidth * ImageScale) / 2f,
                    (ClientRectangle.Height - imgHeight * ImageScale) / 2f);
            }

            Invalidate();
        }
        /// <summary>
        /// Centers the display image and zooms to the actual image size.
        /// Additionally redraws the display.
        /// </summary>
        private void ResetViewTrueSize()
        {
            // can't center display if image is null. but we will redraw
            if (Image != null)
            {
                float imgWidth = Image.Width, imgHeight = Image.Height;

                // remove half a pixel from virtual size if not using a pixel-perfect offset mode
                if (PixelOffsetMode != PixelOffsetMode.HighQuality && PixelOffsetMode != PixelOffsetMode.Half)
                {
                    imgWidth -= 0.5f;
                    imgHeight -= 0.5f;
                }

                ImageScale = 1;
                ImageOrigin = new PointF(
                    (ClientRectangle.Width - imgWidth) / 2f,
                    (ClientRectangle.Height - imgHeight) / 2f);
            }

            Invalidate();
        }

        /// <summary>
        /// Prompts the user to select an image then displays it if possible
        /// </summary>
        private void PromptOpenImage()
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Title = "Open Image";
                if (d.ShowDialog() == DialogResult.OK) SetImage(d.FileName);
            }
        }

        // ------------ //

        // -- public -- //

        // ------------ //

        /// <summary>
        /// Sets the image and display text for the PhotoViewer
        /// </summary>
        /// <param name="image">image to display (null for none)</param>
        /// <param name="title">title to display (null for none)</param>
        public void SetImage(Image image, string title)
        {
            Image = image;
            Text = title != null ? $"{Application.ProductName} - {title}" : Application.ProductName;

            ResetViewFit();
        }

        /// <summary>
        /// Sets the image and display text for the PhotoViewer to the file at path and the path, respectively.
        /// returns true if image found
        /// </summary>
        /// <param name="path">file path to image</param>
        public bool SetImage(string path)
        {
            Image img = Utility.TryGetImage(path);

            if (img == null)
            {
                MessageBox.Show($"Error reading {path}\n\nFile did not exist or was not an image", "Failed to Open Image",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            SetImage(img, Path.GetFullPath(path));
            return true;
        }
    }
}
