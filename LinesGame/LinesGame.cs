using System;
using System.Collections.Generic;
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

        public Game(int width, int height, int colorNumber)
        {
            _gameField = new GameField(width, height, colorNumber);
        }

        int _width = 5;
        int _height = 5;
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
            _gameField = new GameField(_width, _height);
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
            _gameField.GenerateBalls();
            return true;
        }
    }


}
