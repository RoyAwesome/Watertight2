#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_KHR_vulkan_glsl : enable


layout(set = 0, binding = 0) uniform ProjectionView
{
	mat4 projview;
};

layout(set = 1, binding = 0) uniform ModelBuffer
{
	mat4 model;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 outTexCoord;

void main() {
	gl_Position = projview * model * vec4(Position, 1.0);
	fragColor = Color;
	outTexCoord = TexCoord;
}