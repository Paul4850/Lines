using LinesGame;
using System;

namespace Lines
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            int maxMoves = 11;
            var options = new FieldOptions {  Height  = 7, Width = 7, ColorNumber = 3, MinBallsInLine = 3  };
            var game = new Game(options);
            game.Start();
            int moves = 0;
            while (game.Pass() && moves++ < maxMoves) 
                Console.WriteLine(moves);
            
            Console.ReadLine();
        }
    }
}
