using UnityEngine;
using System.Collections;
using System.Collections.Generic; //Access to the dictionary class


public class Grid : MonoBehaviour {

   
   //This is use to know what kind of pieces we can have in it
   public enum PieceType
    {
        EMPTY,
        NORMAL,
        OBSTACLE,
        CHAMP,
        BOMBA,
        TARGET,
        COUNT, //How many kind of piece type we have
    };

    public int xDim;
    public int yDim;
    public float fillTime;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    private Dictionary<PieceType, GameObject> piecePrefabDict;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab; //background for every piece

    private GamePiece[,] pieces;
    private bool inverse = false;

    [System.Serializable] //Our custom struct will show in the inspecto
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };


    // Use this for initialization
    void Start () {

        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for (int i =0; i <piecePrefabs.Length; i++)
        {
            if(!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab); //If the dictionary does not contain the key, we add it in the dictionary
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPrefab,GetPositionGird(x,y), Quaternion.identity);
                background.transform.parent = transform;
             }
        }

        pieces = new GamePiece[xDim, yDim];

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {

                SpawnNewPiece(x,y,PieceType.EMPTY);

            }
        }

        Destroy(pieces[1, 4].gameObject);
        SpawnNewPiece(1, 4, PieceType.OBSTACLE);

        Destroy(pieces[2, 4].gameObject);
        SpawnNewPiece(2, 4, PieceType.OBSTACLE);

        Destroy(pieces[3, 4].gameObject);
        SpawnNewPiece(3, 4, PieceType.OBSTACLE);

        Destroy(pieces[5, 4].gameObject);
        SpawnNewPiece(5, 4, PieceType.OBSTACLE);

        Destroy(pieces[6, 4].gameObject);
        SpawnNewPiece(6, 4, PieceType.OBSTACLE);



        StartCoroutine(Fill());
    }
	

    public Vector2 GetPositionGird(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y);
    }


    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetPositionGird(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;
        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }


    public IEnumerator Fill()
    {


        bool needsRefill = true;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);

            while (FillStep())
            {

                yield return new WaitForSeconds(fillTime);
            }

            needsRefill = ClearAllValidMatches();
        }

    }

    public bool FillStep()
    {
        bool movedPiece = false;

        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;

                if (inverse)
                {
                    x = xDim - 1 - loopX;
                }

                GamePiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;

                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX >= 0 && diagX < xDim)
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];

                                    if (diagonalPiece.Type == PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GamePiece pieceAbove = pieces[diagX, aboveY];

                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.MovableComponent.Move(diagX, y + 1, fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];

            if (pieceBelow.Type == PieceType.EMPTY)
            {


                int random = Random.Range(0, 40);

                if (random == 1)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newChampPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.CHAMP], GetPositionGird(x, -1), Quaternion.identity);
                    newChampPiece.transform.parent = transform;

                    pieces[x, 0] = newChampPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.CHAMP);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ChampComponent.SetChamp((ChampPiece.ChampType)Random.Range(0, pieces[x, 0].ChampComponent.NumChamps));

                    movedPiece = true;
               }else if (random == 2)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newBomba = (GameObject)Instantiate(piecePrefabDict[PieceType.BOMBA], GetPositionGird(x, -1), Quaternion.identity);
                    newBomba.transform.parent = transform;

                    pieces[x, 0] = newBomba.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.BOMBA);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].BombaComponent.SetBomba((Bomba.BombaType)Random.Range(0, pieces[x, 0].BombaComponent.NumBombas));

                    movedPiece = true;
                }
                else if (random == 3)
                {
                    Destroy(pieceBelow.gameObject);
                    GameObject newTarget = (GameObject)Instantiate(piecePrefabDict[PieceType.TARGET], GetPositionGird(x, -1), Quaternion.identity);
                    newTarget.transform.parent = transform;

                    pieces[x, 0] = newTarget.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.TARGET);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].TargetComponent.SetTarget((TargetPiece.TargetType)Random.Range(0, pieces[x, 0].TargetComponent.NumTargets));

                    movedPiece = true;
                }
               
                else
                {

                    Destroy(pieceBelow.gameObject);
                    GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetPositionGird(x, -1), Quaternion.identity);
                    newPiece.transform.parent = transform;

                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPeice.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));

                    movedPiece = true;


                }
              
            }
        }

        return movedPiece;
    }

    public bool isAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1) || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

           

            if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null)
            {

                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                

                StartCoroutine(Fill());

            }
            else
            {

                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;

              
            }
        }
    }
    public void  PressPiece (GamePiece piece)
    {
        pressedPiece = piece;
    }
    public void EnteredPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if (isAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsColored() )
        {
            ColorPeice.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }else { break;
                    
                    }

                  
                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(color)))

                    {
                       //print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(color)))

                    {
                        //print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(color)))

                    {
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(color)))

                    {
                        //print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        if (piece.IsChamp())
        {
            ChampPiece.ChampType champ = piece.ChampComponent.Champ;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(champ)))

                    {
                        print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(champ)))

                    {
                        //print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(champ)))

                    {
                        //print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(champ)))

                    {
                        //print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        if (piece.IsBomba())
        {
            Bomba.BombaType color = piece.BombaComponent.BombaRef ;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(color)))

                    {
                       // print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(color)))

                    {
                       //print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(color)))

                    {
                        //print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(color)))

                    {
                        print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        if (piece.IsTarget())
        {
            TargetPiece.TargetType color = piece.TargetComponent.Target;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(color)))

                    {
                        print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(color)))

                    {
                        print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(color)))

                    {
                        print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(color)))

                    {
                        print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        return null;
    }


    public List<GamePiece> GetMatchSpecial(GamePiece piece, int newX, int newY)
    {
       
        if (piece.IsChamp())
        {
            ChampPiece.ChampType champ = piece.ChampComponent.Champ;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(champ)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(champ)))

                    {
                        print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(champ)))

                    {
                        //print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(champ)))

                    {
                        //print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(champ)))

                    {
                        //print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        if (piece.IsBomba())
        {
            Bomba.BombaType color = piece.BombaComponent.BombaRef;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(color)))

                    {
                        // print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(color)))

                    {
                        //print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(color)))

                    {
                        //print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(color)))

                    {
                        print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        if (piece.IsTarget())
        {
            TargetPiece.TargetType color = piece.TargetComponent.Target;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Para checar primero en el eje horizontal
            horizontalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if (dir == 0)
                    { // izquierda
                        x = newX - xOffset;
                    }
                    else
                    { // derecha
                        x = newX + xOffset;
                    }

                    if (x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if ((pieces[x, newY].IsColored()) && (pieces[x, newY].ColorComponent.Color.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsTarget()) && (pieces[x, newY].TargetComponent.Target.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsChamp()) && (pieces[x, newY].ChampComponent.Champ.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else if ((pieces[x, newY].IsBomba()) && (pieces[x, newY].BombaComponent.BombaRef.Equals(color)))

                    {
                        horizontalPieces.Add(pieces[x, newY]);

                    }
                    else
                    {
                        break;

                    }


                }
            }

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }

            // Si llega aquí, nos toca checar en el eje vertical
            verticalPieces.Add(piece);

            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { // Arriba
                        y = newY - yOffset;
                    }
                    else
                    { // Abajo
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }
                    if ((pieces[newX, y].IsColored()) && (pieces[newX, y].ColorComponent.Color.Equals(color)))

                    {
                        print(pieces[newX, y].ColorComponent.Color);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsTarget()) && (pieces[newX, y].TargetComponent.Target.Equals(color)))

                    {
                        print(pieces[newX, y].TargetComponent.Target);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsChamp()) && (pieces[newX, y].ChampComponent.Champ.Equals(color)))

                    {
                        print(pieces[newX, y].ChampComponent.Champ);
                        verticalPieces.Add(pieces[newX, y]);

                    }
                    else if ((pieces[newX, y].IsBomba()) && (pieces[newX, y].BombaComponent.BombaRef.Equals(color)))

                    {
                        print(pieces[newX, y].BombaComponent.BombaRef);
                        verticalPieces.Add(pieces[newX, y]);

                    }



                    else
                    {
                        break;
                    }
                }
            }

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }
        }
        return null;
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    List<GamePiece> specialMatch = GetMatchSpecial(pieces[x, y], x, y);

                    if (match != null)
                    {

                        for (int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                            }
                        }
                    }
                    if (specialMatch != null)
                    {

                        for (int i = 0; i < specialMatch.Count; i++)
                        {
                            if (ClearPiece(specialMatch[i].X, specialMatch[i].Y))
                            {
                                needsRefill = true;
                            }
                        }
                    }
                }
            }
        }

        return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);

            return true;
        }

        return false;
    }

   public void ClearX(int x, int y)
    {
     


        ClearPiece(x,y);
        ClearPiece(x+1,y+1);
        ClearPiece(x+2,y+2);
        ClearPiece(x - 2, y - 2);
        ClearPiece(x -1, y - 1);


    }


}
