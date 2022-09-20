using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // permet d'eviter la suppression du sprite renderer

public class Chiffre : MonoBehaviour
{
    [SerializeField]
    private Sprite[] lst_texture_chiffres;
    private SpriteRenderer sprite_Renderer = null;

    // Start is called before the first frame update
    void Start()
    {
        if(sprite_Renderer == null)
        {
            sprite_Renderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Change_sprite_chiffre(int pChiffre, bool Wrong_color = false)
    {
        if(pChiffre < 0 || pChiffre > 9)
        {
            Debug.LogWarning("chiffre incorecte lors du changement de sprite : " + pChiffre);
            return;
        }

        Set_visibility(pChiffre); // si 0 = false donc invisible

        sprite_Renderer.sprite = lst_texture_chiffres[(int)pChiffre];

        if(Wrong_color == true)
        {
            sprite_Renderer.color = Color.red;
        }
        else
        {
            sprite_Renderer.color = Color.white;
        }
    }

    private void Set_visibility(int pChiffre)
    {
        bool visible;
        if(pChiffre != 0)
        {
            visible = true;
        }
        else
        {
            visible = false;
        }

        if (sprite_Renderer == null)
        {
            sprite_Renderer = GetComponent<SpriteRenderer>();
        }

        sprite_Renderer.enabled = visible;
    }
}
