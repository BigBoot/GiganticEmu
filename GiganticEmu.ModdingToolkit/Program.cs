using System;
using System.IO;
using System.Linq;
using UELib;
using UELib.Core;

using GiganticEmu.ModdingToolkit;
using System.Collections.Generic;
using System.Reflection;
using GiganticEmu.Shared;
using System.Diagnostics;
using System.Text;
using Avalonia;


internal class Program
{


    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}