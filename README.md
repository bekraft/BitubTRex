# BitubTRex

## Goal

BitubTRex uses the [Xbim libraries](https://github.com/xBimTeam) and adds some domain driven functionalities.

Provided assemblies:
- ```Bitub.Ifc``` (net47) wraps all extensions, workflows and additions concerning Xbim IFC model handling 
- ```Bitub.Dto``` (netcoreapp31, net47) wraps protocol buffers for scene transmitting and other data
- ```Bitub.Dto.Bcf```(netcoreapp31, net47) BCF 2.1 adapter

## Use cases

See the [Wiki](https://github.com/bekraft/BitubTRex/wiki) for the use cases.

- Building IFCs programmatically.
- Transforming IFCs by async transformation requests.
- Exporting IFCs to other formats.

## Licenses

- CDDL [Xbim Essentials](https://github.com/xBimTeam/XbimEssentials) and [Xbim Geometry](https://github.com/xBimTeam/XbimGeometry)
- Apache 2.0 for BitubTRex
