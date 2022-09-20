using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clic_on_chiffre : MonoBehaviour
{
    [SerializeField]
    private int my_Chiffre = -1;

    // Start is called before the first frame update
    void Start()
    {
        if(my_Chiffre < 0 || my_Chiffre > 9)
        {
            my_Chiffre = int.Parse(this.transform.name);
        }
    }

    private void OnMouseDown()
    {
        //Debug.Log("Clic on button chiffre : " + my_Chiffre);
        puzzle_manager.Add_new_chiffre(my_Chiffre);
    }
}
