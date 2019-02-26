using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class TargetPiece : ClearablePiece
{


    public enum TargetType
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
    public struct TargetSprite
    {
        public TargetType target;
        public Sprite sprite;
        public int valor;
    };

    public TargetSprite[] targetSprites;

    private TargetType target;

    public TargetType Target
    {
        get { return target; }
        set { SetTarget(value); }
    }

    public int valor;

    public int Valor
    {
        get { return valor; }
    }

    public int NumTargets
    {
        get { return targetSprites.Length; }
    }

    private SpriteRenderer sprite;
    private Dictionary<TargetType, Sprite> targetSpriteDict;

    void Awake()
    {
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();

        targetSpriteDict = new Dictionary<TargetType, Sprite>();

        for (int i = 0; i < targetSprites.Length; i++)
        {
            if (!targetSpriteDict.ContainsKey(targetSprites[i].target))
            {
                targetSpriteDict.Add(targetSprites[i].target, targetSprites[i].sprite);
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

    public void SetTarget(TargetType newTarget)
    {
        target = newTarget;

        if (targetSpriteDict.ContainsKey(newTarget))
        {
            sprite.sprite = targetSpriteDict[newTarget];
        }
    }

}
