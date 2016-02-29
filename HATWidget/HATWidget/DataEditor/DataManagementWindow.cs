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

using System.Windows; // Window
using System.Windows.Controls; // DockPanel, border, StackPanel
using System.Windows.Data; // Binding
using System.ComponentModel; // CancelEventArgs
using System.Collections.ObjectModel; // ObservableCollection
using System.IO; // Path
using Microsoft.Win32; // SaveFileDialogue

using System.Diagnostics; // [TEMP] remove for the release version

using HATWidget.DS;
using HATWidget.Data;

namespace HATWidget.DataEditor
{
    class DataManagementWindow : Window
    {
        private MainWindow mainWindow;

        private ListView gameListView;

        private Button createAdaptFileBtn;
        private Button createLogFileBtn;
        private Button createSettingsFileBtn;

        private TextBox selectDatapathTB;
        private Button selectDatapathBtn;

        private Button manageScenariosBtn;
        private Button managePlayersBtn;
        private Button removeGameBtn;
        private Button addGameBtn;

        private TextBox gameIDTB;
        //private TextBox gameNameTB; [TODO]

        private DataManager dataManager;

        public DataManagementWindow(MainWindow mainWindow) 
            : base()
        {
            this.mainWindow = mainWindow;

            Title = "HAT - Data Management Widget";

            init();

            Show();
        }

        // [TODO] 1. check if XML file exists
        //          if not then create an XML file using a hard coded schema
        //          if exists then check if XML structure conforms to the XSD file

        private void init() {
            initResources();
            initUI();
            Closing += doClosingStuff;
        }

        private void initResources() {
            dataManager = new DataManager();
        }

