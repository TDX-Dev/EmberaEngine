using ElementalEditor.Editor.AssetHandling;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Core;
using EmberaEngine.Engine.AssetHandling;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Serializing;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using OpenTK.Windowing.Desktop;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using YamlDotNet.Core.Tokens;
using static EmberaEngine.Engine.Utilities.NewModelImporter;

namespace ElementalEditor.Editor.Panels
{
    class ViewportPanel : Panel
    {
        struct SupportedResolution
        {
            public int Width, Height, RefreshRate;

            public override bool Equals(object obj) => obj is SupportedResolution r &&
                r.Width == Width && r.Height == Height && r.RefreshRate == RefreshRate;
        }

        const int ToolbarHeight = 60;
        List<SupportedResolution> supportedResolutions = new();
        SupportedResolution selectedResolution;

        Texture viewportTexture;
        Framebuffer viewportBuffer;
        Framebuffer compositeBuffer;
        Shader blitShader;

        Vector2 viewportPos;
        (int Left, int Top, int Right, int Bottom) scaledViewport;

        int prevViewportHeight, prevViewportWidth;
        int viewportHeight, viewportWidth;

        bool isMouseOverWindow;
        bool isFirstFrame;
        bool freeAspectRatio = false;

        Scene sceneRestore;

        List<ViewportControl> controls;


