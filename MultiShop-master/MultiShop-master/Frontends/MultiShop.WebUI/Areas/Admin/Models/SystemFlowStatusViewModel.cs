namespace MultiShop.WebUI.Areas.Admin.Models
{
    public class PortProbeResult
    {
        public string Label { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool Reachable { get; set; }
    }

    public class LogProbeResult
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public DateTime? LastWriteLocal { get; set; }
    }

    public class SystemFlowStatusViewModel
    {
        public DateTime CheckedAtLocal { get; set; }
        public string? LogsDirectory { get; set; }
        public List<PortProbeResult> Ports { get; set; } = new();
        public List<LogProbeResult> Logs { get; set; } = new();
    }
}
