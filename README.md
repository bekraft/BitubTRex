# BitubTRex

is high-level wrapper of the [Xbim libraries](https://github.com/xBimTeam).
It abstracts IFC model transformations and model transferring tasks.

Provided assemblies:
- ```Bitub.Ifc``` wraps all extensions, workflows and additions concerning Xbim IFC model handling 
- ```Bitub.Transfer``` wraps protocol buffers for scene transmitting and other data

## Bitub.Ifc
#### Extension methods

 Create new instances by ```IfcStore``` extension:
 ```
    using(var ifcStore = IfcStore.Open(...))
    using(var tx = ifcStore.BeginTransaction(...))
    {
        var pSingletonSet = ifcStore.NewIfcPropertySet("My new set");
        var relation = ifcStore.NewIfcRelDefinesByProperties(pSingletonSet);

        foreach(var wall in ifcStore.Instances.OfType<IIfcWall>())
        {
            relation.RelatedObjects.Add(wall);
        }
        tx.Commit();
    }
 ```

#### IFC builder pattern

  Create and modify models by builder pattern:
  ```
    using (var ifcStore = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel))
    {
        var builder = new Ifc4Builder(store, LoggingFactory);
        var site = builder.NewSite("A site");
        var buildingInSite = builder.NewBuilding("A hosted building");
        var storeyInBuilding = builder.NewStorey("A hosted storey");

        var globalPlacement = builder.NewLocalPlacement(new Xbim.Common.Geometry.XbimVector3D());
        var newWallInStorey = builder.NewProduct<IIfcWallStandardCase>(globalPlacement);
    }
  ```

  Builders are context sensitive. New products are hosted within the most recent spatial scope.
  To drop the most recent context, use
  ```
    builder.DropCurrentScope();
  ```
  If you like to create custom scopes (i.e. functional groups) use
  ```
    builder.NewScope(myIfcProductInstance)
  ```

#### IFC transformation pattern

 Transformations map and modify existing instances and data to new by rules.

 Existing transformation requests:
 - Drop IFC specific property sets
 - Drop IFC specific properties
 - ...

## Bitub.Transfer
#### - Scene - model

 A scene model is an extraction of the component meshes and the model hierarchy. It is
meant to be a lightweight import for visualization purposes.

```
IfcSceneExportSummary result;
var loggerFactory = new LoggerFactory().AddConsole();
using (var store = IfcStore.Open(fileName))
{
    var exporter = new IfcSceneExporter(new XbimTesselationContext(loggerFactory), loggerFactory);
    exporter.Settings = settings;

    result = await exporter.Run(store);
}
```

To export the extracted scene into JSON or binaray protobuf follow the [Google Protobuf documentation](https://developers.google.com/protocol-buffers).

```
using (var jsonStream = File.CreateText($"{Path.GetFileNameWithoutExtension(fileName)}.json"))
{
    var json = formatter.Format(result.Scene);
    jsonStream.WriteLine(json);
    jsonStream.Close();    
}
```

#### - Classify - model
To be documented.

#### - Spatial - model
To be documented.

#### - Collab - model
To be documented.

#### - TRex - model
To be documented.