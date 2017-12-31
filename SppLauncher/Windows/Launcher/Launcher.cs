﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using MySql.Data.MySqlClient;
using MySQLClass;
using SppLauncher.Class;
using SppLauncher.Properties;
using WowAccountCreator;

namespace SppLauncher.Windows.Launcher
{
    public partial class Launcher : Form
    {
        #region [ Variable ]
        private static double _remoteEmuVer;
        private const string LwPath = "Server\\Bin\\";
        private const string Sqlpath = "Server\\Database\\bin\\";
        private readonly string _getTemp = Path.GetTempPath();
        private static Process _cmd, _cmd1, _cmd3;
        private DateTime _dt;
        private DateTime _start1 = DateTime.Now;
        private readonly RunMysql run;
        private readonly PerformanceCounter _cpuCounter, _ramCounter;
        private bool _startStop, _allowtext, _restart, _updateYes, _serverConsoleActive, _sysProtect, _worldstarted, _sayno, _Forcerestart;
        private readonly XmlReadWrite _xmlReadWrite;
        private static string _olDrealmList, _updLink, _realmDialogPath, _exportfile, _ipAdress, _importFile;
        private static bool _sqlimport, _sqlexport, _updater;
        private string _realm, _mangosdMem, _realmdMem, _sqlMem, _world, _sql, _sqlStartTime, _realmStartTime, _worldStartTime;
        public static string AutoS , resetBots, RandomizeBots, RealmListPath, Lang, UpdateUnpack, Status, WowExePath;
        public static string SysProt = "1";
        public static bool OnlyMysqlStart, MysqlOn, Dbupdate;
        public static double CurrEmuVer;
        public const string CurrProgVer = "1.2.0"; //? Current program version.

        #endregion

        #region [ Initialize ]

        public Launcher()
        {
            _xmlReadWrite = new XmlReadWrite();
            if(_xmlReadWrite.ReadXML()){Checklang(false);}
            var exePath = AppDomain.CurrentDomain.FriendlyName;
            File.SetAttributes(exePath, FileAttributes.Normal);
            InitializeComponent();
            run = new RunMysql();
            _cpuCounter = new PerformanceCounter();
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName  = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";
            _ramCounter = new PerformanceCounter();
            _ramCounter.CategoryName = "Memory";
            _ramCounter.CounterName = "Available MBytes";
            if (SysProt == "1")
            {
                systemProtectToolStripMenuItem.Checked = true;
            }
            else
            {
                systemProtectToolStripMenuItem.Checked = false;  
            }
            SppTray.Visible = true;
            startWowToolStripMenuItem.ToolTipText = Resources.Launcher_Launcher_Auto_Changed_RealmList_wtf_and_after_exit_SPP_change_back_original_realmlist_;
        }

        private void Launcher_Shown(object sender, EventArgs e)
        {
            InitMethods();
        }

        #endregion

        #region [ Servers ]

        private void bwCloseProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            CloseProcess(false);
        }

