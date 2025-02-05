using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ProceduralMeshes.Streams
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16
    {
        public ushort a;
        public ushort b;
        public ushort c;

        public static implicit operator TriangleUInt16(int3 _t) => new TriangleUInt16
        {
                a = (ushort)_t.x,
                b = (ushort)_t.y,
                c = (ushort)_t.z
        };
    }
}