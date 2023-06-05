Shader "Custom/RoughShader"
{
	Properties
	{
		_Color("Color Value", Color) = (1,1,1,1)
		_SpecColor("Spec Color Value", Color) = (1,1,1,1)
		_Shiny("Shinyness", float) = 45
		_MainTex("Texture Image", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"

		uniform float4 _Color;
	uniform float4 _LightColor0;
	uniform float4 _SpecColor;
	uniform float _Shiny;
	uniform sampler2D _MainTex;
	uniform sampler2D _BumpMap;
	uniform float4 _MainTex_ST;
	uniform float4 _BumpMap_ST;

	struct VS_IN {
		float4 pos : POSITION;
		float3 normal : NORMAL;
		float3 texcoord : TEXCOORD0;
		float4 tangent : TANGENT;
	};

	struct VS_OUT {
		float4 pos : SV_POSITION;
		float3 tex: TEXCOORD0;
		float4 posWorld : TEXCOORD1;
		float3 tangentWorld : TEXCOORD2;
		float3 normalWorld : TEXCOORD3;
		float3 binormalWorld : TEXCOORD4;
	};

	VS_OUT VS(VS_IN input)
	{
		VS_OUT output;

		float4x4 modelMatrix = unity_ObjectToWorld;
		float4x4 modelMatrixInverse = unity_WorldToObject;

		output.tangentWorld = normalize(
			mul(modelMatrix, float4(input.tangent.xyz, 0)).xyz);
		output.normalWorld = normalize(
			mul(float4(input.normal, 0), modelMatrixInverse).xyz);
		output.binormalWorld = normalize(
			cross(output.tangentWorld, output.normalWorld) * input.tangent.w);

		output.posWorld = mul(modelMatrix, input.pos);
		output.pos = UnityObjectToClipPos(input.pos);
		output.tex = input.texcoord;

		return output;
	}

	float4 PS_A(VS_OUT input) : COLOR
	{
	float4 encodedNormal = tex2D(_BumpMap, _BumpMap_ST.xy * input.tex.xy + _BumpMap_ST.zw);

	float3 localCoords = float3(2 * encodedNormal.a - 1, 2 * encodedNormal.g - 1, 0);
	localCoords.z = sqrt(1 - dot(localCoords, localCoords));

	float3x3 local2WorldTranspose = float3x3(input.tangentWorld, input.binormalWorld, input.normalWorld);

	float3 normalDir = normalize(mul(localCoords, local2WorldTranspose));

	float3 viewDir = normalize(_WorldSpaceCameraPos - input.posWorld.xyz);
	float3 lightDir;
	float attenuation;

	float3 pWeight = abs(input.normalWorld);
	pWeight /= pWeight.x + pWeight.y + pWeight.z;

	float4 xP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.x;
	float4 yP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.y;
	float4 zP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.z;

	float4 texColor = xP + yP + zP;

	if (_WorldSpaceLightPos0.w == 0)
	{
		attenuation = 1;
		float3 lightDir = normalize(
			_WorldSpaceLightPos0.xyz
		);
	}
	else
	{
		float3 vertexToLight = _WorldSpaceLightPos0.xyz -
			input.posWorld.xyz;
		attenuation = 1 / pow(1.25, length(vertexToLight));
		lightDir = normalize(vertexToLight);
	}

	float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb * texColor.rgb;

	float3 diffRefl = attenuation * _LightColor0.rgb * _Color.rgb *
		max(0, dot(normalDir, lightDir)) * texColor.rgb;

	float3 specRefl;

	if (dot(normalDir, lightDir) < 0)
	{
		specRefl.xyz = 0;
	}
	else
	{
		specRefl =
			attenuation * _LightColor0.rgb * _SpecColor.rgb *
			pow(max(0, dot(reflect(-lightDir, normalDir), viewDir)), _Shiny);
	}

	return float4(ambient + diffRefl + specRefl, 1);
	}

		float4 PS(VS_OUT input) : COLOR
	{
	float4 encodedNormal = tex2D(_BumpMap, _BumpMap_ST.xy * input.tex.xy + _BumpMap_ST.zw);

	float3 localCoords = float3(2 * encodedNormal.a - 1, 2 * encodedNormal.g - 1, 0);
	localCoords.z = sqrt(1 - dot(localCoords, localCoords));

	float3x3 local2WorldTranspose = float3x3(input.tangentWorld, input.binormalWorld, input.normalWorld);

	float3 normalDir = normalize(mul(localCoords, local2WorldTranspose));

	float3 viewDir = normalize(_WorldSpaceCameraPos - input.posWorld.xyz);
	float3 lightDir;
	float attenuation;

	float3 pWeight = abs(input.normalWorld);
	pWeight /= pWeight.x + pWeight.y + pWeight.z;

	float4 xP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.x;
	float4 yP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.y;
	float4 zP = tex2D(_MainTex, _MainTex_ST.xy * input.tex.xy + _MainTex_ST.zw) * pWeight.z;

	float4 texColor = xP + yP + zP;

	if (_WorldSpaceLightPos0.w == 0)
	{
		attenuation = 1;
		float3 lightDir = normalize(
			_WorldSpaceLightPos0.xyz
		);
	}
	else
	{
		float3 vertexToLight = _WorldSpaceLightPos0.xyz -
			input.posWorld.xyz;
		attenuation = 1 / pow(1.25, length(vertexToLight));
		lightDir = normalize(vertexToLight);
	}

	float3 diffRefl = attenuation * _LightColor0.rgb * _Color.rgb *
		max(0, dot(normalDir, lightDir)) * texColor.rgb;

	float3 specRefl;

	if (dot(normalDir, lightDir) < 0)
	{
		specRefl.xyz = 0;
	}
	else
	{
		specRefl =
			attenuation * _LightColor0.rgb * _SpecColor.rgb *
			pow(max(0, dot(reflect(-lightDir, normalDir), viewDir)), _Shiny);
	}

	return float4(diffRefl + specRefl, 1);
	}
		// End CG
		ENDCG

		SubShader
	{
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex VS
			#pragma fragment PS_A
			ENDCG
		}

			Pass
		{
			Tags {"LightMode" = "ForwardAdd"}
			Blend One One
			CGPROGRAM
			#pragma vertex VS
			#pragma fragment PS
			ENDCG
		}
	}
}