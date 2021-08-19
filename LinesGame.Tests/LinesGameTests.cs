using NUnit.Framework;
using Lines;

namespace LinesGame.Tests
{
    public class LinesGameTests
    {
        Game game;
        [SetUp]
        public void Setup()
        {
            //int width = 4;
            //int height = 4;
            //int colors = 2;
            var options = new FieldOptions { Height = 4, Width = 4, ColorNumber = 3, MinBallsInLine = 3 };
            game = new Game(options, new EmptyPrinter());
        }


        [Test]
        public void TestStartGame()
        {
            game.Start();
            Assert.IsTrue(game.Status == GameStatus.Active);
            Assert.Pass();
        }

        [Test, Timeout(2000)]
        public void TestBallsGeneration()
        {
            game.Start();
            while (game.Pass()) ;
            Assert.IsTrue(game.Status == GameStatus.Finished);
        }
    }
}