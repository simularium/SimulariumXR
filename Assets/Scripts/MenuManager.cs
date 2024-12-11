using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;
using Simularium;


public class MenuManager : MonoBehaviour
{
    public Dataset[] datasets;

    public Button switchButton;
    public GameObject switchButtonEnabled;
    public GameObject switchButtonDisabled;

    public Button deleteButton;
    public GameObject deleteButtonEnabled;
    public GameObject deleteButtonDisabled;

    public Animator datasetMenuAnimator;
    public Transform datasetScrollViewContent;

    [HideInInspector]
    public DatasetButton activeDatasetButton;

    public GameObject modalMenu;

    public ObjectSpawner spawner;

    public Button doneButton;

    public XRInteractionGroup interactionGroup;

    public DebugSlider debugPlaneSlider;
    public GameObject debugPlane;

    public ARPlaneManager planeManager;

    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>( "Tap Start Position" );
    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    [SerializeField]
    XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>( "Drag Current Position" );
    public XRInputValueReader<Vector2> dragCurrentPositionInput
    {
        get => m_DragCurrentPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
    }

    public GameObject playbackControls;

    public GameObject playIcon;
    public GameObject pauseIcon;
    public TMP_Text currentTimeLabel;
    public TMP_Text totalTimeLabel;

    public Button stepForwardButton;
    public GameObject stepForwardEnabled;
    public GameObject stepForwardDisabled;

    public Button stepBackwardButton;
    public GameObject stepBackwardEnabled;
    public GameObject stepBackwardDisabled;

    bool isPointerOverUI;
    bool showDatasetMenu;
    bool showOptionsModal;

    readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions = new List<ARFeatheredPlaneMeshVisualizerCompanion>();