        private void bwCloseProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusIcon();
            StatusChage(Resources.Launcher_startNStop_Derver_is_down, false);
            AnimateStatus(0);
            
        }

        private void bwCloseProcess1_DoWork(object sender, DoWorkEventArgs e)
        {
            CloseProcess(true);
        }

        private void bwCloseProcess1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusIcon();
            WindowState = FormWindowState.Normal;
            Show();
            StartAll();
        }

        private void bwRunExport_DoWork(object sender, DoWorkEventArgs e)
        {
            CloseProcess(true);
        }

        private void bwRunExport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GetSqlOnlineBot.Stop();
            tssLOnline.Text = Resources.Launcher_import_Online_Bots__N_A;
            StatusIcon();
            WindowState = FormWindowState.Normal;
            Show();
            pbAvailableM.Visible = true;
            StatusChage(Resources.Launcher_import_Decompress, false);
            AnimateStatus(1);
            bwImport.RunWorkerAsync();
        }

        private void SaveAnnounce()
        {
            try
            {
                _cmd1.StandardInput.WriteLine("saveall");
                _cmd1.StandardInput.WriteLine(".announce Saving...");
            }
            catch (Exception)
            {
            }
        }

        private void bwCloseSPP_DoWork(object sender, DoWorkEventArgs e)
        {
            CloseProcess(false);
        }

        private void bwCloseSPP_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Hide();
            SppTray.Visible = false;
            Loader.Kill = true;
        }

        private void SetText(string text)
        {
            _realm += text;
        }

        public float GetCurrentCpuUsage()
        {
            try
            {
                return Convert.ToInt32(_cpuCounter.NextValue());
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int GetAvailableRAM()
        {
            try
            {
                return Convert.ToInt32(_ramCounter.NextValue());
            }
            catch (Exception)
            {
                return -1;
            }

        }

        private void SetText3(string text1)
        {
            if (richTextBox1.InvokeRequired)
            {
                SetTextCallback d = SetText3;
                Invoke(d, new object[] {text1});
            }
            else
            {
                _sql              += text1;
                richTextBox1.Text += text1 + Environment.NewLine;
                File.WriteAllText("logs\\mysql.txt", richTextBox1.Text);
            }
        }

        private void SetText1(string text)
        {
            if (rtWorldDev.InvokeRequired)
            {
                SetTextCallback d = SetText1;
                Invoke(d, new object[] {text});
            }
            else
            {
                _world += text;

                if (_allowtext)
                {
                    if (!text.Contains("SPELL") &&
                        !text.Contains("wrong DBC files") &&
                        !text.Contains("incorrect race/class") &&
                        !text.Contains("SD2 ERROR") &&
                        !text.Contains("ERROR:Player") &&
                        !text.Contains("DoScriptText") &&
                        !text.Contains("DB-SCRIPTS") &&
                        !text.Contains("removeSpell") &&
                        !text.Contains("ACTION_T_CAST") &&
                        !text.Contains("SD2") &&
                        !text.Contains("TriggerSpell") &&
                        !text.Contains("ACTION_T_SUMMON") &&
                        !text.Contains("INSERT INTO") &&
                        !text.Contains("SQL ERROR") &&
                        !text.Contains("LoadInventory") &&
                        !text.Contains("MoveSpline") &&
                        !text.Contains("unknown spell id") &&
                        !text.Contains("MapManager"))
                    {
                        _dt = DateTime.Now;

                        if (rtWorldDev.Text      != "") rtWorldDev.Text += Environment.NewLine;
                        rtWorldDev.Text          += _dt.ToString("[" + "HH:mm" + "]: ") + text;
                        rtWorldDev.SelectionStart = rtWorldDev.Text.Length;
                        rtWorldDev.ScrollToCaret();
                    }
                }
            }
        }

        public void WorldStart()
        {
            _start1             = DateTime.Now;
            pbarWorld.Visible   = true;
            pbTempW.Visible     = true;
            pbNotAvailW.Visible = false;
            StatusChage(Resources.Launcher_WorldStart_Loading_World, false);
            WindowSize(false);
            tmrWorld.Start();
            var cmdStartInfo                    = new ProcessStartInfo(LwPath + "worldserver.exe");
            cmdStartInfo.CreateNoWindow         = true;
            cmdStartInfo.RedirectStandardInput  = true;
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError  = true;
            cmdStartInfo.UseShellExecute        = false;
            cmdStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;

            _cmd1 = new Process();
            _cmd1.StartInfo = cmdStartInfo;


            if (_cmd1.Start())
            {
                _cmd1.OutputDataReceived += _cmd_OutputDataReceived1;
                _cmd1.ErrorDataReceived  += _cmd_ErrorDataReceived1;
                _cmd1.Exited             += _cmd_Exited1;
                _cmd1.BeginOutputReadLine();
                _cmd1.BeginErrorReadLine();
            }
            else
            {
                _cmd1 = null;
            }
        }


        internal void RealmdStart()
        {
            _start1             = DateTime.Now;
            StatusChage(Resources.Launcher_RealmdStart_Starting_Realm, false);
            tssStatus.Image     = Resources.search_animation;
            pbTempR.Visible     = true;
            pbNotAvailR.Visible = false;
            tmrRealm.Start();
            var cmdStartInfo                    = new ProcessStartInfo(LwPath + "bnetserver.exe");
            cmdStartInfo.CreateNoWindow         = true;
            cmdStartInfo.RedirectStandardInput  = true;
            cmdStartInfo.RedirectStandardOutput = true;
            cmdStartInfo.RedirectStandardError  = true;
            cmdStartInfo.UseShellExecute        = false;
            cmdStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
            _cmd                                = new Process();
            _cmd.StartInfo                      = cmdStartInfo;

            if (_cmd.Start())
            {
                _cmd.OutputDataReceived += _cmd_OutputDataReceived;
                _cmd.ErrorDataReceived  += _cmd_ErrorDataReceived;
                _cmd.Exited             += _cmd_Exited;
                _cmd.BeginOutputReadLine();
                _cmd.BeginErrorReadLine();
            }
            else
            {
                _cmd = null;
            }
        }

        private void _cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole(e.Data);
        }

        private void _cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole(e.Data, Brushes.Red);
        }

        private void _cmd_Exited(object sender, EventArgs e)
        {
            _cmd.OutputDataReceived -= _cmd_OutputDataReceived;
            _cmd.Exited -= _cmd_Exited;
        }

        private void UpdateConsole(string text)
        {
            UpdateConsole(text, null);
        }

        private void UpdateConsole(string text, Brush color)
        {
            WriteLine(text);
        }

        private void WriteLine(string text)
        {
            if (text != null)
            {
                SetText(text);
            }
        }

        private void _cmd_OutputDataReceived1(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole1(e.Data);
        }

        private void _cmd_ErrorDataReceived1(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole1(e.Data, Brushes.Red);
        }

        private void _cmd_Exited1(object sender, EventArgs e)
        {
            _cmd1.OutputDataReceived -= _cmd_OutputDataReceived1;
            _cmd1.Exited             -= _cmd_Exited1;
        }

        private void UpdateConsole1(string text)
        {
            UpdateConsole1(text, null);
        }

        private void UpdateConsole1(string text, Brush color)
        {
            WriteLine1(text);
        }

        private void WriteLine1(string text)
        {
            if (text != null)
            {
                SetText1(text);
            }
        }

        private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void NotifyBallon(int timeout, string title, string msg, bool crashed)
        {
            if (crashed)
                SppTray.ShowBalloonTip(timeout, title, msg, ToolTipIcon.Warning);
            else
                SppTray.ShowBalloonTip(timeout, title, msg, ToolTipIcon.Info);
        }

        public void StartAll()
        {
            _start1 = DateTime.Now;
            if (!_restart)
            {
                StatusChage(Resources.Launcher_StartAll_Starting_Mysqlm, false);
                AnimateStatus(1);
                pbTempM.Visible     = true;
                pbNotAvailM.Visible = false;
                SqlStartCheck.Start();
                var cmdStartInfo = new ProcessStartInfo("cmd.exe",
                    "/C " + Sqlpath + "mysqld.exe" +
                    " --defaults-file=" + Sqlpath + "my.ini" +
                    " --standalone --console");
                cmdStartInfo.CreateNoWindow         = true;
                cmdStartInfo.RedirectStandardInput  = true;
                cmdStartInfo.RedirectStandardOutput = true;
                cmdStartInfo.RedirectStandardError  = true;
                cmdStartInfo.UseShellExecute        = false;
                cmdStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;

                _cmd3 = new Process();
                try
                {
                    _cmd3.StartInfo = cmdStartInfo;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Some exception: mysql\n" + ex.Message);
                }

                if (_cmd3.Start())
                {
                    _cmd3.OutputDataReceived += _cmd_OutputDataReceived3;
                    _cmd3.ErrorDataReceived  += _cmd_ErrorDataReceived3;
                    _cmd3.Exited             += _cmd_Exited3;
                    _cmd3.BeginOutputReadLine();
                    _cmd3.BeginErrorReadLine();
                }
                else
                {
                    _cmd3 = null;
                }
            }
            else
            {
                pbAvailableM.Visible = true;
                RealmdStart();
            }
        }

        private void _cmd_OutputDataReceived3(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole3(e.Data);
        }
       
        private void _cmd_ErrorDataReceived3(object sender, DataReceivedEventArgs e)
        {
            UpdateConsole3(e.Data, Brushes.Red);
        }

        private void _cmd_Exited3(object sender, EventArgs e)
        {
            _cmd3.OutputDataReceived -= _cmd_OutputDataReceived3;
            _cmd3.Exited -= _cmd_Exited3;
        }

        private void UpdateConsole3(string text)
        {
            UpdateConsole3(text, null);
        }

        private void UpdateConsole3(string text, Brush color)
        {
            WriteLine3(text);
        }

        private void WriteLine3(string text)
        {
            if (text != null)
            {
                SetText3(text);
            }
        }

        private delegate void SetTextCallback(string text);

        private void Check_Tick(object sender, EventArgs e)
        {
            if (!CheckRunwow())
            {
                Check.Stop();
                RealmRestore();
            }
        }

        private void rstchck_DoWork(object sender, DoWorkEventArgs e)
        {
            Checklang(false);
            StatusChage(Resources.Launcher_rstchck_DoWork_Reset_bots, false);
            tssStatus.Image = Resources.search_animation;
            Thread.Sleep(10000);
        }

        private void rstchck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MenuItemsDisableAfterLoad();
            RealmWorldRestart();
        }

        #endregion

        #region [ ProcessTasks ]

        public void CloseProcess(bool restart)
        {
            try
            {
                Checklang(false);
                if (_serverConsoleActive) sendCommandForServerToolStripMenuItem_Click(new object(), new EventArgs());

                foreach (var proc in Process.GetProcessesByName("worldserver"))
                {
                    StatusChage(Resources.CloseProcess_Saving,false);
                    AnimateStatus(1);

                    if (_worldstarted)
                    {
                        int i = 0;
                        Back:
                        switch (_world.Contains("players"))
                        {
                            case true:
                                Thread.Sleep(4000);
                                break;
                            case false:
                                Thread.Sleep(2000);
                                i++;
                                if (i <= 22)
                                {
                                    goto Back;
                                }
                                AnimateStatus(2);
                                StatusChage("Unable to save", false);
                                Thread.Sleep(3000);
                                AnimateStatus(0);
                                break;
                        }
                        _worldstarted = false;
                        AnimateStatus(1);
                        StatusChage(Resources.Launcher_CloseProcess_World_Shutdown, false);
                        //_cmd1.StandardInput.WriteLine("server shutdown 0");
                        Thread.Sleep(1000);
                    }
                    AnimateStatus(1);
                    proc.Kill();
                }

                foreach (var proc in Process.GetProcessesByName("bnetserver"))
                {
                    StatusChage(Resources.Launcher_CloseProcess_Login_Shutdown, false);
                    AnimateStatus(1);
                    proc.Kill();
                }
                AnimateStatus(0);
                foreach (var proc in Process.GetProcessesByName("mysqld"))
                {
                    AnimateStatus(1);
                    if (!restart)
                        ShutdownSql();

                    AnimateStatus(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.Launcher_CloseProcess_ +
                                ex.Message, Resources.Launcher_SearchProcess_Warning, MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void SearchProcess()
        {
            try
            {

                foreach (var proc in Process.GetProcessesByName("worldserver"))
                {
                
                    var result =
                        MessageBox.Show(Resources.Launcher_SearchProcess_, Resources.Launcher_SearchProcess_Warning,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        proc.Kill();
                    }
                    else
                    {
                        Loader.Kill = true;
                    }
                }

                Thread.Sleep(100);

                foreach (var proc in Process.GetProcessesByName("bnetserver"))
                {
                    var result =
                        MessageBox.Show(Resources.Launcher_CloseProcess_Login_Shutdown,
                            Resources.Launcher_SearchProcess_Warning,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        proc.Kill();
                    }
                    else
                    {
                        Loader.Kill = true;
                    }
                }

                if(UnitTestDetector.IsInUnitTest)
                    goto isUnitTest;

                foreach (var proc in Process.GetProcessesByName("mysqld"))
                {
                    var result =
                        MessageBox.Show(Resources.Launcher_SearchProcess2_, Resources.Launcher_SearchProcess_Warning,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        proc.Kill();
                    }
                    else
                    {
                        Loader.Kill = true;
                    }
                }

                isUnitTest:;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.Launcher_SearchProcess0_ +
                                ex.Message, Resources.Launcher_SearchProcess_Warning, MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }


        }

        #endregion

        #region [ OtherMethods ]

        private void InitMethods()
        {
            Checklang(true);
            GetLocalSrvVer();
            tmrUsage.Start();
            StatusBarUpdater.Start();
            MenuItemsDisableAfterLoad();
            WasThisUpdate();
            rtWorldDev.Visible = false;
            WindowSize(true);
            font();
            SearchProcess();
            StatusIcon();
            AutoStart();
        }

        private void AutoStart()
        {
            if (AutoS == "1")
            {
                startstopToolStripMenuItem.Image = Resources.Button_stop_icon;
                startToolStripMenuItem.Image = Resources.Button_stop_icon;
                startstopToolStripMenuItem.Text = Resources.Launcher_startNStop_Stop;
                startToolStripMenuItem.Text = Resources.Launcher_startNStop_Stop;
                startstopToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = false;
                _startStop = true;
                autostartToolStripMenuItem.Checked = true;
                autorunToolStripMenuItem.Checked = true;
                exportImportCharactersToolStripMenuItem.Enabled = false;
                StartAll();
            }
            else
            {
                bwUpdate.RunWorkerAsync();
                exportImportCharactersToolStripMenuItem.Enabled = false;
                startstopToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
                restartToolStripMenuItem1.Enabled = false;
                restartToolStripMenuItem2.Enabled = false;
                sendCommandForServerToolStripMenuItem.Enabled = false;
            }
        }

        public void StatusChage(string msg, bool islink)
        {
            Status = msg;
            tssStatus.IsLink = islink;
        }

        public void Checklang(bool option)
        {
            if (option)
                switch (Lang)
                {
                    case "Hungarian":
                        magyarToolStripMenuItem.Checked = true;
                        break;
                    case "English":
                        englishToolStripMenuItem.Checked = true;
                        break;
                    case "German":
                        germanToolStripMenuItem.Checked = true;
                        break;
                    case "French":
                        frenchToolStripMenuItem.Checked = true;
                        break;
                }
            else
            {
                switch (Lang)
                {
                    case "Hungarian":
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("hu-HU");
                        Thread.CurrentThread.CurrentCulture   = CultureInfo.CreateSpecificCulture("hu-HU");
                        break;
                    case "English":
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                        Thread.CurrentThread.CurrentCulture   = CultureInfo.InvariantCulture;
                        break;
                    case "German":
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de");
                        Thread.CurrentThread.CurrentCulture   = CultureInfo.CreateSpecificCulture("de");
                        break;
                    case "French":
                        Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("fr");
                        Thread.CurrentThread.CurrentCulture   = CultureInfo.CreateSpecificCulture("fr");
                        break;
                }
            }
        }

        private void AnimateStatus(int icon)
        {
            switch (icon)
            {
                case 0:
                    tssStatus.Image = null;
                    break;
                case 1:
                    tssStatus.Image = Resources.search_animation;
                    break;
                case 2:
                    tssStatus.Image = Resources.Warning_icon_mini;
                    break;
                case 3:
                    tssStatus.Image = Resources.check_icon;
                    break;
                case 4:
                    tssStatus.Image = Resources.info_icon_mini;
                    break;
            }
        }

        public void StartNStop()
        {
            if (!_startStop)
            {
                startstopToolStripMenuItem.Enabled            = false;
                startToolStripMenuItem.Enabled                = false;
                restartToolStripMenuItem1.Enabled             = false;
                restartToolStripMenuItem2.Enabled             = false;
                sendCommandForServerToolStripMenuItem.Enabled = false;
                StartAll();
                _startStop                       = true;
                startstopToolStripMenuItem.Text  = Resources.Launcher_startNStop_Stop;
                startToolStripMenuItem.Text      = Resources.Launcher_startNStop_Stop;
                startstopToolStripMenuItem.Image = Resources.Button_stop_icon;
                startToolStripMenuItem.Image     = Resources.Button_stop_icon;
            }
            else
            {
                resetAllRandomBotsToolStripMenuItem.Enabled     = false;
                randomizeBotsToolStripMenuItem.Enabled          = false;
                resetBotsToolStripMenuItem.Enabled              = false;
                randomizeBotsToolStripMenuItem1.Enabled         = false;
                restartToolStripMenuItem1.Enabled               = false;
                restartToolStripMenuItem2.Enabled               = false;
                sendCommandForServerToolStripMenuItem.Enabled   = false;
                exportImportCharactersToolStripMenuItem.Enabled = false;
                startstopToolStripMenuItem.Image                = Resources.Play_1_Hot_icon;
                startToolStripMenuItem.Image                    = Resources.Play_1_Hot_icon;
                startstopToolStripMenuItem.Text                 = Resources.Launcher_startNStop_Start;
                startToolStripMenuItem.Text                     = Resources.Launcher_startNStop_Start;
                _startStop                                      = false;
                CheckMangosCrashed.Stop();
                GetSqlOnlineBot.Stop();
                tssLOnline.Text = Resources.Launcher_startNStop_Online_bot__N_A;
                SaveAnnounce();
                rtWorldDev.Text = "";
                bwStopWorld.RunWorkerAsync();
            }
        }

        public int Getmemory()
        {
            return Convert.ToInt32(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory/1024/1024);
        }

        public void MenuItemsDisableAfterLoad()
        {
            resetAllRandomBotsToolStripMenuItem.Enabled        = false;
            exportImportCharactersToolStripMenuItem.Enabled    = false;
            startstopToolStripMenuItem.Enabled                 = false;
            randomizeBotsToolStripMenuItem.Enabled             = false;
            resetBotsToolStripMenuItem.Enabled                 = false;
            randomizeBotsToolStripMenuItem1.Enabled            = false;
            restartToolStripMenuItem1.Enabled                  = false;
            restartToolStripMenuItem2.Enabled                  = false;
            sendCommandForServerToolStripMenuItem.Enabled      = false;
            lanSwitcherToolStripMenuItem1.Enabled              = false;
            lanSwitcherToolStripMenuItem.Enabled               = false;
            accountToolToolStripMenuItem1.Enabled              = false;
        }

        public void StatusIcon()
        {
            if (!_restart)
            {
                pbNotAvailM.Visible = true;
            }
            //red
            pbNotAvailR.Visible = true;
            pbNotAvailW.Visible = true;
            //yellow
            pbTempM.Visible = false;
            pbTempR.Visible = false;
            pbTempW.Visible = false;
            //green
            pbAvailableM.Visible = false;
            pbAvailableR.Visible = false;
            pbAvailableW.Visible = false;
        }

        public void font()
        {
            try
            {
                var pfc = new PrivateFontCollection();
                pfc.AddFontFile("Server\\font\\Wfont.ttf");
                lblMysql.Font = new Font(pfc.Families[0], 26, FontStyle.Regular);
                lblRealm.Font = new Font(pfc.Families[0], 26, FontStyle.Regular);
                lblWorld.Font = new Font(pfc.Families[0], 26, FontStyle.Regular);
            }
            catch (Exception)
            {
            }
        }

        private void WindowSize(bool min)
        {
            if (min)
            {
                groupBox1.Height = 140;
                Height           = 230;
            }
            else
            {
                groupBox1.Height = 174;
                Height           = 265;
            }
        }

        public void Traymsg()
        {
            if (!_restart)
            {
                NotifyBallon(1000, Resources.Launcher_Traymsg_Servers_Started, Resources.Launcher_Traymsg_Ready_to_play,
                    false);
            }
            else
            {
                NotifyBallon(1000, Resources.Launcher_Traymsg_Servers_Started, Resources.Launcher_Traymsg_Ready_to_play,
                    false);
                _restart = false;
            }
            _world = "";
        }

        public void StartWow()
        {
            CheckLanIpInRealmDatabase();

            if (WowExePath == "" || RealmListPath == "")
            {
                DialogMethod();
            }
            else
            {
                RealmChange();
                Process.Start(WowExePath);
                Check.Start();

            }
        }

        public void DialogMethod()
        {
            var dialog = new OpenFileDialog();

            dialog.InitialDirectory = "c:\\";
            dialog.Filter           = "Executable (*wow.exe)|*wow.exe";
            dialog.FilterIndex      = 2;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    WowExePath       = dialog.FileName;
                    _realmDialogPath = Path.GetDirectoryName(WowExePath);
                    RealmDialog();
                }
                catch (Exception)
                {

                }
            }
        }

        public void RealmDialog()
        {
            var dialog = new OpenFileDialog();

            dialog.InitialDirectory = _realmDialogPath;
            dialog.Filter           = "Realmlist File (*realmlist.wtf)|*realmlist.wtf";
            dialog.FilterIndex      = 2;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                RealmListPath = dialog.FileName;
                _xmlReadWrite.saveMethod();
                RealmChange();
                Process.Start(WowExePath);
                Check.Start();
            }
        }

        private void RealmChange()
        {
            try
            {
                CheckLanIpInRealmDatabase();
                _olDrealmList = File.ReadAllText(RealmListPath, Encoding.UTF8);
                NotifyBallon(1000, Resources.Launcher_RealmRestore_Realmlist, "Set " + _ipAdress, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.Launcher_RealmChange_ + ex.Message);
            }

            try
            {
                File.WriteAllText(RealmListPath, "set realmlist " + _ipAdress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.Launcher_RealmChange0_ + ex.Message);
            }
        }

        private void RealmRestore()
        {
            try
            {
                File.WriteAllText(RealmListPath, _olDrealmList);
                NotifyBallon(1000, Resources.Launcher_RealmRestore_Realmlist, Resources.Launcher_RealmRestore_Restored, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.Launcher_RealmRestore_ + ex.Message);
            }
        }

        private void CheckLanIpInRealmDatabase()
        {
            try
            {
                var client = new MySQLClient("127.0.0.1", "realmd", "root", "123456", 3310);
                _ipAdress = client.Select("realmlist", "id='1'")["address"];
            }
            catch (Exception)
            {
            }
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Enter:
                    e.Handled          = true;
                    e.SuppressKeyPress = true;
                    _cmd1.StandardInput.WriteLine(txbWorldDev.Text);
                    txbWorldDev.Text = "";
                    break;
                case Keys.Escape:
                    txbWorldDev.Text = "";
                    break;
            }
        }

        public static void ShutdownSql()
        {
            MysqlOn                   = false;
            var startInfo             = new ProcessStartInfo();
            startInfo.CreateNoWindow  = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName        = "Server\\Database\\bin\\mysqladmin.exe";
            startInfo.Arguments       = "-u root -p123456 --port=3310 shutdown";
            startInfo.WindowStyle     = ProcessWindowStyle.Hidden;

            try
            {
                using (var exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ResetBots()
        {
            _cmd1.StandardInput.WriteLine("rndbot reset");

            var dialog = MessageBox.Show(Resources.Launcher_ResetBots_, "Question",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                rstchck.RunWorkerAsync();
            }
        }

        public void RandomizeBotsMethod()
        {
            _cmd1.StandardInput.WriteLine("rndbot update");
            MessageBox.Show(Resources.Launcher_RandomizeBotsMethod_);
        }

        public bool CheckRunwow()
        {
            foreach (var proc in Process.GetProcessesByName("Wow"))
            {
                return true;
            }
            return false;
        }

        public bool ProcessView()
        {
            foreach (var proc in Process.GetProcessesByName("worldserver"))
            {
                return false;
            }
            return true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (rtWorldDev.Lines.Length > 250)
            {
                DeleteLine(1);
            }
        }

        private void DeleteLine(int aLine)
        {
            var startIndex = rtWorldDev.GetFirstCharIndexFromLine(aLine);
            var count      = rtWorldDev.Lines[aLine].Length;

            if (aLine < rtWorldDev.Lines.Length - 1)
            {
                count += rtWorldDev.GetFirstCharIndexFromLine(aLine + 1) -
                         ((startIndex + count - 1) + 1);
            }

            rtWorldDev.Text = rtWorldDev.Text.Remove(startIndex, count);
        }

        private void StatusBarUpdater_Tick(object sender, EventArgs e)
        {
            tssStatus.Text = Status;
        }

        public bool GetLocalSrvVer()
        {
            try
            {
                CurrEmuVer = Convert.ToDouble(File.ReadAllText("Server\\version"));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region [ WindowMenuItems ]


        private void sendCommandForServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!sendCommandForServerToolStripMenuItem.Checked)
            {
                sendCommandForServerToolStripMenuItem.Checked = true;
                _serverConsoleActive                          = true;
                groupBox2.Visible                             = true;
                Height                                        = 420;
                rtWorldDev.Visible                            = true;
            }
            else
            {
                sendCommandForServerToolStripMenuItem.Checked = false;
                _serverConsoleActive                          = false;
                groupBox2.Visible                             = false;
                Height                                        = 230;
                rtWorldDev.Visible                            = false;
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateInFile();
        }

     

        private void magyarToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Lang = "Hungarian";
            _xmlReadWrite.saveMethod();
            MessageBox.Show(
                Resources.Launcher_englishToolStripMenuItem_Click_Changes_will_take_effect_when_you_restart_Launcher_,
                Resources.Launcher_magyarToolStripMenuItem_Click_1_Info,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lang = "English";
            _xmlReadWrite.saveMethod();
            MessageBox.Show(
                Resources.Launcher_englishToolStripMenuItem_Click_Changes_will_take_effect_when_you_restart_Launcher_,
                Resources.Launcher_magyarToolStripMenuItem_Click_1_Info,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lang = "French";
            _xmlReadWrite.saveMethod();
            MessageBox.Show(
                Resources.Launcher_englishToolStripMenuItem_Click_Changes_will_take_effect_when_you_restart_Launcher_,
                Resources.Launcher_magyarToolStripMenuItem_Click_1_Info,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void germanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lang = "German";
            _xmlReadWrite.saveMethod();
            MessageBox.Show(
                Resources.Launcher_englishToolStripMenuItem_Click_Changes_will_take_effect_when_you_restart_Launcher_,
                Resources.Launcher_magyarToolStripMenuItem_Click_1_Info,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exportToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void importToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Import();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _allowtext = false;
            StartNStop();
        }

        private void restartToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MenuItemsDisableAfterLoad();
            RealmWorldRestart();
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Import();
        }


        private void restartToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            rtWorldDev.Text = "";
            MenuItemsDisableAfterLoad();
            RealmWorldRestart();
        }

        private void autorunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (autorunToolStripMenuItem.Checked)
            {
                autostartToolStripMenuItem.Checked = false;
                autorunToolStripMenuItem.Checked   = false;
                AutoS                              = "0";
                _xmlReadWrite.saveMethod();
            }
            else
            {
                autostartToolStripMenuItem.Checked = true;
                autorunToolStripMenuItem.Checked   = true;
                AutoS                              = "1";
                _xmlReadWrite.saveMethod();
            }
        }

        private void autostartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (autostartToolStripMenuItem.Checked)
            {
                autostartToolStripMenuItem.Checked = false;
                autorunToolStripMenuItem.Checked   = false;
                AutoS                              = "0";
                _xmlReadWrite.saveMethod();
            }
            else
            {
                autostartToolStripMenuItem.Checked = true;
                autorunToolStripMenuItem.Checked   = true;
                AutoS                              = "1";
                _xmlReadWrite.saveMethod();
            }
        }

        private void startstopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _allowtext = false;
            _world     = "";
            StartNStop();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            Application.OpenForms["Launcher"].Activate();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem1.Enabled = false;
            CheckMangosCrashed.Stop();
            GetSqlOnlineBot.Stop();
            bwCloseSPP.RunWorkerAsync();
        }

        private void worldSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["WorldConf"] == null)
            {
                var show = new WorldConf();
                show.Show();
            }
            else
            {
                Application.OpenForms["WorldConf"].Activate();
            }
        }

        private void worldSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["WorldConf"] == null)
            {
                var show = new WorldConf();
                show.Show();
            }
            else
            {
                Application.OpenForms["WorldConf"].Activate();
            }
        }

        private void tsmHelpUs_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["HelpUsWindow"] == null)
            {
                var show = new HelpUsWindow();
                show.Show();
            }
            else
            {
                Application.OpenForms["HelpUsWindow"].Activate();
            }
        }

        private void reportBugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["BugReport"] == null)
            {
                var show = new BugReport.BugReport();
                show.Show();
            }
            else
            {
                Application.OpenForms["BugReport"].Activate();
            }
        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sReport = new BugReport.BugReport();
            sReport.Show();

            if (Application.OpenForms["BugReport"] == null)
            {
                var show = new BugReport.BugReport();
                show.Show();
            }
            else
            {
                Application.OpenForms["BugReport"].Activate();
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["AboutBox"] == null)
            {
                var show = new AboutBox();
                show.Show();
            }
            else
            {
                Application.OpenForms["Aboutbot"].Activate();
            }
        }

        private void tssStatus_Click(object sender, EventArgs e)
        {
            if(_remoteEmuVer > CurrEmuVer)
            Process.Start(_updLink);
        }

        #endregion

        #region [ TrayMenu ]

        private void openUpdateFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateInFile();
        }

        private void pbAvailableM_MouseHover(object sender, EventArgs e)
        {
            var tt = new ToolTip();
            tt.SetToolTip(pbAvailableM, _sqlStartTime);
        }

        private void pbAvailableR_MouseHover(object sender, EventArgs e)
        {
            var tt = new ToolTip();
            tt.SetToolTip(pbAvailableR, _realmStartTime);
        }

        private void pbAvailableW_MouseHover(object sender, EventArgs e)
        {
            var tt = new ToolTip();
            tt.SetToolTip(pbAvailableW, _worldStartTime);

        }

        private void botSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var botConf = new BotConf();
            botConf.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["AboutBox"] == null)
            {
                var show = new AboutBox();
                show.Show();
            }
            else
            {
                Application.OpenForms["Aboutbox"].Activate();
            }
        }

        private void botSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var bot = new BotConf();
            bot.Show();
        }

        private void SppTray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SppTray.ContextMenuStrip = cmsTray;
                var mi = typeof (NotifyIcon).GetMethod("ShowContextMenu",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(SppTray, null);
                SppTray.ContextMenuStrip = cmsTray;
            }
        }

        private void SppTray_DoubleClick(object sender, EventArgs e)
        {
            Show();
            Application.OpenForms["Launcher"].Activate();
        }

        private void startWowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartWow();
        }

        private void lanSwitcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["LanSwitcher"] == null)
            {
                var show = new LanSwitcher();
                show.Show();
            }
            else
            {
                Application.OpenForms["LanSwitcher"].Activate();
            }
        }

        private void accountToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["WowAccountCreator"] == null)
            {
                var show = new WowaccountCreator();
                show.Show();
            }
            else
            {
                Application.OpenForms["WowAccountCreator"].Activate();
            }
        }

        private void changeWoWPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogMethod();
        }

        private void runWoWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartWow();
        }

        private void lanSwitcherToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["LanSwitcher"] == null)
            {
                var show = new LanSwitcher();
                show.Show();
            }
            else
            {
                Application.OpenForms["LanSwitcher"].Activate();
            }
        }

        private void accountToolToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["WowAccountCreator"] == null)
            {
                var show = new WowaccountCreator();
                show.Show();
            }
            else
            {
                Application.OpenForms["WowAccountCreator"].Activate();
            }
        }

        private void changeWoWPathToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogMethod();
        }

        private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem1.Enabled = false;
            exitToolStripMenuItem2.Enabled = false;
            CheckMangosCrashed.Stop();
            GetSqlOnlineBot.Stop();
            SaveAnnounce();
            bwCloseSPP.RunWorkerAsync();
        }

        public void RealmWorldRestart()
        {
            CheckMangosCrashed.Stop();
            rtWorldDev.Visible = false;
            _allowtext         = false;
            _restart           = true;
            SaveAnnounce();
            bwRestart.RunWorkerAsync();
        }

        private void resetAllRandomBotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetBots();
        }

        private void randomizeBotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeBotsMethod();
        }

        private void resetBotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetBots();
        }

        private void randomizeBotsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RandomizeBotsMethod();
        }

        private void cmsTray_DoubleClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        #endregion

        #region [ Timers ]
        
        private void tmrUsage_Tick(object sender, EventArgs e)
        {
            var ramavail  = GetAvailableRAM();
            var ramtoltal = Convert.ToInt32(Getmemory());
            var ramfree   = ramtoltal - ramavail;
            var ramp      = ramtoltal/100;
            var perecent  = ramfree/ramp;
            var proc  = Process.GetCurrentProcess();
            var usage = Convert.ToInt32(GetCurrentCpuUsage());

            try
            {
                Process[] SqlProcesses = Process.GetProcessesByName("mysqld");
                _sqlMem      = Convert.ToString(SqlProcesses[0].WorkingSet64/1024/1024) + "MB";
            }
            catch (Exception)
            {
                _sqlMem = "N/A";
            }

            try
            {
            Process[] mangosdProcesses = Process.GetProcessesByName("worldserver");
                _mangosdMem      = Convert.ToString(mangosdProcesses[0].WorkingSet64/1024/1024) + "MB";
            }
            catch (Exception)
            {
                _mangosdMem = "N/A";
            }

                try
                {
                Process[] realmdProcesses;
                realmdProcesses = Process.GetProcessesByName("bnetserver");
                _realmdMem      = Convert.ToString(realmdProcesses[0].WorkingSet64/1024/1024) + "MB";
            }
            catch (Exception)
            {
                _realmdMem = "N/A";
            }

            tssUsage.ToolTipText = "Launcher: " + Convert.ToString(proc.WorkingSet64/1024/1024) + "MB" +
                                   "\nMysql Server: " + _sqlMem + "\nLogin Server: " + _realmdMem + "\nGame Server: " +
                                   _mangosdMem;



            if (GetCurrentCpuUsage() != -1)
            {
                tssUsage.Text = "CPU: " + usage + "% |" + " RAM: " + perecent + "%" + " (" + ramfree + "/" + ramtoltal + "MB)";
            }
            else
            {
                tssUsage.Text = "CPU: Error | RAM: N/A (N/A MB)"; 
                perecent = 0;
            }

            if (perecent >= 75) //75
            {
                if (!_sysProtect && _Forcerestart && systemProtectToolStripMenuItem.Checked)
                {
                    _cmd1.StandardInput.WriteLine(Resources.announce_Current_ram_usage + perecent + "%");
                    _cmd1.StandardInput.WriteLine(Resources.announce_If_the_RAM_usage_exceeds);
                    _sysProtect = true;
                }
            }

            if (perecent >= 90) //90
            {
                if (_sysProtect && _Forcerestart && systemProtectToolStripMenuItem.Checked)
                {
                    _Forcerestart = false;
                    _cmd1.StandardInput.WriteLine(Resources.announce_Auto_Restart);
                    Thread.Sleep(1000);
                    MenuItemsDisableAfterLoad();
                    RealmWorldRestart();
                }
            }
        }

        private void tmrRealm_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrRealm.Stop();
                var end1          = DateTime.Now;
                _realmStartTime = (end1 - _start1).TotalSeconds.ToString("##.0" + "sec");
                pbAvailableR.Visible   = true;
                pbTempR.Visible        = false;
                WorldStart();
            }
            catch (Exception)
            {
            }
        }

        private void tmrWorld_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_world.Contains("Initialize data stores..."))
                {
                    pbarWorld.Value = 10;
                }
                if (_world.Contains("Loading Spell Rank Data..."))
                {
                    pbarWorld.Value = 20;
                    _world          = "";
                }
                if (_world.Contains("Loading pet levelup spells..."))
                {
                    pbarWorld.Value = 30;
                    _world          = "";
                }
                if (_world.Contains("Loading Quests..."))
                {
                    pbarWorld.Value = 40;
                    _world          = "";
                }
                if (_world.Contains("Loading SpellArea Data..."))
                {
                    pbarWorld.Value = 50;
                    _world          = "";
                }
                if (_world.Contains("Loading Pet Name Parts..."))
                {
                    pbarWorld.Value = 60;
                    _world          = "";
                }
                if (_world.Contains("Loading gameobject loot templates..."))
                {
                    pbarWorld.Value = 70;
                    _world          = "";
                }
                if (_world.Contains("Loading Guilds..."))
                {
                    pbarWorld.Value = 80;
                    _world          = "";
                }
                if (_world.Contains("Initializing Scripts..."))
                {
                    pbarWorld.Value = 90;
                }
                if (_world.Contains("Loading world quests status..."))
                {
                    tmrWorld.Stop();

                    var end1          = DateTime.Now;
                    _worldStartTime = (end1 - _start1).TotalSeconds.ToString("##.0" + "sec");
                    pbarWorld.Value        = 100;
                    StatusChage(Resources.Launcher_tmrWorld_Tick_Online, false);
                    AnimateStatus(0);
                    pbAvailableW.Visible = true;
                    pbTempW.Visible      = false;
                    pbarWorld.Visible    = false;
                    WindowSize(true);
                    pbarWorld.Value = 0;
                    _allowtext      = true;
                    _sysProtect      = false;
                    _Forcerestart = true;
                    _worldstarted = true;

                    resetAllRandomBotsToolStripMenuItem.Enabled         = true;
                    randomizeBotsToolStripMenuItem.Enabled              = true;
                    resetBotsToolStripMenuItem.Enabled                  = true;
                    randomizeBotsToolStripMenuItem1.Enabled             = true;
                    lanSwitcherToolStripMenuItem1.Enabled               = true;
                    lanSwitcherToolStripMenuItem.Enabled                = true;
                    accountToolToolStripMenuItem1.Enabled               = true;
                    startstopToolStripMenuItem.Enabled                  = true;
                    startToolStripMenuItem.Enabled                      = true;
                    restartToolStripMenuItem1.Enabled                   = true;
                    restartToolStripMenuItem1.Enabled                   = true;
                    sendCommandForServerToolStripMenuItem.Enabled       = true;
                    exportImportCharactersToolStripMenuItem.Enabled     = true;

                    bwUpdate.RunWorkerAsync(); //? Check update.

                    Traymsg();
                    GetSqlOnlineBot.Start(); //? Get online bots.

                    if (_updater)
                    {
                        _updater = false;
                    }

                    CheckMangosCrashed.Start();
                    SrvAnnounce.Start();
                }
            }
            catch (Exception)
            {
            }
        }

        private void SqlStartCheck_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_sql.Contains("Version: '10.1.30-MariaDB'  socket: ''  port: 3310  mariadb.org binary distribution"))
                {
                    MysqlOn = true;
                    SqlStartCheck.Stop();
                    var end1        = DateTime.Now;
                    _sqlStartTime = (end1 - _start1).TotalSeconds.ToString("##.0" + " sec");
                    pbAvailableM.Visible = true;
                    pbTempM.Visible      = false;
                    _sql                 = "";
                    if (_sqlimport) bwImport.RunWorkerAsync();
                    if (Dbupdate)
                    {
                        AnimateStatus(0);
                        Hide();
                        Startupdate();
                    }
                    if (_sqlexport) bwExport.RunWorkerAsync();
                    if (!OnlyMysqlStart)
                    {
                        RealmdStart();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void CheckWowRun_Tick(object sender, EventArgs e)
        {
            CheckWowRun.Stop();
            Traymsg();

            if (!_restart && !CheckRunwow())
            {
                var dialog =
                    MessageBox.Show(Resources.Launcher_CheckWowRun_Tick_Would_you_like_to_start_World_of_Warcraft_,
                        "Question",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes)
                {
                    StartWow();
                }
            }
        }

        private void GetSqlOnlineBot_Tick(object sender, EventArgs e)
        {
            tssLOnline.ToolTipText = Resources.Total_Character__ + new GetAllChar().GetChar("SELECT COUNT(*) FROM characters");
            tssLOnline.Text        = Resources.Launcher_GetSqlOnlineBot_Tick_Online_Bot_ + new GetAllChar().GetChar("SELECT SUM(online) FROM characters");
            SppTray.Text           = tssLOnline.Text;
        }

        private void SrvAnnounce_Tick(object sender, EventArgs e)
        {
            _cmd1.StandardInput.WriteLine(".announce {0}", tssLOnline); //? Announce online bots count every 15 mins.
        }

        private void CheckMangosCrashed_Tick(object sender, EventArgs e)
        {
            if (ProcessView() && !_updater)
            {
                _restart = true;
                NotifyBallon(1000, Resources.Launcher_CheckMangosCrashed_Tick_Mangosd_Crashed,
                    Resources.Launcher_CheckMangosCrashed_Tick_The_process_is_automatically_restart_, true);
                _worldstarted = false;
                RealmWorldRestart();
            }
        }

        #endregion

        #region [ Update ]

        public void UpdateInFile()
        {
            if (MysqlOn)
            {
                var openFile = new OpenFileDialog();
                openFile.Title = "Update";
                openFile.Filter = "SPP Upate (*.sppupdate)|*.sppupdate";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    UpdateUnpack = openFile.FileName;
                    WindowState = FormWindowState.Normal;
                    Show();
                    StatusChage(Resources.Launcher_updateToolStripMenuItem_Click_Decompress_Update, false);
                    AnimateStatus(1);
                    bWUpEx.RunWorkerAsync();
                }
            }
            else
            {
                var openFile = new OpenFileDialog();
                openFile.Title = "Update";
                openFile.Filter = "SPP Upate (*.sppupdate)|*.sppupdate";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    UpdateUnpack = openFile.FileName;
                    WindowState = FormWindowState.Normal;
                    pbAvailableM.Visible = true;
                    Show();
                    StatusChage(Resources.Launcher_updateToolStripMenuItem_Click_Decompress_Update, false);
                    AnimateStatus(1);
                    bWUpEx.RunWorkerAsync();
                }
            }
        }

        private void Startupdate()
        {
            var frm2 = new DatabaseUpdate(this);
            frm2.Show();
        }

        private void bWUpEx_DoWork(object sender, DoWorkEventArgs e)
        {
            new UpdateExtract().Extract();
        }

        private void bWUpEx_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusChage(Resources.Launcher_bWUpEx_RunWorkerCompleted_Completed, false);
            AnimateStatus(0);

            try
            {
                var db    = Convert.ToInt32(File.ReadAllText(@"update\version"));       //? Read update version
                var local = Convert.ToInt32(File.ReadAllText(@"Server\version"));   //? Read databse version
                if (db   != local)
                {
                    if (!MysqlOn)
                    {
                        Dbupdate = true;
                        OnlyMysqlStart = true;
                        StartAll();
                    }
                    else
                    {
                        Dbupdate = true;
                        MenuItemsDisableAfterLoad();
                        rtWorldDev.Visible = false;
                        CheckMangosCrashed.Stop();
                        _allowtext = false;
                        _restart   = true;
                        CloseProcess(true);
                        GetSqlOnlineBot.Stop();
                        tssLOnline.Text = Resources.Launcher_import_Online_Bots__N_A;
                        StatusIcon();
                        pbAvailableM.Visible = true;
                        Hide();
                        Startupdate();
                    }
                }
                else
                {
                    Directory.Delete(@"update", true);
                    MessageBox.Show(Resources.Your_DB_is_up_to_date_);
                    StatusChage(Resources.Launcher_bwUpdate_RunWorkerCompleted_Up_to_date, false);
                }
            }
            catch (Exception)
            { }
        }

        private void WasThisUpdate()
        {
            if (File.Exists("SppLauncher_OLD.exe"))
            {
                File.Delete("SppLauncher_OLD.exe");
                File.SetAttributes("SppLauncher.exe", FileAttributes.Normal);
                var uC = new Update_Completed();
                uC.Show(); //! Open update completed form.
            }
        }

        public bool GetUpdate()
        {
            try
            {
                StatusChage(Resources.Launcher_Checking_Update, false);
                AnimateStatus(1);
                var client     = new WebClient();
                var stream = client.OpenRead("http://spp.splights.eu/updates/update"); //? Get version txt.
                var reader     = new StreamReader(stream);
                var content = reader.ReadToEnd(); //? Example: "<Launcher ver.>;<DB ver.>;<DB Update link>
                var parts = content.Split(';'); //? Load the array.
                content        = parts[0];
                _remoteEmuVer   = Convert.ToDouble(parts[1]);
                _updLink        = parts[2];

                if (content != CurrProgVer)
                {
                    StatusChage(Resources.GetUpdate_New_update_available_, false);
                    AnimateStatus(4);

                    if (MessageBox.Show(
                            Resources.Launcher_bwUpdate_DoWork_New_Version_Available__V + content + "\n" +
                            Resources.Launcher_bwUpdate_DoWork_You_want_to_download_,
                            Resources.Launcher_bwUpdate_DoWork_New_Version, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        _updateYes = true;
                        _sayno = false;
                    }
                    else
                    {
                        _sayno = true;
                    }
                }
            }
            catch (Exception)
            {
                StatusChage(Resources.Launcher_bwUpdate_DoWork_ERROR, false);
                return false;
            }
            return true;
        }

        private void bwUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            Checklang(false);
            GetUpdate();
            CurrEmuVer = Convert.ToDouble(File.ReadAllText("Server\\version")); //? Get local DB version.

            if (_remoteEmuVer > CurrEmuVer)
            {
                StatusChage(Resources.Launcher_bwUpdate_DoWork_New_Server_Update_Available, true);
                AnimateStatus(4);
            }
            else
            {
                if (!_sayno)
                {
                    StatusChage(Resources.Launcher_bwUpdate_RunWorkerCompleted_Up_to_date, false);
                    AnimateStatus(3);
                }
                else
                {
                    StatusChage(Resources.GetUpdate_New_update_available_, false);
                    AnimateStatus(4);
                }
            }
        }

        private void bwUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_updateYes)
            {
                CheckMangosCrashed.Stop();
                GetSqlOnlineBot.Stop();
                CloseProcess(false);
                pbAvailableW.Visible = false;
                pbNotAvailW.Visible  = true;
                pbAvailableR.Visible = false;
                pbNotAvailR.Visible  = true;
                AnimateStatus(1);
                StatusChage("Updating", false);
                Thread.Sleep(1000);
                var update = new Update();
                update.Show();
            }
        }

        #endregion

        #region [ Backup ]

        private void Import()
        {
            if (MysqlOn)
            {
                var openFile = new OpenFileDialog();
                openFile.Title          = Resources.Launcher_import_Import_Characters;
                openFile.Filter         = "SPP Backup (*.sppbackup)|*.sppbackup|All files (*.*)|*.*";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    MenuItemsDisableAfterLoad();
                    _importFile         = openFile.FileName;
                    rtWorldDev.Visible = false;
                    CheckMangosCrashed.Stop();
                    _allowtext = false;
                    _restart = true;
                    bwRunImport.RunWorkerAsync();
                }
            }
            else
            {
                var openFile = new OpenFileDialog();
                openFile.Title          = Resources.Launcher_import_Import_Characters;
                openFile.Filter         = "SPP Backup (*.sppbackup)|*.sppbackup|All files (*.*)|*.*";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    _sqlimport = true;
                    OnlyMysqlStart = true;
                    StartAll();
                    MenuItemsDisableAfterLoad();
                    _importFile   = openFile.FileName;
                    WindowState  = FormWindowState.Normal;
                    Show();
                    StatusChage(Resources.Launcher_import_Decompress, false);
                    AnimateStatus(0);
                }
            }
        }

        private void bwImport_DoWork(object sender, DoWorkEventArgs e)
        {
            AnimateStatus(1);
            Checklang(false);
            ImportExtract();
            StatusChage(Resources.Launcher_import_Import_Characters, false);

            var conn            = "server=127.0.0.1;user=root;pwd=123456;database=characters;port=3310;convertzerodatetime=true;";
            var mb         = new MySqlBackup(conn);
            mb.ImportInfo.FileName = _getTemp + "\\save01";
            mb.Import();

            StatusChage(Resources.Launcher_bwImport_DoWork_Import_Accounts, false);

            var conn1            = "server=127.0.0.1;user=root;pwd=123456;database=realmd;port=3310;convertzerodatetime=true;";
            var mb1         = new MySqlBackup(conn1);
            mb1.ImportInfo.FileName = _getTemp + "\\save02";
            mb1.Import();

            StatusChage("Char Update", false);
            if (Directory.Exists(@"Database\char"))
            {
                InsertMultiple1(@"Database\char", "characters", "*sql");
            }
        }

        private void InsertMultiple1(string updatePath, string db, string filter)
        {
            try
            {
                String[] files = Directory.GetFiles(updatePath, filter, SearchOption.TopDirectoryOnly);
                Thread.Sleep(10);
                foreach (String aFile in files)
                {
                    run.RunMySql("127.0.0.1", 3310, "root", "123456", db, aFile);
                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void bwImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _sqlimport = false;
            if (OnlyMysqlStart)
            {
                ShutdownSql();
                pbAvailableM.Visible               = false;
                pbNotAvailM.Visible                = true;
                startstopToolStripMenuItem.Enabled = true;
            }
            if (!OnlyMysqlStart)
            {
                StartAll();
            }
            OnlyMysqlStart = false;
            StatusChage(Resources.Launcher_bwImport_RunWorkerCompleted_Import_Completed, false);
            AnimateStatus(3);
            File.Delete(_getTemp + "\\save01");
            File.Delete(_getTemp + "\\save02");
        }

        private void ImportExtract() //.sppbackup extract
        {
            var unpck       = _importFile;
            var unpckDir    = _getTemp;
            using (var zip = ZipFile.Read(unpck))
            {
                foreach (var e in zip)
                {
                    e.Password = "89765487";
                    e.Extract(unpckDir, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private void Export()
        {
            if (MysqlOn)
            {
                var saveFile = new SaveFileDialog();
                saveFile.Title          = Resources.Launcher_export_Export_Characters;
                saveFile.Filter         = "SPP Backup (*.sppbackup)|*.sppbackup|All files (*.*)|*.*";
                saveFile.FileName       = Resources.Launcher_export_Backup;

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    AnimateStatus(1);
                    StatusChage(Resources.Launcher_export_Exporting_Characters, false);
                    _exportfile   = saveFile.FileName;
                    bwExport.RunWorkerAsync();
                }
            }
            else
            {
                var saveFile = new SaveFileDialog();
                saveFile.Title          = Resources.Launcher_export_Export_Characters;
                saveFile.Filter         = "SPP Backup (*.sppbackup)|*.sppbackup|All files (*.*)|*.*";
                saveFile.FileName       = Resources.Launcher_export_Backup;

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    OnlyMysqlStart = true;
                    _sqlexport = true;
                    StartAll();
                    AnimateStatus(1);
                    StatusChage(Resources.Launcher_export_Exporting_Characters, false);
                    _exportfile   = saveFile.FileName;
                }
            }
        }

        private void bwExport_DoWork(object sender, DoWorkEventArgs e)
        {
            Checklang(false);
            const string conn = "server=127.0.0.1;user=root;pwd=123456;database=characters;port=3310;convertzerodatetime=true;";
            var mb = new MySqlBackup(conn);
            mb.ExportInfo.FileName = _getTemp + "\\save01";
            mb.Export();

            StatusChage(Resources.Launcher_bwExport_DoWork_Export_Accounts, false);

            const string conn1 = "server=127.0.0.1;user=root;pwd=123456;database=realmd;port=3310;convertzerodatetime=true;";
            var mb1 = new MySqlBackup(conn1);
            mb1.ExportInfo.FileName = _getTemp + "\\save02";
            mb1.Export();

            StatusChage(Resources.Launcher_bwExport_DoWork_Compressing, false);

            using (var zip = new ZipFile()) //create .sppbackup
            {
                zip.Password = "89765487";
                zip.AddFile(_getTemp + "\\save01", @"\");
                zip.AddFile(_getTemp + "\\save02", @"\");
                zip.Save(_exportfile);
            }
        }

        private void bwExport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _sqlexport = false;
            if (OnlyMysqlStart)
            {
                ShutdownSql();

                pbAvailableM.Visible               = false;
                pbNotAvailM.Visible                = true;
                startstopToolStripMenuItem.Enabled = true;
            }

            OnlyMysqlStart = false;
            Status = Resources.Launcher_bwExport_RunWorkerCompleted_Export_Completed;
            StatusChage(Resources.Launcher_bwExport_RunWorkerCompleted_Export_Completed, false);
            AnimateStatus(3);
            File.Delete(_getTemp + "\\save01");
            File.Delete(_getTemp + "\\save02");
        }

        #endregion

        private void systemProtectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (systemProtectToolStripMenuItem.Checked)
            {
                systemProtectToolStripMenuItem.Checked = false;
                //autorunToolStripMenuItem.Checked = false;
                SysProt = "0";
                _xmlReadWrite.saveMethod();
            }
            else
            {
                systemProtectToolStripMenuItem.Checked = true;
                //autorunToolStripMenuItem.Checked = true;
                SysProt = "1";
                _xmlReadWrite.saveMethod();
            }
        }
    }
}