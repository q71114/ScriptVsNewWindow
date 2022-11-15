using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptVsNewWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> log = new();

        public MainWindow()
        {
            InitializeComponent();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async();

            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            WebView.NavigateToString(HTML.OpenWindow);
        }

        private async void CoreWebView2_NewWindowRequested(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            var _deferral = e.GetDeferral();

            if (ScheduleNewWindow.IsChecked == true)
            {
                _ = this.Dispatcher.InvokeAsync(() => OpenNewWindowAsync(e, _deferral));
            }
            else
            {
                await OpenNewWindowAsync(e, _deferral);
            }
        }

        private async Task OpenNewWindowAsync(CoreWebView2NewWindowRequestedEventArgs e, CoreWebView2Deferral deferral)
        {
            Window window = new Window
            {
                Width = this.Width,
                Height = this.Height,
                Left = this.Left + 100,
                Top = this.Top + 100
            };
            var newWebView = new WebView2();

            window.Content = newWebView;
            window.Show();

            await newWebView.EnsureCoreWebView2Async();

            if (SetScripts.SelectedIndex == 1)
            {
                await newWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("alert('NewWindowRequested')");
            }

            if (SetNewWindow.IsChecked == true)
            {
                e.NewWindow = newWebView.CoreWebView2;
            }

            if (Delay.SelectedIndex > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000 * Delay.SelectedIndex));
            }

            if (SetScripts.SelectedIndex == 0)
            {
                await newWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("alert('NewWindowRequested')");
            }

            if (SetSource.IsChecked == true)
            {
                newWebView.Source = new Uri(e.Uri);
            }

            e.Handled = true;
            deferral.Complete();
        }
    }
}
