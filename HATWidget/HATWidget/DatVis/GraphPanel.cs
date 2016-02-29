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

using System.Diagnostics; // Debug
using System.Windows.Controls; // Canvas
using System.Windows.Shapes; // Polyline
using System.Windows.Media; // PointCollection
using System.Windows; // Point
using System.Globalization; // CultureInfo

namespace HATWidget.DatVis {
    public class GraphPanel : Canvas 
    {
        //private WidgetWindow widgetWindow;

        // [2016.01.26]
        private int xResGlobal;
        private int yResGlobal;
        private Point minRawPointGlobal;
        private Point maxRawPointGlobal; // [TODO] currently not used anywhere

        public GraphPanel()
            : base() {
            //this.Background = new SolidColorBrush(Cfg.CANVAS_BG);
            this.Width = Cfg.CANVAS_WIDTH;
            this.Height = Cfg.CANVAS_HEIGHT;
            //this.widgetWindow = widgetWindow;
        }

        public void drawGraph(double[] rawPoints, List<string> xLabels, Color lineColor) {
            List<Point> rawPointsList = new List<Point>();
            List<Color> colorList = new List<Color>();

            for (int currIndex = 0; currIndex < rawPoints.Length; currIndex++) {
                Point point = new Point();
                point.X = currIndex + 1;
                point.Y = rawPoints[currIndex];
                rawPointsList.Add(point);
                colorList.Add(lineColor);
            }

            drawGraph(rawPointsList, xLabels, colorList, true);
        }

        public void drawGraph(double[] rawPoints, List<string> xLabels, List<Color> colorList) {
            List<Point> rawPointsList = new List<Point>();

            for (int currIndex = 0; currIndex < rawPoints.Length; currIndex++) {
                Point point = new Point();
                point.X = currIndex + 1;
                point.Y = rawPoints[currIndex];
                rawPointsList.Add(point);
            }

            drawGraph(rawPointsList, xLabels, colorList, true);
        }

        public void drawGraph(List<Point> rawPoints, List<string> xTickLabels, Color lineColor, bool xIndex) {
            List<Color> colorList = new List<Color>();
            foreach (Point point in rawPoints) { 
                colorList.Add(lineColor); 
            }
            drawGraph(rawPoints, xTickLabels, colorList, xIndex);
        }

        // [2016.01.25]
        public void drawGraph(List<Point> rawPoints, List<string> xTickLabels, List<Color> colorList, bool xIndex) {
            clearCanvas();

            Point[] minMaxPoints = getRange(rawPoints);

            // [TODO]
            if (minMaxPoints == null) {
                Debug.WriteLine("Null points provided. Cannot draw a graph.");
                return;
            }

            drawGraph(rawPoints, minMaxPoints[0], minMaxPoints[1], xTickLabels, colorList, xIndex);
        }

        public void drawGraph(List<Point> rawPoints, Point minRawPoint, Point maxRawPoint, List<string> xTickLabels, Color lineColor, bool xIndex) {
            List<Color> colorList = new List<Color>();
            foreach (Point point in rawPoints) {
                colorList.Add(lineColor);
            }
            drawGraph(rawPoints, minRawPoint, maxRawPoint, xTickLabels, colorList, xIndex);
        }

        public void drawGraph(List<Point> rawPoints, Point minRawPoint, Point maxRawPoint, List<string> xTickLabels, List<Color> colorList, bool xIndex) {
            clearCanvas();

            // [TODO]
            if (minRawPoint == null || maxRawPoint == null) {
                Debug.WriteLine("Unknown range. Cannot draw a graph.");
                return;
            }

            // [TODO][TEMP] uncomment
            if (xTickLabels != null && xTickLabels.Count != rawPoints.Count) {
                Debug.WriteLine("The number of labels and points do not match.");
                return;
            }

            int xRes = getXresolution(minRawPoint, maxRawPoint);
            int yRes = getYresolution(minRawPoint, maxRawPoint);

            PointCollection pixelPointCollection = getPixelPoints(rawPoints, xRes, yRes, minRawPoint);

            drawAxes(xRes, yRes, minRawPoint, maxRawPoint, xTickLabels, xIndex); // [TODO]
            drawLine(pixelPointCollection, colorList[0]);
            drawScatter(pixelPointCollection, colorList);

            minRawPointGlobal = minRawPoint;
            maxRawPointGlobal = maxRawPoint;
            xResGlobal = xRes;
            yResGlobal = yRes;
        }

