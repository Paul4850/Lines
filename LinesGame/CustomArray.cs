using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class CustomArray<T>
    {
        public static T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        public static T[] GetSubRow(T[,] matrix, int rowNumber, int startIndex, int length)
        {
            return Enumerable.Range(startIndex, length)
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        public static T[] GetSubColumn(T[,] matrix, int columnNumber,int startIndex, int length)
        {
            return Enumerable.Range(startIndex, length)
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

    }
}
