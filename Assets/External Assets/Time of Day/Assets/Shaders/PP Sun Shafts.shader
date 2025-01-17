// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Time of Day/Sun Shafts"
{
    Properties
    {
        _MainTex     ("Base",  2D) = "" {}
        _ColorBuffer ("Color", 2D) = "" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct v2f {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
        #if UNITY_UV_STARTS_AT_TOP
        float2 uv1 : TEXCOORD1;
        #endif
    };

    struct v2f_radial {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;
        float2 blurVector : TEXCOORD1;
    };

    sampler2D _MainTex;
    sampler2D _ColorBuffer;
    sampler2D _CameraDepthTexture;

    uniform half4 _SunColor;
    uniform half4 _BlurRadius4;
    uniform half4 _SunPosition;
    uniform half4 _MainTex_TexelSize;

    #define SAMPLES_FLOAT 6.0f
    #define SAMPLES_INT 6

    v2f vert(appdata_img v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy;

        #if UNITY_UV_STARTS_AT_TOP
        o.uv1 = v.texcoord.xy;
        if (_MainTex_TexelSize.y < 0)
            o.uv1.y = 1-o.uv1.y;
        #endif

        return o;
    }

    v2f_radial vert_radial(appdata_img v) {
        v2f_radial o;
        o.pos = UnityObjectToClipPos(v.vertex);

        o.uv.xy =  v.texcoord.xy;
        o.blurVector = (_SunPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;

        return o;
    }

    half TransformColor(half4 skyboxValue) {
        return max(1.01-skyboxValue.a, dot(skyboxValue.rgb, float3 (0.59,0.3,0.11)));
    }

    half4 frag_depth(v2f i) : COLOR {
        #if UNITY_UV_STARTS_AT_TOP
        float depthSample = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv1.xy));
        #else
        float depthSample = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv.xy));
        #endif

        half4 tex = tex2D(_MainTex, i.uv.xy);

        depthSample = Linear01Depth(depthSample);

        // Consider maximum radius
        #if UNITY_UV_STARTS_AT_TOP
        half2 vec = _SunPosition.xy - i.uv1.xy;
        #else
        half2 vec = _SunPosition.xy - i.uv.xy;
        #endif
        half dist = saturate(_SunPosition.w - length(vec));

        half4 outColor = 0;

        // Consider shafts blockers
        if (depthSample > 0.99)
            outColor = TransformColor(tex) * dist;

        return outColor;
    }

    half4 frag_nodepth(v2f i) : COLOR {
        half4 tex = tex2D(_MainTex, i.uv.xy);

        // Consider maximum radius
        #if UNITY_UV_STARTS_AT_TOP
        half2 vec = _SunPosition.xy - i.uv1.xy;
        #else
        half2 vec = _SunPosition.xy - i.uv.xy;
        #endif
        half dist = saturate(_SunPosition.w - length(vec));

        half4 outColor = 0;

        // Consider shafts blockers
        outColor = TransformColor(tex) * dist;

        return outColor;
    }

    half4 frag_radial(v2f_radial i) : COLOR {
        half4 color = half4(0,0,0,0);
        for(int j = 0; j < SAMPLES_INT; j++)
        {
            half4 tmpColor = tex2D(_MainTex, i.uv.xy);
            color += tmpColor;
            i.uv.xy += i.blurVector;
        }
        return color / SAMPLES_FLOAT;
    }

    half4 frag_screen(v2f i) : COLOR {
        half4 colorA = tex2D(_MainTex, i.uv.xy);
        #if UNITY_UV_STARTS_AT_TOP
        half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
        #else
        half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
        #endif
        half4 depthMask = saturate(colorB * _SunColor);
        return 1.0f - (1.0f-colorA) * (1.0f-depthMask);
    }

    half4 frag_add(v2f i) : COLOR {
        half4 colorA = tex2D(_MainTex, i.uv.xy);
        #if UNITY_UV_STARTS_AT_TOP
        half4 colorB = tex2D(_ColorBuffer, i.uv1.xy);
        #else
        half4 colorB = tex2D(_ColorBuffer, i.uv.xy);
        #endif
        half4 depthMask = saturate(colorB * _SunColor);
        return colorA + depthMask;
    }

    ENDCG

    Subshader
    {
        Pass
        {
            Blend Off
            Cull Off
            ZWrite Off
            ZTest Always
            Fog { Mode off }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag_screen
            ENDCG
        }

        Pass
        {
            Blend One Zero
            Cull Off
            ZWrite Off
            ZTest Always
            Fog { Mode off }

            CGPROGRAM
            #pragma vertex vert_radial
            #pragma fragment frag_radial
            ENDCG
        }

        Pass
        {
            Blend Off
            Cull Off
            ZWrite Off
            ZTest Always
            Fog { Mode off }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag_depth
            ENDCG
        }

        Pass
        {
            Blend Off
            Cull Off
            ZWrite Off
            ZTest Always
            Fog { Mode off }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag_nodepth
            ENDCG
        }

        Pass
        {
            Blend Off
            Cull Off
            ZWrite Off
            ZTest Always
            Fog { Mode off }

            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag_add
            ENDCG
        }
    }

    Fallback off
}
