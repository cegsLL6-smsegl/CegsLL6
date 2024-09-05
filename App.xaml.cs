﻿using AeonHacs;
using AeonHacs.Utilities;
using System;
using System.Media;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace CegsLL6;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    SoundPlayer soundPlayer;
    HacsLog EventLog = Hacs.EventLog;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        StartNoticeHandler();
        LoadPreferences();

        var window = new AeonHacs.Wpf.Views.MainWindow();
        window.LoadControlPanel(new Views.ControlPanel(window.Close));
        MainWindow = window;

        //Resources.Add("BackPanel", new BackPanel());

        if (!window.IsClosed)
            MainWindow.Show();

        // Interaction.GetBehaviors(MainWindow).Add(new ScalableWindowBehavior());
    }

    /// <summary>
    /// Save preferences and close the event log before the application exits.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnExit(ExitEventArgs e)
    {
        SavePreferences();
        Hacs.EventLog.Close();
        base.OnExit(e);
    }

    string PreferencesFileName = "Preferences.xaml";
    void LoadPreferences()
    {
        ResourceDictionary prefs = null;
        try
        {
            using (var reader = XmlReader.Create(PreferencesFileName))
                prefs = (ResourceDictionary)XamlReader.Load(reader);
        }
        catch (Exception e) { var ignore = e; }

        if (prefs == null)
            prefs = new ResourceDictionary();
        Resources["PreferencesDictionary"] = prefs;
        Resources.MergedDictionaries.Add(prefs);
    }

    void SavePreferences()
    {
        try
        {
            var prefs = (ResourceDictionary)Resources["PreferencesDictionary"];
            if (prefs != null)
            {
                var writerSettings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "\t",
                    CloseOutput = true,
                };
                using (var writer = System.Xml.XmlWriter.Create(PreferencesFileName, writerSettings))
                    XamlWriter.Save(prefs, writer);
            }
        }
        catch (Exception e) { var ignore = e; }
    }

    #region Notice Handling

    void StartNoticeHandler()
    {
        InitializeSound();
        Notice.DefaultSender.OnNotice += NoticeHandler;
    }

    #region Sound
    void InitializeSound()
    {
        soundPlayer = new SoundPlayer(@"C:\Windows\Media\chord.wav");
        soundPlayer.Load();
    }

    public void PlaySound()
    {
        if (soundPlayer?.IsLoadCompleted ?? false)
            soundPlayer.Play();
    }

    #endregion Sound


    void processNotice(Notice notice)
    {
        if (notice.Text == "PlaySound")
        {
            //if (string.IsNullOrEmpty(notice.Caption))
            PlaySound();
            // else
            // play a specific sound? (does nothing for now);

        }
        else   // The default behavior is to show the notice in a MessageBox
        {
            MessageBox.Show(notice.Text, notice.Caption);
        }
    }

    Notice NoticeHandler(Notice notice)
    {
        if (
            notice.NoticeType == Notice.Type.Tell ||
            notice.NoticeType == Notice.Type.Request)
        {
            processNotice(notice);
            return null;
        }
        else if (notice.NoticeType == Notice.Type.OkCancel)
        {
            if (MessageBox.Show(notice.Text, notice.Caption,
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                return new Notice("Ok");
            else
                return new Notice("Cancel");
        }
        else if (notice.NoticeType == Notice.Type.Warn)
        {
            if (MessageBox.Show(notice.Text, notice.Caption,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                return new Notice("Ok");
            else
                return new Notice("Cancel");
        }
        else
        {
            return new Notice($"Unhandled NoticeType: {notice.NoticeType}");
        }
    }

    #endregion Notice Handling

}
