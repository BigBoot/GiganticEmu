using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Agent;

public class LogManager
{
    private static Regex RE_WINNER = new Regex(@"(?<=ScriptLog: \(RXPAWN_)(NAGA|GRIFFIN)(?=_CONTENT_0\) RXPAWN::DYING:DESTROYED)", RegexOptions.Compiled | RegexOptions.Multiline);
    private static Regex RE_IF_LOG_CLOSE = new Regex(@"^\[\d+: [0-9\.]+\] Log: Log file closed, \d{2}\/\d{2}\/\d{2} \d{2}:\d{2}:\d{2}", RegexOptions.Compiled | RegexOptions.Multiline);
    
    private readonly int _port;
    private readonly string _logPath;
    private readonly string _content;

    public LogManager(int port)
    {
        _port = port;
        _logPath = Path.GetFullPath(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Gigantic", "RxGame", "Logs", $"GiganticEmu.Agent.{_port}.log"));
        
        using (var fs = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var sr = new StreamReader(fs)) 
            {
                _content = sr.ReadToEnd();
            }
        }
    }
    
    public bool IsRecent()
    {
        if (RE_IF_LOG_CLOSE.Matches(_content).Count != 0)
        {
            var logLength = _content.Length;
            var time = _content.Substring(logLength-19, 17);
            var dateTime = DateTime.Parse(time, CultureInfo.CurrentCulture);
            return DateTime.Now.Subtract(dateTime).TotalSeconds <= 300;
        }
        else
        {
            return true;
        }
    }

    public string? GetWinner()
    {
        var matches = RE_WINNER.Matches(_content);
        if (matches.Count != 0) {
            return matches[0].Value == "NAGA" ? "GRIFFIN" : "NAGA";
        }
        else {
            return null;
        }
    }
}
