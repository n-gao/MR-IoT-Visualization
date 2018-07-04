using System;
using System.Collections.Generic;
using System.Text;
using Assets.IoTVisualization_Tests.Scripts;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using IoTVisualization.Networking;
using IoTVisualization.Utils;
using IoTVisualization.Visualization;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace IoTVisualization.Localization
{
    /// <summary>
    /// This MonoBehaviour enables TapToPlace interaction with the attached GameObject.
    /// </summary>
    public class MyTapToPlace : MonoBehaviour, IInputClickHandler
    {
        private static MyTapToPlace _selected;
        /// <summary>
        /// Currently moved instance. if null none is currently moved.
        /// </summary>
        public static MyTapToPlace Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != null)
                    _selected.Drop();
                _selected = value;
                if (value != null)
                    value.PickUp();
            }
        }
        /// <summary>
        /// This event will be called when the user taps to pick up.
        /// </summary>
        public event Action PickedUp;
        /// <summary>
        /// This event will be called when the user taps to place it.
        /// </summary>
        public event Action Dropped;

        private Transform CameraTransform
        {
            get { return Camera.main.transform; }
        }

        private int Layer
        {
            get { return spatialMappingManager != null ? spatialMappingManager.LayerMask : 31; }
        }

        /// <summary>
        /// The velocity of the GameObject which is used when following the users gaze.
        /// </summary>
        public float Velocity = 5.0f;
        /// <summary>
        /// Default distance to the wall the user looks at.
        /// </summary>
        public float DistanceToWall = 0.1f;
        /// <summary>
        /// Step size in which the tests for a suitable position will be done.
        /// </summary>
        public float DistanceSteps = 0.1f;
        /// <summary>
        /// Max distance to a wall in front of the user. -1 = infinity
        /// </summary>
        public float MaxDistance = -1;
        /// <summary>
        /// Indicates whether this object is currently being moved.
        /// </summary>
        public bool IsMoved
        {
            get { return Selected == this; }
            set { Selected = value ? this : null; }
        }

        private SpatialMappingManager spatialMappingManager;

        private Vector3 _destPosition;
        private Quaternion _destRotation;

        // Use this for initialization
        void Start () {
            spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager == null)
            {
                Debug.LogWarning("This script expects that you have a SpatialMappingManager component in your scene.");
            }
        }
	
        // Update is called once per frame
        void Update () {
            if (IsMoved)
            {
                DeterminDestination();
                MoveToDestination();
            }
        }

        /// <summary>
        /// Moves the GameObject to the determined position
        /// </summary>
        private void MoveToDestination()
        {
            transform.position = Vector3.Lerp(transform.position, _destPosition, Time.deltaTime * Velocity);
            transform.rotation = Quaternion.Lerp(transform.rotation, _destRotation, Time.deltaTime * Velocity);
        }

        /// <summary>
        /// Determins the new position of the GameObject by casting a raycast using the main camera and testing whether it would collide with
        /// anything form the enviroment.
        /// </summary>
        private void DeterminDestination()
        {
            Vector3 eulerAngles = Camera.main.transform.localRotation.eulerAngles;
            _destRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0);

            RaycastHit hitInfo;
            var hit = Physics.Raycast(CameraTransform.position, CameraTransform.forward, out hitInfo, 30, Layer);

            if (!hit) return;
            if (hitInfo.distance > MaxDistance && MaxDistance > 0) return;

            float distance = DistanceToWall;

            _raycastBounds = new Bounds(hitInfo.point, Vector3.zero)
            {
                extents = GetComponent<VisualizationLayoutManager>().Size
            };

            while (Physics.OverlapBox(_raycastBounds.center, _raycastBounds.extents, transform.rotation, Layer).Length > 0)
            {
                distance += DistanceSteps;
                _raycastBounds.center -= DistanceSteps * CameraTransform.forward;
            }


            _destPosition = hitInfo.point - distance * CameraTransform.forward;

            Debug.DrawLine(CameraTransform.position, hitInfo.point, Color.red);
        }


        public void OnInputClicked(InputClickedEventData eventData)
        {
            IsMoved = !IsMoved;
        }

        /// <summary>
        /// Called when this is being picked up.
        /// </summary>
        private void PickUp()
        {
            InputManager.Instance.OverrideFocusedObject = gameObject;
            DestroyImmediate(GetComponent<WorldAnchor>());
            if (PickedUp != null)
                PickedUp();
        }

        /// <summary>
        /// Called when this is being dropped.
        /// </summary>
        private void Drop()
        {
            InputManager.Instance.OverrideFocusedObject = null;
            gameObject.AddComponent<WorldAnchor>();
            if (Dropped != null)
                Dropped();
        }

        /// <summary>
        /// Bound used to test for a suitable position.
        /// </summary>
        private Bounds _raycastBounds;

        /// <summary>
        /// Debug drawings
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(255, 0, 255, 0.25f);
            Gizmos.matrix = Matrix4x4.TRS(_raycastBounds.center, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, _raycastBounds.extents);
        }
    }
}