    static MenuManager _Instance;
    public static MenuManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindAnyObjectByType<MenuManager>();
            }
            return _Instance;
        }
    }

    public Player currentPlayer
    {
        get 
        {
            return spawner.GetComponentInChildren<Player>();
        }
    }

    void OnEnable ()
    {
        switchButton.onClick.AddListener( ShowMenu );
        doneButton.onClick.AddListener( HideMenu );
        deleteButton.onClick.AddListener( DeleteAllObjects );
        planeManager.trackablesChanged.AddListener( OnPlaneChanged );
        spawner.objectSpawned += PlayerSpawned;
    }

    void OnDisable ()
    {
        showDatasetMenu = false;
        switchButton.onClick.RemoveListener( ShowMenu );
        doneButton.onClick.RemoveListener( HideMenu );
        deleteButton.onClick.RemoveListener( DeleteAllObjects );
        planeManager.trackablesChanged.RemoveListener( OnPlaneChanged );
        spawner.objectSpawned -= PlayerSpawned;
    }

    void Start ()
    {
        GenerateDatasetButtons();
        planeManager.planePrefab = debugPlane;
        SetSwitchButton( false );
        SetDeleteButton( false );
        playbackControls.SetActive( false );
    }

    void Update ()
    {
        if (showDatasetMenu || showOptionsModal)
        {
            if (!isPointerOverUI && (m_TapStartPositionInput.TryReadValue( out _ ) || m_DragCurrentPositionInput.TryReadValue( out _ )))
            {
                if (showDatasetMenu)
                {
                    HideMenu();
                }

                if (showOptionsModal)
                {
                    modalMenu.SetActive( false );
                }
            }

            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject( -1 );
        }
        else
        {
            isPointerOverUI = false;
        }

        if (!isPointerOverUI && showOptionsModal)
        {
            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject( -1 );
        }
    }

    void PlayerSpawned (GameObject player)
    {
        playbackControls.SetActive( true );
        playIcon.SetActive( false );
        pauseIcon.SetActive( true );
        SetStepButtons( false );
        SetSwitchButton( true );
        SetDeleteButton( true );
    }

    void GenerateDatasetButtons ()
    {
        GameObject buttonPrefab = Resources.Load( "DatasetButton" ) as GameObject;
        for (int d = 0; d < datasets.Length; d++)
        {
            DatasetButton button = (Instantiate( buttonPrefab, datasetScrollViewContent ) as GameObject).GetComponent<DatasetButton>();
            button.SetDataset( datasets[d], d < 1 );
            if (d < 1)
            {
                activeDatasetButton = button;
            }
        }
    }

    public void SwitchDataset (DatasetButton button)
    {
        if (button != activeDatasetButton)
        {
            if (activeDatasetButton != null)
            {
                activeDatasetButton.Deselect();
            }
            activeDatasetButton = button;

            // switch dataset
            Player player = currentPlayer;
            if (player != null)
            {
                player.SetDataset( activeDatasetButton.dataset );
            }
            else
            {
                Debug.LogWarning( "Simularium player not found when switching dataset." );
            }
        }
    }

    void SetSwitchButton (bool enable)
    {
        switchButton.interactable = enable;
        switchButtonEnabled.SetActive( enable );
        switchButtonDisabled.SetActive( !enable );
    }

    void SetDeleteButton (bool enable)
    {
        deleteButton.interactable = enable;
        deleteButtonEnabled.SetActive( enable );
        deleteButtonDisabled.SetActive( !enable );
    }

    void SetStepButtons (bool enable)
    {
        stepForwardButton.interactable = enable;
        stepForwardEnabled.SetActive( enable );
        stepForwardDisabled.SetActive( !enable );
        stepForwardButton.interactable = enable;
        stepBackwardEnabled.SetActive( enable );
        stepBackwardDisabled.SetActive( !enable );
    }

    void ShowMenu ()
    {
        showDatasetMenu = true;
        if (!datasetMenuAnimator.GetBool( "Show" ))
        {
            datasetMenuAnimator.SetBool( "Show", true );
        }
        SetDeleteButton( false );
    }

    public void HideMenu ()
    {
        datasetMenuAnimator.SetBool( "Show", false );
        showDatasetMenu = false;
        bool havePlayer = currentPlayer != null;
        SetSwitchButton( havePlayer );
        SetDeleteButton( havePlayer );
    }

    public void TogglePlay ()
    {
        if (playIcon.activeSelf)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }

    void Play ()
    {
        playIcon.SetActive( false );
        pauseIcon.SetActive( true );

        SetStepButtons( false );

        Player player = currentPlayer;
        if (player != null)
        {
            player.Play();
        }
        else
        {
            Debug.LogWarning( "Simularium player not found when playing." );
        }
    }

    void Pause ()
    {
        playIcon.SetActive( true );
        pauseIcon.SetActive( false );

        SetStepButtons( true );
        
        Player player = currentPlayer;
        if (player != null)
        {
            player.Pause();
        }
        else
        {
            Debug.LogWarning( "Simularium player not found when pausing." );
        }
    }

    public void Step (int direction)
    {
        if (pauseIcon.activeSelf)
        {
            Pause();
        }

        Player player = currentPlayer;
        if (player != null)
        {
            player.IncrementStep( direction );
        }
        else
        {
            Debug.LogWarning( "Simularium player not found when stepping." );
        }
    }

    public void DeleteAllObjects ()
    {
        foreach (Transform child in spawner.transform)
        {
            Destroy( child.gameObject );
        }
        playbackControls.SetActive( false );
        SetSwitchButton( false );
        SetDeleteButton( false );
    }

    public void ShowHideModal ()
    {
        if (modalMenu.activeSelf)
        {
            showOptionsModal = false;
            modalMenu.SetActive( false );
        }
        else
        {
            showOptionsModal = true;
            modalMenu.SetActive( true );
        }
    }

    public void ShowHideDebugPlane ()
    {
        if (debugPlaneSlider.value == 1)
        {
            debugPlaneSlider.value = 0;
            ChangePlaneVisibility( false );
        }
        else
        {
            debugPlaneSlider.value = 1;
            ChangePlaneVisibility( true );
        }
    }

    void ChangePlaneVisibility (bool setVisible)
    {
        var count = featheredPlaneMeshVisualizerCompanions.Count;
        for (int i = 0; i < count; ++i)
        {
            featheredPlaneMeshVisualizerCompanions[i].visualizeSurfaces = setVisible;
        }
    }

    void OnPlaneChanged (ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            foreach (var plane in eventArgs.added)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>( out var visualizer ))
                {
                    featheredPlaneMeshVisualizerCompanions.Add( visualizer );
                    visualizer.visualizeSurfaces = (debugPlaneSlider.value != 0);
                }
            }
        }

        if (eventArgs.removed.Count > 0)
        {
            foreach (var plane in eventArgs.removed)
            {
                if (plane.Value != null && plane.Value.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>( out var visualizer ))
                {
                    featheredPlaneMeshVisualizerCompanions.Remove( visualizer );
                }
            }
        }

        // Fallback if the counts do not match after an update
        if (planeManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in planeManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>( out var visualizer ))
                {
                    featheredPlaneMeshVisualizerCompanions.Add( visualizer );
                    visualizer.visualizeSurfaces = (debugPlaneSlider.value != 0);
                }
            }
        }
    }
}
