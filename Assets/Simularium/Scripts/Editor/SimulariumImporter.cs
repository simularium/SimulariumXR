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

            if (GUILayout.Button("Generate Test data")) 
            {
                dataset = GenerateTestData();
            }
            if (dataset)
            {
                GUILayout.Label ("Created dataset asset.");
            }

            GUILayout.Space( 20 );
            
            GUILayout.EndHorizontal();

            if (GUI.changed) 
            {
                EditorUtility.SetDirty( dataset );
            }
        }

        Dataset GenerateTestData ()
        {
            int totalSteps = 10;
            int resolution = 10;

            Dataset asset = ScriptableObject.CreateInstance<Dataset>();

            asset.totalSteps = totalSteps;
            asset.nAgents = new int[totalSteps];
            asset.positions = new List<float[,]>();
            for (int t = 0; t < totalSteps; t++)
            {
                asset.nAgents[t] = resolution * resolution;
                asset.positions.Add( new float[resolution * resolution, 3] );
                for (int i = 0; i < resolution * resolution; i++)
                {
                    asset.positions[t][i, 0] = Mathf.Floor( i / (float)resolution );
                    asset.positions[t][i, 1] = 5f * t / (float)totalSteps;
                    asset.positions[t][i, 2] = i % resolution;
                }
            }
            asset.agentScale = 2f / resolution;

            AssetDatabase.CreateAsset(asset, "Assets/Simularium/Data/Test.asset");
            AssetDatabase.SaveAssets();

            return asset;
        }
    }
}