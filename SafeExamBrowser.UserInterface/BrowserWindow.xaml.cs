﻿/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.UserInterface
{
	public partial class BrowserWindow : Window, IBrowserWindow
	{
		private bool isMainWindow;
		private IBrowserSettings settings;
		public WindowClosingHandler closing;

		public bool IsMainWindow
		{
			get
			{
				return isMainWindow;
			}
			set
			{
				isMainWindow = value;
				ApplySettings();
			}
		}

		public event AddressChangedHandler AddressChanged;
		public event ActionRequestedHandler BackwardNavigationRequested;
		public event ActionRequestedHandler ForwardNavigationRequested;
		public event ActionRequestedHandler ReloadRequested;

		event WindowClosingHandler IWindow.Closing
		{
			add { closing += value; }
			remove { closing -= value; }
		}

		public BrowserWindow(IBrowserControl browserControl, IBrowserSettings settings)
		{
			this.settings = settings;

			InitializeComponent();
			InitializeBrowserWindow(browserControl);
		}

		public void BringToForeground()
		{
			if (WindowState == WindowState.Minimized)
			{
				WindowState = WindowState.Normal;
			}

			Activate();
		}

		public void UpdateAddress(string url)
		{
			Dispatcher.Invoke(() =>
			{
				UrlTextBox.TextChanged -= UrlTextBox_TextChanged;
				UrlTextBox.Text = url;
				UrlTextBox.TextChanged += UrlTextBox_TextChanged;
			});
		}

		public void UpdateTitle(string title)
		{
			Dispatcher.Invoke(() => Title = title);
		}

		private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			AddressChanged?.Invoke(UrlTextBox.Text);
		}

		private void InitializeBrowserWindow(IBrowserControl browserControl)
		{
			if (browserControl is System.Windows.Forms.Control)
			{
				BrowserControlHost.Child = browserControl as System.Windows.Forms.Control;
			}

			Closing += (o, args) => closing?.Invoke();
			UrlTextBox.TextChanged += UrlTextBox_TextChanged;
			ReloadButton.Click += (o, args) => ReloadRequested?.Invoke();
			BackButton.Click += (o, args) => BackwardNavigationRequested?.Invoke();
			ForwardButton.Click += (o, args) => ForwardNavigationRequested?.Invoke();

			ApplySettings();
		}

		private void ApplySettings()
		{
			if (IsMainWindow && settings.FullScreenMode)
			{
				MaxHeight = SystemParameters.WorkArea.Height;
				ResizeMode = ResizeMode.NoResize;
				WindowState = WindowState.Maximized;
				WindowStyle = WindowStyle.None;
			}

			UrlTextBox.IsEnabled = settings.AllowAddressBar;

			ReloadButton.IsEnabled = settings.AllowReloading;
			ReloadButton.Visibility = settings.AllowReloading ? Visibility.Visible : Visibility.Collapsed;

			BackButton.IsEnabled = settings.AllowBackwardNavigation;
			BackButton.Visibility = settings.AllowBackwardNavigation ? Visibility.Visible : Visibility.Collapsed;

			ForwardButton.IsEnabled = settings.AllowForwardNavigation;
			ForwardButton.Visibility = settings.AllowForwardNavigation ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
