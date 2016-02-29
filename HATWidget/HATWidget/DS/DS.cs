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

namespace HATWidget.DS
{
    public class GameElem
    {
        public string GameName { get; set; }
        public string GameID { get; set; }
    }

    public class GameplayElem
    {
        public string PlayerID { get; set; }
        public string ScenarioID { get; set; }
        public string Timestamp { get; set; }
        public string RT { get; set; }
        public string Accuracy { get; set; }
        public string PlayerRating { get; set; }
        public string ScenarioRating { get; set; }
        public string RatingTrend { get; set; }
    }

    public class ScenarioElem 
    {
        public string ScenarioID { get; set; }
        public double ScenarioRating { get; set; }
        public string Description { get; set; }
    }

    public class PlayerElem
    {
        public string PlayerID { get; set; }
        public string Description { get; set; }
    }

    public class PropertyElem
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    public class AnalysisElem 
    {
        public string AnalysisID { get; set; }
        public string AnalysisName { get; set; }
        public string Description { get; set; }
    }

    public class GameplaySummaryElem
    {
        public string ScenarioID { get; set; }
        public int PlayCount { get; set; }
        public double PlayerRating { get; set; }
        public double Accuracy { get; set; }
        public double RT { get; set; }
    }

    public class ScenarioSummaryElem 
    {
        public string ScenarioID { get; set; }
        public int PlayCount { get; set; }
        public double ScenarioRating { get; set; }
        public double Accuracy { get; set; }
        public double RT { get; set; }
    }
}
