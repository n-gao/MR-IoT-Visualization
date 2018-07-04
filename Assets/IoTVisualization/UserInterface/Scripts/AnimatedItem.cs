using HoloToolkit.Unity;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This compopnent adds easy to use capabilities to display and hide and menu item.
    /// Therefore it has a local position for each state and the ability to disable the 
    /// whole GameObject when hidden.
    /// </summary>
	public class AnimatedItem : MonoBehaviour
	{
	    private enum State
	    {
	        Idle,
            Animated,
            Finished
	    }
        /// <summary>
        /// If this is set to true, the GameObject will be disabled when the item is hidden.
        /// </summary>
	    public bool DisableGameObject = true;
        /// <summary>
        /// Duration of the animation.
        /// </summary>
		public float Duration = 0.5f;
        /// <summary>
        /// Local position when the item is visible.
        /// </summary>
		public Vector3 VisiblePosition = Vector3.zero;
        /// <summary>
        /// Local position when the item is hidden.
        /// </summary>
		public Vector3 HiddenPosition = -Vector3.up;

		private Vector3 _targetPosition;
        /// <summary>
        /// Setting the TargetPosition causes the animation to start.
        /// </summary>
		private Vector3 TargetPosition
		{
			get { return _targetPosition; }
			set
			{
				_lastPosition = _targetPosition;
				_targetPosition = value;
				_change = TargetPosition - _lastPosition;
			}
		}

		private Vector3 _lastPosition;
		private Vector3 _change;
        
		private float _startTime = 0;

		private bool _visible = false;

        private State _state = State.Idle;

	    // Use this for initialization
		void Awake ()
		{
		    MenuColumn coloumn = GetComponentInParent<MenuColumn>();
            if (coloumn != null)
                coloumn.MenuItems.Add(this);
		}
	
		// Update is called once per frame
		void Update ()
		{
		    switch (_state)
		    {
		        case State.Idle:
		            return;
                case State.Animated:
                    Animate();
                    break;
                case State.Finished:
                    FinishAnimation();
                    break;
            }
		}

	    private void Animate()
	    {
	        float timeSinceStart = Time.time - _startTime;
	        transform.localPosition = _lastPosition.EaseInOut(_change, timeSinceStart, Duration);
	        if (timeSinceStart >= Duration)
	            _state = State.Finished;
        }

	    private void FinishAnimation()
	    {
	        if (!_visible)
	            gameObject.SetActive(!DisableGameObject);
	        transform.localPosition = _targetPosition;
	        _state = State.Idle;
        }

        /// <summary>
        /// Displays this item.
        /// </summary>
        /// <param name="instant">If set to true the animation will be skipped.</param>
		public void Show(bool instant = false)
		{
			_visible = true;
			gameObject.SetActive(true);
			_startTime = instant ? Time.time - Duration : Time.time;
			TargetPosition = VisiblePosition;
		    if (instant) transform.localPosition = _targetPosition;
            _state = State.Animated;
        }

        /// <summary>
        /// Hides this item.
        /// </summary>
        /// <param name="instant">If set to true the animation will be skipped.</param>
		public void Hide(bool instant = false)
		{
			_visible = false;
		    _startTime = instant ? Time.time - Duration : Time.time;
            TargetPosition = HiddenPosition;
		    if (instant) transform.localPosition = _targetPosition;
		    _state = State.Animated;
        }
	}
}
