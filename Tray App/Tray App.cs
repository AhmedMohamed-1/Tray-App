using System;
using System.ServiceProcess;
using System.IO.Pipes;
using System.IO;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Net;

namespace Tray_App
{
    public partial class TrayApp : Form
    {
        private EventLog _eventLog;
        private NotifyIcon trayIcon;
        private Task _pipeListenerTask;
        private bool _isListening = false;
        private static string ServiceName = "FolderMonitoringService";

        public TrayApp()
        {
            InitializeComponent();
            HideApp();
            RegisterAppToStartup();
            SetupEventLog();
            SetupNotifyIcon();
            //StartService();


            Task.Run(() => StartTcpListener());
        }

        private async Task StartTcpListener()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, 5000);
                listener.Start();
                _eventLog.WriteEntry("TCP Listener started on port 5000.", EventLogEntryType.Information);

                while (true)
                {
                    try
                    {
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        _ = Task.Run(() => HandleTcpClient(client)); // Handle each client in a separate task
                    }
                    catch (Exception ex)
                    {
                        _eventLog.WriteEntry("TCP Server Error in Tray App (Handling Client): " + ex.Message, EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry("TCP Server Error in Tray App (Startup): " + ex.Message, EventLogEntryType.Error);
            }
        }

        private async Task HandleTcpClient(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                {
                    string message;
                    while ((message = await reader.ReadLineAsync()) != null)
                    {
                        _eventLog.WriteEntry($"Received message: {message}", EventLogEntryType.Information);
                        ShowNotification(message); // 🔹 Display the message
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry("TCP Client Handling Error in Tray App: " + ex.Message, EventLogEntryType.Error);
            }
            finally
            {
                client.Close(); // 🟢 Ensure the connection is closed
            }
        }


        private async void ShowNotification(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() => ShowNotification(message)));
            }
            else
            {
                if (!this.IsHandleCreated) // Ensure the handle exists
                {
                    this.CreateHandle();
                }
                //string uniqueTag = Guid.NewGuid().ToString();
                //new ToastContentBuilder()
                //    .AddText("Folder Monitor Alert")
                //    .AddText(message)
                //    .AddToastActivationInfo(uniqueTag, ToastActivationType.Foreground) // 🔹 Unique ID
                //    .Show();

                await Task.Delay(500);

                trayIcon.ShowBalloonTip(5000, "Folder Monitor Alert", message, ToolTipIcon.Info);

                //Form popup = new Form
                //{
                //    Width = 300,
                //    Height = 100,
                //    StartPosition = FormStartPosition.CenterScreen,
                //    TopMost = true,
                //    FormBorderStyle = FormBorderStyle.FixedDialog
                //};

                //Label lblMessage = new Label
                //{
                //    Text = message,
                //    Dock = DockStyle.Fill,
                //    TextAlign = ContentAlignment.MiddleCenter
                //};

                //popup.Controls.Add(lblMessage);
                //popup.Show();

                _eventLog.WriteEntry($"Notification sent: {message}", EventLogEntryType.Information);
            }
        }

        private void ExitApp(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            _isListening = false;
            trayIcon.Dispose();
            //StopService();
            Application.Exit();
        }

        //private void ShowApp(object sender , EventArgs e)
        //{
        //    this.Show();
        //}
        private void HideApp()
        {
            this.Hide();
        }

        private void RegisterAppToStartup()
        {
            try
            {
                string appName = "TrayApp";
                string appPath = Application.ExecutablePath;

                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (registryKey.GetValue(appName) == null)
                    {
                        registryKey.SetValue(appName, $"\"{appPath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry("Failed to register startup: " + ex.Message, EventLogEntryType.Error);
            }
        }
        private void SetupEventLog()
        {
            _eventLog = new EventLog();
            try
            {
                if (!EventLog.SourceExists("Tray App"))
                {
                    EventLog.CreateEventSource("Tray App", "Application");
                }
                _eventLog.Source = "Tray App";
                _eventLog.Log = "Application"; // Use "Application" log instead of creating a new log.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing Event Log: " + ex.Message);
            }
        }
        private void SetupNotifyIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = Icon,
                Visible = true,
                Text = "Folder Monitor"
            };

            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApp);
            //trayIcon.ContextMenuStrip.Items.Add("Show", null, ShowApp);
        }

        private void TrayApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitApp(sender , e);
        }

        //private void StartService()
        //{
        //    try
        //    {
        //        ServiceController sc = new ServiceController(ServiceName);
        //        if(sc.Status != ServiceControllerStatus.Running)
        //        {
        //            sc.Start();
        //            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
        //            _eventLog.WriteEntry("Service started successfully." , EventLogEntryType.Information);
        //        }
        //        else
        //        {
        //            _eventLog.WriteEntry("Service already running", EventLogEntryType.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventLog.WriteEntry("Starting Service Error" + ex.Message, EventLogEntryType.Error);
        //    }
        //}
        //private void StopService()
        //{
        //    try
        //    {
        //        ServiceController sc = new ServiceController(ServiceName);
        //        if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
        //        {
        //            sc.Stop();
        //            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
        //            _eventLog.WriteEntry("Service Stoped successfully.", EventLogEntryType.Information);
        //        }
        //        else
        //        {
        //            _eventLog.WriteEntry("Service already Stoped", EventLogEntryType.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventLog.WriteEntry("Stopping Service Error" + ex.Message, EventLogEntryType.Error);
        //    }
        //}
    }
}
