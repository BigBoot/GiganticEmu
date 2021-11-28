using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GiganticEmu.Launcher;

public enum MessageBoxType
{
    MessageBox = 0,
    InputBox = 1,
}

public partial class Prompt : Window
{
    public string? Text;
    public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;
    public MessageBoxImage Image { get; set; } = MessageBoxImage.None;
    public MessageBoxType Type { get; set; } = MessageBoxType.MessageBox;
    public string? DefaultResponse { get; set; }
    public MessageBoxResult? Result { get; set; }

    public static string? OKText { get; set; }
    public static string? CancelText { get; set; }
    public static string? YesText { get; set; }
    public static string? NoText { get; set; }

    private Dictionary<string, List<string>> lang = new Dictionary<string, List<string>>()
        {
            {"en-US", new List<string> {"OK","Cancel","Yes","No" } }
        };

    public Prompt()
    {
        InitializeComponent();
    }

    private void Prompt_Loaded(object sender, RoutedEventArgs e)
    {
        ButtonOK.Visibility = Visibility.Collapsed;
        ButtonYes.Visibility = Visibility.Collapsed;
        ButtonNo.Visibility = Visibility.Collapsed;
        ButtonCancel.Visibility = Visibility.Collapsed;

        TextBlockInput.Visibility = Visibility.Collapsed;

        base.Title = string.IsNullOrEmpty(Title) ? "" : Title;
        TextBlockText.Text = Text ?? "";

        LocalizeButtons();

        switch (Button)
        {
            case MessageBoxButton.OK:
                ButtonOK.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.OKCancel:
                ButtonOK.Visibility = Visibility.Visible;
                ButtonCancel.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNo:
                ButtonYes.Visibility = Visibility.Visible;
                ButtonNo.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNoCancel:
                ButtonYes.Visibility = Visibility.Visible;
                ButtonNo.Visibility = Visibility.Visible;
                ButtonCancel.Visibility = Visibility.Visible;
                break;
        }

        switch (Image)
        {
            case MessageBoxImage.Information:
                ImageIcon.Source = new BitmapImage(new Uri("img/information.png", UriKind.Relative));
                break;
            case MessageBoxImage.Error:
                ImageIcon.Source = new BitmapImage(new Uri("img/critical.png", UriKind.Relative));
                break;
            case MessageBoxImage.Exclamation:
                ImageIcon.Source = new BitmapImage(new Uri("img/exclamation.png", UriKind.Relative));
                break;
            case MessageBoxImage.Question:
                ImageIcon.Source = new BitmapImage(new Uri("img/question.png", UriKind.Relative));
                break;
        }

        switch (Type)
        {
            case MessageBoxType.MessageBox:

                break;
            case MessageBoxType.InputBox:
                TextBlockInput.Visibility = Visibility.Visible;
                TextBlockInput.Text = DefaultResponse ?? "";
                TextBlockInput.SelectAll();
                TextBlockInput.Focus();
                break;
        }

    }

    private void LocalizeButtons()
    {
        if (lang.ContainsKey(CultureInfo.CurrentCulture.Name) && lang[CultureInfo.CurrentCulture.Name].Count == 4)
        {
            ButtonOK.Content = lang[CultureInfo.CurrentCulture.Name][0];
            ButtonCancel.Content = lang[CultureInfo.CurrentCulture.Name][1];
            ButtonYes.Content = lang[CultureInfo.CurrentCulture.Name][2];
            ButtonNo.Content = lang[CultureInfo.CurrentCulture.Name][3];
        }

        if (!string.IsNullOrEmpty(OKText)) { ButtonOK.Content = OKText; }
        if (!string.IsNullOrEmpty(CancelText)) { ButtonCancel.Content = CancelText; }
        if (!string.IsNullOrEmpty(YesText)) { ButtonYes.Content = YesText; }
        if (!string.IsNullOrEmpty(NoText)) { ButtonNo.Content = NoText; }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        DialogResult = true;
    }

    private void ButtonNo_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.No;
        DialogResult = true;
    }

    private void ButtonYes_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Yes;
        DialogResult = true;
    }

    private void ButtonOK_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.OK;
        DialogResult = true;
    }

    public static string? ShowInputBox(string text = "", string title = "", MessageBoxImage icon = MessageBoxImage.Information, string? defaultResponse = null, Window? owner = null)
    {
        Prompt w = new Prompt();
        if (!(owner == null)) { w.Owner = owner; w.Icon = owner.Icon; }
        w.Type = MessageBoxType.InputBox;
        w.Text = text;
        w.Title = title;
        w.Button = MessageBoxButton.OKCancel;
        w.Image = icon;
        w.DefaultResponse = defaultResponse;

        if (w.ShowDialog() == true && w.Result == MessageBoxResult.OK)
        {
            return w.TextBlockInput.Text;
        }

        return null;
    }
}