using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuSpotlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Objetos a Ativar")]
    public GameObject spotlightSprite;
    public GameObject ecraEscurecido;

    
    private int posicaoOriginal;

    void Start()
    {
        posicaoOriginal = transform.GetSiblingIndex();

        if (spotlightSprite != null) spotlightSprite.SetActive(false);
        if (ecraEscurecido != null) ecraEscurecido.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ecraEscurecido != null)
        {
            ecraEscurecido.SetActive(true);
            
            ecraEscurecido.transform.SetAsLastSibling();
        }

        if (spotlightSprite != null) spotlightSprite.SetActive(true);

    
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spotlightSprite != null) spotlightSprite.SetActive(false);
        if (ecraEscurecido != null) ecraEscurecido.SetActive(false);

        
        transform.SetSiblingIndex(posicaoOriginal);
    }

    void OnDisable()
    {
        if (spotlightSprite != null) spotlightSprite.SetActive(false);
        if (ecraEscurecido != null) ecraEscurecido.SetActive(false);
    }
}