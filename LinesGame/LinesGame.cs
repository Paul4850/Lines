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
        public Game(FieldOptions options, IPrinter printer )
        {
            _options = options;
            this.printer = printer;
            _gameField = new GameField(options, printer);
            _strategy = new RandomStrategy();
        }

        FieldOptions _options;
        private readonly IPrinter printer;
        GameStatus _gameStatus;
        int _gameScore;
        IGameField _gameField;
        IStrategy _strategy;
        int _movesCount = 0;

        public GameStatus Status { get { return _gameStatus; } }
        public int Score { get { return _gameScore; } }
        public int MoveCount { get { return _movesCount; } }

        public void SetStrategy(IStrategy strategy)
        {
            _strategy = strategy;
        }
        
        public void Start()
        {
            _gameField = new GameField(_options, printer);
            _gameStatus = GameStatus.Active;
            _gameField.GenerateBalls();
            _gameScore = 0;
            _movesCount = 0;
        }

        public void Play()
        {
            while(_gameField.CanGenerateBalls)
            {
                var move = _strategy.GetMove(_gameField.Data);
                if (!_gameField.CanMove(move.StartPoint, move.EndPoint))
                    break;
                Move(move.StartPoint, move.EndPoint);
                _movesCount++;
            }
            _gameScore = _gameField.Score;
        }

        bool Move(Point startPoint, Point endpoint)
        {
            bool canMove = _gameField.CanMove(startPoint, endpoint);
            if (canMove)
                _gameField.Move(startPoint, endpoint);
            return canMove;
        }

        bool Pass()
        {
            if (_gameStatus != GameStatus.Active)
                return false;
            if (!_gameField.CanGenerateBalls) {
                _gameScore = _gameField.Score;
                _gameStatus = GameStatus.Finished;
            }

            var point = _gameField.GetFirstEmptyPoint();
            if (point.HasValue)
            {
                var data = MoveProcessor.PrepareData(this._gameField.Data);
                MoveProcessor.MarkCells(data, point.Value);
                printer.PrintField(data, "Marked cells:", false);
            }

            _gameField.GenerateBalls();
            return true;
        }
    }
}