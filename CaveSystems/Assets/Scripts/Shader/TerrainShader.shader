Shader "Custom/TerrainShader"
{
	Properties
	{
		// The Texture located higher in the cave
		_MainTex("Ground Texture", 2D) = "white" {}
		// The Texture located lower in the cave
		_MainTex2("Wall Texture", 2D) = "white" {}
		// Texture scale
		_TexScale("Texture Scale", Float) = 1
		// The percentage of how much the higher texture is applied
		_BotPercentage("Bottom Percentage", Float) = 1
		_TopPercentage("Top Percentage", Float) = 1
		// The Bot and Top Position of the chunk
		_BotChunkPosY("Bottom Chunk Pos", Float) = 1
		_TopChunkPosY("Top Chunk Pos", Float) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0


		sampler2D _MainTex;
		sampler2D _MainTex2;
		float _TexScale;
		float _BotPercentage;
		float _TopPercentage;
		float _BotChunkPosY;
		float _TopChunkPosY;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		// Pixelshader is run for every pixel on the screen
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float percentage = (IN.worldPos.y - _BotChunkPosY) / (_TopChunkPosY - _BotChunkPosY) * (_TopPercentage - _BotPercentage) + _BotPercentage;
			// Get a the world position modified by scale
			float3 scaledWorldPos = IN.worldPos / _TexScale;
			// Get the current normal in positive direction
			float3 pWeight = abs(IN.worldNormal);
			// Ensure pWeight isn't greater than 1
			pWeight /= pWeight.x + pWeight.y + pWeight.z;

			// Get the texture projection on each axes and weight it by multiplying it by the pWeight
			float3 xP1 = tex2D(_MainTex, scaledWorldPos.yz) * pWeight.x;
			float3 yP1 = tex2D(_MainTex, scaledWorldPos.xz) * pWeight.y;
			float3 zP1 = tex2D(_MainTex, scaledWorldPos.xy) * pWeight.z;

			float3 xP2 = tex2D(_MainTex2, scaledWorldPos.yz) * pWeight.x;
			float3 yP2 = tex2D(_MainTex2, scaledWorldPos.xz) * pWeight.y;
			float3 zP2 = tex2D(_MainTex2, scaledWorldPos.xy) * pWeight.z;

			// Adds both textures together multiplied by the percentage
			o.Albedo = xP1 * (1 - percentage) + xP2 * percentage +
					   yP1 * (1 - percentage) + yP2 * percentage +
					   zP1 * (1 - percentage) + zP2 * percentage;
			//o.Albedo = float3(0, 0, 1) * percentage + float3(1, 0, 0) * (1 - percentage);
		}
		ENDCG
	}
	FallBack "Diffuse"
}