        private void initUI() {
            DockPanel mainDockPanel = new DockPanel {
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

            StackPanel topPanel = new StackPanel();
            topPanel.Orientation = Orientation.Vertical;
            topPanel.Margin = new Thickness { Bottom = Cfg.PANEL_BOTTOM_MARGIN };

            /////////////////////////////////////////////////////////////////////////
            ////// ////// [SC] init top button panel

            StackPanel topButtonPanel = new StackPanel();
            topButtonPanel.Orientation = Orientation.Horizontal;

            createAdaptFileBtn = new Button();
            createAdaptFileBtn.Name = Cfg.CREATE_ADPT_FILE_BTN_NAME;
            createAdaptFileBtn.Content = Cfg.CREATE_ADPT_FILE_BTN_CONTENT;
            createAdaptFileBtn.Width = Cfg.BUTTON_WIDTH;
            createAdaptFileBtn.Click += new RoutedEventHandler(createAdaptFileBtnClick);
            createAdaptFileBtn.HorizontalAlignment = HorizontalAlignment.Left;
            createAdaptFileBtn.Margin = new Thickness { Right = Cfg.ITEM_MARGIN };

            createLogFileBtn = new Button();
            createLogFileBtn.Name = Cfg.CREATE_LOG_FILE_BTN_NAME;
            createLogFileBtn.Content = Cfg.CREATE_LOG_FILE_BTN_CONTENT;
            createLogFileBtn.Width = Cfg.BUTTON_WIDTH;
            createLogFileBtn.Click += new RoutedEventHandler(createLogFileBtnClick);
            createLogFileBtn.HorizontalAlignment = HorizontalAlignment.Left;
            createLogFileBtn.Margin = new Thickness { Right = Cfg.ITEM_MARGIN };

            /*createSettingsFileBtn = new Button(); [TODO]
            createSettingsFileBtn.Name = Cfg.CREATE_SETTINGS_FILE_BTN_NAME;
            createSettingsFileBtn.Content = Cfg.CREATE_SETTINGS_FILE_BTN_CONTENT;
            createSettingsFileBtn.Width = Cfg.BUTTON_WIDTH;
            createSettingsFileBtn.Click += new RoutedEventHandler(createSettingsFileBtnClick);
            createSettingsFileBtn.HorizontalAlignment = HorizontalAlignment.Left;*/

            // [SC] left panel and docking
            topButtonPanel.Children.Add(createAdaptFileBtn);
            topButtonPanel.Children.Add(createLogFileBtn);
            //topButtonPanel.Children.Add(createSettingsFileBtn); [TODO]
            topPanel.Children.Add(topButtonPanel);

            topPanel.Children.Add(new Separator() {
                Margin = new Thickness { Top = Cfg.PANEL_TOP_MARGIN, Bottom = Cfg.PANEL_BOTTOM_MARGIN }
            });

            /////////////////////////////////////////////////////////////////////////
            ////// ////// [SC] init top datapath panel

            StackPanel topDatapathPanel = new StackPanel();
            topDatapathPanel.Orientation = Orientation.Horizontal;

            selectDatapathTB = new TextBox { Width = Cfg.PATH_TEXTBOX_WIDTH };
            selectDatapathTB.IsEnabled = false;
            selectDatapathTB.HorizontalAlignment = HorizontalAlignment.Left;
            selectDatapathTB.Margin = new Thickness { Right = Cfg.ITEM_MARGIN };

            selectDatapathBtn = new Button();
            selectDatapathBtn.Name = Cfg.SELECT_ADAPT_DATAPATH_BTN_NAME;
            selectDatapathBtn.Content = Cfg.SELECT_ADAPT_DATAPATH_BTN_CONTENT;
            selectDatapathBtn.Width = Cfg.BUTTON_WIDTH;
            selectDatapathBtn.Click += new RoutedEventHandler(selectDatapathButtonClick);
            selectDatapathBtn.HorizontalAlignment = HorizontalAlignment.Left;

            topDatapathPanel.Children.Add(selectDatapathTB);
            topDatapathPanel.Children.Add(selectDatapathBtn);
            topPanel.Children.Add(topDatapathPanel);

            topBorder.Child = topPanel;
            mainDockPanel.Children.Add(topBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init left panel

            Border leftBorder = new Border();
            DockPanel.SetDock(leftBorder, Dock.Left);

            StackPanel manageGamePanel = new StackPanel();
            manageGamePanel.Orientation = Orientation.Vertical;
            manageGamePanel.Margin = new Thickness { Right = Cfg.PANEL_RIGHT_MARGIN };

            // [SC] game list view
            GridView gameGridView = new GridView();
            /*GridViewColumn gameNameColumn = new GridViewColumn(); [TODO]
            gameNameColumn.DisplayMemberBinding = new Binding(Cfg.GAME_NAME_BINDING);
            gameNameColumn.Header = Cfg.GAME_NAME_LABEL;
            gameNameColumn.Width = 100; // [TODO]
            gameGridView.Columns.Add(gameNameColumn);*/
            GridViewColumn gameIDColumn = new GridViewColumn();
            gameIDColumn.DisplayMemberBinding = new Binding(Cfg.GAME_ID_BINDING);
            gameIDColumn.Header = Cfg.GAME_ID_LABEL;
            gameIDColumn.Width = 100; // [TODO]
            gameGridView.Columns.Add(gameIDColumn);

            gameListView = new ListView();
            gameListView.Width = Cfg.LISTBOX_WIDTH; // [TODO]
            gameListView.Height = Cfg.LISTBOX_HEIGHT; // [TODO]
            gameListView.HorizontalAlignment = HorizontalAlignment.Left;
            gameListView.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };
            gameListView.View = gameGridView;

            // [SC] adding manage scenarios button
            manageScenariosBtn = new Button();
            manageScenariosBtn.Name = Cfg.MANAGE_SCENARIOS_BTN_NAME;
            manageScenariosBtn.Content = Cfg.MANAGE_SCENARIOS_BTN_CONTENT;
            manageScenariosBtn.Width = Cfg.BUTTON_WIDTH;
            manageScenariosBtn.Click += new RoutedEventHandler(manageScenariosBtnClick);
            manageScenariosBtn.HorizontalAlignment = HorizontalAlignment.Left;
            manageScenariosBtn.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };

            // [SC] adding manage players button
            managePlayersBtn = new Button();
            managePlayersBtn.Name = Cfg.MANAGE_PLAYERS_BTN_NAME;
            managePlayersBtn.Content = Cfg.MANAGE_PLAYERS_BTN_CONTENT;
            managePlayersBtn.Width = Cfg.BUTTON_WIDTH;
            managePlayersBtn.Click += new RoutedEventHandler(managePlayersBtnClick);
            managePlayersBtn.HorizontalAlignment = HorizontalAlignment.Left;
            managePlayersBtn.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };

