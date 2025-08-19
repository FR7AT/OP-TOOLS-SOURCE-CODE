using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Data;
using Microsoft.Win32;
using System.Net.Http;
using System.Diagnostics;
using IOPath = System.IO.Path;
using System.Windows.Threading;
using System.Management;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Threading;

namespace OP_TOOLS {

    public partial class MainWindow : Window {

        private string currentVersion = "1.4.5";
        private string versionUrl = "https://raw.githubusercontent.com/FR7AT/OP-TOOLS/main/optools.txt";
        private string updateUrl = "https://raw.githubusercontent.com/FR7AT/OP-TOOLS/main/OP_TOOLS.exe";
        private string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OP_TOOLS.exe");

        public MainWindow(string title, string message, string type) {

            this.Width = 300;
            this.Height = 85;

            this.Top = SystemParameters.WorkArea.Top + 10;
            this.Left = SystemParameters.WorkArea.Right - this.Width - 10;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Topmost = true;

            SolidColorBrush backgroundColor = Brushes.Transparent;
            SolidColorBrush textColor = Brushes.Black;
            UIElement iconElement;

            UIElement CreateIcon(string symbol, Brush circleColor, Brush symbolColor) {

                Grid iconGrid = new Grid {

                    Width = 32,
                    Height = 32
                };

                Ellipse circle = new Ellipse {

                    Fill = circleColor,
                    Width = 32,
                    Height = 32
                };

                TextBlock symbolText = new TextBlock {

                    Text = symbol,
                    FontWeight = FontWeights.Bold,
                    FontSize = 20,
                    Foreground = symbolColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                iconGrid.Children.Add(circle);
                iconGrid.Children.Add(symbolText);

                return iconGrid;
            }

            switch (type.ToLower()) {

                case "success":
                    backgroundColor = new SolidColorBrush(Color.FromRgb(173, 240, 193));
                    textColor = Brushes.DarkGreen;
                    iconElement = CreateIcon("✔", Brushes.DarkGreen, Brushes.White);
                    break;
                case "error":
                    backgroundColor = new SolidColorBrush(Color.FromRgb(255, 204, 204));
                    textColor = Brushes.DarkRed;
                    iconElement = CreateIcon("✖", Brushes.DarkRed, Brushes.White);
                    break;
                case "warning":
                    backgroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 204));
                    textColor = Brushes.DarkOrange;
                    iconElement = CreateIcon("!", Brushes.DarkOrange, Brushes.White);
                    break;
                case "information":
                    backgroundColor = new SolidColorBrush(Color.FromRgb(204, 229, 255));
                    textColor = Brushes.DarkBlue;
                    iconElement = CreateIcon("i", Brushes.DarkBlue, Brushes.White);
                    break;
                default:
                    iconElement = CreateIcon("?", Brushes.Gray, Brushes.White);
                    break;
            }

            Border border = new Border {

                Background = backgroundColor,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(5)
            };

            Grid grid = new Grid();

            ColumnDefinition iconColumn = new ColumnDefinition { Width = new GridLength(50) };
            ColumnDefinition textColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            grid.ColumnDefinitions.Add(iconColumn);
            grid.ColumnDefinitions.Add(textColumn);
            Grid.SetColumn(iconElement, 0);
            grid.Children.Add(iconElement);
            StackPanel textPanel = new StackPanel { Margin = new Thickness(0, 10, 10, 10) };

            TextBlock titleBlock = new TextBlock {

                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = textColor
            };

            TextBlock messageBlock = new TextBlock {

                Text = message,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = textColor
            };

            textPanel.Children.Add(titleBlock);
            textPanel.Children.Add(messageBlock);
            Grid.SetColumn(textPanel, 1);
            grid.Children.Add(textPanel);

            Grid progressBarGrid = new Grid {

                Height = 5,
                Margin = new Thickness(0, 0, 10, 0),
                Background = backgroundColor,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            Rectangle progressRectangle = new Rectangle {

                Fill = textColor,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300
            };
            
            progressBarGrid.Children.Add(progressRectangle);

            DispatcherTimer progressTimer = new DispatcherTimer {

                Interval = TimeSpan.FromMilliseconds(30)
            };

            double progressWidth = 300;
            progressTimer.Tick += (s, e) => {
                
                progressWidth -= 3;

                if (progressWidth <= 0) {

                    progressTimer.Stop();
                    this.Close();
                }

                else {
                    progressRectangle.Width = progressWidth;
                }
            };

            progressTimer.Start();

            StackPanel mainPanel = new StackPanel {

                VerticalAlignment = VerticalAlignment.Stretch
            };

            mainPanel.Children.Add(grid);
            mainPanel.Children.Add(new Border { Height = 10 });
            mainPanel.Children.Add(progressBarGrid);
            border.Child = mainPanel;
            this.Content = border;
        }

