using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitub.Transfer.Spatial;

namespace Bitub.Transfer.Scene
{
    public static class InMemorySceneModelFilterExtensions
    {
        public static SceneModel FilterBy(this SceneModel model, SceneDataType sceneDataType)
        {
            if (sceneDataType.IsFlaggedExactly(SceneDataType.All))
                return model;
            // TODO
            throw new NotImplementedException();
        }

        public static SceneModel FilterBy(this SceneModel model, ABox aBox)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
