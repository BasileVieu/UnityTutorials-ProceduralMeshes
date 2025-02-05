using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct PointyHexagonGrid : IMeshGenerator
    {
        public int VertexCount => 7 * Resolution * Resolution;

        public int IndexCount => 18 * Resolution * Resolution;

        public int JobLength => Resolution;
        
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3((Resolution > 1 ? 0.75f / Resolution : 0.5f) * sqrt(3.0f), 0.0f, 0.75f + 0.25f / Resolution));

        public void Execute<S>(int _z, S _streams) where S : struct, IMeshStreams
        {
            int vi = 7 * Resolution * _z;
            int ti = 6 * Resolution * _z;

            float h = sqrt(3.0f) / 4.0f;

            float2 centerOffset = 0.0f;

            if (Resolution > 1)
            {
                centerOffset.x = (((_z & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;
                centerOffset.y = -0.375f * (Resolution - 1);
            }

            for (var x = 0; x < Resolution; x++, vi += 7, ti += 6)
            {
                float2 center = (float2(2.0f * h * x, 0.75f * _z) + centerOffset) / Resolution;
                
                float2 xCoordinates = center.x + float2(-h, h) / Resolution;
                float4 zCoordinates = center.y + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;
                
                var vertex = new Vertex();
                vertex.normal.y = 1.0f;
                vertex.tangent.xw = float2(1.0f, -1.0f);

                vertex.position.xz = center;
                vertex.texCoord0 = 0.5f;
                _streams.SetVertex(vi + 0, vertex);

                vertex.position.z = zCoordinates.x;
                vertex.texCoord0.y = 0.0f;
                _streams.SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0.5f - h, 0.25f); 
                _streams.SetVertex(vi + 2, vertex);

                vertex.position.z = zCoordinates.z;
                vertex.texCoord0.y = 0.75f;
                _streams.SetVertex(vi + 3, vertex);

                vertex.position.x = center.x;
                vertex.position.z = zCoordinates.w;
                vertex.texCoord0 = float2(0.5f, 1.0f);
                _streams.SetVertex(vi + 4, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.position.z = zCoordinates.z;
                vertex.texCoord0 = float2(0.5f + h, 0.75f);
                _streams.SetVertex(vi + 5, vertex);

                vertex.position.z = zCoordinates.y;
                vertex.texCoord0.y = 0.25f;
                _streams.SetVertex(vi + 6, vertex);
                
                _streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                _streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                _streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                _streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                _streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                _streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));
            }
        }
    }
}