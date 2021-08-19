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
            //options = new FieldOptions { Height = 4, Width = 4, ColorNumber = 3, MinBallsInLine = 3 };
            var printer = new EmptyPrinter();
            //var printer = new ConsolePrinter();
            var game = new Game(options, printer);
            int gamesCount = 100000;
            int gameNumber = 0;
            ulong totalScore = 0;
            Console.WriteLine("Start: {0}", DateTime.Now);

            while (gameNumber++ < gamesCount)
            {
                game.Start();
                game.Play();
                totalScore += (uint)game.Score;
                if(game.Score > 15)
                    Console.WriteLine("Game {0}, moves: {1}, score: {2}", gameNumber, game.MoveCount, game.Score);
            }

            Console.WriteLine("Everage score: {0}", totalScore/(double)gamesCount);
            Console.WriteLine("End: {0}", DateTime.Now);
            Console.ReadLine();
        }
    }
}
