using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Simularium;

public class DatasetMenu : MonoBehaviour 
{
    public Dataset[] datasets;
    public Transform scrollViewContent;
    public DatasetButton activeButton;

    static DatasetMenu _Instance;
    public static DatasetMenu Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindAnyObjectByType<DatasetMenu>();
            }
            return _Instance;
        }
    }

    public Dataset currentDataset
    {
        get 
        {
            if (activeButton != null)
            {
                return activeButton.dataset;
            }
            return null;
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
            button.SetDataset( datasets[d], d < 1 );
            if (d < 1)
            {
                activeButton = button;
            }
        }
    }

    public void SetActiveButton (DatasetButton button)
    {
        if (button != activeButton)
        {
            if (activeButton != null)
            {
                activeButton.Deselect();
            }
            activeButton = button;
        }
    }
}