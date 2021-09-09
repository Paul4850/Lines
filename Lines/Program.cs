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
            var options = new FieldOptions { Height = 9, Width = 9, ColorNumber = 7, MinBallsInLine = 5 };
            //var options = new FieldOptions { Height = 7, Width = 7, ColorNumber = 5, MinBallsInLine = 5 };
            //var options = new FieldOptions { Height = 4, Width = 4, ColorNumber = 3, MinBallsInLine = 3 };
            //var printer = new EmptyPrinter();
            var printer = new ConsolePrinter();
            var game = new Game(options, printer);
            SimpleStrategy simpleStrategy = (new SimpleStrategy(options.MinBallsInLine, options.ColorNumber, new Size(options.Width, options.Height)));

            ImprovedStrategy improvedStrategy = (new ImprovedStrategy(options.MinBallsInLine, options.ColorNumber, new Size(options.Width, options.Height)));
            //strategy.BottleNeckWeight = 0.025;
            //strategy.DistanceWeight = 0.055;

            //strategy.BottleNeckWeight = 0.025;
            //strategy.DistanceWeight = 1.3;

            game.SetStrategy(improvedStrategy);
            int gamesCount = 500;
            int gameNumber = 0;
            double totalScore = 0;
            Console.WriteLine("Start: {0}", DateTime.Now);
            double totalMoveCount = 0;
            //Console.WriteLine("Game {0}, BottleNeckWeight:  , moves:  , score: {2:F3}");
            while (gameNumber++ < gamesCount)
            {
                double subtotalScore = 0;
                double subtotalMoves = 0;
                int batchSize = 1;
                for (int i = 0; i < batchSize; i++)
                {
                    game.Start();
                    game.Play();
                    subtotalScore += game.Score;
                    subtotalMoves+= game.MoveCount;
                }
                totalScore += subtotalScore/ batchSize;
                totalMoveCount += subtotalMoves/ batchSize;

                double avgScore = subtotalScore / batchSize;
                if (avgScore >= 250)
                    Console.WriteLine("Game {0}, DistanceWeight: {3:F3}, moves: {1:F3}, score: {2:F3}", gameNumber, subtotalMoves/ batchSize, avgScore, 0);
                //Console.WriteLine("Game {0}, moves: {1}, score: {2}", gameNumber, game.MoveCount, game.Score);
                //strategy.BottleNeckWeight += 0.005;
                //strategy.DistanceWeight += 0.1;
            }

            Console.WriteLine("Everage score: {0:F3},  moves {1:F3}", totalScore/(double)gamesCount, totalMoveCount/gamesCount);
            Console.WriteLine("End: {0}", DateTime.Now);
            Console.ReadLine();
        }

        static void PrintComb(int length)
        {
            int maxCombinations = 1 << length;
            for (int i = 1; i < maxCombinations; i++)
            {
                string res = "";
                for (int offset = 0; offset < length; offset++)
                {
                    var exists = i >> offset & 1;
                    res += exists;// (exists == 1? 1 : "o");
                }
                Console.WriteLine(res);
            }
        }

    }
}
