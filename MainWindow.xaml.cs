using System.Diagnostics;
using System.Net;
using System.Windows;

namespace ProxyManagerWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set default values or perform any additional initialization here
            txtProxyServer.Text = "10.50.225.222";
            txtProxyPort.Text = "3128";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
            try
            {
                // Parse proxy server and port
                string proxyServer = txtProxyServer.Text;
                int proxyPort = int.Parse(txtProxyPort.Text);

                // Create a WebProxy with the specified settings
                WebProxy proxy = new WebProxy(proxyServer, proxyPort);

                // Set the default proxy for all HTTP requests
                WebRequest.DefaultWebProxy = proxy;

                // Simulate applying proxy settings for Git
                if (chkGit.IsChecked == true)
                {
                    // Set Git proxy settings using the git command-line tool
                    string gitCommand = $"git config --global http.proxy http://{proxyServer}:{proxyPort}";
                    RunCommand(gitCommand);

                    Console.WriteLine("Git proxy settings applied.");
                    AppendToConsole("Git proxy settings applied.");
                } else
                {
                    // Unset Git proxy settings using the git command-line tool
                    string gitCommand = "git config --global --unset http.proxy";
                    RunCommand(gitCommand);
                    RunCommand("git config --global --unset https.proxy");

                    Console.WriteLine("Git proxy settings removed.");
                    AppendToConsole("Git proxy settings removed.");
                }

                // Simulate applying proxy settings for npm
                if (chkNpm.IsChecked == true)
                {
                    // Set npm proxy settings using the npm command-line tool
                    string npmCommand = $"npm config set proxy http://{proxyServer}:{proxyPort}";
                    RunCommand(npmCommand);
                    RunCommand($"npm config set https-proxy http://{proxyServer}:{proxyPort}");
                    Console.WriteLine("npm proxy settings applied.");
                    AppendToConsole("npm proxy settings applied.");
                }else
                {
                    // Unset npm proxy settings using the npm command-line tool
                    string npmCommand = "npm config delete proxy";
                    RunCommand(npmCommand);
                    RunCommand("npm config delete https-proxy");
                    Console.WriteLine("npm proxy settings removed.");
                    AppendToConsole("npm proxy settings removed.");
                }

                MessageBox.Show("Proxy settings saved and applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunCommand(string command)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the window or perform any cancellation logic
            Close();
        }

        private void AppendToConsole(string text)
        {
            // Append the text to the TextBox
            txtConsole.AppendText(text + Environment.NewLine);

            // Scroll to the end of the TextBox to show the latest output
            txtConsole.ScrollToEnd();
        }
    }
}
