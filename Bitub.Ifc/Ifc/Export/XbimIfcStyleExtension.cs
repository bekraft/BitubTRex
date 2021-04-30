using System.Collections.Generic;
using System.Linq;

using Xbim.Common;
using Xbim.Ifc4.Interfaces;

using Bitub.Dto;
using Bitub.Dto.Scene;

namespace Bitub.Ifc.Export
{
    public static class XbimIfcStyleExtension
    {
        public static Color ToColor(this IIfcColourRgb rgb, float alpha = 1.0f)
        {
            return new Color()
            {
                R = (float)rgb.Red,
                G = (float)rgb.Green,
                B = (float)rgb.Blue,
                A = alpha
            };
        }

        public static ColorOrNormalised ToColorOrNormalised(this IIfcColourOrFactor rgbOrFactor, ColorChannel colorChannel, float alpha = 1.0f)
        {
            if (null == rgbOrFactor)
                return null;

            var color = new ColorOrNormalised();
            if(rgbOrFactor is IExpressRealType real)
            {
                color.Normalised = (float)real.Value;
            } else if (rgbOrFactor is IIfcColourRgb rgb)
            {
                color.Color = rgb.ToColor(alpha);
            }
            return color;
        }

        public static Material ToMaterial(this IIfcSurfaceStyle style)
        {
            var material = new Material()
            {
                Id = new RefId { Nid = style.EntityLabel }, 
                Name = style.Name?.ToString() ?? $"{style.EntityLabel}",
                HintRenderBothFaces = style.Side == IfcSurfaceSide.BOTH,
                HintSwitchFrontRearFaces = style.Side == IfcSurfaceSide.NEGATIVE
            };

            foreach (var surfaceStyle in style.Styles)
            {
                if (surfaceStyle is IIfcSurfaceStyleShading ss)
                {
                    material.ColorChannels.AddRange(ss.ToColorChannel());
                }
                else if (surfaceStyle is IIfcSurfaceStyleRendering sr)
                {
                    material.ColorChannels.AddRange(sr.ToColorChannel());
                    material.HintReflectionShader = sr.ReflectanceMethod.ToString();
                }
                else if (surfaceStyle is IIfcSurfaceStyleLighting sl)
                {
                    material.ColorChannels.AddRange(sl.ToColorChannnel());
                }
            }
            return material;
        }

        public static IEnumerable<ColorOrNormalised> ToColorChannel(this IIfcSurfaceStyleShading ss)
        {
            return new ColorOrNormalised[] {
                new ColorOrNormalised()
                {
                    Channel = ColorChannel.Albedo,
                    Color = ss.SurfaceColour.ToColor(1.0f - (float)(ss.Transparency ?? 0))
                }
            };
        }

        public static IEnumerable<ColorOrNormalised> ToColorChannel(this IIfcSurfaceStyleRendering sr)
        {
            // Adapting Xbim Texture transformation logic here
            float alpha = 1.0f - (float)(sr.Transparency ?? 0);
            return new ColorOrNormalised[]
            {
                new ColorOrNormalised() {
                    Channel = ColorChannel.Albedo,
                    Color = sr.SurfaceColour.ToColor()
                },
                sr.DiffuseColour?.ToColorOrNormalised(ColorChannel.Diffuse, alpha),
                sr.ReflectionColour?.ToColorOrNormalised(ColorChannel.Diffuse, alpha),
                sr.TransmissionColour?.ToColorOrNormalised(ColorChannel.Emmisive, alpha),
                sr.DiffuseTransmissionColour?.ToColorOrNormalised(ColorChannel.DiffuseEmmisive, alpha),
                sr.SpecularColour?.ToColorOrNormalised(ColorChannel.Specular, alpha)
            }.OfType<ColorOrNormalised>();
        }

        public static IEnumerable<ColorOrNormalised> ToColorChannnel(this IIfcSurfaceStyleLighting sl)
        {
            return new ColorOrNormalised[]
            {
                new ColorOrNormalised()
                {
                    Channel = ColorChannel.Diffuse,
                    Color = sl.DiffuseReflectionColour.ToColor()
                },
                new ColorOrNormalised()
                {
                    Channel = ColorChannel.DiffuseEmmisive,
                    Color = sl.DiffuseTransmissionColour.ToColor()
                },
                new ColorOrNormalised()
                {
                    Channel = ColorChannel.Emmisive,
                    Color = sl.TransmissionColour.ToColor()
                },
                new ColorOrNormalised()
                {
                    Channel = ColorChannel.Reflective,
                    Color = sl.ReflectanceColour.ToColor()
                }
            }.OfType<ColorOrNormalised>();
        }
    }
}
