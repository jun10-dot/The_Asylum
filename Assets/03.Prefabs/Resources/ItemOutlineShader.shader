Shader "Draw/OutlineShader" {
    Properties {       //인스펙터에 보이는 수치값 (조정가능)
        _OutlineColor ("Outline Color", Color) = (0,1,0,1) 
        _Outline ("Outline width", Range (0, 1)) = 0.05   
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    struct v2f {
        float4 pos : POSITION;
        float4 color : COLOR;
    };

    uniform float _Outline;
    uniform float4 _OutlineColor;

    v2f vert(appdata v) {
        v2f o;

        v.vertex *= ( 1 + _Outline);

        o.pos = UnityObjectToClipPos(v.vertex);
        o.color = _OutlineColor; //내가 지정한 색깔로 셋팅.
        return o;
    }
    ENDCG

    SubShader {
        Tags { "DisableBatching" = "True" } //주로 디버깅이나 특정 렌더 상태 유지 위해
        Pass {
            Name "OUTLINE"
            Tags {"LightMode" = "Always" }
            Cull Front  //앞면 컬링, 카메라와 향하는 면.. 뒷면만 그림.
            ZWrite On   //깊이 버퍼에 기록.
            ColorMask RGB //알파는 출력되지않음.
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            half4 frag(v2f i) :COLOR { return i.color; } //모든 픽셀에  _OutlineColor를 칠함.
            ENDCG
        }
    }

    Fallback "Diffuse"
}