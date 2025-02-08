#version 330

uniform sampler2D texture;

const uint FLAG_SELECTED = 1u << 0;
const uint FLAG_LEFT_BORDER = 1u << 1;
const uint FLAG_TOP_BORDER = 1u << 2;
const uint FLAG_RIGHT_BORDER = 1u << 3;
const uint FLAG_BOTTOM_BORDER = 1u << 4;
const uint FLAG_DASHED_BORDER = 1u << 5;

uniform int border_size;
uniform int dash_size;

in frag_data
{
	flat uint flags;
	float texture_opacity;
    vec2 tex;
    vec2 cell_space;
    vec3 foreground;
    vec3 background;
    vec4 border_color;
    vec4 selected_color;
};

out vec4 color;

float median(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

vec3 sdf_font()
{
    vec4 msd = texture2D(texture, tex);    
    float sd = median(msd.r, msd.g, msd.b);
    float dist = 0.5 - sd;
    vec2 ddist = vec2(dFdx(dist), dFdy(dist));
    float pixelDist = dist / length(ddist);
    float opacity = clamp(0.5 - pixelDist, 0.0, 1.0);
    return mix(background, foreground, opacity);
}

vec3 bitmap_font(vec3 composite_background)
{
    vec4 v = texture2D(texture, tex);
    
    float alpha = pow(v.a * texture_opacity, 2.2);
    
    vec3 text = v.rgb * foreground;

    return composite_background * (1.0 - alpha) + text * alpha;
}

void main()
{
	vec2 window_pixel = gl_FragCoord.xy;
	
	vec2 dpos = vec2(dFdx(cell_space.x), -dFdy(cell_space.y));
	vec2 size_px = 1.0 / dpos;
	vec2 pixel = cell_space * size_px;
	
	bool isBorder = (pixel.x < border_size && (flags & FLAG_LEFT_BORDER) > 0u)
                    || (pixel.x > size_px.x - border_size && (flags & FLAG_RIGHT_BORDER) != 0u)
                    || (pixel.y < border_size && (flags & FLAG_TOP_BORDER) != 0u)
                    || (pixel.y > size_px.y - border_size && (flags & FLAG_BOTTOM_BORDER) != 0u);
    bool isDash = (((int(window_pixel.x) - int(window_pixel.y)) / dash_size) % 2) == 0;
    isBorder = isBorder && ((flags & FLAG_DASHED_BORDER) == 0u || isDash);
    
    bool isSelected = (flags & FLAG_SELECTED) != 0u;

    vec3 composite_background = mix(background, border_color.rgb, border_color.a * float(isBorder));
    composite_background = mix(composite_background, selected_color.rgb, selected_color.a * float(isSelected));
	vec3 c = bitmap_font(composite_background);
	
	color = vec4(c, 1.0);
}