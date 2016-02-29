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
using System.Windows.Controls; // DockPane, border, StackPanel
using System.Collections.ObjectModel;   // ObservableCollection
using System.ComponentModel; // CancelEventArgs
using System.Windows;  // Window

using HATWidget.Data;
using HATWidget.DS;

namespace HATWidget.DataEditor
{
    class ScenarioEditorWindow : Window
    {

        private DataManagementWindow dmWindow;

        private ListView scenarioListView;
        private Button addScenarioBtn;
        private Button removeScenarioBtn;

        private StackPanel propertyPanel;
        private List<TextBox> propertyValTBList;

        private Button editScenarioBtn;
        private Button saveChangesBtn;
        private Button cancelChangesBtn;
        
        private String gameID; // [TODO]
        private String adaptID; // [TODO]

        private DataManager dataManager;

        public ScenarioEditorWindow(DataManagementWindow dmWindow, String adaptID, String gameID, DataManager dataManager) 
            : base()
        {
            this.dmWindow = dmWindow;
            this.gameID = gameID;
            this.adaptID = adaptID;
            this.dataManager = dataManager;

            Title = "HAT Widget - Scenario editor";

            init();

            Show();
        }

        private void init() {
            initUI();

            initResources();

            Closing += doClosingStuff;
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

            Label gameName = new Label { Content = "Game ID: " + gameID}; // [TODO] add game name?
            topBorder.Child = gameName;

            mainDockPanel.Children.Add(topBorder);

            /////////////////////////////////////////////////////////////////////////
            ////// [SC] init left panel
            Border leftBorder = new Border();
            DockPanel.SetDock(leftBorder, Dock.Left);

            StackPanel scenarioListPanel = new StackPanel();
            scenarioListPanel.Orientation = Orientation.Vertical;
            scenarioListPanel.Margin = new Thickness { Right = Cfg.PANEL_RIGHT_MARGIN };

            GridView scenarioGridView = new GridView();
            GridViewColumn scenarioIDColumn = new GridViewColumn();
            scenarioIDColumn.DisplayMemberBinding = new Binding(Cfg.SCENARIO_ID_BINDING);
            scenarioIDColumn.Header = Cfg.SCENARIO_ID_LABEL;
            scenarioIDColumn.Width = Cfg.GRID_VIEW_COLUMN_WIDTH;
            scenarioGridView.Columns.Add(scenarioIDColumn);
            GridViewColumn scenarioDescrColumn = new GridViewColumn();
            scenarioDescrColumn.DisplayMemberBinding = new Binding(Cfg.SCENARIO_DESCR_BINDING);
            scenarioDescrColumn.Header = Cfg.SCENARIO_DESCR_LABEL;
            scenarioDescrColumn.Width = Cfg.GRID_VIEW_COLUMN_WIDTH;
            scenarioGridView.Columns.Add(scenarioDescrColumn);

            scenarioListView = new ListView();
            scenarioListView.Width = Cfg.LISTBOX_WIDTH; // [TODO]
            scenarioListView.Height = Cfg.LISTBOX_HEIGHT; // [TODO]
            scenarioListView.HorizontalAlignment = HorizontalAlignment.Left;
            scenarioListView.Margin = new Thickness { Bottom = Cfg.ITEM_MARGIN };
            scenarioListView.View = scenarioGridView;
            scenarioListView.SelectedIndex = -1; // [TODO]
            scenarioListView.SelectionChanged += new SelectionChangedEventHandler(scenarioListSelectionHandler);

            addScenarioBtn = new Button {
                Name = Cfg.ADD_SCENARIO_BTN_NAME
                , Content = Cfg.ADD_SCENARIO_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness{ Bottom = Cfg.ITEM_MARGIN }
            };
            addScenarioBtn.Click += new RoutedEventHandler(addScenarioBtnClick);

            removeScenarioBtn = new Button {
                Name = Cfg.REMOVE_SCENARIO_BTN_NAME
                , Content = Cfg.REMOVE_SCENARIO_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
            };
            removeScenarioBtn.Click += new RoutedEventHandler(removeScenarioBtnClick);

            // [STOP][2016.01.14]
            scenarioListPanel.Children.Add(scenarioListView);
            scenarioListPanel.Children.Add(addScenarioBtn);
            scenarioListPanel.Children.Add(removeScenarioBtn);

            leftBorder.Child = scenarioListPanel;
            mainDockPanel.Children.Add(leftBorder);

            /////////////////////////////////////////////////////////////////
            ////// [SC] init right panel
            Border rightBorder = new Border();
            DockPanel.SetDock(rightBorder, Dock.Right);

            StackPanel rightPanel = new StackPanel {
                Orientation = Orientation.Vertical
            };

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

            editScenarioBtn = new Button {
                IsEnabled = false
                , HorizontalAlignment = HorizontalAlignment.Right
                , Name = Cfg.EDIT_SCENARIO_BTN_NAME
                , Content = Cfg.EDIT_SCNEARIO_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
            };
            editScenarioBtn.Click += new RoutedEventHandler(editScenarioBtnClick);

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

            rightBtnPanel.Children.Add(editScenarioBtn);
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

            if (scenarioListView == null) Cfg.showMsg(Cfg.NO_UI_NO_DATA_MSG);
            else if (dataManager == null) Cfg.showMsg(Cfg.NO_DMANAGER_NO_DATA_MSG);
            else updateScenarioListView();
        }

