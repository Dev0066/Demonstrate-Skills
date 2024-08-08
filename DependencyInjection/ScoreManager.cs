namespace Scoring
{
    public interface IScoreManager
    {
        void AddScore(int score);
        int GetScore();
    }

    public class ScoreManager : IScoreManager
    {
        private int _score;

        public void AddScore(int score)
        {
            _score += score;
        }

        public int GetScore()
        {
            return _score;
        }
    }

}

