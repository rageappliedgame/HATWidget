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
using System.Globalization;

namespace HATWidget.Data 
{
    public static class Cfg 
    {
        private static Dictionary<string, string> reservedKeywords;

        // [SC] adaptation XML node references
        public static string adaptationDataNode = "AdaptationData";
        public static string scenarioDataNode = "ScenarioData";
        public static string scenarioNode = "Scenario";
        public static string playerDataNode = "PlayerData";
        public static string playerNode = "Player";

        public static string ratingNode = "Rating";
        public static string playCountNode = "PlayCount";
        public static string kFctNode = "KFactor";
        public static string uNode = "Uncertainty";
        public static string lastPlayedNode = "LastPlayed";
        public static string timeLimitNode = "TimeLimit";

        // [SC] log XML node references
        public static string gameplaysDataNode = "GameplaysData";
        public static string gameplayNode = "Gameplay";
        public static string timestampNode = "Timestamp";
        public static string rtNode = "RT";
        public static string accuracyNode = "Accuracy";
        public static string playerRatingNode = "PlayerRating";
        public static string scenarioRatingNode = "ScenarioRating";

        // [SC] adaptation and log XML shared node references
        public static string adaptationNode = "Adaptation";
        public static string adaptationIDNode = "AdaptationID";
        public static string gameNode = "Game";
        public static string gameIDNode = "GameID";
        public static string scenarioIDNode = "ScenarioID";
        public static string playerIDNode = "PlayerID";
        
        // [SC] non-existing nodes
        public static string ratingTrendNode = "RatingTrend";



        public static string ratingTrendUp = "UP";
        public static string ratingTrendDown = "DOWN";


        // [SC] reserved keywords
        public const string ALL_KEYWORD = "@All";
        



        public static string DATETIME_FORMAT = "yyyy-MM-ddThh:mm:ss";


        

        public const string ADAPT_DATA_FILENAME = "adaptations.xml"; // [TODO]
        public const string LOG_DATA_FILENAME = "gameplaylogs.xml"; // [TODO]

        public const string ADAPT_ROOT_ELEM = "AdaptationData";

        public const string LOG_ROOT_ELEM = "GameplaysData";

        public const string ADAPT_ELEM = "Adaptation";
        public const string ADAPT_ID_ATTR = "AdaptationID";

        public const string SKILL_DIFFICULTY = "Game difficulty - Player skill";


        public const int ROUNDING_DECIMALS = 4;


        //////////////////////////////////////////////////////////////////////////////////
        ////// START: messages

        public const string ADAPT_FILE_NOTOPEN_MSG = "The adaptation file is not open.";

        public const string XML_ERROR_EVENT_MSG = "Error: ";
        public const string XML_WARNING_EVENT_MSG = "Warning: ";
        public const string XML_UNKNOWN_EVENT_MSG = "Unknown XML validation event: ";

        public const string CANT_CREATE_LOGS_FILE_MSG = "Unable to create XML file for gameplay logs.";
        public const string CANT_CREATE_ADAPT_FILE_MSG = "Unable to create adaptation XML file.";

        public const string ADAPT_NOT_EXIST_MSG = "The adaptation with given ID does not exist: ";
        public const string ADAPT_LIST_EMPTY_MSG = "No adaptation list is available: ";

        public const string GAME_NOT_EXIST_MSG = "The game with given ID does not exist: ";
        public const string GAME_ALREADY_EXISTS_MSG = "The game with given ID already exists: ";
        public const string GAME_LIST_EMPTY_MSG = "No game list is available: ";

        public const string PLAYER_ALREADY_EXISTS_MSG = "The player with given ID already exists: ";
        public const string PLAYER_NOT_EXIST_MSG = "The player with given ID does not exist: ";
        public const string PLAYER_LIST_EMPTY_MSG = "No player list available: ";

        public const string SCENARIO_NOT_EXIST_MSG = "The scenario with given ID does not exist: ";
        public const string SCENARIO_LIST_EMPTY_MSG = "No scenario list is available: ";
        public const string SCENARIO_ALREADY_EXISTS_MSG = "The scenario with given ID already exists: ";

        public const string NODE_NOT_EXIST_MSG = "Following node does not exist: ";
        public const string NO_CHILDREN_NODES_MSG = "Following node has no children nodes to retrieve: ";

