#version 460 core

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;

layout(location = 1) out vec3 nearPoint;
layout(location = 2) out vec3 farPoint;

// Grid positions in clip space
vec3 gridPlane[6] = vec3[] (
    vec3(1, 1, 0), vec3(-1, -1, 0), vec3(-1, 1, 0),
    vec3(-1, -1, 0), vec3(1, 1, 0), vec3(1, -1, 0)
);

vec3 UnprojectPoint(float x, float y, float z, mat4 invView, mat4 invProj) {
    vec4 clip = vec4(x, y, z, 1.0);
    vec4 viewSpace = invProj * clip;
    viewSpace /= viewSpace.w;
    vec4 worldSpace = invView * viewSpace;
    return worldSpace.xyz;
}

void main() {
    vec3 p = gridPlane[gl_VertexID];

    // Compute inverse matrices once
    mat4 invProj = inverse(W_PROJECTION_MATRIX);
    mat4 invView = inverse(W_VIEW_MATRIX);

    // Unproject to world space at near and far plane
    nearPoint = UnprojectPoint(p.x, p.y, 0.0, invView, invProj);
    farPoint  = UnprojectPoint(p.x, p.y, 1.0, invView, invProj);

    // Clip space coordinates directly for full-screen quad/grid
    gl_Position = vec4(p, 1.0);
}