        public override void OnAttach()
        {
            // Setting up UI

            controls = new List<ViewportControl>
            {
                new ViewportControl(ViewportAlignment.Left, () =>
                {
                    if (ImGui.BeginCombo("##ResCombo", $"{selectedResolution.Width}x{selectedResolution.Height}", ImGuiComboFlags.HeightLargest))
                    {
                        for (int i = 0; i < supportedResolutions.Count; i++)
                        {
                            var res = supportedResolutions[i];
                            bool isSelected = selectedResolution.Equals(res) && !freeAspectRatio;
                            if (ImGui.Selectable($"{res.Width}x{res.Height}", isSelected))
                            {
                                selectedResolution = res;
                                freeAspectRatio = false;

                                Renderer.Resize(res.Width, res.Height);
                                Screen.Size.X = res.Width;
                                Screen.Size.Y = res.Height;
                                editor.EditorCurrentScene.OnResize(res.Width, res.Height);

                                DebugLogPanel.Log("RESIZED RENDERER", DebugLogPanel.DebugMessageSeverity.Information, "Viewport Change");
                            }
                        }

                        if (ImGui.Selectable("Free Aspect", freeAspectRatio))
                        {
                            freeAspectRatio = true;
                            Renderer.Resize(viewportWidth, viewportHeight);
                            Screen.Size.X = viewportWidth;
                            Screen.Size.Y = viewportHeight;
                            editor.EditorCurrentScene.OnResize(viewportWidth, viewportHeight);
                        }
                        ImGui.EndCombo();
                    }
                }, new Vector2(200f)),
                new ViewportControl(ViewportAlignment.Center, () =>
                {
                    if (ImGui.Button(editor.EditorCurrentScene.IsPlaying ? MaterialDesign.Pause : MaterialDesign.Play_arrow, new Vector2(40f)))
                    {
                        if (editor.EditorCurrentScene.IsPlaying)
                        {
                            int priorGOIndex = editor.EditorCurrentScene.GameObjects.IndexOf(GameObjectPanel.SelectedObject);
                            editor.EditorCurrentScene.Dispose();
                            editor.EditorCurrentScene = sceneRestore;
                            editor.EditorCurrentScene.Initialize();
                            editor.StartEditorComponents();
                            GameObjectPanel.SelectedObject = (priorGOIndex != -1 && priorGOIndex < editor.EditorCurrentScene.GameObjects.Count) ? editor.EditorCurrentScene.GameObjects[priorGOIndex] : null;
                            

                        } else
                        {
                            sceneRestore = SceneSerializer.DeSerialize(SceneSerializer.Serialize(editor.EditorCurrentScene));
                            editor.EditorCurrentScene.Play();
                        }
                    }
                }),

                new ViewportControl(
                ViewportAlignment.Right,
                () =>
                {
                    var btnGroup = new ButtonGroup("TestGroup 1", ButtonGroup.RenderMode.CustomDraw);
                    //btnGroup.padding = new Vector2(12, 12);
                    btnGroup.Add(MaterialDesign.Arrow_back, () => {Console.WriteLine("Clicked!"); });

                    btnGroup.Add(MaterialDesign.Arrow_upward, () => {});
                    btnGroup.Add(MaterialDesign.Arrow_downward, () => {});
                    btnGroup.Add(MaterialDesign.Arrow_forward, () => {});
                    btnGroup.Render();
                },
                new (400, 40)
                ),

                new ViewportControl(ViewportAlignment.Right, () =>
                {
                    var btnGroup = new ButtonGroup("Test Group 2", ButtonGroup.RenderMode.CustomDraw);
                    btnGroup.Add("Save Scene", () =>
                    {
                        byte[] json = SceneSerializer.Serialize(editor.EditorCurrentScene);

                        using (FileStream fs = File.Create(Path.Join(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY, "scene1.dscn")))
                        {
                            fs.Write(json);
                        }

                        DebugLogPanel.Log("Saved Scene: " + editor.EditorCurrentScene.Name, DebugLogPanel.DebugMessageSeverity.Information, "Editor");
                    });

                    btnGroup.Add(MaterialDesign.Settings, () =>
                    {
                        ImGui.OpenPopup("OptionsPopup");

                        if (ImGui.BeginPopup("OptionsPopup"))
                    {
                        if (ImGui.BeginMenu("Overlays"))
                        {
                            ImGui.TextDisabled("Scene Gizmos");

                            ViewportUtil.ToggleGizmo("Colliders", GizmoType.PhysicsCollider);
                            ViewportUtil.ToggleGizmo("Lights", GizmoType.Light);
                            ViewportUtil.ToggleGizmo("Cubes", GizmoType.Cube);
                            ViewportUtil.ToggleGizmo("Circles", GizmoType.Circle);

                            ImGui.Separator();
                            ImGui.TextDisabled("UI Gizmos");

                            ViewportUtil.ToggleGizmo("Textures", GizmoType.Texture);

                            ImGui.Separator();

                            if (ImGui.Button("Enable All"))
                                Guizmo3D.EnabledGizmos = GizmoType.All;
                            if (ImGui.Button("Disable All"))
                                Guizmo3D.EnabledGizmos = GizmoType.None;


                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Render Mode"))
                        {
                            RenderSetting rs = Renderer3D.ActiveRenderingPipeline.GetRenderSettings();
                            bool isSet = false;
                            if (ImGui.MenuItem("Solid", "", rs.renderMode == RenderMode.Solid))
                            {
                                rs.renderMode = RenderMode.Solid;
                                isSet = true;
                            }

                            if (ImGui.MenuItem("Wireframe", "", rs.renderMode == RenderMode.Wireframe))
                            {
                                rs.renderMode = RenderMode.Wireframe;
                                isSet = true;

                            }

                            if (ImGui.MenuItem("Unlit", "", rs.renderMode == RenderMode.Unlit))
                            {
                                rs.renderMode = RenderMode.Unlit;
                                isSet = true;
                            }

                            if (isSet)
                            {
                                Renderer3D.ActiveRenderingPipeline.SetRenderSettings(rs);
                            }

                            ImGui.EndMenu();
                        }


                       ImGui.EndPopup();
                    }
                    });

                    btnGroup.Render();

                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

                    

                    ImGui.PopStyleVar();
                },
                    new Vector2(200, 40)
                )
            };






            compositeBuffer = Renderer3D.GetOutputFrameBuffer();

            InitViewportBuffer();
            blitShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Editor/Assets/Shaders/viewportBlitShader.frag");

            InitSupportedResolutions();
            SetInitialResolution();

            Guizmo3D.Initialize();
        }



        void InitViewportBuffer()
        {
            viewportTexture = new Texture(TextureTarget2d.Texture2D);
            viewportBuffer = new Framebuffer("VIEWPORT FB");
            viewportBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, viewportTexture);
        }

