﻿using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace SteamNotifierHelper
{
    public partial class Helper : Form
    {
        public Helper()
        {
            InitializeComponent();

			SetSettingsValues();
		}

        private void btnAbout_Click(object sender, EventArgs e)
        {
            About abtForm = new About();

            abtForm.ShowDialog();
        }

	    private void btnOpenIgnored_Click(object sender, EventArgs e)
	    {
		    new frmIgnore().ShowDialog();
	    }

		private void SetSettingsValues()
	    {
		    // ensures that changing the checked value upon startup does not trigger the event
		    ckbStartup.CheckedChanged -= ckbStartup_CheckedChanged;
		    ckbMute.CheckedChanged -= ckbMute_CheckedChanged;
		    ckbAppID.CheckedChanged -= ckbAppID_CheckedChanged;

		    ckbStartup.Checked = SteamNotifier.Helpers.Settings.OpenOnStartup;
		    ckbMute.Checked = SteamNotifier.Helpers.Settings.Muted;
		    ckbAppID.Checked = SteamNotifier.Helpers.Settings.ShowAppID;

		    ckbStartup.CheckedChanged += ckbStartup_CheckedChanged;
		    ckbMute.CheckedChanged += ckbMute_CheckedChanged;
		    ckbAppID.CheckedChanged += ckbAppID_CheckedChanged;
		}

        private void ckbStartup_CheckedChanged(object sender, EventArgs e)
        {
	        SteamNotifier.Helpers.Settings.OpenOnStartup = ckbStartup.Checked;
        }

		private void ckbMute_CheckedChanged(object sender, EventArgs e)
		{
			SteamNotifier.Helpers.Settings.Muted = ckbMute.Checked;
		}

		private void ckbAppID_CheckedChanged(object sender, EventArgs e)
		{
			SteamNotifier.Helpers.Settings.ShowAppID = ckbAppID.Checked;
		}
	}
}