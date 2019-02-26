using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearX : ClearablePiece


{

    public bool isX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Clear()
    {
        base.Clear();

        if (isX)
        {
            piece.GridRef.ClearX(piece.X, piece.Y);
        }

    }

   
   

}
