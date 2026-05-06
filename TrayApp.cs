using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpotifyVolumeControl
{
    public class TrayApp : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly KeyboardHook _keyboardHook;
        private readonly AppSettings _settings;
        private readonly SpotifyVolume _spotifyVolume;
        private bool _spotifyMode = false;

        public TrayApp()
        {
            _settings = AppSettings.Load();
            _spotifyVolume = new SpotifyVolume(_settings);
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyPressed += OnKeyPressed;

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = ModeLabel(),
                Visible = true,
                ContextMenuStrip = BuildContextMenu()
            };

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Toggle Spotify Mode", null, (s, e) => ToggleMode());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(RebindLabel(), null, OnRebindClicked);

            var stepMenu = new ToolStripMenuItem("Volume Step");
            foreach (var (label, value) in new (string, float)[] { ("1%", 0.01f), ("2%", 0.02f), ("5%", 0.05f), ("10%", 0.10f) })
            {
                float captured = value;
                var item = new ToolStripMenuItem(label, null, (s, e) => SetVolumeStep(captured))
                {
                    Checked = Math.Abs(_settings.VolumeStep - value) < 0.001f
                };
                stepMenu.DropDownItems.Add(item);
            }
            menu.Items.Add(stepMenu);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => Application.Exit());

            menu.Opening += (s, e) => RefreshMenu(menu);

            return menu;
        }

        private void RefreshMenu(ContextMenuStrip menu)
        {
            menu.Items[2].Text = RebindLabel();

            var stepMenu = (ToolStripMenuItem)menu.Items[3];
            foreach (ToolStripMenuItem item in stepMenu.DropDownItems)
            {
                float itemValue = item.Text switch
                {
                    "1%" => 0.01f,
                    "2%" => 0.02f,
                    "5%" => 0.05f,
                    "10%" => 0.10f,
                    _ => -1f
                };
                item.Checked = Math.Abs(_settings.VolumeStep - itemValue) < 0.001f;
            }
        }

        private string RebindLabel() => $"Toggle Key: {_settings.ToggleKey}";
        private void OnRebindClicked(object? sender, EventArgs e)
        {
            using var form = new RebindForm(_keyboardHook);

            if (form.ShowDialog() == DialogResult.OK && form.CapturedKey != Keys.None)
            {
                _settings.ToggleKey = form.CapturedKey;
                _settings.Save();
                _trayIcon.ShowBalloonTip(500, "Key Rebound", $"Toggle key set to: {form.CapturedKey}", ToolTipIcon.Info);
            }
        }

        private void SetVolumeStep(float step)
        {
            _settings.VolumeStep = step;
            _settings.Save();
        }


        private void OnKeyPressed(Keys key, ref bool handled)
        {
            if (key == _settings.ToggleKey)
            {
                ToggleMode();
                handled = true;
            }
            else if (_spotifyMode && key == Keys.VolumeUp)
            {
                _spotifyVolume.AdjustVolume(true);
                handled = true;
            }
            else if (_spotifyMode && key == Keys.VolumeDown)
            {
                _spotifyVolume.AdjustVolume(false);
                handled = true;
            }
        }

        private void ToggleMode()
        {
            _spotifyMode = !_spotifyMode;
            _trayIcon.Text = ModeLabel();

            string body = _spotifyMode && !SpotifyVolume.IsSpotifyRunning()
                ? "Spotify mode active, but no Spotify session found. Start playing something first."
                : $"Now controlling: {(_spotifyMode ? "Spotify" : "System")} volume";

            _trayIcon.ShowBalloonTip(500, "Volume Mode", body, ToolTipIcon.Info);
        }

        private string ModeLabel() => _spotifyMode
            ? "Volume: Spotify 🎵"
            : "Volume: System 🔊";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keyboardHook.Dispose();
                _trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}