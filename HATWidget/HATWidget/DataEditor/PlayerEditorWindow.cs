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

using System.Windows.Data; // Binding
using System.Windows.Controls; // CockPane, Border, StackPanel
using System.Collections.ObjectModel; // ObservableCollection
using System.ComponentModel; // CancelEventArgs
using System.Windows; // Window

using HATWidget.Data;
using HATWidget.DS;

namespace HATWidget.DataEditor
{
    class PlayerEditorWindow : Window
    {
        private DataManagementWindow dmWindow;

        private ListView playerListView;
        private Button addPlayerBtn;
        private Button removePlayerBtn;

        private StackPanel propertyPanel;
        private List<TextBox> propertyValTBList;

        private Button editPlayerBtn;
        private Button saveChangesBtn;
        private Button cancelChangesBtn;

        private String gameID; // [TODO]
        private String adaptID; // [TODO]

        private DataManager dataManager;

        public PlayerEditorWindow(DataManagementWindow dmWindow, String adaptID, string gameID, DataManager dataManager) 
            : base()
        {
            this.dmWindow = dmWindow;
            this.adaptID = adaptID;
            this.gameID = gameID;
            this.dataManager = dataManager;

            Title = "HAT Widget - Player Editor";

            init();

            Show();
        }

        private void init() {
            initUI();

            initResources();

            Closing += doClosingStuff;
        }

        private void initUI() { 
            DockPanel mainDockPanel = new DockPanel{
                LastChildFill = true
                , Margin = new Thickness {
                    Top = Cfg.MAIN_PANEL_TOP_MARGIN
                    , Bottom = Cfg.MAIN_PANEL_BOTTOM_MARGIN
                    , Right = Cfg.MAIN_PANEL_RIGHT_MARGIN
                    , Left = Cfg.MAIN_PANEL_LEFT_MARGIN
                }
            };

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init top panel
            Border topBorder = new Border();
            DockPanel.SetDock(topBorder, Dock.Top);

            Label gameName = new Label { Content = "Game ID: " + gameID };
            topBorder.Child = gameName;

            mainDockPanel.Children.Add(topBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init left panel
            Border leftBorder = new Border();
            DockPanel.SetDock(leftBorder, Dock.Left);

            StackPanel playerListPanel = new StackPanel();
            playerListPanel.Orientation = Orientation.Vertical;
            playerListPanel.Margin = new Thickness { Right = Cfg.PANEL_RIGHT_MARGIN };

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
            };
            playerListView.View = playerGridView;
            playerListView.SelectionChanged += new SelectionChangedEventHandler(playerListSelectionHandler);

            addPlayerBtn = new Button {
                Name = Cfg.ADD_PLAYER_BTN_NAME
                , Content = Cfg.ADD_PLAYER_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN }
            };
            addPlayerBtn.Click += new RoutedEventHandler(addPlayerBtnClick);

            removePlayerBtn = new Button {
                Name = Cfg.REMOVE_PLAYER_BTN_NAME
                , Content = Cfg.REMOVE_PLAYER_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
            };
            removePlayerBtn.Click += new RoutedEventHandler(removePlayerBtnClick);

            playerListPanel.Children.Add(playerListView);
            playerListPanel.Children.Add(addPlayerBtn);
            playerListPanel.Children.Add(removePlayerBtn);

