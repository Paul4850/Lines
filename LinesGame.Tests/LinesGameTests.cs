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
            int width = 3;
            int height = 3;
            int colors = 2;

            game = new Game(width, height, colors);
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