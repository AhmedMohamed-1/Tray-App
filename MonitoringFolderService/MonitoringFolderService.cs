using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MonitoringFolderService
{
    public partial class MonitoringFolderService : ServiceBase
    {
        private FileSystemWatcher _watcher;
        private EventLog _eventLog;
        private Task _pipeServerTask;
        //private CancellationTokenSource _cts;
        private string sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
        private string logFolder = ConfigurationManager.AppSettings["LogFolder"];

        public MonitoringFolderService()
        {
            InitializeComponent();
            EnsureEventLogSource();
            EnsureDirectoriesExist();


            _watcher = new FileSystemWatcher(sourceFolder, "*.*")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;

            //_cts = new CancellationTokenSource();
            //_pipeServerTask = Task.Run(() => StartPipeServer(_cts.Token));
        }

        private void EnsureEventLogSource()
        {
            string sourceName = "FolderMonitoringService";
            string logName = "Application";

            if (!EventLog.SourceExists(sourceName))
            {
                EventLog.CreateEventSource(sourceName, logName);
            }

            _eventLog = new EventLog { Source = sourceName, Log = logName };
            Console.WriteLine("We are here also");
        }
        private void EnsureDirectoriesExist()
        {
            Console.WriteLine($"Source folder: {sourceFolder}");
            Console.WriteLine($"Log folder: {logFolder}");
            _eventLog.WriteEntry($"Source folder: {sourceFolder}", EventLogEntryType.Information);
            _eventLog.WriteEntry($"Log folder: {logFolder}", EventLogEntryType.Information);
            if (string.IsNullOrEmpty(sourceFolder))
            {
                _eventLog.WriteEntry("Source folder path is not configured or is empty.", EventLogEntryType.Error);
                throw new ArgumentNullException("Source folder path is not configured or is empty.");
            }

            if (string.IsNullOrEmpty(logFolder))
            {
                _eventLog.WriteEntry("Log folder path is not configured or is empty.", EventLogEntryType.Error);
                throw new ArgumentNullException("Log folder path is not configured or is empty.");
            }

            // Create directories if they don't exist
            if (!Directory.Exists(@"E:\FileMonitoring\Source"))
            {
                Directory.CreateDirectory(@"E:\FileMonitoring\Source");
            }

            if (!Directory.Exists(@"E:\FileMonitoring\Logs"))
            {
                Directory.CreateDirectory(@"E:\FileMonitoring\Logs");
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Task.Run(async () =>
            {
                string fileName = Path.GetFileName(e.FullPath);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SendMessageToWinForms($"{fileName} Added at {timestamp}");
                LogMessage($"[ADDED] File: {fileName} at {timestamp} {Environment.NewLine}");
            });
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            Task.Run(async () =>
            {
                string fileName = Path.GetFileName(e.FullPath);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await SendMessageToWinForms($"{fileName} Removed at {timestamp}");
                LogMessage($"[REMOVED] File: {fileName} at {timestamp} {Environment.NewLine}");
            });
        }


        public void LogMessage(string message)
        {
            //string _LogMessage = $"[{DateTime.Now: yyyy-MM-dd HH:mm:ss} {message}]";
            string logFile = Path.Combine(ConfigurationManager.AppSettings["LogFolder"], "log.txt");
            File.AppendAllText(logFile, message);
        }

        private async Task SendMessageToWinForms(string message)
        {
            int retries = 3;

            while (retries-- > 0)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync("127.0.0.1", 5000); // Connect to Tray App (TCP Server)

                        using (var writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
                        {
                            await writer.WriteLineAsync(message);
                            _eventLog.WriteEntry("Message sent to Tray App: " + message, EventLogEntryType.Information);
                        }
                    }
                    return; // Exit if successful
                }
                catch (SocketException)
                {
                    _eventLog.WriteEntry("TCP connection failed: Tray App not running. Retrying...", EventLogEntryType.Warning);
                }
                catch (Exception ex)
                {
                    _eventLog.WriteEntry("Unexpected TCP error: " + ex.Message, EventLogEntryType.Error);
                }

                await Task.Delay(1000); // Wait before retrying
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _watcher.EnableRaisingEvents = true;
                Console.WriteLine("$\"Service Started {DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
                _eventLog.WriteEntry($"Service Started {DateTime.Now:yyyy-MM-dd HH:mm:ss}", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine("$\"Service failed {DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
                _eventLog.WriteEntry($"Service failed to start: {ex.Message} {Environment.NewLine} {ex.StackTrace}", EventLogEntryType.Error);
            }
        }


        protected override void OnStop()
        {
            //_cts.Cancel();
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _eventLog.WriteEntry($"Service Stopped {DateTime.Now:yyyy-MM-dd HH:mm:ss}", EventLogEntryType.Information);
        }

        public void RunInConsole()
        {
            Console.WriteLine("We Begin");
            OnStart(null);
            Console.ReadLine();
        }

    }
}
