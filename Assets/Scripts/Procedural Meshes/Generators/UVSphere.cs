using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
	public struct UVSphere : IMeshGenerator
	{
		public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1) - 2;

		public int IndexCount => 6 * ResolutionU * (ResolutionV - 1);

		public int JobLength => ResolutionU + 1;

		public int Resolution { get; set; }

		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2.0f, 2.0f, 2.0f));

		private int ResolutionV => 2 * Resolution;

		private int ResolutionU => 4 * Resolution;

		public void Execute<S>(int _u, S _streams) where S : struct, IMeshStreams
		{
			if (_u == 0)
			{
				ExecuteSeam(_streams);
			}
			else
			{
				ExecuteRegular(_u, _streams);
			}
		}

		public void ExecuteSeam<S>(S _streams) where S : struct, IMeshStreams
		{
			var vertex = new Vertex();
			vertex.tangent.x = 1.0f;
			vertex.tangent.w = -1.0f;

			for (var v = 1; v < ResolutionV; v++)
			{
				sincos(PI + PI * v / ResolutionV, out vertex.position.z, out vertex.position.y);

				vertex.normal = vertex.position;
				vertex.texCoord0.y = (float)v / ResolutionV;
				_streams.SetVertex(v - 1, vertex);
			}
		}

		public void ExecuteRegular<S>(int _u, S _streams) where S : struct, IMeshStreams
		{
			int vi = (ResolutionV + 1) * _u - 2;
			int ti = 2 * (ResolutionV - 1) * (_u - 1);

			var vertex = new Vertex();
			vertex.position.y = vertex.normal.y = -1.0f;
			
			sincos(2.0f * PI * (_u - 0.5f) / ResolutionU, out vertex.tangent.z, out vertex.tangent.x);
			
			vertex.tangent.w = -1.0f;
			vertex.texCoord0.x = (_u - 0.5f) / ResolutionU;

			_streams.SetVertex(vi, vertex);

			vertex.position.y = vertex.normal.y = 1.0f;
			vertex.texCoord0.y = 1.0f;

			_streams.SetVertex(vi + ResolutionV, vertex);

			vi += 1;

			float2 circle;
			
			sincos(2.0f * PI * _u / ResolutionU, out circle.x, out circle.y);

			vertex.tangent.xz = circle.yx;

			circle.y = -circle.y;

			vertex.texCoord0.x = (float)_u / ResolutionU;

			int shiftLeft = (_u == 1 ? 0 : -1) - ResolutionV;

			_streams.SetTriangle(ti, vi + int3(-1, shiftLeft, 0));

			ti += 1;

			for (var v = 1; v < ResolutionV; v++, vi++)
			{
				sincos(PI + PI * v / ResolutionV, out float circleRadius, out vertex.position.y);

				vertex.position.xz = circle * -circleRadius;
				vertex.normal = vertex.position;
				vertex.texCoord0.y = (float)v / ResolutionV;
				_streams.SetVertex(vi, vertex);

				if (v > 1)
				{
					_streams.SetTriangle(ti + 0, vi + int3(shiftLeft - 1, shiftLeft, -1));
					_streams.SetTriangle(ti + 1, vi + int3(-1, shiftLeft, 0));

					ti += 2;
				}
			}

			_streams.SetTriangle(ti, vi + int3(shiftLeft - 1, 0, -1));
		}
	}
}