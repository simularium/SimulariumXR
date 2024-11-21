using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Simularium
{
    public class SimulariumImporter : EditorWindow {

        public Dataset dataset;

        Color[] colors = null;
        string[] hexColors = new string[] {
            "#fee34d",
            "#f7b232",
            "#bf5736",
            "#94a7fc",
            "#ce8ec9",
            "#58606c",
            "#0ba345",
            "#9267cb",
            "#81dbe6",
            "#bd7800",
            "#bbbb99",
            "#5b79f0",
            "#89a500",
            "#da8692",
            "#418463",
            "#9f516c",
            "#00aabf"
        };

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

        void ConvertColors () 
        {
            colors = new Color[hexColors.Length];
            for (int i = 0; i < hexColors.Length; i++)
            {
                Color color;
                if (ColorUtility.TryParseHtmlString( hexColors[i], out color ))
                {
                    colors[i] = color;
                }
                else 
                {
                    colors[i] = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
                }
            }
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
            ConvertColors();
            for (int t = 0; t < totalSteps; t++)
            {
                int nAgents = resolution * resolution;
                asset.nAgents[t] = nAgents;
                FrameData frame = new FrameData();
                frame.transforms = new float[nAgents * 12];
                frame.colors = new float[nAgents * 3];
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
                    Color color = colors[i % colors.Length];
                    frame.colors[3 * i] = color.r;
                    frame.colors[3 * i + 1] = color.g;
                    frame.colors[3 * i + 2] = color.b;
                }
                asset.frames.Add( frame );
            }

            EditorUtility.SetDirty( asset );

            return asset;
        }
    }
}