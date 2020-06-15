#version 450
#extension GL_ARB_seperate_shader_objects : enable
#extension GL_KHR_vulkan_glsl : enable

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec2 texcoord;

layout(set = 2, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 2, binding = 1) uniform sampler SurfaceSampler;

layout(location = 0) out vec4 outColor;

void main() 
{
    outColor =  texture(sampler2D(SurfaceTexture, SurfaceSampler), texcoord) * fragColor;
}