        public const string UNKNOWN_PLAYER_PROPERTY_MSG = "Unknown player property is found: ";
        public const string EMPTY_NULL_PROPERTY_LIST_MSG = "Property list is either empty or a null object.";
        public const string MISSING_PLAYER_PROPERTY_MSG = "Missing player property: ";
        public const string MISSING_SCENARIO_PROPERTY_MSG = "Missing scenario property: ";

        public const string UNKNOWN_SCENARIO_PROPERTY_MSG = "Unknown scenario property is found: ";

        public const string CANT_ADD_PLAYER_MSG = "Cannot add a new player. ";
        public const string CANT_CHANGE_PLAYER_PROPERTY_MSG = "Cannot change player property. ";

        public const string CANT_ADD_SCENARIO_MSG = "Cannot add a new scenario. ";
        public const string CANT_CHANGE_SCENARIO_PROPERTY_MSG = "Cannot change scenario property. ";



        public const string LOG_FILE_NOTOPEN_MSG = "The gameplay log file is not open.";

        public const string GAMEPLAY_NOT_EXIST_MSG = "The gameplay node with following player or/and scenario IDs does not exist: ";
        public const string GAMEPLAY_LIST_EMPTY_MSG = "No gameplay list is available";

        ////// END: messages
        //////////////////////////////////////////////////////////////////////////////////
        
        //////////////////////////////////////////////////////////////////////////////////
        ////// START:

        static Cfg() {
            reserveKeyword(ALL_KEYWORD);
        }

        public static string reserveKeyword(string keyword) {
            if (reservedKeywords == null) reservedKeywords = new Dictionary<string, string>();
            reservedKeywords.Add(keyword, null);
            return keyword;
        }

        public static bool isReservedKeyword(string keyword) {
            if (reservedKeywords != null && reservedKeywords.ContainsKey(keyword)) return true;
            else return false;
        }

        ////// END:
        //////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////////////////////////////////
        ////// START [TEST]: all methods from here are to be tested 
        
        // [SC] returns true if string is not null, not empty and does not contain any special characters
        public static bool isValidString(string stringVal, string msg) {
            if (stringVal == null) {
                // [TODO] show msg interface
                HATWidget.Cfg.showMsg(msg + " The string value is null."); // [TODO] const msg
                return false;
            }

            if (stringVal.Equals("")) {
                // [TODO] show msg interface
                HATWidget.Cfg.showMsg(msg + " The string value is empty."); // [TODO] const msg
                return false;
            }

            if (stringVal.Contains(Environment.NewLine) || stringVal.Contains("\n")) {
                // [TODO] show msg interface
                HATWidget.Cfg.showMsg(msg + " The string value contains a new line character");
                return false;
            }

            // [TODO] check if string contains any special characters such as end of line

            return true;
        }

        // [SC] returns true if value is any whole (integer) number
        public static bool isValidInteger(double value, string msg) {
            if (value % 1 != 0) {
                HATWidget.Cfg.showMsg(msg + " " + value + " is not an integer value");
                return false;
            }
            return true;
        }

        // [SC] returns true if the value is zero or a positive number
        public static bool isZeroPositiveNumber(double value, string msg) {
            if (value < 0) {
                HATWidget.Cfg.showMsg(msg + " " + value + " is not zero or positive number");
                return false;
            }
            return true;
        }

        // [SC] returns true if the value is a non-zero positive number
        public static bool isPositiveNumber(double value, string msg) {
            if (value <= 0) {
                HATWidget.Cfg.showMsg(msg + " " + value + " is not a positive number");
                return false;
            }
            return true;
        }

        // [SC] returns true if the value is between 0 and 1 (inclusive)
        public static bool isProbability(double value, string msg) {
            if (value < 0 || value > 1) {
                HATWidget.Cfg.showMsg(msg + " " + value + " does not range between 0 and 1");
                return false;
            }
            return true;
        }

        // [SC] returns true if the value is either 0 or 1
        public static bool isBoolean(double value, string msg) {
            if (value != 0 | value != 1) {
                HATWidget.Cfg.showMsg(msg + " " + value + " is neither 0 or 1.");
                return false;
            }
            return true;
        }

        // [SC] return true if the value is not a keyword
        public static bool isNotKeyword(string str, string msg) {
            if (isReservedKeyword(str)) {
                Cfg.showMsg(msg + " The string value '" + str + "' is a reserved keyword.");
                return false;
            }
            return true;
        }

