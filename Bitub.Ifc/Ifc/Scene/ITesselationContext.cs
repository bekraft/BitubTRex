using System;
using Bitub.Dto;
using Bitub.Dto.Scene;

using System.Collections.Generic;

using Xbim.Common;
using Xbim.Common.Geometry;

namespace Bitub.Ifc.Export
{
    /// <summary>
    /// A concrete shape of a product referencing a known / or unknown representation.
    /// </summary>
    public sealed class ProductShape 
    {
        public readonly int productLabel;
        public readonly IEnumerable<Shape> shapes;

        public ProductShape(int entityLabel, IEnumerable<Shape> shapes)
        {
            this.productLabel = entityLabel;
            this.shapes = shapes;
        }
    }

    /// <summary>
    /// A shape representation of one or more products.
    /// </summary>
    public sealed class ShapeRepresentation
    {
        public readonly int shapeLabel;
        public readonly ShapeBody shapeBody;

        public ShapeRepresentation(int shapeLabel, ShapeBody shapeBody)
        {
            this.shapeLabel = shapeLabel;
            this.shapeBody = shapeBody;
        }
    }

    /// <summary>
    /// A scene context and its world transform.
    /// </summary>
    public sealed class SceneContextTransform
    {
        public readonly int contextLabel;
        public readonly SceneContext sceneContext;
        public readonly XbimMatrix3D transform;

        public SceneContextTransform(int contextLabel, SceneContext sceneContext, XbimMatrix3D matrix3D)
        {
            this.contextLabel = contextLabel;
            this.sceneContext = sceneContext;
            this.transform = matrix3D;
        }
    }

    public enum TesselationMessageType
    {
        Context, Representation, Shape
    }

    public sealed class TesselationMessage
    {
        public readonly object message;
        public readonly TesselationMessageType messageType;

        public ProductShape ProductShape { get => message as ProductShape; }
        public ShapeRepresentation ShapeRepresentation { get => message as ShapeRepresentation; }
        public SceneContextTransform SceneContext { get => message as SceneContextTransform; }

        internal TesselationMessage(ProductShape productShape)
        {
            message = productShape;
            messageType = TesselationMessageType.Shape;
        }

        internal TesselationMessage(ShapeRepresentation shapeRepresentation)
        {
            message = shapeRepresentation;
            messageType = TesselationMessageType.Representation;
        }

        internal TesselationMessage(SceneContextTransform sceneContext)
        {
            message = sceneContext;
            messageType = TesselationMessageType.Context;
        }
    }

    /// <summary>
    /// A tesselation provider contract emitting either context, representation or shape instance in an abitrary order.
    /// </summary>
    public interface ITesselationContext<TSettings> where TSettings : ExportPreferences
    {
        IEnumerable<TesselationMessage> Tesselate(IModel m, ExportContext<TSettings> ec, CancelableProgressing progressing);
    }
}
