using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Areas.Admin.Models;
using MultiShop.WebUI.Areas.Admin.Services;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SystemFlowController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public SystemFlowController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            var logsDir = ResolveLogsDirectory();
            var vm = new SystemFlowStatusViewModel
            {
                CheckedAtLocal = DateTime.Now,
                LogsDirectory = logsDir,
                Ports = BuildPortProbes(),
                Logs = BuildLogProbes(logsDir),
            };
            return View(vm);
        }

        private static List<PortProbeResult> BuildPortProbes()
        {
            var defs = new (string Label, int Port)[]
            {
                ("Gateway", 5000),
                ("Identity", 5001),
                ("WebUI", 5178),
                ("Catalog", 7070),
                ("Discount", 7071),
                ("Order", 7072),
                ("Cargo", 7073),
                ("Basket", 7074),
                ("Comment", 7075),
                ("Payment", 7076),
                ("Images", 7077),
                ("Message", 7078),
                ("Favorite", 7079),
                ("Redis", 6379),
                ("MongoDB", 27017),
                ("PostgreSQL", 5432),
            };

            var list = new List<PortProbeResult>(defs.Length);
            foreach (var (label, port) in defs)
            {
                list.Add(new PortProbeResult
                {
                    Label = label,
                    Port = port,
                    Reachable = LocalLiveProbe.TryTcpLocalhost(port),
                });
            }

            return list;
        }

        private string? ResolveLogsDirectory()
        {
            var candidates = new[]
            {
                Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "..", "logs")),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "logs")),
            };

            foreach (var dir in candidates)
            {
                if (Directory.Exists(dir))
                {
                    return dir;
                }
            }

            return null;
        }

        private static List<LogProbeResult> BuildLogProbes(string? logsDir)
        {
            var names = new[] { "WebUI", "Gateway", "Basket", "Identity", "Catalog", "Order" };
            var list = new List<LogProbeResult>();
            if (string.IsNullOrEmpty(logsDir))
            {
                foreach (var n in names)
                {
                    list.Add(new LogProbeResult { Name = n, FileName = $"{n}.out.log", Exists = false });
                }

                return list;
            }

            foreach (var n in names)
            {
                var fileName = $"{n}.out.log";
                var path = Path.Combine(logsDir, fileName);
                if (!System.IO.File.Exists(path))
                {
                    list.Add(new LogProbeResult { Name = n, FileName = fileName, Exists = false });
                    continue;
                }

                var info = new FileInfo(path);
                list.Add(new LogProbeResult
                {
                    Name = n,
                    FileName = fileName,
                    Exists = true,
                    LastWriteLocal = info.LastWriteTime,
                });
            }

            return list;
        }
    }
}
