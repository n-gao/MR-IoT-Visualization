using UnityEngine;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// This component sets the GameObejct position to the position of the main camera and
    /// rotates the GameObject towards a given target.
    /// 
    /// This component is used to let the projector follow the cursor
    /// </summary>
	public class FollowingProjector : MonoBehaviour
	{
        /// <summary>
        /// Target which will be followed.
        /// </summary>
		[SerializeField] private Transform _target;

		// Use this for initialization
		void Start ()
		{
			if (_target == null)
				_target = Camera.main.transform;
		}
	
		// Update is called once per frame
		void Update ()
		{
			transform.position = Camera.main.transform.position;
			transform.LookAt(_target);
		}
	}
}
