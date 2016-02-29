//
// Copyright (c) 2016 Enkhbold Nyamsuren (http://www.bcogs.net , http://www.bcogs.info/)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel; // ObservableCollection
using System.ComponentModel; // CancelEventArgs
using Microsoft.Win32; // SaveFileDialogue
using System.IO; // Path

using System.Diagnostics; // [TEMP] remove for the release version

using HATWidget.DS;
using HATWidget.Data;
using HATWidget.DataEditor;
using HATWidget.DatVis;

namespace HATWidget 
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        public MainWindow() {
            InitializeComponent();

            Title = "HAT - Analytical Widget";

            init();

            Show();
        }

        private void init() {
            initUI();
            Closing += doClosingStuff;
        }

        private void initUI() {
            StackPanel mainPanel = new StackPanel {
                Orientation = Orientation.Vertical
                , Margin = new Thickness {
                    Top = Cfg.MAIN_PANEL_TOP_MARGIN
                    , Bottom = Cfg.MAIN_PANEL_BOTTOM_MARGIN
                    , Right = Cfg.MAIN_PANEL_RIGHT_MARGIN
                    , Left = Cfg.MAIN_PANEL_LEFT_MARGIN
                }
            };

            mainPanel.Children.Add(
                new Label { 
                    Content = "Choose the tool you want to use."
                    , HorizontalAlignment = HorizontalAlignment.Center
                }
            );

            StackPanel btnPanel = new StackPanel { 
                Orientation = Orientation.Horizontal 
                , HorizontalAlignment = HorizontalAlignment.Center
                , Margin = new Thickness { Top = Cfg.PANEL_TOP_MARGIN }
            };

            Button callDataManagerBtn = new Button {
                Name = Cfg.CALL_DATA_MANAGER_BTN_NAME
                , Content = Cfg.CALL_DATA_MANAGER_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Center
                , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
            };
            callDataManagerBtn.Click += new RoutedEventHandler(callDataManagerBtnClick);

            Button callDataVisBtn = new Button {
                Name = Cfg.CALL_DATA_VIS_BTN_NAME
                , Content = Cfg.CALL_DATA_VIS_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Center
            };
            callDataVisBtn.Click += new RoutedEventHandler(callDataVisBtnClick);

            btnPanel.Children.Add(callDataManagerBtn);
            btnPanel.Children.Add(callDataVisBtn);
            mainPanel.Children.Add(btnPanel);

            this.Content = mainPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void callDataManagerBtnClick(object sender, RoutedEventArgs e) {
            Hide();
            try {
                new DataManagementWindow(this);
            }
            catch (Exception) {
                Show();
            }
        }

        public void callDataVisBtnClick(object sender, RoutedEventArgs e) {
            Hide();
            try {
                new DataVisWindow(this);
            }
            catch (Exception) {
                Show();
            }
        }

        public void doClosingStuff(object sender, CancelEventArgs e) {
            // [TODO] is there anything to do here?
        }
    }
}
