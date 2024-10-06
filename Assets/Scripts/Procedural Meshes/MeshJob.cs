using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMeshes
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<G, S> : IJobFor where G : struct, IMeshGenerator where S : struct, IMeshStreams
    {
        private G generator;

        [WriteOnly]
        private S streams;

        public void Execute(int _i) => generator.Execute(_i, streams);

        public static JobHandle ScheduleParallel(Mesh _mesh, Mesh.MeshData _meshData, int _resolution, JobHandle _dependency)
        {
            var job = new MeshJob<G, S>();

            job.generator.Resolution = _resolution;

            job.streams.Setup(_meshData, _mesh.bounds = job.generator.Bounds, job.generator.VertexCount, job.generator.IndexCount);

            return job.ScheduleParallel(job.generator.JobLength, 1, _dependency);
        }
    }

    public delegate JobHandle MeshJobScheduleDelegate(Mesh _mesh, Mesh.MeshData _meshData, int _resolution, JobHandle _dependency);
}