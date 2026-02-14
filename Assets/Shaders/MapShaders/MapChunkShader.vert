#version 330 core
                
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in float aLayer;
layout (location = 4) in float aAO;
                
out vec2 vTexCoord;
out vec3 FragPos; 
out vec3 Normal;
out float vAO;
flat out int vLayer;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() 
{
    gl_Position = vec4(aPosition, 1.0f) * model * view * projection;
    FragPos = vec3(model * vec4(aPosition, 1.0));
    Normal = mat3(transpose(inverse(model))) * aNormal;

    vTexCoord = aTexCoord;

    vLayer = int(aLayer); // Convert float to int

    vAO = aAO;
}

