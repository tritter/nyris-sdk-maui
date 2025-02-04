﻿using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Nyris.UI.Maui;

namespace Nyris.Demo.MAUI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    
    private void OnSearcherStart(object sender, EventArgs e)
    {
        NyrisSearcher
            .Builder("", true)
            .Theme(themeConfig =>
            {
                themeConfig.PrimaryTintColor = Colors.Blue;
            })
            .Start(result =>
            {
                if (result == null)
                {
                    ResultLabel.Text =
                        "the searcher is canceled or an exception is raised which forces the result to be null";
                }
                else
                {
                    ResultLabel.Text = 
                        $"Nyris searcher found ({result.Offers.Count}) offers, with request id: {result.RequestCode})";
                    if(result.Screenshot == null) return;
                    var stream = new MemoryStream(result.Screenshot);
                    ScreenshotResult.Source = ImageSource.FromStream(() => stream);
                }
            });
    }
}