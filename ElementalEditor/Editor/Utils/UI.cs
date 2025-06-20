﻿using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ImGuiNET;
using System.Reflection;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic;
using EmberaEngine.Engine.Core;
using System.Resources;

namespace ElementalEditor.Editor.Utils
{
    public static class UI
    {
        public static bool Button(string label, Vector2 size)
        {
            return ImGui.Button(label, new System.Numerics.Vector2(size.X, size.Y));
        }

        static bool firstProperty_, propertyCurr_, firstField_;
        static int propertyCount;
        static string propertyLabel;
        static uint warnColDefault = ToUIntA(new Vector4(0.5f, 0.5f, 0.7f, 0.5f));
        static bool hasBegun = false;

        public static void BeginPropertyGrid(string id)
        {
            propertyCount = 0;
            firstProperty_ = true;
            hasBegun = ImGui.BeginTable("##" + id, 2);

            if (!hasBegun) return;

            ImGui.TableSetupColumn("Prop", 0);
            ImGui.TableSetupColumn("Val", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();
        }

        public static void EndPropertyGrid()
        {
            if (!hasBegun) return;
            ImGui.EndTable();
            hasBegun = false;
        }

        public static void BeginProperty(string fieldname)
        {
            if (!hasBegun) return;
            propertyCount += 1;
            propertyCurr_ = true;
            firstField_ = true;
            propertyLabel = fieldname;

            if (propertyCount > 1)
            {
                ImGui.TableNextRow();
            }

            ImGui.TableSetColumnIndex(0);

            ImGui.Separator();

            ImGui.TextWrapped(fieldname);



        }

        public static void EndProperty()
        {
            if (!hasBegun) return;
            propertyCurr_ = false;
            firstField_ = true;
        }

        public static void NextField()
        {
            if (!hasBegun) return;
            if (firstField_)
            {
                ImGui.TableSetColumnIndex(1);

                ImGui.Separator();
                firstField_ = false;
                return;
            }
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);

            ImGui.Separator();

        }

        public static bool PropertyInt(ref int value, int min = int.MinValue, int max = int.MaxValue, int step = 1, int stepfast = 2)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            return ImGui.InputInt("##" + propertyCount + propertyLabel, ref value, step, stepfast);
        }

        public static bool PropertyVector2(ref Vector2 value, float speed = 0.2f, float min = float.MinValue, float max = float.MaxValue)
        {
            NextField();

            System.Numerics.Vector2 vec2 = new System.Numerics.Vector2(value.X, value.Y);

            ImGui.SetNextItemWidth(-1);
            bool edited = ImGui.DragFloat2("##" + propertyCount + propertyLabel, ref vec2, speed, min, max);

            value = new Vector2(vec2.X, vec2.Y);
            return edited;
        }

        public static bool PropertyVector3(ref Vector3 value, float speed = 0.2f, float min = float.MinValue, float max = float.MaxValue)
        {
            NextField();

            System.Numerics.Vector3 vec3 = new System.Numerics.Vector3(value.X, value.Y, value.Z);

            ImGui.SetNextItemWidth(-1);
            bool edited = ImGui.DragFloat3("##" + propertyCount + propertyLabel, ref vec3, speed, min, max);

            value = new Vector3(vec3.X, vec3.Y, vec3.Z);

            return edited;
        }

        public static bool PropertyVector4(ref Vector4 value, float speed = 0.2f, float min = float.MinValue, float max = float.MaxValue)
        {
            NextField();

            System.Numerics.Vector4 vec4 = new System.Numerics.Vector4(value.X, value.Y, value.Z, value.W);

            ImGui.SetNextItemWidth(-1);
            bool edited = ImGui.DragFloat4("##" + propertyCount + propertyLabel, ref vec4, speed, min, max);

            value = new Vector4(vec4.X, vec4.Y, vec4.Z, vec4.W);
            return edited;
        }

