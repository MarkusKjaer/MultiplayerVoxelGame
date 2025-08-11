#version 330 core
                
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in float aLayer;
                
out vec2 vTexCoord;
out vec3 FragPos; 
out vec3 Normal;
flat out int vLayer;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() 
{
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection;
    FragPos = vec3(model * vec4(aPosition, 1.0));
    Normal = aNormal; // Pass normal to fragment shader

    vTexCoord = aTexCoord;

    vLayer = int(aLayer); // Convert float to int
}

