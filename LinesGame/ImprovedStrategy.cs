using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class ImprovedStrategy : StrategyBase
    {
        int minBallsInLine = 5;
        int _colorCount = 7;
        Size _fieldSize = new Size();
        List<Line> uncompletedLines = new List<Line>();
        int[,] data = null;
        int _height = 0;
        int _width = 0;
        List<Line> calculatedLines = new List<Line>();
        double[,,] blockingScores = null;
        double[,] bottleNeckScores = null;
        double[] lineWeights;
        double bottleNeckWeight = 1;
        double distanceWeight = 0;
        double[] lineCompletenessWeights;
        double[] gapWeights = new double[] { 0, 1, 2, 3, 4 };

        public ImprovedStrategy(int minBallsInLine, int colorCount, Size fieldSize)
        {
            this.minBallsInLine = minBallsInLine;
            _colorCount = colorCount;
            _fieldSize = fieldSize;
            _height = fieldSize.Height;
            _width = fieldSize.Width;
            blockingScores = new double[_height, _width, _colorCount + 1];
            bottleNeckScores = new double[_height, _width];
            lineWeights = new double[minBallsInLine + 1];
            for (int i = 1; i < minBallsInLine + 1; i++)
                lineWeights[i] = i;
            lineWeights[minBallsInLine - 3] = 15;
            lineWeights[minBallsInLine - 2] = 40;
            lineWeights[minBallsInLine - 1] = 150;
            lineCompletenessWeights = new double[] { 300, 150, 40, 15, 3, 1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
        }

        public double BottleNeckWeight { get => bottleNeckWeight; set => bottleNeckWeight = value; }
        public double DistanceWeight { get => distanceWeight; set => distanceWeight = value; }


        int GetDistanceToCenter(Point p)
        {
            return Math.Abs(p.X - _width / 2) + Math.Abs(p.X - _height / 2);
        }

        public override Move GetMove(int[,] data)
        {
            Move move = new Move();
            //uncompletedLines = GetUncompletedLines(data);
            CalculateLines(data);
            var areasAndBorders = StrategyHelper.GetAreasAndBorders(data);
            CalculateBottleNeckScores(areasAndBorders, data);

            Dictionary<Move, double> moveCandidates = new Dictionary<Move, double>();
            for (int color = 1; color < _colorCount + 1; color++)
            {
                foreach (KeyValuePair<List<Point>, List<Point>> entry in areasAndBorders)
                {
                    var areaCells = entry.Key;
                    var borderCells = entry.Value.Where(c => data[c.Y, c.X] == color).ToList();

                    var borderCellsScores = borderCells.Select(

                        cell =>
                        {
                            double score = Enumerable.Range(1, _colorCount).Select(
                                c => c == color ? -blockingScores[cell.Y, cell.X, c] : blockingScores[cell.Y, cell.X, c]).Sum();
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
                            var sc = Enumerable.Range(1, _colorCount).Select(
                                c => c == color ? -blockingScores[cell.Y, cell.X, c] : blockingScores[cell.Y, cell.X, c]).Sum();
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
            move = moveCandidates.Where(entry => entry.Value == maxAward).FirstOrDefault().Key;
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

        List<Line> CalculateLines(int[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            List<Line> lines = new List<Line>();
            blockingScores = new double[height, width, _colorCount + 1];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - minBallsInLine + 1; j++)
                {
                    var row = CustomArray<int>.GetSubRow(data, i, j, minBallsInLine);
                    var occupiedCellsAroud = StrategyHelper.GetOccupiedInlineCellsAround(row);
                    int leftColor = j > 0 ? data[i, j - 1] : -1;
                    int rightColor = j < width ? data[i, j + 1] : -1;

                    var lineProps = StrategyHelper.GetLineProperties(row, _colorCount);
                    if (lineProps.MainColorBallsCount > 0)
                    {
                        for (int k = 0; k < row.Length; k++)
                        {
                            int stepsToComplete = StrategyHelper.GetStepsNumberToComplete(lineProps, minBallsInLine);
                            if (stepsToComplete <= 3 && data[i, j + k] != EmptyCellValue && data[i, j + k] != lineProps.MainColor)
                            {
                                if (k == 0 && rightColor == EmptyCellValue)
                                    continue;
                                if (k == row.Length - 1 && leftColor == EmptyCellValue)
                                    continue;
                            }
                            blockingScores[i, j + k, lineProps.MainColor] += lineCompletenessWeights[stepsToComplete] * lineWeights[lineProps.MainColorBallsCount];
                        }
                        //if (row[k] != lineProps.MainColor)// && row[k] != EmptyCellValue)
                    }

                    var newLine = new Line()
                    {
                        Start = new Point(j, i),
                        End = new Point(j + minBallsInLine - 1, i),
                        LineProperties = lineProps,
                        Cells = new List<LineCell>()
                    };
                    Enumerable.Range(j, minBallsInLine).ToList().ForEach(
                        x => newLine.Cells.Add(
                            new LineCell()
                            {
                                Location = new Point(x, i),
                                BallColor = row[x - j],
                                OccupiedCellsAround = occupiedCellsAroud[x - j]
                            }));
                    lines.Add(newLine);
                }
            }

            for (int i = 0; i < height - minBallsInLine + 1; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var col = CustomArray<int>.GetSubColumn(data, j, i, minBallsInLine);
                    var occupiedCellsAroud = StrategyHelper.GetOccupiedInlineCellsAround(col);
                    var lineProps = StrategyHelper.GetLineProperties(col, _colorCount);

                    int topColor = i > 0 ? data[i - 1, j] : -1;
                    int bottomColor = i < height ? data[i + 1, j] : -1;

                    if (lineProps.MainColorBallsCount > 0)//>= minBallsInLine - 3)
                    {
                        for (int k = 0; k < col.Length; k++)
                        //if (col[k] != lineProps.MainColor)// && col[k] != EmptyCellValue)
                        {
                            int stepsToComplete = StrategyHelper.GetStepsNumberToComplete(lineProps, minBallsInLine);

                            if (stepsToComplete <= 3 && data[i + k, j] != EmptyCellValue && data[i + k, j] != lineProps.MainColor)
                            {
                                if (k == 0 && bottomColor == EmptyCellValue)
                                    continue;
                                if (k == col.Length - 1 && topColor == EmptyCellValue)
                                    continue;
                            }

                            blockingScores[i + k, j, lineProps.MainColor] += lineWeights[lineProps.MainColorBallsCount] * lineCompletenessWeights[stepsToComplete];
                        }
                        //blockingScores[i + k, j, lineProps.MainColor] += lineWeights[lineProps.MainColorBallsCount];
                    }
                    var newLine = new Line()
                    {
                        Start = new Point(j, i),
                        End = new Point(j, i + minBallsInLine - 1),
                        LineProperties = lineProps,
                        Cells = new List<LineCell>()
                    };
                    Enumerable.Range(i, minBallsInLine).ToList().ForEach(
                        y => newLine.Cells.Add(
                              new LineCell()
                              {
                                  Location = new Point(j, y),
                                  BallColor = col[y - i],
                                  OccupiedCellsAround = occupiedCellsAroud[y - i]
                              }));
                    lines.Add(newLine);
                }
            }
            return lines;
        }
    }
}
