using Microsoft.Win32;
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
            string proxyServer = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyServer", "") as string;

            if (!string.IsNullOrEmpty(proxyServer))
            {
                string[] proxyParts = proxyServer.Split(':');
                txtProxyServer.Text = proxyParts[0];
                txtProxyPort.Text = proxyParts[1];
            }
            else
            {
                txtProxyServer.Text = "10.50.225.222";
                txtProxyPort.Text = "3128";
            }
            
            CheckStatus();
        }

        private void CheckStatus()
        {
            int proxyEnable = (int)Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings").GetValue("ProxyEnable");
            if (proxyEnable == 1)
            {
                chkGlobal.IsChecked = true;
                //txtGlobal.Text = "Global proxy: Enabled";
            }
            else
            {
                chkGlobal.IsChecked = false;
                //txtGlobal.Text = "Global proxy: Disabled";
            }
            string gitStatus = RunCommandAndOutput("git config --global --get http.proxy");
            if (!string.IsNullOrEmpty(gitStatus))
            {
                chkGit.IsChecked = true;
                //txtGit.Text = $"Git proxy: {gitStatus}";
            }
            else
            {
                chkGit.IsChecked = false;
                //txtGit.Text = "Git proxy: Not set";
            }

            string npmStatus = RunCommandAndOutput("npm config get proxy");
            if (npmStatus != "null")
            {
                chkNpm.IsChecked = true;
                //txtNpm.Text = $"npm proxy: {npmStatus}";
            }
            else
            {
                chkNpm.IsChecked = false;
                //txtNpm.Text = "npm proxy: Not set";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //txtConsole.Clear();
            try
            {
                string proxyServer = txtProxyServer.Text;
                int proxyPort = int.Parse(txtProxyPort.Text);
                string proxySocket = $"http://{proxyServer}:{proxyPort}";

                //WebProxy proxy = new WebProxy(proxyServer, proxyPort);
                //WebRequest.DefaultWebProxy = proxy;
                if (chkGlobal.IsChecked == true)
                {
                    GlobalProxyOn(proxySocket);
                    GitOn(proxyServer, proxyPort);
                    NpmOn(proxyServer, proxyPort);

                    chkGlobal.IsChecked = true;
                    chkGit.IsChecked = true;
                    chkNpm.IsChecked = true;
                }
                else
                {
                    GlobalProxyOff();
                    GitOff();
                    NpmOff();

                    chkGlobal.IsChecked = false;
                    chkGit.IsChecked = false;
                    chkNpm.IsChecked = false;
                }

                if (chkGit.IsChecked == true)
                {
                    GitOn(proxyServer, proxyPort);

                    chkGit.IsChecked = true;

                    Console.WriteLine("Git proxy settings applied.");
                    //AppendToConsole("Git proxy settings applied.");
                }
                else
                {
                    GitOff();

                    chkGit.IsChecked = false;

                    Console.WriteLine("Git proxy settings removed.");
                    //AppendToConsole("Git proxy settings removed.");
                }

                if (chkNpm.IsChecked == true)
                {
                    NpmOn(proxyServer, proxyPort);

                    chkNpm.IsChecked = true;
                    Console.WriteLine("npm proxy settings applied.");
                    //AppendToConsole("npm proxy settings applied.");
                }
                else
                {
                    NpmOff();

                    chkNpm.IsChecked = false;
                    Console.WriteLine("npm proxy settings removed.");
                    //AppendToConsole("npm proxy settings removed.");
                }

                CheckStatus();

                MessageBox.Show("Proxy settings saved and applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void GlobalProxyOff()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 0);
            //registry.SetValue("ProxyServer", "");

            Environment.SetEnvironmentVariable("HTTP_PROXY", null, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("HTTPS_PROXY", null, EnvironmentVariableTarget.User);
        }

        private static void GlobalProxyOn(string proxySocket)
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 1);
            //registry.SetValue("ProxyServer", $"{proxyServer}:{proxyPort}");

            Environment.SetEnvironmentVariable("HTTP_PROXY", proxySocket, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("HTTPS_PROXY", proxySocket, EnvironmentVariableTarget.User);
        }

        private void NpmOff()
        {
            string npmCommand = "npm config delete proxy";
            RunCommand(npmCommand);
            RunCommand("npm config delete https-proxy");
        }

        private void NpmOn(string proxyServer, int proxyPort)
        {
            string npmCommand = $"npm config set proxy http://{proxyServer}:{proxyPort}";
            RunCommand(npmCommand);
            RunCommand($"npm config set https-proxy http://{proxyServer}:{proxyPort}");
        }

        private void GitOff()
        {
            string gitCommand = "git config --global --unset http.proxy";
            RunCommand(gitCommand);
            RunCommand("git config --global --unset https.proxy");
        }

        private void GitOn(string proxyServer, int proxyPort)
        {
            string gitCommand = $"git config --global http.proxy http://{proxyServer}:{proxyPort}";
            RunCommand(gitCommand);
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

        //private void AppendToConsole(string text)
        //{
        //    txtConsole.AppendText(text + Environment.NewLine);
        //    txtConsole.ScrollToEnd();
        //}
    }
}
