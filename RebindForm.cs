using System.Drawing;
using System.Windows.Forms;


namespace SpotifyVolumeControl
{
    public class RebindForm : Form
    {
        public Keys CapturedKey { get; private set; } = Keys.None;

        private readonly KeyboardHook _hook;
        private readonly Label _label;

        public RebindForm(KeyboardHook hook)
        {
            _hook = hook;

            Text = "Rebind Toggle Key";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(300, 100);
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;

            _label = new Label
            {
                Text = "Press any key to bind as the toggle...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f)
            };

            Controls.Add(_label);
        }

        protected override void OnShown(System.EventArgs e)
        {
            base.OnShown(e);
            _hook.KeyPressed += OnCapture;
        }

        private void OnCapture(Keys key, ref bool handled)
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey ||
                key == Keys.LShiftKey || key == Keys.RShiftKey ||
                key == Keys.LMenu || key == Keys.RMenu ||
                key == Keys.LWin || key == Keys.RWin)
                return;

            CapturedKey = key;
            handled = true;

            _hook.KeyPressed -= OnCapture;

            Invoke(() =>
            {
                _label.Text = $"Bound to: {key}";
                DialogResult = DialogResult.OK;
                Close();
            });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hook.KeyPressed -= OnCapture;
            base.OnFormClosed(e);
        }
    }

}
