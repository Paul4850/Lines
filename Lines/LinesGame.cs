using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lines
{
    class Game
    {
        public Game()
        { 
        }
        GameStatus _gameStatus;
        int _gameScore;

        GameField gameField;

        public GameStatus GameStatus { get { return _gameStatus; } }
        public int GameScore { get { return _gameScore; } }


        public void GenerateBalls()
        {   

        }

        public bool Move(Point startPoint, Point endpoint)
        {
            bool canMove = CanMove(startPoint, endpoint);
            if(canMove)
            {
                DoMove(startPoint, endpoint);
                UpdateScore();
            }
            UpdateStatus();
            return canMove;
        }

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
    }


}
