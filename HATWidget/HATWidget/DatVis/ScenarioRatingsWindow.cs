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

using System.Diagnostics; // [TEMP]

using System.Windows; // Thickness
using System.Windows.Controls; // StackPanel, DockPanel, Border
using System.Windows.Data; // Binding
using System.Collections.ObjectModel; // ObservableCollection
using System.ComponentModel; // CancelEventArgs

using HATWidget.DS;

namespace HATWidget.DatVis {
    class ScenarioRatingsWindow : Window
    {
        private DataVisWindow dvWindow;

        private ComboBox gameCB;

        private ComboBox modeCB;
        private ListView scenarioListView;

        private Button showSummaryBtn;

        private GraphPanel graphPanel;

        List<Window> openWindows;

        public ScenarioRatingsWindow(DataVisWindow dvWindow)
            : base()
        {
            this.dvWindow = dvWindow;

            Title = "Scenario Ratings";

            init();

            Show();
        }

        private void init() {
            initUI();
            initResources();
            Closing += doClosingStuff;
        }

        private void initUI() {
            DockPanel mainDockPanel = new DockPanel();
            mainDockPanel.LastChildFill = true;

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] left panel
            Border leftBorder = new Border();
            DockPanel.SetDock(leftBorder, Dock.Left);

            StackPanel controlPanel = new StackPanel {
                Orientation = Orientation.Vertical
            };

            Label gameLabel = new Label { Content = "Select game:" }; //[TODO]
            gameCB = new ComboBox {
                SelectedIndex = -1
                , Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN }
            };
            gameCB.SelectionChanged += new SelectionChangedEventHandler(gameCBEventHandler);

