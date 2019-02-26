using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChampPiece : MonoBehaviour
{

    public enum ChampType
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
    public struct ChampSprite
    {
        public ChampType champ;
        public Sprite sprite;
        public int valor;
    };

    public ChampSprite[] champSprites;

    private ChampType champ;

    public ChampType Champ
    {
        get { return champ; }
        set { SetChamp(value); }
    }

    public int valor;

    public int Valor
    {
        get { return valor; }
    }

    public int NumChamps
    {
        get { return champSprites.Length; }
    }

    private SpriteRenderer sprite;
    private Dictionary<ChampType, Sprite> champSpriteDict;

    void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        champSpriteDict = new Dictionary<ChampType, Sprite>();

        for (int i = 0; i < champSprites.Length; i++)
        {
            if (!champSpriteDict.ContainsKey(champSprites[i].champ))
            {
                champSpriteDict.Add(champSprites[i].champ, champSprites[i].sprite);
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

    public void SetChamp(ChampType newChamp)
    {
        champ = newChamp;

        if (champSpriteDict.ContainsKey(newChamp))
        {
            sprite.sprite = champSpriteDict[newChamp];
        }
    }
}
