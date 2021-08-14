namespace Lines
{
    interface IGame
    {
        int GameScore { get; }
        GameStatus Status { get; }
        bool Move(Point startPoint, Point endpoint);
        bool Pass();
        void Start();
    }
}