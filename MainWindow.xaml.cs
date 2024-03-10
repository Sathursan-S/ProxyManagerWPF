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
            txtProxyServer.Text = "10.50.225.222";
            txtProxyPort.Text = "3128";
            CheckStatus();
        }

        private void CheckStatus()
        {
            string gitStatus = RunCommandAndOutput("git config --global --get http.proxy");
            if (!string.IsNullOrEmpty(gitStatus))
            {
                chkGit.IsChecked = true;
                txtGit.Text = $"Git proxy: {gitStatus}";
            }
            else
            {
                chkGit.IsChecked = false;
                txtGit.Text = "Git proxy: Not set";
            }

            string npmStatus = RunCommandAndOutput("npm config get proxy");
            if (npmStatus != "null")
            {
                chkNpm.IsChecked = true;
                txtNpm.Text = $"npm proxy: {npmStatus}";
            }
            else
            {
                chkNpm.IsChecked = false;
                txtNpm.Text = "npm proxy: Not set";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
            try
            {
                string proxyServer = txtProxyServer.Text;
                int proxyPort = int.Parse(txtProxyPort.Text);

                //WebProxy proxy = new WebProxy(proxyServer, proxyPort);
                //WebRequest.DefaultWebProxy = proxy;

                if (chkGit.IsChecked == true)
                {
                    string gitCommand = $"git config --global http.proxy http://{proxyServer}:{proxyPort}";
                    RunCommand(gitCommand);

                    Console.WriteLine("Git proxy settings applied.");
                    AppendToConsole("Git proxy settings applied.");
                }
                else
                {
                    string gitCommand = "git config --global --unset http.proxy";
                    RunCommand(gitCommand);
                    RunCommand("git config --global --unset https.proxy");

                    Console.WriteLine("Git proxy settings removed.");
                    AppendToConsole("Git proxy settings removed.");
                }

                if (chkNpm.IsChecked == true)
                {
                    string npmCommand = $"npm config set proxy http://{proxyServer}:{proxyPort}";
                    RunCommand(npmCommand);
                    RunCommand($"npm config set https-proxy http://{proxyServer}:{proxyPort}");
                    Console.WriteLine("npm proxy settings applied.");
                    AppendToConsole("npm proxy settings applied.");
                }
                else
                {
                    string npmCommand = "npm config delete proxy";
                    RunCommand(npmCommand);
                    RunCommand("npm config delete https-proxy");
                    Console.WriteLine("npm proxy settings removed.");
                    AppendToConsole("npm proxy settings removed.");
                }

                CheckStatus();

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

        private string RunCommandAndOutput(string command)
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

                return output.Trim();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AppendToConsole(string text)
        {
            txtConsole.AppendText(text + Environment.NewLine);
            txtConsole.ScrollToEnd();
        }
    }
}
