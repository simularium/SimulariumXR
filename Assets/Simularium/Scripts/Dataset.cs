using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    [System.Serializable]
    public class Dataset : ScriptableObject
    {
        public static int MAX_AGENTS = 1000000;

        public string name;
        public int totalSteps;
        public int[] nAgents;
        public Color lineColor;
        public List<FrameData> frames;
    }
}