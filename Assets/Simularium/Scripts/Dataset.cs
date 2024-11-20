using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    [System.Serializable]
    public class Dataset : ScriptableObject
    {
        public static int MAX_AGENTS = 1000000;

        public int totalSteps;
        public int[] nAgents;
        public Dictionary<string, DisplayData> displayData;
        public List<FrameData> frames;
    }

    [System.Serializable]
    public class DisplayData
    {
        public Mesh mesh;
        public Color color;
    }
}