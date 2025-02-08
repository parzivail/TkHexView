#version 330

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

uniform mat4 mvp;
uniform vec2 cell_size;

in vertex_data
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
} vertex[];

out frag_data
{
	flat uint flags;
	float texture_opacity;
    vec2 tex;
    vec2 cell_space;
    vec3 foreground;
    vec3 background;
	vec4 border_color;
	vec4 selected_color;
} frag;

void emit(float x, float y)
{
	vec2 cell_space = vec2(x, y);
	
	frag.flags = vertex[0].flags;
	frag.texture_opacity = vertex[0].texture_opacity;
	frag.tex = vertex[0].texture_offset + cell_space * vertex[0].texture_size;
	frag.foreground = vertex[0].foreground;
	frag.background = vertex[0].background;
	frag.border_color = vertex[0].border_color;
	frag.selected_color = vertex[0].selected_color;
	
	frag.cell_space = cell_space;
	
	gl_Position = mvp * (gl_in[0].gl_Position + vec4(x * cell_size.x * vertex[0].width, y * cell_size.y, 0.0, 0.0));
	
	EmitVertex();
}

void main()
{
	emit(0.0, 0.0);
	emit(0.0, 1.0);
	emit(1.0, 0.0);
	emit(1.0, 1.0);
	
	EndPrimitive();
}