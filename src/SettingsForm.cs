namespace PilTakip;

sealed class GlyphPanel : FlowLayoutPanel
{
    public GlyphPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        BluetoothGlyph.Draw(e.Graphics, ClientRectangle, 55);
    }
}

sealed class SettingsForm : Form
{
    static readonly Color Surface = Color.FromArgb(221, 227, 234);
    static readonly Color Ink = Color.FromArgb(43, 52, 64);
    static readonly Color Muted = Color.FromArgb(100, 112, 128);
    static readonly Color Button = Color.FromArgb(200, 209, 220);
    static readonly Color Field = Color.FromArgb(238, 241, 245);

    readonly AppSettings _s;
    readonly Action _changed;
    readonly Func<Task> _refresh;
    readonly GlyphPanel _list;
    bool _loading;

    public SettingsForm(AppSettings s, List<BatteryDevice> devices, Func<Task> refresh, Action changed)
    {
        _s = s;
        _refresh = refresh;
        _changed = changed;

        Text = "Pil Takip";
        Icon = AppIcon.Value;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Surface;
        ForeColor = Ink;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(380, 420);

        var header = new Label
        {
            Text = "Görev çubuğunda yüzdesi görünecek cihazları seç:",
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 14, 12, 0),
        };

        _list = new GlyphPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Surface,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 4, 12, 4),
        };

        var startup = new CheckBox { Text = "Windows ile birlikte başlat", AutoSize = true, Checked = Startup.IsEnabled };
        startup.CheckedChanged += (_, _) => Startup.Set(startup.Checked);

        var threshold = new NumericUpDown
        {
            Minimum = 5,
            Maximum = 50,
            Value = Math.Clamp(_s.LowThreshold, 5, 50),
            Width = 55,
            BackColor = Field,
            BorderStyle = BorderStyle.FixedSingle,
        };
        threshold.ValueChanged += (_, _) =>
        {
            _s.LowThreshold = (int)threshold.Value;
            _s.Save();
            _changed();
        };

        var refreshBtn = new Button
        {
            Text = "Yenile",
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = Button,
            Margin = new Padding(12, 0, 0, 0),
        };
        refreshBtn.FlatAppearance.BorderSize = 0;
        refreshBtn.Click += async (_, _) => await _refresh();

        var row1 = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
        row1.Controls.Add(startup);

        var row2 = new FlowLayoutPanel { AutoSize = true };
        row2.Controls.Add(new Label { Text = "Düşük pil uyarısı (%):", AutoSize = true, Margin = new Padding(3, 6, 4, 0) });
        row2.Controls.Add(threshold);
        row2.Controls.Add(refreshBtn);

        var hint = new Label
        {
            Text = "Listede olmayan cihaz, Windows'a pil bilgisi vermiyor demektir. Yalnızca Bluetooth ile eşleşmiş cihazlar gösterilir.",
            AutoSize = true,
            MaximumSize = new Size(350, 0),
            ForeColor = Muted,
            Margin = new Padding(3, 10, 0, 0),
        };

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(10, 6, 10, 12),
        };
        bottom.Controls.Add(row1, 0, 0);
        bottom.Controls.Add(row2, 0, 1);
        bottom.Controls.Add(hint, 0, 2);

        Controls.Add(_list);
        Controls.Add(bottom);
        Controls.Add(header);

        Reload(devices);
    }

    public void Reload(IEnumerable<BatteryDevice> devices)
    {
        _loading = true;
        _list.SuspendLayout();
        foreach (Control c in _list.Controls) c.Dispose();
        _list.Controls.Clear();

        var seen = new HashSet<string>();
        foreach (var d in devices)
        {
            seen.Add(d.Id);
            string text = d.Connected ? $"{d.Name}  —  %{d.Percent}" : $"{d.Name}  —  bağlı değil";
            AddRow(d.Id, text, _s.Selected.Contains(d.Id));
        }

        foreach (var id in _s.Selected.Where(i => !seen.Contains(i)))
        {
            string name = _s.KnownNames.TryGetValue(id, out var n) ? n : "Bilinmeyen cihaz";
            AddRow(id, $"{name}  —  çevrimdışı", true);
        }

        if (_list.Controls.Count == 0)
        {
            _list.Controls.Add(new Label
            {
                Text = "Pil bilgisi veren bir Bluetooth cihazı bulunamadı.",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Muted,
                Margin = new Padding(4, 8, 4, 0),
            });
        }

        _list.ResumeLayout();
        _loading = false;
    }

    void AddRow(string id, string text, bool selected)
    {
        var box = new CheckBox
        {
            Text = text,
            Tag = id,
            Checked = selected,
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 10.5f),
            Margin = new Padding(4, 6, 4, 6),
        };
        box.CheckedChanged += (_, _) =>
        {
            if (_loading) return;
            if (box.Checked) _s.Selected.Add(id);
            else _s.Selected.Remove(id);
            _s.Save();
            _changed();
        };
        _list.Controls.Add(box);
    }
}
