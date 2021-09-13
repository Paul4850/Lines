using LinesAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LinesGame
{
    public class RandomStrategy : StrategyBase
    {
        public override Move GetMove(int[,] data)
        {
            var emptyCell =  MoveProcessor.GetMatchingCells(data, IsEmptyFilter, 1).FirstOrDefault();
            Point occupiedCell = MoveHelper.GetNearestOccupiedCell(data, emptyCell);
            return new Move { StartPoint = occupiedCell, EndPoint = emptyCell };
        }
    }
}
