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
        int[, ,] blockingScores = null;

        int[] lineWeights;
        public SimpleStrategy(int minBallsInLine, int colorCount, Size fieldSize)
        {
            this.minBallsInLine = minBallsInLine;
            _colorCount = colorCount;
            _fieldSize = fieldSize;
            _height = fieldSize.Height;
            _width = fieldSize.Width;
            blockingScores = new int[_height, _width, _colorCount + 1];
            lineWeights = new int[minBallsInLine+1] ;
            for (int i = 1; i < minBallsInLine + 1; i++)
                lineWeights[i] = i;
            lineWeights[minBallsInLine - 3] = 15;
            lineWeights[minBallsInLine - 2] = 40;
            lineWeights[minBallsInLine - 1] = 150;
        }


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

        public override Move GetMove(int[,] data)
        {
            Move move = new Move();
            uncompletedLines = GetUncompletedLines(data);
            var areasAndBorders = GetAreasAndBorders(data);

            Dictionary<Move, int> moveCandidates = new Dictionary<Move, int>();
            for (int color = 1; color < _colorCount+1; color++)
            {
                foreach (KeyValuePair<List<Point>, List<Point>> entry in areasAndBorders)
                {
                    var areaCells = entry.Key;
                    var borderCells = entry.Value.Where(c => data[c.Y, c.X] == color).ToList();

                    var borderCellsScores = borderCells.Select(
                        cell => Enumerable.Range(1, _colorCount).Select( 
                            c => c == color ? -blockingScores[cell.Y, cell.X, c] : blockingScores[cell.Y, cell.X, c]).Sum()
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

                    var award = mostBlockingScore - leastBlockingScore;
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
            blockingScores = new int[height, width, _colorCount+1];

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
                            //if (row[k] != lineProps.MainColor)// && row[k] != EmptyCellValue)
                                blockingScores[i, j+k, lineProps.MainColor] += lineWeights[lineProps.MainColorBallsCount];
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
                                blockingScores[i + k, j, lineProps.MainColor] += lineWeights[lineProps.MainColorBallsCount];
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