        public void addGraph(List<Point> rawPoints, Color lineColor, bool scatterFlag) {
            List<Color> colorList = new List<Color>();
            foreach (Point point in rawPoints) {
                colorList.Add(lineColor);
            }
            addGraph(rawPoints, colorList, scatterFlag);
        }

        // [2016.01.26]
        public void addGraph(List<Point> rawPoints, List<Color> colorList, bool scatterFlag) {
            if (minRawPointGlobal == null) {
                Debug.WriteLine("Unknown range. Cannot draw graph.");
                return;
            }

            if(rawPoints == null || rawPoints.Count == 0){
                Debug.WriteLine("Cannot draw graph. Null or empty point collection.");
            }

            PointCollection pixelPointCollection = getPixelPoints(rawPoints, xResGlobal, yResGlobal, minRawPointGlobal);

            drawLine(pixelPointCollection, colorList[0]);
            if (scatterFlag) drawScatter(pixelPointCollection, colorList);
        }

        public void drawBarplot(List<double> rawValues, List<string> labels, List<Color> colors, double baseRawValue) {
            clearCanvas();

            double[] minMaxValues = getRange(rawValues);

            if (baseRawValue < minMaxValues[0]) minMaxValues[0] = baseRawValue;
            else if(baseRawValue > minMaxValues[1]) minMaxValues[1] = baseRawValue;

            Point minRawPoint = new Point{ X = 1, Y = minMaxValues[0]};
            Point maxRawPoint = new Point{ X = rawValues.Count, Y = minMaxValues[1]};

            int xRes = getXresolution(minRawPoint, maxRawPoint);
            int yRes = getYresolution(minRawPoint, maxRawPoint);

            Point basePixelValue = getPixelPoint(1, baseRawValue, xRes, yRes, minRawPoint);

            for (int currValIndex = 0; currValIndex < rawValues.Count; currValIndex++) {
                double currRawVal = rawValues[currValIndex];

                Point pixelValue = getPixelPoint(currValIndex+1, currRawVal, xRes, yRes, minRawPoint);
                double barHeight = Math.Abs(basePixelValue.Y - pixelValue.Y);

                Rectangle bar = new Rectangle {
                    Fill = new SolidColorBrush(colors[currValIndex])
                    , Height = barHeight
                    , Width = Cfg.BARPLOT_BAR_WIDTH
                };

                if (pixelValue.Y <= basePixelValue.Y) Canvas.SetTop(bar, pixelValue.Y);
                else Canvas.SetBottom(bar, pixelValue.Y);
                
                Canvas.SetLeft(bar, pixelValue.X);
                this.Children.Add(bar);
            }

            drawAxes(xRes, yRes, minRawPoint, maxRawPoint, labels, true);
        }

        // [2016.01.26]
        public void addLegend(List<string> legendLabels, Color legendColor) {
            addLegend(legendLabels, new List<Color> { legendColor });
        }

