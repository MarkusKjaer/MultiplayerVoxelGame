#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec3 FragPos;
out vec3 Normal;
out vec2 vTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vec3 offsetPosition = aPosition + vec3(0.0, -0.1, 0.0); 
    vec4 worldPos = model * vec4(offsetPosition, 1.0);
    FragPos = worldPos.xyz;
    Normal = mat3(transpose(inverse(model))) * aNormal;
    vTexCoord = aTexCoord;
    gl_Position = vec4(offsetPosition, 1.0f) * model * view * projection;
}