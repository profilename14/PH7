Shader "Custom/PuddleMask"
{
    SubShader{
        Tags{"Queue" = "Transparent+1"}

        Pass {
            Blend Zero One
        }
    }
}