        // [SC] true if any valid string and not a reserved keyword
        public static bool isValidAdaptID(string adaptID) {
            return isValidString(adaptID, "Invalid adaptation ID.") && isNotKeyword(adaptID, "Invalid adaptation ID."); // [TODO] const msg
        }

        // [SC] true if any valid string and not a reserved keyword
        public static bool isValidGameID(string gameID) {
            return isValidString(gameID, "Invalid game ID.") && isNotKeyword(gameID, "Invalid game ID."); // [TODO] const msg
        }

        // [SC] true if any valid string and not a reserved keyword
        public static bool isValidPlayerID(string playerID) {
            return isValidString(playerID, "Invalid player ID.") && isNotKeyword(playerID, "Invalid player ID."); // [TODO] const msg
        }

        // [SC] true if any valid string and not a reserved keyword
        public static bool isValidScenarioID(string scenarioID) {
            return isValidString(scenarioID, "Invalid scenario ID.") && isNotKeyword(scenarioID, "Invalid scenario ID."); // [TODO] const msg
        }

        // [SC] true if rating is any numerical value
        public static bool isValidRating(string rating) {
            if(!isValidString(rating, "Invalid rating.")) { // [TODO] const msg
                return false;
            }

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(rating, out temp)) {
                // [TODO] show msg interface
                HATWidget.Cfg.showMsg(rating + " is not a valid numerical value."); // [TODO] const msg
                return false;
            }

            return true;
        }

        // [SC] play count is any integer value equal to or greater than zero
        public static bool isValidPlayCount(string playCount) {
            string msg = "Invalid play count"; // [TODO] const msg

            if (!isValidString(playCount, msg)) return false;

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(playCount, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + playCount + " is not a vali numerical value."); // [TODO] const msg
                return false;
            }

            if (!isValidInteger(temp, msg)) return false;

            if (!isZeroPositiveNumber(temp, msg)) return false;

            return true;
        }

        // [SC] k factor is any double value equal or greater than zero
        public static bool isValidKFactor(string kFct) {
            string msg = "Invalid K factor";

            if (!isValidString(kFct, msg)) return false;

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(kFct, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + kFct + " is not a valid numerical value"); // [TODO] const msg
                return false;
            }

            if (!isZeroPositiveNumber(temp, msg)) return false;

            return true;
        }

        // [SC] uncertainty is any value between 0 and 1 (inclusive)
        public static bool isValidU(string uVal) {
            string msg = "Invalid uncertainty value.";

            if (!isValidString(uVal, msg)) return false;

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(uVal, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + uVal + " is not a valid numerical value."); // [TODO] const msg
                return false;
            }

            if (!isProbability(temp, msg)) return false;

            return true;
        }

        // [SC] time limit is an integer value greater than 0 
        public static bool isValidTimeLimit(string timeLimit) {
            string msg = "Invalid time limit.";

            if (!isValidString(timeLimit, msg)) return false;

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(timeLimit, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + timeLimit + " is not a valid numerical value."); // [TODO] const msg
                return false;
            }

            if (!isPositiveNumber(temp, msg)) return false;

            return true;
        }

        // [TEST]
        // [SC] any valid positive number
        public static bool isValidRT(string rt) {
            string msg = "Invalid time duration.";

            if (!isValidString(rt, msg)) return false;

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(rt, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + rt + " is not a valid numerical value."); // [TODO] const msg
                return false;
            }

            if (!isPositiveNumber(temp, msg)) return false;

            return true;
        }

        // [TEST]
        // [SC] any valid boolean value
        public static bool isValidAccuracy(string accuracy) {
            string msg = "Invalid accuracy";

            // [TODO] pass by reference
            double temp;
            if (!Double.TryParse(accuracy, out temp)) {
                HATWidget.Cfg.showMsg(msg + " " + " is not a valid numerical value."); // [TODO] const msg
                return false;
            }

            if (!isBoolean(temp, msg)) return false;

            return true;
        }

        // [TEST]
        // [SC] any date in the format scpecified by DATETIME_FORMAT constant 
        public static bool isValidDate(string date) {
            string msg = "Invalid datetime format";

            DateTime val;
            if (!DateTime.TryParseExact(date, DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out val)) {
                HATWidget.Cfg.showMsg(msg);
                return false;
            }

            return true;
        }

        public static void showMsg(string msg){
            HATWidget.Cfg.showMsg(msg);
        }

        ////// END [TEST]: all methods from here to be tested
        ///////////////////////////////////////////////////////////////////////////////
    }
}
