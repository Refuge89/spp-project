﻿using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SppLauncher
{
    public partial class BotConf : Form
    {
        readonly IniFileClass MyIni              = new IniFileClass("config\\aiplayerbot.CONF");
        private string _maps            = " ", _mapsWrite = " ";
        private string[] _arr1          = new string[4];
        private bool DefaultL;
        public BotConf()
        {
            InitializeComponent();

            groupBox1.Text = Launcher.Msgbotsettings;
            label7.Text = Launcher.Msgbots;
            label1.Text = Launcher.Msgminbots;
            label2.Text = Launcher.Msgmaxbots;
            label3.Text = Launcher.Msgminbotsi;
            label4.Text = Launcher.Msgmaxbotsi;
            label6.Text = Launcher.Msgbotsacc;
            label5.Text = Launcher.Msgupinter;
            groupBox2.Text = Launcher.Msgbotmaps;
            groupBox3.Text = Launcher.Msgother;
            cbDel.Text = Launcher.Msgbotacc;
            cbJoin.Text = Launcher.Msgbotlfg;
            cbRndBotLgn.Text = Launcher.Msgrnd;
            button2.Text = Launcher.Msgloaddef;
            button1.Text = Launcher.Msgsave;
            ReadBotConf();

            if (Launcher.lang == "Hungarian")
            {
                label6.Location = new Point(90, 157);
                label3.Location = new Point(55, 105);
                label4.Location = new Point(55, 131);
                label5.Location = new Point(55, 183);
                label7.Location = new Point(120, 22);
                label1.Location = new Point(109, 49);
                label2.Location = new Point(106, 79);
            }
            else
            {
                label6.Location = new Point(86, 157);
                label3.Location = new Point(68, 105);
                label4.Location = new Point(65, 131);
                label5.Location = new Point(75, 183);
                label7.Location = new Point(128, 22);
                label1.Location = new Point(109, 49);
                label2.Location = new Point(106, 79);
            }

            ToolTip tt = new ToolTip();
            tt.SetToolTip(label6, "Function is temporarily disabled");
        }

        private void button1_Click(object sender, EventArgs e)
        {
           SaveIni();
        }

        public void SaveIni()
        {

            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.MinRandomBots", " " + txbMinbot.Text);
            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.MaxRandomBots", " " + txbMaxBot.Text);
            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.MinRandomBotsPerInterval", " " + txbMinBotInter.Text);
            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.MaxRandomBotsPerInterval", " " + txbMaxBotInter.Text);
            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotAccountCount", " " + txbBotAccount.Text);
            MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotUpdateInterval", " " + txbUpdateInter.Text);

            try
            {
                switch (cbDel.Checked)
                {
                    case true:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.DeleteRandomBotAccounts", " 1");
                        break;
                    case false:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.DeleteRandomBotAccounts", " 0");
                        break;
                }
                switch (cbJoin.Checked)
                {
                    case true:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotJoinLfg", " 1");
                        break;
                    case false:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotJoinLfg", " 0");
                        break;
                }

                switch (cbBots.Text)
                {
                    case "Enabled":
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.Enabled", " 1");
                        break;
                    case "Disabled":
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.Enabled", " 0");
                        break;
                }

                switch (cbRndBotLgn.Checked)
                {
                    case true:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotLoginAtStartup", " 1");
                        break;
                    case false:
                        MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotLoginAtStartup", " 0");
                        break;
                }

                string[] arr = new string[4];

                if (cbEastern.Checked)
                    arr[0] = "0";

                if (cbKalimdor.Checked)
                    arr[1] = "1";

                if (cbNorthrend.Checked)
                    arr[2] = "571";

                if (cbOutland.Checked)
                    arr[3] = "530";

                var res = String.Join(",", arr.Where(s => !string.IsNullOrEmpty(s)));

                MyIni.Write("AiPlayerbotConf", "AiPlayerbot.RandomBotMaps", " " + res);
            }
            catch (Exception)
            {
                MessageBox.Show("Some exception: write", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!DefaultL)
                MessageBox.Show("Bot Settings Saved.\nThe changes to take effect bot reset requiered.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            MessageBox.Show("Default settings loaded.\nThe changes to take effect bot reset requiered.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        public void ReadBotConf()
        {
            switch (MyIni.Read("AiPlayerbotConf", "AiPlayerbot.Enabled"))
            {
                case "1":
                    cbBots.Text = "Enabled";
                    break;
                case "0":
                    cbBots.Text = "Disabled";
                    break;
            }
            txbMinbot.Text      = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.MinRandomBots");
            txbMaxBot.Text      = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.MaxRandomBots");
            txbMinBotInter.Text = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.MinRandomBotsPerInterval");
            txbMaxBotInter.Text = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.MaxRandomBotsPerInterval");
            txbBotAccount.Text  = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.RandomBotAccountCount");
            txbUpdateInter.Text = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.RandomBotUpdateInterval");

            switch (MyIni.Read("AiPlayerbotConf", "AiPlayerbot.DeleteRandomBotAccounts"))
            {
                case "1":
                    cbDel.Checked = true;
                    break;
                case "2":
                    cbDel.Checked = true;
                    break;
            }

            switch (MyIni.Read("AiPlayerbotConf", "AiPlayerbot.RandomBotJoinLfg"))
            {
                case "1":
                    cbJoin.Checked = true;
                    break;
                case "0":
                    cbJoin.Checked = false;
                    break;
            }

            switch (MyIni.Read("AiPlayerbotConf", "AiPlayerbot.RandomBotLoginAtStartup"))
            {
                case "1":
                    cbRndBotLgn.Checked = true;
                    break;
                case "0":
                    cbRndBotLgn.Checked = false;
                    break;
            }

            _maps = MyIni.Read("AiPlayerbotConf", "AiPlayerbot.RandomBotMaps");
            _arr1 = _maps.Split(',');

            if (_arr1.Contains("0"))
            {
                cbEastern.Checked = true;
            }

            if (_arr1.Contains("1"))
            {
                cbKalimdor.Checked = true;
            }

            if (_arr1.Contains("530"))
            {
                cbOutland.Checked = true;
            }

            if (_arr1.Contains("571"))
            {
                cbNorthrend.Checked = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cbBots.Text         = "Enabled";
            txbMinbot.Text      = "50";
            txbMaxBot.Text      = "100";
            txbMinBotInter.Text = "50";
            txbMaxBotInter.Text = "100";
            txbBotAccount.Text  = "50";
            txbUpdateInter.Text = "60";

            cbEastern.Checked   = true;
            cbKalimdor.Checked  = true;
            cbNorthrend.Checked = false;
            cbOutland.Checked   = false;
            cbJoin.Checked      = true;
            DefaultL            = true;
            SaveIni();
        }

        private void cbEastern_CheckedChanged(object sender, EventArgs e)
        {
            _mapsWrite = _mapsWrite + "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void cbRndBotLgn_MouseHover(object sender, EventArgs e)
        {
            tTrndBot.Show("All bots join on start.", cbRndBotLgn, 100000);
        }
    }
}