#version 330 core

out vec4 pixelColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 vTexCoord;

uniform sampler2D waterTexture;

uniform vec3 lightPos;
uniform vec3 viewPos;

uniform vec3 lightColor;
uniform vec3 ambient;
uniform vec3 waterColor;
uniform float time;

float hash(vec2 p) {
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f); 

    return mix(
        mix(hash(i + vec2(0,0)), hash(i + vec2(1,0)), u.x),
        mix(hash(i + vec2(0,1)), hash(i + vec2(1,1)), u.x),
        u.y
    );
}

void main()
{
    float waveStrength = 0.1;
    float waveSpeed = 2.0;

    vec2 pos = FragPos.xz;

    float n = noise(pos * 0.8 + time * 0.4)        * 1.0
            + noise(pos * 1.9 - time * 0.3)        * 0.5
            + noise(pos * 3.7 + time * 0.6)        * 0.25
            + noise(pos * 7.1 - time * 0.2)        * 0.125;

    n = n / 1.875; 

    float wave = (n - 0.5) * 2.0;

    vec3 distortedNormal = Normal;
    distortedNormal.x += wave * waveStrength;
    distortedNormal.z += wave * waveStrength * 0.8; 

    vec3 norm = normalize(distortedNormal);

    // Lighting
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // Specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);

    float shininess = 32.0;  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
    vec3 specular = spec * lightColor * 0.6;

    // Texture + Tint
    vec3 texColor = texture(waterTexture, FragPos.xz * 0.25).rgb;
    vec3 baseColor = mix(waterColor, texColor, 0.5);

    vec3 result = (ambient + diffuse) * baseColor + specular;

    float alpha = 0.6;

    pixelColor = vec4(result, alpha);
}