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
    public class GLTFExporter : IExporter<ModelRoot>
    {
        #region Internals        
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;        
        private readonly ITesselationContext<ExportPreferences> tesselatorInstance;
        #endregion

        /// <summary>
        /// Export preferences.
        /// </summary>
        public ExportPreferences Preferences { get; set; } = new ExportPreferences();

        /// <summary>
        /// Default color settings.
        /// </summary>
        public XbimColourMap DefaultProductColorMap { get; set; } = new XbimColourMap(StandardColourMaps.IfcProductTypeMap);


        public GLTFExporter(ITesselationContext<ExportPreferences> tesselatorInstance, ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory?.CreateLogger<GLTFExporter>();
            this.loggerFactory = loggerFactory;
            this.tesselatorInstance = tesselatorInstance;
        }

        public Task<ModelRoot> RunExport(IModel model, CancelableProgressing monitor)
        {
            return Task.Run(() => BuildScene(model, new ExportPreferences(Preferences), monitor));
        }

        private ModelRoot BuildScene(IModel model, ExportPreferences preferences, CancelableProgressing monitor)
        {
            var sceneBuilder = new SharpGLTF.Scenes.SceneBuilder();
            var exportContext = new ExportContext<ExportPreferences>(loggerFactory);

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
