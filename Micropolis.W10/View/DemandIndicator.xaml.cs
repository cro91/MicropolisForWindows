﻿using Micropolis.ViewModels;

namespace Micropolis
{
    // This file is part of Micropolis for WinRT.
    // Copyright (C) 2014 Andreas Balzer, Felix Dietrich, Florian Thurnwald and Ivo Vutov
    // Portions Copyright (C) MicropolisJ by Jason Long
    // Portions Copyright (C) Micropolis Don Hopkins
    // Portions Copyright (C) 1989-2007 Electronic Arts Inc.
    //
    // Micropolis for WinRT is free software; you can redistribute it and/or modify
    // it under the terms of the GNU GPLv3, with Additional terms.
    // See the README file, included in this distribution, for details.
    // Project website: http://code.google.com/p/micropolis/

    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;
    using Engine;

    /// <summary>
    ///     Demand indicator visualizing demand for residential, commercial and industrial zones.
    /// </summary>
    public sealed partial class DemandIndicator
    {
        private DemandIndicatorViewModel _viewModel;
        public DemandIndicatorViewModel ViewModel { get { return _viewModel; } }
        /// <summary>
        ///     Initializes a new instance of the <see cref="DemandIndicator" /> class.
        /// </summary>
        public DemandIndicator()
        {
            InitializeComponent();
            _viewModel=new DemandIndicatorViewModel();
            this.DataContext = _viewModel;
        }

    }
}