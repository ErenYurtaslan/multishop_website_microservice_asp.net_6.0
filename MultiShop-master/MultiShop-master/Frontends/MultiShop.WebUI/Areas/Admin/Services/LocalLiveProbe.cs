using System.Net.Sockets;

namespace MultiShop.WebUI.Areas.Admin.Services
{
    internal static class LocalLiveProbe
    {
        public static bool TryTcpLocalhost(int port, int timeoutMs = 800)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync("127.0.0.1", port);
                if (!connectTask.Wait(timeoutMs))
                {
                    return false;
                }

                return client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
