#version 330 core

in vec2 TexCoords;
in vec3 Normal;

uniform sampler2D texture0;
uniform vec3 lightDir;
uniform vec3 lightColor;
uniform vec3 ambientColor;

out vec4 outputColor;

void main()
{
    vec4 tex = texture(texture0, TexCoords);
    
    if (tex.a < 0.1)
    discard;

    vec3 norm = normalize(Normal);
    float diff = max(dot(norm, -lightDir), 0.0);

    vec3 finalColor = tex.rgb * (ambientColor + diff * lightColor);

    outputColor = vec4(finalColor, tex.a);
}
