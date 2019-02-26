using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Bomba : ClearablePiece
{

  
    public enum BombaType
    {
        GREEN,
        LGREEN,
        RED,
        LRED,
        BLUE,
        LBLUE,
        ANY,
        COUNT,
    };

    [System.Serializable]
    public struct BombaSprite
    {
        public BombaType bomba;
        public Sprite sprite;
        public int valor;
    };

    public BombaSprite[] bombaSprites;

    private BombaType bomba;

    public BombaType BombaRef
    {
        get { return bomba; }
        set { SetBomba(value); }
    }

   //public int valor;


    public int NumBombas
    {
        get { return bombaSprites.Length; }
    }

    private SpriteRenderer sprite;
    private Dictionary<BombaType, Sprite> bombaSpriteDict;

    void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        bombaSpriteDict = new Dictionary<BombaType, Sprite>();

        for (int i = 0; i < bombaSprites.Length; i++)
        {
            if (!bombaSpriteDict.ContainsKey(bombaSprites[i].bomba))
            {
                bombaSpriteDict.Add(bombaSprites[i].bomba, bombaSprites[i].sprite);
            }
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetBomba(BombaType newBomba)
    {
        bomba = newBomba;

        if (bombaSpriteDict.ContainsKey(newBomba))
        {
            sprite.sprite = bombaSpriteDict[newBomba];
        }
    }

}
