using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;

using Microsoft.Extensions.Logging;

using Xbim.Common;

using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto;

using SceneComponent = Bitub.Dto.Scene.Component;
using SceneMaterial = Bitub.Dto.Scene.Material;

using Bitub.Ifc.Concept;
using Bitub.Dto.Concept;

using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

using SharpGLTF.Geometry.VertexTypes;

namespace Bitub.Ifc.Export
{
    public class IfcGLTFSceneExporter
    {
        #region Internals        
        private readonly ILogger logger;
        #endregion

        /// <summary>
        /// Export preferences.
        /// </summary>
        public IfcExportSettings Preferences { get; private set; }

        /// <summary>
        /// Default color settings.
        /// </summary>
        public XbimColourMap DefaultProductColorMap { get; set; } = new XbimColourMap(StandardColourMaps.IfcProductTypeMap);


        public IfcGLTFSceneExporter(IfcExportSettings preferences, ILoggerFactory loggerFactory = null)
        {
            logger = loggerFactory?.CreateLogger<IfcGLTFSceneExporter>();
            Preferences = preferences;
        }

        public Task<IfcGLTFExportResult> RunExport(IModel model, CancelableProgressing monitor)
        {
            return Task.Run(() =>
            {
                try
                {
                    return new IfcGLTFExportResult(
                        BuildScene(model, new IfcExportSettings(Preferences), monitor), model, Preferences);
                }
                catch (Exception e)
                {
                    monitor?.State.MarkBroken();
                    logger.LogError("{0}: {1} [{2}]", e.GetType().Name, e.Message, e.StackTrace);
                    return new IfcGLTFExportResult(e, model, Preferences);
                }
            });
        }

        public ModelRoot BuildScene(IModel model, IfcExportSettings settings, CancelableProgressing monitor)
        {
            var sceneBuilder = new SharpGLTF.Scenes.SceneBuilder();

            var materials = model.ToMaterialBySurfaceStyles().ToDictionary(
                m => m.Id.Nid,
                m => ConvertToMaterial(m));

            // TODO

            return sceneBuilder.ToGltf2();
        }

        public static MaterialBuilder ConvertToMaterial(SceneMaterial m)
        {
            return new MaterialBuilder();
        }
    }
}
