using Machete.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Machete.InputSystem
{
    public class TouchManager : Manager<TouchManager>
    {
        public UnityAction<Vector3, Vector3, Vector3> OnTouchDrag;
        public UnityAction<Vector3, Vector3, Vector3> OnTouchFixedDrag;

        public UnityAction<Vector3> OnTouchUp;
        public UnityAction<Vector3> OnTouchFixedUp;

        public UnityAction<Vector3> OnTouchDown;
        public UnityAction<Vector3> OnTouchFixedDown;

        private Vector3 startPosition;
        private Vector3 fixedStartPosition;

        private Vector3 currentPosition;
        private Vector3 fixedCurrentPosition;

        private Vector3 lastPosition;
        private Vector3 fixedLastPosition;

        private Vector3 deltaPosition;
        private Vector3 fixedDeltaPosition;

        private Vector3 totalDeltaPosition;
        private Vector3 fixedTotalDeltaPosition;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                startPosition = Input.mousePosition;
                lastPosition = startPosition;
                OnTouchDown?.Invoke(startPosition);
            }
            else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                OnTouchUp?.Invoke(lastPosition);
            }
            else if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                currentPosition = Input.mousePosition;
                deltaPosition = lastPosition - currentPosition;
                lastPosition = currentPosition;
                totalDeltaPosition = currentPosition - startPosition;

                OnTouchDrag?.Invoke(currentPosition, deltaPosition, totalDeltaPosition);
            }
        }

        private void FixedUpdate()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                fixedStartPosition = Input.mousePosition;
                fixedLastPosition = fixedStartPosition;
                OnTouchFixedDown?.Invoke(fixedStartPosition);
            }
            else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                OnTouchFixedUp?.Invoke(fixedLastPosition);
            }
            else if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                fixedCurrentPosition = Input.mousePosition;
                fixedDeltaPosition = fixedLastPosition - fixedCurrentPosition;
                fixedLastPosition = fixedCurrentPosition;
                fixedTotalDeltaPosition = fixedCurrentPosition - startPosition;

                OnTouchFixedDrag?.Invoke(fixedCurrentPosition, fixedDeltaPosition, fixedTotalDeltaPosition);
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