        // [2016.01.26]
        public void addLegend(List<string> legendLabels, List<Color> legendColor) {
            // [SC]
            if (legendLabels == null || legendLabels.Count == 0) {
                Debug.WriteLine("Cannot draw legend. Null or empty legend list.");
                return;
            }

            // [SC] draw the legend on the top right corner
            Point startPoint = getPixelPoint(minRawPointGlobal.X, maxRawPointGlobal.Y, xResGlobal, yResGlobal, minRawPointGlobal);

            for (int currLegendIndex = 0; currLegendIndex < legendLabels.Count; currLegendIndex++) {
                Color textColor = Cfg.TEXT_COLOR;
                if(legendColor.Count > currLegendIndex) textColor = legendColor[currLegendIndex];

                TextBlock textBlock = addText(startPoint.X, startPoint.Y, legendLabels[currLegendIndex], 0, textColor);

                startPoint.Y = startPoint.Y + MeasureString(textBlock).Height + Cfg.LEGEND_LABEL_VERT_SPACING;
            }
        }

        // [TODO]
        private void drawAxes(int xRes, int yRes, Point minRawPoint, Point maxRawPoint, List<string> xTickLabels, bool xIndex) {
            Point startPointX = new Point();
            Point endPointX = new Point();

            startPointX.X = getStartPixelX();
            startPointX.Y = getEndPixelY() + Cfg.AXIS_OFFSET;
            endPointX.X = getEndPixelX();
            endPointX.Y = getEndPixelY() + Cfg.AXIS_OFFSET;

            Point startPointY = new Point();
            Point endPointY = new Point();

            startPointY.Y = getStartPixelY();
            startPointY.X = getStartPixelX() - Cfg.AXIS_OFFSET;
            endPointY.Y = getEndPixelY();
            endPointY.X = getStartPixelX() - Cfg.AXIS_OFFSET;

            drawLine(startPointX, endPointX, Cfg.LINE_COLOR);
            drawLine(startPointY, endPointY, Cfg.LINE_COLOR);


            double rangeX = maxRawPoint.X - minRawPoint.X;
            int breakPointCountX = 10; // [TODO]
            double breakLengthX = 0;
            // [TODO] if true then values of x-axis are indices
            if (xTickLabels != null) {
                breakPointCountX = xTickLabels.Count;
                breakLengthX = rangeX / (breakPointCountX - 1);
            }
            else if (xIndex) {
                if (rangeX + 1 < breakPointCountX) {
                    breakPointCountX = (int)rangeX + 1;
                    breakLengthX = rangeX / (breakPointCountX - 1); // [TODO] this is always 1
                }
                else {
                    if ((rangeX + 1) % 2 == 0) rangeX++;
                    while (rangeX % breakPointCountX != 0) breakPointCountX--;
                    breakLengthX = rangeX / breakPointCountX;
                }
            }

            double rangeY = maxRawPoint.Y - minRawPoint.Y;
            int breakPointCountY = 10;
            double breakLengthY = rangeY / (breakPointCountY - 1);

            // [TODO]
            double rawX = minRawPoint.X;
            double rawY = minRawPoint.Y;

            if (xTickLabels == null) {
                // [SC] drawing tick labels for X axis as numbers
                for (int currBreakIndex = 0; currBreakIndex < breakPointCountX; currBreakIndex++) {
                    double currBreakX = minRawPoint.X + (currBreakIndex * breakLengthX);
                    Point pixelPoint = getPixelPoint(currBreakX, rawY, xRes, yRes, minRawPoint);
                    addText(pixelPoint.X, pixelPoint.Y + Cfg.X_AXIS_OFFSET, "" + Math.Round(currBreakX, 3), 0, Cfg.TEXT_COLOR);
                }
            }
            else {
                // [SC] drawing tick labels for X axis as custom strings
                for (int currBreakIndex = 0; currBreakIndex < breakPointCountX; currBreakIndex++) {
                    string tickLabel = xTickLabels[currBreakIndex];
                    Size labelSize = MeasureString(tickLabel);

                    double currBreakX = minRawPoint.X + (currBreakIndex * breakLengthX);

                    Point pixelPoint = getPixelPoint(currBreakX, rawY, xRes, yRes, minRawPoint);
                    addText(pixelPoint.X, pixelPoint.Y + labelSize.Width + Cfg.X_AXIS_OFFSET, tickLabel, -90, Cfg.TEXT_COLOR); // [TODO] -90 constant
                }
            }

            // [SC] drawing tick labels for Y axis
            for (int currBreakIndex = 0; currBreakIndex < breakPointCountY; currBreakIndex++) {
                double currBreakY = Math.Round(minRawPoint.Y + (currBreakIndex * breakLengthY), 3);
                Point pixelPoint = getPixelPoint(rawX, currBreakY, xRes, yRes, minRawPoint);
                addText(pixelPoint.X + Cfg.Y_AXIS_TICK_MARGIN - Cfg.AXIS_OFFSET, pixelPoint.Y, "" + currBreakY, 0, Cfg.TEXT_COLOR);
                //addText(pixelPoint.X - Cfg.Y_AXIS_TICK_MARGIN - Cfg.AXIS_OFFSET, pixelPoint.Y, "" + currBreakY);
            }

            // [SC] drawing X axis label
            Point labelPixelPoint = getPixelPoint((maxRawPoint.X + minRawPoint.X) / 2, rawY, xRes, yRes, minRawPoint);
            if(xTickLabels == null) addText(labelPixelPoint.X, labelPixelPoint.Y + Cfg.AXIS_OFFSET - Cfg.X_AXIS_LABEL_MARGIN, "Playthroughs", 0, Cfg.TEXT_COLOR);

            // [SC] drawing Y axis label           
            labelPixelPoint = getPixelPoint(rawX, (maxRawPoint.Y + minRawPoint.Y) / 2, xRes, yRes, minRawPoint);
            addText(labelPixelPoint.X - Cfg.AXIS_OFFSET + Cfg.Y_AXIS_TICK_MARGIN + Cfg.Y_AXIS_LABEL_MARGIN, labelPixelPoint.Y, "Rating", -90, Cfg.TEXT_COLOR);
        }

