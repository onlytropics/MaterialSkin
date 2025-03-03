﻿namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Data;
    using System.Windows.Forms;
    using System.Diagnostics;

    [System.ComponentModel.DesignerCategory("")]
    public class MaterialComboBox : ComboBox, IMaterialControl
    {
        #region DPI-Awareness
        private float dpiMultiplicator;
        protected int dpiAdjust(int value) => (int) Math.Round(value * dpiMultiplicator);
        protected float dpiAdjust(float value) => value * dpiMultiplicator;
        #endregion

        // For some reason, even when overriding the AutoSize property, it doesn't appear on the properties panel, so we have to create a new one.
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Layout")]
        private bool _AutoResize;

        public bool AutoResize
        {
            get { return _AutoResize; }
            set
            {
                _AutoResize = value;
                recalculateAutoSize();
            }
        }

        //Properties for managing the material design properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private bool _UseTallSize;

        [Category("Material Skin"), DefaultValue(true), Description("Using a larger size enables the hint to always be visible")]
        public bool UseTallSize
        {
            get { return _UseTallSize; }
            set
            {
                _UseTallSize = value;
                setHeightVars();
                Invalidate();
                if (editBox != null) editBox.UseTallSize = value;
            }
        }

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseAccent { get; set; }

        private string _hint = string.Empty;

        [Category("Material Skin"), DefaultValue(""), Localizable(true)]
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                hasHint = !String.IsNullOrEmpty(Hint);
                Invalidate();
                if (editBox != null) editBox.Hint = value;
            }
        }

        private int _startIndex;
        public int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
                try
                {
                    if (base.Items.Count > 0)
                    {
                        base.SelectedIndex = value;
                    }
                }
                catch
                {
                }
                Invalidate();
            }
        }

        private const int TEXT_SMALL_SIZE = 18;
        private const int TEXT_SMALL_Y = 4;
        private const int BOTTOM_PADDING = 4;

        #region constants adjusted for dpi
        private int TextSmallSize;
        private int TextSmallY;
        private int BottomPadding;
        #endregion

        private int HEIGHT = 50;
        private int LINE_Y;

        private bool hasHint;

        private readonly AnimationManager _animationManager;

        #region Editable DropDown
        private MaterialTextBox2 editBox;

        public new event EventHandler TextChanged;

        private string _text;
        public override string Text { 
            get
            {
                return _text;
            }
            set
            {
                if (editBox != null) editBox.Text = value;
                _text = value;
                TextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        public MaterialComboBox()
        {
            dpiMultiplicator = DeviceDpi/96f;
            TextSmallSize = dpiAdjust(TEXT_SMALL_SIZE);
            TextSmallY = dpiAdjust(TEXT_SMALL_Y);
            BottomPadding = dpiAdjust(BOTTOM_PADDING);
            HEIGHT = dpiAdjust(HEIGHT);

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Material Properties
            Hint = "";
            UseAccent = true;
            UseTallSize = true;
            MaxDropDownItems = 4;

            Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle2, DeviceDpi);
            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownWidth = Width;
            DropDownStyle = ComboBoxStyle.DropDownList;

            // Animations
            _animationManager = new AnimationManager(true)
            {
                Increment = 0.08,
                AnimationType = AnimationType.EaseInOut
            };
            _animationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationFinished += sender => _animationManager.SetProgress(0);
            DropDownClosed += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0 && !Focused) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };
            DropDown += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
            };
            GotFocus += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                Invalidate();
            };
            MouseLeave += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                Invalidate();
            };
            SelectedIndexChanged += (sender, args) =>
            {
                Invalidate();
            };
            KeyUp += (sender, args) =>
            { 
                if (Enabled && DropDownStyle == ComboBoxStyle.DropDownList && (args.KeyCode == Keys.Delete || args.KeyCode == Keys.Back))
                {
                    SelectedIndex = -1;
                    Invalidate();
                }
            };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;

            g.Clear(Parent.BackColor);
            g.FillRectangle(Enabled ? Focused ?
                SkinManager.BackgroundFocusBrush : // Focused
                MouseState == MouseState.HOVER ?
                SkinManager.BackgroundHoverBrush : // Hover
                SkinManager.BackgroundAlternativeBrush : // normal
                SkinManager.BackgroundDisabledBrush // Disabled
                , ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, LINE_Y);

            //Set color and brush
            Color SelectedColor = new Color();
            if (UseAccent)
                SelectedColor = SkinManager.ColorScheme.AccentColor;
            else
                SelectedColor = SkinManager.ColorScheme.PrimaryColor;
            SolidBrush SelectedBrush = new SolidBrush(SelectedColor);

            // Create and Draw the arrow
            System.Drawing.Drawing2D.GraphicsPath pth = new System.Drawing.Drawing2D.GraphicsPath();
            float FormPadding = dpiAdjust((float)SkinManager.FORM_PADDING);
            PointF TopRight = new PointF(this.Width - dpiAdjust(0.5f) - FormPadding, (this.Height >> 1) - dpiAdjust(2.5f));
            PointF MidBottom = new PointF(this.Width - dpiAdjust(4.5f) - FormPadding, (this.Height >> 1) + dpiAdjust(2.5f));
            PointF TopLeft = new PointF(this.Width - dpiAdjust(8.5f) - FormPadding, (this.Height >> 1) - dpiAdjust(2.5f));
            pth.AddLine(TopLeft, TopRight);
            pth.AddLine(TopRight, MidBottom);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.FillPath((SolidBrush)(Enabled ? DroppedDown || Focused ?
                SelectedBrush : //DroppedDown or Focused
                SkinManager.TextHighEmphasisBrush : //Not DroppedDown and not Focused
                new SolidBrush(DrawHelper.BlendColor(SkinManager.TextHighEmphasisColor, SkinManager.SwitchOffDisabledThumbColor, 197))  //Disabled
                ), pth);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            // HintText
            bool userTextPresent = SelectedIndex >= 0;
            Rectangle hintRect = new Rectangle((int)FormPadding, ClientRectangle.Y, Width, LINE_Y);
            int hintTextSize = 16;

            // bottom line base
            g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);

            if (!_animationManager.IsAnimating())
            {
                // No animation
                if (hasHint && UseTallSize && (DroppedDown || Focused || SelectedIndex >= 0))
                {
                    // hint text
                    hintRect = new Rectangle((int)FormPadding, TextSmallY, Width, TextSmallSize);
                    hintTextSize = 12;
                }

                // bottom line
                if (DroppedDown || Focused)
                {
                    g.FillRectangle(SelectedBrush, 0, LINE_Y, Width, 2);
                }
            }
            else
            {
                // Animate - Focus got/lost
                double animationProgress = _animationManager.GetProgress();

                // hint Animation
                if (hasHint && UseTallSize)
                {
                    hintRect = new Rectangle(
                        (int)FormPadding,
                        userTextPresent && !_animationManager.IsAnimating() ? (TextSmallY) : ClientRectangle.Y + (int)((TextSmallY - ClientRectangle.Y) * animationProgress),
                        Width,
                        userTextPresent && !_animationManager.IsAnimating() ? (TextSmallSize) : (int)(LINE_Y + (TextSmallSize - LINE_Y) * animationProgress));
                    hintTextSize = userTextPresent && !_animationManager.IsAnimating() ? 12 : (int)(16 + (12 - 16) * animationProgress);
                }

                // Line Animation
                int LineAnimationWidth = (int)(Width * animationProgress);
                int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                g.FillRectangle(SelectedBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
            }

            // Calc text Rect
            Rectangle textRect = new Rectangle(
                (int)FormPadding,
                hasHint && UseTallSize ? (hintRect.Y + hintRect.Height) - dpiAdjust(2) : ClientRectangle.Y,
                ClientRectangle.Width - (int)FormPadding * 3 - dpiAdjust(8),
                hasHint && UseTallSize ? LINE_Y - (hintRect.Y + hintRect.Height) : LINE_Y);

            g.Clip = new Region(textRect);

            if (_text != null)
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                // Draw user text
                NativeText.DrawTransparentText(
                    _text,
                    SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1, DeviceDpi),
                    Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                    textRect.Location,
                    textRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }

            g.ResetClip();

            // Draw hint text
            if (hasHint && (UseTallSize || String.IsNullOrEmpty(Text)))
            {
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                    Hint,
                    SkinManager.getTextBoxFontBySize(hintTextSize, DeviceDpi),
                    Enabled ? DroppedDown || Focused ? 
                    SelectedColor : // Focus 
                    SkinManager.TextMediumEmphasisColor : // not focused
                    SkinManager.TextDisabledOrHintColor, // Disabled
                    hintRect.Location,
                    hintRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
        }

        private void CustomMeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            e.ItemHeight = HEIGHT - dpiAdjust(7);
        }

        private void CustomDrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            //if (e.Index < 0 || e.Index > Items.Count || !Focused) return;

            Graphics g = e.Graphics;

            // Draw the background of the item.
            g.FillRectangle(SkinManager.BackgroundBrush, e.Bounds);

            // Hover
            if (e.State.HasFlag(DrawItemState.Focus)) // Focus == hover
            {
                g.FillRectangle(SkinManager.BackgroundHoverBrush, e.Bounds);
            }
            
            string Text = "";
            if (!string.IsNullOrWhiteSpace(DisplayMember))
            {
                if (!Items[e.Index].GetType().Equals(typeof(DataRowView)))
                {
                    var item = Items[e.Index].GetType().GetProperty(DisplayMember)?.GetValue(Items[e.Index]);
                    Text = item?.ToString();
                }
                else
                {
                    var table = ((DataRow)Items[e.Index].GetType().GetProperty("Row").GetValue(Items[e.Index])).Table;
                    Text = table.Rows[e.Index][DisplayMember].ToString();
                }
            }
            else
            {
                if (e.Index >= 0 && e.Index < Items.Count)
                    Text = Items[e.Index].ToString();
            }

            float FormPadding = dpiAdjust((float)SkinManager.FORM_PADDING);
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(
                Text,
                SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle1, DeviceDpi),
                SkinManager.TextHighEmphasisNoAlphaColor,
                new Point(e.Bounds.Location.X + (int)FormPadding, e.Bounds.Location.Y),
                new Size(e.Bounds.Size.Width - (int)FormPadding * 2, e.Bounds.Size.Height),
                NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle); ;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MouseState = MouseState.OUT;
            MeasureItem += CustomMeasureItem;
            DrawItem += CustomDrawItem;
            DrawMode = DrawMode.OwnerDrawVariable;
            recalculateAutoSize();
            setHeightVars();

            if (DropDownStyle == ComboBoxStyle.DropDown)
            {
                editBox = new MaterialTextBox2();
                editBox.Visible = true;
                editBox.Location = new Point(0, 0);
                float FormPadding = dpiAdjust((float)SkinManager.FORM_PADDING);
                editBox.Size = new Size((int)(Width - dpiAdjust(8.5f) - 2*FormPadding), Height);
                editBox.Hint = Hint;
                editBox.BackColor = BackColor;
                editBox.Text = _text;
                this.Controls.Add(editBox);

                this.SelectedValueChanged += (sender, args) => {
                    if (base.SelectedItem != null)
                        _text = editBox.Text = base.SelectedItem.ToString();
                };

                editBox.TextChanged += (sender, args) => {
                    _text = editBox.Text;
                    TextChanged?.Invoke(this, EventArgs.Empty);
                };

                SizeChanged += (sender, args) => {
                    float locFormPadding = dpiAdjust((float)SkinManager.FORM_PADDING);
                    editBox.Size = new Size((int)(Width - dpiAdjust(8.5f) - 2 * locFormPadding), Height);
                };
            }
            if (!DesignMode)
                DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            recalculateAutoSize();
            setHeightVars();
            if (editBox != null)
            {
                editBox.Size = new Size(Size.Width - 30, Size.Height);
            }
        }

        private void setHeightVars()
        {
            HEIGHT = dpiAdjust(UseTallSize ? 50 : 36);
            Size = new Size(Size.Width, HEIGHT);
            LINE_Y = HEIGHT - BottomPadding;
            ItemHeight = HEIGHT - dpiAdjust(7);
            DropDownHeight = ItemHeight * MaxDropDownItems + dpiAdjust(2);
        }

        public void recalculateAutoSize()
        {
            if (!AutoResize) return;

            int w = DropDownWidth;
            int padding = dpiAdjust(SkinManager.FORM_PADDING * 3);
            int vertScrollBarWidth = (Items.Count > MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            Graphics g = CreateGraphics();
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var itemsList = this.Items.Cast<object>().Select(item => item.ToString());
                foreach (string s in itemsList)
                {
                    int newWidth = NativeText.MeasureLogString(s, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1, DeviceDpi)).Width + vertScrollBarWidth + padding;
                    if (w < newWidth) w = newWidth;
                }
            }

            if (Width != w)
            {
                DropDownWidth = w;
                Width = w;
            }
        }
    }
}
