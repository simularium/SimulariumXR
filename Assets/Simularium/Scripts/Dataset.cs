using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    [System.Serializable]
    public class Dataset : ScriptableObject
    {
        public static int MAX_AGENTS = 1000000;

        public string datasetName;
        public int totalSteps;
        public int[] nMeshAgents;
        public bool hasLines;
        public Color lineColor;
        public float lineThickness;
        public List<FrameData> frames;
    }
}