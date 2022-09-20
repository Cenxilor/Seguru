using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class puzzle_tile : MonoBehaviour
{
    public enum Wrong_state
    {
        chiffre_adj,
        chiffre_zone,
        none
    }

    // Variable public 

    public bool side_is_BIG = false;

    [Space(30)]
    public bool Is_clicable = true;
    public int zone_attachment_ID = -1;
    public int Line_ID = -1;
    public int Column_ID = -1;
    public int last_chiffre { get; private set; }
    public int current_chiffre { get; private set; }
    public Color native_color { get; private set; }
    public Color native_Side_color { get; private set; }
    public Wrong_state my_wrong_state = Wrong_state.none;

    // Variable privée

    private SpriteRenderer spriteRenderer;
    private Chiffre my_chiffre;

    private float min_side_size = 0.75f;
    private float max_side_size = 2f;

    private SpriteRenderer[] tbl_sides = new SpriteRenderer[3];
    private const float side_convertion = 0.01f;
    private float side_scale 
    {
        get
        {
            if(side_is_BIG)
            {
                return max_side_size * side_convertion;
            }
            else
            {
                return min_side_size * side_convertion;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        tbl_sides = Set_tbl_sides(); // recupere les sprites renderers des enfants

        Set_Side_dimension();

        Get_Side_SpriteRenderer(Name_of_Side.left_side);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            current_chiffre++;
            my_chiffre.Change_sprite_chiffre(current_chiffre);
        }
    }

    public void Set_my_Chiffre(int pChiffre) // -1 permet de juste mettre a jour le sprite
    {
        // VERIFICATION 
        if (my_chiffre == null)
        {
            my_chiffre = gameObject.GetComponentInChildren<Chiffre>(); // my_chiffre n'existe pas on le récupere

        }
        else if (pChiffre < 0 || pChiffre > 9)
        {
            Debug.LogWarning("chiffre incorecte lors de la mise a jour du chiffre courant");
            return;
        }

        // changement du chiffre
        last_chiffre = current_chiffre;
        current_chiffre = pChiffre;

        my_chiffre.Change_sprite_chiffre(current_chiffre);        
    }

    public void Set_wrong_state(Wrong_state pNew_wrong_state = Wrong_state.none)
    {
        my_wrong_state = pNew_wrong_state;

        // si le wrong_state est du a un chifre adjacent alors doit passer le chiffre en rouge 
        my_chiffre.Change_sprite_chiffre(current_chiffre, pNew_wrong_state == Wrong_state.chiffre_adj);
    }

    public void Set_Id_on_puzzle_tab(int pLin, int pCol)
    {
        Line_ID = pLin;
        Column_ID = pCol;
    }

    public enum Name_of_Side
    {
        left_side,
        right_side,
        top_side,
        bot_side,
        none
    }

    // used Side_size qui renvoi un dimention en fonction de 'side_is_big'
    private void Set_Side_dimension()
    {
        foreach (SpriteRenderer spr_render in tbl_sides)
        {
            string spr_name = spr_render.transform.name;
            float new_position = 0f;

            if (spr_name == Name_of_Side.left_side.ToString()) // left side
            {
                spr_render.transform.localScale = new Vector3(side_scale, 1.0f, 1.0f);
                new_position = 0.5f - (side_scale / 2); //0.5f car ma case vos une unité
                spr_render.transform.localPosition = new Vector3(-new_position, 0f, 0f);
            }
            else if (spr_name == Name_of_Side.right_side.ToString()) // right side
            {
                spr_render.transform.localScale = new Vector3(side_scale, 1.0f, 1.0f);
                new_position = 0.5f - (side_scale / 2);
                spr_render.transform.localPosition = new Vector3(new_position, 0f, 0f);
            }
            else if (spr_name == Name_of_Side.top_side.ToString()) // top side
            {
                spr_render.transform.localScale = new Vector3(1.0f, side_scale, 1.0f);
                new_position = 0.5f - (side_scale / 2);
                spr_render.transform.localPosition = new Vector3(0f, new_position, 0f);
            }
            else if (spr_name == Name_of_Side.bot_side.ToString()) // bottom side
            {
                spr_render.transform.localScale = new Vector3(1.0f, side_scale, 1.0f);
                new_position = 0.5f - (side_scale / 2);
                spr_render.transform.localPosition = new Vector3(0f, -new_position, 0f);
            }
        }
    }

    private SpriteRenderer Get_Side_SpriteRenderer(Name_of_Side pName_side)
    {
        SpriteRenderer cherch_SpriteRenderer = null;

        for (int i = 0; i < tbl_sides.Length; i++)
        {
            SpriteRenderer spr_render = tbl_sides[i];            
            if (spr_render.transform.name == pName_side.ToString())
            {
                cherch_SpriteRenderer = spr_render;
            }
        }

        //Debug.Log("This Renderer is on " + cherch_SpriteRenderer.transform.name + " & his scale : " + cherch_SpriteRenderer.transform.localScale);

        return cherch_SpriteRenderer;
    }

    private SpriteRenderer[] Set_tbl_sides()
    {
        SpriteRenderer[] final_tbl_sides = new SpriteRenderer[4];
        SpriteRenderer[] tempon_tbl_sides = new SpriteRenderer[4]; // recupere tous les renderers (y compris le principal) 
        tempon_tbl_sides = gameObject.GetComponentsInChildren<SpriteRenderer>(false);
        for (int i = 1; i < tempon_tbl_sides.Length - 1; i++)
        {
            final_tbl_sides[i - 1] = tempon_tbl_sides[i]; // copie les éléments, permet de supprimer le render natif (parent)
            //Debug.Log("id " + i + " is " + tempon_tbl_sides[i].transform.name + ", scale : " + tempon_tbl_sides[i].transform.localScale);
        }

        native_Side_color = final_tbl_sides[0].color;

        return final_tbl_sides;
    }

    private void OnMouseDown()
    {
        if(Is_clicable == false) { return; }

        //Debug.Log("you tuch : [" + Line_ID + ","+ Column_ID + "]");

        On_Clic();
    }

    public void Change_tile_color(Color pNew_color, bool pChange_native_to = false)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        spriteRenderer.color = pNew_color;

        if(pChange_native_to == true)
        {
            native_color = pNew_color;
        }
    }

    public void On_Clic()
    {
        //supprime l'ancien ref
        if (puzzle_manager.current_tile_clic != null)
        {
            foreach (SpriteRenderer spr_renderer_SIDE in puzzle_manager.current_tile_clic.tbl_sides)
            {
                spr_renderer_SIDE.color = native_Side_color;
            }

            //puzzle_loader.current_tile_clic.Change_tile_color(puzzle_loader.current_tile_clic.native_color);
        }

        // maj new ref
        puzzle_manager.current_tile_clic = this;

        foreach (SpriteRenderer spr_renderer_SIDE in puzzle_manager.current_tile_clic.tbl_sides)
        {
            spr_renderer_SIDE.color = Color.yellow;
        }
        //puzzle_loader.current_tile_clic.spriteRenderer.color = Color.yellow;
    }

}
