using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SharedSquareGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution +  1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;
        
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));

        public void Execute<S>(int _z, S _streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * _z;
            int ti = 2 * Resolution * (_z - 1);

            var vertex = new Vertex();
            vertex.normal.y = 1.0f;
            vertex.tangent.xw = float2(1.0f, -1.0f);

            vertex.position.x = -0.5f;
            vertex.position.z = (float)_z / Resolution - 0.5f;
            vertex.texCoord0.y = (float)_z / Resolution;
            _streams.SetVertex(vi, vertex);

            vi += 1;

            for (var x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.position.x = (float)x / Resolution - 0.5f;
                vertex.texCoord0.x = (float)x / Resolution;
                _streams.SetVertex(vi, vertex);

                if (_z > 0)
                {
                    _streams.SetTriangle(ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1));
                    _streams.SetTriangle(ti + 1, vi + int3(-Resolution - 1, -1, 0));
                }
            }
        }
    }
}