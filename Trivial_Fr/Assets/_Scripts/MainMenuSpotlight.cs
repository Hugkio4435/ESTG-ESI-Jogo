using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuSpotlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Objetos a Ativar")]
    public GameObject spotlightSprite;
    public GameObject ecraEscurecido;

    // Guarda a posição original do botão na lista para o devolver ao sítio certo
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
            // Empurra o ecrã escuro para o fundo da lista (tapa todos os botões)
            ecraEscurecido.transform.SetAsLastSibling();
        }

        if (spotlightSprite != null) spotlightSprite.SetActive(true);

        // Empurra ESTE botão para o fundo da lista (fica à frente do ecrã escuro)
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spotlightSprite != null) spotlightSprite.SetActive(false);
        if (ecraEscurecido != null) ecraEscurecido.SetActive(false);

        // Devolve o botão à sua posição original na hierarquia
        transform.SetSiblingIndex(posicaoOriginal);
    }

    void OnDisable()
    {
        if (spotlightSprite != null) spotlightSprite.SetActive(false);
        if (ecraEscurecido != null) ecraEscurecido.SetActive(false);
    }
}