        public static bool PropertyString(ref string value, bool multiline = true)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            bool edited;
            if (multiline)
                edited = ImGui.InputTextMultiline("##" + propertyCount + propertyLabel, ref value, 32000, new System.Numerics.Vector2(-1, 100));
            else
                edited = ImGui.InputText("##" + propertyCount + propertyLabel, ref value, 32000);
            return edited;
        }

        public static bool PropertyColor4(ref Color4 value, bool isFloat = false)
        {
            NextField();

            System.Numerics.Vector4 color4 = new System.Numerics.Vector4(value.R, value.G, value.B, value.A);
            ImGui.SetNextItemWidth(-1);
            bool edited = ImGui.ColorEdit4("##" + propertyCount + propertyLabel, ref color4, isFloat ? ImGuiColorEditFlags.Float : ImGuiColorEditFlags.None);
            value = new Color4(color4.X, color4.Y, color4.Z, color4.W);
            return edited;
        }

        public static bool PropertyFloat(ref float value, float min = float.MinValue, float max = float.MaxValue, float speed = 0.2f)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            return ImGui.DragFloat("##" + propertyCount + propertyLabel, ref value, speed, min, max);
        }

        public static bool PropertyEnum(ref int currentItem, string[] items, int item_count)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            return ImGui.Combo("##" + propertyCount + propertyLabel, ref currentItem, items, item_count);
        }

        public static bool PropertyBool(ref bool value)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            return ImGui.Checkbox("##" + propertyCount + propertyLabel, ref value);
        }

        public static void PropertyTexture(IntPtr value)
        {
            NextField();
            ImGui.Image(value, new System.Numerics.Vector2(ImGui.GetColumnWidth() * 0.2f, ImGui.GetColumnWidth() * 0.2f));
        }

        public static void PropertyText(string value)
        {
            NextField();

            ImGui.SetNextItemWidth(-1);
            ImGui.Text(value);
        }

        public static void PropertyType(Type fieldType)
        {
            NextField();

            ImGui.SetNextItemWidth(-1);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0f, 0f));

            System.Numerics.Vector2 cursorPos = ImGui.GetCursorPos();

            ImGui.BeginDisabled();

            ImGui.Button("", new System.Numerics.Vector2(-1, 32));

            ImGui.EndDisabled();

            ImGui.SetCursorPos(cursorPos);

            ImGui.Button(fieldType.Name, new System.Numerics.Vector2(-1, 30));
            ImGui.PopStyleVar();
        }

        public static float DrawComponentProperty(PropertyInfo[] propertyInfo, object component)
        {
            float sizeY = 0;
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
            for (int i = 0; i < propertyInfo.Length; i++)
            {

                PropertyInfo property = propertyInfo[i];
                if (property.DeclaringType == typeof(Component) || !property.CanWrite) { continue; }

                BeginProperty(property.Name);

                if (property.PropertyType == typeof(int))
                {
                    DrawIntProperty(property, component);
                }
                else if (property.PropertyType == typeof(float))
                {
                    DrawFloatProperty(property, component);
                }
                else if (property.PropertyType == typeof(string))
                {
                    DrawStringProperty(property, component);
                }
                else if (property.PropertyType == typeof(Vector2))
                {

                    DrawVec2Property(property, component);
                }
                else if (property.PropertyType == typeof(Vector3))
                {

                    DrawVec3Property(property, component);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    DrawBoolProperty(property, component);
                }
                else if (property.PropertyType == typeof(Color4))
                {
                    DrawColor4Property(property, component);
                }
                else if (property.PropertyType.IsEnum)
                {
                    DrawEnumProperty(property, component);
                }
                else if (property.PropertyType == typeof(Texture))
                {
                    DrawTextureProperty(property, component);
                }
                else if (property.PropertyType == typeof(List<>))
                {
                    DrawListProperty(property, component);
                }
                else
                {
                    DrawTypeProperty(property, component);
                }


                sizeY += ImGui.GetItemRectSize().Y;

                EndProperty();
            }

            ImGui.PopStyleVar();
            return sizeY;
        }


        public static float DrawComponentField(FieldInfo[] fieldInfo, object component)
        {
            float sizeY = 0;
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
            for (int i = 0; i < fieldInfo.Length; i++)
            {

                FieldInfo field = fieldInfo[i];
                if (field.IsPrivate || field.DeclaringType == typeof(Component) || field.Attributes == FieldAttributes.NotSerialized) { continue; }

                BeginProperty(field.Name);

                if (field.FieldType == typeof(int))
                {
                    DrawIntField(field, component);
                }
                else if (field.FieldType == typeof(float))
                {

                    DrawFloatField(field, component);
                }
                else if (field.FieldType == typeof(string))
                {
                    DrawStringField(field, component);
                }
                else if (field.FieldType == typeof(Vector2))
                {

                    DrawVec2Field(field, component);
                }
                else if (field.FieldType == typeof(Vector3))
                {

                    DrawVec3Field(field, component);
                }
                else if (field.FieldType == typeof(bool))
                {
                    DrawBoolField(field, component);
                }
                else if (field.FieldType == typeof(Color4))
                {
                    DrawColor4Field(field, component);
                }
                else if (field.FieldType.IsEnum)
                {
                    DrawEnumField(field, component);
                }
                else if (field.FieldType == typeof(Texture))
                {
                    //DrawTextureField(field, component);
                }
                else if (field.FieldType == typeof(List<>))
                {
                    DrawListField(field, component);
                }
                else
                {
                    DrawTypeField(field, component);
                }

                sizeY += ImGui.GetItemRectSize().Y;

                EndProperty();
            }

            ImGui.PopStyleVar();
            return sizeY;
        }

        public static void DrawIntField(FieldInfo field, object component)
        {
            int val = (int)field.GetValue(component);
            if (PropertyInt(ref val));
                field.SetValue(component, val);
        }

        public static void DrawFloatField(FieldInfo field, object component)
        {
            float val = (float)field.GetValue(component);
            PropertyFloat(ref val);
            field.SetValue(component, val);
        }

        //public static void DrawTextureField(FieldInfo field, object component)
        //{
        //    Texture val = (Texture)field.GetValue(component);
        //    PropertyTexture((IntPtr)(val == null ? 0 : val.GetRendererID()));
        //    Texture newVal = HandleDropTexture();
        //    val = newVal == null ? val : newVal;
        //    field.SetValue(component, val);
        //}

        public static void DrawStringField(FieldInfo field, object component)
        {
            string val = (string)field.GetValue(component);
            if (PropertyString(ref val))
                field.SetValue(component, val);
        }

        public static void DrawTypeField(FieldInfo field, object component)
        {
            PropertyType(field.FieldType);
        }

        public static void DrawVec2Field(FieldInfo field, object component)
        {
            Vector2 val = (Vector2)field.GetValue(component);
            if (PropertyVector2(ref val))
                field.SetValue(component, val);
        }

        public static void DrawVec3Field(FieldInfo field, object component)
        {
            Vector3 val = (Vector3)field.GetValue(component);
            if (PropertyVector3(ref val))
                field.SetValue(component, val);
        }

        public static void DrawColor4Field(FieldInfo field, object component)
        {
            Color4 val = (Color4)field.GetValue(component);
            if (PropertyColor4(ref val))
                field.SetValue(component, val);
        }
        public static void DrawBoolField(FieldInfo field, object component)
        {
            bool val = (bool)field.GetValue(component);
            if (PropertyBool(ref val))
                field.SetValue(component, val);
        }

        public static void DrawEnumField(FieldInfo field, object component)
        {
            string[] vs = field.GetType().GetEnumNames();

            int index = Array.IndexOf(Enum.GetValues(field.FieldType), field.GetValue(component));

            if (PropertyEnum(ref index, vs.ToArray(), vs.Length))
                field.SetValue(component, field.FieldType.GetEnumValues().GetValue(index));
        }

        public static void DrawListField(FieldInfo field, object component)
        {
            //List<> ListObjects = (List<>)field.GetValue(component);
            //for (int i = 0; i < ListObjects.Count; i++)
            //{

            //}
        }

        public static void DrawIntProperty(PropertyInfo field, object component)
        {
            int val = (int)field.GetValue(component);
            if (PropertyInt(ref val))
                field.SetValue(component, val);
        }

        public static void DrawFloatProperty(PropertyInfo field, object component)
        {
            float val = (float)field.GetValue(component);
            if (PropertyFloat(ref val))
            {
                field.SetValue(component, val);
            }
        }

        public static void DrawTextureProperty(PropertyInfo field, object component)
        {
            Texture val = (Texture)field.GetValue(component);
            PropertyTexture((IntPtr)(val == null ? 0 : val.GetRendererID()));
            //Texture newVal = HandleDropTexture();
            //val = newVal == null ? val : newVal;
            //field.SetValue(component, val);
        }

        public static void DrawStringProperty(PropertyInfo field, object component)
        {
            string val = (string)field.GetValue(component);
            if (PropertyString(ref val))
                field.SetValue(component, val);
        }

        public static void DrawTypeProperty(PropertyInfo field, object component)
        {
            PropertyType(field.PropertyType);
        }

        public static void DrawVec2Property(PropertyInfo field, object component)
        {
            Vector2 val = (Vector2)field.GetValue(component);
            if (PropertyVector2(ref val))
                field.SetValue(component, val);
        }

        public static void DrawVec3Property(PropertyInfo field, object component)
        {
            Vector3 val = (Vector3)field.GetValue(component);
            if (PropertyVector3(ref val))
                field.SetValue(component, val);
        }

        public static void DrawColor4Property(PropertyInfo field, object component)
        {
            Color4 val = (Color4)field.GetValue(component);
            if (PropertyColor4(ref val))
                field.SetValue(component, val);
        }
        public static void DrawBoolProperty(PropertyInfo field, object component)
        {
            bool val = (bool)field.GetValue(component);
            if (PropertyBool(ref val))
                field.SetValue(component, val);
        }

        public static void DrawEnumProperty(PropertyInfo field, object component)
        {
            List<string> vs = new List<string>();
            foreach (var v in field.PropertyType.GetEnumValues())
            {
                vs.Add(v.ToString());
            }

            int index = Array.IndexOf(Enum.GetValues(field.PropertyType), field.GetValue(component));

            if (PropertyEnum(ref index, vs.ToArray(), vs.Count))
                field.SetValue(component, field.PropertyType.GetEnumValues().GetValue(index));
        }

        public static void DrawListProperty(PropertyInfo field, object component)
        {
            //List<> ListObjects = (List<>)field.GetValue(component);
            //for (int i = 0; i < ListObjects.Count; i++)
            //{

            //}
        }

        public static void DrawWarning(string text)
        {
            NextField();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, warnColDefault);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(1, 1, 1, 1));
            ImGui.SetNextItemWidth(-1);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();
        }

        public static bool DrawButtonWithText(string label, string buttonLabel)
        {
            NextField();
            ImGui.TextWrapped(label);
            return ImGui.Button(buttonLabel);
        }

        public static bool DrawButton(string label)
        {
            NextField();
            return ImGui.Button(label);
        }

        public static uint ToUint(Vector4i c)
        {
            return (uint)((c.W << 24) | (c.X << 16) | (c.Y << 8) | (c.Z << 0));
        }

        public static uint ToUIntA(Vector4 color)
        {

            return ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(color.X, color.Y, color.Z, color.W));

        }

        //public static Texture HandleDropTexture()
        //{
        //    if (ImGui.BeginDragDropTarget())
        //    {
        //        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        //        {
        //            DragFileItem item = EditorLayer.DragDropService.GetDragFile();
        //            Texture texture = (Texture)Resources.Load(item.fileName);
        //            return texture;
        //        }
        //        ImGui.EndDragDropTarget();
        //    }
        //    return null;
        //}

    }
}