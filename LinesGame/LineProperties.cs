using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LinesGame
{
    public class Cell
    {
        public Point Point { get; set; }
        public int Color { get; set; }
    }
    public class LineCell
    {
        public Point Location { get; set; }
        public int BallColor { get; set; }
        public int OccupiedCellsAround { get; set; }
    }

    public class Line
    {
        public LineProperties LineProperties { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }

        public List<LineCell> Cells { get; set; }
    }

    public class LineProperties
    {
        public int MainColor { get; set; }
        public int MainColorBallsCount { get; set; }
        public int EmptyCellsCount { get; set; }
        public int Length { get; set; }
        public bool HasSingleColor { get { return MainColorBallsCount + EmptyCellsCount == Length && EmptyCellsCount < Length; } }
        public double GapScore { get; set; }
        public double LineColorCenter { get; set; }
    }
}
