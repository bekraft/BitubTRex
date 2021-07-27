using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using System.Numerics;

using Microsoft.Extensions.Logging;

using Xbim.Common;

using Xbim.Ifc;

using Bitub.Dto;

using SharpGLTF.Scenes;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

using Bitub.Dto.Scene;

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

        /// <summary>
        /// Only used if material reference is missing.
        /// </summary>
        public MaterialBuilder DefaultMissingMaterial { get; set; } = new MaterialBuilder().WithBaseColor(new Vector4(0.75f, 0.75f, 0.75f, 1));

        /// <summary>
        /// If true, context WCS will be set as root transform.
        /// </summary>
        public bool IsExportingContextWCS { get; set; } = false;


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

        private ModelRoot BuildScene(IModel model, ExportPreferences exportPreferences, CancelableProgressing monitor)
        {
            var mainScene = new SceneBuilder();
            var exportContext = new ExportContext<ExportPreferences>(loggerFactory);

            exportContext.InitContextsAndScaleFromModel(model, exportPreferences);

            // Init caching dictionaries
            var materials = model.ToMaterialBySurfaceStyles().ToDictionary(m => m.Id, m => MapMaterial(m));
            var sceneContexts = new Dictionary<Qualifier, SceneBuilder>();
            var bodyFacets = new Dictionary<RefId, Facet[]>();
            var queuedShapes = new List<Shape>();

            // Process tesselation messages
            foreach (var msg in tesselatorInstance.Tesselate(model, exportContext, monitor))
            {
                if (monitor?.State.IsAboutCancelling ?? false)
                {
                    logger?.LogInformation("Canceled model export of '{0}'", model.Header.FileName);
                    monitor.State.MarkCanceled();
                    break;
                }

                switch (msg.messageType)
                {
                    case TesselationMessageType.Context:
                        var contextScene = new SceneBuilder();
                        sceneContexts.Add(msg.SceneContext.sceneContext.Name, contextScene);
                        mainScene.AddScene(contextScene, IsExportingContextWCS ? msg.SceneContext.sceneContext.Wcs.ToNetMatrix4x4(): Matrix4x4.Identity);
                        break;
                    case TesselationMessageType.Representation:
                        var vertexMap = msg.ShapeRepresentation.shapeBody.Points.ToDictionary(pt => pt.Id);
                        bodyFacets.Add(
                            msg.ShapeRepresentation.shapeBody.Id, 
                            msg.ShapeRepresentation.shapeBody.Bodies.SelectMany(body => AssembleBody(body, vertexMap)).ToArray());
                        break;
                    case TesselationMessageType.Shape:
                        MapSceneBodies(model, msg.ProductShape.shapes,
                            sceneContexts, bodyFacets, materials,
                            shape => queuedShapes.Add(shape));
                        break;
                }
            }

            MapSceneBodies(model, queuedShapes, sceneContexts, bodyFacets, materials,
                shape => logger?.LogWarning("Either shape body ({0}) or scene context ({1}) not found for shape body.", shape.ShapeBody, shape.Context));

            return mainScene.ToGltf2();
        }

        #region Helpers

        private void MapSceneBodies(IModel model, IEnumerable<Shape> shapes, 
            IDictionary<Qualifier, SceneBuilder> sceneContextNodes, IDictionary<RefId, Facet[]> bodyAssembledFaces, IDictionary<RefId, MaterialBuilder> materials, 
            Action<Shape> handleMissingContextOrShapeBody)
        {
            foreach (var shape in shapes)
            {
                Facet[] assembledShape;
                SceneBuilder partialScene;
                if (bodyAssembledFaces.TryGetValue(shape.ShapeBody, out assembledShape)
                    && sceneContextNodes.TryGetValue(shape.Context, out partialScene))
                {

                    MaterialBuilder material;
                    if (!materials.TryGetValue(shape.Material, out material))
                    {
                        // If not present check for default (if negative)
                        if (shape.Material.Nid < 0)
                        {
                            var sceneMaterial = DefaultProductColorMap.ToMaterialByIfcTypeID(model, -shape.Material.Nid, nid => new RefId { Nid = -nid });
                            material = MapMaterial(sceneMaterial);
                            materials.Add(sceneMaterial.Id, material);
                        }
                        else
                        {
                            logger?.LogWarning("No material with RefID({0}) has been found. Using default material.", shape.Material);
                            material = DefaultMissingMaterial;
                        }
                    }

                    // Use default materials
                    var mesh = MapShapedBody(material, assembledShape);
                    partialScene.AddRigidMesh(mesh, shape.Transform.ToNetMatrix4x4());
                }
                else
                {
                    handleMissingContextOrShapeBody?.Invoke(shape);
                }
            }
        }

        private static IEnumerable<VertexPosition> MapVertexes(PtArray ptArray)
        {
            for (int i = 0; i < ptArray.Xyz.Count; i += 3)
            {
                yield return new VertexPosition(ptArray.Xyz[i], ptArray.Xyz[i + 1], ptArray.Xyz[i + 2]);
            }
        }

        private static VertexPositionNormal MapVertexNormal(Dto.Spatial.XYZ xyz, Dto.Spatial.XYZ n)
        {
            return new VertexPositionNormal(xyz.X, xyz.Y, xyz.Z, n.X, n.Y, n.Z);
        }

        private static MeshBuilder<VertexPositionNormal> MapShapedBody(MaterialBuilder material, Facet[] assembledShape)
        {
            var mesh = new MeshBuilder<VertexPositionNormal>();
            var tesselated = mesh.UsePrimitive(material);
            foreach (var f in assembledShape)
            {
                Dto.Spatial.XYZ[] normals;
                if (f.HasNormals)
                    normals = Enumerable.Range(0, 3).Select(k => f.GetNormal(k)).ToArray();
                else
                    normals = Enumerable.Repeat(f.Normal, 3).ToArray();

                tesselated.AddTriangle(MapVertexNormal(f.GetXYZ(0), normals[0]), MapVertexNormal(f.GetXYZ(1), normals[1]), MapVertexNormal(f.GetXYZ(2), normals[2]));
            }
            return mesh;
        }

        private static IEnumerable<Facet> AssembleBody(Body body, IDictionary<RefId, PtArray> vertexMap)
        {
            switch(body.BodySelectCase)
            {
                case Body.BodySelectOneofCase.MeshBody:                    
                    return body.MeshBody.ToFacets(vertexMap[body.MeshBody.Pts]);
                case Body.BodySelectOneofCase.FaceBody:
                    return body.FaceBody.ToFacets(vertexMap[body.FaceBody.Pts]);
                case Body.BodySelectOneofCase.WireBody:
                    throw new NotSupportedException("Wire body not supported.");
                default:
                    throw new NotImplementedException($"{body.BodySelectCase} not implemented.");
            }
        }

        private static Vector4 MapColor4(ColorOrNormalised c)
        {
            var (rgb, a) = MapColor3(c);
            return new Vector4(rgb, a);
        }

        private static (Vector3, float) MapColor3(ColorOrNormalised c)
        {
            switch (c.ColorOrValueCase)
            {
                case ColorOrNormalised.ColorOrValueOneofCase.Color:
                    return (new Vector3(c.Color.R, c.Color.G, c.Color.B), c.Color.A);
                case ColorOrNormalised.ColorOrValueOneofCase.Normalised:
                    return (new Vector3(c.Normalised, c.Normalised, c.Normalised), 1);
                case ColorOrNormalised.ColorOrValueOneofCase.None:
                    return (new Vector3(0.5f, 0.5f, 0.5f), 1);
                default:
                    throw new NotImplementedException();
            }
        }

        private static MaterialBuilder MapMaterialChannel(MaterialBuilder material, ColorOrNormalised c)
        {
            switch(c.Channel)
            {
                case ColorChannel.Albedo:
                    return material.WithBaseColor(MapColor4(c));
                case ColorChannel.Diffuse:
                    return material.WithDiffuse(MapColor4(c));
                case ColorChannel.Emmissive:
                    var (rgb, _) = MapColor3(c);
                    return material.WithEmissive(rgb);
                case ColorChannel.Specular:
                    var (specular, glossiness) = MapColor3(c);                    
                    return material.WithSpecularGlossiness(specular, glossiness);
                default:
                    throw new NotImplementedException();
            }
        }

        private static MaterialBuilder MapMaterial(Dto.Scene.Material m)
        {            
            var mb = new MaterialBuilder();
            m.ColorChannels.ForEach(ch => MapMaterialChannel(mb, ch));
            return mb;
        }

        #endregion
    }
}
