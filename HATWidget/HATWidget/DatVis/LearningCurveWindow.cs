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

using HATWidget.DS;

namespace HATWidget.DatVis
{
    class LearningCurveWindow : Window
    {
        private DataVisWindow dvWindow;

        private ComboBox gameCB;

        private ListView playerListView;

        private Button showSummaryBtn;
        private Button showDetailsBtn;

        private GraphPanel graphPanel;

        private List<Window> openWindows;

        public LearningCurveWindow(DataVisWindow dvWindow) 
            : base()
        {
            this.dvWindow = dvWindow;

            Title = "Learning Curve";

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
            Label playerLabel = new Label { Content = "Select player:" };

            GridView playerGridView = new GridView();
            GridViewColumn playerIDColumn = new GridViewColumn {
                DisplayMemberBinding = new Binding(Cfg.PLAYER_ID_BINDING)
                , Header = Cfg.PLAYER_ID_LABEL
                , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
            };
            playerGridView.Columns.Add(playerIDColumn);
            GridViewColumn playerDescrColumn = new GridViewColumn {
                DisplayMemberBinding = new Binding(Cfg.PLAYER_DESCR_BINDING)
                , Header = Cfg.PLAYER_DESCR_LABEL
                , Width = Cfg.GRID_VIEW_COLUMN_WIDTH
            };
            playerGridView.Columns.Add(playerDescrColumn);

            playerListView = new ListView {
                Width = Cfg.LISTBOX_WIDTH
                , Height = Cfg.LISTBOX_HEIGHT
                , HorizontalAlignment = HorizontalAlignment.Left
                , Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN }
                , SelectedIndex  = -1
                , IsEnabled = false
            };
            playerListView.View = playerGridView;
            playerListView.SelectionChanged += new SelectionChangedEventHandler(playerListSelectionHandler);

            showSummaryBtn = new Button {
                Name = Cfg.SHOW_GAMEPLAY_SUMMARY_BTN_NAME
                , Content = Cfg.SHOW_GAMEPLAY_SUMMARY_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness{ Top = Cfg.ITEM_MARGIN }
                , IsEnabled = false
            };
            showSummaryBtn.Click += new RoutedEventHandler(showSummaryBtnClick);

            showDetailsBtn = new Button {
                Name = Cfg.SHOW_GAMEPLAY_DETAILS_BTN_NAME
                , Content = Cfg.SHOW_GAMEPLAY_DETAILS_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness{ Top = Cfg.ITEM_MARGIN }
                , IsEnabled = false
            };
            showDetailsBtn.Click += new RoutedEventHandler(showDetailsBtnClick);

            controlPanel.Children.Add(gameLabel);
            controlPanel.Children.Add(gameCB);
            controlPanel.Children.Add(playerLabel);
            controlPanel.Children.Add(playerListView);
            controlPanel.Children.Add(showSummaryBtn);
            controlPanel.Children.Add(showDetailsBtn);

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

        private void updatePlayerListView(string gameID){
            playerListView.ItemsSource = dvWindow.getAllPlayersAdapt(gameID);
            playerListView.SelectedIndex = -1;
        }

        private void gameCBEventHandler(object sender, SelectionChangedEventArgs e) {
            ComboBox source = sender as ComboBox;

            if (source == gameCB && gameCB.SelectedIndex != -1) {
                updatePlayerListView(gameCB.SelectedItem as string);
                playerListView.IsEnabled = true;
                showSummaryBtn.IsEnabled = false;
                showDetailsBtn.IsEnabled = false;
            }
        }

