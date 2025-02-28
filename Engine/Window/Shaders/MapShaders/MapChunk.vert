#version 330 core
                
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayer;
                
out vec2 vTexCoord;
flat out int vLayer;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() 
{
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection;

    vTexCoord = aTexCoord;

    vLayer = int(aLayer); // Convert float to int
}

