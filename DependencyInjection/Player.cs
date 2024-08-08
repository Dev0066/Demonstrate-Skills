using UnityEngine;
using Scoring;

namespace GamePlay
{
    public interface IPlayer
    {
        void Move();
        void CollectCoin(ICoin coin);
    }

    public class Player : IPlayer
    {
        private readonly IEnemy _enemy;
        private readonly IScoreManager _scoreManager;

        public Player(IEnemy enemy, IScoreManager scoreManager)
        {
            _enemy = enemy;
            _scoreManager = scoreManager;
        }

        public void Move()
        {
            Debug.Log("Player is moving somehow");
        }

        public void CollectCoin(ICoin coin)
        {
            _scoreManager.AddScore(coin.Value);
            Debug.Log("Coin collected!");
        }
    }
}

