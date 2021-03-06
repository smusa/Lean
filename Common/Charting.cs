﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Logging;

namespace QuantConnect 
{
    /// <summary>
    /// Single Parent Chart Object for Custom Charting
    /// </summary>
    [JsonObjectAttribute]
    public class Chart 
    {
        /// Name of the Chart:
        public string Name = "";

        /// Type of the Chart, Overlayed or Stacked.
        public ChartType ChartType = ChartType.Overlay;

        /// List of Series Objects for this Chart:
        public Dictionary<string, Series> Series = new Dictionary<string,Series>();

        /// <summary>
        /// Default constructor for chart:
        /// </summary>
        public Chart() { }

        /// <summary>
        /// Chart Constructor:
        /// </summary>
        /// <param name="name">Name of the Chart</param>
        /// <param name="type"> Type of the chart</param>
        public Chart(string name, ChartType type = ChartType.Overlay) 
        {
            Name = name;
            Series = new Dictionary<string, Series>();
            ChartType = type;
        }

        /// <summary>
        /// Add a reference to this chart series:
        /// </summary>
        /// <param name="series">Chart series class object</param>
        public void AddSeries(Series series) 
        {
            //If we dont already have this series, add to the chrt:
            if (!Series.ContainsKey(series.Name))
            {
                Series.Add(series.Name, series);
            }
            else 
            {
                throw new Exception("Chart.AddSeries(): Chart series name already exists");
            }
        }

        /// <summary>
        /// Fetch the updates of the chart, and save the index position.
        /// </summary>
        /// <returns></returns>
        public Chart GetUpdates() 
        {
            var copy = new Chart(Name, ChartType);
            try
            {   
                foreach (var series in Series.Values)
                {
                    copy.AddSeries(series.GetUpdates());
                }
            }
            catch (Exception err) {
                Log.Error("Chart.GetUpdates(): " + err.Message);
            }
            return copy;
        }
    }


    /// <summary>
    /// Chart Series Object - Series data and properties for a chart:
    /// </summary>
    [JsonObjectAttribute]
    public class Series
    {
        /// <summary>
        /// Name of the Series:
        /// </summary>
        public string Name = "";

        /// <summary>
        ///  Values for the series plot:
        /// These values are assumed to be in ascending time order (first points earliest, last points latest)
        /// </summary>
        public List<ChartPoint> Values = new List<ChartPoint>();

        /// <summary>
        /// Chart type for the series:
        /// </summary>
        public SeriesType SeriesType = SeriesType.Line;

        /// Get the index of the last fetch update request to only retrieve the "delta" of the previous request.
        private int _updatePosition = 0;

        /// <summary>
        /// Default constructor for chart series
        /// </summary>
        public Series() { }

        /// <summary>
        /// Constructor method for Chart Series
        /// </summary>
        /// <param name="name">Name of the chart series</param>
        /// <param name="type">Type of the chart series</param>
        public Series(string name, SeriesType type = SeriesType.Line) 
        {
            Name = name;
            Values = new List<ChartPoint>();
            SeriesType = type;
        }

        /// <summary>
        /// Add a new point to this series:
        /// </summary>
        /// <param name="time">Time of the chart point</param>
        /// <param name="value">Value of the chart point</param>
        /// <param name="liveMode">This is a live mode point</param>
        public void AddPoint(DateTime time, decimal value, bool liveMode = false) 
        {
            //Round off the chart values to significant figures:
            var v = ((double)value).RoundToSignificantDigits(5);

            if (Values.Count < 4000 || liveMode)
            {
                Values.Add(new ChartPoint(time, value));
            }
        }


        /// <summary>
        /// Get the updates since the last call to this function.
        /// </summary>
        /// <returns>List of the updates from the series</returns>
        public Series GetUpdates() 
        {
            var copy = new Series(Name, SeriesType);
            try
            {
                //Add the updates since the last 
                for (var i = _updatePosition; i < Values.Count; i++)
                {
                    copy.Values.Add(Values[i]);
                }
                //Shuffle the update point to now:
                _updatePosition = Values.Count;
            }
            catch (Exception err) {
                Log.Error("Series.GetUpdates(): " + err.Message);
            }
            return copy;
        }
    }


    /// <summary>
    /// Single Chart Point Value Type for QCAlgorithm.Plot();
    /// </summary>
    [JsonObjectAttribute]
    public struct ChartPoint
    {
        /// Time of this chart point: lower case for javascript encoding simplicty
        public long x;

        /// Value of this chart point:  lower case for javascript encoding simplicty
        public decimal y;

        ///Constructor for datetime-value arguements:
        public ChartPoint(DateTime time, decimal value) 
        {
            x = Convert.ToInt64(Time.DateTimeToUnixTimeStamp(time.ToUniversalTime()));
            y = value;
        }

        ///Cloner Constructor:
        public ChartPoint(ChartPoint point) 
        {
            x = point.x;
            y = point.y;
        }
    }


    /// <summary>
    /// Available types of charts
    /// </summary>
    public enum SeriesType 
    { 
        /// Line Plot for Value Types
        Line,
        /// Scatter Plot for Chart Distinct Types
        Scatter,
        /// Charts
        Candle,
        /// Bar chart.
        Bar
    }

    /// <summary>
    /// Type of chart - should we draw the series as overlayed or stacked
    /// </summary>
    public enum ChartType 
    { 
        /// Overlayed stacked
        Overlay,
        /// Stacked series on top of each other.
        Stacked
    }

} // End QC Namespace:
