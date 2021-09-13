using LinesAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    enum ScoreStatus
    { 
        Updated = 0,
        Modified,
        Calculating
    }

    public class ImprovedStrategy : StrategyBase
    {
        int minBallsInLine = 5;
        int _colorCount = 7;
        Size _fieldSize = new Size();
        int[,] data = null;
        int _height = 0;
        int _width = 0;
        double[,,] blockingScores = null;
        double[,] bottleNeckScores = null;
        double[] lineWeights;
        double bottleNeckWeight = 1;
        double distanceWeight = 0;
        double[] lineCompletenessWeights;
        double[] gapWeights = new double[] { 0, 1, 2, 3, 4 };
        ScoreStatus[,] scoreStatuses = null;
        double[,] totalBlockingScores;
        List<Cell> postponedChanges = new List<Cell>();
        double lineCenterScale = 2;


        //IPrinter printer = new ConsolePrinter();
        IPrinter printer = new EmptyPrinter();
        public ImprovedStrategy(int minBallsInLine, int colorCount, Size fieldSize)
        {
            this.minBallsInLine = minBallsInLine;
            _colorCount = colorCount;
            _fieldSize = fieldSize;
            _height = fieldSize.Height;
            _width = fieldSize.Width;
            data = new int[_height, _width];

            blockingScores = new double[_height, _width, _colorCount + 1];
            totalBlockingScores = new double[_height, _width];
            bottleNeckScores = new double[_height, _width];
            lineWeights = new double[minBallsInLine + 1];
            for (int i = 1; i < minBallsInLine + 1; i++)
                lineWeights[i] = i;
            lineWeights[minBallsInLine - 3] = 15;
            lineWeights[minBallsInLine - 2] = 40;
            lineWeights[minBallsInLine - 1] = 150;
            lineCompletenessWeights = new double[] { 300, 150, 40, 15, 3, 1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };

            scoreStatuses = new ScoreStatus[_height, _width];
        }

        public double BottleNeckWeight { get => bottleNeckWeight; set => bottleNeckWeight = value; }
        public double DistanceWeight { get => distanceWeight; set => distanceWeight = value; }

        int GetDistanceToCenter(Point p)
        {
            return Math.Abs(p.X - _width / 2) + Math.Abs(p.X - _height / 2);
        }

        List<Cell> GetDifference(int[,] oldData, int[,] newData) 
        {
            if (oldData.GetLength(0) != newData.GetLength(0) || oldData.GetLength(1) != newData.GetLength(1))
                throw new Exception("Dimentions don't match");

            List<Cell> changes = new List<Cell>();
            for (int i = 0; i < oldData.GetLength(0); i++)
                for (int j = 0; j < oldData.GetLength(1); j++)
                {
                    if (oldData[i, j] != newData[i, j])
                        changes.Add(new Cell() { Color = newData[i, j], Point = new Point(j, i) });
                }
            return changes;
        }

        public override Move GetMove(int[,] newData)
        {
            var differences = GetDifference(this.data, newData);
            differences.ForEach(c => data[c.Point.Y, c.Point.X] = c.Color);
            differences.AddRange(postponedChanges.Where(c => !differences.Select(cc => cc.Point).Contains(c.Point)));
            CalculateScores(differences);
            
            var areasAndBorders = StrategyHelper.GetAreasAndBorders(newData);
            //CalculateBottleNeckScores(areasAndBorders, newData);

            Dictionary<Move, double> moveCandidates = new Dictionary<Move, double>();

            for (int color = 1; color < _colorCount + 1; color++)
            {
                foreach (KeyValuePair<List<Point>, List<Point>> entry in areasAndBorders)
                {
                    var areaCells = entry.Key;
                    var borderCells = entry.Value.Where(c => newData[c.Y, c.X] == color).ToList();

                    var borderCellsScores = borderCells.Select(

                        cell =>
                        {
                            double score = totalBlockingScores[cell.Y, cell.X] - 2 * blockingScores[cell.Y, cell.X, color];
                            //score += bottleNeckScores[cell.Y, cell.X] * BottleNeckWeight;
                            //score += GetDistanceToCenter(cell);
                            return score;
                        }
                    );
                    if (borderCellsScores.Count() == 0)
                        continue;
                    var mostBlockingScore = borderCellsScores.Max();
                    var mostBlockingOccupiedCell = borderCells[Array.IndexOf(borderCellsScores.ToArray(), mostBlockingScore)];

                    var areaCellsScores = areaCells.Select(
                        cell =>
                        {
                            double sc = totalBlockingScores[cell.Y, cell.X] - 2 * blockingScores[cell.Y, cell.X, color];
                            var distance = Math.Abs(mostBlockingOccupiedCell.X - cell.X) + Math.Abs(mostBlockingOccupiedCell.Y - cell.Y);
                            sc -= distance * distanceWeight;
                            return sc;
                        }
                    );

                    var leastBlockingScore = areaCellsScores.Min();
                    var leastBlockingEmptyCell = areaCells[Array.IndexOf(areaCellsScores.ToArray(), leastBlockingScore)];


                    var award = (mostBlockingScore - leastBlockingScore);// * Math.Log(distance);
                    moveCandidates.Add(new Move() { StartPoint = mostBlockingOccupiedCell, EndPoint = leastBlockingEmptyCell }, award);
                }
            }
            var maxAward = moveCandidates.Values.Max();

            Move move = moveCandidates.Where(entry => entry.Value == maxAward).FirstOrDefault().Key;

            postponedChanges.Clear();
            postponedChanges.Add(new Cell() {  Point = move.EndPoint, Color = data[move.StartPoint.Y, move.StartPoint.X]});
            postponedChanges.Add(new Cell() { Point = move.StartPoint, Color = 0});

            data[move.EndPoint.Y, move.EndPoint.X] = data[move.StartPoint.Y, move.StartPoint.X];
            data[move.StartPoint.Y, move.StartPoint.X] = 0;
            return move;
        }

        public void CalculateBottleNeckScores(Dictionary<List<Point>, List<Point>> areasAndBorders, int[,] data)
        {
            if (areasAndBorders.Count < 2) return;

            var borders = areasAndBorders.Values.ToArray();
            var areas = areasAndBorders.Keys.ToArray();

            var allBorders = new List<Point>();
            borders.ToList().ForEach(b => allBorders.AddRange(b));
            allBorders = allBorders.Distinct().ToList();

            allBorders.ForEach(
                b =>
                {
                    var cellsAround = StrategyHelper.GetCellsAround(b, data);
                    List<List<Point>> containingAreas = new List<List<Point>>();
                    cellsAround.Where(c => data[c.Y, c.X] == EmptyCellValue).ToList().ForEach(
                        c =>
                        {
                            containingAreas.AddRange(areas.Where(a => a.Contains(c)));
                        }
                    );
                    containingAreas = containingAreas.Distinct().ToList();
                    if (containingAreas.Count > 1)
                    {
                        var areasCellCounts = containingAreas.Select(a => a.Count);
                        bottleNeckScores[b.Y, b.X] = areasCellCounts.Aggregate((total, next) => total * next);
                    }
                }
                );
        }

        void ResetBlockingScoresToUpdate(List<Cell> changedCells)
        {
            changedCells.ForEach(
                 c => {
                     var x = c.Point.X;
                     var y = c.Point.Y;

                     for (int row = Math.Max(0, y - minBallsInLine + 1); row < Math.Min(_height, y + minBallsInLine); row++)
                     {
                         for (int color = 0; color < _colorCount + 1; color++)
                             blockingScores[row, x, color] = 0;
                         totalBlockingScores[row, x] = 0;
                         scoreStatuses[row, x] = ScoreStatus.Modified;
                     }

                     for (int col = Math.Max(0, x - minBallsInLine + 1); col < Math.Min(_width, x + minBallsInLine); col++)
                     {
                         for (int color = 0; color < _colorCount + 1; color++)
                             blockingScores[y, col, color] = 0;
                         totalBlockingScores[y, col] = 0;
                         scoreStatuses[y, col] = ScoreStatus.Modified;
                     }
                 }
            );
        }

        void SetAllScoreStatuses(ScoreStatus status)
        {
            for(int i =0; i< scoreStatuses.GetLength(0); i++)
                for (int j = 0; j < scoreStatuses.GetLength(1); j++)
                    scoreStatuses[i, j] = status;
        }

        void CalculateScores(List<Cell> changedCells)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            ResetBlockingScoresToUpdate(changedCells);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - minBallsInLine + 1; j++)
                {
                    {
                        //var lineScoreStatuses = CustomArray<ScoreStatus>.GetSubRow(scoreStatuses, i, j, minBallsInLine);
                        //if (lineScoreStatuses.Where(s => s != ScoreStatus.Updated).Count() == 0)
                        //    continue;

                        var row = CustomArray<int>.GetSubRow(data, i, j, minBallsInLine);
                        int leftColor = j > 0 ? data[i, j - 1] : -1;
                        int rightColor = j < width ? data[i, j + 1] : -1;

                        var mainColorBallsCount = StrategyHelper.GetMaxColorCount(row, _colorCount);

                        //var lineProps = StrategyHelper.GetLineProperties(row, _colorCount);
                        if (mainColorBallsCount > 0)
                        {
                            var lineProps = StrategyHelper.GetLineProperties(row, _colorCount);
                            for (int k = 0; k < row.Length; k++)
                            {
                                if (scoreStatuses[i, j + k] == ScoreStatus.Updated)
                                    continue;
                                scoreStatuses[i, j + k] = ScoreStatus.Calculating;
                                int stepsToComplete = StrategyHelper.GetStepsNumberToComplete(lineProps, minBallsInLine);
                                if (stepsToComplete <= 3 && data[i, j + k] != EmptyCellValue && data[i, j + k] != lineProps.MainColor)
                                {
                                    if (k == 0 && rightColor == EmptyCellValue)
                                        continue;
                                    if (k == row.Length - 1 && leftColor == EmptyCellValue)
                                        continue;
                                }

                                var distanceToLineCenter = Math.Abs(lineProps.LineColorCenter - k) / row.Length;
                                var lineCenterFactor = (lineCenterScale - distanceToLineCenter)/ lineCenterScale;
                                var score = lineCompletenessWeights[stepsToComplete] * lineWeights[lineProps.MainColorBallsCount] * lineCenterFactor;

                                //double fieldCenterDistance = GetDistanceToCenter(new Point(j+k, i)) / (0.5 * (_height + _width));
                                //double fieldCenterFactor = (fieldCenterScale - fieldCenterDistance) / fieldCenterScale;
                                //score *= fieldCenterFactor;

                                blockingScores[i, j + k, lineProps.MainColor] += score;
                                totalBlockingScores[i, j+k] += score;
                            }
                        }
                    }

                }
            }

            for (int i = 0; i < height - minBallsInLine + 1; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    {
                        //var lineScoreStatuses = CustomArray<ScoreStatus>.GetSubColumn(scoreStatuses, j, i, minBallsInLine);
                        //if (lineScoreStatuses.Where(s => s != ScoreStatus.Updated).Count() == 0)
                        //    continue;
                        
                        var col = CustomArray<int>.GetSubColumn(data, j, i, minBallsInLine);
                        var occupiedCellsAroud = StrategyHelper.GetOccupiedInlineCellsAround(col);
                        var mainColorBallsCount = StrategyHelper.GetMaxColorCount(col, _colorCount);

                        int topColor = i > 0 ? data[i - 1, j] : -1;
                        int bottomColor = i < height ? data[i + 1, j] : -1;

                        if (mainColorBallsCount > 0)//>= minBallsInLine - 3)
                        {
                            var lineProps = StrategyHelper.GetLineProperties(col, _colorCount);
                            for (int k = 0; k < col.Length; k++)
                            {
                                if (scoreStatuses[i + k, j] == ScoreStatus.Updated)
                                    continue;
                                scoreStatuses[i + k, j] = ScoreStatus.Calculating;
                                int stepsToComplete = StrategyHelper.GetStepsNumberToComplete(lineProps, minBallsInLine);

                                if (stepsToComplete <= 3 && data[i + k, j] != EmptyCellValue && data[i + k, j] != lineProps.MainColor)
                                {
                                    if (k == 0 && bottomColor == EmptyCellValue)
                                        continue;
                                    if (k == col.Length - 1 && topColor == EmptyCellValue)
                                        continue;
                                }


                                var distanceToLineCenter = Math.Abs(lineProps.LineColorCenter - k) / col.Length;
                                var lineCenterFactor = (lineCenterScale - distanceToLineCenter) / lineCenterScale;
                                
                                var score = lineCompletenessWeights[stepsToComplete] * lineWeights[lineProps.MainColorBallsCount] * lineCenterFactor;

                                //double fieldCenterDistance = GetDistanceToCenter(new Point(j, i + k))/(0.5*(_height + _width));
                                //double fieldCenterFactor = (fieldCenterScale - fieldCenterDistance) / fieldCenterScale;
                                //score *= fieldCenterFactor;

                                blockingScores[i + k, j, lineProps.MainColor] += score;
                                totalBlockingScores[i + k, j] += score;
                            }
                        }
                    }
                }
            }
            printer.PrintField(scoreStatuses, "scoreStatuses", false);
            SetAllScoreStatuses(ScoreStatus.Updated);
        }

        double fieldCenterScale = 2;
    }
}
