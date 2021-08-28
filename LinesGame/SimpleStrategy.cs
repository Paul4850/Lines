using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class LineCell
    {
        public Point Location { get; set; }
        public int BallColor { get; set; }
        public int OccupiedCellsAround { get; set; }
    }

    public class Line
    {
        public LineProperties LineProperties { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }

        public List<LineCell> Cells { get; set; }
    }

    public class LineProperties {
        public int MainColor { get; set; }
        public int MainColorBallsCount { get; set; }
        public int EmptyCellsCount { get; set; }
        public int Length { get; set; }
        public bool HasSingleColor { get { return MainColorBallsCount + EmptyCellsCount == Length && EmptyCellsCount < Length; } }
    }

    public class SimpleStrategy : StrategyBase
    {
        public const int EmptyCellValue = 0;

        int minBallsInLine = 5;
        int _colorCount = 7;
        Size _fieldSize = new Size();
        List<Line> uncompletedLines = new List<Line>();
        int[,] data = null;
        int _height = 0;
        int _width = 0;
        List<Line> calculatedLines = new List<Line>();
        double[, ,] blockingScores = null;

        double[,] bottleNeckScores = null;

        double[] lineWeights;

        double bottleNeckWeight = 1;
        double distanceWeight = 0;
        public SimpleStrategy(int minBallsInLine, int colorCount, Size fieldSize)
        {
            this.minBallsInLine = minBallsInLine;
            _colorCount = colorCount;
            _fieldSize = fieldSize;
            _height = fieldSize.Height;
            _width = fieldSize.Width;
            blockingScores = new double[_height, _width, _colorCount + 1];
            bottleNeckScores = new double[_height, _width];
            lineWeights = new double[minBallsInLine+1] ;
            for (int i = 1; i < minBallsInLine + 1; i++)
                lineWeights[i] = i;
            lineWeights[minBallsInLine - 3] = 15;
            lineWeights[minBallsInLine - 2] = 40;
            lineWeights[minBallsInLine - 1] = 150;
            //lineCompletenessWeights = new double[minBallsInLine * 2];
            //lineCompletenessWeights = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            lineCompletenessWeights = new double[] {300, 150, 40, 15, 3, 1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1};
        }
        double[] lineCompletenessWeights;

        public int GetStepsNumberToComplete(LineProperties lineProperties) 
        { 
            return lineProperties.EmptyCellsCount + 2 * (minBallsInLine - lineProperties.MainColorBallsCount - lineProperties.EmptyCellsCount); 
        }

        // TODO:
        // - pack balls within lines to fill gaps
        // - take ball for gap from center rather than from border
        // - remove balls of other color from almost completed lines. put them to complete other line if possible. 


        //ball selection criteria
        // - distance to the ball
        // - lines blocked by the ball
        // - lines including the ball
        // - distance to center

   
        IPrinter printer = new ConsolePrinter();

        void PrintComb(int length)
        {
            int maxCombinations = 1 >> length;
            for (int i = 1; i < maxCombinations; i++)
            {
                string res = "";
                for (int offset = 0; offset < length; offset++)
                {
                    var exists = i >> offset & 1;
                    res += exists.ToString();

                    Console.WriteLine(res);
                }
            }
        }

        List<Point> GetCellsAround(Point cell, int[,] data)
        {

            int top = Math.Max(0, cell.Y - 1);
            int left = Math.Max(0, cell.X - 1);
            int bottom = Math.Min(data.GetLength(0) - 1, cell.Y + 1);
            int right = Math.Min(data.GetLength(1) - 1, cell.X + 1);

            List<Point> cellsAround = new List<Point>();

            for (int i = top; i <= bottom; i++)
                for (int j = left; j <= right; j++)
                {
                    if (Math.Abs(cell.Y - i) == Math.Abs(cell.X - j))
                        continue;
                    cellsAround.Add(new Point(j, i));
                }
            return cellsAround;
        }

        public void CalculateBottleNeckScores(Dictionary<List<Point>, List<Point>> areasAndBorders, int[,]data)
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
                    var cellsAround = GetCellsAround(b, data);
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

            //var areasCellCounts = areasAndBorders.Values.Select( a => a.Count).ToList();
            //int length = borders.Length;
            //int maxCombinations = 1 << length;
            //Dictionary<int, List<Point>> intersections = new Dictionary<int, List<Point>>();
            //for (int bordersCount = borders.Length; bordersCount > 1; bordersCount--)
            //{
            //    for (int i = 1; i < maxCombinations; i++)
            //    {
            //        int count = 0;
            //        for (int offset = 0; offset < length; offset++)
            //            count += i >> offset & 1;
            //        if (count != bordersCount)
            //            continue;

            //        List<Point> intersection = null;
            //        for (int offset = 0; offset < length; offset++)
            //        {
            //            var exists = (i >> offset & 1) == 1;
            //            if (exists)
            //            {
            //                if (intersection == null)
            //                {
            //                    intersection = new List<Point>();
            //                    intersection.AddRange(borders[offset]);
            //                }
            //                else
            //                    intersection = intersection.Intersect(borders[offset]).ToList();
            //                if (intersection.Count == 0)
            //                    break;
            //            }
            //        }
            //        intersections.Values.ToList().ForEach(ii => intersection = intersection.Intersect(ii).ToList());
            //        if (intersection.Count > 0)
            //        {
            //            int totalAreaCellCount = 0;
            //            for (int offset = 0; offset < length; offset++)
            //            {
            //                var exists = (i >> offset & 1) == 1;
            //                if (exists)
            //                    totalAreaCellCount += areasCellCounts[offset];
            //            }
            //            intersections.Add(totalAreaCellCount, intersection);
            //        }
            //    }
            //}


            //foreach (KeyValuePair<List<Point>, List<Point>> entry in areasAndBorders)
            //{
            //    var areaCells = entry.Key;
            //    var borderCells = entry.Value;
                
            //}
        }


        public override Move GetMove(int[,] data)
        {
            Move move = new Move();
            uncompletedLines = GetUncompletedLines(data);
            var areasAndBorders = GetAreasAndBorders(data);
            CalculateBottleNeckScores(areasAndBorders, data);

            Dictionary<Move, double> moveCandidates = new Dictionary<Move, double>();
            for (int color = 1; color < _colorCount+1; color++)
            {
                foreach (KeyValuePair<List<Point>, List<Point>> entry in areasAndBorders)
                {
                    var areaCells = entry.Key;
                    var borderCells = entry.Value.Where(c => data[c.Y, c.X] == color).ToList();

                    var borderCellsScores = borderCells.Select(

                        cell =>
                        {
                            double  score = Enumerable.Range(1, _colorCount).Select(
                                c => c == color ? -blockingScores[cell.Y, cell.X, c] : blockingScores[cell.Y, cell.X, c]).Sum();
                            //score += bottleNeckScores[cell.Y, cell.X] * BottleNeckWeight;
                            return score;
                        } 
                    );
                    if (borderCellsScores.Count() == 0)
                        continue;
                    var mostBlockingScore = borderCellsScores.Max();
                    var mostBlockingOccupiedCell = borderCells[Array.IndexOf(borderCellsScores.ToArray(), mostBlockingScore)];

                    var areaCellsScores = areaCells.Select(
                        cell => Enumerable.Range(1, _colorCount).Select(
                            c => c == color ? -blockingScores[cell.Y, cell.X, c] : blockingScores[cell.Y, cell.X, c]).Sum()
                    );
                    var leastBlockingScore = areaCellsScores.Min();
                    var leastBlockingEmptyCell = areaCells[Array.IndexOf(areaCellsScores.ToArray(), leastBlockingScore)];

                    var distance = Math.Abs(mostBlockingOccupiedCell.X - leastBlockingEmptyCell.X) + Math.Abs(mostBlockingOccupiedCell.X - leastBlockingEmptyCell.X);
                    var award = (mostBlockingScore - leastBlockingScore);// * Math.Log(distance);
                    moveCandidates.Add(new Move() { StartPoint = mostBlockingOccupiedCell, EndPoint = leastBlockingEmptyCell }, award);
                }
            }
            var maxAward = moveCandidates.Values.Max();
            move = moveCandidates.Where(entry => entry.Value == maxAward).FirstOrDefault().Key;
            return move;
        }
        public Move GetMoveOld(int[,] data)
        {
            this.data = data;
            int _height = data.GetLength(0);
            int _width = data.GetLength(1);

            uncompletedLines = GetUncompletedLines(data);
            var areasAndBorders = GetAreasAndBorders(data);
            //printer.PrintField(blockingScores.GetValue([0], "blockingScores:", false);

            var emptyCell = MoveProcessor.GetMatchingCells(data, IsEmptyFilter, 1).FirstOrDefault();
            Point occupiedCell = MoveProcessor.GetNearestOccupiedCell(data, emptyCell);

            //TODO:
            //1. select all lines of a given length. 
            //2. find colors of the line empty cells
            //3. among borders of corresponding areas, find cell with maximum blocking score. choose it as the best ball
            
            uncompletedLines.Any(
                line =>
                {
                    var color = line.LineProperties.MainColor;

                    var lineCells = line.Cells.Where(c => c.BallColor == EmptyCellValue).ToList();
                    lineCells.Sort(
                        (c1, c2) => c1.OccupiedCellsAround.CompareTo(c2.OccupiedCellsAround)
                        );
                    lineCells.Reverse();
                    var emptyCells = lineCells.Select(c => c.Location).ToList();

                    var lineOccupiedCells = line.Cells.Where(c => c.BallColor == color).Select(c => c.Location); 
                    return  emptyCells.Any(
                        cell =>
                        {
                            var area = areasAndBorders.Keys.FirstOrDefault(list => list.Any(p => p.X == cell.X && p.Y == cell.Y));
                            var borders = areasAndBorders[area];

                            var occupiedCells = borders.Where(
                                 p =>
                                    data[p.Y, p.X] == color
                                ).ToList();

                            occupiedCells = occupiedCells.Except(lineOccupiedCells).ToList();

                            if (occupiedCells.Count() > 0)
                            {
                                emptyCell = cell;
                                occupiedCell = occupiedCells.ToList()[0];
                                return true;
                            }
                            return false;
                        }
                    );
                }
                );
            return new Move { StartPoint = occupiedCell, EndPoint = emptyCell };
        }

        List<Line> linesToComplete = new List<Line>();

        public double BottleNeckWeight { get => bottleNeckWeight; set => bottleNeckWeight = value; }
        public double DistanceWeight { get => distanceWeight; set => distanceWeight = value; }

        Dictionary<List<Point>, List<Point>> GetAreasAndBorders(int[,] inputData)
        {
            var preparedData = MoveProcessor.PrepareData(inputData);

            Func<int, bool> EmptyCellFilter = (x) => { return x == (int)CellType.Empty; };
            var emptyCells = MoveProcessor.GetMatchingCells(preparedData, EmptyCellFilter, 1);

            Dictionary<List<Point>, List<Point>> areasAndBorders = new Dictionary<List<Point>, List<Point>>();
            while (emptyCells.Count() > 0)
            {
                var start = emptyCells[0];
                List<Point> markedCells = new List<Point>();
                markedCells.Add(start);
                List<Point> lastMarkedCells = new List<Point>();
                var occupiedCells = new List<Point>();
                lastMarkedCells.Add(start);
                int markValue = 0;
                preparedData[start.Y, start.X] = markValue;
                while (lastMarkedCells.Count > 0)
                {
                    MoveProcessor.MarkNextCells(preparedData, lastMarkedCells, ++markValue, occupiedCells);
                    markedCells.AddRange(lastMarkedCells);
                }
                
                areasAndBorders.Add(markedCells, occupiedCells.Distinct().ToList());
                emptyCells = MoveProcessor.GetMatchingCells(preparedData, EmptyCellFilter, 1);
            }
            return areasAndBorders;
        }

        
        LineProperties GetLineProperties(int[] cellValues)
        {
            var lineProperties = new LineProperties();
            int emptyCellsCount = cellValues.Where(v => v == EmptyCellValue).Count();
            if (emptyCellsCount == cellValues.Length) 
                return new LineProperties() { MainColorBallsCount = 0,  MainColor = 0, EmptyCellsCount = cellValues.Length, Length  = cellValues.Length };

            int[] counters = new int[_colorCount + 1];
            var colorCounts = cellValues.Where(v => v != EmptyCellValue).Select(v => ++counters[v]).ToList();
            int ballCellsCount = counters.Skip(1).Max();

            return new LineProperties() { 
                MainColorBallsCount = colorCounts.Max(),
                MainColor = Array.IndexOf(counters, ballCellsCount), 
                EmptyCellsCount = emptyCellsCount, 
                Length = cellValues.Length };
        }

        

        List<Line> CalculateLines(int[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            List<Line> lines = new List<Line>();
            blockingScores = new double[height, width, _colorCount+1];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - minBallsInLine + 1; j++)
                {
                    var row = CustomArray<int>.GetSubRow(data, i, j, minBallsInLine);
                    var occupiedCellsAroud = GetOccupiedInlineCellsAround(row);

                    var lineProps = GetLineProperties(row);
                    if (lineProps.MainColorBallsCount > 0)
                    {
                        for (int k = 0; k < row.Length; k++)
                        {
                            int stepsToComplete = GetStepsNumberToComplete(lineProps);
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

            for (int i = 0; i < height-minBallsInLine + 1; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var col = CustomArray<int>.GetSubColumn(data, j, i, minBallsInLine);
                    var occupiedCellsAroud = GetOccupiedInlineCellsAround(col);
                    var lineProps = GetLineProperties(col);
                    
                    if (lineProps.MainColorBallsCount > 0)//>= minBallsInLine - 3)
                    {
                        for (int k = 0; k < col.Length; k++)
                        //if (col[k] != lineProps.MainColor)// && col[k] != EmptyCellValue)
                        {
                            int stepsToComplete = GetStepsNumberToComplete(lineProps);
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

        List<Line> GetUncompletedLines(int[,] data) {
            var lines = CalculateLines(data);
            lines = lines.Where(l => l.LineProperties.HasSingleColor).ToList();
            lines.Sort((p1, p2) => p1.LineProperties.MainColorBallsCount.CompareTo(p2.LineProperties.MainColorBallsCount));
            lines.Reverse();
            return lines;
        }

        private List<int> GetOccupiedInlineCellsAround(int[] row)
        {
            var rowEx = row.ToList();
            rowEx.Insert(0, 0);
            rowEx.Add(0);

            var occupiedCellsAround = Enumerable.Range(1, minBallsInLine)
              .Select(index => Math.Min(rowEx[index - 1], 1) + Math.Min(rowEx[index + 1], 1))
              .ToList();

            return occupiedCellsAround;
        }
    }
}