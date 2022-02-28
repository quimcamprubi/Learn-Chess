using UnityEngine;

namespace UI
{
    [CreateAssetMenu (menuName = "Theme/Board Colors")]
    public class BoardColors : ScriptableObject
    {
        public SquareColors lightColors;
        public SquareColors darkColors;
        [System.Serializable]
        public struct SquareColors
        {
            public Color normal;
            public Color legal;
            public Color selected;
        }
    }
}