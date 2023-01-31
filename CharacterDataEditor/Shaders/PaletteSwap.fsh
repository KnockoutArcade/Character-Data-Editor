#version 460

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec4 basecolor0;
uniform vec4 basecolor1;
uniform vec4 basecolor2;
uniform vec4 basecolor3;
uniform vec4 basecolor4;
uniform vec4 basecolor5;
uniform vec4 basecolor6;
uniform vec4 basecolor7;
uniform vec4 basecolor8;
uniform vec4 basecolor9;
uniform vec4 basecolor10;
uniform vec4 basecolor11;
uniform vec4 basecolor12;
uniform vec4 basecolor13;
uniform vec4 basecolor14;
uniform vec4 basecolor15;
uniform vec4 basecolor16;
uniform vec4 basecolor17;
uniform vec4 basecolor18;
uniform vec4 basecolor19;

uniform vec4 swapcolor0;
uniform vec4 swapcolor1;
uniform vec4 swapcolor2;
uniform vec4 swapcolor3;
uniform vec4 swapcolor4;
uniform vec4 swapcolor5;
uniform vec4 swapcolor6;
uniform vec4 swapcolor7;
uniform vec4 swapcolor8;
uniform vec4 swapcolor9;
uniform vec4 swapcolor10;
uniform vec4 swapcolor11;
uniform vec4 swapcolor12;
uniform vec4 swapcolor13;
uniform vec4 swapcolor14;
uniform vec4 swapcolor15;
uniform vec4 swapcolor16;
uniform vec4 swapcolor17;
uniform vec4 swapcolor18;
uniform vec4 swapcolor19;

out vec4 finalColor;

void main()
{
    float range = 1.0f / 255.0f; //set a value to allow for pixels to be slightly off
    vec4 texel = texture2D(texture0, fragTexCoord); //get working texel color

    if(abs(texel.r - basecolor0.r) <= range && abs(texel.g - basecolor0.g) <= range && abs(texel.b - basecolor0.b) <= range)
    {
        texel.rgb = swapcolor0.rgb;
    }
    else if(abs(texel.r - basecolor1.r) <= range && abs(texel.g - basecolor1.g) <= range && abs(texel.b - basecolor1.b) <= range)
    {
        texel.rgb = swapcolor1.rgb;
    }
    else if(abs(texel.r - basecolor2.r) <= range && abs(texel.g - basecolor2.g) <= range && abs(texel.b - basecolor2.b) <= range)
    {
        texel.rgb = swapcolor2.rgb;
    }
    else if(abs(texel.r - basecolor3.r) <= range && abs(texel.g - basecolor3.g) <= range && abs(texel.b - basecolor3.b) <= range)
    {
        texel.rgb = swapcolor3.rgb;
    }
    else if(abs(texel.r - basecolor4.r) <= range && abs(texel.g - basecolor4.g) <= range && abs(texel.b - basecolor4.b) <= range)
    {
        texel.rgb = swapcolor4.rgb;
    }
    else if(abs(texel.r - basecolor5.r) <= range && abs(texel.g - basecolor5.g) <= range && abs(texel.b - basecolor5.b) <= range)
    {
        texel.rgb = swapcolor5.rgb;
    }
    else if(abs(texel.r - basecolor6.r) <= range && abs(texel.g - basecolor6.g) <= range && abs(texel.b - basecolor6.b) <= range)
    {
        texel.rgb = swapcolor6.rgb;
    }
    else if(abs(texel.r - basecolor7.r) <= range && abs(texel.g - basecolor7.g) <= range && abs(texel.b - basecolor7.b) <= range)
    {
        texel.rgb = swapcolor7.rgb;
    }
    else if(abs(texel.r - basecolor8.r) <= range && abs(texel.g - basecolor8.g) <= range && abs(texel.b - basecolor8.b) <= range)
    {
        texel.rgb = swapcolor8.rgb;
    }
    else if(abs(texel.r - basecolor9.r) <= range && abs(texel.g - basecolor9.g) <= range && abs(texel.b - basecolor9.b) <= range)
    {
        texel.rgb = swapcolor9.rgb;
    }
    else if(abs(texel.r - basecolor10.r) <= range && abs(texel.g - basecolor10.g) <= range && abs(texel.b - basecolor10.b) <= range)
    {
        texel.rgb = swapcolor10.rgb;
    }
    else if(abs(texel.r - basecolor11.r) <= range && abs(texel.g - basecolor11.g) <= range && abs(texel.b - basecolor11.b) <= range)
    {
        texel.rgb = swapcolor11.rgb;
    }
    else if(abs(texel.r - basecolor12.r) <= range && abs(texel.g - basecolor12.g) <= range && abs(texel.b - basecolor12.b) <= range)
    {
        texel.rgb = swapcolor12.rgb;
    }
    else if(abs(texel.r - basecolor13.r) <= range && abs(texel.g - basecolor13.g) <= range && abs(texel.b - basecolor13.b) <= range)
    {
        texel.rgb = swapcolor13.rgb;
    }
    else if(abs(texel.r - basecolor14.r) <= range && abs(texel.g - basecolor14.g) <= range && abs(texel.b - basecolor14.b) <= range)
    {
        texel.rgb = swapcolor14.rgb;
    }
    else if(abs(texel.r - basecolor15.r) <= range && abs(texel.g - basecolor15.g) <= range && abs(texel.b - basecolor15.b) <= range)
    {
        texel.rgb = swapcolor15.rgb;
    }
    else if(abs(texel.r - basecolor16.r) <= range && abs(texel.g - basecolor16.g) <= range && abs(texel.b - basecolor16.b) <= range)
    {
        texel.rgb = swapcolor16.rgb;
    }
    else if(abs(texel.r - basecolor17.r) <= range && abs(texel.g - basecolor17.g) <= range && abs(texel.b - basecolor17.b) <= range)
    {
        texel.rgb = swapcolor17.rgb;
    }
    else if(abs(texel.r - basecolor18.r) <= range && abs(texel.g - basecolor18.g) <= range && abs(texel.b - basecolor18.b) <= range)
    {
        texel.rgb = swapcolor18.rgb;
    }
    else if(abs(texel.r - basecolor19.r) <= range && abs(texel.g - basecolor19.g) <= range && abs(texel.b - basecolor19.b) <= range)
    {
        texel.rgb = swapcolor19.rgb;
    }

    finalColor = texel;
}