        private Point getPixelPoint(double rawPointX, double rawPointY, int xRes, int yRes, Point minRawPoint) {
            Point pixelPoint = new Point();
            pixelPoint.X = getStartPixelX() + (int)((rawPointX - minRawPoint.X) * xRes);
            pixelPoint.Y = getEndPixelY() - (int)((rawPointY - minRawPoint.Y) * yRes);
            //pixelPoint.Y = getStartPixelY() + (int)(rawPoint.Y * yRes); // [TODO][DEBUG]
            return pixelPoint;
        }

        private PointCollection getPixelPoints(List<Point> rawPoints, int xRes, int yRes, Point minRawPoint) {
            PointCollection pixelPointCollection = new PointCollection();
            foreach (Point rawPoint in rawPoints) {
                pixelPointCollection.Add(getPixelPoint(rawPoint.X, rawPoint.Y, xRes, yRes, minRawPoint));
            }
            return pixelPointCollection;
        }

        private int getStartPixelX() {
            return 0 + Cfg.DRAW_AREA_MARGIN;
        }

        private int getStartPixelY() {
            return 0 + Cfg.DRAW_AREA_MARGIN;
        }

        private int getEndPixelX() {
            return getWidth() - Cfg.DRAW_AREA_MARGIN;
        }

        private int getEndPixelY() {
            return getHeight() - Cfg.DRAW_AREA_MARGIN;
        }

        // [SC] returns the number of horizontal pixels per raw point
        private int getXresolution(Point minPoint, Point maxPoint) {
            return (int)((getEndPixelX() - getStartPixelX()) / (maxPoint.X - minPoint.X));
        }

        // [SC] returns the number of vertical pixels per raw point
        private int getYresolution(Point minPoint, Point maxPoint) {
            return (int)((getEndPixelY() - getStartPixelY()) / (maxPoint.Y - minPoint.Y));
        }

        // [SC] get min and max values for Y axis
        private double[] getRange(List<double> rawValues) {
            if (rawValues == null || rawValues.Count == 0) return null;

            double minValue = rawValues[0];
            double maxValue = rawValues[0];

            foreach (double value in rawValues) {
                if (value > maxValue) maxValue = value;
                else if (value < minValue) minValue = value;
            }

            return new double[] { minValue, maxValue };
        }

