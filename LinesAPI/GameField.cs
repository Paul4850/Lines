using LinesAPI;
using LinesGame;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        int score = 0;
        private int minBallsInLine = 5;
        IPrinter printer = null;

        public GameField(GameOptions options, IPrinter printer= null)
        {
            this.printer = printer;
            this.width = options.Width;
            this.height = options.Height;
            colorNumber = options.ColorNumber;
            minBallsInLine = options.MinBallsInLine;

            var maxColor = Enum.GetValues(typeof(BallColor)).Length;
            this.colorNumber = Math.Min(maxColor, colorNumber);

            Intialize();
        }

        public int[,] Data { get { return data; } }

        //public BallColor[] NextColorsToGenerate { get { return nextColorsToGenerate; } }

        private void Intialize()
        {
            data = new int[height, width];
            nextColorsToGenerate = new BallColor[ballNumberToGenerate];

            nextColorsToGenerate = Enumerable
                .Repeat(0, ballNumberToGenerate)
                .Select(i => (BallColor)colorRandom.Next(1, colorNumber + 1))
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
                printer?.PrintField(data);
                ProcessMove();
            }
            printer?.PrintField(data);
            printer?.PrintScore(Score);
        }

        private int ProcessMove()
        {
            var changes = MoveHelper.ProcessMove(data, minBallsInLine);
            Score += MoveHelper.CalcScore(changes, minBallsInLine);

            int cleanedCellsCount = 0;
            changes.Values.ToList().ForEach(
                   list =>
                   {
                       list.ForEach(
                           p => data[p.Y, p.X] = 0
                       );
                       cleanedCellsCount += list.Count;
                   }
                );
            emptyCellsNumber += cleanedCellsCount;
            return cleanedCellsCount;
        }

        public void Move(Point start, Point end)
        {
            bool canMove = CanMove(start, end);
            if (canMove)
            {
                DoMove(start, end);
                printer?.PrintField(data, "Move:");
                int cleanedCellsNumber = ProcessMove();
                canGenerateBalls = CanGenerateBalls;
                if (canGenerateBalls && cleanedCellsNumber == 0)
                {
                    DoGenerateBalls();
                    ProcessMove();
                }
                printer?.PrintField(data, "Process and generate:");
            }
        }
        public bool CanGenerateBalls => (emptyCellsNumber >= ballNumberToGenerate);

        public int Score { get => score; set => score = value; }

        private void DoGenerateBalls()
        {
            nextColorsToGenerate.ToList().ForEach(
                color =>
                {
                    var position = ballPositionRandom.Next(emptyCellsNumber - 1);
                    var emptyCellsCount = 0;
                    int x = 0;
                    int y = 0;
                    for (int i = 0; i < height; i++)
                    { 
                        for (int j = 0; j < width; j++)
                        {
                            if (data[i, j] == 0) emptyCellsCount++;
                            if (emptyCellsCount == position + 1)
                            {
                                x = j;
                                y = i;
                                break;
                            }
                        }
                        if (emptyCellsCount == position + 1)
                            break;
                    }
                    data[y, x] = (int)color;
                    emptyCellsNumber--;
                }
                );
            
            nextColorsToGenerate = Enumerable
                .Repeat(0, ballNumberToGenerate)
                .Select(i => (BallColor)colorRandom.Next(1, colorNumber + 1))
                .ToArray();
        }

        private void DoMove(Point start, Point end)
        {
            var val = data[start.Y, start.X];
            data[start.Y, start.X] = 0;
            data[end.Y, end.X] = val;
        }
    }
}
