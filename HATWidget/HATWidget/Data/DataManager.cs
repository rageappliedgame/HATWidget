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
using System.Xml.Schema;
using System.Xml.XPath;
using System.Collections.ObjectModel; // ObservableCollection
using System.Diagnostics; // Debug
using System.Windows; // Point
using System.Reflection;

using System.IO;

using HATWidget.DS;

namespace HATWidget.Data 
{
    public class DataManager 
    {
        private XmlSchema adaptSchema; // [SC] is used to validate adaptation XML file against XML schema
        private XmlSchema logsSchema; // [SC] is used to validate XML file of gameplay logs against XML schema 

        private bool openAdaptStreamFlag; // [SC] true if XML for adaptation was successfully opened and validated
        private bool validXMLFlag; // [SC] false if XML for adaptation is syntactically invalid with respect to the XML Schema adaptation.xsd

        private XmlDocument adaptXmlDoc; // [SC] refers to the DOM of the currently opened adaptation XML file
        private string currAdaptDatapath; // [SC] stores path for the currently opened adaptation XML file

        /// <summary>
        /// constructor
        /// </summary>
        public DataManager() {
            // [SC] initializing flags
            openAdaptStreamFlag = false; // [2016.01.15]
            validXMLFlag = true;

            // [SC] initializing XML schema-based validator
            adaptSchema = XmlSchema.Read(Assembly.GetExecutingAssembly().GetManifestResourceStream("HATWidget.Resources.adaptations.xsd"), null);
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
                    Cfg.showMsg(Cfg.XML_ERROR_EVENT_MSG + e.Message);
                    validXMLFlag = false;
                    break;
                case XmlSeverityType.Warning:
                    Cfg.showMsg(Cfg.XML_WARNING_EVENT_MSG + e.Message);
                    validXMLFlag = false;
                    break;
                default:
                    Cfg.showMsg(Cfg.XML_UNKNOWN_EVENT_MSG + e.Message);
                    validXMLFlag = false;
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: adaptation file management methods
        #region adaptation file management methods

        /// <summary>
        /// creates a template XML file for gameplay logs
        /// </summary>
        /// <param name="datapath">absolute path where the file should be stored</param>
        public void createLogXML(string datapath) {
            try {
                // [SC] creating a document
                XmlDocument xmlDoc = new XmlDocument();

                // [SC] creating a declaration
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = xmlDoc.DocumentElement;
                xmlDoc.InsertBefore(xmlDeclaration, root);

                // [SC] adding the root element
                XmlElement adaptRootElem = xmlDoc.CreateElement(Cfg.LOG_ROOT_ELEM);
                xmlDoc.AppendChild(adaptRootElem);

                // [SC] adding adaptation type element
                XmlElement adaptElem = xmlDoc.CreateElement(Cfg.ADAPT_ELEM);
                adaptRootElem.AppendChild(adaptElem);

                // [SC] setting adaptation type ID attribute
                XmlAttribute adaptAdaptIDAttr = xmlDoc.CreateAttribute(Cfg.ADAPT_ID_ATTR);
                adaptAdaptIDAttr.Value = Cfg.SKILL_DIFFICULTY;
                adaptElem.Attributes.Append(adaptAdaptIDAttr);

                // [SC] validating the XML doc against the "gameplaylogs.xsd" schema
                xmlDoc.Schemas.Add(logsSchema);
                xmlDoc.Validate(ValidationEventHandler);

                // [SC] if XML doc was validated then save it into a file
                if (validXMLFlag) {
                    xmlDoc.Save(datapath);
                }
                else {
                    validXMLFlag = true;
                    Cfg.showMsg(Cfg.CANT_CREATE_LOGS_FILE_MSG);
                }
            }
            catch (Exception) {
                // [TODO]
            }
        }

        /// <summary>
        /// creates a template XML file for adaptations 
        /// </summary>
        /// <param name="datapath">absolute path where the file should be stored</param>
        public void createAdaptXML(string datapath) {
            try {
                // [SC] creating a document
                XmlDocument xmlDoc = new XmlDocument();

                // [SC] creating a declaration
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = xmlDoc.DocumentElement;
                xmlDoc.InsertBefore(xmlDeclaration, root);

                // [SC] adding the root element
                XmlElement adaptRootElem = xmlDoc.CreateElement(Cfg.ADAPT_ROOT_ELEM);
                xmlDoc.AppendChild(adaptRootElem);

                // [SC] adding adaptation type element
                XmlElement AdaptElem = xmlDoc.CreateElement(Cfg.ADAPT_ELEM);
                adaptRootElem.AppendChild(AdaptElem);

                // [SC] setting adaptation type ID attribute
                XmlAttribute adaptAdaptIDAttr = xmlDoc.CreateAttribute(Cfg.ADAPT_ID_ATTR);
                adaptAdaptIDAttr.Value = Cfg.SKILL_DIFFICULTY;
                AdaptElem.Attributes.Append(adaptAdaptIDAttr);

                // [SC] validating the XML doc against "adaptations.xsd" schema
                xmlDoc.Schemas.Add(adaptSchema);
                xmlDoc.Validate(ValidationEventHandler);

                // [SC] if XML doc was validated then save it to a file
                if (validXMLFlag) {
                    xmlDoc.Save(datapath);
                }
                else {
                    validXMLFlag = true;
                    Cfg.showMsg(Cfg.CANT_CREATE_ADAPT_FILE_MSG);
                }
            }
            catch (Exception) {
                // [TODO]
            }
        }

        /// <summary>
        /// opens adaptation XML files as DOM object
        /// </summary>
        /// <param name="datapath">absolute path to the XML file</param>
        /// <returns>true if XML file was successfully opened and validated against schema</returns>
        public bool openAdaptStream(string datapath) {
            try {
                closeAdaptStream(true);
                
                // [SC] load the xml file as DOM and validate it against schema during loading
                adaptXmlDoc = new XmlDocument();
                adaptXmlDoc.Load(datapath);

                // [SC] validating the XML file against "adaptations.xml" schema
                adaptXmlDoc.Schemas.Add(adaptSchema);
                adaptXmlDoc.Validate(ValidationEventHandler);

                // [SC] if true then XML file was successfully validated against schema
                if (validXMLFlag) {
                    currAdaptDatapath = datapath; // [TODO]
                    openAdaptStreamFlag = true;
                }
                else {
                    // [SC] invalid XML structure; close the DOM without any changes
                    closeAdaptStream(false);
                }

                return openAdaptStreamFlag;
            }
            catch (System.IO.FileFormatException) {
                return false;
            }
        }

        /// <summary>
        /// saves XML DOM for adaptations to the source file from which it was loaded
        /// </summary>
        /// <returns>returns true if the DOM object was save to the file</returns>
        public bool saveAdaptStream() {
            if (currAdaptDatapath != null && hasOpenAdaptStream()) { // [TODO] currDatapath
                // [SC] if DOM object does not have schema then add one
                if (!adaptXmlDoc.Schemas.Contains(adaptSchema)) adaptXmlDoc.Schemas.Add(adaptSchema);

                // [SC] validate xml doc
                adaptXmlDoc.Validate(ValidationEventHandler);
                
                // [SC] XML document was validated; save it into the file
                if (validXMLFlag) {
                    adaptXmlDoc.Save(currAdaptDatapath);
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
        /// remove XML DOM object for adaptations
        /// </summary>
        /// <param name="saveFlag">if true then DOM object will be saved to the source file before its removal</param>
        public void closeAdaptStream(bool saveFlag) {
            if (saveFlag) saveAdaptStream(); // [TODO]
            currAdaptDatapath = null; // [TODO]
            openAdaptStreamFlag = false;
            validXMLFlag = true;
            adaptXmlDoc = null;
        }

        /// <summary>
        /// returns true if XML DOM object for adaptations is available
        /// </summary>
        /// <returns>returns true if XML DOM object for adaptations is available</returns>
        public bool hasOpenAdaptStream() {
            return (openAdaptStreamFlag && adaptXmlDoc != null);
        }

        #endregion adaptation file management methods
        ////// END: adaptation file management methods
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
        /// given IDs of various elements and a target node, creates an Xpath to the target node in adaptation XML
        /// </summary>
        /// <param name="root">if true then root node will be specified in the Xpath</param>
        /// <param name="adaptID">adaptation ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="gameID">game ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="playerID">player ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="scenarioID">scenario ID or a HATWidget.Data.Cfg.ALL_KEYWORD keyword</param>
        /// <param name="nodeName">target node in adaptation XML</param>
        /// <returns>Xpath to the target node</returns>
        private string getAdaptXPath(bool root, string adaptID, string gameID, string playerID, string scenarioID, string nodeName) {
            string xPathString = "";
            if (root) xPathString += Cfg.adaptationDataNode;

            if (adaptID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.adaptationNode;
                if (!adaptID.Equals(Cfg.ALL_KEYWORD)) xPathString += "[@" + Cfg.adaptationIDNode + "='" + adaptID + "']";
            }

            if (gameID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.gameNode;
                if (!gameID.Equals(Cfg.ALL_KEYWORD)) xPathString += "[@" + Cfg.gameIDNode + "='" + gameID + "']";
            }

            if (playerID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.playerDataNode + "/" + Cfg.playerNode;
                if(!playerID.Equals(Cfg.ALL_KEYWORD)) xPathString += "[@" + Cfg.playerIDNode + "='" + playerID + "']";
            }
            else if (scenarioID != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += Cfg.scenarioDataNode + "/" + Cfg.scenarioNode;
                if(!scenarioID.Equals(Cfg.ALL_KEYWORD)) xPathString += "[@" + Cfg.scenarioIDNode + "='" + scenarioID + "']";
            }

            if (nodeName != null) {
                if (!xPathString.Equals("")) xPathString += "/";
                xPathString += nodeName;
            }

            return xPathString;
        }

        /// <summary>
        /// retrieves a single node from the adaptation XML DOM; the node to be retrieved is specified by a combination of one or more method parameters
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
            if (!hasOpenAdaptStream()) {
                if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_FILE_NOTOPEN_MSG);
                return null;
            }

            // [SC] get root node
            XmlNode currNode = adaptXmlDoc.DocumentElement;

            // [SC] if specific adaptation is requested by its ID then navigate to the respective node
            if (adaptID != null) {
                if (!Cfg.isValidAdaptID(adaptID)) return null;

                XmlNode adaptNode = selectSingleNode(currNode, getAdaptXPath(false, adaptID, null, null, null, null));
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
                
                XmlNode gameNode = selectSingleNode(currNode, getAdaptXPath(false, null, gameID, null, null, null));
                if (gameNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.GAME_NOT_EXIST_MSG + gameID);
                    return null;
                }

                currNode = gameNode;

                if (nodeName != null && nodeName.Equals(Cfg.gameNode)) nodeName = null;
            }

            // [SC] if specific player or scenario is requested by its ID then navigate to the respective node
            if (playerID != null) {
                if (!Cfg.isValidPlayerID(playerID)) return null;

                XmlNode playerNode = selectSingleNode(currNode, getAdaptXPath(false, null, null, playerID, null, null));
                if (playerNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.PLAYER_NOT_EXIST_MSG + playerID);
                    return null;
                }

                currNode = playerNode;

                if (nodeName != null && (nodeName.Equals(Cfg.playerNode) || nodeName.Equals(Cfg.playerDataNode))) nodeName = null;
            }
            else if (scenarioID != null){
                if (!Cfg.isValidScenarioID(scenarioID)) return null;

                XmlNode scenarioNode = selectSingleNode(currNode, getAdaptXPath(false, null, null, null, scenarioID, null));
                if (scenarioNode == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.SCENARIO_NOT_EXIST_MSG + scenarioID);
                    return null;
                }

                currNode = scenarioNode;

                if (nodeName != null && (nodeName.Equals(Cfg.scenarioNode) || nodeName.Equals(Cfg.scenarioDataNode))) nodeName = null;
            }