            // [SC] adding game remove button
            removeGameBtn = new Button();
            removeGameBtn.Name = Cfg.REMOVE_GAME_BTN_NAME;
            removeGameBtn.Content = Cfg.REMOVE_GAME_BTN_CONTENT;
            removeGameBtn.Width = Cfg.BUTTON_WIDTH;
            removeGameBtn.Click += new RoutedEventHandler(removeGameBtnClick);
            removeGameBtn.HorizontalAlignment = HorizontalAlignment.Left;

            // [SC] left panel and docking
            manageGamePanel.Children.Add(gameListView);
            manageGamePanel.Children.Add(manageScenariosBtn);
            manageGamePanel.Children.Add(managePlayersBtn);
            manageGamePanel.Children.Add(removeGameBtn);

            leftBorder.Child = manageGamePanel;
            mainDockPanel.Children.Add(leftBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init right panel

            Border rightBorder = new Border();
            DockPanel.SetDock(rightBorder, Dock.Right);

            StackPanel addGamePanel = new StackPanel();
            addGamePanel.Orientation = Orientation.Vertical;

            // [SC] game ID entry field - horizontal panel
            StackPanel gameIDPanel = new StackPanel();
            gameIDPanel.Orientation = Orientation.Horizontal;
            gameIDPanel.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };
            // [SC] game ID entry field - label
            Label gameIDL = new Label() { Content = Cfg.GAME_ID_LABEL };
            gameIDL.Width = Cfg.MAX_LABEL_WIDTH;
            gameIDL.HorizontalAlignment = HorizontalAlignment.Left;
            gameIDPanel.Children.Add(gameIDL);
            // [SC] game ID entry field - textbox
            gameIDTB = new TextBox();
            gameIDTB.MaxLines = 1;
            gameIDTB.Width = Cfg.TEXTBOX_WIDTH;
            gameIDTB.HorizontalAlignment = HorizontalAlignment.Left;
            gameIDPanel.Children.Add(gameIDTB);

            // [SC] game name entry field - horizontal panel [TODO]
            /*StackPanel gameNamePanel = new StackPanel();
            gameNamePanel.Orientation = Orientation.Horizontal;
            gameNamePanel.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };
            // [SC] game name entry field - label
            Label gameNameL = new Label() { Content = Cfg.GAME_NAME_LABEL };
            gameNameL.Width = Cfg.MAX_LABEL_WIDTH;
            gameNameL.HorizontalAlignment = HorizontalAlignment.Left;
            gameNamePanel.Children.Add(gameNameL);
            // [SC] game name entry field - textbox
            gameNameTB = new TextBox();
            gameNameTB.MaxLines = 1;
            gameNameTB.Width = Cfg.TEXTBOX_WIDTH;
            gameNameTB.HorizontalAlignment = HorizontalAlignment.Left;
            gameNamePanel.Children.Add(gameNameTB);*/

            // [SC] add game button
            addGameBtn = new Button();
            addGameBtn.Name = Cfg.ADD_GAME_BTN_NAME;
            addGameBtn.Content = Cfg.ADD_GAME_BTN_CONTENT;
            addGameBtn.Width = Cfg.BUTTON_WIDTH;
            addGameBtn.HorizontalAlignment = HorizontalAlignment.Left;
            addGameBtn.Click += new RoutedEventHandler(addGameBtnClick);

