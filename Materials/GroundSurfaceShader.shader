Shader "Custom/GroundSurfaceShader"
{
    Properties
    {
        _FlatSurfaceTex ("Flat Surface Texture", 2D) = "white" {}
        _SlopedSurfaceTex ("Sloped Surface Texture", 2D) = "white" {}
        _TexTransitionSpeed("Texture Transition Speed", float) = 1
        _FlatTexScale("Flat Texture Scale", float) = 1
        _SlopedTexScale("Sloped Texture Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
            float2 uv_MainTex;
            float3 vertexNormal;
        };

        // Transfer the vertex normal to the Input structure
        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexNormal = abs(v.normal);
        }

        sampler2D _FlatSurfaceTex;
        sampler2D _SlopedSurfaceTex;
        float _TexTransitionSpeed;
        float _FlatTexScale;
        float _SlopedTexScale;

        float getStrengthValue(float x) {
            return pow(x, _TexTransitionSpeed) / (pow(x, _TexTransitionSpeed) + pow(1 - x, _TexTransitionSpeed));
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            half yNormal = 1 - sqrt(pow(IN.vertexNormal.x, 2) + pow(IN.vertexNormal.z, 2));

            float transitionValue = getStrengthValue(yNormal);
            float3 xNormalValue = IN.vertexNormal.x * (1 - transitionValue) * tex2D(_SlopedSurfaceTex, IN.uv_MainTex * _SlopedTexScale);
            float3 zNormalValue = IN.vertexNormal.z * (1 - transitionValue) * tex2D(_SlopedSurfaceTex, float2(IN.uv_MainTex.y, IN.uv_MainTex.x) * _SlopedTexScale);
            float3 yNormalValue = transitionValue * tex2D(_FlatSurfaceTex, IN.uv_MainTex * _FlatTexScale);
            o.Albedo = xNormalValue + zNormalValue + yNormalValue;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
