using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class puzzle_manager : MonoBehaviour
{
    public static bool Victory = false;

    public static List<Color> lst_zone_color = new List<Color>() { Color.white, Color.magenta, Color.green, Color.cyan, Color.gray, Color.blue, Color.black };
    private Color transparent = new Color(1f, 1f, 1f, 0f);
    public static puzzle_tile current_tile_clic;

    public puzzle_tile Tile_prefab;
    private Vector3 Tile_lossy_scale
    {
        get { return Tile_prefab.transform.lossyScale; }
    }

    private static int Nbr_Lin;
    private static int Nbr_Col;
    private static puzzle_tile[,] tbl_Puzzel;

    private static int nbr_of_zone = 0;
    private static int[] tbl_nbrTile_of_zone;

    private Transform puzzle_tiles_ancre = null;

    // ##############################     INITIALISATION DU PUZZLE     #################################

    public void Start()
    {
        if(Tile_prefab == null)
        {
            Debug.LogWarning("vous avez oublier de passer le prefab de Tile à 'puzzle_loader !! ");
        }
        else
        {
            Load_puzzle();
        }
    }

    private void Load_puzzle()
    {
        Victory = false;

        Nbr_Lin = 4;
        Nbr_Col = 4;
        Puzzle_creator.Create_new_puzzle(Nbr_Lin, Nbr_Col);
        tbl_Puzzel = new puzzle_tile[Nbr_Lin, Nbr_Col];

        // créer l'ancrage des tiles pour Unity editor
        GameObject obj = new GameObject("Tiles_puzzle");
        puzzle_tiles_ancre = obj.transform;

        lst_zone_color[lst_zone_color.Count - 1] = transparent; // remplace la black de la fin de la liste par une couleur transparente
        Create_background_puzzle();

        // contruit la liste des zones
        foreach (int item in Puzzle_creator.tbl_zone_puzzle)
        {
            if (item > nbr_of_zone)
            {
                nbr_of_zone = item;
            }
        }
        tbl_nbrTile_of_zone = new int[nbr_of_zone + 1]; // créer le tableau avec les zones
        foreach (int item in Puzzle_creator.tbl_zone_puzzle)
        {
            tbl_nbrTile_of_zone[item] += 1; // ajoute 1 et permet d'obtenir le nombre de tile dans une zone
        }
    }

    public void Create_background_puzzle()
    {
        Debug.Log("Create the puzzle : start");

        // replace puzzel_loader
        Vector3 new_pos = Vector3.zero;
        new_pos.x = -((Nbr_Col / 2) * Tile_lossy_scale.x);
        new_pos.y = ((Nbr_Lin / 2) * Tile_lossy_scale.y);

        if(Nbr_Col % 2 == 0) // si le nombre de colonne est paire alors on recentre
        {
            new_pos.x += Tile_lossy_scale.x/2;
        }
        if(Nbr_Lin %2 == 0) // si le nombre de lignes  est paire alors on recentre
        {
            new_pos.y -= Tile_lossy_scale.y/2;
        }

        this.transform.position = new_pos; // replace le background pour qu'il soit centré (création des tuile TOP/RIGHT)

        Vector3 position_tile = Vector3.zero;
        for (int lin = 0; lin < Nbr_Lin; lin++)
        {
            position_tile.y = this.transform.position.y - lin * Tile_lossy_scale.y;
            for (int col = 0; col < Nbr_Col; col++)
            {
                position_tile.x = this.transform.position.x + col * Tile_lossy_scale.x;

                Add_new_Tile(lin, col, position_tile);
            }
        }
        Debug.Log("Create the puzzle : finish");
    }

    private void Add_new_Tile(int pLin, int pCol, Vector3 position)
    {
        //Debug.Log("tile puzzle create: [" + pLin + "," + pCol + "]");

        int chiffre = Puzzle_creator.tbl_chiffre_puzzle[pLin, pCol];
        int zone = Puzzle_creator.tbl_zone_puzzle[pLin, pCol];

        puzzle_tile new_tile = puzzle_tile.Instantiate<puzzle_tile>(Tile_prefab, position, Quaternion.identity);

        /* Change la couleur du chiffre
        SpriteRenderer spr_tile = new_tile.GetComponentInChildren<Transform>().Find("Chiffre").GetComponent<SpriteRenderer>();
        spr_tile.color = lst_chiffre_color[tbl_chiffre_puzzle[pLin, pCol]];*/

        // met a jour le tableau qui contient les chiffres
        new_tile.transform.SetParent(puzzle_tiles_ancre, true);
        new_tile.Set_Id_on_puzzle_tab(pLin, pCol);
        new_tile.Set_my_Chiffre(chiffre);

        if (chiffre != 0) // si ce n'est pas une case vise alors le joueurs ne pourra pas la modifier
        {
            new_tile.Is_clicable = false;
        }

        new_tile.zone_attachment_ID = zone;
        new_tile.Change_tile_color(lst_zone_color[new_tile.zone_attachment_ID], true);

        tbl_Puzzel[pLin, pCol] = new_tile;
    }

    // ##############################     GESTION DU PUZZLE     #################################

    public static void Add_new_chiffre(int pChiffre)
    {
        if(current_tile_clic != null)
        {
            // mise a jour du chiffre
            current_tile_clic.Set_my_Chiffre(pChiffre);

            // MAJ DES ZONES (verifie si les changements de la case actuelle influance l'ancienne étas des cases de la zone)

            // test chiffre trop grand pour la zone
            Color new_color = Color.white;
            if (Cherch_if_zoneIsCorrect() == true) // si la zone est maintenant conforme alors la remettre en couleur initiale
            {
                new_color = current_tile_clic.native_color;
            }
            else
            {
                Debug.LogWarning("Clic on : " + pChiffre + ", vous essayer de mettre un chiffre trop grand dans cette zone");
                new_color = Color.red;
            }
            foreach (puzzle_tile tile in tbl_Puzzel)
            {
                if (tile.zone_attachment_ID == current_tile_clic.zone_attachment_ID)
                {
                    tile.Set_wrong_state(puzzle_tile.Wrong_state.chiffre_zone);
                    tile.Change_tile_color(new_color);
                }
            }

            //test chiffre deja existant
            puzzle_tile.Wrong_state new_wrong_state = Cherch_if_chiffre_exist();
            current_tile_clic.Set_wrong_state(new_wrong_state);

            // VERIFICATION DE VICTOIRE
            Test_victory();
        }
    }

    private static bool Cherch_if_zoneIsCorrect(int Id_zone = -1)
    {
        bool zone_is_correct = true;

        if (Id_zone == -1) // verifie la zone de la case actuelle 
        {
            Id_zone = current_tile_clic.zone_attachment_ID;
        }

        foreach (puzzle_tile tile in tbl_Puzzel)
        {
            if (tile.zone_attachment_ID == Id_zone)
            {
                if (tile.current_chiffre > tbl_nbrTile_of_zone[Id_zone])
                {
                    //Debug.Log("zone incorrect : " + tile.current_chiffre + ", "  + tbl_nbrTile_of_zone[Id_zone]);
                    return false;
                }
            }
        }

        return zone_is_correct;
    }

    // renvoie le type d'erreur constaté (si none alors aucune erreur)
    private static puzzle_tile.Wrong_state Cherch_if_chiffre_exist()
    {
        puzzle_tile.Wrong_state this_wrong_state = puzzle_tile.Wrong_state.none;

        // parcours du tbl puzzle => puzzle_tile
        for (int lin = 0; lin < tbl_Puzzel.GetLength(0); lin++)
        {
            for (int col = 0; col < tbl_Puzzel.GetLength(1); col++)
            {
                puzzle_tile compare_tile = tbl_Puzzel[lin, col];// recupere la tuile a comparer

                // verification que la case comparé est différente de celle ciblé
                if (compare_tile != current_tile_clic && compare_tile.current_chiffre != 0) 
                {
                    // si la case appartient a la même zone que la case à changer
                    if (compare_tile.zone_attachment_ID == current_tile_clic.zone_attachment_ID)
                    {
                        this_wrong_state = Test_LastANDcurrent_chiffre(compare_tile);
                    }
                    else // pas dans la même zone
                    {
                        // verification que la case est adjacente
                        if ((compare_tile.Column_ID >= current_tile_clic.Column_ID - 1 && compare_tile.Column_ID <= current_tile_clic.Column_ID + 1)
                           &&
                           (compare_tile.Line_ID >= current_tile_clic.Line_ID - 1 && compare_tile.Line_ID <= current_tile_clic.Line_ID + 1))
                        {
                            this_wrong_state = Test_LastANDcurrent_chiffre(compare_tile);
                        }
                    }
                }
            }
        }

        return this_wrong_state;
    }

    private static puzzle_tile.Wrong_state Test_LastANDcurrent_chiffre(puzzle_tile pCompare_tile)
    {
        puzzle_tile.Wrong_state this_wrong_state = puzzle_tile.Wrong_state.none;

        // si la case regardé porte le même chiffre que le chiffre demandé
        if (pCompare_tile.current_chiffre == current_tile_clic.current_chiffre)
        {
            this_wrong_state = puzzle_tile.Wrong_state.chiffre_adj;
            pCompare_tile.Set_wrong_state(this_wrong_state);
        }
        // si la case regardé porte le même chiffre que l'ancien de la case changé
        else if (pCompare_tile.current_chiffre == current_tile_clic.last_chiffre)
        {
            pCompare_tile.Set_wrong_state(puzzle_tile.Wrong_state.none);
        }

        return this_wrong_state;
    }

    private static void Test_victory()
    {
        bool puzzle_isFinished = true;
        foreach (puzzle_tile tile in tbl_Puzzel)
        {
            if (tile.current_chiffre == 0)
            {
                puzzle_isFinished = false;
            }
        }

        if (puzzle_isFinished == true)
        {
            Victory = true;
        }
    }
}
