using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;

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
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = dataset;
            }
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (dataset)
            {
                GUILayout.Label ("Created test Dataset.");
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
                frame.transforms = new float[nAgents * 12];
                frame.colors = new float[nAgents * 4];
                float angle = (360f / (float)totalSteps) * t;
                for (int i = 0; i < nAgents; i++)
                {
                    // transform matrices
                    Vector3 position = new Vector3(
                        2f * Mathf.Floor( i / (float)resolution ),
                        5f * t / (float)totalSteps,
                        2f * (i % resolution)
                    );
                    Quaternion rotation = Quaternion.AngleAxis( 
                        angle + (360f / (float)nAgents) * i, 
                        Vector3.up
                    );
                    float scale = i / (float)nAgents;

                    float3x3 r = new float3x3(rotation) * scale;
                    float3x4 matrix = new float3x4(r.c0, r.c1, r.c2, position);
                    for (int n = 0; n < 12; n++)
                    {
                        frame.transforms[12 * i + n] = matrix[n % 4][Mathf.FloorToInt( n / 4f )];
                    }
                    
                    // colors
                    frame.colors[4 * i] = i / (float)nAgents;
                    frame.colors[4 * i + 1] = 0;
                    frame.colors[4 * i + 2] = 0;
                    frame.colors[4 * i + 3] = 1f;
                }
                asset.frames.Add( frame );
            }

            EditorUtility.SetDirty( asset );

            return asset;
        }
    }
}