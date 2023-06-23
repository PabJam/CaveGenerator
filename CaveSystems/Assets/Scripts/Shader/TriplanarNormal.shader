// Code from : https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// Normal Mapping for a Triplanar Shader - Ben Golus 2017
// Unity Surface Shader example shader

// Implements correct triplanar normals in a Surface Shader with out computing or passing additional information from the
// vertex shader.

Shader "Custom/TriplanarNormal" {
    Properties{
        _MainTex("Ground Texture", 2D) = "white" {}
        _MainTex2("Wall Texture", 2D) = "white" {}
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _BumpMap2("Normal Map 2", 2D) = "bump" {}
        //_Glossiness("Smoothness", Range(0, 1)) = 0.5
        //[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        // The percentage of how much the higher texture is applied
        _BotPercentage("Bottom Percentage", Float) = 1
        _TopPercentage("Top Percentage", Float) = 1
        // The Bot and Top Position of the chunk
        _BotChunkPosY("Bottom Chunk Pos", Float) = 1
        _TopChunkPosY("Top Chunk Pos", Float) = 1
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            #include "UnityStandardUtils.cginc"

            // flip UVs horizontally to correct for back side projection
            #define TRIPLANAR_CORRECT_PROJECTED_U

            // offset UVs to prevent obvious mirroring
            // #define TRIPLANAR_UV_OFFSET

            // Reoriented Normal Mapping
            // http://blog.selfshadow.com/publications/blending-in-detail/
            // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
            half3 blend_rnm(half3 n1, half3 n2)
            {
                n1.z += 1;
                n2.xy = -n2.xy;

                return n1 * dot(n1, n2) / n1.z - n2;
            }

            sampler2D _MainTex;
            sampler2D _MainTex2;
            float4 _MainTex_ST;

            sampler2D _BumpMap;
            sampler2D _BumpMap2;

            //half _Glossiness;
            //half _Metallic;

            float _BotPercentage;
            float _TopPercentage;
            float _BotChunkPosY;
            float _TopChunkPosY;

            struct Input {
                float3 worldPos;
                float3 worldNormal;
                INTERNAL_DATA
            };

            float3 WorldToTangentNormalVector(Input IN, float3 normal) {
                float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
                float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
                float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
                float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
                return normalize(mul(t2w, normal));
            }

            void surf(Input IN, inout SurfaceOutputStandard o) {

                // work around bug where IN.worldNormal is always (0,0,0)!
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

                float percentage = (IN.worldPos.y - _BotChunkPosY) / (_TopChunkPosY - _BotChunkPosY) * (_TopPercentage - _BotPercentage) + _BotPercentage;

                // calculate triplanar blend
                half3 triblend = saturate(pow(IN.worldNormal, 4));
                triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                // calculate triplanar uvs
                // applying texture scale and offset values ala TRANSFORM_TEX macro
                float2 uvX = IN.worldPos.zy * _MainTex_ST.xy + _MainTex_ST.zy;
                float2 uvY = IN.worldPos.xz * _MainTex_ST.xy + _MainTex_ST.zy;
                float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy + _MainTex_ST.zy;

                // offset UVs to prevent obvious mirroring
            #if defined(TRIPLANAR_UV_OFFSET)
                uvY += 0.33;
                uvZ += 0.67;
            #endif

                // minor optimization of sign(). prevents return value of 0
                half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

                // flip UVs horizontally to correct for back side projection
            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                uvX.x *= axisSign.x;
                uvY.x *= axisSign.y;
                uvZ.x *= -axisSign.z;
            #endif

                // albedo textures
                fixed4 colX1 = tex2D(_MainTex, uvX);
                fixed4 colY1 = tex2D(_MainTex, uvY);
                fixed4 colZ1 = tex2D(_MainTex, uvZ);
                fixed4 col1 = colX1 * triblend.x + colY1 * triblend.y + colZ1 * triblend.z;

                fixed4 colX2 = tex2D(_MainTex2, uvX);
                fixed4 colY2 = tex2D(_MainTex2, uvY);
                fixed4 colZ2 = tex2D(_MainTex2, uvZ);
                fixed4 col2 = colX2 * triblend.x + colY2 * triblend.y + colZ2 * triblend.z;

                // tangent space normal maps
                half3 tnormalX1 = UnpackNormal(tex2D(_BumpMap, uvX));
                half3 tnormalY1 = UnpackNormal(tex2D(_BumpMap, uvY));
                half3 tnormalZ1 = UnpackNormal(tex2D(_BumpMap, uvZ));

                half3 tnormalX2 = UnpackNormal(tex2D(_BumpMap2, uvX));
                half3 tnormalY2 = UnpackNormal(tex2D(_BumpMap2, uvY));
                half3 tnormalZ2 = UnpackNormal(tex2D(_BumpMap2, uvZ));

                // flip normal maps' x axis to account for flipped UVs
            #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                tnormalX1.x *= axisSign.x;
                tnormalY1.x *= axisSign.y;
                tnormalZ1.x *= -axisSign.z;

                tnormalX2.x *= axisSign.x;
                tnormalY2.x *= axisSign.y;
                tnormalZ2.x *= -axisSign.z;
            #endif

                half3 absVertNormal = abs(IN.worldNormal);

                // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                tnormalX1 = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX1);
                tnormalY1 = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY1);
                tnormalZ1 = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ1);

                tnormalX2 = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX2);
                tnormalY2 = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY2);
                tnormalZ2 = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ2);

                // apply world space sign to tangent space Z
                tnormalX1.z *= axisSign.x;
                tnormalY1.z *= axisSign.y;
                tnormalZ1.z *= axisSign.z;

                tnormalX2.z *= axisSign.x;
                tnormalY2.z *= axisSign.y;
                tnormalZ2.z *= axisSign.z;

                // sizzle tangent normals to match world normal and blend together
                half3 worldNormal1 = normalize(
                    tnormalX1.zyx * triblend.x +
                    tnormalY1.xzy * triblend.y +
                    tnormalZ1.xyz * triblend.z
                    );

                half3 worldNormal2 = normalize(
                    tnormalX2.zyx * triblend.x +
                    tnormalY2.xzy * triblend.y +
                    tnormalZ2.xyz * triblend.z
                    );

                half3 worldNormal = normalize(
                    worldNormal1 * (1 - percentage) + 
                    worldNormal2 * percentage
                );

                // set surface ouput properties
                o.Albedo = col1.rgb * (1 - percentage) + col2.rgb * percentage;
                //o.Metallic = _Metallic;
                //o.Smoothness = _Glossiness;

                // convert world space normals into tangent normals
                o.Normal = WorldToTangentNormalVector(IN, worldNormal);
            }
            ENDCG
        }
            FallBack "Diffuse"
}