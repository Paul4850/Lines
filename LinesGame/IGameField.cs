﻿using System.Drawing;

namespace Lines
{
    public interface IGameField
    {
        BallColor[] NextColorsToGenerate { get; }
        int[,] Data { get; }
        bool CanGenerateBalls { get; }
        bool CanMove(Point start, Point end);
        void GenerateBalls();
        void Move(Point start, Point end);
    }
}