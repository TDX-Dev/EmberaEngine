# Devoid Engine

Devoid Engine is a custom-built game engine designed as a personal learning project to better understand modern game engine architecture and rendering systems. It primarily uses OpenGL for rendering and is written in C# using OpenTK. While it is not intended for production use, it includes many modern engine features and supports editor integration.

## Features

- [x] 3D Renderer
- [x] 2D Renderer
- [x] 2D Animations
- [ ] 3D Skeletal Animations 
- [x] Parent-Child Transforms
- [x] BEPU Physics Integration
- [x] Component System
- [x] Asset Management
- [x] Type Serialization
- [x] Assimp Model Importer!
- [x] Asset hotreloading
- [x] Async Texture Loading
- [x] Async Mesh Loading
- [x] Async Materials Loading

## Renderer Features

- [x] Bloom
- [x] Screen space ambient occlusion
- [x] Physically based rendering
- [x] Image based lighting
- [x] Skybox rendering with HDRI support
- [x] Tonemapping

## Screenshots

![Preview 1](https://github.com/TDX-Dev/EmberaEngine/blob/main/Previews/PREVIEW_2.png)

![Preview 2](https://github.com/TDX-Dev/EmberaEngine/blob/main/Previews/PREVIEW_1.png)

## Installation
You need .NET 8.0 to run this project.
Currently, to work with the editor you will **require** a editor project to launch via the engine starter project.

## Usage

### Creating a GameObject

```
GameObject gameObject = this.gameObject.scene.addGameObject(); // Optionally specify a name or set gameObject.Name
```

### Adding components

```
gameObject.AddComponent<MeshRenderer>();
// or
MeshRenderer meshRenderer = new MeshRenderer();
gameObject.AddComponent(meshRenderer);
```


## Contributing
👋 Thanks for your interest! However, this project isn't accepting outside contributions. Feel free to fork it or open issues for feedback.

## License

GPL-3.0 License. See [LICENSE](LICENSE) for more info.