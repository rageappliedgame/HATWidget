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
    class ScenarioSummaryWindow : Window
    {
        private ListView scenarioListView;

        ScenarioRatingsWindow parentWindow;

        public ScenarioSummaryWindow(ScenarioRatingsWindow parentWindow, ObservableCollection<GameplayElem> gameplayElem) {
            this.parentWindow = parentWindow;

            Title = "Scenario Summary";

            init(gameplayElem);

            Show();
        }

        private void init(ObservableCollection<GameplayElem> gameplayElemList) {
            Closing += doClosingStuff;
            initUI(initResources(gameplayElemList));
        }

        private ObservableCollection<ScenarioSummaryElem> initResources(ObservableCollection<GameplayElem> gameplayElemList) {
            ObservableCollection<ScenarioSummaryElem> ssElemList = new ObservableCollection<ScenarioSummaryElem>();

            foreach(GameplayElem gameplayElem in gameplayElemList){
                double currRating = 0;
                double currAccuracy = 0;
                double currRT = 0;

                if(!Double.TryParse(gameplayElem.ScenarioRating, out currRating)
                    || !Double.TryParse(gameplayElem.Accuracy, out currAccuracy)
                    || !Double.TryParse(gameplayElem.RT, out currRT)){
                    // [TODO]
                }

                bool existsFlag = false;

                foreach(ScenarioSummaryElem ssElem in ssElemList){
                    if (ssElem.ScenarioID.Equals(gameplayElem.ScenarioID)) {
                        ssElem.PlayCount++;
                        ssElem.ScenarioRating += currRating;
                        ssElem.Accuracy += currAccuracy;
                        ssElem.RT += currRT;

                        existsFlag = true;
                        break;
                    }
                }

                if (!existsFlag) {
                    ssElemList.Add(
                        new ScenarioSummaryElem {
                            ScenarioID = gameplayElem.ScenarioID
                            , PlayCount = 1
                            , ScenarioRating = currRating
                            , Accuracy = currAccuracy
                            , RT = currRT
                        }
                    );
                }
            }

            foreach (ScenarioSummaryElem ssElem in ssElemList) {
                ssElem.ScenarioRating = Math.Round(ssElem.ScenarioRating / ssElem.PlayCount, 3);
                ssElem.Accuracy = Math.Round(ssElem.Accuracy / ssElem.PlayCount, 3);
                ssElem.RT = Math.Round(ssElem.RT / ssElem.PlayCount, 0);
            }

            return ssElemList;
        }

        private void initUI(ObservableCollection<ScenarioSummaryElem> ssElemList) {
            StackPanel mainPanel = new StackPanel {
                Orientation = Orientation.Vertical
                , Margin = new Thickness{
                    Top = Cfg.MAIN_PANEL_TOP_MARGIN
                    , Bottom = Cfg.MAIN_PANEL_BOTTOM_MARGIN
                    , Right = Cfg.MAIN_PANEL_RIGHT_MARGIN
                    , Left = Cfg.MAIN_PANEL_LEFT_MARGIN
                }
            };

            // [TODO] better organization is possible
            List<PropertyElem> gameplayPropertyList = new List<PropertyElem> { 
                new PropertyElem { PropertyName = Data.Cfg.scenarioIDNode, PropertyValue = Data.Cfg.scenarioIDNode }
                , new PropertyElem { PropertyName = Data.Cfg.playCountNode, PropertyValue = Data.Cfg.playCountNode }
                , new PropertyElem { PropertyName = Data.Cfg.scenarioRatingNode, PropertyValue = Data.Cfg.scenarioRatingNode }
                , new PropertyElem { PropertyName = Data.Cfg.accuracyNode, PropertyValue = Data.Cfg.accuracyNode }
                , new PropertyElem { PropertyName = Data.Cfg.rtNode, PropertyValue = Data.Cfg.rtNode }
            };

            GridView gameplayGridView = new GridView { AllowsColumnReorder = true };
            foreach (PropertyElem propertyElem in gameplayPropertyList){
                // [SC] adding columns with headers and data binding
                GridViewColumn column = new GridViewColumn{
                    DisplayMemberBinding = new Binding(propertyElem.PropertyName)
                    , Header = propertyElem.PropertyValue
                    , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
                };
                gameplayGridView.Columns.Add(column);
            }

            scenarioListView = new ListView {
                Height = Cfg.LISTBOX_HEIGHT
                , HorizontalAlignment = HorizontalAlignment.Left
                , SelectedIndex = -1
            };
            scenarioListView.View = gameplayGridView;
            scenarioListView.ItemsSource = ssElemList;
            scenarioListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(new SorterHelper(scenarioListView).GridViewColumnHeaderClickedHandler));

            Button closeBtn = new Button {
                Name = Cfg.CLOSE_BTN_NAME
                , Content = Cfg.CLOSE_BTN_COTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Center
                , Margin = new Thickness { Top = Cfg.ITEM_MARGIN }
            };
            closeBtn.Click += new RoutedEventHandler(closeBtnClick);

            mainPanel.Children.Add(scenarioListView);
            mainPanel.Children.Add(closeBtn);

            this.Content = mainPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void closeBtnClick(object source, RoutedEventArgs e) {
            this.Close();
        }

        private void doClosingStuff(object source, CancelEventArgs e) {
            parentWindow.unregisterChildWindow(this);
        }
    }
}
