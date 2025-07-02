#version 440 core

float near = 0.01;
float far = 10000.0;

layout(location = 0) out vec4 outColor;
layout(location = 1) in vec3 nearPoint;
layout(location = 2) in vec3 farPoint;

uniform mat4 W_VIEW_MATRIX;
uniform mat4 W_PROJECTION_MATRIX;
uniform vec3 C_VIEWPOS;


vec4 grid(vec3 fragPos3D, float scale, bool drawAxis) {
    vec2 coord = fragPos3D.xz * scale;
    vec2 gridUV = abs(fract(coord) - 0.5);

    float thickness = 0.02;

    // Use screen-space derivatives for anti-aliasing
    float aax = fwidth(gridUV.x);
    float aay = fwidth(gridUV.y);

    float lineX = smoothstep(thickness + aax, thickness - aax, gridUV.x);
    float lineY = smoothstep(thickness + aay, thickness - aay, gridUV.y);

    float line = max(lineX, lineY); // OR use: line = max(lineX, lineY);

    vec3 gridColor = vec3(0.2);
    float alpha = line;

    // Highlight axes
    if (drawAxis && abs(fragPos3D.x) < 0.01) return vec4(25, 0, 0, 1);
    if (drawAxis && abs(fragPos3D.z) < 0.01) return vec4(0, 0, 25, 1);

    return vec4(gridColor, alpha);
}




float computeDepth(vec3 pos) {
    vec4 clip = W_PROJECTION_MATRIX * W_VIEW_MATRIX * vec4(pos, 1.0);
    return (clip.z / clip.w) * 0.5 + 0.5; // convert from NDC [-1,1] to [0,1]
}

float computeLinearDepth(vec3 pos) {
    vec4 clip = W_PROJECTION_MATRIX * W_VIEW_MATRIX * vec4(pos, 1.0);
    float z = (clip.z / clip.w) * 2.0 - 1.0; // NDC depth
    float linear = (2.0 * near * far) / (far + near - z * (far - near));
    return linear / far; // Normalize [0,1]
}

void main() {
    float t = -nearPoint.y / (farPoint.y - nearPoint.y); // intersection with Y=0 plane
    if (t < 0.0 || t > 1.0) discard;

    vec3 fragPos3D = mix(nearPoint, farPoint, t);

    gl_FragDepth = computeDepth(fragPos3D);

    // --- Fade after certain radius from camera ---
    float dist = distance(fragPos3D, C_VIEWPOS);

    // Control these values
    float fadeStart = 50.0;
    float fadeEnd   = 100.0;

    float fading = 1.0 - smoothstep(fadeStart, fadeEnd, dist); // 1 to 0

    vec4 fineGrid = grid(fragPos3D, 10.0, true);
    vec4 coarseGrid = grid(fragPos3D, 1.0, true);
    
    outColor = fineGrid + coarseGrid;
    outColor.a *= fading;

    // Optional discard if fully faded
    if (outColor.a <= 0.01) discard;
}
