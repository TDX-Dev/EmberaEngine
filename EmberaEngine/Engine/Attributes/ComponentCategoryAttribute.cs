using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentCategoryAttribute : Attribute
    {
        public string CategoryName { get; }
        public string HexColor { get; }

        public ComponentCategoryAttribute(string categoryName, string hexColor)
        {
            CategoryName = categoryName;
            HexColor = hexColor;
        }

        public Color4 GetColor()
        {
            return ColorConverter.FromHex(HexColor);
        } 
    }

}
