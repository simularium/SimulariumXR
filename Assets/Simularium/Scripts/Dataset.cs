using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    public class Dataset : ScriptableObject
    {
        public static int MAX_AGENTS = 1000000;

        public int totalSteps;
        public int[] nAgents;
        public List<float[,]> positions = new List<float[,]>();
        public float agentScale;
    }
}