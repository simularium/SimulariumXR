using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Simularium
{
    public class SimulariumImporter : EditorWindow {

        public Dataset dataset;

        [MenuItem( "Window/Simularium Importer" )]
        static void Init () 
        {
            EditorWindow.GetWindow( typeof( SimulariumImporter ) );
        }

        void OnGUI () 
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Simularium Importer", EditorStyles.boldLabel);
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Test data")) 
            {
                dataset = GenerateTestData();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (dataset)
            {
                GUILayout.Label ("Created test Dataset.");
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = dataset;
            }
            
            GUILayout.EndHorizontal();
        }

        Dataset GenerateTestData ()
        {
            int totalSteps = 10;
            int resolution = 10;

            Dataset asset = ScriptableObject.CreateInstance<Dataset>();
            AssetDatabase.CreateAsset(asset, "Assets/Simularium/Data/TestDataset.asset");
            AssetDatabase.SaveAssets();

            asset.totalSteps = totalSteps;
            asset.nAgents = new int[totalSteps];
            asset.frames = new List<FrameData>();
            for (int t = 0; t < totalSteps; t++)
            {
                int nAgents = resolution * resolution;
                asset.nAgents[t] = nAgents;
                FrameData frame = new FrameData();
                frame.positions = new float[nAgents * 3];
                frame.radii = new float[nAgents];
                for (int i = 0; i < nAgents; i++)
                {
                    frame.positions[3 * i] = Mathf.Floor( i / (float)resolution );
                    frame.positions[3 * i + 1] = 5f * t / (float)totalSteps;
                    frame.positions[3 * i + 2] = i % resolution;
                    frame.radii[i] = i / (float)nAgents;
                }
                asset.frames.Add( frame );
            }

            EditorUtility.SetDirty( asset );

            return asset;
        }
    }
}