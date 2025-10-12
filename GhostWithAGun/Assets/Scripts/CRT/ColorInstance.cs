using UnityEngine;

[ExecuteInEditMode]
public class ColorInstance : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.white;
    Renderer rend;
    MaterialPropertyBlock block;

    void OnValidate()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (block == null) block = new MaterialPropertyBlock();

        block.SetColor("_BaseColor", baseColor);
        rend.SetPropertyBlock(block);
    }
}