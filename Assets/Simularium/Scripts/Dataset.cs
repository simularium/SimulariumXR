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
        public List<FrameData> frames;
    }
}