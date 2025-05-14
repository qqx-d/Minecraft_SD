#version 330 core
in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

uniform sampler2D texture0;
uniform vec3 cameraPos;

const float minAlpha = 0.2;
const float maxAlpha = 0.8;
const float fadeStart = 10.0;
const float fadeEnd   = 50.0;

const float waterSurfaceY = 4.0;
const float maxDepth = 3;

void main()
{
    vec4 texColor = texture(texture0, TexCoord);
    if (texColor.a < 0.1)
    discard;

    float dist = length(cameraPos - FragPos);
    float t = clamp((dist - fadeStart) / (fadeEnd - fadeStart), 0.0, 1.0);
    float alpha = mix(maxAlpha, minAlpha, t);

    vec3 viewDir = normalize(cameraPos - FragPos);
    float angleFactor = 1.0 - abs(dot(viewDir, normalize(Normal)));

    float depth = clamp(waterSurfaceY - FragPos.y, 0.0, maxDepth);
    float depthFactor = depth / maxDepth;

    vec3 deepColor = vec3(0.0, 0.1, 0.3);
    vec3 finalColor = mix(texColor.rgb, deepColor, angleFactor * depthFactor);

    FragColor = vec4(finalColor, texColor.a * alpha);
}
