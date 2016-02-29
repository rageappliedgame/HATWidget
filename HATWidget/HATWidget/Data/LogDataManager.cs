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

using System.Xml;
using System.Xml.Schema; // XmlSchema
using System.Xml.XPath; // XPathNavigator
using System.Collections.ObjectModel; // ObservableCollection
using System.Diagnostics; // Debug
using System.Windows; // Point
using System.Reflection; // Assembly

using HATWidget.DS;

namespace HATWidget.Data
{
    class LogDataManager
    {
        private XmlSchema logsSchema; // [SC] is used to validate XML file against XML schema

        private bool openLogStreamFlag; // [SC] true if XML for gameplaylogs was successfully opened and validated
        private bool validXMLFlag; // [SC] false if XML for gameplaylogs is sintactically invalid with respect to the XML Schema "gameplaylogs.xsd" 
        
        private XmlDocument logXmlDoc; // [SC] refers to the DOM of the currently opened gameplaylogs XML file
        private string currLogDatapath;  // [SC] stores path for the currently opened adaptation XML file

        /// <summary>
        /// constructor
        /// </summary>
        public LogDataManager() {
            // [SC] initializing boolean flags
            openLogStreamFlag = false;
            validXMLFlag = true;

            // [SC] initializing XML Schema-based validator
            logsSchema = XmlSchema.Read(Assembly.GetExecutingAssembly().GetManifestResourceStream("HATWidget.Resources.gameplaylogs.xsd"), null);
        }

