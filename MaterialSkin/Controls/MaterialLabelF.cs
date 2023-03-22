namespace MaterialSkin.Controls
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    [System.ComponentModel.DesignerCategory("")]
    public class MaterialLabelF : Label, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private ContentAlignment _TextAlign = ContentAlignment.TopLeft;

        [DefaultValue(typeof(ContentAlignment), "TopLeft")]
        public override ContentAlignment TextAlign
        {
            get
            {
                return _TextAlign;
            }
            set
            {
                _TextAlign = value;
                updateAligment();
                Invalidate();
            }
        }

        [Category("Material Skin"),
        DefaultValue(false)]
        public bool HighEmphasis { get; set; }

        [Category("Material Skin"),
        DefaultValue(false)]
        public bool UseAccent { get; set; }

        public bool Multiline = true;

        public bool UseCustomColor = false;

        private MaterialSkinManager.fontType _fontType = MaterialSkinManager.fontType.Body1;

        [Category("Material Skin"),
        DefaultValue(typeof(MaterialSkinManager.fontType), "Body1")]
        public MaterialSkinManager.fontType FontType
        {
            get
            {
                return _fontType;
            }
            set
            {
                _fontType = value;
                Font = SkinManager.getFontByType(_fontType, DeviceDpi);
                Refresh();
            }
        }

        public MaterialLabelF()
        {
            FontType = MaterialSkinManager.fontType.Body1;
            TextAlign = ContentAlignment.TopLeft;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (AutoSize)
            {
                Size strSize;
                using (NativeTextRenderer NativeText = new NativeTextRenderer(CreateGraphics()))
                {
                    strSize = NativeText.MeasureString(Text, Font);
                    strSize.Width += 1; // necessary to avoid a bug when autosize = true
                }
                return strSize;
            }
            else
            {
                return proposedSize;
            }
        }

        private NativeTextRenderer.TextAlignFlags Alignment;

        private void updateAligment()
        {
            switch (_TextAlign)
            {
                case ContentAlignment.TopLeft:
                    Alignment = NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Left;
                    break;

                case ContentAlignment.TopCenter:
                    Alignment = NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Center;
                    break;

                case ContentAlignment.TopRight:
                    Alignment = NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Right;
                    break;

                case ContentAlignment.MiddleLeft:
                    Alignment = NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Left;
                    break;

                case ContentAlignment.MiddleCenter:
                    Alignment = NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Center;
                    break;

                case ContentAlignment.MiddleRight:
                    Alignment = NativeTextRenderer.TextAlignFlags.Middle | NativeTextRenderer.TextAlignFlags.Right;
                    break;

                case ContentAlignment.BottomLeft:
                    Alignment = NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Left;
                    break;

                case ContentAlignment.BottomCenter:
                    Alignment = NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Center;
                    break;

                case ContentAlignment.BottomRight:
                    Alignment = NativeTextRenderer.TextAlignFlags.Bottom | NativeTextRenderer.TextAlignFlags.Right;
                    break;

                default:
                    Alignment = NativeTextRenderer.TextAlignFlags.Top | NativeTextRenderer.TextAlignFlags.Left;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent.BackColor);

            // Draw Text
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                if (Multiline)
                NativeText.DrawMultilineTransparentText(
                    Text,
                    Font,
                    UseCustomColor ? ForeColor :
                    Enabled ? HighEmphasis ? UseAccent ?
                    SkinManager.ColorScheme.AccentColor : // High emphasis, accent
                    (SkinManager.Theme == MaterialSkin.MaterialSkinManager.Themes.LIGHT) ?
                    SkinManager.ColorScheme.PrimaryColor : // High emphasis, primary Light theme
                    SkinManager.ColorScheme.PrimaryColor.Lighten(0.25f) : // High emphasis, primary Dark theme
                    SkinManager.TextHighEmphasisColor : // Normal
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    ClientRectangle.Location,
                    ClientRectangle.Size,
                    Alignment);
                else
                NativeText.DrawTransparentText(
                    Text,
                    Font,
                    UseCustomColor ? ForeColor :
                    Enabled ? HighEmphasis ? UseAccent ?
                    SkinManager.ColorScheme.AccentColor : // High emphasis, accent
                    (SkinManager.Theme == MaterialSkin.MaterialSkinManager.Themes.LIGHT) ?
                    SkinManager.ColorScheme.PrimaryColor : // High emphasis, primary Light theme
                    SkinManager.ColorScheme.PrimaryColor.Lighten(0.25f) : // High emphasis, primary Dark theme
                    SkinManager.TextHighEmphasisColor : // Normal
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    ClientRectangle.Location,
                    ClientRectangle.Size,
                    Alignment);
            }
        }

    }
}
