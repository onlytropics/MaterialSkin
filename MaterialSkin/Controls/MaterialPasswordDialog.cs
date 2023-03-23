namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    public class MaterialPasswordDialog : MaterialDialog  
    {

        private const int LEFT_RIGHT_PADDING = 24;
        private const int BUTTON_PADDING = 8;
        private const int BUTTON_HEIGHT = 36;
        private const int TEXT_TOP_PADDING = 17;
        private const int TEXT_BOTTOM_PADDING = 28;
        private int _header_Height = 40;

        protected MaterialMaskedTextBox _maskedTextBox;

        public String Password => _maskedTextBox.Text;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        /// <summary>
        /// Constructer Setting up the Layout
        /// </summary>
        public MaterialPasswordDialog(Form ParentForm, string Title, string Text, string ValidationButtonText, bool ShowCancelButton, string CancelButtonText, bool UseAccentColor)
            :base(ParentForm,Title,Text,ValidationButtonText,ShowCancelButton,CancelButtonText,UseAccentColor)
        {
            Shown += (s, e) =>
            {
                _maskedTextBox.Focus();
            };
        }

        protected override int AddDialogControls(Point Location, int maxWidth)
        {
            _maskedTextBox = new MaterialMaskedTextBox
            {
                AutoSize = false,
                Location = Location,
                Size = new Size(maxWidth,48),
                UseSystemPasswordChar = true,
                Hint = "Password",
                UseTallSize = true
            };

            Controls.Add(_maskedTextBox);
            return _maskedTextBox.Height;
        }
    }
}