        void InitSupportedResolutions()
        {
            var videoModes = Monitors.GetMonitors()[0].SupportedVideoModes;
            foreach (var mode in videoModes)
            {
                if (mode.RefreshRate != 60) continue;
                supportedResolutions.Add(new SupportedResolution
                {
                    Width = mode.Width,
                    Height = mode.Height,
                    RefreshRate = mode.RefreshRate
                });
            }
        }

        void SetInitialResolution()
        {
            var monitor = Monitors.GetMonitors()[0];
            foreach (var res in supportedResolutions)
            {
                if (res.Width == monitor.HorizontalResolution && res.Height == monitor.VerticalResolution)
                {
                    selectedResolution = res;
                    selectedResolution.Width = 1920;
                    selectedResolution.Height = 1080;
                    ApplyResolution(selectedResolution.Width, selectedResolution.Height);
                    break;
                }
            }
        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            EditorUI.BeginWindow(MaterialDesign.Landscape + " Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            isMouseOverWindow = ImGui.IsWindowHovered();
            editor.EditorCamera.LockCamera = !isMouseOverWindow;

            // 📌 DRAW TOOLBAR FIRST
            DrawToolbar(); // This will now occupy vertical space in layout

            viewportPos = ImGui.GetCursorScreenPos();
            Vector2 avail = ImGui.GetContentRegionAvail();
            viewportWidth = (int)avail.X;
            viewportHeight = (int)avail.Y;

            HandleViewportResize();

            scaledViewport = CalculateScaledViewport(viewportWidth, viewportHeight, selectedResolution.Width, selectedResolution.Height);

            RenderToViewport();

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);

            ImGui.Image((IntPtr)viewportBuffer.GetFramebufferTexture(0).GetRendererID(),
                        new Vector2(viewportWidth, viewportHeight),
                        new Vector2(0, 0), new Vector2(1, -1));

            ImGui.PopStyleVar(2);

            if (ImGui.BeginDragDropTarget())
            {
                HandleViewportDrop();


                ImGui.EndDragDropTarget();
            }


            ImGui.SetCursorPos(new Vector2(scaledViewport.Left + 20, scaledViewport.Top + ToolbarHeight * 2));
            ImGui.SetNextItemWidth(100);

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, EditorUI.FramePadding);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, EditorUI.FramePadding);
            EditorUI.DrawCollapsingHeader("Info", () =>
            {
                UI.BeginPropertyGrid("##scene_info_grid");

                UI.BeginProperty("Scene:");
                UI.PropertyText(editor.EditorCurrentScene.Name ?? "No Name Set");
                UI.EndProperty();

                UI.BeginProperty("GameObjects:");
                UI.PropertyText(editor.EditorCurrentScene.GameObjects.Count.ToString());
                UI.EndProperty();

                UI.BeginProperty("Meshes:");
                UI.PropertyText(Renderer3D.GetMeshes().Count.ToString());
                UI.EndProperty();

                UI.EndPropertyGrid();
            }, 400);
            ImGui.PopStyleVar(2);

            EditorUI.EndWindow();
            ImGui.PopStyleVar();

