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

using System.Windows.Media; // Color

using System.Reflection;

namespace HATWidget
{
    class Cfg
    {
        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Main Window

        // [SC] name bindings
        public const string CALL_DATA_MANAGER_BTN_NAME = "callDataManager";
        public const string CALL_DATA_VIS_BTN_NAME = "callDataVis";

        // [SC] labels
        public const string CALL_DATA_MANAGER_BTN_CONTENT = "Start Data Manager";
        public const string CALL_DATA_VIS_BTN_CONTENT = "Start Data Visualizer";

        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Data Management - Main window
        
        // [SC] name bindings
        public const string CREATE_ADPT_FILE_BTN_NAME = "createAdaptFile";
        public const string CREATE_LOG_FILE_BTN_NAME = "createLogFile";
        public const string CREATE_SETTINGS_FILE_BTN_NAME = "createSettingsFile";
        public const string SELECT_ADAPT_DATAPATH_BTN_NAME = "selectAdaptDatapath";
        public const string SELECT_LOG_DATAPATH_BTN_NAME = "selectLogDatapath";
        public const string MANAGE_SCENARIOS_BTN_NAME = "manageScenarios";
        public const string MANAGE_PLAYERS_BTN_NAME = "managePlayers";
        public const string REMOVE_GAME_BTN_NAME = "removeGame";
        public const string ADD_GAME_BTN_NAME = "addGame";

        public const string GAME_NAME_BINDING = "GameName";
        public const string GAME_ID_BINDING = "GameID";

        // [SC] labels
        public const string CREATE_ADPT_FILE_BTN_CONTENT = "Create Adaptation File";
        public const string CREATE_LOG_FILE_BTN_CONTENT = "Create Log File";
        public const string CREATE_SETTINGS_FILE_BTN_CONTENT = "Create Settings File";
        public const string SELECT_ADAPT_DATAPATH_BTN_CONTENT = "Select Adaptation File";
        public const string SELECT_LOG_DATAPATH_BTN_CONTENT = "Select Log File";
        public const string MANAGE_SCENARIOS_BTN_CONTENT = "Manage Scenarios";
        public const string MANAGE_PLAYERS_BTN_CONTENT = "Manage Players";
        public const string REMOVE_GAME_BTN_CONTENT = "Remove Game";
        public const string ADD_GAME_BTN_CONTENT = "Add Game";

        public const string GAME_ID_LABEL = "Game ID";
        public const string GAME_NAME_LABEL = "Game Name";

        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Data Management - Scenario editor window
        // [SC] name bindings
        public const string ADD_SCENARIO_BTN_NAME = "addScenario";
        public const string EDIT_SCENARIO_BTN_NAME = "editScenario";
        public const string REMOVE_SCENARIO_BTN_NAME = "removeScenario";

        public const string SAVE_CHANGES_BTN_NAME = "saveChanges";
        public const string CANCEL_CHANGES_BTN_NAME = "cancelChanges";

        public const string ADD_BTN_NAME = "add";
        public const string CANCEL_BTN_NAME = "cancel";

        public const string SCENARIO_ID_BINDING = "ScenarioID";
        public const string SCENARIO_DESCR_BINDING = "Description";

        public const string PROPERTY_NAME_BINDING = "PropertyName";
        public const string PROPERTY_VALUE_BINDING = "PropertyValue";

        // [SC] labels
        public const string ADD_SCENARIO_BTN_CONTENT = "Add Scenario";
        public const string EDIT_SCNEARIO_BTN_CONTENT = "Edit Scenario";
        public const string REMOVE_SCENARIO_BTN_CONTENT = "Remove Scenario";

        public const string SAVE_CHANGES_BTN_CONTENT = "Save Changes";
        public const string CANCEL_CHANGES_BTN_CONTENT = "Cancel Changes";

        public const string ADD_BTN_CONTENT = "Add";
        public const string CANCEL_BTN_CONTENT = "Cancel";

        public const string SCENARIO_ID_LABEL = "Scenario ID";
        public const string SCENARIO_DESCR_LABEL = "Description";

        public const string PROPERTY_NAME_LABEL = "Property";
        public const string PROPERTY_VALUE_LABEL = "Value";

        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Data Management - Player editor window

        // [SC] name bindings
        public const string ADD_PLAYER_BTN_NAME = "addPlayer";
        public const string EDIT_PLAYER_BTN_NAME = "editPlayer";
        public const string REMOVE_PLAYER_BTN_NAME = "removePlayer";

        public const string PLAYER_ID_BINDING = "PlayerID";
        public const string PLAYER_DESCR_BINDING = "Description";

        // [SC] labels
        public const string ADD_PLAYER_BTN_CONTENT = "Add Player";
        public const string EDIT_PLAYER_BTN_CONTENT = "Edit Player";
        public const string REMOVE_PLAYER_BTN_CONTENT = "Remove Player";

        public const string PLAYER_ID_LABEL = "Player ID";
        public const string PLAYER_DESCR_LABEL = "Description";


        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Data Management - UI layout constants

        ///////////////////////////////////////////////////////////////////////////
        ////// [SC] Learning curve - UI layout constants

        // [SC] name bindings
        public const string SHOW_GAMEPLAY_SUMMARY_BTN_NAME = "showGameplaySummary";
        public const string SHOW_GAMEPLAY_DETAILS_BTN_NAME = "showGameplayDetails";
        public const string CLOSE_BTN_NAME = "close";
        
