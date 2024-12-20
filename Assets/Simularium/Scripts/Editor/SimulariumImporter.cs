using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;
using System.IO;

namespace Simularium
{
    public class SimulariumImporter : EditorWindow {

        public Dataset dataset;

        string error = "";

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

            GUILayout.Label( "Simularium Importer", EditorStyles.boldLabel );
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button( "Generate Test data" )) 
            {
                dataset = GenerateTestData();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = dataset;
            }
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (dataset)
            {
                GUILayout.Label( "Created test Dataset." );
            }
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button( "Import Simularium File" )) 
            {
                string path = EditorUtility.OpenFilePanel( "Choose Simularium file", "", "simularium" );
                if (path.Length != 0)
                {
                    dataset = GenerateSimulariumData( path );
                }
            }
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            
            if (error.Length != 0)
            {
                GUILayout.Label( error );
            }

            GUILayout.EndHorizontal();
        }

        string ConfigureOutputPath ()
        {
            string outputPath = "Assets/Simularium/Data/";
            if (!AssetDatabase.IsValidFolder( outputPath + "Resources" )) {
                AssetDatabase.CreateFolder( "Assets/Simularium/Data", "Resources" );
            }
            return outputPath;
        }

        Dataset GenerateSimulariumData (string inputPath)
        {
            Dataset asset = ScriptableObject.CreateInstance<Dataset>();
            string outputPath = ConfigureOutputPath();
            asset.datasetName = Path.GetFileNameWithoutExtension( inputPath );
            AssetDatabase.CreateAsset( asset, outputPath + asset.datasetName + ".asset" );
            AssetDatabase.SaveAssets();

            error = Parser.ParseSimulariumFile( inputPath, outputPath, asset );

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty( asset );

            return asset;
        }

        void ConvertColors () 
        {
            colors = new Color[hexColors.Length];
            for (int i = 0; i < hexColors.Length; i++)
            {
                colors[i] = Parser.ParseColor( hexColors[i] );
            }
        }

        Dataset GenerateTestData ()
        {
            int totalSteps = 10;
            int resolution = 10;

            Dataset asset = ScriptableObject.CreateInstance<Dataset>();
            string path = ConfigureOutputPath();
            AssetDatabase.CreateAsset( asset, path + "TestDataset.asset" );

            asset.datasetName = "TestDataset";
            asset.totalSteps = totalSteps;
            asset.timeStep = 0.1f;
            asset.timeLabel = "ns";
            asset.nMeshAgents = new int[totalSteps];
            asset.hasLines = true;
            asset.lineColor = new Color( 0.6f, 1.0f, 0.8f, 1.0f );
            asset.lineThickness = 1.0f;
            asset.frames = new List<FrameData>();
            ConvertColors();
            for (int t = 0; t < totalSteps; t++)
            {
                int nMeshAgents = resolution * resolution;
                asset.nMeshAgents[t] = nMeshAgents;
                FrameData frame = new FrameData();
                frame.meshTransforms = new float[nMeshAgents * 12];
                frame.meshColors = new float[nMeshAgents * 3];
                float angle = (360f / (float)totalSteps) * t;
                for (int i = 0; i < nMeshAgents; i++)
                {
                    // mesh transform matrices
                    Vector3 position = new Vector3(
                        2f * Mathf.Floor( i / (float)resolution ),
                        10f * t / (float)totalSteps,
                        2f * (i % resolution)
                    );
                    Quaternion rotation = Quaternion.AngleAxis( 
                        angle + (360f / (float)nMeshAgents) * i, 
                        Vector3.up
                    );
                    float scale = i / (float)nMeshAgents;

                    float3x3 r = new float3x3(rotation) * scale;
                    float3x4 matrix = new float3x4(r.c0, r.c1, r.c2, position);
                    for (int n = 0; n < 12; n++)
                    {
                        frame.meshTransforms[12 * i + n] = matrix[n % 4][Mathf.FloorToInt( n / 4f )];
                    }
                    
                    // mesh colors
                    Color color = colors[i % colors.Length];
                    frame.meshColors[3 * i] = color.r;
                    frame.meshColors[3 * i + 1] = color.g;
                    frame.meshColors[3 * i + 2] = color.b;
                }
                
                // lines
                Mesh lineMesh = new Mesh();
                float z = 10f * t / (float)totalSteps;
                Vector3[] rawPoints = new Vector3[] {
                    new Vector3( 0.0f, 0.0f, z ),
                    new Vector3( 20.0f, 20.0f, z ),
                    new Vector3( 0.0f, 20.0f, z ),
                    new Vector3( 20.0f, 0.0f, z ),
                    new Vector3( 0.0f, 0.0f, z )
                };
                Vector3[] points = new Vector3[2 * rawPoints.Length - 2];
                int j = 0;
                for (int i = 0; i < rawPoints.Length; i++) // make points into line segments
                {
                    points[j] = rawPoints[i];
                    j++;
                    if (i > 0 && i < rawPoints.Length - 1) 
                    {
                        points[j] = rawPoints[i];
                        j++;
                    }
                }
                int[] ixs = new int[] {
                    0, 1, 2, 3, 4, 5, 6, 7
                };
                lineMesh.vertices = points;
                lineMesh.SetIndices( ixs, MeshTopology.Lines, 0 );
                MeshUtility.Optimize( lineMesh );
                AssetDatabase.CreateAsset( lineMesh, path + "Resources/TestDataset_Mesh_" + t + ".asset" );

                asset.frames.Add( frame );
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty( asset );

            return asset;
        }
    }
}