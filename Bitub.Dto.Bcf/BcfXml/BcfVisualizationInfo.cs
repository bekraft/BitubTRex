using System;

using Bitub.Dto.Spatial;

using System.Xml.Serialization;
using System.Xml;

namespace Bitub.Dto.BcfXml
{
    [XmlRoot(ElementName = "VisualizationInfo")]
    public class BcfVisualizationInfo
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns { get; set; }

        [XmlAttribute(AttributeName = "Guid")]
        public System.Guid ID { get; set; } = System.Guid.NewGuid();

        [XmlElement(ElementName = "Components")]
        public BcfViewpointComponents Components { get; set; }
        [XmlElement(ElementName = "PerspectiveCamera")]
        public BcfPerspectiveCamera PerspectiveCamera { get; set; }
        [XmlElement(ElementName = "OrthogonalCamera")]
        public BcfOrthogonalCamera OrthogonalCamera { get; set; }
        [XmlArray(ElementName = "Lines"), XmlArrayItem(ElementName = "Line")]
        public BcfLine[] Lines { get; set; } = new BcfLine[0];
        [XmlArray(ElementName = "ClippingPlanes"), XmlArrayItem(ElementName = "ClippingPlane")]
        public BcfClippingPlane[] ClippingPlanes { get; set; } = new BcfClippingPlane[0];
        [XmlElement(ElementName = "Bitmap")]
        public BcfBitmap[] Bitmap { get; set; } = new BcfBitmap[0];

        [XmlAnyAttribute]
        public XmlAttribute[] Unknown { get; set; }
    }

    public class BcfViewpointComponents
    {
        [XmlElement(ElementName = "ViewSetupHints")]
        public BcfViewSetupHints ViewSetupHints { get; set; }
        [XmlArray(ElementName = "Selection"), XmlArrayItem(ElementName = "Component")]
        public BcfComponent[] Selection { get; set; } = new BcfComponent[0];
        [XmlElement(ElementName = "Visibility")]
        public BcfVisibility Visibility { get; set; }
        [XmlElement(ElementName = "Coloring")]
        public BcfComponentColoring Coloring { get; set; }
    }

    public class BcfComponent
    {
        [XmlAttribute(AttributeName = "IfcGuid")]
        public string IfcGuid { get; set; }

        [XmlElement(ElementName = "OriginatingSystem")]
        public string OriginatingSystem { get; set; }
        [XmlElement(ElementName = "AuthoringToolId")]
        public string AuthoringToolId { get; set; }
    }

    public class BcfCamera
    {
        [XmlElement(ElementName = "CameraViewPoint")]
        public XYZ CameraViewPoint { get; set; }
        [XmlElement(ElementName = "CameraDirection")]
        public XYZ CameraDirection { get; set; }
        [XmlElement(ElementName = "CameraUpVector")]
        public XYZ CameraUpVector { get; set; }
    }

    public class BcfPerspectiveCamera : BcfCamera
    {
        [XmlElement(ElementName = "FieldOfView")]
        public float FieldOfView { get; set; }
    }

    public class BcfOrthogonalCamera : BcfCamera
    {
        [XmlElement(ElementName = "ViewToWorldScale")]
        public float ViewToWorldScale { get; set; }
    }

    public class BcfViewSetupHints
    {
        [XmlAttribute(AttributeName = "SpacesVisible")]
        public bool IsSpacesVisible { get; set; }
        [XmlAttribute(AttributeName = "SpaceBoundariesVisible")]
        public bool IsSpaceBoundariesVisible { get; set; }
        [XmlAttribute(AttributeName = "OpeningsVisible")]
        public bool IsOpeningsVisible { get; set; }
    }

    public class BcfColor
    {
        [XmlElement(ElementName = "Component")]
        public BcfComponent[] Component { get; set; } = new BcfComponent[0];
        [XmlAttribute(AttributeName = "Color")]
        public string Color { get; set; }
    }

    public class BcfComponentColoring
    {
        [XmlElement(ElementName = "Color")]
        public BcfColor[] Color { get; set; } = new BcfColor[0];
    }

    public class BcfVisibility
    {
        [XmlArray(ElementName = "Exceptions"), XmlArrayItem(ElementName = "Component")]
        public BcfComponent[] Exceptions { get; set; } = new BcfComponent[0];
        [XmlAttribute(AttributeName = "DefaultVisibility")]
        public bool IsDefaultVisibility { get; set; }
    }

    public class BcfLine
    {
        [XmlElement(ElementName = "StartPoint")]
        public XYZ StartPoint { get; set; }
        [XmlElement(ElementName = "EndPoint")]
        public XYZ EndPoint { get; set; }
    }

    public class BcfClippingPlane
    {
        [XmlElement(ElementName = "Location")]
        public XYZ Location { get; set; }
        [XmlElement(ElementName = "Direction")]
        public XYZ Direction { get; set; }
    }

    public enum BcfBitmapFormat
    {
        PNG, JPG
    }

    public class BcfBitmap
    {
        [XmlElement(ElementName = "Bitmap")]
        public BcfBitmapFormat Format { get; set; } = BcfBitmapFormat.PNG;
        [XmlElement(ElementName = "Reference")]
        public string Reference { get; set; }
        [XmlElement(ElementName = "Location")]
        public XYZ Location { get; set; }
        [XmlElement(ElementName = "Normal")]
        public XYZ Normal { get; set; }
        [XmlElement(ElementName = "Up")]
        public XYZ Up { get; set; }
        [XmlElement(ElementName = "Height")]
        public float Height { get; set; }
    }
}
