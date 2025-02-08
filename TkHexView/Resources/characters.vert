#version 330

in vec4 position;
in uint flags;
in float width;
in float texture_opacity;
in vec2 texture_offset;
in vec2 texture_size;
in vec3 foreground;
in vec3 background;
in vec4 border_color;
in vec4 selected_color;

out vertex_data
{
	flat uint flags;
	float width;
	float texture_opacity;
	vec2 texture_offset;
	vec2 texture_size;
	vec3 foreground;
	vec3 background;
	vec4 border_color;
	vec4 selected_color;
} vertex;

void main()
{
	vertex.flags = flags;
	vertex.width = width;
	vertex.texture_opacity = texture_opacity;
	vertex.texture_offset = texture_offset;
	vertex.texture_size = texture_size;
	vertex.foreground = foreground;
	vertex.background = background;
	vertex.border_color = border_color;
	vertex.selected_color = selected_color;
	gl_Position = position;
}