﻿using System;
using System.Windows.Forms;
using ClipboardMonitorLite.Cloud;
using ClipboardMonitorLite.FormControls;
using ClipboardMonitorLite.FileOperations;
using SettingsLib;
using ClipboardMonitorLite.ClipboardActions;
using ClipboardMonitorLite.Languages.LanguageControl;

namespace ClipboardMonitorLite
{
    public partial class MainForm : Form
    {
        private Settings _settings;
        private FormEvents _formEvents;
        private CreateJsonFile _createJson;
        private InboundMessage _inboundMessage;
        private LanguageOnForm _languageOnForm;
        private SettingsHandler _settingsHandler;
        private CheckConnection _checkConnection;
        private OutgoingMessage _outgoingMessage;
        private LaunchOnStartup _startWithWindows;
        private WriteHistoryFile _writeHistoryFile;
        private ClipboardManager _clipboardManager;
        private CloudInteractions _cloudInteractions;

        public MainForm()
        {
            InitializeComponent();
            _createJson = new CreateJsonFile();
            _settingsHandler = new SettingsHandler();
            _settings = _settingsHandler.LoadSettingsFile();
            _inboundMessage = new InboundMessage();
            _outgoingMessage = new OutgoingMessage();
            _clipboardManager = new ClipboardManager(_inboundMessage, _outgoingMessage, _settings);
            _cloudInteractions = new CloudInteractions(_inboundMessage, _outgoingMessage, _settings, _clipboardManager);
            _writeHistoryFile = new WriteHistoryFile(_clipboardManager, _settings);
            _languageOnForm = new LanguageOnForm();
            _formEvents = new FormEvents(_clipboardManager, notificationIcon, this, _settings, _writeHistoryFile);
            _startWithWindows = new LaunchOnStartup(_settings);
            _checkConnection = new CheckConnection(timer_checkConnection, Label_Connection_DONOTMODIFY);
            EnumSetLang();
            BindProperties();
            BindButtonActions();
        }

        private void BindProperties()
        {
            CopiedItemBox.DataBindings.Add("Text", _clipboardManager, "ClipboardHistory",
                true, DataSourceUpdateMode.OnPropertyChanged);

            Btn_Donate.DataBindings.Add("Visible", _settings, "ShowDonation",
                true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void BindButtonActions()
        {
            FormClosing += _formEvents.FormClosing;
            Btn_Donate.Click += _formEvents.DonationClick;
            CopiedItemBox.LinkClicked += _formEvents.LinkClicked;
            Btn_EmptyHistory.Click += _formEvents.ClearHistoryClick;
            Btn_EmptyClipboard.Click += _formEvents.ClearClipboardClick;
            notificationIcon.DoubleClick += _formEvents.RestoreWindowClick;
            exitToolStripMenuItem.Click += _formEvents.ExitApplicationClick;
            restoreToolStripMenuItem.Click += _formEvents.RestoreWindowClick;
            emptyHistoryToolStripMenuItem.Click += _formEvents.ClearHistoryClick;
            emptyClipboardToolStripMenuItem.Click += _formEvents.ClearClipboardClick;
        }
        private void Btn_MoreOptions_Click(object sender, EventArgs e)
        {
            OptionsForm form = new OptionsForm(_settings, _formEvents);
            form.ShowDialog();
            form.Dispose();
            _createJson.CreateFile(_settings);
            _languageOnForm.SetLang(_settings, this);
        }

        private void EnumSetLang()
        {
            _languageOnForm.SetLang(_settings, this);
        }
    }
}