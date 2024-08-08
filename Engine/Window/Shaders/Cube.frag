#version 330 core

out vec4 pixelColor;

in vec2 vTexCoord;

uniform sampler2D ourTexture;

void main() 
{
    pixelColor = texture(ourTexture, vTexCoord);
}
