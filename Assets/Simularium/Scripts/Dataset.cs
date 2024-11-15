using UnityEngine;

namespace Simularium
{
    [CreateAssetMenu(fileName = "Dataset", menuName = "Simularium/Dataset", order = 1)]
    public class Dataset : ScriptableObject
    {
        public float[] positions;
    }
}