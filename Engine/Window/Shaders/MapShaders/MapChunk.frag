#version 330 core

out vec4 pixelColor;

in vec2 vTexCoord;
flat in int vLayer; 

uniform mediump sampler2DArray ourTextureArray;

void main() 
{
    pixelColor = texture(ourTextureArray, vec3(vTexCoord, vLayer));
}