            // [SC] if some specific node is requested by its name then navigate to the respective node
            if (nodeName != null) {
                // [TODO] nodeName validity check

                XmlNode node = selectSingleNode(currNode, getAdaptXPath(false, null, null, null, null, nodeName));
                if (node == null) {
                    if (showMsgFlag) Cfg.showMsg(Cfg.NODE_NOT_EXIST_MSG + nodeName);
                    return null;
                }

                currNode = node;
            }

            return currNode;
        }

        /// <summary>
        /// retrieves a list of nodes from the adaptation XML DOM; criteria for the list is specified by one or more method parameters 
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
            if (!hasOpenAdaptStream()) {
                if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_FILE_NOTOPEN_MSG);
                return null;
            }

            // [SC] get root node
            XmlNode currNode = adaptXmlDoc.DocumentElement;

            if (adaptID != null) {
                if (adaptID.Equals(Cfg.ALL_KEYWORD)) { // [SC] return a list of all adaptation nodes
                    XmlNodeList adaptNodeList = selectNodeList(currNode, getAdaptXPath(false, adaptID, null, null, null, null));
                    
                    // [SC] if true then the list is empty
                    if (adaptNodeList == null || adaptNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    } 
                    else return adaptNodeList;
                } 
                else if (Cfg.isValidAdaptID(adaptID)) { // [SC] navigate to the node of requested adaptation node defined by its ID
                    XmlNode adaptNode = selectSingleNode(currNode, getAdaptXPath(false, adaptID, null, null, null, null));
                    
                    // [SC] the true adaptation node with specified ID was not found
                    if (adaptNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.ADAPT_NOT_EXIST_MSG + adaptID);
                        return null;
                    }

                    currNode = adaptNode;
                }
            }

            if (gameID != null) {
                if (gameID.Equals(Cfg.ALL_KEYWORD)) { // [SC] return a list of all game nodes
                    XmlNodeList gameNodeList = selectNodeList(currNode, getAdaptXPath(false, null, gameID, null, null, null));

                    // [SC] if true then the game node list is empty
                    if (gameNodeList == null || gameNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAME_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    } 
                    else return gameNodeList;
                }
                else if (Cfg.isValidGameID(gameID)) { // [SC] navigate to the node of the requested game node defined by its ID
                    XmlNode gameNode = selectSingleNode(currNode, getAdaptXPath(false, null, gameID, null, null, null));

                    // [SC] if true then the game node with the specified ID was not found
                    if (gameNode == null){
                        if (showMsgFlag) Cfg.showMsg(Cfg.GAME_NOT_EXIST_MSG + gameID);
                        return null;
                    }

                    currNode = gameNode;
                }
            }

            if (playerID != null) {
                if (playerID.Equals(Cfg.ALL_KEYWORD)) { // [SC] return a list of all player nodes
                    XmlNodeList playerNodeList = selectNodeList(currNode, getAdaptXPath(false, null, null, playerID, null, null));

                    // [SC] if true then the player node list is empty
                    if (playerNodeList == null || playerNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.PLAYER_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    } 
                    else return playerNodeList;
                }
                else if (Cfg.isValidPlayerID(playerID)) { // [SC] navigate to the node of the requested player node defined by its ID
                    XmlNode playerNode = selectSingleNode(currNode, getAdaptXPath(false, null, null, playerID, null, null));

                    // [SC] if true then the player node with the specified ID was not found
                    if (playerNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.PLAYER_NOT_EXIST_MSG + playerID);
                        return null;
                    }

                    currNode = playerNode;
                }
            } 
            else if (scenarioID != null) {
                if (scenarioID.Equals(Cfg.ALL_KEYWORD)) { // [SC] return a list of all scenario nodes
                    XmlNodeList scenarioNodeList = selectNodeList(currNode, getAdaptXPath(false, null, null, null, scenarioID, null));

                    // [SC] if true then the scenario node list is empty
                    if(scenarioNodeList == null || scenarioNodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.SCENARIO_LIST_EMPTY_MSG + currNode.Name);
                        return null;
                    } 
                    else return scenarioNodeList;
                }
                else if (Cfg.isValidScenarioID(scenarioID)) { // [SC] navigate to the node of the requested scenario node defined by its ID
                    XmlNode scenarioNode = selectSingleNode(currNode, getAdaptXPath(false, null, null, null, scenarioID, null));

                    // [SC] if true then the scenario node with the specified ID was not found
                    if (scenarioNode == null) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.SCENARIO_NOT_EXIST_MSG + scenarioID);
                        return null;
                    }

                    currNode = scenarioNode;
                }
            }

            if (nodeName != null) {
                if (nodeName.Equals(Cfg.ALL_KEYWORD)) { // [SC] return a list of all child nodes from current node
                    XmlNodeList nodeList = currNode.ChildNodes;

                    // [SC] if true then the current node does not have any child nodes
                    if (nodeList == null || nodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.NO_CHILDREN_NODES_MSG + currNode.Name);
                        return null;
                    }
                    else return nodeList;
                }
                else { // [SC] return a list of child nodes with names matcing the requested name
                    XmlNodeList nodeList = selectNodeList(currNode, getAdaptXPath(false, null, null, null, null, nodeName));

                    // [SC] if true then the current node does not have any child nodes with the specified name
                    if (nodeList == null || nodeList.Count == 0) {
                        if (showMsgFlag) Cfg.showMsg(Cfg.NO_CHILDREN_NODES_MSG + currNode.Name + "; Requested child node: " + nodeName);
                        return null;
                    }
                    else return nodeList;
                }
            }

            return null;
        }

        #endregion general methods
        ////// END: general methods
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for adaptation data
        #region methods for adaptation data
        
        /// <summary>
        /// returns ID of all adaptations specified in the adaptation XML
        /// </summary>
        /// <returns>An observable collection of strings representing adaptation IDs</returns>
        public ObservableCollection<string> getAllAdaptations() {
            // [SC] get all adaptation nodes
            XmlNodeList adaptNodeList = selectNodeList(null, null, null, null, Cfg.adaptationNode, false);
            if (adaptNodeList == null) return null;

            // [SC] extract adaptation IDs and store them in a string list
            ObservableCollection<string> adaptIDList = new ObservableCollection<string>();
            foreach (XmlNode adaptNode in adaptNodeList) {
                XmlAttribute adaptID = (adaptNode as XmlElement).GetAttributeNode(Cfg.adaptationIDNode);
                adaptIDList.Add(adaptID.InnerXml);
            }

            return adaptIDList;
        }

        #endregion methods for adaptation data
        ////// END: methods for adaptation data
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for game data
        #region methods for game data

        /// <summary>
        /// in adaptation DOM object, adds a new Game element with specified ID to the adaptation element with specified ID
        /// </summary>
        /// <param name="adaptID">ID of existing adaptation</param>
        /// <param name="gameID">a new game ID</param>
        /// <param name="gameName">game name/description</param>
        /// <returns>returns true if the new game element was added to XML DOM</returns>
        public bool addGame(string adaptID, string gameID, string gameName) {
            // [TODO] game name

            // [SC] get the adaptation node
            XmlNode adaptNode = selectSingleNode(adaptID, null, null, null, null, true);
            if (adaptNode == null) return false;

            // [SC] verify validity of the game ID
            if (!Cfg.isValidGameID(gameID)) return false;

            // [SC] verify that a game with the same ID does not exist
            XmlNode gameNode = selectSingleNode(adaptNode, getAdaptXPath(false, null, gameID, null, null, null));
            if (gameNode != null) {
                Cfg.showMsg(Cfg.GAME_ALREADY_EXISTS_MSG + gameID);
                return false;
            }

            // [SC] creating Game element
            XmlElement gameElem = adaptXmlDoc.CreateElement(Cfg.gameNode);

            // [SC] adding GameID attribute
            XmlAttribute gameIDAttr = adaptXmlDoc.CreateAttribute(Cfg.gameIDNode);
            gameIDAttr.Value = gameID;
            gameElem.Attributes.Append(gameIDAttr);

            // [SC] adding ScenarioData element
            XmlElement scenarioDataElem = adaptXmlDoc.CreateElement(Cfg.scenarioDataNode);
            gameElem.AppendChild(scenarioDataElem);

            // [SC] adding PlayerData element
            XmlElement playerDataElem = adaptXmlDoc.CreateElement(Cfg.playerDataNode);
            gameElem.AppendChild(playerDataElem);

            adaptNode.AppendChild(gameElem);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// in adaptation DOM object, removes the game element with specified game ID; child elements are removed as well.
        /// </summary>
        /// <param name="adaptID">ID of the adaptation element containing the game element</param>
        /// <param name="gameID">ID of the game element</param>
        /// <returns>returns true if the game element was removed</returns>
        public bool removeGame(string adaptID, string gameID) {
            // [TODO] game name

            // [SC] retrieving the game node
            XmlNode gameNode = selectSingleNode(adaptID, gameID, null, null, null, true);
            if (gameNode == null) return false;

            // [SC] removing the game node
            selectSingleNode(adaptID, null, null, null, null, true).RemoveChild(gameNode);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// returns a list of IDs of all games specified within adaptation with given ID
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <returns>list of game IDs as GameElem objects</returns>
        public ObservableCollection<GameElem> getAllGames(string adaptID) {
            // [SC] retrieve all game nodes
            XmlNodeList gameNodeList = selectNodeList(adaptID, Cfg.ALL_KEYWORD, null, null, null, true);
            if (gameNodeList == null) return null;

            // [SC] extract game IDs and store them in GameElem data structures
            ObservableCollection<GameElem> gameList = new ObservableCollection<GameElem>();
            foreach (XmlNode gameNode in gameNodeList) { 
                XmlAttribute gameID = (gameNode as XmlElement).GetAttributeNode(Cfg.gameIDNode);
                gameList.Add(new GameElem { GameName = "Bla", GameID = gameID.InnerText }); // [TODO]
            }

            return gameList;
        }

        /// <summary>
        /// returns a list of IDs of all games specified within adaptation with given ID
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <returns>list of game IDs as string values</returns>
        public ObservableCollection<string> getAllGameID(string adaptID) { // [2016.01.15] renamed
            // [SC] retrieve all game nodes
            XmlNodeList gameNodeList = selectNodeList(adaptID, Cfg.ALL_KEYWORD, null, null, null, true);
            if (gameNodeList == null) return null;
            
            // [SC] extract game IDs and store them in a string list
            ObservableCollection<string> gameIDList = new ObservableCollection<string>();
            foreach (XmlNode gameNode in gameNodeList) {
                XmlAttribute gameID = (gameNode as XmlElement).GetAttributeNode(Cfg.gameIDNode);
                gameIDList.Add(gameID.InnerXml);
            }

            return gameIDList;
        }

        #endregion methods for game data
        ////// START: methods for game data
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for player data
        #region methods for player data

        /// <summary>
        /// Adds a new player for game with given ID using an adaptation with given ID
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">new player ID</param>
        /// <param name="propertyList">list of properties with values for the new player</param>
        /// <returns>return true if new player was added to the XML data</returns>
        public bool addPlayer(string adaptID, string gameID, string playerID, List<PropertyElem> propertyList) {
            // [SC] reetrieve PlayerData node
            XmlNode playerDataNode = selectSingleNode(adaptID, gameID, null, null, Cfg.playerDataNode, true);
            if (playerDataNode == null) return false;
            
            // [SC] verify that the playerID has a valid value
            if (!Cfg.isValidPlayerID(playerID)) return false;

            // [SC] verifying validity of player's properties
            foreach (PropertyElem propertyElem in propertyList) {
                if (propertyElem.PropertyName.Equals(Cfg.ratingNode)) {
                    if (!Cfg.isValidRating(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.playCountNode)) {
                    if (!Cfg.isValidPlayCount(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.kFctNode)) {
                    if (!Cfg.isValidKFactor(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.uNode)) {
                    if (!Cfg.isValidU(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.lastPlayedNode)) {
                    if (!Cfg.isValidDate(propertyElem.PropertyValue)) return false;
                }
                else {
                    // [TODO] what to do here? it is an unknown property
                    Cfg.showMsg(Cfg.CANT_ADD_PLAYER_MSG + Cfg.UNKNOWN_PLAYER_PROPERTY_MSG + propertyElem.PropertyName);
                    return false;
                }
            }

            // [SC] verify that the player node with the same ID does not exist
            XmlNode playerNode = selectSingleNode(adaptID, gameID, playerID, null, null, false);
            if (playerNode != null) {
                Cfg.showMsg(Cfg.PLAYER_ALREADY_EXISTS_MSG + playerID);
                return false;
            }

            // [SC] creating a player element
            XmlElement playerElem = adaptXmlDoc.CreateElement(Cfg.playerNode);

            // [SC] adding PlayerID attribute
            XmlAttribute playerIDAttr = adaptXmlDoc.CreateAttribute(Cfg.playerIDNode);
            playerIDAttr.Value = playerID;
            playerElem.Attributes.Append(playerIDAttr);

            // [SC] adding all property elements
            foreach (PropertyElem propertyElem in propertyList) {
                XmlElement xmlElem = adaptXmlDoc.CreateElement(propertyElem.PropertyName);
                xmlElem.InnerText = propertyElem.PropertyValue;
                playerElem.AppendChild(xmlElem);
            }

            // [SC] adding the new player element to the XML dom
            playerDataNode.AppendChild(playerElem);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// In XML data, removes a player with given ID from a game with 'gameID' using adaptation with 'adaptID'.
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">ID of a player to be removed</param>
        /// <returns>returns true if the player was removed</returns>
        public bool removePlayer(string adaptID, string gameID, string playerID) {
            // [SC] verify that the player node exists
            XmlNode playerNode = selectSingleNode(adaptID, gameID, playerID, null, null, true);
            if (playerNode == null) {
                Cfg.showMsg(Cfg.PLAYER_NOT_EXIST_MSG + playerID);
                return false;
            }

            // [SC] removing the scenario node
            selectSingleNode(adaptID, gameID, null, null, Cfg.playerDataNode, true).RemoveChild(playerNode);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// returns a PlayerElem list of IDs of all players in a game with 'gameID' and using adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>list of player IDs as PlayerElem objects</returns>
        public ObservableCollection<PlayerElem> getAllPlayers(string adaptID, string gameID) {
            // [SC] retrieve all player nodes and verify that there is at least one player node
            XmlNodeList playerNodeList = selectNodeList(adaptID, gameID, Cfg.ALL_KEYWORD, null, null, true);
            if (playerNodeList == null) return null;

            // [SC] extract player IDs and store them in PlayerElem DS
            ObservableCollection<PlayerElem> playerList = new ObservableCollection<PlayerElem>();
            foreach (XmlNode playerNode in playerNodeList) { 
                XmlAttribute playerID = (playerNode as XmlElement).GetAttributeNode(Cfg.playerIDNode);
                playerList.Add(new PlayerElem { PlayerID = playerID.InnerText, Description = "Bla" }); // [TODO] replace "Bla"
            }

            return playerList;
        }

        /// <summary>
        /// returns a string list of IDs of all players in a game with 'gameID' and using adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>list of player IDs as strings</returns>
        public ObservableCollection<string> getAllPlayerID(string adaptID, string gameID) { // [2016.01.19] renamed
            // [SC] retrieve all player nodes and verify that there is at least one player node
            XmlNodeList playerNodeList = selectNodeList(adaptID, gameID, Cfg.ALL_KEYWORD, null, null, true);
            if (playerNodeList == null) return null;

            // [SC] extract player IDs and store then in a string list
            ObservableCollection<string> playerIDList = new ObservableCollection<string>();
            foreach (XmlNode playerNode in playerNodeList) {
                XmlAttribute playerID = (playerNode as XmlElement).GetAttributeNode(Cfg.playerIDNode);
                playerIDList.Add(playerID.InnerXml);
            }

            return playerIDList;
        }

        /// <summary>
        /// using adaptation data, calculates the mean of all players' rating in a game with 'gameID' using adaptation 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>mean rating as string value</returns>
        public string getMeanPlayersRatings(string adaptID, string gameID) {
            // [SC] get a list of rating nodes from all players
            XmlNodeList ratingNodeList = selectNodeList(adaptID, gameID, Cfg.ALL_KEYWORD, null, Cfg.ratingNode, true);
            if (ratingNodeList == null) return null;

            // [SC] calculate mean rating
            double meanRating = 0;
            int sampleSize = 0;
            foreach (XmlNode ratingNode in ratingNodeList) {
                double currRating;
                if(Double.TryParse(ratingNode.InnerText, out currRating)){
                    meanRating += currRating;
                    sampleSize++;
                }
            }

            return "" + (meanRating / sampleSize);
        }

        /// <summary>
        /// get rating of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>player rating as string</returns>
        public string getPlayerRating(string adaptID, string gameID, string playerID) {
            XmlNode ratingNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.ratingNode, true);
            if (ratingNode == null) return null;
            else return ratingNode.InnerText;
        }

        /// <summary>
        /// get play count of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>player's play count as string</returns>
        public string getPlayerPlayCount(string adaptID, string gameID, string playerID) {
            XmlNode playCountNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.playCountNode, true);
            if (playCountNode == null) return null;
            else return playCountNode.InnerText;
        }

        /// <summary>
        /// get K factor of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>player's K factor as string</returns>
        public string getPlayerKFct(string adaptID, string gameID, string playerID) {
            XmlNode kFctNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.kFctNode, true);
            if (kFctNode == null) return null;
            else return kFctNode.InnerText;
        }

        /// <summary>
        /// get uncertainty of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>player's uncertainty as string</returns>
        public string getPlayerUncertainty(string adaptID, string gameID, string playerID) {
            XmlNode uNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.uNode, true);
            if (uNode == null) return null;
            else return uNode.InnerText;
        }

        /// <summary>
        /// get last played timestamp of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>player's last played timestamp as string</returns>
        public string getPlayerLastPlayed(string adaptID, string gameID, string playerID) {
            XmlNode lastPlayedNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.lastPlayedNode, true);
            if (lastPlayedNode == null) return null;
            else return lastPlayedNode.InnerText;
        }

        /// <summary>
        /// get a list of properties (name-value pairs) of a player specified with 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID';
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <returns>a list of PropertyElem objects</returns>
        public ObservableCollection<PropertyElem> getPlayerProperties(string adaptID, string gameID, string playerID) {
            // [SC] retrieve all properties of the player
            XmlNodeList propertyNodeList = selectNodeList(adaptID, gameID, playerID, null, Cfg.ALL_KEYWORD, true);
            if (propertyNodeList == null) return null;

            // [SC] store property name and value pairs in a list of PropertyElem DS
            ObservableCollection<PropertyElem> propertyList = new ObservableCollection<PropertyElem>();
            foreach (XmlNode propertyNode in propertyNodeList) {
                propertyList.Add(new PropertyElem { PropertyName = propertyNode.Name, PropertyValue = propertyNode.InnerText });
            }

            return propertyList;
        }

        /// <summary>
        /// set a new value for the rating of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="newRating">a new rating</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setPlayerRating(string adaptID, string gameID, string playerID, double newRating) {
            if (!Cfg.isValidRating("" + newRating)) return false;
            XmlNode ratingNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.ratingNode, true);
            if (ratingNode == null) return false;
            ratingNode.InnerText = "" + newRating;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the play count of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="newPlayCount">a new value for the playcount</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setPlayerPlayCount(string adaptID, string gameID, string playerID, double newPlayCount) {
            if (!Cfg.isValidPlayCount("" + newPlayCount)) return false;
            XmlNode playCountNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.playCountNode, true);
            if (playCountNode == null) return false;
            playCountNode.InnerText = "" + newPlayCount;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the K factor of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="newKFct">a new value for the K factor</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setPlayerKFct(string adaptID, string gameID, string playerID, double newKFct) {
            if (!Cfg.isValidKFactor(""+newKFct)) return false;
            XmlNode kFctNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.kFctNode, true);
            if (kFctNode == null) return false;
            kFctNode.InnerText = "" + newKFct;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the uncertainty of a player inidcate by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="newUncertainty">a new value for the uncertainty</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setPlayerUncertainty(string adaptID, string gameID, string playerID, double newUncertainty) {
            if (!Cfg.isValidU(""+newUncertainty)) return false;
            XmlNode uNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.uNode, true);
            if (uNode == null) return false;
            uNode.InnerText = "" + newUncertainty;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the last played timestamp of a player indicated by 'playerID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="newLastPlayed">a new value for last played timestamp</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setPlayerLastPlayed(string adaptID, string gameID, string playerID, string newLastPlayed) {
            if (!Cfg.isValidDate(newLastPlayed)) return false;
            XmlNode lastPlayedNode = selectSingleNode(adaptID, gameID, playerID, null, Cfg.lastPlayedNode, true);
            if (lastPlayedNode == null) return false;
            lastPlayedNode.InnerText = "" + newLastPlayed;
            return saveAdaptStream();
        }

        /// <summary>
        /// set all properties of a player with new values provided as a list of PropertyElem object
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="playerID">player ID</param>
        /// <param name="propertyList">list of property name-value pairs</param>
        /// <returns>returns true if all properties were set to the new values</returns>
        public bool setPlayerProperties(string adaptID, string gameID, string playerID, List<PropertyElem> propertyList) {
            // [SC] verify that the player exists
            XmlNode playerNode = selectSingleNode(adaptID, gameID, playerID, null, null, true);
            if (playerNode == null) return false;

            // [SC] make sure the list is not a null object or empty
            if (propertyList == null || propertyList.Count == 0) {
                Cfg.showMsg(Cfg.EMPTY_NULL_PROPERTY_LIST_MSG);
                return false;
            }

            // [SC] verify validity of all property values
            foreach (PropertyElem propertyElem in propertyList) {
                if (propertyElem.PropertyName.Equals(Cfg.ratingNode)) {
                    if (!Cfg.isValidRating(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.playCountNode)) {
                    if (!Cfg.isValidPlayCount(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.kFctNode)) {
                    if (!Cfg.isValidKFactor(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.uNode)) {
                    if (!Cfg.isValidU(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.lastPlayedNode)) {
                    if (!Cfg.isValidDate(propertyElem.PropertyValue)) return false;
                }
                else { 
                    // [TODO] what to do if unknown property element is found
                    Cfg.showMsg(Cfg.CANT_CHANGE_PLAYER_PROPERTY_MSG + Cfg.UNKNOWN_PLAYER_PROPERTY_MSG + propertyElem.PropertyName);
                    return false;
                }
            }
 
            // [SC] first, loop to make sure all properties are present
            foreach (PropertyElem propertyElemDS in propertyList) {
                XmlNode propertyNode = selectSingleNode(playerNode, propertyElemDS.PropertyName);

                if (propertyNode == null) {
                    Cfg.showMsg(Cfg.MISSING_PLAYER_PROPERTY_MSG + propertyElemDS.PropertyName);
                    return false;
                }
            }

            // [SC] updated all property values in XML doc
            foreach (PropertyElem propertyElemDS in propertyList) {
                XmlNode propertyNode = selectSingleNode(playerNode, propertyElemDS.PropertyName);

                propertyNode.InnerText = propertyElemDS.PropertyValue;
            }

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        #endregion methods for player data
        ////// END: methods for player data
        ////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////
        ////// START: methods for scenario data
        #region methods for scenario data

        /// <summary>
        /// adds a new scenario to the game with 'gameID' using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">ID for the new scenario</param>
        /// <param name="propertyList">list of property name-value pairs for the new scenario</param>
        /// <returns>returns true if the new scenario was created and saved</returns>
        public bool addScenario(string adaptID, string gameID, string scenarioID, List<PropertyElem> propertyList) {
            // [SC] retrieve the scenario data node
            XmlNode scenarioDataNode = selectSingleNode(adaptID, gameID, null, null, Cfg.scenarioDataNode, true);
            if (scenarioDataNode == null) return false;

            // [SC] verifying validity of the scenario ID string
            if (!Cfg.isValidScenarioID(scenarioID)) return false;

            // [SC] verifying validity of values of property elements
            // [TODO] need to make sure that all property elems are present
            // [TODO] need to verify that the same property elem was not defined more than once
            foreach (PropertyElem propertyElem in propertyList) {
                if (propertyElem.PropertyName.Equals(Cfg.ratingNode)) {
                    if (!Cfg.isValidRating(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.playCountNode)) {
                    if (!Cfg.isValidPlayCount(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.kFctNode)) {
                    if (!Cfg.isValidKFactor(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.uNode)) {
                    if (!Cfg.isValidU(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.lastPlayedNode)) {
                    if (!Cfg.isValidDate(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.timeLimitNode)) {
                    if (!Cfg.isValidTimeLimit(propertyElem.PropertyValue)) return false;
                }
                else { 
                    // [TODO] unknown property element found what todo?
                    Cfg.showMsg(Cfg.CANT_ADD_SCENARIO_MSG + Cfg.UNKNOWN_SCENARIO_PROPERTY_MSG + propertyElem.PropertyName);
                    return false;
                }
            }

            // [SC] verify that the scenario node does not exist
            XmlNode scenarioNode = selectSingleNode(adaptID, gameID, null, scenarioID, null, false);
            if (scenarioNode != null) {
                Cfg.showMsg(Cfg.SCENARIO_ALREADY_EXISTS_MSG + scenarioID);
                return false;
            }

            // [SC] creating a Scenario element
            XmlElement scenarioElem = adaptXmlDoc.CreateElement(Cfg.scenarioNode);

            // [SC] adding ScenarioID attribute
            XmlAttribute scenarioIDAttr = adaptXmlDoc.CreateAttribute(Cfg.scenarioIDNode);
            scenarioIDAttr.Value = scenarioID;
            scenarioElem.Attributes.Append(scenarioIDAttr);

            // [SC] adding all property elements
            foreach (PropertyElem propertyElem in propertyList) {
                XmlElement xmlElem = adaptXmlDoc.CreateElement(propertyElem.PropertyName);
                xmlElem.InnerText = propertyElem.PropertyValue;
                scenarioElem.AppendChild(xmlElem);
            }

            // [SC] adding the new Scenario element
            scenarioDataNode.AppendChild(scenarioElem);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// removes the scenario (and its child nodes) with given ID from a game with 'gameID' using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>returns true if the scenario was removed</returns>
        public bool removeScenario(string adaptID, string gameID, string scenarioID) {
            // [SC] retrieve the scenario node
            XmlNode scenarioNode = selectSingleNode(adaptID, gameID, null, scenarioID, null, true);
            if (scenarioNode == null) return false;

            // [SC] removing the scenario node
            selectSingleNode(adaptID, gameID, null, null, Cfg.scenarioDataNode, true).RemoveChild(scenarioNode);

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        /// <summary>
        /// get a list of ratings of all scenarios in a game with 'gameID' using an adaptation 'adaptID'; ratings are sorted in an increasing order
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>list of ScenarioElem objects storing scenario ID-rating pairs</returns>
        public ObservableCollection<ScenarioElem> getAllScenariosData(string adaptID, string gameID) {
            // [SC] verify that the ScenarioData node is present
            XmlNode scenarioDataNode = selectSingleNode(adaptID, gameID, null, null, Cfg.scenarioDataNode, true);
            if (scenarioDataNode == null) return null;

            // [SC] create a sortable list of scenario nodes
            XPathNavigator docNav = scenarioDataNode.CreateNavigator();
            XPathExpression exp = docNav.Compile(Cfg.scenarioNode);
            exp.AddSort(Cfg.ratingNode, XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Number);

            // [SC] stores scenario ID and rating pairs in a list of ScenarioElem objects
            ObservableCollection<ScenarioElem> scenarioList = new ObservableCollection<ScenarioElem>();
            foreach (XPathNavigator currNav in docNav.Select(exp)) {
                string scenarioID = currNav.SelectSingleNode("@" + Cfg.scenarioIDNode).Value;
                double scenarioRating;
                if (Double.TryParse(currNav.SelectSingleNode(Cfg.ratingNode).Value, out scenarioRating)) {
                    scenarioList.Add(new ScenarioElem { ScenarioID = scenarioID, ScenarioRating = scenarioRating });
                }
            }

            return scenarioList;
        }

        /// <summary>
        /// returns a ScenarioElem list of IDs of all scenarios in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>list of scenario IDs as ScenarioElem objects</returns>
        public ObservableCollection<ScenarioElem> getAllScenarios(string adaptID, string gameID) {
            // [SC] retrieve all scenario nodes and verify that there is at least one scenario node
            XmlNodeList scenarioNodeList = selectNodeList(adaptID, gameID, null, Cfg.ALL_KEYWORD, null, true);
            if (scenarioNodeList == null) return null;

            // [SC] stores scenario ID and rating pairs in a list of ScenarioElem objects
            ObservableCollection<ScenarioElem> scenarioList = new ObservableCollection<ScenarioElem>();
            foreach (XmlNode scenarioNode in scenarioNodeList) { 
                XmlAttribute scenarioID = (scenarioNode as XmlElement).GetAttributeNode(Cfg.scenarioIDNode);
                scenarioList.Add(new ScenarioElem { ScenarioID = scenarioID.InnerText, Description = "Bla" }); // [TODO] replace "Bla"
            }

            return scenarioList;
        }

        /// <summary>
        /// returns a string list of IDs of all scenarios in a game with 'gameID' and using adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <returns>list of scenario IDs as strings</returns>
        public ObservableCollection<string> getAllScenarioID(string adaptID, string gameID) {
            // [SC] retrieve all scenario nodes and verify that there is at least one scenario node
            XmlNodeList scenariNodeList = selectNodeList(adaptID, gameID, null, Cfg.ALL_KEYWORD, null, true);
            if (scenariNodeList == null) return null;

            // [SC] extract player IDs and store them in a string list
            ObservableCollection<string> scenarioIDList = new ObservableCollection<string>();
            foreach (XmlNode scenarioNode in scenariNodeList) { 
                XmlAttribute scenarioID = (scenarioNode as XmlElement).GetAttributeNode(Cfg.scenarioIDNode);
                scenarioIDList.Add(scenarioID.InnerXml);
            }

            return scenarioIDList;
        }

        /// <summary>
        /// get rating of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>scenario rating as string</returns>
        public string getScenarioRating(string adaptID, string gameID, string scenarioID) {
            XmlNode ratingNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.ratingNode, true);
            if (ratingNode == null) return null;
            else return ratingNode.InnerText;
        }

        /// <summary>
        /// get play count of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>play count as string</returns>
        public string getScenarioPlayCount(string adaptID, string gameID, string scenarioID) {
            XmlNode playCountNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.playCountNode, true);
            if (playCountNode == null) return null;
            else return playCountNode.InnerText;
        }

        /// <summary>
        /// get K factor of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>K factor as string</returns>
        public string getScenarioKFct(string adaptID, string gameID, string scenarioID) {
            XmlNode kFctNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.kFctNode, true);
            if (kFctNode == null) return null;
            else return kFctNode.InnerText;
        }

        /// <summary>
        /// get uncertainty of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>uncertainty as string</returns>
        public string getScenarioUncertainty(string adaptID, string gameID, string scenarioID) {
            XmlNode uNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.uNode, true);
            if (uNode == null) return null;
            else return uNode.InnerText;
        }

        /// <summary>
        /// get last played timestamp of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>last played timestamp as string</returns>
        public string getScenarioLastPlayed(string adaptID, string gameID, string scenarioID) {
            XmlNode lastPlayedNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.lastPlayedNode, true);
            if (lastPlayedNode == null) return null;
            else return lastPlayedNode.InnerText;
        }

        /// <summary>
        /// get time limit of a scenario indicated by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>time limit as string</returns>
        public string getScenarioTimeLimit(string adaptID, string gameID, string scenarioID) {
            XmlNode timeLimitNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.timeLimitNode, true);
            if (timeLimitNode == null) return null;
            else return timeLimitNode.InnerText;
        }

        /// <summary>
        /// get a list of properties (name-value pair) of a scenario specified with 'scenarioID' in a game with 'gameID' using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <returns>a list of PropertyElem objects</returns>
        public ObservableCollection<PropertyElem> getScenarioProperties(string adaptID, string gameID, string scenarioID) {
            // [SC] get the list of all scenario properties
            XmlNodeList propertyNodeList = selectNodeList(adaptID, gameID, null, scenarioID, Cfg.ALL_KEYWORD, true);
            if (propertyNodeList == null) return null;

            // [SC] store property name-value pairs in a list of PropertyElem objects
            ObservableCollection<PropertyElem> propertyList = new ObservableCollection<PropertyElem>();
            foreach (XmlNode propertyNode in propertyNodeList) {
                propertyList.Add(new PropertyElem { PropertyName = propertyNode.Name, PropertyValue = propertyNode.InnerText });
            }

            return propertyList;
        }

        /// <summary>
        /// set a new value for the rating of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newRating">new rating</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioRating(string adaptID, string gameID, string scenarioID, double newRating) {
            if (!Cfg.isValidRating(""+newRating)) return false;
            XmlNode ratingNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.ratingNode, true);
            if (ratingNode == null) return false;
            ratingNode.InnerText = "" + newRating;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the play count of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newPlayCount">new value for the play count</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioPlayCount(string adaptID, string gameID, string scenarioID, double newPlayCount) {
            if (!Cfg.isValidPlayCount(""+newPlayCount)) return false;
            XmlNode playCountNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.playCountNode, true);
            if (playCountNode == null) return false;
            playCountNode.InnerText = "" + newPlayCount;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the K factor of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newKFct">new value for the K factor</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioKFct(string adaptID, string gameID, string scenarioID, double newKFct) {
            if (!Cfg.isValidKFactor(""+newKFct)) return false;
            XmlNode kFctNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.kFctNode, true);
            if (kFctNode == null) return false;
            kFctNode.InnerText = "" + newKFct;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the uncertainty of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newUncertianty">new value for the uncertainty</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioUncertainty(string adaptID, string gameID, string scenarioID, double newUncertianty) {
            if (!Cfg.isValidU(""+newUncertianty)) return false;
            XmlNode uNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.uNode, true);
            if (uNode == null) return false;
            uNode.InnerText = "" + newUncertianty;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the last played timestamp of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newLastPlayed">new value for the last played timestamp</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioLastPlayed(string adaptID, string gameID, string scenarioID, string newLastPlayed) {
            if (!Cfg.isValidDate(newLastPlayed)) return false;
            XmlNode lastPlayedNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.lastPlayedNode, true);
            if (lastPlayedNode == null) return false;
            lastPlayedNode.InnerText = newLastPlayed;
            return saveAdaptStream();
        }

        /// <summary>
        /// set a new value for the time limit of a scenario inidcate by 'scenarioID' in a game with 'gameID' and using an adaptation with 'adaptID'
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">game ID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="newTimeLimit">new value for the time limit</param>
        /// <returns>returns true if the new value was set</returns>
        public bool setScenarioTimeLimit(string adaptID, string gameID, string scenarioID, double newTimeLimit) {
            if (!Cfg.isValidTimeLimit(""+newTimeLimit)) return false;
            XmlNode timeLimitNode = selectSingleNode(adaptID, gameID, null, scenarioID, Cfg.timeLimitNode, true);
            if (timeLimitNode == null) return false;
            timeLimitNode.InnerText = "" + newTimeLimit;
            return saveAdaptStream();
        }

        /// <summary>
        /// set all properties of a scenario with new values provided as a list of PropertyElem object
        /// </summary>
        /// <param name="adaptID">adaptation ID</param>
        /// <param name="gameID">gameID</param>
        /// <param name="scenarioID">scenario ID</param>
        /// <param name="propertyList">a list of property name--value pairs</param>
        /// <returns></returns>
        public bool setScenarioProperties(string adaptID, string gameID, string scenarioID, List<PropertyElem> propertyList) {
            // [SC] verify that the scenario exists
            XmlNode scenarioNode = selectSingleNode(adaptID, gameID, null, scenarioID, null, true);
            if (scenarioNode == null) return false;

            // [SC] make sure the list is not a null object or an empty list
            if (propertyList == null || propertyList.Count == 0) {
                Cfg.showMsg(Cfg.EMPTY_NULL_PROPERTY_LIST_MSG);
                return false;
            }

            foreach (PropertyElem propertyElem in propertyList) {
                if (propertyElem.PropertyName.Equals(Cfg.ratingNode)) {
                    if (!Cfg.isValidRating(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.playCountNode)) {
                    if (!Cfg.isValidPlayCount(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.kFctNode)) {
                    if (!Cfg.isValidKFactor(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.uNode)) {
                    if (!Cfg.isValidU(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.lastPlayedNode)) {
                    if (!Cfg.isValidDate(propertyElem.PropertyValue)) return false;
                }
                else if (propertyElem.PropertyName.Equals(Cfg.timeLimitNode)) {
                    if (!Cfg.isValidTimeLimit(propertyElem.PropertyValue)) return false;
                }
                else {
                    // [TODO] what to do if unknown property element is found
                    Cfg.showMsg(Cfg.CANT_CHANGE_SCENARIO_PROPERTY_MSG + Cfg.UNKNOWN_SCENARIO_PROPERTY_MSG + propertyElem.PropertyName);
                    return false;
                }
            }

            // [SC] first, loop to make sure all properties are present
            foreach (PropertyElem propertyElemDS in propertyList) {
                XmlNode propertyNode = selectSingleNode(scenarioNode, propertyElemDS.PropertyName);

                if (propertyNode == null) {
                    Cfg.showMsg(Cfg.MISSING_PLAYER_PROPERTY_MSG + propertyElemDS.PropertyName);
                    return false;
                }
            }

            // [SC] updated all property values in XML doc
            foreach (PropertyElem propertyElem in propertyList) {
                XmlNode propertyNode = selectSingleNode(scenarioNode, propertyElem.PropertyName);

                propertyNode.InnerText = propertyElem.PropertyValue;
            }

            // [TODO] what if save was not successful?
            return saveAdaptStream();
        }

        #endregion methods for scenario data
        ////// END: methods for scenario data
        ////////////////////////////////////////////////////////////////////////////////////
    }
}
