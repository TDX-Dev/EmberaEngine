using EmberaEngine.Engine.Attributes;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public class FontIconEntry
    {
        public Color4 iconColor;
        public string icon;

        public override string ToString() => icon;
    }

    public static class IconRegistry
    {
        private static readonly Dictionary<Type, List<FontIconEntry>> _fontIcons = new();
        private static readonly Dictionary<Type, Texture> _textureIcons = new();
        private static Texture _defaultTextureIcon;

        private static readonly FontIconEntry _defaultFontIcon = new()
        {
            iconColor = Color4.AliceBlue,
            icon = BootstrapIconFont.Bug
        };

        static IconRegistry()
        {
            // GameObject
            Register<GameObject>(new FontIconEntry { iconColor = Color4.AliceBlue, icon = BootstrapIconFont.Box });
            Register<GameObject>(new FontIconEntry { iconColor = Color4.AliceBlue, icon = BootstrapIconFont.Boxes });

            // --- Transform/Layout Components ---
            Register<Transform>(Category(ComponentCategory.Transform, BootstrapIconFont.Intersect));
            Register<RectTransform>(Category(ComponentCategory.Transform, BootstrapIconFont.BoxArrowRight));
            Register<AnchorComponent>(Category(ComponentCategory.Transform, BootstrapIconFont.BoundingBox));

            // --- Rendering ---
            Register<MeshRenderer>(Category(ComponentCategory.Rendering, BootstrapIconFont.BoxFill));
            Register<LightComponent>(new FontIconEntry { iconColor = Color4.Gold, icon = BootstrapIconFont.Lightbulb });
            Register<WorldEnvironment>(Category(ComponentCategory.Environment, BootstrapIconFont.Sliders));

            // --- Physics ---
            Register<ColliderComponent3D>(Category(ComponentCategory.Physics, BootstrapIconFont.App));
            Register<RigidBody3D>(Category(ComponentCategory.Physics, BootstrapIconFont.Back));

            // --- Animation ---
            Register<Animator2D>(Category(ComponentCategory.Animation, BootstrapIconFont.PersonArmsUp));

            // --- Camera ---
            Register<CameraComponent3D>(Category(ComponentCategory.Camera, BootstrapIconFont.CameraVideo));
        }

        private static FontIconEntry Category(ComponentCategory category, string iconOverride = null)
        {
            var baseEntry = ComponentCategoryRegistry.CategoryInfos[category];
            return new FontIconEntry
            {
                iconColor = baseEntry.IconColor,
                icon = iconOverride ?? baseEntry.DefaultIcon
            };
        }

        private static void Register<T>(FontIconEntry icon)
        {
            var type = typeof(T);
            if (!_fontIcons.TryGetValue(type, out var list))
                _fontIcons[type] = list = new List<FontIconEntry>();

            list.Add(icon);
        }

        private static void Register<T>(Texture icon)
        {
            _textureIcons[typeof(T)] = icon;
        }

        public static Texture GetTextureIcon(Type type)
        {
            return _textureIcons.TryGetValue(type, out var tex) ? tex : _defaultTextureIcon;
        }

        public static FontIconEntry GetFontIcon(Type type, int state = 0)
        {
            if (_fontIcons.TryGetValue(type, out var icons) && state < icons.Count)
                return icons[state];

            var attr = type.GetCustomAttribute<ComponentCategoryAttribute>();
            if (attr != null)
            {
                return new FontIconEntry
                {
                    iconColor = attr.GetColor(),
                    icon = BootstrapIconFont.QuestionCircle // fallback generic icon
                };
            }

            return _defaultFontIcon;
        }

    }
}
