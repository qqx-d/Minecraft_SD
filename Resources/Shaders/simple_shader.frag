#version 330 core

in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;

uniform sampler2D texture0;
uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform vec3 cameraPos;

out vec4 outputColor;

const vec3 fogColor = vec3(0.6, 0.7, 0.9);
const float fogStart = 45.0;
const float fogEnd   = 150.0;

void main()
{
    vec4 tex = texture(texture0, TexCoords);
    if(tex.a < 0.1)
        discard;

    vec3 norm = normalize(Normal);
    float diff = max(dot(norm, -lightDir), 0.0);

    vec3 color = tex.rgb * (ambientColor + diff * lightColor);

    float dist = length(cameraPos - FragPos);
    float fogFactor = clamp((fogEnd - dist) / (fogEnd - fogStart), 0.0, 1.0);
    color = mix(fogColor, color, fogFactor);

    outputColor = vec4(color, tex.a);
}