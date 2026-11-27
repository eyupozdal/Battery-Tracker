using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace PilTakip;

public sealed record BatteryDevice(string Id, string Name, int Percent, bool Connected);

public static class BatteryReader
{
    const string BatteryKey = "{104EA319-6EE2-4701-BD47-8DDBF425BBE5} 2";
    const string ContainerKey = "System.Devices.ContainerId";
    const string AepContainerKey = "System.Devices.Aep.ContainerId";
    const string AepConnectedKey = "System.Devices.Aep.IsConnected";

    public static async Task<List<BatteryDevice>> ReadAsync()
    {
        var paired = new Dictionary<Guid, (string Name, bool Connected)>();
        string[] selectors =
        {
            BluetoothDevice.GetDeviceSelectorFromPairingState(true),
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(true),
        };

        foreach (var selector in selectors)
        {
            try
            {
                var found = await DeviceInformation.FindAllAsync(selector, new[] { AepContainerKey, AepConnectedKey });
                foreach (var d in found)
                {
                    if (!d.Properties.TryGetValue(AepContainerKey, out var cObj) || cObj is not Guid cid) continue;
                    bool connected = d.Properties.TryGetValue(AepConnectedKey, out var connObj) && connObj is bool b && b;
                    if (!paired.TryGetValue(cid, out var old) || (connected && !old.Connected))
                        paired[cid] = (d.Name, connected);
                }
            }
            catch { }
        }

        var result = new Dictionary<Guid, BatteryDevice>();
        var nodes = await DeviceInformation.FindAllAsync(
            "", new[] { BatteryKey, ContainerKey }, DeviceInformationKind.Device);

        foreach (var n in nodes)
        {
            if (!n.Properties.TryGetValue(BatteryKey, out var batObj) || batObj is null) continue;
            if (!n.Properties.TryGetValue(ContainerKey, out var idObj) || idObj is not Guid containerId) continue;
            if (!paired.TryGetValue(containerId, out var info)) continue;
            if (result.ContainsKey(containerId)) continue;

            int pct;
            try { pct = Convert.ToInt32(batObj); } catch { continue; }
            if (pct < 0 || pct > 100) continue;

            string name = string.IsNullOrWhiteSpace(info.Name) ? n.Name : info.Name;
            result[containerId] = new BatteryDevice(containerId.ToString(), name, pct, info.Connected);
        }

        return result.Values.OrderBy(d => d.Name).ToList();
    }
}
