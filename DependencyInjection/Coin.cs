using UnityEngine;

namespace GamePlay
{
    public interface ICoin
    {
        int Value { get; }
    }

    public class Coin : ICoin
    {
        public int Value { get; private set; }

        public Coin(int value)
        {
            Value = value;
        }
    }
}

