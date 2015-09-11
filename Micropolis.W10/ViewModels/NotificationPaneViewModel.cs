﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Engine;
using Micropolis.Common;
using Microsoft.ApplicationInsights;

namespace Micropolis.ViewModels
{
    public class NotificationPaneViewModel : BindableBase
    {
        public NotificationPaneViewModel()
        {
            Messages=new ObservableCollection<string>();
            try { 
            _telemetry = new TelemetryClient();
            }
            catch (Exception) { }
        }

        private static readonly Size VIEWPORT_SIZE = new Size(160, 160);
        private static readonly SolidColorBrush QUERY_COLOR = new SolidColorBrush(Color.FromArgb(255, 255, 165, 0));
        private MainGamePageViewModel _mainPageViewModel;

        /// <summary>
        /// Sets up this instance after basic initialization.
        /// </summary>
        /// <param name="mainPage"></param>
        public void SetUpAfterBasicInit(MainGamePageViewModel mainPageViewModel)
        {
            _mainPageViewModel = mainPageViewModel;
            DismissButtonText = Strings.GetString("notification.dismiss");
            DismissCommand = new DelegateCommand(() => { OnDismissClicked(); }); 
        }

        private string _dismissButtonText;
        public string DismissButtonText { get { return _dismissButtonText; } set { SetProperty(ref _dismissButtonText, value); } }

        private DelegateCommand _dismissCommand;
        public DelegateCommand DismissCommand { get { return _dismissCommand; } set { SetProperty(ref _dismissCommand, value); } }


        /// <summary>
        /// Called when user clicked the dismiss button to close the pane.
        /// </summary>
        private void OnDismissClicked()
        {
            try { 
            _telemetry.TrackEvent("NotificationPaneDismissClicked");
            }
            catch (Exception) { }

            _mainPageViewModel.HideNotificationPanel();
        }

        private WriteableBitmap _mapImage;
        public WriteableBitmap MapImage { get { return _mapImage; } set { SetProperty(ref _mapImage, value); } }

        /// <summary>
        /// Sets the picture of the panel.
        /// </summary>
        /// <param name="xpos">xpos in map</param>
        /// <param name="ypos">ypos in map</param>
        private void SetPicture(int xpos, int ypos)
        {
            Size sz = VIEWPORT_SIZE;

            MapImage = _mainPageViewModel.GetLandscapeFromDrawingArea(xpos, ypos, sz);

            
        }

        /// <summary>
        /// Shows the specified message for the specified map coordinates.
        /// </summary>
        /// <param name="msg">message to show</param>
        /// <param name="xpos">xpos in map</param>
        /// <param name="ypos">ypos in map</param>
        public void ShowMessage(MicropolisMessage msg, int xpos, int ypos)
        {
            try { 
            _telemetry.TrackEvent("NotificationPaneMessageShown"+msg.Name);
            }
            catch (Exception) { }

            SetPicture(xpos, ypos);

            if (InfoPaneIsVisible == true)
            {
                InfoPaneIsVisible = false;
            }

            HeaderTextBlockText = Strings.GetString(msg.Name + ".title");
            HeaderSPBackground = new SolidColorBrush(ColorParser.ParseColor(Strings.GetString(msg.Name + ".color")));

            Messages.Clear();
            string[] messages = Strings.GetString(msg.Name + ".detail").Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string message in messages)
            {
                Messages.Add(message);
            }
            
            DetailPaneIsVisible = true;
        }

        private SolidColorBrush _headerSPBackground;
        public SolidColorBrush HeaderSPBackground { get { return _headerSPBackground; } set { SetProperty(ref _headerSPBackground, value); } }

        private string _headerTextBlockText;
        public string HeaderTextBlockText { get { return _headerTextBlockText; } set { SetProperty(ref _headerTextBlockText, value); } }

        private bool _infoPaneIsVisible;
        public bool InfoPaneIsVisible { get { return _infoPaneIsVisible; } set { SetProperty(ref _infoPaneIsVisible, value); } }


        private bool _detailPaneIsVisible;
        public bool DetailPaneIsVisible { get { return _detailPaneIsVisible; } set { SetProperty(ref _detailPaneIsVisible, value); } }

        public ObservableCollection<string> Messages { get; set; } 

