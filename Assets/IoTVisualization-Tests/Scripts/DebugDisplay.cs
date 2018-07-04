using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DebugDisplay : Singleton<DebugDisplay> {

        public void SetText(string text)
        {
            GetComponent<TextMesh>().text = text;
        }
    }
}
