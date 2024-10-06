using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
	private static MeshJobScheduleDelegate[] jobs =
	{
			MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
			MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
			MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
			MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
			MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
			MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
			MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
			MeshJob<Icosphere, PositionStream>.ScheduleParallel,
			MeshJob<GeoIcosphere, PositionStream>.ScheduleParallel,
			MeshJob<Octasphere, SingleStream>.ScheduleParallel,
			MeshJob<GeoOctasphere, SingleStream>.ScheduleParallel,
			MeshJob<UVSphere, SingleStream>.ScheduleParallel
	};

	public enum MeshType
	{
		SquareGrid,
		SharedSquareGrid,
		SharedTriangleGrid,
		PointyHexagonGrid,
		FlatHexagonGrid,
		CubeSphere,
		SharedCubeSphere,
		Icosphere,
		GeoIcosphere,
		Octasphere,
		GeoOctasphere,
		UVSphere
	}

	[System.Flags]
	public enum MeshOptimizationMode
    {
		Nothing = 0,
		ReorderIndices = 1,
		ReorderVertices = 0b10
    }

	public enum MaterialMode
	{
		Flat,
		Ripple,
		LatLonMap,
		CubeMap
	}

	[System.Flags]
	public enum GizmoMode
	{
		Nothing = 0,
		Vertices = 1,
		Normals = 0b10,
		Tangents = 0b100,
		Triangles = 0b1000
	}

	[SerializeField] private MeshType meshType;

	[SerializeField] private MeshOptimizationMode meshOptimization;

	[SerializeField] [Range(1, 50)] private int resolution = 1;

	[SerializeField] private GizmoMode gizmos;

	[SerializeField] private MaterialMode material;

	[SerializeField] private Material[] materials;

	private Mesh mesh;

	[System.NonSerialized] private Vector3[] vertices;
	[System.NonSerialized] private Vector3[] normals;

	[System.NonSerialized] private Vector4[] tangents;

	[System.NonSerialized] private int[] triangles;

	void Awake()
	{
		mesh = new Mesh
		{
				name = "Procedural Mesh"
		};

		GetComponent<MeshFilter>().mesh = mesh;
	}

    void OnValidate()
    {
		enabled = true;
    }

	void Update()
	{
		GenerateMesh();

		enabled = false;

		vertices = null;
		normals = null;
		tangents = null;
		triangles = null;

		GetComponent<MeshRenderer>().material = materials[(int)material];
	}

	void OnDrawGizmos()
	{
		if (gizmos == GizmoMode.Nothing
				|| mesh == null)
		{
			return;
		}

		bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
		bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
		bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;
		bool drawTriangles = (gizmos & GizmoMode.Triangles) != 0;

		if (vertices == null)
		{
			vertices = mesh.vertices;
		}

		if (drawNormals
				&& normals == null)
		{
			drawNormals = mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal);

			if (drawNormals)
			{
				normals = mesh.normals;
			}
		}

		if (drawTangents
				&& tangents == null)
		{
			drawTangents = mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent);

			if (drawTangents)
			{
				tangents = mesh.tangents;
			}
		}

		if (drawTriangles
			&& triangles == null)
        {
			triangles = mesh.triangles;
        }

		Transform t = transform;

		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 position = t.TransformPoint(vertices[i]);

			if (drawVertices)
			{
				Gizmos.color = Color.cyan;

				Gizmos.DrawSphere(position, 0.02f);
			}

			if (drawNormals)
			{
				Gizmos.color = Color.green;

				Gizmos.DrawRay(position, t.TransformDirection(normals[i] * 0.2f));
			}

			if (drawTangents)
			{
				Gizmos.color = Color.red;

				Gizmos.DrawRay(position, t.TransformDirection(tangents[i] * 0.2f));
			}
		}

		if (drawTriangles)
        {
			float colorStep = 1.0f / (triangles.Length - 3);

			for (int i = 0; i < triangles.Length; i += 3)
            {
				float c = i * colorStep;

				Gizmos.color = new Color(c, 0.0f, c);
				
				Gizmos.DrawSphere(t.TransformPoint(vertices[triangles[i]] + vertices[triangles[i + 1]] + vertices[triangles[i + 2]]) * (1.0f / 3.0f), 0.02f);
            }
        }
	}

	void GenerateMesh()
	{
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		jobs[(int)meshType](mesh, meshData, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
		
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();

		if (meshOptimization == MeshOptimizationMode.ReorderIndices)
		{
			mesh.OptimizeIndexBuffers();
		}
		else if (meshOptimization == MeshOptimizationMode.ReorderVertices)
		{
			mesh.OptimizeReorderVertexBuffer();
		}
		else
		{
			mesh.Optimize();
		}
	}
}