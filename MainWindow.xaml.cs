using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScriptVsNewWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> log = new();

        private bool enableLog = false;

        public MainWindow()
        {
            InitializeComponent();

            _ = InitializeAsync();
        }

        public ICollection<string> Log => this.log;

        private async Task InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async();

            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            WebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            WebView.NavigateToString(HTML.OpenWindow);
        }

        private void LogEvent(string @event)
        {
            if (this.enableLog)
            {
                log.Add($"[{DateTime.Now.ToString("hh:mm:ss.ffff")}] {@event}");
            }
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (sender is CoreWebView2 webView)
            {
                LogEvent($"NavigationStarting - '{webView.Source}'");
            }
        }

        private void CoreWebView2_ContentLoading(object? sender, CoreWebView2ContentLoadingEventArgs e)
        {
            if (sender is CoreWebView2 webView)
            {
                LogEvent($"ContentLoading - '{webView.Source}'");
            }
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.enableLog = true;
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

            newWebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            newWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            newWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            newWebView.CoreWebView2.ContentLoading += CoreWebView2_ContentLoading;

            if (SetScripts.SelectedIndex == 1)
            {
                LogEvent($"Start Loading Scripts");
                await newWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("alert('NewWindowRequested')");
                LogEvent($"Completed Loading Scripts");
            }

            if (SetNewWindow.IsChecked == true)
            {
                LogEvent($"Assigning NewWindow");
                e.NewWindow = newWebView.CoreWebView2;
                LogEvent($"Assigned NewWindow");
            }

            if (Delay.SelectedIndex > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000 * Delay.SelectedIndex));
            }

            if (SetScripts.SelectedIndex == 0)
            {
                LogEvent($"Start Loading Scripts");
                await newWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("console.log('NewWindowRequested')");
                LogEvent($"Completed Loading Scripts");
            }

            if (SetSource.IsChecked == true)
            {
                LogEvent($"Setting Source - '{e.Uri}'");
                newWebView.Source = new Uri(e.Uri);
                LogEvent($"Set Source - '{e.Uri}'");
            }

            e.Handled = true;
            deferral.Complete();
        }

        private void CopyLog_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();

            foreach (var @event in this.log)
            {
                builder.AppendLine(@event);
            }

            Clipboard.SetText(builder.ToString());
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            this.log.Clear();
        }
    }
}
