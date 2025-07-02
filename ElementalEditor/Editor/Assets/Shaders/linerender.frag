#version 440 core

layout(location = 0) out vec4 color;

uniform vec3 C_VIEWPOS;
uniform vec4 LINE_COLOR;

void main()
{
	color = LINE_COLOR;;
}