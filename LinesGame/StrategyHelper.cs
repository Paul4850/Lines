using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class StrategyHelper
    {
        public static List<Point> GetCellsAround(Point cell, int[,] data)
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

        public static Dictionary<List<Point>, List<Point>> GetAreasAndBorders(int[,] inputData)
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

        public static int GetMaxColorCount(int[] cellValues, int colorCount)
        {
            int[] counters = new int[colorCount + 1];
            cellValues.ToList().ForEach(v => ++counters[v]);

            return  counters.Skip(1).Max();
        }


        public static LineProperties GetLineProperties(int[] cellValues, int colorCount)
        {
            int emptyCellsCount = cellValues.Where(v => v == StrategyBase.EmptyCellValue).Count();
            if (emptyCellsCount == cellValues.Length)
                return new LineProperties() { MainColorBallsCount = 0, MainColor = 0, EmptyCellsCount = cellValues.Length, Length = cellValues.Length };

            int[] counters = new int[colorCount + 1];
            cellValues.ToList().ForEach(v => ++counters[v]);

            int ballCellsCount = counters.Skip(1).Max();
            var mainColors = new List<int>();

            for (int i = 1; i < counters.Length; i++)
            {
                if (counters[i] == ballCellsCount)
                {
                    mainColors.Add(i);
                }
            }

            Dictionary<int, double> colorsByGaps = new Dictionary<int, double>();
            mainColors.ForEach(color => colorsByGaps.Add(color, GetGapScore(cellValues, color)));
            double gapScore = colorsByGaps.Count == 0 ? 0 : colorsByGaps.Values.Min();
            var mainColor = colorsByGaps.FirstOrDefault(entry => entry.Value == gapScore).Key;

            return new LineProperties()
            {
                GapScore = gapScore,
                MainColorBallsCount = ballCellsCount,
                MainColor = mainColor,
                EmptyCellsCount = emptyCellsCount,
                Length = cellValues.Length
            };
        }

        public static List<int> GetOccupiedInlineCellsAround(int[] row)
        {
            var rowEx = row.ToList();
            rowEx.Insert(0, 0);
            rowEx.Add(0);

            var occupiedCellsAround = Enumerable.Range(1, row.Length )
              .Select(index => Math.Min(rowEx[index - 1], 1) + Math.Min(rowEx[index + 1], 1))
              .ToList();

            return occupiedCellsAround;
        }

        public static double GetGapScore(int[] cellValues, int color)
        {
            double gapScore = 0;
            var gapLen = 0;
            int lastMainColorPosition = -1;
            for (int i = 0; i < cellValues.Length; i++)
            {
                if (cellValues[i] == color)
                {
                    //gapScore += gapLen * gapWeights[gapLen];
                    gapScore += gapLen * gapLen;
                    lastMainColorPosition = i;
                    gapLen = 0;
                }

                else
                {
                    if (lastMainColorPosition >= 0)
                        gapLen++;
                }
            }
            return gapScore;
        }

        public static int GetStepsNumberToComplete(LineProperties lineProperties, int minBallsInLine)
        {
            return lineProperties.EmptyCellsCount + 2 * (minBallsInLine - lineProperties.MainColorBallsCount - lineProperties.EmptyCellsCount);
        }
    }
}