        /// <summary>
        /// [SC] event handler called by XML Schema-based validator
        /// </summary>
        /// <param name="sender">XmlReaderSettings 'settings'</param>
        /// <param name="e">XML validation event</param>
        private void ValidationEventHandler(object sender, ValidationEventArgs e) {
            switch (e.Severity) {
                case XmlSeverityType.Error:
                    HATWidget.Cfg.showMsg("Error: " + e.Message);
                    validXMLFlag = false;
                    break;
                case XmlSeverityType.Warning:
                    HATWidget.Cfg.showMsg("Warning: " + e.Message);
                    validXMLFlag = false;
                    break;
                default:
                    HATWidget.Cfg.showMsg("Unknown XML validation event: " + e.Message);
                    validXMLFlag = false;
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: log file management methods
        #region log file management methods

        /// <summary>
        /// opens gameplaylogs XML files as DOM object
        /// </summary>
        /// <param name="datapath">absolute path to the XML file</param>
        /// <returns>true if XML file was successfully opened and validated against schema</returns>
        public bool openLogSteam(string datapath) {
            try {
                closeLogStream(true);

                // [SC] load the xml file as DOM object and validate it against the schema
                logXmlDoc = new XmlDocument();
                logXmlDoc.Load(datapath);

                // [SC] validating XML file against "gameplaylogs.xsd" schema
                logXmlDoc.Schemas.Add(logsSchema);
                logXmlDoc.Validate(ValidationEventHandler);

                // [SC] if true then XML file was successfully validated against schema
                if (validXMLFlag) {
                    currLogDatapath = datapath; // [TODO]
                    openLogStreamFlag = true;
                }
                else {
                    // [SC] invalid XML structure; close the DOM without any changes
                    closeLogStream(false);
                }

                return openLogStreamFlag;
            }
            catch (System.IO.FileFormatException) {
                return false;
            }
        }

        /// <summary>
        /// saves XML DOM for gameplay logs to the source file from which it was loaded
        /// </summary>
        /// <returns>returns true if DOM object was saved to the file</returns>
        public bool saveLogStream() {
            if (currLogDatapath != null && hasOpenLogStream()) { // [TODO]
                // [SC] if DOM object does not have schema then add one
                if (!logXmlDoc.Schemas.Contains(logsSchema)) logXmlDoc.Schemas.Add(logsSchema);
                
                // [SC] validate xml doc
                logXmlDoc.Validate(ValidationEventHandler);
                
                // [SC] XML document was validated; save it into the file
                if (validXMLFlag) {
                    logXmlDoc.Save(currLogDatapath);
                    return true;
                }
                else {
                    validXMLFlag = true;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// remove XML DOM object for gameplay logs
        /// </summary>
        /// <param name="saveFlag">if true then DOM object will be saved to the source file before its removal</param>
        public void closeLogStream(bool saveFlag) {
            if (saveFlag) saveLogStream(); // [TODO]
            currLogDatapath = null; // [TODO]
            openLogStreamFlag = false;
            validXMLFlag = true;
            logXmlDoc = null;
        }

        /// <summary>
        /// return true if XML DOM object for gameplay logs is available
        /// </summary>
        /// <returns>return true if XML DOM object for gameplay logs is available</returns>
        public bool hasOpenLogStream() {
            return (openLogStreamFlag && logXmlDoc != null);
        }

        #endregion log file management methods
        ////// START: log file management methods
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: general method
        #region general methods

        /// <summary>
        /// given a parent node, returns a single child node at specified Xpath
        /// </summary>
        /// <param name="xmlNode">parent node</param>
        /// <param name="xPathString">Xpath to the child node</param>
        /// <returns>a single XmlNode</returns>
        public XmlNode selectSingleNode(XmlNode xmlNode, string xPathString) {
            try {
                XmlNode newNode = xmlNode.SelectSingleNode(xPathString);
                return newNode;
            }
            catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// given a parent node, returns a list of childe nodes at specified Xpath
        /// </summary>
        /// <param name="xmlNode">parent node</param>
        /// <param name="xPathString">Xpath to child nodes</param>
        /// <returns>a list of XmlNode</returns>
        public XmlNodeList selectNodeList(XmlNode xmlNode, string xPathString) {
            try {
                XmlNodeList nodeList = xmlNode.SelectNodes(xPathString);
                return nodeList;
            }
            catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// given IDs of various elements and a target node, creates an Xpath to the target node in gameplay logs XML DOM
        /// </summary>
        /// <param name="root">if true then root node will be specified in the Xpath</param>
        /// <param name="adaptID">adaptation ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="gameID">game ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="playerID">player ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="scenarioID">scenario ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="nodeName">target node in adaptation XML</param>
        /// <returns>Xpath to the target node</returns>
        private string getLogXPath(bool root, string adaptID, string gameID, string playerID, string scenarioID, string nodeName) {
            string xPathString = "";
            if (root) xPathString += Cfg.gameplaysDataNode;

            if (adaptID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.adaptationNode + "[@" + Cfg.adaptationIDNode + "='" + adaptID + "']";
                if (nodeName != null && nodeName.Equals(Cfg.adaptationNode)) nodeName = null;
            }

            if (gameID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.gameNode + "[@" + Cfg.gameIDNode + "='" + gameID + "']";
                if (nodeName != null && nodeName.Equals(Cfg.gameNode)) nodeName = null;
            }
            
            if (playerID != null || scenarioID != null 
                    || (nodeName != null && 
                            (nodeName.Equals(Cfg.rtNode)
                                || nodeName.Equals(Cfg.accuracyNode)
                                || nodeName.Equals(Cfg.playerRatingNode)
                                || nodeName.Equals(Cfg.scenarioRatingNode)))
                ) {
                if (!xPathString.Equals("")) xPathString += "/";
                
                xPathString += Cfg.gameplayNode;

                if (playerID != null && scenarioID != null)
                    xPathString += "[@" + Cfg.playerIDNode + "='" + playerID + "' and @" + Cfg.scenarioIDNode + "='" + scenarioID + "']";
                else if (playerID != null)
                    xPathString += "[@" + Cfg.playerIDNode + "='" + playerID + "']";
                else if (scenarioID != null)
                    xPathString += "[@" + Cfg.scenarioIDNode + "='" + scenarioID + "']";

                if (nodeName != null && nodeName.Equals(Cfg.gameplayNode)) nodeName = null;
            }

            if (nodeName != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += nodeName;
            }

            return xPathString;
        }

        /// <summary>
        /// retrieves a single node from the gameplay logs XML DOM; the node to be retrieved is specified by a combination of one or more method parameters
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="nodeName">name of specific XML node that should be returned</param>
        /// <param name="showMsgFlag">true if a error/warning message should be shown</param>
        /// <returns>XmlNode object</returns>
        public XmlNode selectSingleNode(string adaptID, string gameID, string playerID, string scenarioID, string nodeName, bool showMsgFlag) {
            // [SC] verify that the adaptation file was loaded
            if (!hasOpenLogStream()) {
                if (showMsgFlag) Cfg.showMsg(Cfg.LOG_FILE_NOTOPEN_MSG);
                return null;
            }

            // [SC] get root node
            XmlNode currNode = logXmlDoc.DocumentElement;

            // [SC] if specific adaptation is requested by its ID then navigate to the respective node
            if (adaptID != null) {
                if (!Cfg.isValidAdaptID(adaptID)) return null;

                XmlNode adaptNode = selectSingleNode(currNode, getLogXPath(false, adaptID, null, null, null, null));
                if (adaptNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_NOT_EXIST_MSG + adaptID);
                    return null;
                }

                currNode = adaptNode;

                if (nodeName != null && nodeName.Equals(Cfg.adaptationNode)) nodeName = null;
            }

            // [SC] if specific game is requested by its ID then navigate to the respective node
            if (gameID != null) {
                if (!Cfg.isValidGameID(gameID)) return null;

                XmlNode gameNode = selectSingleNode(currNode, getLogXPath(false, null, gameID, null, null, null));
                if (gameNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.GAME_NOT_EXIST_MSG + gameID);
                    return null;
                }

                currNode = gameNode;

                if (nodeName != null && nodeName.Equals(Cfg.gameNode)) nodeName = null;
            }

            // [SC] if playerID was defined then check its validity
            if (playerID != null && !Cfg.isValidPlayerID(playerID)) return null;

            // [SC] if scenario ID was defined then checks its validity
            if (scenarioID != null && !Cfg.isValidScenarioID(scenarioID)) return null;

            // [SC] if specified then retrieve a specific gameplay node by player and/or scenario IDs
            if (playerID != null || scenarioID != null) {
                XmlNode gameplayNode = selectSingleNode(currNode, getLogXPath(false, null, null, playerID, scenarioID, null));
                if (gameplayNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.GAMEPLAY_NOT_EXIST_MSG + playerID + " " + scenarioID);
                    return null;
                }

                currNode = gameplayNode;

                if (nodeName != null && nodeName.Equals(Cfg.gameplayNode)) nodeName = null;
            }

            // [SC] if some specific node is requested by its name then navigate to the respective node
            if (nodeName != null) {
                // [TODO] nodeName validity check

                XmlNode node = selectSingleNode(currNode, getLogXPath(false, null, null, null, null, nodeName));
                if (node == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.NODE_NOT_EXIST_MSG + nodeName);
                    return null;
                }

                currNode = node;
            }

            return currNode;
        }

        /// <summary>
        /// retrieves a list of nodes from the gameplay logs XML DOM; criteria for the list is specified by one or more method parameters 
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="nodeName">the name of specific XML nodes that should be retrieved</param>
        /// <param name="showMsgFlag">true if a error/warning message should be shown</param>
        /// <returns>XmlNodeList object</returns>
        public XmlNodeList selectNodeList(string adaptID, string gameID, string playerID, string scenarioID, string nodeName, bool showMsgFlag) {
            // [SC] verify that the adaptation file was loaded
            if (!hasOpenLogStream()) {
                if (showMsgFlag) Cfg.showMsg(Cfg.LOG_FILE_NOTOPEN_MSG);
                return null;
            }

            // [SC] if playerID was defined then check its validity
            if (playerID != null && !Cfg.isValidPlayerID(playerID)) return null;

            // [SC] if scenario ID was defined then checks its validity
            if (scenarioID != null && !Cfg.isValidScenarioID(scenarioID)) return null;

            if (nodeName == null) return null;

            // [SC] get root node
            XmlNode currNode = logXmlDoc.DocumentElement;

            if (adaptID != null) {
                // [SC]  if adaptation ID was defined then check its validity
                if (!Cfg.isValidAdaptID(adaptID)) return null;

                if (nodeName.Equals(Cfg.adaptationNode)) {
                    XmlNodeList adaptNodeList = selectNodeList(currNode, getLogXPath(false, adaptID, null, null, null, null));

                    // [SC] if true then the game node list is empty
                    if (adaptNodeList == null || adaptNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    }
                    else return adaptNodeList;
                }
                else {
                    // [SC] navigate to the node of requested adaptation node defined by its ID
                    XmlNode adaptNode = selectSingleNode(currNode, getLogXPath(false, adaptID, null, null, null, null));

                    // [SC] the true adaptation node with specified ID was not found
                    if (adaptNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_NOT_EXIST_MSG + adaptID);
                        return null;
                    }

                    currNode = adaptNode;
                }
            }

            if (gameID != null) {
                // [SC] if gameID was defined then check its validity
                if (!Cfg.isValidGameID(gameID)) return null;

                if (nodeName.Equals(Cfg.gameNode)) {
                    XmlNodeList gameNodeList = selectNodeList(currNode, getLogXPath(false, null, gameID, null, null, null));

                    // [SC] if true then the game node list is empty
                    if (gameNodeList == null || gameNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAME_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    }
                    else return gameNodeList;
                }
                else {
                    // [SC] navigate to the node of the requested game node defined by its ID
                    XmlNode gameNode = selectSingleNode(currNode, getLogXPath(false, null, gameID, null, null, null));

                    // [SC] if true then the game node with the specified ID was not found
                    if (gameNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAME_NOT_EXIST_MSG + gameID);
                        return null;
                    }

                    currNode = gameNode;
                }
            }

            if (playerID != null || scenarioID != null) {

                if (nodeName.Equals(Cfg.gameplayNode)) {
                    XmlNodeList gameplayNodeList = selectNodeList(currNode, getLogXPath(false, null, null, playerID, scenarioID, null));

                    // [SC] if true then the game node list is empty
                    if (gameplayNodeList == null || gameplayNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAMEPLAY_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    }
                    else return gameplayNodeList;
                }
                else {
                    // [SC] navigate to the first gameplay node with requested player and/or scenario IDs
                    XmlNode gameplayNode = selectSingleNode(currNode, getLogXPath(false, null, null, playerID, scenarioID, null));

                    // [SC] if true then the gameplay node with specified player and/or scenario IDs was not foun
                    if (gameplayNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAMEPLAY_NOT_EXIST_MSG + playerID + scenarioID);
                        return null;
                    }

                    currNode = gameplayNode;
                }                
            }
            
            if (nodeName != null) {
                XmlNodeList nodeList = selectNodeList(currNode, getLogXPath(false, null, null, null, null, nodeName));

                // [SC] if true then the current node does not have any child nodes with the specified name
                if (nodeList == null || nodeList.Count == 0) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.NO_CHILDREN_NODES_MSG + currNode.Name + "; Requested child node: " + nodeName);
                    return null;
                }
                else return nodeList;
            }