            addGamePanel.Children.Add(gameIDPanel);
            //addGamePanel.Children.Add(gameNamePanel); [TODO]
            addGamePanel.Children.Add(addGameBtn);

            rightBorder.Child = addGamePanel;
            mainDockPanel.Children.Add(rightBorder);

            /////////////////////////////////////////////////////////////////
            ////// [SC] fill main window

            this.Content = mainDockPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        // [TODO]
        private void enableControls() {
            // [SC] enabling listviews
            gameListView.IsEnabled = true;
            gameListView.SelectedIndex = -1;

            // [SC] enabling textboxes
            gameIDTB.IsEnabled = true;
            gameIDTB.Text = "";

            /*[TODO] gameNameTB.IsEnabled = true;
            gameNameTB.Text = "";*/

            // [SC] enabling buttons
            manageScenariosBtn.IsEnabled = true;
            managePlayersBtn.IsEnabled = true;
            removeGameBtn.IsEnabled = true;
            addGameBtn.IsEnabled = true;
        }

        // [TODO]
        private void disableControls() {
            // [SC] disabling listboxes
            gameListView.IsEnabled = false;
            gameListView.SelectedIndex = -1;

            // [SC] disabling textboxes
            gameIDTB.IsEnabled = false;
            gameIDTB.Text = "";

            /*[TODO] gameNameTB.IsEnabled = false;
            gameNameTB.Text = "";*/

            // [SC] disabling buttons
            manageScenariosBtn.IsEnabled = false;
            managePlayersBtn.IsEnabled = false;
            removeGameBtn.IsEnabled = false;
            addGameBtn.IsEnabled = false;
        }

        ///////////////////////////////////////////////////////////////////////////
        ////// START: Button event handlers
        #region Button event handlers

        private void createAdaptFileBtnClick(object sender, RoutedEventArgs e) {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "XML file (*.xml)|*.xml";
            if (saveDialog.ShowDialog() == true) {
                dataManager.createAdaptXML(saveDialog.FileName);
            }
        }

        private void createLogFileBtnClick(object sender, RoutedEventArgs e) {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "XML file (*.xml)|*.xml";
            if (saveDialog.ShowDialog() == true) {
                dataManager.createLogXML(saveDialog.FileName);
            }
        }

        private void createSettingsFileBtnClick(object sender, RoutedEventArgs e) {
            // [TODO]
            Cfg.showMsg("Clicked create settings.");
        }

        // [TODO] what if another stream is already open
        private void selectDatapathButtonClick(object sender, RoutedEventArgs e) {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML files (*.xml)|*.xml";
            openDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (openDialog.ShowDialog() == true) {
                selectDatapathTB.Text = null;

                // [SC] check if new data stream was opened successfully
                if (dataManager.openAdaptStream(openDialog.FileName)) {
                    selectDatapathTB.Text = openDialog.FileName;

                    // [TODO] check for XML validity against schema

                    gameListView.ItemsSource = dataManager.getAllGames(Data.Cfg.SKILL_DIFFICULTY);
                }
            }
        }

        private void manageScenariosBtnClick(object sender, RoutedEventArgs e) {
            if (gameListView.SelectedIndex == -1) {
                Cfg.showMsg(Cfg.SELECT_GAME_MSG);
                return;
            }

            // [TODO] what if gameID is a null or empty string
            GameElem gameElem = gameListView.SelectedItem as GameElem;

            Hide();
            try {
                new ScenarioEditorWindow(this, Data.Cfg.SKILL_DIFFICULTY, gameElem.GameID, dataManager);
            }
            catch (Exception) {
                Show();
            }
        }

