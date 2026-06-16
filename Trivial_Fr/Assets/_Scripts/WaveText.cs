using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WaveText : MonoBehaviour
{
    [Header("Wave Movement")]
    public float waveHeight = 10f;
    public float waveSpeed = 2f;
    public float characterOffset = 0.4f;

    [Header("Color Shift")]
    public bool animateColor = true;
    public float colorSpeed = 1f;

    [Tooltip("Add the colors you want the text to cycle through.")]
    public Color[] colorPool = new Color[]
    {
        Color.cyan,
        Color.magenta,
        Color.yellow
    };

    private TMP_Text textMesh;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;

        if (textInfo.characterCount == 0)
            return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            float wave = Mathf.Sin(Time.time * waveSpeed + i * characterOffset);

            Vector3 offset = new Vector3(0, wave * waveHeight, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;

            if (animateColor && colorPool != null && colorPool.Length > 0)
            {
                Color color = GetColorFromPool(Time.time * colorSpeed + i * 0.15f);

                
                color.a = 1f;

                Color32 finalColor = color;

                colors[vertexIndex + 0] = finalColor;
                colors[vertexIndex + 1] = finalColor;
                colors[vertexIndex + 2] = finalColor;
                colors[vertexIndex + 3] = finalColor;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;

            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    Color GetColorFromPool(float time)
    {
        if (colorPool == null || colorPool.Length == 0)
            return Color.white;

        if (colorPool.Length == 1)
            return colorPool[0];

        float scaledTime = Mathf.Repeat(time, colorPool.Length);

        int currentIndex = Mathf.FloorToInt(scaledTime);
        int nextIndex = (currentIndex + 1) % colorPool.Length;

        float blend = scaledTime - currentIndex;

        Color currentColor = colorPool[currentIndex];
        Color nextColor = colorPool[nextIndex];

        currentColor.a = 1f;
        nextColor.a = 1f;

        return Color.Lerp(currentColor, nextColor, blend);
    }
}