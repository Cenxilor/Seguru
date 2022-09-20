using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Puzzle_creator
{
    public static int Nbr_Lin { get; private set; }
    public static int Nbr_Col { get; private set; }
    public static int Chiffre_max { get; private set; }
    public static int nbr_cases_zone_vides { get; private set; }

    private static List<Zone> lst_zones = new List<Zone>();
    private static int null_ref = -1;

    public static int[,] tbl_zone_puzzle = new int[4, 4];
    /*
    { { 4, 2 , 1, 1},
      { 4, 2 , 2, 1},
      { 3, 2 , 2, 2},
      { 3, 3 , 3, 3},
     };
    */

    public static int[,] tbl_chiffre_puzzle = new int[4, 4];
    /*
    { { 0, 3 , 0, 3},
      { 0, 0 , 6, 0},
      { 0, 1 , 4, 0},
      { 0, 3 , 0, 0},
     };
    */

    public static void Create_new_puzzle(int pNbr_lin, int pNbr_col, int pChiffre_max = 6)
    {
        Nbr_Lin = pNbr_lin;
        Nbr_Col = pNbr_col;
        Chiffre_max = pChiffre_max;
        lst_zones = new List<Zone>();
        tbl_zone_puzzle = Create_tbl_Zone(); // creation des zones necessaire avant chiffre

        int max_loop = 2; // nbr de création maximum
        while (max_loop > 0)
        {
            max_loop--;

            tbl_chiffre_puzzle = Create_tbl_chiffre();
            if(tbl_chiffre_puzzle == null) // le tableau de zone est inremplissable
            {
                tbl_zone_puzzle = Create_tbl_Zone(); // création d'un nouveau tableau de zones
            }
            else
            {
                Debug.LogWarning("TABLEAU CREER !!!!");
                break;
            }
        }
        if(max_loop <= 0)
        {
            Debug.LogError(" LE TABLEAU N'A PAS PU ETRE CREE SUITE A 2 TENTATIVE");
        }
        
    }

    private static int[,] Create_tbl_Zone()
    {
        nbr_cases_zone_vides = Nbr_Lin * Nbr_Col; // reset le nombre de cases vides (utilisé pour les zones)

        int[,] my_tbl_zone = new int[Nbr_Lin, Nbr_Col];
        Reset_tbl(my_tbl_zone, null_ref);

        int id_zone = 0;
        while (nbr_cases_zone_vides > 0)
        {
            Zone my_zone = new Zone(my_tbl_zone, id_zone);
            nbr_cases_zone_vides -= my_zone.Nbr_cases_of_zone;

            lst_zones.Add(my_zone);
            id_zone++;
        }
        return my_tbl_zone;
    }



    private static int[,] Create_tbl_chiffre()
    {
        int[,] my_tbl_chiffre = null;

        int max_loop = 100000;
        bool Tableau_validate = false;

        while (!Tableau_validate && max_loop > 0)
        {
            max_loop--; // evite une boucle infini !
            Tableau_validate = true; // permet de sortir de la boucle /!\ remis a false si erreur

            // INIT VARIABLE 
            List<Zone> copie_lst_zone = new List<Zone>();
            my_tbl_chiffre = new int[Nbr_Lin, Nbr_Col];


            nbr_cases_zone_vides = Nbr_Lin * Nbr_Col; // reset le nombre de cases vides (utilisé pour les chiffres)
            Reset_tbl(my_tbl_chiffre, null_ref);
            foreach (Zone zone in lst_zones)
            {
                copie_lst_zone.Add(zone);
            }


            // remplissage du tableau de zone
            while (nbr_cases_zone_vides > 0)
            {
                // récupération de la plus petite zone restantes dans la liste des zones
                Zone smaller_zone_rest = null; //lst_zones[Random.Range(0, lst_zones.Count)]; // recupère une zone aleatoire
                int Id_supp = -1;
                for (int i = 0; i < copie_lst_zone.Count; i++)
                {
                    Zone zone = copie_lst_zone[i];
                    if (smaller_zone_rest == null) // première affectation avec le premiere éléments restant
                    {
                        smaller_zone_rest = zone;
                        Id_supp = i;
                    }
                    else if (zone.Nbr_cases_of_zone < smaller_zone_rest.Nbr_cases_of_zone) // récupère la plus petite zone
                    {
                        smaller_zone_rest = zone;
                        Id_supp = i;
                    }
                }
                // supprime la zone choisi (car elle va être remplit)
                if (copie_lst_zone.Count > 0)
                {
                    copie_lst_zone.RemoveAt(Id_supp);
                }

                List<int> lst_chiffre_possible = new List<int>();
                for (int n = 1; n <= smaller_zone_rest.Nbr_cases_of_zone; n++) // parcours tous les chiffres possible
                {
                    lst_chiffre_possible.Add(n);
                }
                // parcours tous les chiffres possible pour cette zonne
                for (int i = 1; i <= smaller_zone_rest.Nbr_cases_of_zone; i++)
                {
                    Tile my_tile = null;

                    my_tile = Cherche_tile_vierge(smaller_zone_rest, my_tbl_chiffre);

                    if (my_tile != null) // verification que la tuile est valide
                    {
                        int new_chiffre = Cherche_chiffre_correct(my_tile, my_tbl_chiffre, lst_chiffre_possible);
                        my_tbl_chiffre[my_tile.lin_id, my_tile.col_id] = new_chiffre;
                        lst_chiffre_possible.Remove(new_chiffre);
                        nbr_cases_zone_vides--; // case valide donc la case n'est plus vide;
                        /*
                        if (new_chiffre > 0 && new_chiffre <= 9)
                        {
                            
                        }
                        else // CHIFFRE INCORRECT
                        {
                            //Debug.Log("CHIFFRE INCCORRECT !! = " + new_chiffre);
                            nbr_cases_zone_vides = -1; // sort de la boucle d'incrémentation des chiffres
                            Tableau_validate = false;
                            break;
                        }*/
                    }
                    else // TUILE INVALIDE donc la zone est pleine !
                    {
                        break;
                    }
                }
            }
        }

        if (max_loop <= 0)
        {
            Debug.LogError(" LE TABLEAU DE CHIFFRE N'A PAS PU ETRE REMPLIT !!");
            my_tbl_chiffre = null;
        }

        return my_tbl_chiffre;
    }

    private static int Cherche_chiffre_correct(Tile pTile_ref, int[,] my_tbl_chiffre, List<int> pLst_chiffre_possible)
    {
        int chiffre = -1;

        foreach (int chiffre_possible in pLst_chiffre_possible)
        {
            bool conforme = true;
            for (int lin = -1; lin <= 1 && conforme == true; lin++)
            {
                for (int col = -1; col <= 1 && conforme == true; col++)
                {
                    if (lin == 0 && col == 0 && conforme == true) { continue; }
                    if (pTile_ref.lin_id == 0 && lin == -1) { continue; }
                    if (pTile_ref.lin_id == (my_tbl_chiffre.GetLength(0)-1) && lin == 1) { continue; }
                    if (pTile_ref.col_id == 0 && col == -1) { continue; }
                    if (pTile_ref.col_id == (my_tbl_chiffre.GetLength(1) - 1) && col == 1) { continue; }

                    if (my_tbl_chiffre[pTile_ref.lin_id + lin, pTile_ref.col_id + col] == chiffre_possible) // verifier bordure 
                    {
                        conforme = false;
                    }
                }
            } 

            if(conforme == true)
            {
                chiffre = chiffre_possible;
                //Debug.Log("chiffre possible en [" + pTile_ref.lin_id + "," + pTile_ref.col_id + "] : " + chiffre);
                break;
            }
        }
        

        return chiffre;
    }

    private static Tile Cherche_tile_vierge(Zone pZone, int[,] pTbl_chiffre)
    {
        Tile my_tile = null;

        List<int> lst_chiffre_possible = new List<int>();
        while (true)
        {
            my_tile = pZone.lst_zone_Tile[Random.Range(0, pZone.lst_zone_Tile.Count)];
            int chiffre_test = pTbl_chiffre[my_tile.lin_id, my_tile.col_id];

            if (chiffre_test == null_ref) // la tuile est vierge donc stop FOUND
            {
                break; // si on à bien une tuile vide
            }
            else // tuile déjà remplit
            {
                if (!lst_chiffre_possible.Contains(chiffre_test)) // tuile jamais vue
                {
                    lst_chiffre_possible.Add(chiffre_test);
                }
                else // tuile déjà vue donc chiffre présent dans la liste 
                {
                    if (lst_chiffre_possible.Count >= pZone.Nbr_cases_of_zone) // toute la zone est remplit
                    {
                        my_tile = null;
                        break;
                    }
                }
            }
        }

        return my_tile;   // si tile = null, toute la zone est remplit
    }

    private static void Reset_tbl(int[,] pTbl_to_reset, int Null_ref)
    {
        for (int lin = 0; lin < pTbl_to_reset.GetLength(0); lin++)
        {
            for (int col = 0; col < pTbl_to_reset.GetLength(1); col++)
            {
                pTbl_to_reset[lin, col] = Null_ref; // remplit le tableau de -1
            }
        }
    }
    
}

