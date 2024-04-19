using System.Collections.Generic;
using Bitub.Dto.Spatial;

namespace Bitub.Dto.Scene.Validation
{
    public class MeshBuilder
    {
        private KdRange _kdRange;
        
        public double Eps { get; private set; }
        
        public MeshBuilder(double eps)
        {
            Eps = eps;
        }

        public void Append(XYZ point)
        {
            
        }

        public void Append(Face f)
        {
            
        }
    }
}