            DrawImportWindow();
        }

        unsafe void HandleViewportDrop()
        {
            var payload = ImGui.AcceptDragDropPayload("ASSET_DRAG");
            if (payload.NativePtr == null) return;

            string data = Marshal.PtrToStringAnsi(payload.Data);
            
            string fileExtension = Path.GetExtension(data).Replace(".", "");
            string resolvedAssetType = AssetType.ResolveAssetType(fileExtension);


            if (resolvedAssetType == AssetType.SCENE_FILE)
            {
                Scene scene = SceneSerializer.DeSerialize(File.ReadAllBytes(Path.Combine(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY, data)));

                editor.EditorCurrentScene.Dispose();
                editor.EditorCurrentScene = scene;
                GameObjectPanel.SelectedObject = null;
                scene.Initialize();
                editor.StartEditorComponents();

                Renderer3D.SetRenderCamera(editor.EditorCamera.Camera);
            } else if (resolvedAssetType == AssetType.MODEL_FILE)
            {
                OpenImportWindow(data);
            } else if (resolvedAssetType == AssetType.MESH_FILE)
            {
                GameObject go = editor.EditorCurrentScene.addGameObject("MeshObj");
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                MeshReference meshReference = (MeshReference)AssetLoader.Load<Mesh>(data);
                if (meshReference.isLoaded)
                {
                    mr.SetMesh(meshReference.value);
                    Console.WriteLine("Material " + (meshReference.value).Id);
                } else
                {
                    meshReference.OnLoad += (value) =>
                    {

                        mr.SetMesh(value);
                        Console.WriteLine("Material " + value.MaterialReference);
                    };
                }


            }
            


        }

        string importFilePath = null;
        bool showImportWindow = false;
        float importScale = 1.0f;
        string meshSavePath = "";

        void OpenImportWindow(string filepath)
        {
            importFilePath = filepath;
            showImportWindow = true;
        }

        void DrawImportWindow()
        {
            if (!showImportWindow) return;

            ImGui.OpenPopup("Import Model");

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 20));

            if (ImGui.BeginPopupModal("Import Model", ref showImportWindow, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"Importing:\n{importFilePath}");
                
                ImGui.Text("Scale");
                ImGui.SliderFloat("##Scale", ref importScale, 0.01f, 10.0f);

                ImGui.Text("Save Meshes at");
                ImGui.SetNextItemWidth(-1);
                FolderDropdownWidget.FolderDropdown("##Folder Select", ref meshSavePath, Path.Combine(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY));

                if (ImGui.Button("Import", new Vector2(-1, 40)))
                {
                    // Actually load the model using your importer
                    string fullPath = Path.Combine(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY, importFilePath);
                    Console.WriteLine(importFilePath);
                    var spec = new ModelLoaderSpecification()
                    {
                        resourcePath = importFilePath,
                        importScale = importScale
                    };

                    DebugLogPanel.Log("Loading model at: " + importFilePath, DebugLogPanel.DebugMessageSeverity.Information, "Model Importer");
                    DateTime time = DateTime.Now;

                    ModelGraphData modelGraph = NewModelImporter.Load(spec);

                    // Map of old -> new material IDs
                    var materialGuidMap = new Dictionary<Guid, Guid>();

                    // Assign final IDs and register materials first
                    for (int i = 0; i < modelGraph.materials.Count; i++)
                    {
                        var material = modelGraph.materials[i];
                        Guid oldGuid = material.Id;

                        string relPath = Path.Combine("Materials", material.GetHashCode() + ".dmat");

                        if (AssetLookup.pathToGuid.TryGetValue(relPath, out Guid existingGuid))
                        {
                            material.Id = existingGuid;
                        }
                        else
                        {
                            Guid newGuid = material.Id;
                            if (AssetLookup.guidToPath.ContainsKey(newGuid))
                            {
                                newGuid = Guid.NewGuid();
                            }

                            material.Id = newGuid;
                            AssetLookup.RegisterFile(newGuid, relPath);
                        }

                        materialGuidMap[oldGuid] = material.Id;

                        DiskUtilities.SaveMaterial(VirtualFileSystem.ResolvePath(relPath), (PBRMaterial)material);
                    }
                    for (int i = 0; i < modelGraph.meshNodes.Count; i++)
                    {
                        MeshNode meshNode = modelGraph.meshNodes[i];
                        // Fix material reference GUID
                        if (materialGuidMap.TryGetValue(meshNode.mesh.MaterialReference, out Guid newMatGuid))
                        {
                            meshNode.mesh.MaterialReference = newMatGuid;
                        }

                        string relPath = Path.Combine("Meshes", meshNode.name + ".dmsh");

                        if (AssetLookup.pathToGuid.TryGetValue(relPath, out Guid existingGuid))
                        {
                            meshNode.mesh.Id = existingGuid;
                        }
                        else
                        {
                            Guid newGuid = meshNode.mesh.Id;
                            if (AssetLookup.guidToPath.ContainsKey(newGuid))
                            {
                                newGuid = Guid.NewGuid();
                            }

                            meshNode.mesh.Id = newGuid;
                            AssetLookup.RegisterFile(newGuid, relPath);
                        }

                        DiskUtilities.SaveMesh(VirtualFileSystem.ResolvePath(relPath), meshNode.mesh);
                    }

                    DebugLogPanel.Log($"Loaded Model ({(DateTime.Now - time).Seconds} Seconds)", DebugLogPanel.DebugMessageSeverity.Information, "Model Importer");

                    editor.EditorCurrentScene.addGameObject(ImporterUtils.ConvertToGameObjectTree(modelGraph));

                    // TODO: Save meshes and materials to asset DB here

                    showImportWindow = false;
                }

                if (ImGui.Button("Cancel", new Vector2(-1, 40)))
                {
                    DebugLogPanel.Log("Cancelled Import", DebugLogPanel.DebugMessageSeverity.Information, "Model Importer");
                    showImportWindow = false;
                }

                ImGui.EndPopup();
            }

            ImGui.PopStyleVar();
        }

        void HandleViewportResize()
        {
            if (viewportHeight != prevViewportHeight || viewportWidth != prevViewportWidth)
            {
                prevViewportHeight = viewportHeight;
                prevViewportWidth = viewportWidth;

                viewportTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                viewportTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            }
        }

        void RenderToViewport()
        {
            var src = (0, 0, selectedResolution.Width, selectedResolution.Height);
            var dst = (scaledViewport.Left, scaledViewport.Top, scaledViewport.Right, scaledViewport.Bottom);

            if (!freeAspectRatio)
                ClearViewport();

            Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, src, dst,
                OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit,
                OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);
        }

        void DrawToolbar()
        {
            // Extracted: Same as your `DrawViewportTools` but modularized — we can leave as is or optionally break down more.
            ViewportUtil.DrawViewportTools(ToolbarHeight, controls);
        }


        public override void OnKeyDown(KeyboardEvent key)
        {
            if (isMouseOverWindow)
                Input.OnKeyDown(key.Key);
        }

        public override void OnKeyUp(KeyboardEvent key)
        {
            Input.OnKeyUp(key.Key);
        }

        public override void OnMouseMove(MouseMoveEvent move)
        {

            if (freeAspectRatio)
            {
                move.position.X = (int)MapValue(move.position.X - viewportPos.X, 0, editor.app.window.Size.X, 0, viewportWidth);
                move.position.Y = (int)MapValue(move.position.Y - (viewportPos.Y + 46), 0, editor.app.window.Size.Y, 0, viewportHeight);
            }
            else
            {
                move.position.X = (int)MapValue(move.position.X, viewportPos.X + scaledViewport.Left, viewportPos.X + scaledViewport.Right, 0, selectedResolution.Width);
                move.position.Y = (int)MapValue(move.position.Y, viewportPos.Y + scaledViewport.Top, viewportPos.Y + scaledViewport.Bottom, selectedResolution.Height, 0);
            }

            Input.OnMouseMove(move);
        }

        public override void OnMouseWheel(MouseWheelEvent wheel)
        {
            if (isMouseOverWindow)
                Input.OnMouseWheel(wheel);
        }

        public override void OnMouseButton(MouseButtonEvent button) { }

        public override void OnRender()
        {
            if (freeAspectRatio && (viewportWidth != prevViewportWidth || viewportHeight != prevViewportHeight))
            {
                ApplyResolution(viewportWidth, viewportHeight);
                editor.EditorCurrentScene.OnResize(viewportWidth, viewportHeight);
            }
        }

        void ApplyResolution(int width, int height)
        {
            Screen.Size.X = width;
            Screen.Size.Y = height;
            Renderer.Resize(width, height);
        }

        public void ClearViewport()
        {
            viewportBuffer.Bind();
            GraphicsState.ClearColor(0, 0, 0, 0);
            GraphicsState.Clear();
            Framebuffer.Unbind();
        }

        public (int, int, int, int) CalculateScaledViewport(int viewportW, int viewportH, int targetW, int targetH)
        {
            int usableHeight = viewportH;
            float scale = Math.Min((float)viewportW / targetW, (float)usableHeight / targetH);
            int sw = (int)(targetW * scale);
            int sh = (int)(targetH * scale);

            int left = (viewportW - sw) / 2;
            int top = (usableHeight - sh) / 2;
            return (left, top, left + sw, top + sh);
        }

        static float MapValue(float val, float inMin, float inMax, float outMin, float outMax)
        {
            return ((val - inMin) * (outMax - outMin)) / (inMax - inMin) + outMin;
        }
    }
}
