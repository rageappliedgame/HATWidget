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


using System.ComponentModel; // CancelEventArgs
using System.Windows.Controls; // DockPanel, Border, StackPanel
using System.Windows; // Window

using HATWidget.DS;

namespace HATWidget.DataEditor
{
    class AddScenarioWindow : Window
    {
        private ScenarioEditorWindow parentWindow;

        private Button addBtn;
        private Button cancelBtn;

        private TextBox scenarioIDTB;

        private List<PropertyElem> propertyList; // [TODO]
        private List<TextBox> propertyValTBList;

        public AddScenarioWindow(ScenarioEditorWindow parentWindow)
            : base()
        {
            this.parentWindow = parentWindow;

            Title = "Add new scenario";

            init();

            Show();
        }

        private void init() {
            initResources();

            initUI();

            Closing += doClosingStuff;
        }

        private void initResources() {
            propertyValTBList = new List<TextBox>();

            propertyList = new List<PropertyElem>();
            // [TODO] default values
            // [TEST] constant names for property elems
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.ratingNode, PropertyValue = "0.01" });
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.playCountNode, PropertyValue = "0" });
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.kFctNode, PropertyValue = "0.0075" });
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.uNode, PropertyValue = "1.0" });
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.lastPlayedNode, PropertyValue = "2015-07-22T11:56:17" });
            propertyList.Add(new PropertyElem { PropertyName = HATWidget.Data.Cfg.timeLimitNode, PropertyValue = "900000" });
        }

        private void initUI() {
            StackPanel mainPanel = new StackPanel{
                Orientation = Orientation.Vertical
                , Margin = new Thickness {
                    Top = Cfg.MAIN_PANEL_TOP_MARGIN
                    , Bottom = Cfg.MAIN_PANEL_BOTTOM_MARGIN
                    , Right = Cfg.MAIN_PANEL_RIGHT_MARGIN
                    , Left = Cfg.MAIN_PANEL_LEFT_MARGIN
                }
            };

            StackPanel propertyPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Label scenarioIDL = new Label {
                Content = Cfg.SCENARIO_ID_LABEL
                , Width = Cfg.MAX_LABEL_WIDTH
                , Margin = new Thickness {  Right = Cfg.ITEM_MARGIN }
                , HorizontalAlignment = HorizontalAlignment.Left
            };

            scenarioIDTB = new TextBox {
                Width = Cfg.PATH_TEXTBOX_WIDTH // [TODO]
                , HorizontalAlignment = HorizontalAlignment.Left
            };

            propertyPanel.Children.Add(scenarioIDL);
            propertyPanel.Children.Add(scenarioIDTB);
            mainPanel.Children.Add(propertyPanel);

            foreach (PropertyElem propertyElem in propertyList) {
                propertyPanel = new StackPanel { Orientation = Orientation.Horizontal };

                Label propertyNameL = new Label {
                    Content = propertyElem.PropertyName
                    , Width = Cfg.MAX_LABEL_WIDTH
                    , Margin = new Thickness {  Right = Cfg.ITEM_MARGIN }
                    , HorizontalAlignment = HorizontalAlignment.Left
                };

                TextBox propertyValueTB = new TextBox {
                    Name = propertyElem.PropertyName
                    , Text = propertyElem.PropertyValue
                    , Width = Cfg.PATH_TEXTBOX_WIDTH // [TODO]
                    , HorizontalAlignment = HorizontalAlignment.Left
                };

                propertyValTBList.Add(propertyValueTB);

                propertyPanel.Children.Add(propertyNameL);
                propertyPanel.Children.Add(propertyValueTB);
                mainPanel.Children.Add(propertyPanel);
            }

            ////////////////////////////////////////////////////////
            ////// adding buttons
            StackPanel btnPanel = new StackPanel {
                Orientation = Orientation.Horizontal
                , Margin = new Thickness { Top = Cfg.PANEL_TOP_MARGIN }
            };

            addBtn = new Button { 
                Name = Cfg.ADD_BTN_NAME
                , Content = Cfg.ADD_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
                , Margin = new Thickness { Right = Cfg.ITEM_MARGIN }
            };
            addBtn.Click += new RoutedEventHandler(addBtnClick);

            cancelBtn = new Button { 
                Name = Cfg.CANCEL_BTN_NAME
                , Content = Cfg.CANCEL_BTN_CONTENT
                , Width = Cfg.BUTTON_WIDTH
                , HorizontalAlignment = HorizontalAlignment.Right
            };
            cancelBtn.Click += new RoutedEventHandler(cancelBtnClick);

            btnPanel.Children.Add(addBtn);
            btnPanel.Children.Add(cancelBtn);
            mainPanel.Children.Add(btnPanel);

            this.Content = mainPanel;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        // [TODO] require anay verification or control logic?
        private void addBtnClick(object sender, RoutedEventArgs e) {
            propertyList.Clear();

            foreach (TextBox propertyTB in propertyValTBList) {
                propertyList.Add(new PropertyElem { PropertyName = propertyTB.Name, PropertyValue = propertyTB.Text });
            }

            // [SC] if true then new scenario data was successfully added and stored
            if (parentWindow.addScenario(scenarioIDTB.Text, propertyList)) {
                parentWindow.updateScenarioListView();
                this.Close();
            }
        }

        private void cancelBtnClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        // [SC] handler for window close event
        private void doClosingStuff(object sender, CancelEventArgs e) {
            parentWindow.Show(); // [SC] show the parent window which was hidden
        }
    }
}
