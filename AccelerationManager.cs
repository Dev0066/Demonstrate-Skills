using Machete.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Machete.InputSystem
{
    public class AccelerationManager : Manager<AccelerationManager>
    {
        public UnityAction OnShake;

        [SerializeField] private float accelerometerUpdateInterval = 1.0f / 60.0f;
        [SerializeField] private float lowPassKernelWidthInSeconds = 1.0f;
        [SerializeField] private float shakeDetectionThreshold = 2.0f;

        float lowPassFilterFactor;
        Vector3 lowPassValue;

        protected override void OnAwakeManager()
        {
            lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
            shakeDetectionThreshold *= shakeDetectionThreshold;
            lowPassValue = Input.acceleration;
        }

        private void Update()
        {
            Vector3 acceleration = Input.acceleration;
            lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
            Vector3 deltaAcceleration = acceleration - lowPassValue;

            if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
            {
                // Perform your "shaking actions" here. If necessary, add suitable
                // guards in the if check above to avoid redundant handling during
                // the same shake (e.g. a minimum refractory period).
                //Debug.Log("Shake event detected at time " + Time.time);
                OnShake?.Invoke();
            }
        }

        #region Manager related

        protected internal override bool IsPersistentManager()
        {
            return true;
        }

        protected internal override bool HasInitialization()
        {
            return false;
        }

        #endregion
    }
}
