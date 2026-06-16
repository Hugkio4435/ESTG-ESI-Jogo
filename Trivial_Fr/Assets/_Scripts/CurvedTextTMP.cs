using UnityEngine;
using TMPro;

[ExecuteAlways] 
public class CurvedTextTMP : MonoBehaviour
{
    public TMP_Text myText;

    [Header("Definições da Curva")]
    public AnimationCurve vertexCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1f), new Keyframe(1, 0));
    public float curveMultiplier = 10f;

    void Update()
    {
        if (myText == null) myText = GetComponent<TMP_Text>();

        myText.ForceMeshUpdate();
        TMP_TextInfo textInfo = myText.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        float boundsMinX = myText.bounds.min.x;
        float boundsMaxX = myText.bounds.max.x;

        
        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            
            for (int j = 0; j < 4; j++)
            {
                Vector3 origPos = vertices[vertexIndex + j];

                
                float charMidBaselinePos = (origPos.x - boundsMinX) / (boundsMaxX - boundsMinX);

                
                origPos.y += vertexCurve.Evaluate(charMidBaselinePos) * curveMultiplier;

                vertices[vertexIndex + j] = origPos;
            }
        }

       
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            if (textInfo.meshInfo[i].mesh != null)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                myText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}