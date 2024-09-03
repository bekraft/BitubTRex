# Bitub.Dto 

This is a custom Data Transfer Objects (DTO) Library wrapped around scene, spatial and semantic concepts scopes in AEC industries.

- ```Scene```, a 3D scene graph exchange protocol
- ```Spatial```, a basic spatial exchange protocol
- ```Concept```, an experimental semantic validation rule and exchange protocol (testing only)

Internally, protobuf is used to serialize and deserialize data incoming or outgoing as binary or JSON.

The main goal is to provide a common object model to these scopes with additional extensions and helpers.

# License

Apache License, Version 2.0, Bernold Kraft