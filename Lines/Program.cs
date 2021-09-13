using LinesAPI;
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

            var options = new GameOptions { Height = 9, Width = 9, ColorNumber = 7, MinBallsInLine = 5 };
            //var options = new GameOptions { Height = 7, Width = 7, ColorNumber = 5, MinBallsInLine = 5 };
            //var options = new GameOptions { Height = 4, Width = 4, ColorNumber = 3, MinBallsInLine = 3 };
            //var printer = new EmptyPrinter();
            var printer = new ConsolePrinter();
            SimpleStrategy simpleStrategy = (new SimpleStrategy(options.MinBallsInLine, options.ColorNumber, new Size(options.Width, options.Height)));

            ImprovedStrategy improvedStrategy = (new ImprovedStrategy(options.MinBallsInLine, options.ColorNumber, new Size(options.Width, options.Height)));
            improvedStrategy.BottleNeckWeight = 0.025;
            improvedStrategy.DistanceWeight = 0.055;

            double totalScore = 0;
            int totalGames = 500;
            int scoreThreshold = 200;

            Console.WriteLine("Start: {0}", DateTime.Now);
            for (int i = 0; i < totalGames; i++)
            {
                var score  = PlayGame(options, improvedStrategy);
                totalScore += score;
                if(score >= scoreThreshold)
                    Console.WriteLine("game {0}, score: {1:F1}", i, score);
            }
            Console.WriteLine("total games {0}, avg score: {1:F2}", totalGames, totalScore/ totalGames);
            Console.WriteLine("End: {0}", DateTime.Now);
            Console.ReadLine();
        }

        public static int PlayGame(GameOptions gameOptions, IStrategy strategy)
        {
            LinesEnvironment environment = new LinesEnvironment(gameOptions);
            environment.Reset();

            while (!environment.IsFinished)
            {
                var data = environment.GetData();
                var action = strategy.GetMove(data);
                environment.Step(action.StartPoint, action.EndPoint);
            }

            return environment.GetScore();
        }

    }
}
