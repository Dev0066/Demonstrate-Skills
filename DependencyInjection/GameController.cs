using System.Diagnostics;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class GameController : MonoBehaviour
    {
        [Inject] private IPlayer _player;
        [Inject] private IEnemy _enemy;
        [Inject] private IScoreManager _scoreManager;

        private void Start()
        {
            _player.Move();
            _enemy.Attack();

            var coin = new Coin(10); // Normally, the coin would be instantiated by Zenject
            _player.CollectCoin(coin);

            Debug.Log($"Current Score: {_scoreManager.GetScore()}");
        }
    }
}