        public MainWindow() {

            Task.Run(() => CheckForUpdates());
            InitializeComponent();
            Task.Run(() => LoadAndCustomizeForm());
            Task.Run(() => CheckFileAndShowButtons());
            this.Topmost = true;
            Dispatcher.BeginInvoke(new Action(() => {
                this.Topmost = false;
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            string userName = Environment.UserName;
            MainWindow window = new MainWindow("info", $"Welcome to {userName}", "information");
            window.Show();
            VersionTextBlock.Text = $"Version : {currentVersion}";
        }

        private void CheckForUpdates() {

            try {

                if (!IsInternetAvailable()) {

                    Application.Current.Dispatcher.Invoke(() => {

                        Window errorWindow = new Window {

                            Title = "Error",
                            Width = 350,
                            Height = 200,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            ResizeMode = ResizeMode.NoResize,
                            Topmost = true,
                            WindowStyle = WindowStyle.None,
                            AllowsTransparency = true,
                            Background = Brushes.Transparent,
                            ShowInTaskbar = false
                        };

                        errorWindow.Closing += (s, e) => {

                            e.Cancel = true;
                        };

                        StackPanel stackPanel = new StackPanel {

                            Orientation = Orientation.Vertical,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        TextBlock message = new TextBlock {

                            Text = "⚠️ \n No Internet Connection.",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(20),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            FontSize = 15,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Red,
                            TextAlignment = TextAlignment.Center
                        };

                        Button exitButton = new Button {

                            Content = "OK",
                            Width = 100,
                            Margin = new Thickness(10),
                            Background = Brushes.Red,
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            BorderThickness = new Thickness(0)
                        };

                        exitButton.Click += (s, e) => {

                            Application.Current.Shutdown();
                        };

                        Border border = new Border {

                            CornerRadius = new CornerRadius(15),
                            Background = Brushes.White,
                            BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                            BorderThickness = new Thickness(3),
                            Padding = new Thickness(10),
                            Child = stackPanel
                        };

                        stackPanel.Children.Add(message);
                        stackPanel.Children.Add(exitButton);
                        errorWindow.Content = stackPanel;
                        errorWindow.Content = border;

                        if (Application.Current.MainWindow != null) {

                            Application.Current.MainWindow.IsEnabled = false;
                        }

                        errorWindow.ShowDialog();
                    });

                    return;
                }

                using (WebClient client = new WebClient()) {

                    string latestVersion = client.DownloadString(versionUrl).Trim();

                    if (latestVersion != currentVersion) {

                        Application.Current.Dispatcher.Invoke(() => {

                            if (Application.Current.MainWindow != null) {

                                Application.Current.MainWindow.IsEnabled = false;
                            }

                            Window updateWindow = new Window {

                                Title = "Update",
                                Width = 350,
                                Height = 200,
                                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                                ResizeMode = ResizeMode.NoResize,
                                Topmost = true,
                                WindowStyle = WindowStyle.None,
                                AllowsTransparency = true,
                                Background = Brushes.Transparent
                            };

                            updateWindow.Closing += (s, e) => {

                                e.Cancel = true;
                            };

                            StackPanel stackPanel = new StackPanel {

                                Orientation = Orientation.Vertical,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            TextBlock message = new TextBlock {

                                Text = $"📢 \n New Version Is Ready To Download!",
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(10),
                                FontSize = 15,
                                FontWeight = FontWeights.Bold,
                                TextAlignment = TextAlignment.Center,
                                Foreground = new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            ProgressBar progressBar = new ProgressBar {

                                Width = 300,
                                Height = 10,
                                Margin = new Thickness(10),
                                Minimum = 0,
                                Maximum = 100,
                                Visibility = Visibility.Collapsed
                            };

                            Button yesButton = new Button {

                                Content = "Yes",
                                Width = 100,
                                Margin = new Thickness(10)
                            };

                            yesButton.Click += (s, e) => {

                                yesButton.IsEnabled = false;
                                progressBar.Visibility = Visibility.Visible;
                                message.Text = "Downloading...";
                                DownloadUpdate(progressBar, updateWindow);
                            };

                            Button noButton = new Button {

                                Content = "No",
                                Width = 100,
                                Margin = new Thickness(10)
                            };

                            noButton.Click += (s, e) => {

                                updateWindow.Close();
                                Application.Current.Shutdown();
                            };

                            Border border = new Border {

                                CornerRadius = new CornerRadius(15),
                                Background = Brushes.White,
                                BorderBrush = new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                                BorderThickness = new Thickness(3),
                                Padding = new Thickness(10),
                                Child = stackPanel
                            };

                            stackPanel.Children.Add(message);
                            stackPanel.Children.Add(progressBar);
                            stackPanel.Children.Add(yesButton);
                            stackPanel.Children.Add(noButton);
                            updateWindow.Content = stackPanel;
                            updateWindow.Content = border;
                            updateWindow.ShowDialog();
                        });
                    }
                }
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    Window errorWindow = new Window {

                        Title = "Error",
                        Width = 350,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize,
                        Topmost = true,
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true,
                        Background = Brushes.Transparent
                    };

                    errorWindow.Closing += (s, e) => {

                        e.Cancel = true;
                    };

                    StackPanel stackPanel = new StackPanel {

                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    TextBlock message = new TextBlock {

                        Text = $"⚠️ \n Error checking for updates.",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Red,
                        TextAlignment = TextAlignment.Center
                    };

                    Button exitButton = new Button {

                        Content = "OK",
                        Width = 100,
                        Margin = new Thickness(10),
                        Background = Brushes.Red,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0)
                    };

                    exitButton.Click += (s, e) => {

                        Application.Current.Shutdown();
                    };

                    Border border = new Border {

                        CornerRadius = new CornerRadius(15),
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                        BorderThickness = new Thickness(3),
                        Padding = new Thickness(10),
                        Child = stackPanel
                    };

                    stackPanel.Children.Add(message);
                    stackPanel.Children.Add(exitButton);
                    errorWindow.Content = stackPanel;
                    errorWindow.Content = border;
                    errorWindow.ShowDialog();
                });
            }
        }

        private bool IsInternetAvailable() {

            try {

                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com")) {

                    return true;
                }
            }
            catch {

                return false;
            }
        }

        private void DownloadUpdate(ProgressBar progressBar, Window updateWindow) {

            try {

                using (WebClient client = new WebClient()) {

                    client.DownloadProgressChanged += (s, e) => {

                        Application.Current.Dispatcher.Invoke(() => {

                            progressBar.Value = e.ProgressPercentage;
                        });
                    };

                    client.DownloadFileCompleted += (s, e) => {

                        Application.Current.Dispatcher.Invoke(() => {

                            if (e.Error != null) {

                                ShowCustomMessage("Error", $"An error occurred while downloading the update:\n{e.Error.Message}", true);
                                return;
                            }

                            ShowCustomMessage("Success", "Downloaded Successfully.", false);
                            InstallUpdate(Process.GetCurrentProcess().MainModule.FileName, Process.GetCurrentProcess().MainModule.FileName + ".new");
                        });
                    };

                    string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                    string tempPath = currentPath + ".new";

                    client.DownloadFileAsync(new Uri(updateUrl), tempPath);
                }
            }
            catch (Exception ex) {

                Application.Current.Dispatcher.Invoke(() => {

                    MessageBox.Show($"An error occurred while downloading the update: {ex.Message}");
                });
            }
        }

        private void InstallUpdate(string currentPath, string tempPath) {
            
            try {

                string batchScript = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "update.bat");

                File.WriteAllText(batchScript, $@"
        @echo off
        :loop
        tasklist | find /i ""{System.IO.Path.GetFileName(currentPath)}"" >nul 2>nul
        if not errorlevel 1 (
            timeout /t 1 >nul
            goto loop
        )
        move /y ""{tempPath}"" ""{currentPath}""
        start """" ""{currentPath}""
        del ""%~f0""
        ");

                Process.Start(new ProcessStartInfo {

                    FileName = batchScript,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex) {

                MessageBox.Show($"An error occurred while installing the update: {ex.Message}");
            }
        }

        private void ShowCustomMessage(string title, string message, bool isError) {


            Window messageWindow = new Window {

                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                WindowStyle = WindowStyle.None,
                Owner = Application.Current.MainWindow
            };

            StackPanel stackPanel = new StackPanel {

                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string symbol = isError ? "⚠️" : "✔";

            TextBlock messageText = new TextBlock {

                Text = $"{symbol}\n{message}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button okButton = new Button {

                Content = "OK",
                Width = 100,
                Margin = new Thickness(10),
                Background = isError ? Brushes.Red : Brushes.Green,
                Foreground = Brushes.White
            };

            okButton.Click += (s, e) => {

                messageWindow.Close();
            };

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(okButton);
            messageWindow.Content = stackPanel;
            messageWindow.ShowDialog();
        }

        private void ShowCustomMessageSystemInfo(string title, string message, bool isError) {

            Window messageWindow = new Window {

                Title = title,
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Owner = Application.Current.MainWindow,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                ShowInTaskbar = false
            };

            Grid mainGrid = new Grid { Margin = new Thickness(0) };

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border header = new Border {

                Background = new SolidColorBrush(Color.FromRgb(0, 136, 255)),
                Height = 50,
                Child = new TextBlock {

                    Text = title,
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            Grid.SetRow(header, 0);
            mainGrid.Children.Add(header);

            Border contentBorder = new Border {

                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(20),
                Effect = new System.Windows.Media.Effects.DropShadowEffect {

                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.4
                }
            };

            ScrollViewer scrollViewer = new ScrollViewer {

                Content = new TextBlock {

                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(34, 34, 34)),
                    VerticalAlignment = VerticalAlignment.Top
                },
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            contentBorder.Child = scrollViewer;

            Grid.SetRow(contentBorder, 1);
            mainGrid.Children.Add(contentBorder);

            Button closeButton = new Button {

                Content = "CLOSE",
                Width = 100,
                Height = 36,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(0, 136, 255)),
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => { messageWindow.Close(); };

            Grid.SetRow(closeButton, 2);
            mainGrid.Children.Add(closeButton);

            messageWindow.Content = mainGrid;

            messageWindow.ShowDialog();
        }

        private string selectedPackage = "";
        string randomDeviceId = GenerateRandomNumber(17);
        string randomAndroidId = GenerateRandomNumber(16);

        [DllImport("psapi.dll")]
        static extern bool EmptyWorkingSet(IntPtr hProcess);

        private void CheckFileAndShowButtons() {

            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opfarhatfrsn.txt");
            bool showButtons = false;

            if (File.Exists(filePath)) {

                string content = File.ReadAllText(filePath).Trim();

                if (content == "opfrsnfarhat") {

                    Dispatcher.Invoke(() => {

                        Button1.Visibility = Visibility.Visible;
                        Button2.Visibility = Visibility.Visible;
                        Button3.Visibility = Visibility.Visible;
                        Button4.Visibility = Visibility.Visible;
                    });
                    showButtons = true;
                }
            }

            if (!showButtons) {

                OpenBrowserLink("https://www.opal3ab.com/discord");
            }
        }

        private void OpenBrowserLink(string url) {

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {

                FileName = url,
                UseShellExecute = true
            });
        }

        public static double CalculateSimilarity(int firstNumber, int secondNumber) {

            return (1.0 - Math.Abs(firstNumber - secondNumber) / (double)Math.Max(firstNumber, secondNumber)) * 100;
        }

        private async void LoadAndCustomizeForm() {

            await downloadFiles();
        }

        private async Task downloadFiles() {

            await Task.Run(() => {
                string currentUser = Environment.UserName;

                string savePath = $@"C:\Users\{currentUser}\";

                using (WebClient client = new WebClient()) {

                    if (!File.Exists(System.IO.Path.Combine(savePath, "adb.exe"))) {

                        client.DownloadFile("https://raw.githubusercontent.com/FR7AT/OPTOP/main/OP/adb.exe", System.IO.Path.Combine(savePath, "adb.exe"));
                    }

                    if (!File.Exists(System.IO.Path.Combine(savePath, "AdbWinUsbApi.dll"))) {

                        client.DownloadFile("https://raw.githubusercontent.com/FR7AT/OPTOP/main/OP/AdbWinUsbApi.dll", System.IO.Path.Combine(savePath, "AdbWinUsbApi.dll"));
                    }

                    if (!File.Exists(System.IO.Path.Combine(savePath, "AdbWinApi.dll"))) {

                        client.DownloadFile("https://raw.githubusercontent.com/FR7AT/OPTOP/main/OP/AdbWinApi.dll", System.IO.Path.Combine(savePath, "AdbWinApi.dll"));
                    }
                }
            });
        }

        private void DisableAllButtonsExcept(Button activeButton) {

            foreach (var child in LogicalTreeHelper.GetChildren(SystemPage)) {

                if (child is Button button && button != activeButton) {

                    button.IsEnabled = false;
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(GameLoopPage)) {

                if (child is Button button && button != activeButton) {

                    button.IsEnabled = false;
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(AndroidPage)) {

                if (child is Button button && button != activeButton) {

                    button.IsEnabled = false;
                }
            }
        }

        private void EnableAllButtons() {

            foreach (var child in LogicalTreeHelper.GetChildren(SystemPage)) {

                if (child is Button button) {

                    button.IsEnabled = true;
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(GameLoopPage)) {

                if (child is Button button) {

                    button.IsEnabled = true;
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(AndroidPage)) {

                if (child is Button button) {

                    button.IsEnabled = true;
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem selectedItem) {

                HomePage.Visibility = Visibility.Collapsed;
                GameLoopPage.Visibility = Visibility.Collapsed;
                AndroidPage.Visibility = Visibility.Collapsed;
                SystemPage.Visibility = Visibility.Collapsed;
                ProgramsPage.Visibility = Visibility.Collapsed;

                var tag = selectedItem.Tag.ToString();
                var selectedPage = this.FindName(tag) as UIElement;
                if (selectedPage != null) {

                    selectedPage.Visibility = Visibility.Visible;
                }
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e) {

            ProgramsPage1.Visibility = Visibility.Collapsed;
            ProgramsPage2.Visibility = Visibility.Visible;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e) {

            ProgramsPage2.Visibility = Visibility.Collapsed;
            ProgramsPage1.Visibility = Visibility.Visible;
        }

        private void ExecuteCommandLine(string command) {

            try {

                ProcessStartInfo startInfo = new ProcessStartInfo {

                    FileName = "cmd.exe",
                    Arguments = "/c " + command,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = new Process()) {

                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception) {

            }
        }

        private void ExecuteAdbCommand(string command) {

            string currentUser = Environment.UserName;
            string adbPath = $@"C:\Users\{currentUser}\adb.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo {

                FileName = adbPath,
                Arguments = command,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process process = new Process()) {

                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("Output: " + output);
                Console.WriteLine("Error: " + error);
            }
        }

        private string GenerateRandomString(int length) {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        static string GenerateRandomNumber(int length) {

            var random = new Random(Guid.NewGuid().GetHashCode());
            var randomValue = new char[length];
            for (int i = 0; i < length; i++) {
                randomValue[i] = (char)('0' + random.Next(0, 10));
            }
            return new string(randomValue);
        }

        private async Task DownloadFileAsync(string url, ProgressBar progressBar) {

            try {

                string fileName = System.IO.Path.GetFileName(new Uri(url).AbsolutePath);

                progressBar.Value = 0;
                progressBar.Visibility = Visibility.Visible;

                using (WebClient client = new WebClient()) {
                    client.DownloadProgressChanged += (s, e) => {

                        Dispatcher.Invoke(() => {
                            progressBar.Value = e.ProgressPercentage;
                        });
                    };

                    await client.DownloadFileTaskAsync(new Uri(url), fileName);
                }
            }
            catch {

            }
            finally {

                progressBar.Visibility = Visibility.Hidden;
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

            var comboBox = sender as System.Windows.Controls.ComboBox;
            var selectedOption = (comboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

            switch (selectedOption) {

                case "Global":
                    selectedPackage = "com.tencent.ig";
                    break;
                case "Korea":
                    selectedPackage = "com.pubg.krmobile";
                    break;
                case "Taiwan":
                    selectedPackage = "com.rekoo.pubgm";
                    break;
                case "Vietnam":
                    selectedPackage = "com.vng.pubgmobile";
                    break;
                default:
                    selectedPackage = "";
                    break;
            }
        }

        private async void DownloadFullDriver_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Full_Driver.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadAllInOne_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/All_in_One_Runtimes.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadUpdateBlocker_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Windows_Update_Blocker.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadDirectx11_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/DirectX_11.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadDirectx_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/DirectX.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadJava_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Java.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadDefender_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Defender_Control.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadUltraViewer_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/UltraViewer.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadRevoUninstaller_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Revo_Uninstaller_Pro.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadNetmodVpn_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Netmod_vpn.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadUnlocker_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Unlocker.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadWin10Active_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Windows_10_activation.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadTrafficMonitor_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Traffic_Monitor.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadFaststone10_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Faststone_10.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadEditHosts_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Edit_Hosts.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadCamtasia_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Camtasia_Studio.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadWinrar_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/winrar.exe";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadShadowDefender_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Shadow_defender.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadCCleaner_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/CCleaner.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadMalware_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Malware.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadMicrosoftXna_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Microsoft_XNA.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadNetFramework_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/NET_Framework.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadSystemInfo_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Systeminformer.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadNotepad_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Notepad++.rar";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, FileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DisableReports_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteCommandLine("reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\" /v Disabled /t REG_DWORD /d 1 /f");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Reports Disabled Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void ChangeMACAddress_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                string script = @"
        $adapter = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' } | Select-Object -First 1

        if ($adapter -ne $null) {
            $randomMac = -join ((1..6 | ForEach-Object { '{0:X2}' -f (Get-Random -Minimum 0 -Maximum 256) }))

            Set-NetAdapterAdvancedProperty -Name $adapter.Name -RegistryKeyword 'NetworkAddress' -RegistryValue $randomMac

            Disable-NetAdapter -Name $adapter.Name -Confirm:$false
            Enable-NetAdapter -Name $adapter.Name -Confirm:$false
        }
        ";

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{script}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Mac Address Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void DeleteCache_Click(object sender, RoutedEventArgs e) {
            
            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                string currentUser = Environment.UserName;

                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cache\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Microsoft\\Edge\\User Data\\Default\\Cache\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Mozilla\\Firefox\\Profiles\\*\\cache2\\entries\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Roaming\\Opera Software\\Opera Stable\\Cache\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Cache\"");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Cache Cleared Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void ResetNetwork_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteCommandLine("ipconfig /flushdns");
                ExecuteCommandLine("ipconfig /release");
                ExecuteCommandLine("ipconfig /renew");
                ExecuteCommandLine("netsh int ip reset");
                ExecuteCommandLine("netsh winsock reset");
                ExecuteCommandLine("netsh interface ipv4 reset");
                ExecuteCommandLine("netsh interface ipv6 reset");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Network Reset Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void CleanDisk_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteCommandLine("rd /Q /S C:\\$Recycle.Bin");
                ExecuteCommandLine("Cleanmgr /sagerun:16");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Disk Cleaned Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void ResetFirewall_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteCommandLine("netsh advfirewall reset");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Firewall Reset Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void CleanTmp_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                string currentUser = Environment.UserName;
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Microsoft\\Windows\\Temporary Internet Files\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Roaming\\Microsoft\\Office\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Microsoft\\Windows\\INetCookies\"");
                ExecuteCommandLine($"rmdir /S /Q \"C:\\Users\\{currentUser}\\AppData\\Local\\Temp\"");
                ExecuteCommandLine("del /F /S /Q C:\\*.tmp");
                ExecuteCommandLine("del /F /S /Q C:\\*._mp");
                ExecuteCommandLine("del /F /S /Q C:\\*.log");
                ExecuteCommandLine("del /F /S /Q C:\\*.gid");
                ExecuteCommandLine("del /F /S /Q C:\\*.chk");
                ExecuteCommandLine("del /F /S /Q C:\\*.old");
                ExecuteCommandLine("del /F /S /Q C:\\Windows\\*.bak");
                ExecuteCommandLine("del C:\\WIN386.SWP");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Temporary Files Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void AddHosts_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(async () => {

                string link = "https://farhat.opal3ab.com/optools/hosts";
                string targetPath = @"C:\Windows\System32\drivers\etc\hosts";

                if (File.Exists(targetPath)) {

                    File.Delete(targetPath);
                }

                string tempFilePath = System.IO.Path.GetTempFileName();

                using (HttpClient client = new HttpClient()) {

                    var response = await client.GetAsync(link);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                File.Copy(tempFilePath, targetPath, true);
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Hosts Updated Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void CleanTencent_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AppMarket.exe" };

                foreach (string process in processesToKill) {

                    ExecuteCommandLine("taskkill /F /IM " + process);
                }

                ExecuteCommandLine("net stop QMEmulatorService");
                ExecuteCommandLine("net stop aow_drv");
                ExecuteCommandLine("del /Q C:\\aow_drv.log");

                string currentUser = Environment.UserName;
                ExecuteCommandLine($"rmdir /S /Q C:\\Users\\{currentUser}\\AppData\\Local\\Tencent");
                ExecuteCommandLine($"rmdir /S /Q C:\\Users\\{currentUser}\\AppData\\Roaming\\Tencent");
                ExecuteCommandLine("rmdir /S /Q C:\\ProgramData\\Tencent");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Tencent Data Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void P1080P_Korea_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AppMarket.exe", "GameLoop.exe" };

                foreach (string process in processesToKill) {

                    ExecuteCommandLine("taskkill /F /IM " + process);
                }

                ExecuteCommandLine("net stop QMEmulatorService");
                ExecuteCommandLine("net stop aow_drv");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""VMDeviceManufacturer"" /t REG_SZ /d ""samsung"" /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""VMDeviceModel"" /t REG_SZ /d ""SM-X926B"" /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""VMPhoneDevice"" /t REG_SZ /d ""Galaxy Tab S10 Ultra 5G"" /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""com.pubg.krmobile_ContentScale"" /t REG_DWORD /d ""1"" /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""com.pubg.krmobile_FPSLevel"" /t REG_DWORD /d ""90"" /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""com.pubg.krmobile_RenderQuality"" /t REG_DWORD /d ""1"" /f");

            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "1080P Korea Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async Task UpdateHostsFile() {

            try {

                string link = "https://farhat.opal3ab.com/optools/Korean/hosts";
                string targetPath = @"C:\Windows\System32\drivers\etc\hosts";

                if (File.Exists(targetPath)) {

                    File.Delete(targetPath);
                }

                string tempFilePath = System.IO.Path.GetTempFileName();

                using (HttpClient client = new HttpClient()) {

                    var response = await client.GetAsync(link);
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {

                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                File.Copy(tempFilePath, targetPath, true);
            }
            catch {

            }
        }

        private async void Key_Keymap_Kr_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await UpdateHostsFile();

                await Task.Run(async () => {

                string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AppMarket.exe", "GameLoop.exe" };

                foreach (string process in processesToKill) {

                    ExecuteCommandLine("taskkill /F /IM " + process);
                }

                object regValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MobileGamePC", "DisplayIcon", null);

                if (regValue != null && regValue is string regString) {

                    string basePath = System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(regString))
                                      .Replace("appmarket\\AppMarket.exe", "");
                    string targetPath = System.IO.Path.Combine(basePath, "ui");

                    string checkFilePath = System.IO.Path.Combine(targetPath, "AEngine.dll");
                    if (!File.Exists(checkFilePath)) {
                        return;
                    }

                    if (!Directory.Exists(targetPath)) {

                        Directory.CreateDirectory(targetPath);
                    }

                    string filePath = System.IO.Path.Combine(targetPath, "fn4.rar");
                    using (HttpClient client = new HttpClient()) {

                        byte[] fileBytes = await client.GetByteArrayAsync("https://farhat.opal3ab.com/optools/Korean/fn4.rar");
                        File.WriteAllBytes(filePath, fileBytes);
                    }

                    string unrarPath = System.IO.Path.Combine(targetPath, "UnRAR.exe");
                    using (HttpClient client = new HttpClient()) {

                        byte[] unrarBytes = await client.GetByteArrayAsync("https://farhat.opal3ab.com/optools/Korean/UnRAR.exe");
                        File.WriteAllBytes(unrarPath, unrarBytes);
                    }

                    string outputDir = targetPath;
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = unrarPath;
                    process.StartInfo.Arguments = $"x -y \"{filePath}\" \"{outputDir}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    File.Delete(filePath);
                    File.Delete(unrarPath);
                }

            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Key Keymap Korea Successfully!", "success");
                    successWindow.Show();
                });
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {
                EnableAllButtons();
            }
        }

        private async void Key_Keymap_Gl_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(async () => {

                string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AppMarket.exe", "GameLoop.exe" };

                foreach (string process in processesToKill) {

                    ExecuteCommandLine("taskkill /F /IM " + process);
                }

                object regValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MobileGamePC", "DisplayIcon", null);
                if (regValue != null && regValue is string regString) {

                    string basePath = System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(regString))
                                      .Replace("appmarket\\AppMarket.exe", "");
                    string targetPath = System.IO.Path.Combine(basePath, "ui");

                    string checkFilePath = System.IO.Path.Combine(targetPath, "AEngine.dll");
                    if (!File.Exists(checkFilePath)) {
                        return;
                    }

                    if (!Directory.Exists(targetPath)) {

                        Directory.CreateDirectory(targetPath);
                    }

                    string filePath = System.IO.Path.Combine(targetPath, "Gl1.rar");
                    using (HttpClient client = new HttpClient()) {

                        byte[] fileBytes = await client.GetByteArrayAsync("https://farhat.opal3ab.com/optools/Global/Gl1.rar");
                        File.WriteAllBytes(filePath, fileBytes);
                    }

                    string unrarPath = System.IO.Path.Combine(targetPath, "UnRAR.exe");
                    using (HttpClient client = new HttpClient()) {

                        byte[] unrarBytes = await client.GetByteArrayAsync("https://farhat.opal3ab.com/optools/Korean/UnRAR.exe");
                        File.WriteAllBytes(unrarPath, unrarBytes);
                    }

                    string outputDir = targetPath;
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = unrarPath;
                    process.StartInfo.Arguments = $"x -y \"{filePath}\" \"{outputDir}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    File.Delete(filePath);
                    File.Delete(unrarPath);
                }
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Key Keymap Global Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void InstallApkButton_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""AdbDisable"" /t REG_DWORD /d 0 /f");
                ExecuteCommandLine(@"Reg.exe add ""HKCU\SOFTWARE\Tencent\MobileGamePC"" /v ""RootEnabled"" /t REG_DWORD /d 1 /f");
                ExecuteAdbCommand("kill-server");
                ExecuteAdbCommand("devices");
                ExecuteAdbCommand("install *.apk");
            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Apk Installed Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }

        }

        private async void DownloadGameloop_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://down.gameloop.com/channel/3/16412/GLP_installer_1000218456_market.exe";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, AndroidFileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadApkPure_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/APKPure.apk";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, AndroidFileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadFXFile_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Fx_file_explorer.apk";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, AndroidFileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadArabicKd_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Arabic_keyboard.apk";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, AndroidFileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void DownloadTwitter_Click(object sender, RoutedEventArgs e) {

            string downloadUrl = "https://farhat.opal3ab.com/optools/Twitter.apk";

            if (string.IsNullOrEmpty(downloadUrl) || !Uri.IsWellFormedUriString(downloadUrl, UriKind.Absolute) || !downloadUrl.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });

                return;
            }

            await DownloadFileAsync(downloadUrl, AndroidFileDownloadProgressBar);

            Application.Current.Dispatcher.Invoke(() => {

                MainWindow successWindow = new MainWindow("Success", "Downloaded Successfully!", "success");
                successWindow.Show();
            });
        }

        private async void PaksOut_Click(object sender, RoutedEventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {

                ShowCustomMessage("Info", "Please Select Pubg Version.", true);
                return;
            }

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteAdbCommand("kill-server");
                ExecuteAdbCommand("devices");
                ExecuteAdbCommand($"shell cp -r /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/Paks /data/share1");

            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Paks Copied Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void PaksIn_Click(object sender, RoutedEventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {

                ShowCustomMessage("Info", "Please Select Pubg Version.", true);
                return;
            }

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                ExecuteAdbCommand("kill-server");
                ExecuteAdbCommand("devices");
                ExecuteAdbCommand($"shell cp -r /data/share1/Paks /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved");

            });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Paks Copied Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void OpenGameloop_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);
            
            string selectedFile = "";

            Application.Current.Dispatcher.Invoke(() => {

                Window selectionWindow = new Window {

                    Title = "Select emulator",
                    Width = 250,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Transparent,
                    AllowsTransparency = true,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false
                };

                Border border = new Border {

                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.DodgerBlue,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(15),
                    Background = Brushes.White,
                };

                StackPanel stackPanel = new StackPanel {

                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button yesButton = new Button {

                    Content = "AndroidEmulatorEx",
                    Width = 160,
                    Height = 40,
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    Margin = new Thickness(10),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                Button noButton = new Button {

                    Content = "AndroidEmulatorEn",
                    Width = 160,
                    Height = 40,
                    Background = Brushes.DodgerBlue,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    Margin = new Thickness(10),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                Button closeButton = new Button {

                    Content = "CLOSE",
                    Width = 70,
                    Height = 30,
                    Background = Brushes.IndianRed,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    Margin = new Thickness(10),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                yesButton.Click += (senderArgs, eventArgs) => {

                    selectedFile = "AndroidEmulatorEx.exe";
                    selectionWindow.DialogResult = true;
                };

                noButton.Click += (senderArgs, eventArgs) => {

                    selectedFile = "AndroidEmulatorEn.exe";
                    selectionWindow.DialogResult = true;
                };

                closeButton.Click += (senderArgs, eventArgs) => {

                    selectionWindow.DialogResult = false;
                };

                stackPanel.Children.Add(yesButton);
                stackPanel.Children.Add(noButton);
                stackPanel.Children.Add(closeButton);
                border.Child = stackPanel;

                selectionWindow.Content = border;

                bool? result = selectionWindow.ShowDialog();

                if (result != true) {

                    selectedFile = "";
                }
            });

            if (string.IsNullOrEmpty(selectedFile)) {

                EnableAllButtons();
                return;
            }

            await Task.Run(() => {

                try {

                    string filePath = "";

                    object regValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MobileGamePC", "DisplayIcon", null);

                    if (regValue is string regString) {

                        string s = Encoding.Default.GetString(Encoding.Default.GetBytes(regString));
                        s = s.Replace("appmarket\\AppMarket.exe", "");
                        filePath = System.IO.Path.Combine(s, "ui", selectedFile);

                        string uiPath = System.IO.Path.Combine(s, "AOW_Rootfs_100", "0");
                        string file188Path = System.IO.Path.Combine(uiPath, "188");
                        string searchText = "ro.build.version.release=";
                        string replaceText = "ro.build.version.release=11";

                        if (File.Exists(file188Path)) {

                            string[] lines = File.ReadAllLines(file188Path);

                            for (int i = 0; i < lines.Length; i++) {

                                if (lines[i].StartsWith(searchText)) {

                                    lines[i] = replaceText;
                                }
                            }

                            File.WriteAllLines(file188Path, lines);
                        }

                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) {

                            ProcessStartInfo startInfo = new ProcessStartInfo {

                                FileName = filePath,
                                Arguments = "-vm 100",
                                UseShellExecute = true,
                                CreateNoWindow = false
                            };

                            Process.Start(startInfo);

                            Application.Current.Dispatcher.Invoke(() => {

                                MainWindow successWindow = new MainWindow("Success", "Gameloop Open Successfully!", "success");
                                successWindow.Show();
                            });
                        }
                    }
                    else {

                        Application.Current.Dispatcher.Invoke(() => {

                            MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                            errorWindow.Show();
                        });
                    }
                }
                catch (Exception) {

                }
            });

            EnableAllButtons();
        }

        private async void KillGameloop_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AppMarket.exe", "GameLoop.exe" };

                    foreach (string process in processesToKill) {

                        ExecuteCommandLine("taskkill /F /IM " + process);
                    }

                    ExecuteCommandLine("net stop QMEmulatorService");
                    ExecuteCommandLine("net stop aow_drv");
                    ExecuteCommandLine("del /Q C:\\aow_drv.log");
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Gameloop Kill Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void FixGameloop_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            await Task.Run(() => {

                string[] processesToKill = { "AndroidEmulatorEn.exe", "AndroidEmulatorEx.exe", "AndroidEmulator.exe", "AppMarket.exe", "GameLoop.exe" };

                foreach (string process in processesToKill) {

                    ExecuteCommandLine("taskkill /F /IM " + process);
                }

                ExecuteCommandLine("net stop QMEmulatorService");
                ExecuteCommandLine("net stop aow_drv");

                object regValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MobileGamePC", "DisplayIcon", null);

                if (regValue != null && regValue is string regString) {

                    string basePath = Encoding.Default.GetString(Encoding.Default.GetBytes(regString));
                    basePath = basePath.Replace("appmarket\\AppMarket.exe", "").Trim();

                    string uiPath = System.IO.Path.Combine(basePath, "AOW_Rootfs_100\\0");

                    if (Directory.Exists(uiPath)) {

                        string[] filesToDelete = { "0.ini", "0", "368", "368.ini" };

                        foreach (string fileName in filesToDelete) {

                            string filePath = System.IO.Path.Combine(uiPath, fileName);

                            if (File.Exists(filePath)) {

                                File.Delete(filePath);
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() => {

                            MainWindow successWindow = new MainWindow("Success", "Gameloop Fix Successfully!", "success");
                            successWindow.Show();
                        });
                    }
                }

                else {

                    Application.Current.Dispatcher.Invoke(() => {

                        MainWindow warningWindow = new MainWindow("Error", "Operation Encountered A Problem!", "Error");
                        warningWindow.Show();
                    });
                }
            });

            EnableAllButtons();
        }

        private async void CleanPubg_Click(object sender, RoutedEventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {

                ShowCustomMessage("Info", "Please Select Pubg Version.", true);
                return;
            }

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    ExecuteAdbCommand("kill-server");
                    ExecuteAdbCommand("devices");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/Paks /sdcard/Android/data");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames/Active.sav /sdcard/Android/data");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/{selectedPackage}/files/ProgramBinaryCache /sdcard/Android/data");
                    ExecuteAdbCommand($"shell mv /data/data/{selectedPackage}/shared_prefs/device_id.xml /sdcard/Android/data");
                    ExecuteAdbCommand($"shell pm clear {selectedPackage}");
                    ExecuteAdbCommand($"shell mkdir -p /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames");
                    ExecuteAdbCommand($"shell mkdir -p /data/data/{selectedPackage}/shared_prefs");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/Paks /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/Active.sav /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/ProgramBinaryCache /sdcard/Android/data/{selectedPackage}/files");
                    ExecuteAdbCommand($"shell mv /sdcard/Android/data/device_id.xml /data/data/{selectedPackage}/shared_prefs");
                    ExecuteAdbCommand($"shell pm grant {selectedPackage} android.permission.WRITE_EXTERNAL_STORAGE");
                    ExecuteAdbCommand($"shell pm grant {selectedPackage} android.permission.RECORD_AUDIO");
                    ExecuteAdbCommand($"shell pm grant {selectedPackage} android.permission.READ_CALENDAR");
                    ExecuteAdbCommand("shell find / -name \"HANYCJLZOEUS_TOKEN2.dat\" -exec rm {} \\;");
                    ExecuteAdbCommand("shell find / -name \".system_android_l2\" -exec rm {} \\;");
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Pubg Cleaned Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void FixBanGameloop_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    string regCommand1 = "reg add \"HKEY_CURRENT_USER\\Software\\Tencent\\MobileGamePC\" /v \"VMDeviceManufacturer\" /t REG_SZ /d samsung /f";
                    string randomString = GenerateRandomString(1);
                    string regCommand2 = "reg add \"HKEY_CURRENT_USER\\Software\\Tencent\\MobileGamePC\" /v \"VMDeviceModel\" /t REG_SZ /d SM-G988" + randomString + " /f";

                    ExecuteCommandLine(regCommand1);
                    ExecuteCommandLine(regCommand2);
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Fix Gameloop Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void ResetGuest_Click(object sender, RoutedEventArgs e) {

            if (string.IsNullOrEmpty(selectedPackage)) {

                ShowCustomMessage("Info", "Please Select Pubg Version.", true);
                return;
            }

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    ExecuteAdbCommand("kill-server");
                    ExecuteAdbCommand("devices");
                    ExecuteAdbCommand($"shell setprop android.device.id {randomDeviceId}");
                    ExecuteAdbCommand($"shell setprop ro.android_id {randomAndroidId}");
                    ExecuteAdbCommand($"shell rm -rf /data/share1/hardware_info.txt");
                    ExecuteAdbCommand($"shell rm -rf /data/share1/pictures");
                    ExecuteAdbCommand($"shell rm -rf /data/data/{selectedPackage}/databases");
                    ExecuteAdbCommand($"shell rm -rf /data/data/{selectedPackage}/files");
                    ExecuteAdbCommand($"shell rm -rf /data/data/{selectedPackage}/shared_prefs/device_id.xml");
                    ExecuteAdbCommand($"shell rm -rf /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Intermediate");
                    ExecuteAdbCommand($"shell rm -rf /sdcard/Android/data/{selectedPackage}/files/UE4Game/ShadowTrackerExtra/ShadowTrackerExtra/Saved/SaveGames/loginInfoFile.json");
                    ExecuteAdbCommand("shell find / -name \"HANYCJLZOEUS_TOKEN2.dat\" -exec rm {} \\;");
                    ExecuteAdbCommand("shell find / -name \".system_android_l2\" -exec rm {} \\;");
                    ExecuteAdbCommand($"shell am start {selectedPackage}/com.epicgames.ue4.SplashActivity filter");
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Guest Reset Successfully!", "success");
                    successWindow.Show();
                });
            }

            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }

            finally {

                EnableAllButtons();
            }
        }

        private async void opfarhatAnogs_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    Thread.Sleep(1000);
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Libue4 Preview Executed", "success");
                    successWindow.Show();
                });
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }
            finally {

                EnableAllButtons();
            }
        }

        private async void opfarhatUe4_Click(object sender, RoutedEventArgs e) {
            
            DisableAllButtonsExcept((Button)sender);

            try {
                await Task.Run(() => {

                    Thread.Sleep(1000);
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Libue4 Preview Executed", "success");
                    successWindow.Show();
                });
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }
            finally {

                EnableAllButtons();
            }
        }

        private async void opfarhatOpenGameloop_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            await Task.Run(() => {

                string fileName = "AndroidEmulatorEx.exe";
                string filePath = "";

                object regValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MobileGamePC", "DisplayIcon", null);

                if (regValue != null && regValue is string regString) {

                    byte[] value = System.Text.Encoding.Default.GetBytes(regString);
                    string s = System.Text.Encoding.Default.GetString(value);
                    s = s.Replace("appmarket\\AppMarket.exe", "");
                    filePath = System.IO.Path.Combine(s, "ui", fileName);

                    string uiPath = System.IO.Path.Combine(s, "AOW_Rootfs_100", "0");
                    string file188Path = System.IO.Path.Combine(uiPath, "188");
                    string searchText = "ro.build.version.release=";
                    string replaceText = "ro.build.version.release=11";

                    if (File.Exists(file188Path)) {

                        string[] lines = File.ReadAllLines(file188Path);

                        for (int i = 0; i < lines.Length; i++) {

                            if (lines[i].StartsWith(searchText)) {

                                lines[i] = replaceText;
                            }
                        }

                        File.WriteAllLines(file188Path, lines);
                    }

                    if (!string.IsNullOrEmpty(filePath)) {

                        string arguments = "-cmd StartApk -param -startpkg com.tencent.ig -engine aow -vm 100";
                        ProcessStartInfo startInfo = new ProcessStartInfo {

                            FileName = filePath,
                            Arguments = arguments,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        };

                        Process.Start(startInfo);

                        Application.Current.Dispatcher.Invoke(() => {

                            MainWindow successWindow = new MainWindow("Success", "Gameloop Open Successfully!", "success");
                            successWindow.Show();
                        });
                    }
                }

                else {

                    Application.Current.Dispatcher.Invoke(() => {

                        MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                        errorWindow.Show();
                    });
                }
            });

            EnableAllButtons();
        }

        private async void DisplaySystemInfo_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                string systemInfo = string.Empty;

                await Task.Run(() => {

                    var computerSystemSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                    foreach (var info in computerSystemSearcher.Get()) {

                        systemInfo += $"Manufacturer: {info["Manufacturer"]}\n";
                        systemInfo += $"Model: {info["Model"]}\n";
                        systemInfo += $"Memory: {Convert.ToInt64(info["TotalPhysicalMemory"]) / (1024 * 1024)} MB\n";
                        string fullUserName = info["UserName"]?.ToString();
                        if (!string.IsNullOrEmpty(fullUserName) && fullUserName.Contains("\\")) {

                            systemInfo += $"User Name: {fullUserName.Split('\\')[1]}\n";
                        }
                    }

                    var operatingSystemSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                    foreach (var info in operatingSystemSearcher.Get()) {

                        systemInfo += $"System: {info["Caption"]}\n";
                        systemInfo += $"Version: {info["Version"]}\n";
                        systemInfo += $"Architecture: {info["OSArchitecture"]}\n";
                        systemInfo += $"Build Number: {info["BuildNumber"]}\n";
                        systemInfo += $"Serial Number: {info["SerialNumber"]}\n";
                    }

                    var processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                    foreach (var info in processorSearcher.Get()) {

                        systemInfo += $"Processor: {info["Name"]}\n";
                        systemInfo += $"Cores: {info["NumberOfCores"]}\n";
                        systemInfo += $"Threads: {info["ThreadCount"]}\n";
                    }
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "System Info Successfully!", "success");
                    successWindow.Show();
                    ShowCustomMessageSystemInfo("System Info", systemInfo, true);

                });
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() => {
                    MainWindow errorWindow = new MainWindow("Error", "Operation Encountered A Problem!", "error");
                    errorWindow.Show();
                });
            }
            finally {

                EnableAllButtons();
            }
        }

        private async void ClearRAM_Click(object sender, RoutedEventArgs e) {

            DisableAllButtonsExcept((Button)sender);

            try {

                await Task.Run(() => {

                    foreach (var process in System.Diagnostics.Process.GetProcesses()) {

                        try {

                            EmptyWorkingSet(process.Handle);
                        }
                        catch {

                        }
                    }
                });

                Application.Current.Dispatcher.Invoke(() => {

                    MainWindow successWindow = new MainWindow("Success", "Ram Booster Successfully!", "success");
                    successWindow.Show();
                });
            }
            catch (Exception) {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow errorWindow = new MainWindow("Error", $"Error clearing RAM", "error");
                    errorWindow.Show();
                });
            }
            finally {

                EnableAllButtons();
            }
        }

        private async void ClearRegistry_Click(object sender, RoutedEventArgs e)
        {
            DisableAllButtonsExcept((Button)sender);

            try
            {
                await Task.Run(() =>
                {
                    string[] registryPaths =
                    {
                        "Software\\Microsoft",
                        "Software\\Classes",
                        "Software\\Wow6432Node"
                    };

                    foreach (string path in registryPaths)
                    {
                        try
                        {
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true))
                            {
                                if (key != null)
                                {
                                    string[] subKeys = key.GetSubKeyNames();

                                    foreach (string subKey in subKeys)
                                    {
                                        try
                                        {
                                            using (RegistryKey testKey = key.OpenSubKey(subKey))
                                            {
                                                if (testKey != null && testKey.ValueCount == 0 && testKey.SubKeyCount == 0)
                                                {
                                                    key.DeleteSubKey(subKey);
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow successWindow = new MainWindow("Success", "Registry Clear Successfully!", "success");
                    successWindow.Show();
                });
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow errorWindow = new MainWindow("Error", "Error cleaning registry", "error");
                    errorWindow.Show();
                });
            }
            finally
            {
                EnableAllButtons();
            }
        }

    }
}
