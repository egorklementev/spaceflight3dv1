using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemManager : MonoBehaviour {
    
    [Header("Grid Size")]
    [Range(4, 12)]
    public int gridSizeX = 8;
    [Range(4, 12)]
    public int gridSizeY = 5;
    [Space(10)]

    [Header("Gem's parameters")]
    public float moveTime = 1f;
    public float fallTime = 1f;
    public float scaleSpeed = 1f;
    public float scaleFactor = 1.25f;
    public float destroyTime = 25f;
    public float destructionForce = 20f;
    [Space(10)]

    [Header("Gem's number")]
    [Range(3, 8)]
    public int gemsAvailable = 3;
    [Range(3, 5)]
    public int sequenceSize = 3;
    [Space(10)]

    [Header("Gem's bonuses")]
    [Range(0, 20)]
    public int meteorBonusChance = 10;
    public float meteorSpeed = 1f;
    public Vector3 meteorOffset = new Vector3(0, 0, 0);
    public int meteorsPerBonus = 1;
    [Space(10)]

    [Header("Prefabs")]
    public GameObject[] gemObjs;
    public GameObject[] dGemObjs;
    public GameObject[] meteorGems;
    public GameObject[] dMeteorGems;
    public GameObject meteorPrefab;
    [Space(10)]
    
    [HideInInspector]
    public bool areGemsActing = false;
    [HideInInspector]
    public bool isBonusActing = false;
    [HideInInspector]
    public int score;
    public static Cell[,] grid;
    
    // Related to mouse selection
    private Vector2 firstSelected;
    private Vector2 secondSelected;
    private Vector2 UNSELECTED = new Vector2(-1, -1);

    // Check for deleting the sequence of the gems
    private bool needToCheckGrid = false;

    // Bonuses
    private int meteorBlasts = 0;
    private List<Vector2> avPositions = new List<Vector2>();
    
    // Colors of the gems
    private int[] gemIndices;
    private Gem.GemColor[] colors;

    // Size & position of the gems
    private float gemSize = 1.5f;
    private float gemOffset = 1.5f;
    private float pixelSize;
    private float angularSize;

    // ------------------------------- //

    private void Awake()
    {
        LoadOptionsData();

        grid = new Cell[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                grid[x, y] = new Cell();
            }
        }
        
        colors = new Gem.GemColor[8];
        colors[0] = Gem.GemColor.BLUE;
        colors[1] = Gem.GemColor.DIAMOND;
        colors[2] = Gem.GemColor.GREEN;
        colors[3] = Gem.GemColor.MAGENTA;
        colors[4] = Gem.GemColor.ORANGE;
        colors[5] = Gem.GemColor.RED;
        colors[6] = Gem.GemColor.WHITE;
        colors[7] = Gem.GemColor.YELLOW;

        gemIndices = new int[gemsAvailable];
        for (int i = 0; i < gemsAvailable; i++)
        {
            gemIndices[i] = Utils.GetRandomInt(0, 8);
        }
        Utils.ResetRandomIntList();

        firstSelected = UNSELECTED;
        secondSelected = UNSELECTED;

        GameObject gemExample = Instantiate(gemObjs[0], transform);
        float diameter = gemExample.GetComponent<Collider>().bounds.extents.magnitude;
        float distance = Vector3.Distance(gemExample.transform.position, Camera.main.transform.position);        
        angularSize = (diameter / distance) * Mathf.Rad2Deg;
        
        UpdateGemTransform();
        Destroy(gemExample);
        SpawnGemGrid();
    }

    private void Update(){
        // Check if something happening to prevent input
        areGemsActing = false; 
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                if (!grid[x, y].IsEmpty && grid[x, y].GemInside.IsActing)
                {
                    areGemsActing = true;
                    break;
                }
            }
            if (areGemsActing)
            {
                break;
            }
        }

        // Grid generation
        if (Input.GetKeyDown("space") && !isBonusActing && !areGemsActing)
        {
            foreach (Cell cell in grid)
            {
                if (cell.GemInside != null)
                {
                    Destroy(cell.GemInside.GameObj);
                }
            }
            SpawnGemGrid(); 
        }

        // Gem resize
        if (Input.GetKeyDown(KeyCode.R) && !isBonusActing && !areGemsActing)
        {
            foreach (Cell cell in grid)
            {
                if (cell.GemInside != null)
                {
                    Destroy(cell.GemInside.GameObj);
                }
            }
            UpdateGemTransform();
            SpawnGemGrid();
        }

        // Bonuses handling
        if (meteorBlasts > 0 && !areGemsActing && !isBonusActing)
        {
            isBonusActing = true;
            avPositions.Clear();
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (!grid[x, y].IsEmpty)
                    {
                        avPositions.Add(new Vector2(x, y));
                    }
                }
            }
            for (int i = 0; i < meteorBlasts; ++i)
            {
                StartCoroutine(SpawnMeteor(meteorBlasts - (i + 1), .25f * i));
            }
            meteorBlasts = 0;
        }
    }

    private void LateUpdate()
    {
        if (!isBonusActing)
        {
            bool wasEmpty = false;
            for (int x = 0; x < gridSizeX; ++x)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (grid[x, y].IsEmpty)
                    {
                        wasEmpty = true;
                        GenerateNewGems();
                        break;
                    }
                }
                if (wasEmpty)
                {
                    break;
                }
            }
        }

        if (!areGemsActing && needToCheckGrid)
        {
            needToCheckGrid = false;
            CheckGemGrid();
        }
        
    }

    // ------------------------------- //

    // Scales selected gem 
    public void GemSelectionFirst(RaycastHit hitInfo)
    {
        int x = hitInfo.transform.gameObject.GetComponent<GemInfo>().xPos;
        int y = hitInfo.transform.gameObject.GetComponent<GemInfo>().yPos;
        Vector2 curSelected = new Vector2(x, y);

        if (firstSelected.Equals(UNSELECTED))
        {
            StartCoroutine(grid[x, y].GemInside.Scale(scaleFactor, scaleSpeed));
            firstSelected = curSelected;
        }
        else if (firstSelected.Equals(curSelected))
        {
            StartCoroutine(grid[x, y].GemInside.ScaleToLocal(scaleSpeed));
            firstSelected = UNSELECTED;
        }
        else
        {
            int sX = (int) firstSelected.x;
            int sY = (int) firstSelected.y;            
            StartCoroutine(grid[sX, sY].GemInside.ScaleToLocal(scaleSpeed));
            StartCoroutine(grid[x, y].GemInside.Scale(scaleFactor, scaleSpeed));
            firstSelected = curSelected;           
        }
    }

    // Scales second selected gem if it is valid 
    public void GemSelectionSecond(RaycastHit hitInfo)
    {
        int x = hitInfo.transform.gameObject.GetComponent<GemInfo>().xPos;
        int y = hitInfo.transform.gameObject.GetComponent<GemInfo>().yPos;
        Vector2 curSelected = new Vector2(x, y);

        if (!firstSelected.Equals(curSelected))
        {
            if (secondSelected.Equals(UNSELECTED) && IsValidPosition(curSelected, firstSelected))
            {
                StartCoroutine(grid[x, y].GemInside.Scale(scaleFactor, scaleSpeed));
                secondSelected = curSelected;   
            }
            else if (!secondSelected.Equals(curSelected) && IsValidPosition(curSelected, firstSelected))
            {
                int sX = (int)secondSelected.x;
                int sY = (int)secondSelected.y;
                StartCoroutine(grid[sX, sY].GemInside.ScaleToLocal(scaleSpeed));
                StartCoroutine(grid[x, y].GemInside.Scale(scaleFactor, scaleSpeed));
                secondSelected = curSelected;
            }
        }
        else if (!secondSelected.Equals(UNSELECTED))
        {
            int sX = (int)secondSelected.x;
            int sY = (int)secondSelected.y;
            StartCoroutine(grid[sX, sY].GemInside.ScaleToLocal(scaleSpeed));
            secondSelected = UNSELECTED;
        }
    }

    // Resets both gems, after LMB is up and if there were two selected, performs swapping
    public void GemSelectionReset()
    {
        if (!firstSelected.Equals(UNSELECTED) && !secondSelected.Equals(UNSELECTED))
        {
            SwapGems(firstSelected, secondSelected);

            int x = (int)firstSelected.x;
            int y = (int)firstSelected.y;
            StartCoroutine(grid[x, y].GemInside.ScaleToLocal(scaleSpeed));
            firstSelected = UNSELECTED;

            x = (int)secondSelected.x;
            y = (int)secondSelected.y;
            StartCoroutine(grid[x, y].GemInside.ScaleToLocal(scaleSpeed));
            secondSelected = UNSELECTED;
        }
        if (!firstSelected.Equals(UNSELECTED))
        {
            int x = (int)firstSelected.x;
            int y = (int)firstSelected.y;
            StartCoroutine(grid[x, y].GemInside.ScaleToLocal(scaleSpeed));
            firstSelected = UNSELECTED;
        }
        if (!secondSelected.Equals(UNSELECTED))
        {
            int x = (int)secondSelected.x;
            int y = (int)secondSelected.y;
            StartCoroutine(grid[x, y].GemInside.ScaleToLocal(scaleSpeed));
            secondSelected = UNSELECTED;
        }
    }
    
    public void GemSelectionDebug(RaycastHit hitInfo)
    {
        // Debug code
    }

    // ------------------------------- //

    // Fills the entire grid with new gems
    private void SpawnGemGrid()
    {
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                Vector3 position = transform.position;
                position.x += x * gemOffset;
                position.y += y * gemOffset;

                SpawnGem(x, y, position);

            }
        }

        needToCheckGrid = true;
    }

    // Generates new gems if there are empty cells
    private void GenerateNewGems() {
        for (int x = 0; x < gridSizeX; ++x)
        {
            int cellsToMoveDown = 0;
            for (int y = 0; y < gridSizeY; y++)
            {
                if (grid[x, y].IsEmpty)
                {
                    cellsToMoveDown++;
                }
                else
                {
                    grid[x, y - cellsToMoveDown].IsEmpty = false;
                    grid[x, y - cellsToMoveDown].GemInside = grid[x, y].GemInside;
                    grid[x, y - cellsToMoveDown].GemInside.GameObj.GetComponent<GemInfo>().xPos = x;
                    grid[x, y - cellsToMoveDown].GemInside.GameObj.GetComponent<GemInfo>().yPos = y - cellsToMoveDown;
                    StartCoroutine(grid[x, y].GemInside.Move(
                        new Vector2(x, y - cellsToMoveDown),
                        transform.position,
                        gemOffset,
                        fallTime));
                }
            }
            for (int y = gridSizeY - cellsToMoveDown; y < gridSizeY; y++)
            {
                float offset = (y - (gridSizeY - cellsToMoveDown)) * gemOffset + gridSizeY * gemOffset; // Magic
                Vector3 position = transform.position;
                position.x += x * gemOffset;
                position.y += offset;

                SpawnGem(x, y, position);

                StartCoroutine(grid[x, y].GemInside.Move(
                    new Vector2(x, y), 
                    transform.position,
                    gemOffset,
                    fallTime));
            }
        }
        areGemsActing = true;
        needToCheckGrid = true;
    }

    // Performs swapping of the given gems
    private void SwapGems(Vector2 first, Vector2 second)
    {
        int fX = (int)first.x;
        int fY = (int)first.y;

        int sX = (int)second.x;
        int sY = (int)second.y;

        StartCoroutine(grid[fX, fY].GemInside.Move(
            new Vector2(sX, sY),
            transform.position,
            gemOffset,
            moveTime));
        StartCoroutine(grid[sX, sY].GemInside.Move(
            new Vector2(fX, fY),
            transform.position,
            gemOffset,
            moveTime));

        grid[fX, fY].GemInside.GameObj.GetComponent<GemInfo>().xPos = sX;
        grid[fX, fY].GemInside.GameObj.GetComponent<GemInfo>().yPos = sY;
        grid[sX, sY].GemInside.GameObj.GetComponent<GemInfo>().xPos = fX;
        grid[sX, sY].GemInside.GameObj.GetComponent<GemInfo>().yPos = fY;

        Gem tempGem = grid[fX,fY].GemInside;
        grid[fX,fY].GemInside = grid[sX,sY].GemInside;
        grid[sX, sY].GemInside = tempGem;

        needToCheckGrid = true;
    }

    // Checks the entire grid for the 3+ sequences
    private void CheckGemGrid()
    {
        bool[,] gemsToDestroy = new bool[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                gemsToDestroy[x, y] = false;
            }
        }

        // Rows
        for (int y = 0; y < gridSizeY; ++y)
        {
            int curX = 0;
            int currentEqual = 1;
            while (curX < (gridSizeX - 1))
            {
                while (curX < (gridSizeX - 1) &&
                    grid[curX, y].GemInside.Color.Equals(grid[curX + 1, y].GemInside.Color))
                {
                    currentEqual++;
                    curX++;
                }

                if (currentEqual >= sequenceSize)
                {
                    for (int x = curX - currentEqual + 1; x <= curX; ++x)
                    {
                        gemsToDestroy[x, y] = true;
                    }
                }

                currentEqual = 1;
                curX++;
            }
        }

        // Columns
        for (int x = 0; x < gridSizeX; ++x)
        {
            int curY = 0;
            int currentEqual = 1;
            while (curY < (gridSizeY - 1))
            {
                while (curY < (gridSizeY - 1) &&
                    grid[x, curY].GemInside.Color.Equals(grid[x, curY + 1].GemInside.Color))
                {
                    currentEqual++;
                    curY++;
                }

                if (currentEqual >= sequenceSize)
                {
                    for (int y = curY - currentEqual + 1; y <= curY; ++y)
                    {
                        gemsToDestroy[x, y] = true;
                    }
                }

                currentEqual = 1;
                curY++;
            }
        }

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (gemsToDestroy[x, y])
                {
                    DestroyGem(x, y);
                }
            }
        }
    }

    // ------------------------------- //

    // Destroys given gem, and spawns another one above
    public void DestroyGem(int x, int y)
    {
        if (!grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().isBonus)
        {
            DestroyRegular(x, y);
        } else
        {
            switch(grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().bonusId)
            {
                case 1:
                    DestroyMeteor(x, y);
                    break;
              //case 2:
            }
        }
    }

    private void DestroyRegular(int x, int y)
    {
        score += 10;

        GameObject brokenGem = Instantiate(
            dGemObjs[grid[x, y].GemInside.ColorIndex],
            grid[x, y].GemInside.GameObj.transform.position,
            grid[x, y].GemInside.GameObj.transform.rotation);

        brokenGem.transform.localScale = new Vector3(gemSize, gemSize, gemSize);

        Vector3 forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject firstPart = brokenGem.transform.Find("1").gameObject;
        firstPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject secondPart = brokenGem.transform.Find("2").gameObject;
        secondPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject thirdPart = brokenGem.transform.Find("3").gameObject;
        thirdPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        brokenGem.transform.parent = transform;

        Destroy(grid[x, y].GemInside.GameObj);
        Destroy(brokenGem, destroyTime * Time.deltaTime);
        grid[x, y].GemInside = null;
        grid[x, y].IsEmpty = true;
    }

    private void DestroyMeteor(int x, int y)
    {
        score += 20;

        meteorBlasts += meteorsPerBonus;

        GameObject brokenGem = Instantiate(
            dMeteorGems[grid[x, y].GemInside.ColorIndex],
            grid[x, y].GemInside.GameObj.transform.position,
            grid[x, y].GemInside.GameObj.transform.rotation);

        brokenGem.transform.localScale = new Vector3(gemSize, gemSize, gemSize);

        Vector3 forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject firstPart = brokenGem.transform.Find("1").gameObject;
        firstPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject secondPart = brokenGem.transform.Find("2").gameObject;
        secondPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject thirdPart = brokenGem.transform.Find("3").gameObject;
        thirdPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        GameObject meteorPart = brokenGem.transform.Find("DestroyedMeteor").gameObject;

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject firstMeteorPart = meteorPart.transform.Find("1").gameObject;
        firstMeteorPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        forceDirection = new Vector3(
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce),
            Random.Range(-destructionForce, destructionForce)
            );
        GameObject secondMeteorPart = meteorPart.transform.Find("2").gameObject;
        secondMeteorPart.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

        brokenGem.transform.parent = transform;

        Destroy(grid[x, y].GemInside.GameObj);
        Destroy(brokenGem, destroyTime * Time.deltaTime);
        grid[x, y].GemInside = null;
        grid[x, y].IsEmpty = true;
    }

    // Returns whether it is possible to move gem in given direction
    private bool IsValidPosition(Vector2 curSel, Vector2 prevSel)
    {
        bool leftValid = prevSel.x > 0 && curSel.x == prevSel.x - 1 && curSel.y == prevSel.y;
        bool rightValid = prevSel.x < (gridSizeX - 1) && curSel.x == prevSel.x + 1 && curSel.y == prevSel.y;
        bool upValid = prevSel.y < (gridSizeY - 1) && curSel.y == prevSel.y + 1 && curSel.x == prevSel.x;
        bool downValid = prevSel.y > 0 && curSel.y == prevSel.y - 1 && curSel.x == prevSel.x;

        return leftValid || rightValid || downValid || upValid;
    }

    // ------------------------------- //

    private void SpawnGem(int x, int y, Vector3 position)
    {
        // First part for meteor
        // Second part for regular
        // Third part for regular

        int randomNum = Random.Range(1, 101);

        if (randomNum <= meteorBonusChance)
        {
            SpawnMeteorGem(x, y, position);
        } else
        {
            SpawnRegularGem(x, y, position);
        }
    }

    private void SpawnRegularGem(int x, int y, Vector3 position)
    {
        int index = gemIndices[Random.Range(0, gemsAvailable)];

        grid[x, y].GemInside = new Gem(
            Instantiate(gemObjs[index], position, transform.rotation),
            colors[index]);
        grid[x, y].GemInside.GameObj.transform.parent = transform;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().xPos = x;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().yPos = y;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().isBonus = false;
        grid[x, y].GemInside.GameObj.transform.localScale = new Vector3(gemSize, gemSize, gemSize);
        grid[x, y].GemInside.LocalScale = grid[x, y].GemInside.GameObj.transform.localScale;
        grid[x, y].IsEmpty = false;
    }

    private IEnumerator SpawnMeteor(int i, float delay)
    {
        yield return new WaitForSeconds(delay);

        int index = Random.Range(0, avPositions.Count);
        Vector2 randPos = avPositions[index];
        int x = (int)randPos.x;
        int y = (int)randPos.y;
        avPositions.RemoveAt(index);
        
        Vector3 position = grid[x, y].GemInside.GameObj.transform.position;
        position += meteorOffset;

        GameObject meteor = Instantiate(meteorPrefab);
        meteor.transform.SetPositionAndRotation(position, meteor.transform.rotation);
        meteor.transform.localScale = new Vector3(gemSize, gemSize, gemSize);

        if (i == 0)
        {
            meteor.name = "lastMeteor";
        }

        position -= meteorOffset;
        StartCoroutine(MeteorFall(meteor.transform, x, y, meteorSpeed));
    }
    private void SpawnMeteorGem(int x, int y, Vector3 position)
    {
        int index = gemIndices[Random.Range(0, gemsAvailable)];

        grid[x, y].GemInside = new Gem(
            Instantiate(meteorGems[index], position, transform.rotation),
            colors[index]);
        grid[x, y].GemInside.GameObj.transform.parent = transform;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().xPos = x;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().yPos = y;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().isBonus = true;
        grid[x, y].GemInside.GameObj.GetComponent<GemInfo>().bonusId = 1;
        grid[x, y].GemInside.GameObj.transform.localScale = new Vector3(gemSize, gemSize, gemSize);
        grid[x, y].GemInside.LocalScale = grid[x, y].GemInside.GameObj.transform.localScale;
        grid[x, y].IsEmpty = false;
    }

    // ------------------------------- //

    private void UpdateGemTransform()
    {
        Camera cam = Camera.main;
        pixelSize = ((angularSize * Screen.height) / cam.fieldOfView);
        gemSize = (Mathf.Sqrt((.64f * Screen.width * Screen.height)/(gridSizeX * gridSizeY))) / (2f * pixelSize);
        gemOffset = gemSize * 1.5f;
        float offset = (gridSizeX - 1) * gemOffset;
        transform.SetPositionAndRotation(new Vector3(-(offset / 2f), 1, 5), transform.rotation);
    }

    private void LoadOptionsData()
    {
        gemsAvailable = PlayerPrefs.GetInt("gemNumber", 3);
        sequenceSize = PlayerPrefs.GetInt("seqSize", 3);
        gridSizeX = PlayerPrefs.GetInt("sizeX", 8);
        gridSizeY = PlayerPrefs.GetInt("sizeY", 5);
    }

    private IEnumerator MeteorFall(Transform objectToMove, int x, int y, float speed)
    {
        Vector3 destination = grid[x, y].GemInside.GameObj.transform.position;
        Vector3 start = objectToMove.transform.position;
        float step = (speed / (start - destination).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            if (objectToMove != null)
            {
                t += step;
                if (!grid[x, y].IsEmpty)
                {
                    destination = grid[x, y].GemInside.GameObj.transform.position;
                }
                objectToMove.position = Vector3.Lerp(start, destination, t);
                yield return new WaitForFixedUpdate();
            } else
            {
                break;
            }
            
        }
        if (objectToMove != null)
        {
            objectToMove.position = destination;
        }
    }
    
}
