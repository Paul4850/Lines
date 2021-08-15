using System;

namespace Lines
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            int width = 4;
            int height = 4;
            int colors = 2;

            int maxMoves = 20;
            var game = new Game(width, height, colors);
            game.Start();
            while (game.Pass() && maxMoves-- > 0) ;
            

            Console.ReadLine();
        }
    }
}
