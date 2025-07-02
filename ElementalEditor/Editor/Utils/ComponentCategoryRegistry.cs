using EmberaEngine.Engine.Attributes;
using EmberaEngine.Engine.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public enum ComponentCategory
    {
        Transform,
        Rendering,
        Physics,
        UI,
        Animation,
        Camera,
        Environment,
        Other
    }


    public class CategoryInfo
    {
        public Color4 IconColor { get; set; }
        public string DisplayName { get; set; }
        public string? DefaultIcon { get; }  // optional

        public CategoryInfo(Color4 color, string label, string? defaultIcon = null)
        {
            IconColor = color;
            DisplayName = label;
            DefaultIcon = defaultIcon;
        }
    }


    public static class ComponentCategoryRegistry
    {
        public static readonly Dictionary<ComponentCategory, CategoryInfo> CategoryInfos = new()
        {
            [ComponentCategory.Transform] = new(Color4.ForestGreen, "Transform/Layout"),
            [ComponentCategory.Rendering] = new(Color4.DodgerBlue, "Rendering"),
            [ComponentCategory.Physics] = new(Color4.OrangeRed, "Physics"),
            [ComponentCategory.UI] = new(Color4.MediumSeaGreen, "UI"),
            [ComponentCategory.Animation] = new(Color4.MediumPurple, "Animation"),
            [ComponentCategory.Camera] = new(Color4.CadetBlue, "Camera"),
            [ComponentCategory.Environment] = new(Color4.SkyBlue, "Environment"),
            [ComponentCategory.Other] = new(Color4.Gray, "Other")
        };

        private static readonly Dictionary<Type, ComponentCategory> _typeToCategory = new()
        {
            [typeof(Transform)] = ComponentCategory.Transform,
            [typeof(RectTransform)] = ComponentCategory.Transform,
            [typeof(AnchorComponent)] = ComponentCategory.Transform,

            [typeof(MeshRenderer)] = ComponentCategory.Rendering,
            [typeof(LightComponent)] = ComponentCategory.Rendering,

            [typeof(ColliderComponent3D)] = ComponentCategory.Physics,
            [typeof(RigidBody3D)] = ComponentCategory.Physics,

            [typeof(Animator2D)] = ComponentCategory.Animation,

            [typeof(CameraComponent3D)] = ComponentCategory.Camera,

            [typeof(WorldEnvironment)] = ComponentCategory.Environment
        };

        public static ComponentCategory GetCategoryFor(Type type)
        {
            if (_typeToCategory.TryGetValue(type, out var category))
                return category;

            var attr = type.GetCustomAttribute<ComponentCategoryAttribute>();
            if (attr != null)
            {
                // Dynamically assign to `Other`, and store info in CategoryInfos if not already present.
                if (!CategoryInfos.ContainsKey(ComponentCategory.Other))
                {
                    CategoryInfos[ComponentCategory.Other] = new CategoryInfo(attr.GetColor(), attr.CategoryName);
                }
                return ComponentCategory.Other;
            }

            return ComponentCategory.Other;
        }


        public static CategoryInfo GetCategoryInfo(Type type)
        {
            if (_typeToCategory.TryGetValue(type, out var category))
                return CategoryInfos[category];

            var attr = type.GetCustomAttribute<ComponentCategoryAttribute>();
            if (attr != null)
            {
                return new CategoryInfo(attr.GetColor(), attr.CategoryName);
            }

            return CategoryInfos[ComponentCategory.Other];
        }

    }

}