        private bool updatePropertyPanel(string scenarioID) {
            if (propertyPanel == null) {
                Cfg.showMsg(Cfg.NO_UI_NO_DATA_MSG);
                return false;
            }

            clearPropertyPanel();

            ObservableCollection<PropertyElem> propertyList = dataManager.getScenarioProperties(adaptID, gameID, scenarioID);

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
                    // [TODO], IsReadOnly = true
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

        private void clearPropertyPanel(){
            propertyPanel.Children.Clear();
            propertyValTBList.Clear();
        }

        public void updateScenarioListView() {
            scenarioListView.ItemsSource = dataManager.getAllScenarios(adaptID, gameID);
            scenarioListView.SelectedIndex = -1;
        }

        public bool addScenario(string scenarioID, List<PropertyElem> propertyList) {
            // [TODO] make sure scenario with the same ID does not exist
            return dataManager.addScenario(adaptID, gameID, scenarioID, propertyList);
        }

        ///////////////////////////////////////////////////////////////////////////
        ////// START: Button and ListView event handlers
        #region Button and ListView event handlers

        private void scenarioListSelectionHandler(object sender, SelectionChangedEventArgs e) {
            ListView source = sender as ListView;
            if (source == scenarioListView) {
                if (scenarioListView.SelectedIndex == -1) {
                    clearPropertyPanel();
                    editScenarioBtn.IsEnabled = false;
                    saveChangesBtn.IsEnabled = false;
                    cancelChangesBtn.IsEnabled = false;
                }
                else {
                    ScenarioElem scenarioElem = source.SelectedItem as ScenarioElem;
                    if (updatePropertyPanel(scenarioElem.ScenarioID)) {
                        editScenarioBtn.IsEnabled = true;
                        saveChangesBtn.IsEnabled = false;
                        cancelChangesBtn.IsEnabled = false;
                    }
                }
            }
        }

        private void addScenarioBtnClick(object sender, RoutedEventArgs e) {
            Hide();
            try {
                AddScenarioWindow addScenarioWindow = new AddScenarioWindow(this);
            }
            catch (Exception) {
                Show();
            }
        }

        private void removeScenarioBtnClick(object sender, RoutedEventArgs e) {
            if (scenarioListView.SelectedIndex == -1) {
                Cfg.showMsg(Cfg.SELECT_SCENARIO_MSG);
                return;
            }

            // [TODO] what if scenarioID is a null or empty string
            ScenarioElem scenarioElem = scenarioListView.SelectedItem as ScenarioElem;

            // [TODO] anything to do if removal was unsuccessful
            if (dataManager.removeScenario(adaptID, gameID, scenarioElem.ScenarioID)) updateScenarioListView();
        }

        private void editScenarioBtnClick(object sender, RoutedEventArgs e) {
            foreach (TextBox propertyValTB in propertyValTBList) {
                // [TODO] propertyValTB.IsReadOnly = false;
                propertyValTB.IsEnabled = true;
            }

            editScenarioBtn.IsEnabled = false;
            saveChangesBtn.IsEnabled = true;
            cancelChangesBtn.IsEnabled = true;
            addScenarioBtn.IsEnabled = false;
            removeScenarioBtn.IsEnabled = false;
        }

        private void saveChangesBtnClick(object sender, RoutedEventArgs e) {
            ScenarioElem scenarioElem = scenarioListView.SelectedItem as ScenarioElem;

            List<PropertyElem> propertyList = new List<PropertyElem>();

            foreach (TextBox propertyValTB in propertyValTBList) {
                propertyList.Add(new PropertyElem { PropertyName = propertyValTB.Name, PropertyValue = propertyValTB.Text });
            }

            if (dataManager.setScenarioProperties(adaptID, gameID, scenarioElem.ScenarioID, propertyList)) {
                // [SC] disable all property text boxes
                foreach (TextBox propertyValTB in propertyValTBList) propertyValTB.IsEnabled = false; // [TODO] propertyValTB.IsReadOnly = true;
                
                // [SC] change states of all buttons
                editScenarioBtn.IsEnabled = true;
                saveChangesBtn.IsEnabled = false;
                cancelChangesBtn.IsEnabled = false;
                addScenarioBtn.IsEnabled = true;
                removeScenarioBtn.IsEnabled = true;
            }
        }

        private void cancelChangesBtnClick(object sender, RoutedEventArgs e) {
            ScenarioElem scenarioElem = scenarioListView.SelectedItem as ScenarioElem;

            updatePropertyPanel(scenarioElem.ScenarioID);

            editScenarioBtn.IsEnabled = true;
            saveChangesBtn.IsEnabled = false;
            cancelChangesBtn.IsEnabled = false;
            addScenarioBtn.IsEnabled = true;
            removeScenarioBtn.IsEnabled = true;
        }

        #endregion Button and ListView event handlers
        ////// END: Button and ListView event handlers
        ///////////////////////////////////////////////////////////////////////////

        private void doClosingStuff(object sender, CancelEventArgs e) {
            dmWindow.Show(); // [SC] show the main window which was hidden
        }
    }
}
