using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshGenerator
    {
        int VertexCount { get; }
        
        int IndexCount { get; }
        
        int JobLength { get; }
        
        int Resolution { get; set; }
        
        Bounds Bounds { get; }
        
        void Execute<S>(int _i, S _streams) where S : struct, IMeshStreams;
    }
}