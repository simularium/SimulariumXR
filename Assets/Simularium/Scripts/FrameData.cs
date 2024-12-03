using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    [System.Serializable]
    public class FrameData
    {
        [HideInInspector]
        public float[] meshTransforms;
        [HideInInspector]
        public float[] meshColors;
    }
}