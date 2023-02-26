using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MeshGenerator))]
[CanEditMultipleObjects]
public class InspectorMeshGenerator : Editor
{
    int meshWidthPrev;
    int meshHeightPrev;
    float noiseXOffsetPrev;
    float noiseYOffsetPrev;
    float noiseIntensityPrev;
    NoiseMapData[] noiseMapsPrev;

    bool hasChangedValue { get
        {
            return meshWidthPrev != generator.meshWidth || meshHeightPrev != generator.meshHeight || noiseXOffsetPrev != generator.noiseXOffset || noiseYOffsetPrev != generator.noiseYOffset || noiseIntensityPrev != generator.noiseIntensity || noiseMapsPrev != generator.noiseMaps;
        } }

    MeshGenerator generator { get { return target as MeshGenerator; } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Generate Mesh"))
            {
                generator.CreateMesh(null, null, null, null, null);
            }

            if (generator.autoUpdateMesh && hasChangedValue)
            {
                meshWidthPrev = generator.meshWidth;
                meshHeightPrev = generator.meshHeight;
                noiseXOffsetPrev = generator.noiseXOffset;
                noiseYOffsetPrev = generator.noiseYOffset;
                noiseIntensityPrev = generator.noiseIntensity;
                noiseMapsPrev = generator.noiseMaps;

                generator.CreateMesh(null, null, null, null, null);
            }

            if (generator.autoUpdateNoiseTexture)
            {
                generator.CreateTexture();
            }
        }
    }
}
#endif