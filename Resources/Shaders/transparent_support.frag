#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

uniform sampler2D texture0;
uniform vec3 cameraPos;

const float minAlpha = 0.5;
const float maxAlpha = 0.9;
const float fadeStart = 30.0;
const float fadeEnd   = 80.0;

const float waterSurfaceY = 4.0;
const float maxDepth = 3.0;

const vec3 fogColor = vec3(0.6, 0.7, 0.9);
const float fogStart = 45.0;
const float fogEnd   = 150.0;

void main()
{
    vec4 texColor = texture(texture0, TexCoord);
    if (texColor.a < 0.1)
        discard;
    
    float dist = length(cameraPos - FragPos);
    
    float fadeFactor = clamp((dist - fadeStart) / (fadeEnd - fadeStart), 0.0, 1.0);
    float alpha = mix(maxAlpha, minAlpha, fadeFactor);
    
    vec3 viewDir = normalize(cameraPos - FragPos);
    float angleFactor = 1.0 - abs(dot(viewDir, normalize(Normal)));

    float visualOffset = 0.05;
    vec3 adjustedFragPos = FragPos;
    adjustedFragPos.y -= visualOffset;

    float depth = clamp(waterSurfaceY - adjustedFragPos.y, 0.0, maxDepth);
    float depthFactor = depth / maxDepth;

    vec3 deepColor = vec3(0.0, 0.1, 0.3);
    vec3 baseColor = mix(texColor.rgb, deepColor, angleFactor * depthFactor);
    
    float fogFactor = clamp((fogEnd - dist) / (fogEnd - fogStart), 0.0, 1.0);
    vec3 finalColor = mix(fogColor, baseColor, fogFactor);

    FragColor = vec4(finalColor, texColor.a * alpha);
}