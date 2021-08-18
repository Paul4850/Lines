using LinesGame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lines
{
    public class Game : IGame
    {
        public Game()
        {
        }

        public Game(FieldOptions options)
        {
            _options = options;
            _gameField = new GameField(options);
        }

        FieldOptions _options;
        //int _width = 5;
        //int _height = 5;
        //int _colorNumber = 4;
        GameStatus _gameStatus;
        int _gameScore;
        IGameField _gameField;

        private void UpdateStatus()
        {
            throw new NotImplementedException();
        }

        private void UpdateScore()
        {
            throw new NotImplementedException();
        }

        void DoMove(Point startPoint, Point endpoint)
        {

        }

        bool CanMove(Point startPoint, Point endpoint)
        {
            return true;
        }

        public void Start()
        {
            _gameField = new GameField(_options);
            _gameStatus = GameStatus.Active;
            _gameField.GenerateBalls();
        }
        public GameStatus Status { get { return _gameStatus; } }
        public int GameScore { get { return _gameScore; } }

        public bool Move(Point startPoint, Point endpoint)
        {
            bool canMove = CanMove(startPoint, endpoint);
            if (canMove)
            {
                DoMove(startPoint, endpoint);
                UpdateScore();
            }
            UpdateStatus();
            return canMove;
        }

        public bool Pass()
        {
            if (_gameStatus != GameStatus.Active)
                return false;
            if (!_gameField.CanGenerateBalls)
                _gameStatus = GameStatus.Finished;

            var point = _gameField.GetFirstEmptyPoint();
            if (point.HasValue)
            {
                var data = MoveProcessor.PrepareData(this._gameField.Data);
                MoveProcessor.MarkCells(data, point.Value);
                ConsolePrinter.PrintField(data, false);
            }

            _gameField.GenerateBalls();
            return true;
        }
    }


}
