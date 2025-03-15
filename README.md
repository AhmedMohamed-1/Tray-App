# **TrayApp & MonitoringFolderService**

## 📌 **Overview**  
**TrayApp** is a lightweight system tray application designed to work alongside **MonitoringFolderService**, a Windows Service for automated file monitoring and logging. The service runs in the background, ensuring persistent and reliable operation without user interaction.  

You can also use **MonitoringFolderService** as a standalone service without **TrayApp**. It will function independently, logging all monitored events to the log folder specified in `App.config`. However, without the app, you will not receive real-time notifications.

---

## ✨ **Features**
- 📂 **Automated Folder Monitoring** – Tracks folder changes in real-time, detecting additions and deletions.
- 📜 **Event Logging** – Logs system events directly to a file.
- 🖥 **Runs in Background** – Operates silently without requiring user interaction.
- 🔄 **Auto-Start** – The service starts automatically with Windows.
- ⚡ **Lightweight** – Minimal resource usage for efficient performance.

---

## 🛠 **Technologies Used**
- **C# (.NET Framework)** – Core development language.
- **Windows Services** – Background execution and automation.
- **System.IO** – File system monitoring.
- **Event Logging** – Capturing system activity.

---

## ⚠️ Important Note

Before running the service, you **must** update the monitored folder path in the `App.config` file inside the service folder. The default paths are:

- **Monitored Folder:** `E:\FolderMonitoring\Source`
- **Logs Folder:** `E:\FolderMonitoring\Logs`

If your system does not have an `E:` drive, the service will fail to start. Modify these paths according to your system configuration before proceeding.

---

## 🚀 **Installation Guide**
### **1️⃣ Installing TrayApp (System Tray Application)**
1. **Download & Extract** the project folder.
2. Navigate to:  
   `MonitoringFolderService/bin/Debug/`
3. Locate **TrayApp.exe**
4. **Run TrayApp.exe** manually for the first time.
5. The TrayApp will now **start automatically** every time you turn on your PC.

---

### **2️⃣ Installing MonitoringFolderService (Windows Service)**
#### 📌 **Method 1: Using SC Command**
1. **Download & Extract** the service folder.
2. Navigate to:  
   `MonitoringFolderService/bin/Debug/`
3. Locate **MonitoringFolderService.exe**
4. Open **Command Prompt as Administrator** (Press `Win + R`, type `cmd`, and press `Ctrl + Shift + Enter`).
5. Run the following command to install the service:
   ```sh
   sc create MonitoringFolderService binPath= "C:\Path\To\MonitoringFolderService.exe" start= auto
   ```
   *(Replace `C:\Path\To\` with the actual directory path.)*
6. Start the service manually for the first time:
   ```sh
   sc start MonitoringFolderService
   ```
7. The service will now **start automatically** every time you turn on your PC.

#### 📌 **Method 2: Using InstallUtil**
1. **Open Command Prompt as Administrator**.
2. Navigate to the .NET Framework directory:
   ```sh
   cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319
   ```
3. Run the InstallUtil command to install the service:
   ```sh
   InstallUtil "C:\Path\To\MonitoringFolderService.exe"
   ```
4. To uninstall the service:
   ```sh
   InstallUtil /u "C:\Path\To\MonitoringFolderService.exe"
   ```

---

## 🔧 **Uninstalling the Service**
1. Open **Command Prompt as Administrator**.
2. Stop the service:
   ```sh
   sc stop MonitoringFolderService
   ```
3. Delete the service:
   ```sh
   sc delete MonitoringFolderService
   ```
4. Manually delete the **MonitoringFolderService.exe** file if needed.

---

## 🛠 **Managing the Service**
- To **restart** the service:
  ```sh
  sc stop MonitoringFolderService && sc start MonitoringFolderService
  ```
- To check service status:
  ```sh
  sc query MonitoringFolderService
  ```
- To manually start the service:
  ```sh
  sc start MonitoringFolderService
  ```

---

## 🤝 **How to Contribute**
We welcome contributions! To get started:
1. **Fork the Repository** – Clone your own copy.
2. **Create a Branch** – Work on a feature or bugfix.
3. **Commit Changes** – Ensure clear commit messages.
4. **Push to GitHub** – Submit a pull request.
5. **Review & Merge** – Your contribution will be reviewed and merged.

---

## 💡 **Notes**
- Ensure you have **Administrator Privileges** when installing or managing the service.
- If you encounter issues, check the **Windows Event Viewer** for logs.
- The service runs **silently** in the background and logs file activities into a designated log file.

---

📌 **Now you're all set!** 🚀 Enjoy seamless, automated monitoring with **TrayApp** & **MonitoringFolderService**!

