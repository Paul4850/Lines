using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class Line
    {
        public LineProperties LineProperties { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }

        public Dictionary<Point, int> Cells { get; set; }
        //public static Line Empty { get { return new Line {  BallsCount = 0, Color = 0, Start = Point.Empty, End = Point.Empty}; } }
        //public static bool IsEmpty(Line line) { return line.BallsCount == 0 && line.Color == 0 && line.Start == Point.Empty && line.End == Point.Empty; } 
    }

    public class LineProperties {
        public int Color { get; set; }
        public int OneColorBallsCount { get; set; }
        public int EmptyCellsCount { get; set; }
        public int Length { get; set; }
        public bool HasSingleColor { get { return OneColorBallsCount + EmptyCellsCount == Length && EmptyCellsCount < Length; } }
    }

    public class SimpleStrategy : StrategyBase
    {
        public const int EmptyCellValue = 0;

        int minBallsInLine = 5;
        public SimpleStrategy(int minBallsInLine)
        {
            this.minBallsInLine = minBallsInLine;
        }
        public override Move GetMove(int[,] data)
        {
            var lines = GetUncompletedLines(data);
            var areasAndBorders = GetAreasAndBorders(data);

            var emptyCell = MoveProcessor.GetMatchingCells(data, IsEmptyFilter, 1).FirstOrDefault();
            Point occupiedCell = MoveProcessor.GetNearestOccupiedCell(data, emptyCell);

            lines.Any(
                line =>
                {
                    var color = line.LineProperties.Color;
                    var emptyCells = line.Cells.Where(c => c.Value == EmptyCellValue).ToDictionary(p => p.Key, p=> p.Value).Keys.ToList();
                    var lineOccupiedCells = line.Cells.Where(c => c.Value == color).ToDictionary(p => p.Key, p => p.Value).Keys.ToList();
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
                return new LineProperties() { OneColorBallsCount = 0,  Color = 0, EmptyCellsCount = cellValues.Length, Length  = cellValues.Length };

            int ballColor = cellValues.FirstOrDefault(v => v != EmptyCellValue);
            int ballCellsCount = cellValues.Where(v => v == ballColor).Count();

            return new LineProperties() { 
                OneColorBallsCount = ballCellsCount,  
                Color = ballColor, 
                EmptyCellsCount = emptyCellsCount, 
                Length = cellValues.Length };
        }

        List<Line> GetUncompletedLines(int[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            List<Line> lines = new List<Line>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - minBallsInLine + 1; j++)
                {
                    var row = CustomArray<int>.GetSubRow(data, i, j, minBallsInLine);
                    var lineProps = GetLineProperties(row);
                    if (lineProps.HasSingleColor)
                    {
                        var newLine = new Line() { Start = new Point(j, i), End = new Point(j + minBallsInLine - 1, i), LineProperties = lineProps,
                            Cells = new  Dictionary<Point, int>()
                        };
                        Enumerable.Range(j, minBallsInLine).ToList().ForEach(x => newLine.Cells.Add(new Point(x, i), row[x-j]));
                        lines.Add(newLine);
                    }
                        
                }
            }

            for (int i = 0; i < height-minBallsInLine + 1; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var col = CustomArray<int>.GetSubColumn(data, j, i, minBallsInLine);
                    var lineProps = GetLineProperties(col);
                    if (lineProps.HasSingleColor)
                    { 
                        var newLine = new Line()
                        {
                            Start = new Point(j, i),
                            End = new Point(j, i + minBallsInLine - 1),
                            LineProperties = lineProps,
                            Cells = new Dictionary<Point, int>()
                        };
                        Enumerable.Range(i, minBallsInLine).ToList().ForEach(y => newLine.Cells.Add(new Point(j, y), col[y - i]));
                        lines.Add(newLine);
                    }
                }
            }

            lines.Sort((p1, p2) => p1.LineProperties.OneColorBallsCount.CompareTo(p2.LineProperties.OneColorBallsCount));
            lines.Reverse();
            return lines;
        }
    }
}