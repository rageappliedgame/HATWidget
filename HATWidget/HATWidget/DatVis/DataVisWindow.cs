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
using System.ComponentModel; // CancelEventArgs
using System.Windows.Controls; // StackPanel, Border, DockPanel
using System.Collections.ObjectModel; // ObservableCollection
using Microsoft.Win32; // SaveFileDialogue

using HATWidget.Data;
using HATWidget.DS;

namespace HATWidget.DatVis {
    
    // [SC] on which items the player is performing bad and good
    // [SC] player's rating compared to group mean
    // [SC] players rating chnage
    // [SC] how many time player played scenario and percentage of successes

    class DataVisWindow : Window
    {
        private MainWindow mainWindow;

        private DataManager dataManager;
        private bool adaptLoadedFlag = false;
        private bool logLoadedFlag = false;
        
        private LogDataManager logManager;


        private TextBox adaptDatapathTB;
        private TextBox logDatapathTB;

        private List<Window> openWindows;

        public DataVisWindow(MainWindow mainWindow)
            : base()
        {
            this.mainWindow = mainWindow;

            Title = "HAT - Data Visualization Widget";

            init();

            Show();
        }

        private void init() {
            initResources();

            initUI();

            Closing += doClosingStuff;
        }

        private void initResources() {
            dataManager = new DataManager();
            logManager = new LogDataManager();

            openWindows = new List<Window>();
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

            StackPanel topPanel = new StackPanel { Orientation = Orientation.Vertical };
            
            StackPanel adaptPathPanel = new StackPanel {
                Orientation = Orientation.Horizontal
                , Margin = new Thickness { Bottom = Cfg.PANEL_BOTTOM_MARGIN }
            };
            adaptDatapathTB = new TextBox { 
                Width = Cfg.PATH_TEXTBOX_WIDTH
                , IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Left
                , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
            };
            Button selectAdaptPathBtn = new Button {
                Name = Cfg.SELECT_ADAPT_DATAPATH_BTN_NAME
                , Content = Cfg.SELECT_ADAPT_DATAPATH_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Left
            };
            selectAdaptPathBtn.Click += new RoutedEventHandler(selectAdaptPathBtnClick);
            adaptPathPanel.Children.Add(adaptDatapathTB);
            adaptPathPanel.Children.Add(selectAdaptPathBtn);
            topPanel.Children.Add(adaptPathPanel);

            StackPanel logPathPanel = new StackPanel { Orientation = Orientation.Horizontal };
            logDatapathTB = new TextBox {
                Width = Cfg.PATH_TEXTBOX_WIDTH
                , IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Left
                , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
            };
            Button selectLogPathBtn = new Button {
                Name = Cfg.SELECT_LOG_DATAPATH_BTN_NAME
                , Content = Cfg.SELECT_LOG_DATAPATH_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Left
            };
            selectLogPathBtn.Click += new RoutedEventHandler(selectLogPathBtnClick);
            logPathPanel.Children.Add(logDatapathTB);
            logPathPanel.Children.Add(selectLogPathBtn);
            topPanel.Children.Add(logPathPanel);

            topPanel.Children.Add(new Separator() {
                Margin = new Thickness { Top = Cfg.PANEL_TOP_MARGIN, Bottom = Cfg.PANEL_BOTTOM_MARGIN }
            });

            topBorder.Child = topPanel;
            mainDockPanel.Children.Add(topBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] bottom panel
            /// grid - Column 1: available analysis buttons; Column 2: description of analysis

            Border bottomBorder = new Border();
            DockPanel.SetDock(bottomBorder, Dock.Bottom);

            StackPanel bottomPanel = new StackPanel { Orientation = Orientation.Vertical };

            List<AnalysisElem> alsList = new List<AnalysisElem>();
            alsList.Add(new AnalysisElem {AnalysisID="learningCurve", AnalysisName="Player Learning Curve", Description="Shows player's learning curve as a function of rating changing over time."}); // [TODO]
            alsList.Add(new AnalysisElem {AnalysisID="scenarioRatings",AnalysisName="Scenario Difficulty Ratings", Description="Difficulty ratings of game scenarios."}); // [TODO]
            //alsList.Add(new AnalysisElem {AnalysisID = "finalRating", AnalysisName = "Player Final Rating", Description="Player's rating compared to group average."}); // [TODO]

            foreach (AnalysisElem alsElem in alsList) {
                StackPanel alsBtnPanel = new StackPanel { Orientation = Orientation.Horizontal };

                Button alsBtn = new Button {
                    Name = alsElem.AnalysisID
                    , Content = alsElem.AnalysisName
                    , Width = Cfg.BUTTON_WIDTH
                    , HorizontalAlignment = HorizontalAlignment.Left
                    , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
                };
                alsBtn.Click += new RoutedEventHandler(alsBtnClick);

                Label alsL = new Label {
                    Content = alsElem.Description
                    , HorizontalAlignment = HorizontalAlignment.Left
                };

                alsBtnPanel.Children.Add(alsBtn);
                alsBtnPanel.Children.Add(alsL);
                bottomPanel.Children.Add(alsBtnPanel);

                bottomPanel.Children.Add(new Separator() {
                    Margin = new Thickness { Top = Cfg.PANEL_TOP_MARGIN, Bottom = Cfg.PANEL_BOTTOM_MARGIN }
                });
            }

            bottomBorder.Child = bottomPanel;
            mainDockPanel.Children.Add(bottomBorder);

            this.Content = mainDockPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public ObservableCollection<string> getAllGameIDAdapt() {
            return dataManager.getAllGameID(Data.Cfg.SKILL_DIFFICULTY);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for players

        public ObservableCollection<PlayerElem> getAllPlayersAdapt(string gameID) {
            return dataManager.getAllPlayers(Data.Cfg.SKILL_DIFFICULTY, gameID);
        }

        public ObservableCollection<string> getAllPlayersIDAdapt(string gameID) {
            return dataManager.getAllPlayerID(Data.Cfg.SKILL_DIFFICULTY, gameID);
        }

        public string getPlayerRatingAdapt(string gameID, string playerID) {
            return dataManager.getPlayerRating(Data.Cfg.SKILL_DIFFICULTY, gameID, playerID);
        }

        public string getMeanPlayersRatingAdapt(string gameID) {
            return dataManager.getMeanPlayersRatings(Data.Cfg.SKILL_DIFFICULTY, gameID);
        }

        public List<Point> getPlayerRatingChanges(string gameID, string playerID) {
            return logManager.getPlayerRatingChanges(Data.Cfg.SKILL_DIFFICULTY, gameID, null, playerID);
        }

        public List<List<Point>> getMeanPlayerRatingChanges(string gameID, int playCount) {
            ObservableCollection<string> playerIDList = getAllPlayersIDAdapt(gameID);
            return logManager.getMeanPlayerRatingChanges(Data.Cfg.SKILL_DIFFICULTY, gameID, playerIDList, playCount);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for scenarios

        public ObservableCollection<ScenarioElem> getAllScenariosAdapt(string gameID) {
            return dataManager.getAllScenarios(Data.Cfg.SKILL_DIFFICULTY, gameID);
        }

        public ObservableCollection<ScenarioElem> getAllScenariosDataAdapt(string gameID) {
            return dataManager.getAllScenariosData(Data.Cfg.SKILL_DIFFICULTY, gameID);
        }

        public List<Point> getScenarioRatingChanges(string gameID, string scenarioID) {
            return logManager.getScenarioRatingChanges(Data.Cfg.SKILL_DIFFICULTY, gameID, scenarioID, null);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        ////// START: method for gameplays

        public ObservableCollection<GameplayElem> getPlayerGameplaysLog(string gameID, string playerID) {
            return logManager.getGameplays(Data.Cfg.SKILL_DIFFICULTY, gameID, null, playerID);
        }

        public ObservableCollection<GameplayElem> getScenarioGameplaysLog(string gameID, string scenarioID) {
            return logManager.getGameplays(Data.Cfg.SKILL_DIFFICULTY, gameID, scenarioID, null);
        }

        public ObservableCollection<GameplayElem> getGameplaysLog(string gameID) {
            return logManager.getGameplays(Data.Cfg.SKILL_DIFFICULTY, gameID, null, null);
        }


        // [TODO]
        //public ObservableCollection<string> getAllGameIDLog() {
        //    return logManager.getAllGameID(Data.Cfg.SKILL_DIFFICULTY);
        //}

        // [TODO]
        //public ObservableCollection<PlayerElem> getAllPlayersLog(string gameID) {
        //    return logManager.getAllPlayers(Data.Cfg.SKILL_DIFFICULTY, gameID);
        //}

        private void selectAdaptPathBtnClick(object sender, RoutedEventArgs e) {
            if (!hasOpenWindow()) {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "XML files (*.xml)|*.xml";
                openDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (openDialog.ShowDialog() == true) {
                    adaptDatapathTB.Text = null;

                    // [SC] check if new data stream was opened successfully
                    if (dataManager.openAdaptStream(openDialog.FileName)) {
                        adaptDatapathTB.Text = openDialog.FileName;

                        // [TODO] check for XML validity against schema

                        adaptLoadedFlag = true;
                    }
                }
            }
            else {
                Cfg.showMsg("Please, close all open windows at first.");
            }
        }

        private void selectLogPathBtnClick(object sender, RoutedEventArgs e) {
            if (!hasOpenWindow()) {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "XML files (*.xml)|*.xml";
                openDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (openDialog.ShowDialog() == true) {
                    logDatapathTB.Text = null;

                    // [SC] check if new data stream was opened successfully
                    if (logManager.openLogSteam(openDialog.FileName)) {
                        logDatapathTB.Text = openDialog.FileName;

                        // [TODO] check for XML validity against schema

                        logLoadedFlag = true;
                    }
                }
            }
            else {
                Cfg.showMsg("Please, close all open windows at first.");
            }
        }

        private void alsBtnClick(object sender, RoutedEventArgs e) {
            // [SC] make sure both adaptation and log XML files were loaded
            if (!(logLoadedFlag && adaptLoadedFlag)) {
                Cfg.showMsg(Cfg.LOAD_ADAPT_LOG_FILES_MSG);
                return;
            }

            Button alsBtn = sender as Button;

            if (alsBtn.Name.Equals("learningCurve")) {
                registerChildWindow(new LearningCurveWindow(this));
            }
            else if (alsBtn.Name.Equals("scenarioRatings")) {
                registerChildWindow(new ScenarioRatingsWindow(this));
            }
            else if (alsBtn.Name.Equals("finalRating")) {
                registerChildWindow(new PlayerFinalRatingWindow(this));
            }
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
            closeAllChildWindows();
            mainWindow.Show();
        }
    }
}