public class Zone
{
    public int IDnum_of_zone { get; private set; }
    public List<Tile> lst_zone_Tile { get; private set;}
    public Tile my_depart { get; private set; }
    public int Nbr_cases_of_zone { get; private set; }

    public Zone(int[,] ptbl_zone, int pId_of_zone, int null_ref = -1) // chenille (a simplifier créer une classe coordonné similaire a vecteur2
    {
        IDnum_of_zone = pId_of_zone;
        //Debug.Log("@@@@@@@@@@@@@ new zone id : " + IDnum_of_zone);

        // recherche le nombre de case maximum en fonction du puzzle
        if (Puzzle_creator.nbr_cases_zone_vides < Puzzle_creator.Chiffre_max) // si il reste peu de case vide
        {
            Nbr_cases_of_zone = Puzzle_creator.Chiffre_max; // on le met au max pour eviter d'avoir beaucoup de 1 cases, sera tronqué 
        }
        else
        {
            Nbr_cases_of_zone = Random.Range(2, Puzzle_creator.Chiffre_max);
        }

        lst_zone_Tile = new List<Tile>(); // contiendra les coordonnées des cases (sera copié dans le tableau definitif)

        // obtien le point de départ (première case sans zone)
        bool depart_found = false;
        for (int lin = 0; lin < ptbl_zone.GetLength(0) && !depart_found; lin++)
        {
            for (int col = 0; col < ptbl_zone.GetLength(1) && !depart_found; col++)
            {
                int idZone_of_case = ptbl_zone[lin, col];
                if (idZone_of_case == null_ref && !depart_found)
                {
                    my_depart = new Tile(lin, col, IDnum_of_zone);
                    lst_zone_Tile.Add(my_depart);
                    ptbl_zone[my_depart.lin_id, my_depart.col_id] = pId_of_zone;
                    depart_found = true;
                }
            }
        }

        // remplie le tableau contenant les cases de la zone (my_zone_coord)
        if (Nbr_cases_of_zone > 1) // s'assure qu'il n'y a pas qu'une seul case (sinon c'est celle de départ)
        {

            int Nbr_cases_validate = 1; // depart déjà placé

            for (int i = 1; i < Nbr_cases_of_zone && Nbr_cases_validate < Nbr_cases_of_zone; i++) // de 1 au (nombre de cases-1) (car depart existe deja)
            {
                // last_case n'intervient que si il y à déjà une case précédente
                Tile last_case = new Tile(-1,-1, IDnum_of_zone);
                if (lst_zone_Tile.Count > 1)
                {
                    last_case = new Tile(lst_zone_Tile[lst_zone_Tile.Count - 2].lin_id, lst_zone_Tile[lst_zone_Tile.Count - 2].col_id, IDnum_of_zone);
                }
                
                Tile current_case = new Tile(lst_zone_Tile[lst_zone_Tile.Count - 1].lin_id, lst_zone_Tile[lst_zone_Tile.Count - 1].col_id, IDnum_of_zone);
                Tile next_case = new Tile(current_case.lin_id, current_case.col_id, IDnum_of_zone);
                List<Tile> Coord_possible = new List<Tile>();

                //Debug.Log("last case n°" + (i + 1) + " [" + current_case.lin_id + "," + current_case.col_id + "]");

                // recherche les cases adjacentes vierges
                if (current_case.lin_id != Puzzle_creator.Nbr_Lin - 1) // sud
                {
                    next_case.lin_id = current_case.lin_id + 1;
                    next_case.col_id = current_case.col_id + 0;

                    Tile coordonnee_possible = Add_new_possibility(ptbl_zone, last_case, next_case, null_ref);
                    if (coordonnee_possible != null)
                    {
                        Coord_possible.Add(coordonnee_possible);
                        //Debug.Log("essai n°" + (i + 1) + ", Sud possible [" + (coordonnee_possible.lin_id) + "," + (coordonnee_possible.col_id) + "]");
                    }
                }
                
                if (current_case.lin_id != 0) // nord
                {
                    next_case.lin_id = current_case.lin_id - 1;
                    next_case.col_id = current_case.col_id + 0;

                    Tile coordonnee_possible = Add_new_possibility(ptbl_zone, last_case, next_case, null_ref);
                    if (coordonnee_possible != null)
                    {
                        Coord_possible.Add(coordonnee_possible);
                        //Debug.Log("essai n°" + (i + 1) + ", Nord possible [" + (coordonnee_possible.lin_id) + "," + (coordonnee_possible.col_id) + "]");
                    }
                }
                
                if (current_case.col_id != Puzzle_creator.Nbr_Col - 1) // est
                {
                    next_case.lin_id = current_case.lin_id + 0;
                    next_case.col_id = current_case.col_id + 1;

                    Tile coordonnee_possible = Add_new_possibility(ptbl_zone, last_case, next_case, null_ref);
                    if (coordonnee_possible != null)
                    {
                        Coord_possible.Add(coordonnee_possible);
                        //Debug.Log("essai n°" + (i + 1) + ", Est possible [" + (coordonnee_possible.lin_id) + "," + (coordonnee_possible.col_id) + "]");
                    }
                }

                if (current_case.col_id != 0) // ouest
                {
                    next_case.lin_id = current_case.lin_id + 0;
                    next_case.col_id = current_case.col_id - 1;

                    Tile coordonnee_possible = Add_new_possibility(ptbl_zone, last_case, next_case, null_ref);
                    if (coordonnee_possible != null)
                    {
                        Coord_possible.Add(coordonnee_possible);
                        //Debug.Log("essai n°" + (i + 1) + ", Ouest possible [" + (coordonnee_possible.lin_id) + "," + (coordonnee_possible.col_id) + "]");
                    }
                }

                if (Coord_possible.Count <= 0) // si il n'y en a aucune on tronque la zone 
                {
                    Nbr_cases_of_zone = Nbr_cases_validate;
                    //Debug.LogWarning("aucune case adjacente disponible ... tronque la zone à " + Nbr_cases_of_zone + " cases");
                    break; // sort de la boucle car aucune possibilité
                }
                else
                {
                    // tant que il y a une coordonné possible, et que l'ont a pas atteind le nombre de case max de la zone
                    while(Coord_possible.Count > 0 && Nbr_cases_validate < Nbr_cases_of_zone)
                    {
                        int choise_alea = Random.Range(0, Coord_possible.Count); // choisi une case vierge aléatoire
                        lst_zone_Tile.Add(Coord_possible[choise_alea]);
                        ptbl_zone[Coord_possible[choise_alea].lin_id, Coord_possible[choise_alea].col_id] = pId_of_zone;
                        Coord_possible.RemoveAt(choise_alea);
                        Nbr_cases_validate++;
                        //Debug.Log("case n°" + Nbr_cases_validate + " [" + my_zone_coord[i].lin_id + "," + my_zone_coord[i].col_id + "]");
                        
                        if (Random.Range(1,100) % 2 == 0) // seulement si le nombre est paire
                        {
                            //Debug.Log("choice no more possibility");
                            break; // si on est sur un nombre paire on arrète d'ajouter une case
                        }
                    }
                }
                
            }
        }
        
    }

    private Tile Add_new_possibility(int[,] ptbl_zone, Tile last_case, Tile next_case, int null_ref)
    {
        Tile coordonnee_possible = null;

        if (ptbl_zone[next_case.lin_id, next_case.col_id] == null_ref)
        {
            if (lst_zone_Tile.Count > 1)
            {
                if (next_case.col_id != last_case.col_id)
                {
                    coordonnee_possible = new Tile(next_case.lin_id, next_case.col_id, IDnum_of_zone);
                    
                }
            }
            else
            {
                coordonnee_possible = new Tile(next_case.lin_id, next_case.col_id, IDnum_of_zone);
            }
        }

        return coordonnee_possible;
    }

}

public class Tile
{
    public int lin_id;
    public int col_id;
    public int Id_zone; 

    public Tile(int pLin, int pCol,int pZone_ID)
    {
        lin_id = pLin;
        col_id = pCol;
        Id_zone = pZone_ID;
    }
}
