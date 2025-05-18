#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 TexCoord;
out vec3 Normal;
out vec3 FragPos;

const vec3 WaterOffset = vec3(0, 0.15, 0);

void main()
{
    vec3 adjustedPos = aPos - WaterOffset;

    vec4 worldPos = vec4(adjustedPos, 1.0) * model;
    FragPos = worldPos.xyz;

    vec4 viewPos = worldPos * view;
    gl_Position = viewPos * projection;

    TexCoord = aTexCoord;
    Normal = mat3(transpose(inverse(model))) * aNormal;
}

    