            return null;
        }

        #endregion general methods
        ////// END: general methods
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for game data
        #region methods for game data

        /// <summary>
        /// returns a list of IDs of all games specified within adaptation with given ID
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <returns>list of game IDs as string values</returns>
        public ObservableCollection<string> getAllGameID(string adaptID) { // [2016.01.15] renamed
            // [SC] retrieve all game nodes
            XmlNodeList gameNodeList = selectNodeList(adaptID, null, null, null, Cfg.gameNode, true);
            if (gameNodeList == null) return null;

            // [SC] extract game IDs and store them in a string list
            ObservableCollection<string> gameIDList = new ObservableCollection<string>();
            foreach(XmlNode gameNode in gameNodeList) {
                XmlAttribute gameID = (gameNode as XmlElement).GetAttributeNode(Cfg.gameIDNode);
                gameIDList.Add(gameID.InnerXml);
            }

            return gameIDList;
        }

        #endregion methods for game data
        ////// END: methods for game data
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for player data
        #region methods for player data

        #endregion methods for player data
        ////// START: methods for player data
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for gameplay node

        // [TEST]
        /// <summary>
        /// creates a new Gameplay node in the gameplay logs XML DOM and saves it to the file
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="rt">response time or the duration of time player spent on the scenario</param>
        /// <param name="accuracy">accuracy</param>
        /// <param name="userRating">user/player rating</param>
        /// <param name="scenarioRating">scenario rating</param>
        /// <param name="timestamp">timestamp at the end of gameplay; includes both date and time</param>
        /// <returns>true if gameplay record was added</returns>
        public bool createNewRecord(string adaptID, string gameID, string playerID, string scenarioID
                                , double rt, double accuracy, double userRating, double scenarioRating, string timestamp) {

            // [SC] check validity of all IDs and properties
            if (!Cfg.isValidPlayerID(playerID)) return false;
            if (!Cfg.isValidScenarioID(scenarioID)) return false;
            if (!Cfg.isValidRT("" + rt)) return false;
            if (!Cfg.isValidAccuracy("" + accuracy)) return false;
            if (!Cfg.isValidRating("" + userRating)) return false;
            if (!Cfg.isValidRating("" + scenarioRating)) return false;
            if (!Cfg.isValidDate(timestamp)) return false;

            XmlNode gameNode = selectSingleNode(adaptID, gameID, null, null, null, true);
            if (gameNode == null) return false;

            XmlElement gameplayElem = logXmlDoc.CreateElement(Cfg.gameplayNode);

            XmlAttribute playerIDAttr = logXmlDoc.CreateAttribute(Cfg.playerIDNode);
            playerIDAttr.Value = playerID;
            gameplayElem.Attributes.Append(playerIDAttr);

            XmlAttribute scenarioIDAttr = logXmlDoc.CreateAttribute(Cfg.scenarioIDNode);
            scenarioIDAttr.Value = scenarioID;
            gameplayElem.Attributes.Append(scenarioIDAttr);

            XmlAttribute timestampAttr = logXmlDoc.CreateAttribute(Cfg.timestampNode);
            timestampAttr.Value = timestamp;
            gameplayElem.Attributes.Append(timestampAttr);

            XmlElement rtElem = logXmlDoc.CreateElement(Cfg.rtNode);
            rtElem.InnerText = "" + rt;
            gameplayElem.AppendChild(rtElem);

            XmlElement accuracyElem = logXmlDoc.CreateElement(Cfg.accuracyNode);
            accuracyElem.InnerText = "" + accuracy;
            gameplayElem.AppendChild(accuracyElem);

            XmlElement userRatingElem = logXmlDoc.CreateElement(Cfg.playerRatingNode);
            userRatingElem.InnerText = "" + userRating;
            gameplayElem.AppendChild(userRatingElem);

            XmlElement scenarioRatingElem = logXmlDoc.CreateElement(Cfg.scenarioRatingNode);
            scenarioRatingElem.InnerText = "" + scenarioRating;
            gameplayElem.AppendChild(scenarioRatingElem);

            gameNode.AppendChild(gameplayElem);

            return saveLogStream();
        }

        /// <summary>
        /// returns time series of means and standard errors calculated from means of specified players in a game with 'gameID' using an adaptation 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerIDList">ID list of players who's ratings should be used in calculations</param>
        /// <param name="playCount">max length of time series</param>
        /// <returns>a list containing three sublists; the first sublist is the lower SE boundary, the second sublist is means, and the third sublist is upper boudary</returns>
        public List<List<Point>> getMeanPlayerRatingChanges(string adaptID, string gameID, ObservableCollection<string> playerIDList, int playCount) {
            if (adaptID == null || gameID == null) return null;

            if (playerIDList.Count == 0) return null;
            
            // [SC] retrieve the game node
            XmlNode gameNode = selectSingleNode(adaptID, gameID, null, null, null, true);
            if (gameNode == null) return null;

            // [SC] stores the maximum lenght of player's rating timeseries 
            int maxPlayCount = 0;

            // [SC] extract ratings for each player in playerIDList and store them as an element of nodeListList array
            XmlNodeList[] nodeListList = new XmlNodeList[playerIDList.Count];
            for (int currPlayerIndex = 0; currPlayerIndex < playerIDList.Count; currPlayerIndex++) {
                XmlNodeList playerRatingNodeList = selectNodeList(gameNode, getLogXPath(false, null, null, playerIDList[currPlayerIndex], null, Cfg.playerRatingNode));
                
                nodeListList[currPlayerIndex] = playerRatingNodeList;

                if (maxPlayCount < playerRatingNodeList.Count) maxPlayCount = playerRatingNodeList.Count;
            }

            // [SC] 
            if (playCount > 0 && playCount < maxPlayCount) maxPlayCount = playCount;

            List<Point> meanList = new List<Point>();
            List<Point> lowerSEList = new List<Point>();
            List<Point> upperSEList = new List<Point>();
            for (int currMeanIndex = 0; currMeanIndex < maxPlayCount; currMeanIndex++) {
                double currMean = 0;
                double currSE = 0;
                int currN = 0;
                
                foreach (XmlNodeList nodeList in nodeListList) {
                    if (currMeanIndex < nodeList.Count) {
                        double currRating;
                        if (Double.TryParse(nodeList[currMeanIndex].InnerText, out currRating)) {
                            currMean += currRating;
                            currN++;
                        }
                    }
                }

                // [TODO] should currN should be higher than 1?
                // [TODO] the constant for rounding should be moved to Cfg
                if (currN > 0) {
                    currMean /= currN; // [SC] sample mean
                   
                    foreach (XmlNodeList nodeList in nodeListList) {
                        if (currMeanIndex < nodeList.Count) {
                            double currRating;
                            if (Double.TryParse(nodeList[currMeanIndex].InnerText, out currRating)) {
                                currSE += Math.Pow((currRating - currMean), 2);
                            }
                        }
                    }
                    currSE = Math.Sqrt(currSE/currN); // [SC] standard deviation

                    currSE /= Math.Sqrt(currN); // [SC] standard error

                    meanList.Add(new Point { X = currMeanIndex + 1, Y = Math.Round(currMean, Cfg.ROUNDING_DECIMALS) });
                    lowerSEList.Add(new Point { X = currMeanIndex + 1, Y = Math.Round(currMean - currSE, Cfg.ROUNDING_DECIMALS) });
                    upperSEList.Add(new Point { X = currMeanIndex + 1, Y = Math.Round(currMean + currSE, Cfg.ROUNDING_DECIMALS) });
                }
            }

            return new List<List<Point>> { lowerSEList, meanList, upperSEList };
        }

        /// <summary>
        /// returns a list of points that represents time series of player's ratings
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>a list of points that represents time series of player ratings</returns>
        public List<Point> getPlayerRatingChanges(string adaptID, string gameID, string scenarioID, string playerID) {
            return getRatingChanges(adaptID, gameID, scenarioID, playerID, Cfg.playerRatingNode);
        }

        /// <summary>
        /// returns a list of points that represents time series of scenario's ratings
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>a list of points that represents time series of scenario's rating</returns>
        public List<Point> getScenarioRatingChanges(string adaptID, string gameID, string scenarioID, string playerID) {
            return getRatingChanges(adaptID, gameID, scenarioID, playerID, Cfg.scenarioRatingNode);
        }

        /// <summary>
        /// returns a list of points that represents time series of either player's or scenario's ratings
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="ratingType">either Cfg.PlayerRatingNode or Cfg.scenarioRatingNode</param>
        /// <returns>a list of points that represents time series of rating</returns>
        private List<Point> getRatingChanges(string adaptID, string gameID, string scenarioID, string playerID, string ratingType) {
            if (adaptID == null || gameID == null) return null;

            // [SC] retrieve game node
            XmlNode gameNode = selectSingleNode(adaptID, gameID, null, null, null, true);
            if (gameNode == null) return null;

            // [SC] validate player and scenario IDs
            if (playerID != null && !Cfg.isValidPlayerID(playerID)) return null;
            if (scenarioID != null && !Cfg.isValidScenarioID(scenarioID)) return null;

            // [SC] create a sortable XML DOM navigator
            XPathNavigator docNav = gameNode.CreateNavigator();
            XPathExpression exp = docNav.Compile(getLogXPath(false, null, null, playerID, scenarioID, null));
            // [SC] sorting by ascending order of playthrough's datetimes
            exp.AddSort("@" + Cfg.timestampNode, XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Text);
            
            List<Point> ratings = new List<Point>();
            int currIndex = 0;
            foreach (XPathNavigator currNav in docNav.Select(exp)) {
                double rating;
                if (Double.TryParse(currNav.SelectSingleNode(ratingType).Value, out rating)) {
                    ratings.Add(new Point() { X = ++currIndex, Y = rating });
                }
            }

            return ratings;
        }

        /// <summary>
        /// returns list of gameplays matching the retrieval criteria
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>a list of GameplayElem objects</returns>
        public ObservableCollection<GameplayElem> getGameplays(string adaptID, string gameID, string scenarioID, string playerID) {
            // [TODO] sort by dates

            if (adaptID == null || gameID == null) return null;
            
            // [SC] retrieve matching gameplay nodes
            XmlNodeList gameplayNodeList = selectNodeList(adaptID, gameID, playerID, scenarioID, Cfg.gameplayNode, true);
            if (gameplayNodeList == null) return null;

            // 
            ObservableCollection<GameplayElem> gameplayElemList = new ObservableCollection<GameplayElem>();
            foreach (XmlNode gameplayNode in gameplayNodeList) {
                gameplayElemList.Add(
                    // [TODO] XML node constants
                    new GameplayElem {
                        PlayerID = (gameplayNode as XmlElement).GetAttribute(Cfg.playerIDNode)
                        , ScenarioID = (gameplayNode as XmlElement).GetAttribute(Cfg.scenarioIDNode)
                        , Timestamp = (gameplayNode as XmlElement).GetAttribute(Cfg.timestampNode)
                        , RT = selectSingleNode(gameplayNode, Cfg.rtNode).InnerText
                        , Accuracy = selectSingleNode(gameplayNode, Cfg.accuracyNode).InnerText
                        , PlayerRating = selectSingleNode(gameplayNode, Cfg.playerRatingNode).InnerText
                        , ScenarioRating = selectSingleNode(gameplayNode, Cfg.scenarioRatingNode).InnerText
                    }
                );
            }

            return gameplayElemList;
        }

        ////// END: methods for gameplay
        ////////////////////////////////////////////////////////////////////////////////////
    }
}
