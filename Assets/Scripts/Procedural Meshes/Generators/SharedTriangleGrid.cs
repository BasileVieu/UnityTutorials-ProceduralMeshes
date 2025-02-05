using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SharedTriangleGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution +  1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;
        
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1.0f + 0.5f / Resolution, 0.0f, sqrt(3.0f) / 2.0f));

        public void Execute<S>(int _z, S _streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * _z;
            int ti = 2 * Resolution * (_z - 1);

            float xOffset = -0.25f;
            float uOffset = 0.0f;

            int iA = -Resolution - 2;
            int iB = -Resolution - 1;
            int iC = -1;
            int iD = 0;
            int3 tA = int3(iA, iC, iD);
            int3 tB = int3(iA, iD, iB);

            if ((_z & 1) == 1)
            {
                xOffset = 0.25f;
                uOffset = 0.5f / (Resolution + 0.5f);

                tA = int3(iA, iC, iB);
                tB = int3(iB, iC, iD);
            }

            xOffset = xOffset / Resolution - 0.5f;

            var vertex = new Vertex();
            vertex.normal.y = 1.0f;
            vertex.tangent.xw = float2(1.0f, -1.0f);

            vertex.position.x = xOffset;
            vertex.position.z = ((float)_z / Resolution - 0.5f) * sqrt(3.0f) / 2.0f;
            vertex.texCoord0.x = uOffset;
            vertex.texCoord0.y = vertex.position.z / (1.0f + 0.5f / Resolution) + 0.5f;
            _streams.SetVertex(vi, vertex);

            vi += 1;

            for (var x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.position.x = (float)x / Resolution + xOffset;
                vertex.texCoord0.x = x / (Resolution + 0.5f) + uOffset;
                _streams.SetVertex(vi, vertex);

                if (_z > 0)
                {
                    _streams.SetTriangle(ti + 0, vi + tA);
                    _streams.SetTriangle(ti + 1, vi + tB);
                }
            }
        }
    }
}