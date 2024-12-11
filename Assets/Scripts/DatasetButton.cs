using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Simularium;

public class DatasetButton : MonoBehaviour 
{
    public Dataset dataset;
    public TMP_Text text;
    public GameObject selectionBox;

    public void SetDataset (Dataset _dataset, bool selected)
    {
        dataset = _dataset;
        text.text = dataset.datasetName;
        selectionBox.SetActive( selected );
    }

    public void Deselect ()
    {
        selectionBox.SetActive( false );
    }

    public void OnClick ()
    {
        MenuManager.Instance.SwitchDataset( this );
    }
}