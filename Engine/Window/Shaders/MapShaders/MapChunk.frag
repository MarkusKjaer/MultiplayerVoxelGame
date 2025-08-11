#version 330 core

out vec4 pixelColor;

in vec2 vTexCoord;
in vec3 FragPos;  
in vec3 Normal;

flat in int vLayer; 

uniform mediump sampler2DArray ourTextureArray;
uniform vec3 lightPos;  

uniform vec3 lightColor;   // Color/intensity of the light
uniform vec3 ambient;      // Ambient light component
uniform vec3 objectColor;  // Base color (multiplies texture)

void main() 
{
    // Normalize normal and light direction
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    // Diffuse shading
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // Combine lighting
    vec3 lighting = ambient + diffuse;

    // Sample texture and apply object color
    vec3 texColor = texture(ourTextureArray, vec3(vTexCoord, vLayer)).rgb * objectColor;

    // Final color: texture * lighting
    vec3 result = lighting * texColor;

    pixelColor = vec4(result, 1.0);
}
