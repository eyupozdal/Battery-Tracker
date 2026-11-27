namespace PilTakip;

sealed class TrayContext : ApplicationContext
{
    readonly AppSettings _s = AppSettings.Load();
    readonly System.Windows.Forms.Timer _timer = new();
    readonly Dictionary<string, NotifyIcon> _icons = new();
    readonly HashSet<string> _lowNotified = new();
    NotifyIcon? _idle;
    List<BatteryDevice> _latest = new();
    SettingsForm? _form;
    bool _busy;

    public TrayContext()
    {
        _timer.Interval = Math.Max(15, _s.IntervalSeconds) * 1000;
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();

        UpdateIcons();
        _ = RefreshAsync();
        if (_s.Selected.Count == 0) ShowSettings();
    }

    async Task RefreshAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            _latest = await Task.Run(() => BatteryReader.ReadAsync());
            foreach (var d in _latest) _s.KnownNames[d.Id] = d.Name;
            _s.Save();
            UpdateIcons();
            _form?.Reload(_latest);
        }
        catch { }
        finally { _busy = false; }
    }

    void UpdateIcons()
    {
        var active = _latest.Where(d => _s.Selected.Contains(d.Id) && d.Connected).ToList();

        foreach (var id in _icons.Keys.Except(active.Select(a => a.Id)).ToList())
        {
            DisposeIcon(_icons[id]);
            _icons.Remove(id);
        }

        foreach (var d in active)
        {
            if (!_icons.TryGetValue(d.Id, out var ni))
            {
                ni = NewNotifyIcon();
                _icons[d.Id] = ni;
            }
            var old = ni.Icon;
            ni.Icon = IconFactory.Create(d.Percent, ColorFor(d.Percent));
            old?.Dispose();
            ni.Text = Clip($"{d.Name}: %{d.Percent}");
            ni.Visible = true;
            CheckLow(d, ni);
        }

        if (active.Count == 0)
        {
            _idle ??= NewNotifyIcon();
            _idle.Icon = AppIcon.Value;
            _idle.Text = Clip("Pil Takip: gösterilecek cihaz yok");
            _idle.Visible = true;
        }
        else if (_idle != null)
        {
            DisposeIcon(_idle);
            _idle = null;
        }
    }

    Color ColorFor(int p)
    {
        if (p <= _s.LowThreshold) return Color.FromArgb(198, 40, 40);
        if (p <= _s.LowThreshold * 2) return Color.FromArgb(230, 126, 0);
        return Color.FromArgb(46, 125, 50);
    }

    void CheckLow(BatteryDevice d, NotifyIcon ni)
    {
        if (d.Percent <= _s.LowThreshold)
        {
            if (_lowNotified.Add(d.Id))
                ni.ShowBalloonTip(5000, "Pil azalıyor", $"{d.Name}: %{d.Percent}", ToolTipIcon.Warning);
        }
        else if (d.Percent > _s.LowThreshold + 5) _lowNotified.Remove(d.Id);
    }

    NotifyIcon NewNotifyIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Ayarlar…", null, (_, _) => ShowSettings());
        menu.Items.Add("Şimdi yenile", null, async (_, _) => await RefreshAsync());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Çıkış", null, (_, _) => Shutdown());

        var ni = new NotifyIcon { ContextMenuStrip = menu };
        ni.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowSettings(); };
        return ni;
    }

    void ShowSettings()
    {
        if (_form != null && !_form.IsDisposed) { _form.Activate(); return; }
        _form = new SettingsForm(_s, _latest, RefreshAsync, UpdateIcons);
        _form.FormClosed += (_, _) => _form = null;
        _form.Show();
    }

    void Shutdown()
    {
        _timer.Stop();
        foreach (var ni in _icons.Values) DisposeIcon(ni);
        _icons.Clear();
        if (_idle != null) DisposeIcon(_idle);
        _form?.Close();
        ExitThread();
    }

    static void DisposeIcon(NotifyIcon ni)
    {
        ni.Visible = false;
        if (!ReferenceEquals(ni.Icon, AppIcon.Value)) ni.Icon?.Dispose();
        ni.ContextMenuStrip?.Dispose();
        ni.Dispose();
    }

    static string Clip(string s) => s.Length <= 63 ? s : s[..63];
}
