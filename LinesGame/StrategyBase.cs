using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LinesGame
{
    public interface IStrategy
    {
        Move GetMove(int[,] data);
    }

    public class Move
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
    }

    public abstract class StrategyBase : IStrategy
    {
        protected bool IsEmptyFilter(int value) { return value == 0; }
        protected bool IsOccupiedFilter(int value) { return value != 0; }
        public abstract Move GetMove(int[,] data);

        public int[,] CopyArray(int[,] input)
        {
            int[,] result = new int[input.GetLength(0), input.GetLength(1)]; //Create a result array that is the same length as the input array
            for (int x = 0; x < input.GetLength(0); ++x) //Iterate through the horizontal rows of the two dimensional array
            {
                for (int y = 0; y < input.GetLength(1); ++y) //Iterate throught the vertical rows, to add more dimensions add another for loop for z
                {
                    result[x, y] = input[x, y]; //Change result x,y to input x,y
                }
            }
            return result;
        }
    }
}
