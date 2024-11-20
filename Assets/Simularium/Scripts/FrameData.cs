using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    [System.Serializable]
    public class FrameData
    {
        [HideInInspector]
        public float[] transforms;
        public float[] colors;
    }
}