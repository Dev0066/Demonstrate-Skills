using System.Diagnostics;
using UnityEngine;

namespace GamePlay
{
    public interface IEnemy
    {
        void Attack();
    }

    public class Enemy : IEnemy
    {
        public void Attack()
        {
            Debug.Log("Enemy attacks!");
        }
    }
}