        // [SC] labels
        public const string SHOW_GAMEPLAY_SUMMARY_BTN_CONTENT = "Show Gameplay Summary";
        public const string SHOW_GAMEPLAY_DETAILS_BTN_CONTENT = "Show Gameplay Details";
        public const string CLOSE_BTN_COTENT = "Close";



        public const int DRAW_AREA_MARGIN = 150;

        public const int CANVAS_WIDTH = 900; // [SC] canvas width in pixels
        public const int CANVAS_HEIGHT = 600; // [SC] canvas height in pixels

        public const int X_AXIS_LABEL_MARGIN = -20;
        public const int Y_AXIS_LABEL_MARGIN = -20;
        public const int X_AXIS_TICK_MARGIN = 0;
        public const int Y_AXIS_TICK_MARGIN = -50;
        public const int AXIS_OFFSET = 10; // [TODO]
        public const int X_AXIS_OFFSET = 20;

        public static Color CANVAS_BG = Colors.LightCyan;
        public static Color LINE_COLOR = Colors.Black;
        public static Color TEXT_COLOR = Colors.Black;
        public static Color PLAYER_RATING_COLOR = Colors.Green;
        public static Color MEAN_PLAYERS_RATING_COLOR = Colors.Blue;
        public static Color ALARM_POINT_COLOR = Colors.Red;
        public static Color SCENARIO_RATING_COLOR = Colors.Green;
        public const int LINE_WIDTH = 1;

        public const int PCH_WIDTH = 10;
        
        public const int BARPLOT_BAR_WIDTH = 50; // [SC] this values is in pixels
        public const int BARPLOT_BAR_SPACING = 5; // [SC] this value is in pixels

        public const int LEGEND_LABEL_VERT_SPACING = 5; // [SC] this value is in pixels

        public const int GRID_VIEW_COLUMN_WIDTH = 200;

        public const int PROPERTY_PANEL_WIDTH = 400;
        public const int PROPERTY_PANEL_HEIGHT = 300;

        public const int LISTBOX_WIDTH = 200;
        public const int LISTBOX_HEIGHT = 300;

        public const int BUTTON_WIDTH = 150;

        public const int MAX_LABEL_WIDTH = 100;

        public const int TEXTBOX_WIDTH = 200;
        public const int PATH_TEXTBOX_WIDTH = 400;

        public const int PANEL_RIGHT_MARGIN = 50;
        public const int PANEL_TOP_MARGIN = 20;
        public const int PANEL_BOTTOM_MARGIN = 20;

        public const int ITEM_MARGIN = 10;

        public const int MAIN_PANEL_TOP_MARGIN = 20;
        public const int MAIN_PANEL_BOTTOM_MARGIN = MAIN_PANEL_TOP_MARGIN;
        public const int MAIN_PANEL_LEFT_MARGIN = MAIN_PANEL_TOP_MARGIN;
        public const int MAIN_PANEL_RIGHT_MARGIN = MAIN_PANEL_TOP_MARGIN;


        public const string NO_UI_NO_DATA_MSG = "Cannot show data. UI was not initialized.";
        public const string NO_DMANAGER_NO_DATA_MSG = "Cannot load data. Data Manager was not instantiated.";

        public const string NULL_STRING_MSG = "String is NULL";
        public const string EMPTY_STRING_MSG = "String is empty.";
        public const string INVALID_STR_FORMAT_MSG = "Invalid format.";

        public const string NULL_GAME_ID_MSG = "Invalid game ID. " + NULL_STRING_MSG;
        public const string EMPTY_GAME_ID_MSG = "Invalid game ID. " + EMPTY_STRING_MSG;
        // [TODO] public const string DUPLICATE_GAME_ID_MSG = "A game with given ID already exists. ID should unique.";
        public const string INVALID_GAME_ID_FORMAT_MSG = "Invalid game ID format. " + INVALID_STR_FORMAT_MSG;

        public const string NULL_GAME_NAME_MSG = "Invalid game name. " + NULL_STRING_MSG;
        public const string EMPTY_GAME_NAME_MSG = "Invalid game name. " + EMPTY_STRING_MSG;
        public const string INVALID_GAME_NAME_FORMAT_MSG = "Invalid game name format. " + INVALID_STR_FORMAT_MSG;

        public const string SELECT_GAME_MSG = "Select a game from the list.";

        public const string SELECT_SCENARIO_MSG = "Select a scenario from the list.";

        public const string SELECT_PLAYER_MSG = "Select a player from the list.";

        public const string LOAD_ADAPT_LOG_FILES_MSG = "Load both adaptation and log XML files.";
        

        public static void showMsg(string msg) {
            System.Windows.MessageBox.Show(msg);
        }

        /*
        /// <summary>
        /// Takes the full name of a resource and loads it in to a stream.
        /// </summary>
        /// <param name="resourceName">Assuming an embedded resource is a file
        /// called info.png and is located in a folder called Resources, it
        /// will be compiled in to the assembly with this fully qualified
        /// name: Full.Assembly.Name.Resources.info.png. That is the string
        /// that you should pass to this method.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(string resourceName) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Get the list of all emdedded resources in the assembly.
        /// </summary>
        /// <returns>An array of fully qualified resource names</returns>
        public static string[] GetEmbeddedResourceNames() {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
        */
    }
}
