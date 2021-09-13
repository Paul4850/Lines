using LinesAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{

    // TODO:
    // - pack balls within lines to fill gaps
    // - take ball for gap from center rather than from border
    // + remove balls of other color from almost completed lines. put them to complete other line if possible. 

    //ball selection criteria
    // - distance to the ball
    // + lines blocked by the ball
    // + lines including the ball
    // - distance to center

    //TODO:
    //1. [del] select all lines of a given length. 
    //2. [del] find colors of the line empty cells
    //3. + among borders of corresponding areas, find cell with maximum blocking score. choose it as the best ball

    public class SimpleStrategy : StrategyBase
    {
        int minBallsInLine = 5;
        int _colorCount = 7;
        Size _fieldSize = new Size();
        List<Line> uncompletedLines = new List<Line>();
        int[,] data = null;
        int _height = 0;
        int _width = 0;

        public SimpleStrategy(int minBallsInLine, int colorCount, Size fieldSize)
        {
            this.minBallsInLine = minBallsInLine;
            _colorCount = colorCount;
            _fieldSize = fieldSize;
            _height = fieldSize.Height;
            _width = fieldSize.Width;
        }

        public override Move GetMove(int[,] data)
        {
            this.data = data;
            int _height = data.GetLength(0);
            int _width = data.GetLength(1);

            uncompletedLines = GetUncompletedLines(data);
            var areasAndBorders = StrategyHelper.GetAreasAndBorders(data);

            var emptyCell = MoveProcessor.GetMatchingCells(data, IsEmptyFilter, 1).FirstOrDefault();
            Point occupiedCell = MoveHelper.GetNearestOccupiedCell(data, emptyCell);
            
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

        List<Line> CalculateLines(int[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            List<Line> lines = new List<Line>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width - minBallsInLine + 1; j++)
                {
                    var row = CustomArray<int>.GetSubRow(data, i, j, minBallsInLine);
                    var occupiedCellsAroud = StrategyHelper.GetOccupiedInlineCellsAround(row);
                    int leftColor = j > 0 ? data[i, j - 1] : -1;
                    int rightColor = j < width ? data[i, j + 1] : -1;

                    var lineProps = StrategyHelper.GetLineProperties(row, _colorCount);

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
    }
}