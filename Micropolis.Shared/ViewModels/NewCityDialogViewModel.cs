﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Engine;
using Micropolis.Common;
using Microsoft.ApplicationInsights;

namespace Micropolis.ViewModels
{
    public class NewCityDialogViewModel : BindableBase
    {
         private readonly Dictionary<int, LevelButtonViewModel> _levelBtns = new Dictionary<int, LevelButtonViewModel>();
        private readonly Stack<Engine.Micropolis> _nextMaps = new Stack<Engine.Micropolis>();
        private readonly Stack<Engine.Micropolis> _previousMaps = new Stack<Engine.Micropolis>();
        private Engine.Micropolis _engine;
        private bool _firstTime;

        public ObservableCollection<LevelButtonViewModel> Levels { get; set; } 

        public NewCityDialogViewModel(OverlayMapViewModel mapPaneViewModel)
        {
            _telemetry = new TelemetryClient();

            _mapPaneViewModel = mapPaneViewModel;
            Levels=new ObservableCollection<LevelButtonViewModel>();
            TitleTextBlockText = Strings.GetString("welcome.caption");
            //mapPane.Destroy();

            _engine = new Engine.Micropolis();
            new MapGenerator(_engine).GenerateNewCity();
            //mapPane = new OverlayMapView(engine);
            _mapPaneViewModel.SetUpAfterBasicInit(_engine);


            for (int lev = GameLevel.MIN_LEVEL; lev <= GameLevel.MAX_LEVEL; lev++)
            {
                int x = lev;
                var radioBtn = new LevelButtonViewModel {Text = Strings.GetString("menu.difficulty." + lev)};
                radioBtn.ClickCommand = new DelegateCommand(() => { SetGameLevel(x); });

                Levels.Add(radioBtn);
                _levelBtns.Add(x, radioBtn);
            }
            SetGameLevel(GameLevel.MIN_LEVEL);
            PreviousMapButtonText = Strings.GetString("welcome.previous_map");
            PreviousMapCommand = new DelegateCommand(() => { OnPreviousMapClicked(); });
            ThisMapButtonText = Strings.GetString("welcome.play_this_map");
            NextMapButtonText = Strings.GetString("welcome.next_map");
            NextMapCommand = new DelegateCommand(() => { OnNextMapClicked(); });
            CancelButtonText = Strings.GetString("welcome.cancel");
            

            LoadCityButtonText = Strings.GetString("welcome.load_city");

            LoadCityCommand = new DelegateCommand(() => { OnLoadCityClicked(); });

            

        }

        private string _titleTextBlockText;
        public string TitleTextBlockText { get { return _titleTextBlockText; } set { SetProperty(ref _titleTextBlockText, value); } }

        private string _thisMapButtonText;
        public string ThisMapButtonText { get { return _thisMapButtonText; } set { SetProperty(ref _thisMapButtonText, value); } }

        private string _previousMapButtonText;
        public string PreviousMapButtonText { get { return _previousMapButtonText; } set { SetProperty(ref _previousMapButtonText, value); } }

        private bool _previousMapButtonIsEnabled;
        public bool PreviousMapButtonIsEnabled { get { return _previousMapButtonIsEnabled; } set { SetProperty(ref _previousMapButtonIsEnabled, value); } }

        private string _nextMapButtonText;
        public string NextMapButtonText { get { return _nextMapButtonText; } set { SetProperty(ref _nextMapButtonText, value); } }

        private string _cancelButtonText;
        public string CancelButtonText { get { return _cancelButtonText; } set { SetProperty(ref _cancelButtonText, value); } }

        private string _loadCityButtonText;
        public string LoadCityButtonText { get { return _loadCityButtonText; } set { SetProperty(ref _loadCityButtonText, value); } }

        private DelegateCommand _thisMapCommand;
        public DelegateCommand ThisMapCommand { get { return _thisMapCommand; } set { SetProperty(ref _thisMapCommand, value); } }

        private DelegateCommand _previousMapCommand;
        public DelegateCommand PreviousMapCommand { get { return _previousMapCommand; } set { SetProperty(ref _previousMapCommand, value); } }

        private DelegateCommand _nextMapCommand;
        public DelegateCommand NextMapCommand { get { return _nextMapCommand; } set { SetProperty(ref _nextMapCommand, value); } }

        private DelegateCommand _loadCityCommand;
        public DelegateCommand LoadCityCommand { get { return _loadCityCommand; } set { SetProperty(ref _loadCityCommand, value); } }

