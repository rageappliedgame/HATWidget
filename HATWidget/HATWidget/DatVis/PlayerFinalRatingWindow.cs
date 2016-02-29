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
    class PlayerFinalRatingWindow : Window
    {
        private DataVisWindow dvWindow;

        private ComboBox gameCB;

        private ListView playerListView;

        private GraphPanel graphPanel;

        public PlayerFinalRatingWindow(DataVisWindow dvWindow) 
            : base()
        {
            this.dvWindow = dvWindow;

            Title = "Player Final Rating";

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
                , SelectedIndex = -1
                , IsEnabled = false
            };
            playerListView.View = playerGridView;
            playerListView.SelectionChanged += new SelectionChangedEventHandler(playerListSelectionHandler);

            controlPanel.Children.Add(gameLabel);
            controlPanel.Children.Add(gameCB);
            controlPanel.Children.Add(playerLabel);
            controlPanel.Children.Add(playerListView);

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
            updateGameListView();
        }

        private void updateGameListView() {
            gameCB.ItemsSource = dvWindow.getAllGameIDAdapt();
            gameCB.SelectedIndex = -1;
        }

        private void updatePlayerListView(string gameID) {
            playerListView.ItemsSource = dvWindow.getAllPlayersAdapt(gameID);
            playerListView.SelectedIndex = -1;
        }

        private void gameCBEventHandler(object sender, SelectionChangedEventArgs e) {
            ComboBox source = sender as ComboBox;

            if (source == gameCB && gameCB.SelectedIndex != -1) {
                updatePlayerListView(gameCB.SelectedItem as string);
                playerListView.IsEnabled = true;
            }
        }

        private void playerListSelectionHandler(object sender, SelectionChangedEventArgs e) {
            ListView source = sender as ListView;
            if (source == playerListView) {
                // [SC] if true then selection was cleared
                if (playerListView.SelectedIndex == -1) {
                    // [SC] clear graphPanel
                    graphPanel.clearCanvas();
                }
                else {
                    string gameID = gameCB.SelectedItem as string;
                    PlayerElem playerElem = source.SelectedItem as PlayerElem;

                    double playerRating;
                    if(!Double.TryParse(dvWindow.getPlayerRatingAdapt(gameID, playerElem.PlayerID), out playerRating)){
                        Cfg.showMsg("Cannot retrieve player's rating.");
                        return;
                    }

                    double meanPlayersRating;
                    if (!Double.TryParse(dvWindow.getMeanPlayersRatingAdapt(gameID), out meanPlayersRating)) {
                        Cfg.showMsg("Cannot calculate group mean rating.");
                        return;
                    }

                    graphPanel.drawBarplot(
                        new List<double> { playerRating, meanPlayersRating }
                        , new List<string> {"Player rating", "Group mean rating" }
                        , new List<Color> { Cfg.PLAYER_RATING_COLOR, Cfg.MEAN_PLAYERS_RATING_COLOR }
                        , 0);
                }
            }
        }

        private void doClosingStuff(object sender, CancelEventArgs e) {
            dvWindow.unregisterChildWindow(this);
        }
    }
}
