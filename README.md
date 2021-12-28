# Bitub TRex
![Dev status](https://dev.azure.com/bitub/BitubTRex/_apis/build/status/bekraft.BitubTRex?branchName=dev)

a cross-plattform .NET core / standard library collection for manipulation of data in AEC domain.

## Schemata

- Scene, a 3D scene graph exchange protocol
- Spatial, a basic spatial exchange protocol
- Concept, a semantic validation rule and exchange protocol

## Provided assemblies:
- ```Bitub.Dto``` (netstandard2.1, net5.0) wraps protocol buffers for scene transmitting and other data
- ```Bitub.Dto.Bcf``` (netstandard2.1, net5.0) BCF 2.1 adapter
- ```Bitub.Dto.Cpi``` (netstandard2.1, net5.0) CPI 1.2+ adapter

## Supported 3rd party schemata

- ```BCF-XML```, aka Building Collaboration Format scheme based on XML)
- ```CPI-XML```, widely used multi-container schema based on XML

## Building and usage

```
dotnet build -c [Debug|Beta|Release]
```

## License

Apache-2 License