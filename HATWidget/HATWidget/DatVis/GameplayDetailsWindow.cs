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

using System.Windows; // Thickness
using System.Windows.Controls; // StackPanel, DockPanel, Border
using System.Windows.Data; // Binding
using System.Windows.Media; // Color
using System.Collections.ObjectModel; // ObservableCollection
using System.ComponentModel; // CancelEventArgs

using System.Diagnostics;

using HATWidget.DS;

namespace HATWidget.DatVis 
{
    class GameplayDetailsWindow : Window
    {
        private ListView gameplayListView;

        LearningCurveWindow parentWindow;

        public GameplayDetailsWindow(LearningCurveWindow parentWindow, ObservableCollection<GameplayElem> gameplayElemList) {
            this.parentWindow = parentWindow;

            Title = "Gameplay Details";

            init(gameplayElemList);

            Show();
        }

        private void init(ObservableCollection<GameplayElem> gameplayElemList) {
            Closing += doClosingStuff;
            initResources(gameplayElemList);
            initUI(gameplayElemList);
        }

        private void initResources(ObservableCollection<GameplayElem> gameplayElemList) {
            double prevRating = 0;
            bool prevRatingFlag = false;
            foreach (GameplayElem gameplayElem in gameplayElemList) { 
                double currRating;
                if(Double.TryParse(gameplayElem.PlayerRating, out currRating)) { // [TODO] if false declare error?
                    if (prevRatingFlag) {
                        if (prevRating >= currRating) gameplayElem.RatingTrend = Data.Cfg.ratingTrendDown;
                        else gameplayElem.RatingTrend = Data.Cfg.ratingTrendUp;
                    }
                    else {
                        prevRatingFlag = true;
                        gameplayElem.RatingTrend = Data.Cfg.ratingTrendUp;
                    }
                    prevRating = currRating;
                }
            }
        }

        private void initUI(ObservableCollection<GameplayElem> gameplayElemList) {
            StackPanel mainPanel = new StackPanel {
                Orientation = Orientation.Vertical
                , Margin = new Thickness{
                    Top = Cfg.MAIN_PANEL_TOP_MARGIN
                    , Bottom = Cfg.MAIN_PANEL_BOTTOM_MARGIN
                    , Right = Cfg.MAIN_PANEL_RIGHT_MARGIN
                    , Left = Cfg.MAIN_PANEL_LEFT_MARGIN
                }
            };

            // [TODO] better coding should be possible
            List<PropertyElem> gameplayPropertyList = new List<PropertyElem>{
                new PropertyElem { PropertyName = Data.Cfg.scenarioIDNode, PropertyValue = Data.Cfg.scenarioIDNode }
                , new PropertyElem { PropertyName = Data.Cfg.timestampNode, PropertyValue = Data.Cfg.timestampNode }
                , new PropertyElem { PropertyName = Data.Cfg.playerRatingNode, PropertyValue = Data.Cfg.playerRatingNode }
                , new PropertyElem { PropertyName = Data.Cfg.accuracyNode, PropertyValue = Data.Cfg.accuracyNode }
                , new PropertyElem { PropertyName = Data.Cfg.rtNode, PropertyValue = Data.Cfg.rtNode }
                , new PropertyElem { PropertyName = Data.Cfg.ratingTrendNode, PropertyValue = Data.Cfg.ratingTrendNode }
            };

            GridView gameplayGridView = new GridView();
            foreach (PropertyElem propertyElem in gameplayPropertyList) {
                GridViewColumn column = new GridViewColumn{
                    DisplayMemberBinding = new Binding(propertyElem.PropertyName)
                    , Header = propertyElem.PropertyValue
                    , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
                };
                gameplayGridView.Columns.Add(column);
            }

            gameplayListView = new ListView {
                Height = Cfg.LISTBOX_HEIGHT
                , HorizontalAlignment = HorizontalAlignment.Left
                , SelectedIndex = -1
            };
            gameplayListView.View = gameplayGridView;
            gameplayListView.ItemsSource = gameplayElemList;
            gameplayListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(new SorterHelper(gameplayListView).GridViewColumnHeaderClickedHandler));

            Button closeBtn = new Button{
                Name = Cfg.CLOSE_BTN_NAME
                , Content = Cfg.CLOSE_BTN_COTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Center
                , Margin = new Thickness { Top = Cfg.ITEM_MARGIN }
            };
            closeBtn.Click += new RoutedEventHandler(closeBtnClick);

            mainPanel.Children.Add(gameplayListView);
            mainPanel.Children.Add(closeBtn);

            this.Content = mainPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;

            Style style = new Style();
            style.TargetType = typeof(ListViewItem);
            DataTrigger trigger = new DataTrigger();
            trigger.Binding = new Binding(Data.Cfg.ratingTrendNode);
            trigger.Value = Data.Cfg.ratingTrendDown;
            trigger.Setters.Add(new Setter(ListViewItem.ForegroundProperty, Brushes.Red));
            style.Triggers.Add(trigger);
            gameplayListView.ItemContainerStyle = style;
        }

        private void closeBtnClick(object source, RoutedEventArgs e) {
            this.Close();
        }

        private void doClosingStuff(object sender, CancelEventArgs e) {
            parentWindow.unregisterChildWindow(this);
        }
    }
}
