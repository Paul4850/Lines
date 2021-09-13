using System;
using System.Collections.Generic;
using System.Text;

namespace LinesAPI
{
    public interface IPrinter
    {
        void PrintField<T>(T[,] data, string label = "", bool useLetters = true);
        void PrintScore(int score);
    }
}
