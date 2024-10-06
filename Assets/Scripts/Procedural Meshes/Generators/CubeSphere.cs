using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
	public struct CubeSphere : IMeshGenerator
	{
		struct Side
		{
			public int id;
			public float3 uvOrigin;
			public float3 uVector;
			public float3 vVector;
		}

		public int VertexCount => 6 * 4 * Resolution * Resolution;

		public int IndexCount => 6 * 6 * Resolution * Resolution;

		public int JobLength => 6 * Resolution;

		public int Resolution { get; set; }

		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2.0f, 2.0f, 2.0f));

		public void Execute<S>(int _i, S _streams) where S : struct, IMeshStreams
		{
			int u = _i / 6;

			Side side = GetSide(_i - 6 * u);

			int vi = 4 * Resolution * (Resolution * side.id + u);
			int ti = 2 * Resolution * (Resolution * side.id + u);

			float3 uA = side.uvOrigin + side.uVector * u / Resolution;
			float3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;

			float3 pA = CubeToSphere(uA);
			float3 pB = CubeToSphere(uB);

			var vertex = new Vertex();
			vertex.tangent = float4(normalize(pB - pA), -1.0f);

			for (var v = 1; v <= Resolution; v++, vi += 4, ti += 2)
			{
				float3 pC = CubeToSphere(uA + side.vVector * v / Resolution);
				float3 pD = CubeToSphere(uB + side.vVector * v / Resolution);

				vertex.position = pA;
				vertex.normal = normalize(cross(pC - pA, vertex.tangent.xyz));
				vertex.texCoord0 = 0.0f;
				_streams.SetVertex(vi + 0, vertex);

				vertex.position = pB;
				vertex.normal = normalize(cross(pD - pB, vertex.tangent.xyz));
				vertex.texCoord0 = float2(1.0f, 0.0f);
				_streams.SetVertex(vi + 1, vertex);

				vertex.position = pC;
				vertex.tangent.xyz = normalize(pD - pC);
				vertex.normal = normalize(cross(pC - pA, vertex.tangent.xyz));
				vertex.texCoord0 = float2(0.0f, 1.0f);
				_streams.SetVertex(vi + 2, vertex);

				vertex.position = pD;
				vertex.normal = pD;
				vertex.normal = normalize(cross(pD - pB, vertex.tangent.xyz));
				vertex.texCoord0 = 1.0f;
				_streams.SetVertex(vi + 3, vertex);

				_streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
				_streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));

				pA = pC;
				pB = pD;
			}
		}

		static float3 CubeToSphere(float3 _p) => _p * sqrt(1.0f - ((_p * _p).yxx + (_p * _p).zzy) / 2.0f + (_p * _p).yxx * (_p * _p).zzy / 3.0f);

		static Side GetSide(int _id) => _id switch
		{
				0 => new Side
				{
						id = _id,
						uvOrigin = -1.0f,
						uVector = 2.0f * right(),
						vVector = 2.0f * up()
				},
				1 => new Side
				{
						id = _id,
						uvOrigin = float3(1.0f, -1.0f, -1.0f),
						uVector = 2.0f * forward(),
						vVector = 2.0f * up()
				},
				2 => new Side
				{
						id = _id,
						uvOrigin = -1.0f,
						uVector = 2.0f * forward(),
						vVector = 2.0f * right()
				},
				3 => new Side
				{
						id = _id,
						uvOrigin = float3(-1.0f, -1.0f, 1.0f),
						uVector = 2.0f * up(),
						vVector = 2.0f * right()
				},
				4 => new Side
				{
						id = _id,
						uvOrigin = -1.0f,
						uVector = 2.0f * up(),
						vVector = 2.0f * forward()
				},
				_ => new Side
				{
						id = _id,
						uvOrigin = float3(-1.0f, 1.0f, -1.0f),
						uVector = 2.0f * right(),
						vVector = 2.0f * forward()
				}
		};
	}
}