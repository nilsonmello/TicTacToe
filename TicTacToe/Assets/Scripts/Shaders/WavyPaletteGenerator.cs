using UnityEngine; 
public class WavyPaletteGenerator : MonoBehaviour
{
    public Color[] paletteColors;
    public Material targetMaterial;

    private Texture2D paletteTexture;

    void Start()
    {
        GeneratePaletteTexture();
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_PaletteTex", paletteTexture);
        }
    }
    void OnValidate()
    {
        GeneratePaletteTexture();
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_PaletteTex", paletteTexture);
        }
    }
    private void GeneratePaletteTexture()
    {
        paletteTexture = new Texture2D(paletteColors.Length, 1, TextureFormat.RGBA32, false);
        paletteTexture.wrapMode = TextureWrapMode.Clamp;
        paletteTexture.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < paletteColors.Length; i++)
        {
            paletteTexture.SetPixel(i, 0, paletteColors[i]);
        }
        paletteTexture.Apply();
    }
    public void UpdatePalette(Color[] newColors)
    {
        paletteColors = newColors;
        GeneratePaletteTexture();
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_PaletteTex", paletteTexture);
        }
    }
}
