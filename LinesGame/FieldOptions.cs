﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LinesGame
{
    public class  FieldOptions
    {
        public FieldOptions() { }
        public int Height { get; set; }
        public int Width { get; set; }
        public int ColorNumber { get; set; }
        public int MinBallsInLine { get; set; }
    }
}
