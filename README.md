# Bitub TRex
![Build status](https://dev.azure.com/bitub/BitubTRex/_apis/build/status/bekraft.BitubTRex?branchName=dev&label=DEV)
![Build status](https://dev.azure.com/bitub/BitubTRex/_apis/build/status/bekraft.BitubTRex?branchName=master&label=MASTER)

a cross-plattform .NET core / standard library collection for manipulation of data in AEC domain. It is used as baseline library for [Bitub TRexXbim](https://github.com/bekraft/BitubTRexXbim). It provides an API to compiled protocol buffers to exchange scene, spatial and semantic data of AEC models from multiple sources.

## Protocol schemata

- ```Scene```, a 3D scene graph exchange protocol
- ```Spatial```, a basic spatial exchange protocol
- ```Concept```, an experimental semantic validation rule and exchange protocol (testing only)

## Provided assemblies:
- ```Bitub.Dto``` wraps compiled protocol buffers for information exchange.
- ```Bitub.Dto.Bcf``` reading & writing of BCF 2.1 XML 
- ```Bitub.Dto.Cpi``` reading & writing of CPIXML files

## Supported 3rd party schemata

- ```BCF-XML```, aka Building Collaboration Format scheme based on XML)
- ```CPI-XML```, widely used multi-container schema based on XML

## Building and usage

```
dotnet build -c [Debug|Beta|Release]
```

## License

>Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
>
>       http://www.apache.org/licenses/LICENSE-2.0
>
>Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.