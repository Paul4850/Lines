using Lines;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public enum CellType { 
        Occupied = -2,
        Empty = -1
    }

    public class MoveProcessor
    {
        public static int[,] PrepareData(int[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            int[,] dataPrepared = new int[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    dataPrepared[i, j] = data[i, j] > 0 ? (int)CellType.Occupied : (int)CellType.Empty;

            return dataPrepared;
        }

        public static bool CanMove(int[,] data, Point start, Point end)
        {
            if (data[start.Y, start.X] == 0 || data[end.Y, end.X] == 0)
                return false;

            int[,] dataPrepared = PrepareData(data);
            MarkCells(dataPrepared, start);

            return dataPrepared[end.Y, end.X] != (int)CellType.Empty;
        }

        public static void MarkCells(int[,] data, Point start)
        {
            List<Point> lastMarkedCells = new List<Point>();
            lastMarkedCells.Add(start);
            int markValue = 0;
            data[start.Y, start.X] = markValue;
            while (lastMarkedCells.Count > 0)
                MarkNextCells(data, lastMarkedCells, ++markValue);
        }

        public static void MarkNextCells(int[,] data, List<Point> lastMarkedCells, int markValue)
        {
            if (lastMarkedCells.Count == 0)
                return;
            var cell = lastMarkedCells.FirstOrDefault();
            var cellsToMark = new List<Point>();

            lastMarkedCells.ForEach(cell =>
            {
                int top = Math.Max(0, cell.Y - 1);
                int left = Math.Max(0, cell.X - 1);
                int bottom = Math.Min(data.GetLength(0) - 1, cell.Y + 1);
                int right = Math.Min(data.GetLength(1) - 1, cell.X + 1);
                
                for (int i = top; i <= bottom; i++)
                    for (int j = left; j <= right; j++)
                    {
                        if (Math.Abs(cell.Y - i) == Math.Abs(cell.X - j))
                            continue;
                        if (data[i, j] == (int)CellType.Empty)
                        {
                            data[i, j] = markValue;
                            cellsToMark.Add(new Point(j, i));
                        }
                    }
             
            });

            lastMarkedCells.Clear();
            lastMarkedCells.AddRange(cellsToMark);
        }

        public static int[,] Transpose(int[,] input)
        {
            int[,] result = new int[input.GetLength(1), input.GetLength(0)]; 
            for (int x = 0; x < input.GetLength(0); ++x) 
            {
                for (int y = 0; y < input.GetLength(1); ++y) 
                {
                    result[y, x] = input[x, y]; 
                }
            }
            return result;
        }

        public static Dictionary<int, List<Point>> ProcessMove(int[,] data, int minBallsInLine = 5)
        {
            var changes = new Dictionary<int, List<Point>>();

            GetHorizontalLines(data, minBallsInLine, changes);
            GetVerticalLines(data, minBallsInLine, changes);

            if (changes.ContainsKey(0))
                changes.Remove(0);
            changes.Keys.ToList().ForEach(key => changes[key] = changes[key].Distinct().ToList());
            return changes;
        }

        private static void GetDiagonalLines(int[,] data, int minBallsInLine, Dictionary<int, List<Point>> changes)
        {
            //TODO: rewrite.
            //left-bottom to right-top

            var transposedData = Transpose(data);
            var transposedChanges = new Dictionary<int, List<Point>>();
            GetHorizontalLines(transposedData, minBallsInLine, transposedChanges);

            transposedChanges.Keys.ToList().ForEach(
                key => transposedChanges[key] = transposedChanges[key].Select(p => new Point(p.Y, p.X)).ToList());

            transposedChanges.Keys.ToList().ForEach(
                 key => {
                     if (!changes.ContainsKey(key))
                         changes.Add(key, new List<Point>());
                     changes[key].AddRange(transposedChanges[key]);
                 });
        }

        private static void GetVerticalLines(int[,] data, int minBallsInLine, Dictionary<int, List<Point>> changes)
        {
            var transposedData = Transpose(data);
            var transposedChanges = new Dictionary<int, List<Point>>();
            GetHorizontalLines(transposedData,  minBallsInLine, transposedChanges);

            transposedChanges.Keys.ToList().ForEach(
                key => transposedChanges[key] = transposedChanges[key].Select(p => new Point(p.Y, p.X)).ToList());

            transposedChanges.Keys.ToList().ForEach(
                 key => {
                     if (!changes.ContainsKey(key))
                         changes.Add(key, new List<Point>());
                     changes[key].AddRange(transposedChanges[key]);
                 });
        }

        private static void GetHorizontalLines(int[,] data, int minBallsInLine, Dictionary<int, List<Point>> changes)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                int startIndex = 0;
                int endIndex = 0;
                int startValue = data[i, startIndex];
                int length = 1;
                for (int j = 1; j < width; j++)
                {
                    if (data[i, j] == startValue)
                    {
                        length++;
                        endIndex++;
                    }
                    else
                    {
                        if (length >= minBallsInLine)
                        {
                            if (!changes.ContainsKey(startValue))
                            {
                                changes.Add(startValue, new List<Point>());
                            }

                            for (int k = startIndex; k <= endIndex; k++)
                            {
                                changes[startValue].Add(new Point(k, i));
                            }
                        }
                        startValue = data[i, j];
                        length = 1;
                        startIndex = endIndex = j;
                    }
                }
                if (length >= minBallsInLine)
                {
                    if (!changes.ContainsKey(startValue))
                    {
                        changes.Add(startValue, new List<Point>());
                    }

                    for (int k = startIndex; k <= endIndex; k++)
                    {
                        changes[startValue].Add(new Point(k, i));
                    }
                }
            }
        }

        public static int CalcScore(Dictionary<int, List<Point>> changes, int minLength = 5)
        {
            int score = 0;
            
            changes.Keys.ToList().ForEach(
                 c => {
                     int length = changes[c].Count;
                     score += (minLength + length) * (length - minLength + 1) / 2;
                 }
                ); 
            return score;
        }
    }
}
