using Lines;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class MoveProcessor
    {
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

        public static int CalcScore(Dictionary<int, List<Point>> changes)
        {
            int score = 0;
            
            int minLength = 5;

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