        // [SC] get min and max values for X and Y axis
        private Point[] getRange(List<Point> rawPoints) {

            if (rawPoints.Count == 0 || rawPoints[0] == null) return null;

            Point minPoint = new Point();
            Point maxPoint = new Point();

            minPoint.X = rawPoints[0].X;
            minPoint.Y = rawPoints[0].Y;

            maxPoint.X = rawPoints[0].X;
            maxPoint.Y = rawPoints[0].Y;

            for (int currPIndex = 1; currPIndex < rawPoints.Count; currPIndex++) {
                if (rawPoints[currPIndex] == null) return null;

                if (rawPoints[currPIndex].X < minPoint.X) minPoint.X = rawPoints[currPIndex].X;
                else if (rawPoints[currPIndex].X > maxPoint.X) maxPoint.X = rawPoints[currPIndex].X;

                if (rawPoints[currPIndex].Y < minPoint.Y) minPoint.Y = rawPoints[currPIndex].Y;
                else if (rawPoints[currPIndex].Y > maxPoint.Y) maxPoint.Y = rawPoints[currPIndex].Y;
            }

            return new Point[] { minPoint, maxPoint };
        }

        private void drawScatter(PointCollection pointCollection, List<Color> colorList) {
            int currIndex = 0;
            foreach (Point point in pointCollection) {
                Ellipse ellipse = new Ellipse { Width = Cfg.PCH_WIDTH, Height = Cfg.PCH_WIDTH };
                ellipse.StrokeThickness = Cfg.LINE_WIDTH;
                ellipse.Stroke = new SolidColorBrush(colorList[currIndex++]);
                Canvas.SetLeft(ellipse, point.X - (Cfg.PCH_WIDTH / 2));
                Canvas.SetTop(ellipse, point.Y - (Cfg.PCH_WIDTH / 2));
                this.Children.Add(ellipse);
            }
        }

        private void drawLine(Point startPoint, Point endPoint, Color lineColor) {
            Line line = new Line();
            line.X1 = startPoint.X;
            line.Y1 = startPoint.Y;
            line.X2 = endPoint.X;
            line.Y2 = endPoint.Y;
            line.Stroke = new SolidColorBrush(lineColor);
            line.StrokeThickness = Cfg.LINE_WIDTH;
            this.Children.Add(line);
        }

        // [SC] pointCollection points should be in pixels
        private void drawLine(PointCollection pointCollection, Color lineColor) {
            Polyline line = new Polyline();
            line.Points = pointCollection;
            line.Stroke = new SolidColorBrush(lineColor);
            line.StrokeThickness = Cfg.LINE_WIDTH;
            this.Children.Add(line);
        }

        private TextBlock addText(double x, double y, string text, int rotateAngle, Color textColor) {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(textColor);
            addText(x, y, textBlock, rotateAngle);
            return textBlock;
        }

        private TextBlock addText(double x, double y, TextBlock textBlock, int rotateAngle) {
            if (rotateAngle != 0) {
                RotateTransform rt = new RotateTransform(rotateAngle);
                textBlock.RenderTransform = rt;
            }
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            this.Children.Add(textBlock);
            return textBlock;
        }

        public int getWidth() {
            return (int)this.Width;
        }

        public int getHeight() {
            return (int)this.Height;
        }

        public void clearCanvas() {
            this.Children.Clear();
        }

        private Size MeasureString(string str) {
            return MeasureString(new TextBlock { Text = str });
        }

        // [SOURCE: http://stackoverflow.com/questions/9264398/how-to-calculate-wpf-textblock-width-for-its-known-font-size-and-characters]
        // [AUTHOR: http://stackoverflow.com/users/92371/randomengy]
        private Size MeasureString(TextBlock textBlock) {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