        /// <summary>
        /// Show zone status for specified coordinates and zonestatus
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <param name="zone"></param>
        public void ShowZoneStatus(int xpos, int ypos, ZoneStatus zone)
        {
            DetailPaneIsVisible = false;

            HeaderTextBlockText = Strings.GetString("notification.query_hdr");
            HeaderSPBackground = QUERY_COLOR;

            String buildingStr = zone.Building != -1 ? Strings.GetString("zone." + zone.Building) : "";
            String popDensityStr = Strings.GetString("status." + zone.PopDensity);
            String landValueStr = Strings.GetString("status." + zone.LandValue);
            String crimeLevelStr = Strings.GetString("status." + zone.CrimeLevel);
            String pollutionStr = Strings.GetString("status." + zone.Pollution);
            String growthRateStr = Strings.GetString("status." + zone.GrowthRate);

            SetPicture(xpos, ypos);
            InfoPaneIsVisible = true;

            T1TextBlockText = Strings.GetString("notification.zone_lbl");
            BuildStrTextBlockText = buildingStr;
            NotificationDensityTextBlockText = Strings.GetString("notification.density_lbl");
            PopDensityStrTextBlockText = popDensityStr;
            NotificationValueTextBlockText = Strings.GetString("notification.value_lbl");
            LandValueStrTextBlockText = landValueStr;
            NotificationCrimeTextBlockText = Strings.GetString("notification.crime_lbl");
            CrimeLevelStrTextBlockText = crimeLevelStr;
            NotificationPollutionTextBlockText = Strings.GetString("notification.pollution_lbl");
            PollutionStrTextBlockText = pollutionStr;
            NotificationGrowthTextBlockText = Strings.GetString("notification.growth_lbl");
            GrowthRateStrTextBlockText = growthRateStr;
            _mainPageViewModel.ShowNotificationPanel();
        }

        private string _t1TextBlockText;
        public string T1TextBlockText { get { return _t1TextBlockText; } set { SetProperty(ref _t1TextBlockText, value); } }


        private string _buildStrTextBlockText;
        public string BuildStrTextBlockText { get { return _buildStrTextBlockText; } set { SetProperty(ref _buildStrTextBlockText, value); } }


        private string _popDensityStrTextBlockText;
        public string PopDensityStrTextBlockText { get { return _popDensityStrTextBlockText; } set { SetProperty(ref _popDensityStrTextBlockText, value); } }

        private string _notificationValueTextBlockText;
        public string NotificationValueTextBlockText { get { return _notificationValueTextBlockText; } set { SetProperty(ref _notificationValueTextBlockText, value); } }


        private string _notificationDensityTextBlockText;
        public string NotificationDensityTextBlockText { get { return _notificationDensityTextBlockText; } set { SetProperty(ref _notificationDensityTextBlockText, value); } }


        private string _landValueStrTextBlockText;
        public string LandValueStrTextBlockText { get { return _landValueStrTextBlockText; } set { SetProperty(ref _landValueStrTextBlockText, value); } }

        private string _notificationCrimeTextBlockText;
        public string NotificationCrimeTextBlockText { get { return _notificationCrimeTextBlockText; } set { SetProperty(ref _notificationCrimeTextBlockText, value); } }

        private string _crimeLevelStrTextBlockText;
        public string CrimeLevelStrTextBlockText { get { return _crimeLevelStrTextBlockText; } set { SetProperty(ref _crimeLevelStrTextBlockText, value); } }

        private string _notificationPollutionTextBlockText;
        public string NotificationPollutionTextBlockText { get { return _notificationPollutionTextBlockText; } set { SetProperty(ref _notificationPollutionTextBlockText, value); } }

        private string _pollutionStrTextBlockText;
        public string PollutionStrTextBlockText { get { return _pollutionStrTextBlockText; } set { SetProperty(ref _pollutionStrTextBlockText, value); } }

        private string _notificationGrowthTextBlockText;
        public string NotificationGrowthTextBlockText { get { return _notificationGrowthTextBlockText; } set { SetProperty(ref _notificationGrowthTextBlockText, value); } }

        private string _growthRateStrTextBlockText;
        private TelemetryClient _telemetry;
        public string GrowthRateStrTextBlockText { get { return _growthRateStrTextBlockText; } set { SetProperty(ref _growthRateStrTextBlockText, value); } }
    }
}