using System;

namespace Bitub.Dto.Scene.Validation
{
    [Flags]
    public enum MeshManifoldResult
    {
        Closed = 0, 
        Open = 1, 
        NonManifold = 2
    }
}