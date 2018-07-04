using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This component contains a method to close the application.
    /// </summary>
	public class CloseButton : MonoBehaviour {
        
		public void Close()
		{
			Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
	}
}
