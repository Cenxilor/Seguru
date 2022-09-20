using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Main_script : MonoBehaviour
{

    public static bool Is_just_win = true;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Is_just_win == true && puzzle_manager.Victory == true)
        {
            Debug.LogWarning("VICTOIRE");
            Is_just_win = false;
        }
    }
}
