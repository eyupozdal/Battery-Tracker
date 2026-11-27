namespace PilTakip;

static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, "PilTakip.SingleInstance", out bool created);
        if (!created) return;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayContext());
    }
}
