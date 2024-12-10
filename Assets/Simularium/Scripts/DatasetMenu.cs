using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Simularium
{
    public class DatasetMenu : MonoBehaviour 
    {
        public Dataset[] datasets;
        public Transform scrollViewContent;
        public Dataset currentDataset;

        static DatasetMenu _Instance;
        public static DatasetMenu Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = GameObject.FindObjectOfType<DatasetMenu>();
                }
                return _Instance;
            }
        }

        void Awake ()
        {
            GenerateDatasetButtons();
        }

        void GenerateDatasetButtons ()
        {
            GameObject buttonPrefab = Resources.Load( "DatasetButton" ) as GameObject;
            for (int d = 0; d < datasets.Length; d++)
            {
                DatasetButton button = (Instantiate( buttonPrefab, scrollViewContent ) as GameObject).GetComponent<DatasetButton>();
                button.transform.localPosition = new Vector3( d * 175f, -100f, 0 );
                button.SetDataset( datasets[d] );
            }
        }

        public void SetDataset (Dataset dataset)
        {
            if (dataset != currentDataset)
            {
                currentDataset = dataset;
            }
        }
    }
}