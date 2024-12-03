using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simularium
{
    public class BinaryHeader 
    {
        public int nBlocks;
        public int[] blockOffsets;
        public int[] blockTypes;
        public int[] blockLengths;
        
        public BinaryHeader (int _nBlocks) 
        {
            nBlocks = _nBlocks;
            blockOffsets = new int[nBlocks];
            blockTypes = new int[nBlocks];
            blockLengths = new int[nBlocks];
        }
    }

    public class DisplayData
    {
        public float radius;
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
        public Dictionary<string,DisplayDataEntry> typeMapping;
    }

    public class Parser {

        static string FILE_IDENTIFIER = "SIMULARIUMBINARY";
        static int TRAJ_INFO_BLOCK_TYPE = 1;
        static int SPATIAL_BLOCK_TYPE = 3;
        static int BLOCK_HEADER_LENGTH = 8;

        static BinaryHeader ParseHeader (BinaryReader reader)
        {
            int headerLength = reader.ReadInt32();
            int version = reader.ReadInt32();
            int nBlocks = reader.ReadInt32();
            BinaryHeader result = new BinaryHeader( nBlocks );
            for (int b = 0; b < nBlocks; b++)
            {
                result.blockOffsets[b] = reader.ReadInt32();
                result.blockTypes[b] = reader.ReadInt32();
                result.blockLengths[b] = reader.ReadInt32();
            }
            return result;
        }

        public static string ParseSimulariumFile (string path)
        {
            Stream stream = new FileStream( path, FileMode.Open );
            BinaryReader reader = new BinaryReader( stream );

            string headerStr = new string( reader.ReadChars( 16 ) );
            if (headerStr != FILE_IDENTIFIER)
            {
                return "ERROR: file must be in Simularium Binary format.";
            }

            BinaryHeader header = ParseHeader( reader );
            for (int b = 0; b < header.nBlocks; b++)
            {
                int blockType = reader.ReadInt32();
                int blockLength = reader.ReadInt32();
                if (blockLength <= BLOCK_HEADER_LENGTH)
                {
                    // empty block
                    continue;
                }
                if (blockType == TRAJ_INFO_BLOCK_TYPE)
                {
                    byte[] buffer = reader.ReadBytes( blockLength - BLOCK_HEADER_LENGTH );
                    string jsonStr = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    jsonStr = jsonStr.Substring( 0, jsonStr.Length );
                    TrajectoryInfo trajInfo = JsonConvert.DeserializeObject<TrajectoryInfo>( jsonStr );

                    Debug.Log( trajInfo.typeMapping );
                }
                else if (header.blockTypes[b] == SPATIAL_BLOCK_TYPE)
                {
                    // TODO parse spatial data
                }
                else 
                {
                    // advance reader to next block
                    reader.ReadBytes( blockLength - BLOCK_HEADER_LENGTH );
                }
            }
            return "";
        }

    }
}