        private void playerListSelectionHandler(object sender, SelectionChangedEventArgs e) {
            ListView source = sender as ListView;
            if (source == playerListView) {
                // [SC] if true then selection was cleared
                if (playerListView.SelectedIndex == -1) {
                    // [SC] clear graphPanel
                    graphPanel.clearCanvas();
                    showSummaryBtn.IsEnabled = false;
                    showDetailsBtn.IsEnabled = false;
                }
                else {
                    string gameID = gameCB.SelectedItem as string;
                    PlayerElem playerElem = source.SelectedItem as PlayerElem;

                    // [SC] getting player's changes in ratings
                    List<Point> playerRatingChanges = dvWindow.getPlayerRatingChanges(gameID, playerElem.PlayerID);

                    // [SC] if no ratings available then do nothing
                    if (playerRatingChanges == null || playerRatingChanges.Count == 0) return;

                    // [SC] color code the points
                    List<Color> colorList = new List<Color>();
                    double prevRating = playerRatingChanges[0].Y;
                    foreach (Point point in playerRatingChanges) {
                        if (point.Y >= prevRating) colorList.Add(Cfg.PLAYER_RATING_COLOR);
                        else colorList.Add(Cfg.ALARM_POINT_COLOR);
                        prevRating = point.Y;
                    }

                    // [SC] plotting player's changes in ratings
                    graphPanel.drawGraph(playerRatingChanges, null, colorList, true);

                    // [SC] getting means of all players' rating changes
                    List<List<Point>> ratingChanges = dvWindow.getMeanPlayerRatingChanges(gameID, playerRatingChanges.Count);
                    // [SC] plotting means on the existing graph
                    graphPanel.addGraph(ratingChanges[1], Cfg.MEAN_PLAYERS_RATING_COLOR, true);
                    // [SC] plotting the lower boundary for standard errors
                    graphPanel.addGraph(ratingChanges[0], Cfg.MEAN_PLAYERS_RATING_COLOR, false);
                    // [SC] plotting the upper boundary for standard errors 
                    graphPanel.addGraph(ratingChanges[2], Cfg.MEAN_PLAYERS_RATING_COLOR, false);

                    graphPanel.addLegend(new List<string> { "Player ratings", "Group mean ratings" }, new List<Color> { Cfg.PLAYER_RATING_COLOR, Cfg.MEAN_PLAYERS_RATING_COLOR });

                    showSummaryBtn.IsEnabled = true;
                    showDetailsBtn.IsEnabled = true;
                }
            }
        }

        private void showSummaryBtnClick(object sender, RoutedEventArgs e) {
            string gameID = gameCB.SelectedItem as string;
            PlayerElem playerElem = playerListView.SelectedItem as PlayerElem;

            ObservableCollection<GameplayElem> gameplayElemList = dvWindow.getPlayerGameplaysLog(gameID, playerElem.PlayerID);

            if (gameplayElemList == null || gameplayElemList.Count == 0) {
                Cfg.showMsg("Null or empty gameplay list ");
                return;
            }

            registerChildWindow(new GameplaySummaryWindow(this, gameplayElemList));
        }

        private void showDetailsBtnClick(object sender, RoutedEventArgs e) {
            string gameID = gameCB.SelectedItem as string;
            PlayerElem playerElem = playerListView.SelectedItem as PlayerElem;

            ObservableCollection<GameplayElem> gameplayElemList = dvWindow.getPlayerGameplaysLog(gameID, playerElem.PlayerID);

            if(gameplayElemList == null || gameplayElemList.Count == 0){
                Cfg.showMsg("Null or empty gameplay list ");
                return;
            }

            registerChildWindow(new GameplayDetailsWindow(this, gameplayElemList));
        }

        private void registerChildWindow(Window childWindow) {
            openWindows.Add(childWindow);
        }

        public void unregisterChildWindow(Window childWindow) {
            openWindows.Remove(childWindow);
        }

        private void closeAllChildWindows () {
            foreach(Window childWindow in openWindows){
                childWindow.Close();
            }
        }

        private bool hasOpenWindow() {
            return openWindows.Count > 0;
        }

        private void doClosingStuff(object sender, CancelEventArgs e) {
            closeAllChildWindows();
            dvWindow.unregisterChildWindow(this);
        }
    }
}
