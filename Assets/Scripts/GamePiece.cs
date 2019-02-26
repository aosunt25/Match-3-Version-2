using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{

    private int x;
    private int y;

    public int X
    {
        get { return x; }
        set
        {
            if (IsMovable())
            {
                x = value;
            }
        }
    }

    public int Y
    {
        get { return y; }
        set
        {
            if (IsMovable())
            {
                y = value;
            }
        }
    }


    public bool IsMovable()
    {
        return movableComponent != null;
    }

    public bool IsClearable()
    {
        return clearableComponent != null;
    }

    public bool IsChamp()
    {
        return champComponent != null;
    }
    public bool IsBomba()
    {
        return bombaComponent != null;
    }

    public bool IsTarget()
    {
        return targetComponent != null;
    }

    public bool IsColored()
    {
        return colorComponent != null;
    }

    private Grid.PieceType type;

    public Grid.PieceType Type
    {
        get { return type; }
    }

    private Grid grid;

    public Grid GridRef
    {
        get { return grid; }
    }



    public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }


    private MovePiece movableComponent;

    public MovePiece MovableComponent
    {
        get { return movableComponent; }
    }


    private ColorPeice colorComponent;

    public ColorPeice ColorComponent
    {
        get { return colorComponent; }
    }


    private Bomba bombaComponent;

    public Bomba BombaComponent
    {
        get { return bombaComponent; }
    }

    private ChampPiece champComponent;

    public ChampPiece ChampComponent
    {
        get { return champComponent; }
    }

    private TargetPiece targetComponent;

    public TargetPiece TargetComponent
    {
        get { return targetComponent; }
    }


   

    private ClearablePiece clearableComponent;

    public ClearablePiece ClearableComponent
    {
        get { return clearableComponent; }
    }

    private void Awake()
    {
        movableComponent = GetComponent<MovePiece>();
        colorComponent = GetComponent<ColorPeice>();
        clearableComponent = GetComponent<ClearablePiece>();
        champComponent = GetComponent<ChampPiece>();
        bombaComponent = GetComponent<Bomba>();
        targetComponent = GetComponent<TargetPiece>();

    }

    private void OnMouseEnter()
    {
        grid.EnteredPiece(this);
    }
    private void OnMouseDown()
    {
        grid.PressPiece(this);
    }
    private void OnMouseUp()
    {
        grid.ReleasePiece();
    }
}
