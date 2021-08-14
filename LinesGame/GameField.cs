using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lines
{
    public class GameField : IGameField
    {
        int width;
        int height;
        int colorNumber;
        bool canGenerateBalls = true;

        int[,] data;

        int ballNumberToGenerate = 3;
        BallColor[] nextColorsToGenerate;

        Random colorRandom = new Random();
        Random ballPositionRandom = new Random();

        int emptyCellsNumber = 0;

        public GameField(int width = 5, int height = 5, int colorNumber = 4)
        {
            this.width = width;
            this.height = height;

            var maxColor = Enum.GetValues(typeof(BallColor)).Length;
            this.colorNumber = Math.Max(maxColor, colorNumber);

            Intialize();
        }

        public int[,] Data { get { return data; } }

        public BallColor[] NextColorsToGenerate { get { return nextColorsToGenerate; } }

        private void Intialize()
        {
            data = new int[width, height];
            nextColorsToGenerate = new BallColor[ballNumberToGenerate];

            nextColorsToGenerate = Enumerable
                .Repeat(0, ballNumberToGenerate)
                .Select(i => (BallColor)colorRandom.Next(1, colorNumber))
                .ToArray();
            emptyCellsNumber = width * height;
        }

        public bool CanMove(Point start, Point end)
        {
            return canGenerateBalls;
        }

        public void GenerateBalls()
        {
            canGenerateBalls = CanGenerateBalls;
            if (canGenerateBalls)
            {
                DoGenerateBalls();
                ProcessMove();
            }
        }

        public void Move(Point start, Point end)
        {
            bool canMove = CanMove(start, end);
            if (canMove)
            {
                DoMove(start, end);
                canGenerateBalls = CanGenerateBalls;
                if (canGenerateBalls)
                {
                    DoGenerateBalls();
                    ProcessMove();
                }
            }
        }
        public bool CanGenerateBalls => (emptyCellsNumber >= ballNumberToGenerate);

        private void ProcessMove()
        {

        }


        private void DoGenerateBalls()
        {
            nextColorsToGenerate.ToList().ForEach(
                color =>
                {
                    var position = ballPositionRandom.Next(emptyCellsNumber - 1);
                    var emptyCellsCount = 0;
                    int x = 0;
                    int y = 0;
                    for (int i = 0; i < width; i++)
                    { 
                        for (int j = 0; j < height; j++)
                        {
                            if (data[i, j] == 0) emptyCellsCount++;
                            if (emptyCellsCount == position)
                            {
                                x = j;
                                y = i;
                                break;
                            }
                        }
                        if (emptyCellsCount == position)
                            break;
                    }
                    data[y, x] = (int)color;
                    emptyCellsNumber--;
                }
                );
            
            nextColorsToGenerate = Enumerable
                .Repeat(0, ballNumberToGenerate)
                .Select(i => (BallColor)colorRandom.Next(1, colorNumber))
                .ToArray();
        }

        private void DoMove(Point start, Point end)
        {
            throw new NotImplementedException();
        }

    }
}