            leftBorder.Child = playerListPanel;
            mainDockPanel.Children.Add(leftBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init right panel
            Border rightBorder = new Border();
            DockPanel.SetDock(rightBorder, Dock.Right);

            StackPanel rightPanel = new StackPanel { Orientation = Orientation.Vertical };

            propertyPanel = new StackPanel {
                Orientation = Orientation.Vertical
                , Width = Cfg.PROPERTY_PANEL_WIDTH
                , Height = Cfg.PROPERTY_PANEL_HEIGHT
                , Margin = new Thickness { Bottom = Cfg.PANEL_BOTTOM_MARGIN }
            };

            StackPanel rightBtnPanel = new StackPanel {
                Orientation = Orientation.Horizontal
                , HorizontalAlignment = HorizontalAlignment.Right
            };

            editPlayerBtn = new Button {
                IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Right
                , Name = Cfg.EDIT_PLAYER_BTN_NAME
                , Content = Cfg.EDIT_PLAYER_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
            };
            editPlayerBtn.Click += new RoutedEventHandler(editPlayerBtnClick);

            saveChangesBtn = new Button {
                IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness { Left = Cfg.ITEM_MARGIN }
                , Name = Cfg.SAVE_CHANGES_BTN_NAME
                , Content = Cfg.SAVE_CHANGES_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
            };
            saveChangesBtn.Click += new RoutedEventHandler(saveChangesBtnClick);

            cancelChangesBtn = new Button {
                IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness { Left = Cfg.ITEM_MARGIN }
                , Name = Cfg.CANCEL_CHANGES_BTN_NAME
                , Content = Cfg.CANCEL_CHANGES_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
            };
            cancelChangesBtn.Click += new RoutedEventHandler(cancelChangesBtnClick);

            rightBtnPanel.Children.Add(editPlayerBtn);
            rightBtnPanel.Children.Add(saveChangesBtn);
            rightBtnPanel.Children.Add(cancelChangesBtn);

            rightPanel.Children.Add(propertyPanel);
            rightPanel.Children.Add(rightBtnPanel);

            rightBorder.Child = rightPanel;
            mainDockPanel.Children.Add(rightBorder);

            /////////////////////////////////////////////////////////////////
            ////// [SC] fill main window
            this.Content = mainDockPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void initResources() {
            propertyValTBList = new List<TextBox>();

            if (playerListView == null) Cfg.showMsg(Cfg.NO_UI_NO_DATA_MSG);
            else if (dataManager == null) Cfg.showMsg(Cfg.NO_DMANAGER_NO_DATA_MSG);
            else updatePlayerListView();
        }

        public bool updatePropertyPanel(string playerID) {
            // [SC] make sure that UI panel was initialized propertly
            if (propertyPanel == null) {
                Cfg.showMsg(Cfg.NO_UI_NO_DATA_MSG);
                return false;
            }

            clearPropertyPanel();

            ObservableCollection<PropertyElem> propertyList = dataManager.getPlayerProperties(adaptID, gameID, playerID);

            if (propertyList.Count == 0) return false;

            foreach (PropertyElem propertyElem in propertyList) {
                StackPanel subPanel = new StackPanel();
                subPanel.Orientation = Orientation.Horizontal;

                Label propertyNameL = new Label {
                    Content = propertyElem.PropertyName
                    , Width = Cfg.MAX_LABEL_WIDTH
                    , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
                    , HorizontalAlignment = HorizontalAlignment.Left
                };

                TextBox propertyValueTB = new TextBox {
                    Name = propertyElem.PropertyName
                    , Text = propertyElem.PropertyValue
                    , Width = Cfg.PATH_TEXTBOX_WIDTH // [TODO]
                    , IsEnabled = false
                    , HorizontalAlignment = HorizontalAlignment.Left
                };

                propertyValTBList.Add(propertyValueTB);

                subPanel.Children.Add(propertyNameL);
                subPanel.Children.Add(propertyValueTB);

                propertyPanel.Children.Add(subPanel);
            }

            return true;
        }

        public void clearPropertyPanel() {
            propertyPanel.Children.Clear();
            propertyValTBList.Clear();
        }

        public void updatePlayerListView() {
            playerListView.ItemsSource = dataManager.getAllPlayers(adaptID, gameID);
            playerListView.SelectedIndex = -1;
        }

        public bool addPlayer(string playerID, List<PropertyElem> propertyList) {
            return dataManager.addPlayer(adaptID, gameID, playerID, propertyList); // [TODO] make sure the method is in DataManager
        }

        ///////////////////////////////////////////////////////////////////////////
        ////// START: Button and ListView event handler
        #region Button and ListView event handler

        // [TEST]
        private void playerListSelectionHandler(object sender, SelectionChangedEventArgs e) {
            ListView source = sender as ListView;
            if (source == playerListView) {
                // [SC] if true then selection was cleared
                if (playerListView.SelectedIndex == -1) {
                    clearPropertyPanel();
                    editPlayerBtn.IsEnabled = false;
                    saveChangesBtn.IsEnabled = false;
                    cancelChangesBtn.IsEnabled = false;
                }
                else {
                    PlayerElem playerElem = source.SelectedItem as PlayerElem;
                    if (updatePropertyPanel(playerElem.PlayerID)) {
                        editPlayerBtn.IsEnabled = true;
                        saveChangesBtn.IsEnabled = false;
                        cancelChangesBtn.IsEnabled = false;
                    }
                }
            }
        }

        private void addPlayerBtnClick(object sender, RoutedEventArgs e) {
            Hide();
            try {
                new AddPlayerWindow(this);
            }
            catch (Exception) {
                Show();
            }
        }

        private void removePlayerBtnClick(object sender, RoutedEventArgs e) {
            if (playerListView.SelectedIndex == -1) {
                Cfg.showMsg(Cfg.SELECT_PLAYER_MSG);
                return;
            }

            // [TODO] what if playerID  is a null or empty string
            PlayerElem playerElem = playerListView.SelectedItem as PlayerElem;

            // [TODO] is there anything to do if removal was unsuccessful?
            if (dataManager.removePlayer(adaptID, gameID, playerElem.PlayerID)) updatePlayerListView();
        }

        // [TEST]
        private void editPlayerBtnClick(object sender, RoutedEventArgs e) {
            foreach(TextBox propertyValTB in propertyValTBList){
                propertyValTB.IsEnabled = true;
            }

            editPlayerBtn.IsEnabled = false;
            saveChangesBtn.IsEnabled = true;
            cancelChangesBtn.IsEnabled = true;

            addPlayerBtn.IsEnabled = false;
            removePlayerBtn.IsEnabled = false;
        }

        private void saveChangesBtnClick(object sender, RoutedEventArgs e) {
            PlayerElem playerElem = playerListView.SelectedItem as PlayerElem;

            List<PropertyElem> propertyList = new List<PropertyElem>();

            foreach (TextBox propertyValTB in propertyValTBList) {
                propertyList.Add(new PropertyElem { PropertyName = propertyValTB.Name, PropertyValue = propertyValTB.Text });
            }

            if (dataManager.setPlayerProperties(adaptID, gameID, playerElem.PlayerID, propertyList)) {
                // [SC] disable all property text boxes
                foreach(TextBox propertyValTB in propertyValTBList) propertyValTB.IsEnabled = false;

                // [SC] change states of all buttons
                editPlayerBtn.IsEnabled = true;
                saveChangesBtn.IsEnabled = false;
                cancelChangesBtn.IsEnabled = false;
                addPlayerBtn.IsEnabled = true;
                removePlayerBtn.IsEnabled = true;
            }
        }

        private void cancelChangesBtnClick(object sender, RoutedEventArgs e) {
            PlayerElem playerElem = playerListView.SelectedItem as PlayerElem;

            updatePropertyPanel(playerElem.PlayerID);

            editPlayerBtn.IsEnabled = true;
            saveChangesBtn.IsEnabled = false;
            cancelChangesBtn.IsEnabled = false;
            addPlayerBtn.IsEnabled = true;
            removePlayerBtn.IsEnabled = true;
        }

        #endregion Button and ListView event handler
        ////// END: Button and ListView event handlers
        ///////////////////////////////////////////////////////////////////////////

        private void doClosingStuff(object sender, CancelEventArgs e) {
            dmWindow.Show(); // [SC] show the main window which was hidden
        }
    }
}
