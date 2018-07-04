using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace IoTVisualization.Localization
{
	public class Moveable : MonoBehaviour, IInputClickHandler
	{

		public bool IsMoving = false;

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
			if (IsMoving)
			{
	        
			}
		}

		public void OnInputClicked(InputClickedEventData eventData)
		{
			IsMoving = !IsMoving;
		}
	}
}
