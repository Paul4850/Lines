using Lines;
using LinesGame;
using System;
using System.Drawing;

namespace LinesAPI
{
    public class LinesEnvironment
    {
        public string Name { get { return "Lines v01"; } }
        bool Move(Point startPoint, Point endpoint)
        {
            bool canMove = _gameField.CanMove(startPoint, endpoint);
            if (canMove)
                _gameField.Move(startPoint, endpoint);
            return canMove;
        }

        GameOptions _fieldOptions;
        IGameField _gameField = null;
        public LinesEnvironment(GameOptions fieldOptions)
        {
            _fieldOptions = fieldOptions;
        }
        
        public void Step(Point start, Point end)
        {
            Move(start, end);
        }

        public void Reset()
        {
            _gameField = new GameField(_fieldOptions);
            _gameField.GenerateBalls();
        }

        public int[,] GetData()
        {
            return _gameField.Data;
        }

        public bool IsFinished
        {
            get { return !_gameField.CanGenerateBalls; }
        }

        public int GetScore()
        {
            return _gameField.Score;
        }
    }

}
