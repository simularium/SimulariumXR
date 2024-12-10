using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Simularium
{
    public class DatasetButton : MonoBehaviour 
    {
        public Dataset dataset;
        public TMP_Text text;

        public void SetDataset (Dataset _dataset)
        {
            dataset = _dataset;
            text.text = dataset.datasetName;
        }

        public void OnClick ()
        {
            DatasetMenu.Instance.SetDataset( dataset );
        }
    }
}