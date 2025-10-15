#ifndef SAMPLE_SH_SG_INCLUDED
#define SAMPLE_SH_SG_INCLUDED

void AmbientLight_float(float3 WorldNormal, out float3 Ambient)
{
#ifdef SHADERGRAPH_PREVIEW
      Ambient = 0.0f;
#else
        Ambient = SampleSH(WorldNormal);
#endif
}


#endif // SAMPLE_SH_SG_INCLUDED