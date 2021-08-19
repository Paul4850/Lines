using LinesGame;
using System;

namespace Lines
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            int maxMoves = 200;
            var options = new FieldOptions {  Height  = 9, Width = 9, ColorNumber = 7, MinBallsInLine = 5  };
            var printer = new EmptyPrinter();
            var game = new Game(options, printer);
            int gamesCount = 10000;
            int gameNumber = 0;
            while(gameNumber++ < gamesCount)
            { 
                game.Start();
                int moves = 0;
                while (game.Pass() && moves++ < maxMoves) ;
                if (game.GameScore > 5)
                    Console.WriteLine("Game {0}, moves: {1}, score: {2}", gameNumber, moves, game.GameScore);
            }
            Console.ReadLine();
        }
    }
}
