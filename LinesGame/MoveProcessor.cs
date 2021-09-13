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
        public static List<Point> GetMatchingCells(int[,] data, Func<int, bool> MatchFilter, int limit = 0)
        {
            int cellsLimit = limit;
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            if (cellsLimit == 0) cellsLimit = height * width;
            var cells = new List<Point>();

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    if (MatchFilter(data[i, j]))
                         cells.Add(new Point(j, i));
                    if (cells.Count == limit)
                        return cells;
                }
            return cells;
        }
    }
}
