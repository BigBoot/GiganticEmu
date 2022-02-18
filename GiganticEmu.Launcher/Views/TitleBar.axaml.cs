using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Threading.Tasks;


namespace GiganticEmu.Launcher;

public partial class TitleBar : UserControl
{
    private Button closeButton;

    private DockPanel titleBar;
    private DockPanel titleBarBackground;
    private NativeMenuBar seamlessMenuBar;
    private NativeMenuBar defaultMenuBar;

    public static readonly StyledProperty<bool> IsSeamlessProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsSeamless));

    public bool IsSeamless
    {
        get { return GetValue(IsSeamlessProperty); }
        set
        {
            SetValue(IsSeamlessProperty, value);
            if (titleBarBackground != null &&
                seamlessMenuBar != null &&
                defaultMenuBar != null)
            {
                titleBarBackground.IsVisible = IsSeamless ? false : true;
                seamlessMenuBar.IsVisible = IsSeamless ? true : false;
                defaultMenuBar.IsVisible = IsSeamless ? false : true;

                if (IsSeamless == false)
                {
                    titleBar.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
                }
            }
        }
    }

    public TitleBar()
    {
        this.InitializeComponent();

        closeButton = this.FindControl<Button>("CloseButton");

        closeButton.Click += CloseWindow;

        titleBar = this.FindControl<DockPanel>("TitleBar");
        titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground");
        seamlessMenuBar = this.FindControl<NativeMenuBar>("SeamlessMenuBar");
        defaultMenuBar = this.FindControl<NativeMenuBar>("DefaultMenuBar");

        SubscribeToWindowState();
    }

    private void CloseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot!;
        hostWindow.Close();
    }

    private void MaximizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot!;

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot!;
        hostWindow.WindowState = WindowState.Minimized;
    }

    private async void SubscribeToWindowState()
    {
        Window hostWindow = (Window)this.VisualRoot!;

        while (hostWindow == null)
        {
            hostWindow = (Window)this.VisualRoot!;
            await Task.Delay(50);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}