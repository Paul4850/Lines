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
        public abstract Move GetMove(int[,] data);
    }


}
