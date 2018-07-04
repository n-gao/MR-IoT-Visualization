using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component organizes the menu which follows the user. It should be attached
    /// to the root GameObject of the menu.
    /// </summary>
    public class FollowingMenu : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether the object which is being followed has been overwritten.
        /// </summary>
        private bool IsTransformOverwritten { get { return _overrideTransform != null; } }
        /// <summary>
        /// Use this transform to override the transform which this menu follows.
        /// </summary>
        private Transform _overrideTransform = null;
        /// <summary>
        /// The transform which the menu follows.
        /// Default value is the main camera.
        /// </summary>
        public Transform ToFollow;
        /// <summary>
        /// The distance to the followed object.
        /// </summary>
        public float Distance;
        /// <summary>
        /// An offset in y-Direction which will be applied to the menu.
        /// </summary>
        public float YOffset = -.5f;
        /// <summary>
        /// The angle width of a coloumn of the menu.
        /// </summary>
        public float ColumnAngleWidth = 15;
        /// <summary>
        /// Array containing all coloumns.
        /// </summary>
        public List<MenuColumn> Coloumns { get; private set; }
        /// <summary>
        /// Indicates whether the menu is open.
        /// </summary>
        private bool Open
        {
            get
            {
                for (int i = Coloumns.Count - 1; i >= 0; i--)
                {
                    var coloumn = Coloumns[i];
                    if (coloumn == null)
                    {
                        Coloumns.RemoveAt(i);
                        continue;
                    }
                    if (Coloumns[i].Open)
                        return true;
                }
                return false;
            }
        }
        /// <summary>
        /// The transform which the menu follows.
        /// </summary>
        public Transform FollowedTransform
        {
            get { return IsTransformOverwritten ? _overrideTransform : Camera.main.transform; }
        }

        private bool _lastState = false;

        void Awake()
        {
            Coloumns = new List<MenuColumn>();
        }

        // Use this for initialization
        void Start () {
            if (ToFollow == null)
            {
                ToFollow = Camera.main.transform;
            }
            _lastState = Open;
        }
	
        // Update is called once per frame
        void Update ()
        {
            bool open = Open;
            if (!open)
            {
                Vector3 targetPos = ToFollow.position + Vector3.ProjectOnPlane(ToFollow.forward, Vector3.up) * Distance;
                targetPos.y += YOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);
            }
            //Lock the menu when any coloumn is open and unlock it when all are closed
            if (_lastState != open)
            {
                if (open)
                {
                    LockMenuRotation();
                }
                else
                {
                    UnlockMenuRotation();
                }
            }
            _lastState = open;
        }

        /// <summary>
        /// Unlocks the menu so it continues to follow.
        /// </summary>
        private void UnlockMenuRotation()
        {
            InterpolatedBillboard.OverrideCenter = null;
            Destroy(_overrideTransform.gameObject);
            _overrideTransform = null;
            Destroy(GetComponent<WorldAnchor>());
        }

        /// <summary>
        /// Locks the menu at its place.
        /// </summary>
        private void LockMenuRotation()
        {
            InterpolatedBillboard.OverrideCenter = Camera.main.transform.position;
            _overrideTransform = new GameObject("MenuCameraPosition").transform;
            _overrideTransform.position = Camera.main.transform.position;
            _overrideTransform.rotation = Camera.main.transform.rotation;
            _overrideTransform.gameObject.AddComponent<WorldAnchor>();
            gameObject.AddComponent<WorldAnchor>();
        }
    }
}