        private void managePlayersBtnClick(object sender, RoutedEventArgs e) {
            if (gameListView.SelectedIndex == -1) {
                Cfg.showMsg(Cfg.SELECT_GAME_MSG);
                return;
            }

            // [TODO] what if gameID is a null or empty value
            GameElem gameElem = gameListView.SelectedItem as GameElem;

            Hide();
            try {
                new PlayerEditorWindow(this, Data.Cfg.SKILL_DIFFICULTY, gameElem.GameID, dataManager);
            }
            catch (Exception) {
                Show();
            }
        }

        private void removeGameBtnClick(object sender, RoutedEventArgs e) {
            if (gameListView.SelectedIndex == -1) {
                Cfg.showMsg(Cfg.SELECT_GAME_MSG);
                return;
            }

            // [TODO] what if gameID is a null or empty string
            GameElem gameElem = gameListView.SelectedItem as GameElem;

            if (dataManager.removeGame(Data.Cfg.SKILL_DIFFICULTY, gameElem.GameID))
                updateGameListView(Data.Cfg.SKILL_DIFFICULTY);
        }

        private void addGameBtnClick(object sender, RoutedEventArgs e) {
            //if (!(validateGameID(gameIDTB.Text) && validateGameName(gameNameTB.Text))) return; [TODO]
            if (!(validateGameID(gameIDTB.Text))) return;

            // [TODO] verify duplicate game ID against listview instead of XML doc?
            //if (dataManager.addGame(Data.Cfg.SKILL_DIFFICULTY, gameIDTB.Text, gameNameTB.Text)) {
            if (dataManager.addGame(Data.Cfg.SKILL_DIFFICULTY, gameIDTB.Text, null)) {
                gameIDTB.Text = null;
                //gameNameTB.Text = null;

                updateGameListView(Data.Cfg.SKILL_DIFFICULTY);
            }
        }

        #endregion Button event handlers
        ////// END: Button event handlers
        ///////////////////////////////////////////////////////////////////////////


        private void updateGameListView(string adaptID) {
            gameListView.ItemsSource = dataManager.getAllGames(Data.Cfg.SKILL_DIFFICULTY);
            gameListView.SelectedIndex = -1;
        }

        private bool validateGameID(string gameID) {
            // [SC] verify for null string
            if (gameID == null) {
                Cfg.showMsg(Cfg.NULL_GAME_ID_MSG);
                return false;
            }

            // [SC] verify for empty string
            if (gameID.Equals("")) {
                Cfg.showMsg(Cfg.EMPTY_GAME_ID_MSG);
                return false;
            }

            // [SC] verify for valid format for game id
            if (!hasValidFormat(gameID)) {
                Cfg.showMsg(Cfg.INVALID_GAME_ID_FORMAT_MSG);
                return false;
            }

            // [TODO][SC] verify for duplicated game id
            //if (gameIDExists(gameID)) {
            //    Cfg.showMsg(Cfg.DUPLICATE_GAME_ID_MSG);
            //    return false;
            //}

            return true;
        }

        /*[TODO] private bool validateGameName(string gameName) {
            // [SC] verify for null string
            if (gameName == null) {
                Cfg.showMsg(Cfg.NULL_GAME_NAME_MSG);
                return false;
            }

            // [SC] verify for empty string
            if (gameName.Equals("")) {
                Cfg.showMsg(Cfg.EMPTY_GAME_NAME_MSG);
                return false;
            }

            // [SC] verify for valid format of game id
            if (!hasValidFormat(gameName)) {
                Cfg.showMsg(Cfg.INVALID_GAME_NAME_FORMAT_MSG);
                return false;
            }

            return true;
        }*/

        private bool hasValidFormat(string strInput) {
            // [TODO] valid id should not contain empty spaces, but game names can
            return true;
        }

        private void doClosingStuff(object sender, CancelEventArgs e) {
            mainWindow.Show(); // [SC] show the main window
        }
    }
}
