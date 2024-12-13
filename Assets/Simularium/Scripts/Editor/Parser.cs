using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simularium
{
    public class DisplayData
    {
        public string displayType;
        public string url;
        public string color;
    }

    public class DisplayDataEntry
    {
        public string name;
        public DisplayData geometry;
    }

    public class TimeUnits
    {
        public float magnitude;
        public string name;
    }

    public class TrajectoryInfo
    {
        public TimeUnits timeUnits;
        public float timeStepSize;
        public int totalSteps;
        public TimeUnits spatialUnits;
        public Vector3 size;
        public Dictionary<int,DisplayDataEntry> typeMapping;
    }

    public class Parser {

        static string FILE_IDENTIFIER = "SIMULARIUMBINARY";
        static int TRAJ_INFO_BLOCK_TYPE = 1;
        static int SPATIAL_BLOCK_TYPE = 3;
        static int BLOCK_HEADER_LENGTH = 8;
        static float DEFAULT_VIZ_TYPE = 1000.0f;
        static float FIBER_VIZ_TYPE = 1001.0f;
        static int MAX_FIBER_POINTS = 81920;
        static float UNITY_SCALE_FACTOR = 0.02f;

        static int ParseHeader (BinaryReader reader)
        {
            int headerLength = reader.ReadInt32();
            int version = reader.ReadInt32();
            int nBlocks = reader.ReadInt32();
            for (int b = 0; b < nBlocks; b++)
            {
                // advance reader through block info
                reader.ReadBytes( 3 * 4 );
            }
            return nBlocks;
        }

        static TrajectoryInfo ParseTrajectoryInfo (BinaryReader reader, int blockLength)
        {
            byte[] buffer = reader.ReadBytes( blockLength );
            string jsonStr = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            jsonStr = jsonStr.Substring( 0, jsonStr.Length );
            return JsonConvert.DeserializeObject<TrajectoryInfo>( jsonStr );
        }

        static float3x4 GetTransformMatrix (
            float posX, float posY, float posZ, 
            float rotX, float rotY, float rotZ, 
            float scale
        )
        {
            Vector3 position = UNITY_SCALE_FACTOR * new Vector3( posX, posY, -posZ );
            float3x3 r = new float3x3( Quaternion.Euler( rotX, rotY, -rotZ ) ) * UNITY_SCALE_FACTOR * 2 * scale;
            return new float3x4( r.c0, r.c1, r.c2, position );
        }

        public static Color ParseColor (string hexColor)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString( hexColor, out color ))
            {
                return color;
            }
            else 
            {
                return new Color( 254f / 255f, 227f / 255f, 77f / 255f, 1f );
            }
        }

        static void ParseSpatialData (
            BinaryReader reader, TrajectoryInfo trajInfo, Dataset dataset, string outputPath
        )
        {
            if (trajInfo == null)
            {
                Debug.Log( "TrajInfo is null" );
            }
            int version = reader.ReadInt32();
            dataset.totalSteps = reader.ReadInt32();
            dataset.timeStep = trajInfo.timeStepSize;
            dataset.timeLabel = trajInfo.timeUnits.name;
            dataset.nMeshAgents = new int[dataset.totalSteps];
            dataset.frames = new List<FrameData>();
            reader.ReadBytes( dataset.totalSteps * 2 * 4 ); // skip frame offsets and lengths
            bool lineColorSet = false;
            for (int t = 0; t < dataset.totalSteps; t++)
            {
                int frameIX = reader.ReadInt32();
                float time = reader.ReadSingle();
                int nAgents = reader.ReadInt32();
                FrameData frame = new FrameData();
                frame.meshTransforms = new float[nAgents * 12];
                frame.meshColors = new float[nAgents * 3];
                Vector3[] lineVertices = new Vector3[MAX_FIBER_POINTS];
                int[] lineIndices = new int[MAX_FIBER_POINTS];
                int pointIX = 0;
                bool frameHasLines = false;
                for (int n = 0; n < nAgents; n++)
                {
                    float vizType = reader.ReadSingle();
                    float uid = reader.ReadSingle();
                    float typeIDf = reader.ReadSingle();
                    int typeID = Mathf.RoundToInt( typeIDf );
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();
                    float posZ = reader.ReadSingle();
                    float rotX = reader.ReadSingle();
                    float rotY = reader.ReadSingle();
                    float rotZ = reader.ReadSingle();
                    float scale = reader.ReadSingle();
                    float nSubpointsf = reader.ReadSingle();
                    int nSubpoints = Mathf.RoundToInt( nSubpointsf );
                    if (nSubpoints <= 0)
                    {
                        if (vizType != DEFAULT_VIZ_TYPE)
                        {
                            Debug.Log( 
                                "Agent " + n + " at frame " + t + " is not a default type " +
                                "but has no subpoints, defaulting to mesh." 
                            );
                        }
                        // mesh agent
                        float3x4 matrix = GetTransformMatrix( posX, posY, posZ, rotX, rotY, rotZ, scale );
                        int ix = dataset.nMeshAgents[t];
                        dataset.nMeshAgents[t]++;
                        for (int v = 0; v < 12; v++)
                        {
                            frame.meshTransforms[12 * ix + v] = matrix[v % 4][Mathf.FloorToInt( v / 4f )];
                        }
                        Color color = new Color( 254f / 255f, 227f / 255f, 77f / 255f, 1f );
                        if (trajInfo != null && trajInfo.typeMapping.ContainsKey( typeID ))
                        {
                            color = ParseColor( trajInfo.typeMapping[typeID].geometry.color );
                        }
                        frame.meshColors[3 * ix] = color.r;
                        frame.meshColors[3 * ix + 1] = color.g;
                        frame.meshColors[3 * ix + 2] = color.b;
                    }
                    else
                    {
                        if (nSubpoints % 3 != 0)
                        {
                            if (Mathf.Approximately( vizType, FIBER_VIZ_TYPE ))
                            {
                                Debug.Log( 
                                    "Agent " + n + " at frame " + t + " is a fiber type " +
                                    "but has the wrong number of subpoints, skipping." 
                                );
                            }
                            else
                            {
                                Debug.Log( 
                                    "Skipping agent " + n + " at frame " + t +
                                    " since type and number of subpoints do not match." 
                                );
                            }
                            reader.ReadBytes( nSubpoints ); // advance through subpoints
                            continue;
                        }
                        frameHasLines = true;
                        dataset.hasLines = true;
                        int nPoints = Mathf.RoundToInt( nSubpoints / 3f );
                        for (int p = 0; p < nPoints; p++)
                        {
                            float x = reader.ReadSingle();
                            float y = reader.ReadSingle();
                            float z = -1 * reader.ReadSingle();
                            lineVertices[pointIX] = UNITY_SCALE_FACTOR * new Vector3( x, y, z );
                            lineIndices[pointIX] = pointIX;
                            pointIX++;
                            if (p > 0 && p < nPoints - 1)
                            {
                                lineVertices[pointIX] = UNITY_SCALE_FACTOR * new Vector3( x, y, z );
                                lineIndices[pointIX] = pointIX;
                                pointIX++;
                            }
                        }
                        if (!lineColorSet && trajInfo != null && trajInfo.typeMapping.ContainsKey( typeID ))
                        {
                            // set line render settings based on first fiber encountered
                            dataset.lineColor = ParseColor( trajInfo.typeMapping[typeID].geometry.color );
                            dataset.lineThickness = scale;
                            lineColorSet = true;
                        }
                    }
                }
                dataset.frames.Add( frame );

                if (frameHasLines)
                {
                    Mesh lineMesh = new Mesh();
                    lineMesh.vertices = lineVertices;
                    lineMesh.SetIndices( lineIndices, MeshTopology.Lines, 0 );
                    MeshUtility.Optimize( lineMesh );
                    AssetDatabase.CreateAsset( lineMesh, outputPath + "Resources/" + dataset.datasetName + "_Mesh_" + t + ".asset" );
                }
            }
        }

        public static string ParseSimulariumFile (string inputPath, string outputPath, Dataset dataset)
        {
            Stream stream = new FileStream( inputPath, FileMode.Open );
            BinaryReader reader = new BinaryReader( stream );

            string headerStr = new string( reader.ReadChars( FILE_IDENTIFIER.Length ) );
            if (headerStr != FILE_IDENTIFIER)
            {
                return "ERROR: file must be in Simularium Binary format.";
            }

            int nBlocks = ParseHeader( reader );
            TrajectoryInfo trajInfo = null;
            for (int b = 0; b < nBlocks; b++)
            {
                int blockType = reader.ReadInt32();
                int blockLength = reader.ReadInt32();
                blockLength -= BLOCK_HEADER_LENGTH;
                if (blockLength <= 0)
                {
                    // empty block
                    continue;
                }
                if (blockType == TRAJ_INFO_BLOCK_TYPE)
                {
                    trajInfo = ParseTrajectoryInfo( reader, blockLength );
                }
                else if (blockType == SPATIAL_BLOCK_TYPE)
                {
                    ParseSpatialData( reader, trajInfo, dataset, outputPath );
                }
                else 
                {
                    // advance reader to next block (skip plots etc)
                    reader.ReadBytes( blockLength );
                }
            }
            return "";
        }

    }
}