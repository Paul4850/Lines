using LinesGame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lines
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lines simulator");

            int maxMoves = 200;
            var options = new FieldOptions {  Height  = 9, Width = 9, ColorNumber = 7, MinBallsInLine = 5  };
            //var options = new FieldOptions {  Height  = 7, Width = 7, ColorNumber = 7, MinBallsInLine = 5  };
            //var options = new FieldOptions { Height = 4, Width = 4, ColorNumber = 3, MinBallsInLine = 3 };
            var printer = new EmptyPrinter();
            //var printer = new ConsolePrinter();
            var game = new Game(options, printer);
            game.SetStrategy(new SimpleStrategy(options.MinBallsInLine, options.ColorNumber, new Size(options.Width, options.Height)));
            int gamesCount = 10000;
            int gameNumber = 0;
            ulong totalScore = 0;
            Console.WriteLine("Start: {0}", DateTime.Now);
            long totalMoveCount = 0;
            while (gameNumber++ < gamesCount)
            {
                game.Start();
                game.Play();
                totalScore += (uint)game.Score;
                totalMoveCount += game.MoveCount;
                if (game.Score > 140)
                    Console.WriteLine("Game {0}, moves: {1}, score: {2}", gameNumber, game.MoveCount, game.Score);
            }

            Console.WriteLine("Everage score: {0},  moves {1}", totalScore/(double)gamesCount, totalMoveCount/gamesCount);
            Console.WriteLine("End: {0}", DateTime.Now);
            Console.ReadLine();
        }
    }
}
