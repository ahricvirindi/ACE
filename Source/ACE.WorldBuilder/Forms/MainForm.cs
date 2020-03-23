using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using ACE.WorldBuilder.Forms;
using ACE.Database;
using ACE.Common;
using log4net;
using log4net.Config;


namespace ACE.WorldBuilder
{
    public partial class MainForm : Form
    {
        private CharactersForm charsForm;
        private LandblocksForm landblocksForm;
        private NpcsForm npcsForm;
        private CreaturesForm creaturesForm;
        private QuestsForm questsForm;
        private SettingsForm settingsForm;
        private ItemsForm itemsForm;


        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainForm()
        {
            InitializeComponent();

            var loaded = true;

            loaded = IniitalizeLogging();
            if (loaded) { loaded = InitializeConfiguration(); }
            if (loaded) { loaded = InitializeDatabases(); }

            if (!loaded)
            {
                foreach (ToolStripItem m in menu.Items)
                {
                    if (m.Text != "&File" && m.Text != "&Help") m.Enabled = false;
                }
            }
        }

        private bool IniitalizeLogging()
        {
            try
            {
                var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
                var log4netFileInfo = new FileInfo("log4net.config");
                XmlConfigurator.Configure(logRepository, log4netFileInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("One or more errors occurred setting up logging: {0}", ex.Message));
                return false;
            }

            return true;
        }

        private bool InitializeConfiguration()
        {
            try
            {
                ConfigManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("One or more errors occurred loading configurations: {0}", ex.Message));
                return false;
            }

            return true;
        }

        private bool InitializeDatabases()
        {
            try
            {
                DatabaseManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("One or more errors occurred initializing database connections: {0}", ex.Message));
                return false;
            }

            return true;
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                settingsForm = new SettingsForm();
                settingsForm.MdiParent = this;
                settingsForm.Reset();
            }

            settingsForm.Show();
            settingsForm.Focus();
        }

        private void menuCharacters_Click(object sender, EventArgs e)
        {
            if (charsForm == null || charsForm.IsDisposed)
            {
                charsForm = new CharactersForm();
                charsForm.MdiParent = this;
                charsForm.Reset();
            }

            charsForm.Show();
            charsForm.Focus();
        }

        private void menuQuests_Click(object sender, EventArgs e)
        {
            if (questsForm == null || questsForm.IsDisposed)
            {
                questsForm = new QuestsForm();
                questsForm.MdiParent = this;
                questsForm.Reset();
            }

            questsForm.Show();
            questsForm.Focus();
        }

        private void menuNpcs_Click(object sender, EventArgs e)
        {
            if (npcsForm == null || npcsForm.IsDisposed)
            {
                npcsForm = new NpcsForm();
                npcsForm.MdiParent = this;
                npcsForm.Reset();
            }

            npcsForm.Show();
            npcsForm.Focus();
        }

        private void menuLandblocks_Click(object sender, EventArgs e)
        {
            if (landblocksForm == null || landblocksForm.IsDisposed)
            {
                landblocksForm = new LandblocksForm();
                landblocksForm.MdiParent = this;
                landblocksForm.Reset();
            }

            landblocksForm.Show();
            landblocksForm.Focus();
        }

        private void menuCreatures_Click(object sender, EventArgs e)
        {
            if (creaturesForm == null || creaturesForm.IsDisposed)
            {
                creaturesForm = new CreaturesForm();
                creaturesForm.MdiParent = this;
                creaturesForm.Reset();
            }

            creaturesForm.Show();
            creaturesForm.Focus();
        }

        private void menuItems_Click(object sender, EventArgs e)
        {
            if (itemsForm == null || itemsForm.IsDisposed)
            {
                itemsForm = new ItemsForm();
                itemsForm.MdiParent = this;
                itemsForm.Reset();
            }

            itemsForm.Show();
            itemsForm.Focus();
        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}