            // [SC] select player
            Label modeLabel = new Label { Content = "Select mode:" }; // [TODO]
            modeCB = new ComboBox {
                SelectedIndex = -1
                , ItemsSource = new List<string> { "All", "Individual" } // [TODO]
                , Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN }
            };
            modeCB.SelectionChanged += new SelectionChangedEventHandler(modeCBEventHandler);


            Label scenarioLabel = new Label { Content = "Select scenario:" }; // [TODO]

            GridView scenarioGridView = new GridView();
            GridViewColumn scenarioIDColumn = new GridViewColumn {
                DisplayMemberBinding = new Binding(Cfg.SCENARIO_ID_BINDING)
                , Header = Cfg.SCENARIO_ID_LABEL
                , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
            };
            scenarioGridView.Columns.Add(scenarioIDColumn);
            GridViewColumn scenarioDescrColumn = new GridViewColumn {
                DisplayMemberBinding = new Binding(Cfg.SCENARIO_DESCR_BINDING)
                , Header = Cfg.SCENARIO_DESCR_LABEL
                , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
            };
            scenarioGridView.Columns.Add(scenarioDescrColumn);

            scenarioListView = new ListView {
                Width = Cfg.LISTBOX_WIDTH
                , Height = Cfg.LISTBOX_HEIGHT
                , HorizontalAlignment = HorizontalAlignment.Left
                , Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN }
                , SelectedIndex = -1
                , IsEnabled = false
            };
            scenarioListView.View = scenarioGridView;
            scenarioListView.SelectionChanged += new SelectionChangedEventHandler(scenarioListSelectionHandler);

            showSummaryBtn = new Button {
                Name = Cfg.SHOW_GAMEPLAY_SUMMARY_BTN_NAME
                , Content = Cfg.SHOW_GAMEPLAY_SUMMARY_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness { Top = Cfg.ITEM_MARGIN }
                , IsEnabled = false
            };
            showSummaryBtn.Click += new RoutedEventHandler(showSummaryBtnClick);

            controlPanel.Children.Add(gameLabel);
            controlPanel.Children.Add(gameCB);
            controlPanel.Children.Add(modeLabel);
            controlPanel.Children.Add(modeCB);
            controlPanel.Children.Add(scenarioLabel);
            controlPanel.Children.Add(scenarioListView);
            controlPanel.Children.Add(showSummaryBtn);

            leftBorder.Child = controlPanel;
            mainDockPanel.Children.Add(leftBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] right panel
            Border rightBorder = new Border();
            DockPanel.SetDock(rightBorder, Dock.Right);

            graphPanel = new GraphPanel();
            rightBorder.Child = graphPanel;
            mainDockPanel.Children.Add(rightBorder);

            this.Content = mainDockPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void initResources() {
            openWindows = new List<Window>();
            updateGameListView();
        }

        private void updateGameListView() {
            gameCB.ItemsSource = dvWindow.getAllGameIDAdapt();
            gameCB.SelectedIndex = -1;
        }

        private void updateScenarioListView(string gameID) {
            scenarioListView.ItemsSource = dvWindow.getAllScenariosAdapt(gameID);
            scenarioListView.SelectedIndex = -1;
        }

        private void drawScenarioRatings(string gameID) {
            ObservableCollection<ScenarioElem> scenariosData = dvWindow.getAllScenariosDataAdapt(gameID);
            List<Point> rawPoints = new List<Point>();
            List<string> xTickLabels = new List<string>();
            for (int xPoint = 0; xPoint < scenariosData.Count; xPoint++) {
                rawPoints.Add(new Point { X = xPoint + 1, Y = scenariosData[xPoint].ScenarioRating });
                xTickLabels.Add(scenariosData[xPoint].ScenarioID);
            }
            graphPanel.drawGraph(rawPoints, xTickLabels, Cfg.SCENARIO_RATING_COLOR, true);
        }

        private void gameCBEventHandler(object sender, SelectionChangedEventArgs e) {
            ComboBox source = sender as ComboBox;

            if (source == gameCB) {
                if (gameCB.SelectedIndex == -1) {
                    modeCB.IsEnabled = false;
                    showSummaryBtn.IsEnabled = false;
                }
                else {
                    updateScenarioListView(gameCB.SelectedItem as string);
                    showSummaryBtn.IsEnabled = true;
                    modeCB.IsEnabled = true;
                }
                modeCB.SelectedIndex = -1;
                graphPanel.clearCanvas();
            }
        }

        private void modeCBEventHandler(object sender, SelectionChangedEventArgs e) {
            ComboBox source = sender as ComboBox;

            if (source == modeCB) {
                if (modeCB.SelectedIndex == -1) {
                    scenarioListView.IsEnabled = false;
                }
                else {
                    if ((modeCB.SelectedItem as string).Equals("All")) {
                        scenarioListView.IsEnabled = false;
                        drawScenarioRatings(gameCB.SelectedItem as string);
                    }
                    else {
                        scenarioListView.IsEnabled = true;
                        graphPanel.clearCanvas();
                    }
                }
            }
        }

        private void scenarioListSelectionHandler(object sender, SelectionChangedEventArgs e) {
            ListView source = sender as ListView;
            if (source == scenarioListView) {
                // [SC] if true then selection was cleared
                if (scenarioListView.SelectedIndex == -1) {
                    // [SC] clear graphPanel
                    graphPanel.clearCanvas();
                }
                else {
                    string gameID = gameCB.SelectedItem as string;
                    ScenarioElem scenarioElem = source.SelectedItem as ScenarioElem;
                    graphPanel.drawGraph(dvWindow.getScenarioRatingChanges(gameID, scenarioElem.ScenarioID), null, Cfg.SCENARIO_RATING_COLOR, true);
                }
            }
        }

        private void showSummaryBtnClick(object source, RoutedEventArgs e) {
            string gameID = gameCB.SelectedItem as string;
            ScenarioElem scenarioElem = scenarioListView.SelectedItem as ScenarioElem;

            ObservableCollection<GameplayElem> gameplayElemList = dvWindow.getGameplaysLog(gameID);

            if (gameplayElemList == null || gameplayElemList.Count == 0) {
                Cfg.showMsg("Null or empty gameplay list.");
                return;
            }

            registerChildWindow(new ScenarioSummaryWindow(this, gameplayElemList));
        }

        private void registerChildWindow(Window childWindow) {
            openWindows.Add(childWindow);
        }

        public void unregisterChildWindow(Window childWindow) {
            openWindows.Remove(childWindow);
        }

        private void closeAllChildWindows() {
            foreach (Window childWindow in openWindows) {
                childWindow.Close();
            }
        }

        private bool hasOpenWindow() {
            return openWindows.Count > 0;
        }

        private void doClosingStuff(object sender, CancelEventArgs e) {
            dvWindow.unregisterChildWindow(this);
        }
    }
}
