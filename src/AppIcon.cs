namespace PilTakip;

static class AppIcon
{
    public static readonly Icon Value = Load();

    static Icon Load()
    {
        try
        {
            var path = Environment.ProcessPath;
            if (path != null)
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null) return icon;
            }
        }
        catch { }
        return SystemIcons.Application;
    }
}
