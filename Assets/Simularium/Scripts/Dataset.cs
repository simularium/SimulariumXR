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
        public float timeStep;
        public string timeLabel;
        public int targetFPS = 100;
        public int[] nMeshAgents;
        public bool hasLines;
        public Color lineColor;
        public float lineThickness;
        public List<FrameData> frames;
        public float globalScale = 1f;
    }
}