        private DelegateCommand _cancelCommand;
        private OverlayMapViewModel _mapPaneViewModel;
        private MainGamePageViewModel _mainPageViewModel;
        private TelemetryClient _telemetry;
        public DelegateCommand CancelCommand { get { return _cancelCommand; } set { SetProperty(ref _cancelCommand, value); } }
        public MainGamePageViewModel MainPageViewModel { get { return _mainPageViewModel; } set
        {
            _mainPageViewModel = value; 
            CancelCommand = new DelegateCommand(() =>
            {
                MainPageViewModel.HideNewGameDialogPanel();
                OnCancelClicked();
            });
            ThisMapCommand = new DelegateCommand(() =>
            {
                MainPageViewModel.HideNewGameDialogPanel();
                OnPlayClicked();
            });
        } }

        /// <summary>
        /// Called when user clicked previous map button to go to the preview map.
        /// </summary>
        private void OnPreviousMapClicked()
        {
            _telemetry.TrackEvent("NewCityDialogPreviousMapClicked");

            if (_previousMaps.Count == 0)
                return;

            _nextMaps.Push(_engine);
            _engine = _previousMaps.Pop();
            _mapPaneViewModel.SetEngine(_engine);

            PreviousMapButtonIsEnabled = _previousMaps.Count != 0;
        }
        
        /// <summary>
        /// Called when user clicked next map button to go to next map.
        /// </summary>
        private void OnNextMapClicked()
        {
            _telemetry.TrackEvent("NewCityDialogNextMapClicked");

            if (_nextMaps.Count == 0)
            {
                var m = new Engine.Micropolis();
                new MapGenerator(m).GenerateNewCity();
                _nextMaps.Push(m);
            }

            _previousMaps.Push(_engine);
            _engine = _nextMaps.Pop();
            _mapPaneViewModel.SetEngine(_engine);

            PreviousMapButtonIsEnabled = true;
        }

        /// <summary>
        /// Called when user clicked load button to load a city from disk.
        /// </summary>
        private async void OnLoadCityClicked()
        {
            _telemetry.TrackEvent("NewCityDialogLoadMapClicked");
            try
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".cty");
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var newEngine = new Engine.Micropolis();
                    Stream stream = await file.OpenStreamForReadAsync();
                    newEngine.LoadFile(stream);
                    StartPlaying(newEngine, file);
                    MainPageViewModel.HideNewGameDialogPanel();
                }
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog(Strings.GetString("main.error_caption") + e);
                dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Start to play the specified file with the specified engine.
        /// </summary>
        /// <param name="newEngine">engine to play with</param>
        /// <param name="file">file to load and play</param>
        private void StartPlaying(Engine.Micropolis newEngine, StorageFile file)
        {
            MainGamePageViewModel win = _mainPageViewModel;
            win.SetEngine(newEngine);
            win.CurrentFile = file;
            win.MakeClean();
        }

        /// <summary>
        /// Called when user clicked the play button to play the currently visible map.
        /// </summary>
        private void OnPlayClicked()
        {
            _telemetry.TrackEvent("NewCityDialogPlayMapClicked");

            _engine.SetGameLevel(GetSelectedGameLevel());
            _engine.SetFunds(GameLevel.GetStartingFunds(_engine.GameLevel));
            StartPlaying(_engine, null);
            MainGamePageViewModel win = _mainPageViewModel;
            win.OnDifficultyClicked(GetSelectedGameLevel());
        }

        /// <summary>
        /// Called when user clicked cancel button to abort the new game dialog.
        /// </summary>
        private void OnCancelClicked()
        {
            _telemetry.TrackEvent("NewCityDialogCancelMapClicked");

            MainGamePageViewModel win = _mainPageViewModel;
            win.HideNewGameDialogPanel();
        }

        /// <summary>
        /// Gets the selected level
        /// </summary>
        /// <returns>selected level</returns>
        private int GetSelectedGameLevel()
        {
            foreach (int lev in _levelBtns.Keys)
            {
                if (_levelBtns[lev].IsChecked)
                {
                    return lev;
                }
            }
            return GameLevel.MIN_LEVEL;
        }

        /// <summary>
        /// Sets game level
        /// </summary>
        /// <param name="level">level to set engine to</param>
        private void SetGameLevel(int level)
        {
            _telemetry.TrackEvent("NewCityDialogSelectLevel"+level);

            foreach (int lev in _levelBtns.Keys)
            {
                _levelBtns[lev].IsChecked = (lev == level);
            }
        }
    }
}
