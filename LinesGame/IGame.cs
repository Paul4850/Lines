﻿using LinesGame;
using System.Drawing;

namespace Lines
{
    interface IGame
    {
        int Score { get; }
        int MoveCount { get; }
        GameStatus Status { get; }
        void Play();
        void Start();
        void SetStrategy(IStrategy strategy);
    }
}