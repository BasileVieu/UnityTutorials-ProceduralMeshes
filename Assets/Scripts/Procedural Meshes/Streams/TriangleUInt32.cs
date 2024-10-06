using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ProceduralMeshes.Streams
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt32
    {
        public int a;
        public int b;
        public int c;

        public static implicit operator TriangleUInt32(int3 _t) => new TriangleUInt32
        {
            a = _t.x,
            b = _t.y,
            c = _t.z
        };
    }
}