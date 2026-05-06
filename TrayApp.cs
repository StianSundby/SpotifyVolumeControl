using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpotifyVolumeControl
{
    public class TrayApp : Form
    {
        private readonly NotifyIcon _trayIcon;
        private readonly KeyboardHook _keyboardHook;
        private bool _spotifyMode = false;

        public TrayApp()
        {
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

            var toggleItem = new ToolStripMenuItem("Toggle Spotify Mode", null, (s, e) => ToggleMode());
            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => Application.Exit());

            return menu;
        }

        private void OnKeyPressed(Keys key, ref bool handled)
        {
            switch (key)
            {
                case Keys.F24:
                    ToggleMode();
                    handled = true;
                    break;

                case Keys.VolumeUp when _spotifyMode:
                    SpotifyVolume.AdjustVolume(volumeUp: true);
                    handled = true; //block the key so system volume stays untouched.
                    break;

                case Keys.VolumeDown when _spotifyMode:
                    SpotifyVolume.AdjustVolume(volumeUp: false);
                    handled = true;
                    break;
            }
        }

        private void ToggleMode()
        {
            _spotifyMode = !_spotifyMode;
            _trayIcon.Text = ModeLabel();

            string body = _spotifyMode && !SpotifyVolume.IsSpotifyRunning()
                ? "Spotify mode active, but no Spotify session found. Start playing something first."
                : $"Now controlling: {(_spotifyMode ? "Spotify" : "System")} volume";

            _trayIcon.ShowBalloonTip(1500, "Volume Mode", body, ToolTipIcon.Info);
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