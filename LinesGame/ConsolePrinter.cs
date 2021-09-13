using LinesAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinesGame
{
    public class EmptyPrinter : IPrinter
    {
        public void PrintField<T>(T[,] data, string label = "", bool useLetters = true)
        {
        }

        public void PrintScore(int score)
        {
            ;
        }
    }

    public class ConsolePrinter: IPrinter
    {
        public void PrintField<T>(T [,] data , string label = "", bool useLetters = true)
        {
            char[] letters = new char[] {'*', '*', '0', 'R', 'G', 'B', 'Y', 'P', 'T', 'N' };
            char[] numbers = new char[] {'*', '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'M' };
            char[] wildcards = useLetters ? letters : numbers;
            int minValue = -2;
            Console.WriteLine(label);
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    var value = Math.Max(Convert.ToInt32(data[i, j]), minValue) - minValue;
                    value = Math.Min(value, wildcards.Length - 1);
                    Console.Write(wildcards[value] + " ");
                }
                    
                Console.WriteLine();
            }
        }

        public void PrintScore(int score)
        {
            Console.WriteLine("Score: {0}", score);
        }
    }
}
