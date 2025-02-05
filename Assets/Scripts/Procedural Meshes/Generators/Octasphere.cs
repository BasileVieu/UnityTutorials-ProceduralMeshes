using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct Octasphere : IMeshGenerator
    {
        struct Rhombus
        {
            public int id;
            public float3 leftCorner;
            public float3 rightCorner;
        }

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2.0f, 2.0f, 2.0f));

        public int VertexCount => 4 * Resolution * Resolution + 2 * Resolution + 7;

        public int IndexCount => 6 * 4 * Resolution * Resolution;

        public int JobLength => 4 * Resolution + 1;

        public int Resolution { get; set; }

        public void Execute<S>(int _i, S _streams) where S : struct, IMeshStreams
        {
            if (_i == 0)
            {
                ExecutePolesAndSeam(_streams);
            }
            else
            {
                ExecuteRegular(_i - 1, _streams);
            }
        }

        public void ExecuteRegular<S>(int _i, S _streams) where S : struct, IMeshStreams
        {
            int u = _i / 4;

            Rhombus rhombus = GetRhombus(_i - 4 * u);

            int vi = Resolution * (Resolution * rhombus.id + u + 2) + 7;
            int ti = 2 * Resolution * (Resolution * rhombus.id + u);
            bool firstColumn = u == 0;

            int4 quad = int4(vi,
                firstColumn ? rhombus.id : vi - Resolution,
                firstColumn ? rhombus.id == 0 ? 8 : vi - Resolution * (Resolution + u) : vi - Resolution + 1,
                vi + 1);

            u += 1;

            float3 columnBottomDir = rhombus.rightCorner - down();
            float3 columnBottomStart = down() + columnBottomDir * u / Resolution;
            float3 columnBottomEnd = rhombus.leftCorner + columnBottomDir * u / Resolution;

            float3 columnTopDir = up() - rhombus.leftCorner;
            float3 columnTopStart = rhombus.rightCorner + columnTopDir * ((float)u / Resolution - 1.0f);
            float3 columnTopEnd = rhombus.leftCorner + columnTopDir * u / Resolution;

            var vertex = new Vertex();
            vertex.normal = vertex.position = normalize(columnBottomStart);
            vertex.tangent.xz = GetTangentXZ(vertex.position);
            vertex.tangent.w = -1.0f;
            vertex.texCoord0 = GetTexCoord(vertex.position);

            _streams.SetVertex(vi, vertex);

            vi += 1;

            for (int v = 1; v < Resolution; v++, vi++, ti += 2)
            {
                if (v <= Resolution - u)
                {
                    vertex.position = lerp(columnBottomStart, columnBottomEnd, (float)v / Resolution);
                }
                else
                {
                    vertex.position = lerp(columnTopStart, columnTopEnd, (float)v / Resolution);
                }

                vertex.normal = vertex.position = normalize(vertex.position);
                vertex.tangent.xz = GetTangentXZ(vertex.position);
                vertex.texCoord0 = GetTexCoord(vertex.position);

                _streams.SetVertex(vi, vertex);

                _streams.SetTriangle(ti + 0, quad.xyz);
                _streams.SetTriangle(ti + 1, quad.xzw);

                quad.y = quad.z;
                quad += int4(1, 0, firstColumn && rhombus.id != 0 ? Resolution : 1, 1);
            }

            quad.z = Resolution * Resolution * rhombus.id + Resolution + u + 6;
            quad.w = u < Resolution ? quad.z + 1 : rhombus.id + 4;

            _streams.SetTriangle(ti + 0, quad.xyz);
            _streams.SetTriangle(ti + 1, quad.xzw);
        }

        public void ExecutePolesAndSeam<S>(S _streams) where S : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.tangent = float4(sqrt(0.5f), 0f, sqrt(0.5f), -1.0f);
            vertex.texCoord0.x = 0.125f;

            for (int i = 0; i < 4; i++)
            {
                vertex.position = vertex.normal = down();
                vertex.texCoord0.y = 0.0f;

                _streams.SetVertex(i, vertex);

                vertex.position = vertex.normal = up();
                vertex.texCoord0.y = 1.0f;

                _streams.SetVertex(i + 4, vertex);

                vertex.tangent.xz = float2(-vertex.tangent.z, vertex.tangent.x);
                vertex.texCoord0.x += 0.25f;
            }

            vertex.tangent.xz = float2(1.0f, 0.0f);
            vertex.texCoord0.x = 0.0f;

            for (int v = 1; v < 2 * Resolution; v++)
            {
                if (v < Resolution)
                {
                    vertex.position = lerp(down(), back(), (float)v / Resolution);
                }
                else
                {
                    vertex.position = lerp(back(), up(), (float)(v - Resolution) / Resolution);
                }

                vertex.normal = vertex.position = normalize(vertex.position);
                vertex.texCoord0.y = GetTexCoord(vertex.position).y;

                _streams.SetVertex(v + 7, vertex);
            }
        }

        static Rhombus GetRhombus(int _id) => _id switch
        {
            0 => new Rhombus
            {
                id = _id,
                leftCorner = back(),
                rightCorner = right()
            },
            1 => new Rhombus
            {
                id = _id,
                leftCorner = right(),
                rightCorner = forward()
            },
            2 => new Rhombus
            {
                id = _id,
                leftCorner = forward(),
                rightCorner = left()
            },
            _ => new Rhombus
            {
                id = _id,
                leftCorner = left(),
                rightCorner = back()
            }
        };

        static float2 GetTangentXZ(float3 _p) => normalize(float2(-_p.z, _p.x));

        static float2 GetTexCoord(float3 _p)
        {
            var texCoord = float2(atan2(_p.x, _p.z) / (-2.0f * PI) + 0.5f, asin(_p.y) / PI + 0.5f);

            if (texCoord.x < 1e-6f)
            {
                texCoord.x = 1.0f;
            }

            return texCoord;
        }
    }
}