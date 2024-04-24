using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Used to add different tiles with their spawn ratios
[System.Serializable]
public class TileWithSpawnRatios
{
    public TileBase tile; //Sprite of a tile
    public float spawnRatio; // Spawn ratio of a tile
}

[System.Serializable]
public class DecorationTile
{
    public TileBase tile; //Sprite of a tile
    public int spawnNumber; //Number of spawns of this tile
}

public class WorldGeneration : MonoBehaviour
{
    public int mapHeight; // Customizable mapHeight
    public int mapWidth; // Customizable mapWidth
    
    public Tilemap ground; //layer for rivers should be on the bottom
    public TileBase[] waterTiles; // list of all water tiles
    public int numberOfRivers; //How many rivers to generate

    public Tilemap grass; // layer for grass
    public TileWithSpawnRatios[] grassTilesWithRatios; // list of all grass tiles with their spawnratios
    

    public Tilemap shores; // Layer for shores should be above river but under everything else
    public RuleTile shoreTiles; // RuleTile for shores

    public Tilemap decoration; // Layer for decoration tiles should be on top
    public DecorationTile[] decorationTiles; // list of all decorationTiles

    public int numberOfForests; // how many forests to generate
    public TileBase[] treesTiles; // lits of possible trees in the forests
    // Start is called before the first frame update
    void Start()
    {
        int[,] map = new int[mapWidth, mapHeight]; // 2 Dementional array for checking if something is placed in this tile
        for (int i = 0; i < numberOfRivers; i++) generate_rivers(ref map); //Add rivers to the map on the ground layer
        generate_shores(ref map); // Generate shores
        generate_ground(grassTilesWithRatios, ref map); //Generate a map with grassTiles
        for (int i = 0; i < numberOfForests; i++) generate_forests(ref map); // Generate numberOfForests forests
        generate_decoration(ref map); // Generate everyhin else
    }

    // Function to generate ground from an array of tiles with their spawn ratios
    public void generate_ground(TileWithSpawnRatios[] ArrayOfTiles, ref int [,] map)
    {
        // Calculate the total sum of spawn ratios for all grass tiles
        float totalSpawnRatio = CalculateTotalSpawnRatio(ArrayOfTiles);

        // For each tile starting from the bottom left and progressing row by row.
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //Randomly select a tile
                TileBase selectedTile = GetRandomTile(ArrayOfTiles, totalSpawnRatio);
                
                // if statement for checking if on coordinates x,y there is no river tile
                if (map[x, y] == 0) grass.SetTile(new Vector3Int(x, y, 0), selectedTile); // place selected grass tile
            }
        }
    }

    // Helper function to calculate the total sum of spawn ratios. Taking array of tiles with ratios as argument.
    private float CalculateTotalSpawnRatio(TileWithSpawnRatios[] ArrayOfTiles)
    {
        float totalSpawnRatio = 0f;
        foreach (var tile in ArrayOfTiles)
        {
            totalSpawnRatio += tile.spawnRatio;
        }
        return totalSpawnRatio;
    }

    //Helper function to randomly choose a random tile. Taking array of tiles with ratios and float of totalSpawnRatio as arguments.
    private TileBase GetRandomTile(TileWithSpawnRatios[] ArrayOfTiles, float totalSpawnRatio)
    {
        //Generate a random value between 0 and totalSpawnRRatio
        float randomValue = Random.Range(0f, totalSpawnRatio);

        // Iterate through tiles from an array to find the selected tile
        foreach (var tile in ArrayOfTiles)
        {
            randomValue -= tile.spawnRatio;
            if (randomValue <= 0)
            {
                return tile.tile;
            }
        }
        // This should not happen if spawn ratios are properly defined
        Debug.LogError("Failed to select a grass tile. Check spawn ratios.");
        return null;
    }

    // Function to generate rivers
    public void generate_rivers(ref int[,] map)
    {
        int[] indicator = { 0, 0 }; // Create an indicator
        int v = 1;
        int h = 1; 
        const int riverWidth = 5; // river width (should be odd number)
        TileBase selectedTile;
        var (spoint, epoint) = createPoints(); //starting and ending points

        // calculate horiznotal and vertical distance
        int[] distance = { epoint[0] - spoint[0], epoint[1] - spoint[1]};

        // check if end point is to the left or right of spoint and if needed set aboslute value of a distance
        // h value indicates to which direction will river move (-1 for to the left, (defult) 1 to the right)
        if (distance[0] < 0)
        {
            distance[0] = 0 - distance[0];
            h = -1;
        }
        // check if end point is to the top or bottom of spoint and if needed set aboslute value of a distance
        // h value indicates to which direction will river move (-1 for to the bottom, (defult) 1 to the top)
        if (distance[1] < 0)
        {
            distance[1] = 0 - distance[1];
            v = -1;
        }

        // Set indicator on the start point
        indicator[0] = spoint[0]; 
        indicator[1] = spoint[1];

        // randomly choose if river will move horizontaly or verticaly
        // if random value x < horiznotal distance it will move horizontaly, else it will move verticaly
        do
        {
            int x = Random.Range(0, distance[0] + distance[1]); //set x to a random value
            if (x < distance[0]) // if random value < horiznotal distance indicator move horizontaly
            {
                if (isInMap(indicator[0], indicator[1])) //for security check if indicator still in map borders (by deafult it is supposed to always be)
                {
                    selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                    ground.SetTile(new Vector3Int(indicator[0], indicator[1], 0), selectedTile); // Place tile on the indicator
                    map[indicator[0], indicator[1]] = 1; // mark where river tile is
                }
                // make river wider
                for (int i = 1; i <= (riverWidth - 1) / 2; i++)
                {
                    // make river wider to the top of indicator
                    if (isInMap(indicator[0], indicator[1] + i)) // check if cooridiantes to place tile still in border
                    {
                        selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                        ground.SetTile(new Vector3Int(indicator[0], indicator[1] + i, 0), selectedTile); // Place tile to the top of the indicator
                        map[indicator[0], indicator[1] + i] = 1; // mark where river tile is
                    }
                    // make river wider to the bottom of indicator
                    if (isInMap(indicator[0], indicator[1] - i)) // check if cooridiantes to place tile still in border
                    {
                        selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                        ground.SetTile(new Vector3Int(indicator[0], indicator[1] - i, 0), selectedTile); // Place tile to the bottom of the indicator
                        map[indicator[0], indicator[1] - i] = 1; // mark where river tile is
                    }
                }
                indicator[0] += h; // move indicator Heriznotaly closer to epoint
                distance[0]--; // lower horizontal distance to epoint


            }
            // move indicator Verticaly closer to epoint right and place tiles
            else // if rando value >= horiznotal distance indicator move Verticaly
            {
                if (isInMap(indicator[0], indicator[1])) //for security check if indicator still in map borders (by deafult it is supposed to always be)
                {
                    selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                    ground.SetTile(new Vector3Int(indicator[0], indicator[1], 0), selectedTile); // Place tile on the indicator
                    map[indicator[0], indicator[1]] = 1; // mark where river tile is
                }
                //make river wider
                for (int i = 1; i<= (riverWidth-1) / 2; i++)
                {
                    // make river wider to the right of indicator
                    if (isInMap(indicator[0] + i, indicator[1])) // check if cooridiantes to place tile still in border
                    {
                        selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                        ground.SetTile(new Vector3Int(indicator[0] + i, indicator[1], 0), selectedTile); // Place tile to the right of the indicator
                        map[indicator[0] + i, indicator[1] ] = 1; // mark where river tile is
                    }
                    // make river wider to the left of indicator
                    if (isInMap(indicator[0] - i, indicator[1])) // check if cooridiantes to place tile still in border
                    {
                        selectedTile = waterTiles[Random.Range(0, waterTiles.Length)]; // Randomly select a tile
                        ground.SetTile(new Vector3Int(indicator[0] - i, indicator[1], 0), selectedTile); // Place tile to the left of the indicator
                        map[indicator[0] - i, indicator[1]] = 1; // mark where river tile is
                    }
                }
                indicator[1] += v; // move indicator Verticaly closer to epoint
                distance[1]--; // lower Vertical distance to epoint

            }

        } while (distance[0] + distance[1] > 0); // do it until reaching and epoint
    }

    // Helper function for creating starting and ending points
    private (int[], int[]) createPoints()
    {
        int[] spoint = {0,0}; //starting point as an array
        int[] epoint = {0,0}; //ending point as an array
        int p = Random.Range(0, 2); // generate starting and ending points (0 for points being on Vertical edges of the map, 1 for points being on Horiznotal edges of the map)
        switch (p)
        {
            // Creat starting and ending points on Vertical edges of the map
            case 0:
                {
                    spoint = new int[] { Random.Range(4, mapWidth-4), 0 };
                    epoint = new int[] { Random.Range(4, mapWidth-4), mapHeight};
                    break;
                }
            // Creat starting and ending points on Horiznotal edges of the map
            case 1:
                {
                    spoint = new int[] { 0, Random.Range(4, mapHeight-4) };
                    epoint = new int[] { mapWidth, Random.Range(4, mapHeight-4) };
                    break;
                }
            }
        return (spoint, epoint);
    }

    // Helper function for checking if coordinates x, y are in the map
    private bool isInMap(int x, int y)
    {
        if (x < 0 || x > mapWidth-1 || y < 0 || y > mapHeight-1) return false;
        else return true;
    }

    // function to generate shores
    public void generate_shores(ref int[,] map)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[x,y] == 0)
                {
                    if (x - 1 > 0 && y - 1 > 0 && map[x -1,y -1] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x - 1, y - 1, 0)) shores.SetTile(new Vector3Int(x -1, y -1, 0), shoreTiles);
                    }
                    if (y-1 > 0 && map[x, y -1] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x, y -1, 0)) shores.SetTile(new Vector3Int(x, y -1, 0), shoreTiles);
                    }
                    if (x + 1 < mapWidth && y-1 > 0 && map[x + 1, y - 1] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x + 1, y - 1, 0)) shores.SetTile(new Vector3Int(x + 1, y - 1, 0), shoreTiles);
                    }
                    if (x - 1 > 0 && map[x - 1, y] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x - 1, y, 0)) shores.SetTile(new Vector3Int(x - 1, y, 0), shoreTiles);
                    }
                    if (x + 1 < mapWidth && map[x + 1, y] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x + 1, y, 0)) shores.SetTile(new Vector3Int(x + 1, y, 0), shoreTiles);
                    }
                    if (x - 1 > 0 && y + 1 < mapHeight && map[x -1, y + 1] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x - 1, y + 1, 0)) shores.SetTile(new Vector3Int(x - 1, y + 1, 0), shoreTiles);
                    }
                    if (y + 1 < mapHeight && map[x, y + 1] == 1)
                    {
                        if (IsTileEmpty(x,y,0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x, y + 1, 0)) shores.SetTile(new Vector3Int(x, y + 1, 0), shoreTiles);
                    }
                    if (x + 1 < mapWidth && y + 1 < mapHeight && map[x + 1, y + 1] == 1)
                    {
                        if (IsTileEmpty(x, y, 0)) shores.SetTile(new Vector3Int(x, y, 0), shoreTiles);
                        if (IsTileEmpty(x + 1, y + 1, 0)) shores.SetTile(new Vector3Int(x + 1, y + 1, 0), shoreTiles);
                    }
                }
            }
        }
    }

    // Helper function for checking if tile from coordiantes x, y is empty
    private bool IsTileEmpty(int x, int y, int z = 0)
    {
        Vector3Int position = new Vector3Int(x, y, z);
        return shores.GetTile(position) == null; // if empty returns true
    }

    //Funtion to place everything else (characters, animals, trees, etc.)
    public void generate_decoration(ref int[,] map)
    {
        int x, y;
        //for each decoration tile
        foreach (var tile in decorationTiles)
        {
            // get the size of a decoration tile
            Sprite sprite = (tile.tile as Tile).sprite;
            float width = sprite.bounds.size.x; // divided by 16 as one cell has 16 pixels
            float height = sprite.bounds.size.y; // divided by 16 as one cell has 16 pixels
            // try to spawn spawnNumber number of decoration tile
            for (int i = 0; i < tile.spawnNumber; i++)
            {
                // Choose random position to spawn a tile
                x = Random.Range(0+(int)(width), mapWidth-(int)(width));
                y = Random.Range(0+(int)(height), mapHeight-(int)(height));

                bool shouldplace = true;
                // check every cell that decoration will occupy, if there is something already there
                for (int p = 0; p <width; p++)
                {
                    for (int q = 0; q <height; q++)
                    {
                        if (map[x + p, y + q] != 1)
                        {
                            continue;
                        }
                        else shouldplace = false; // one of the cells is occupied
                    }
                }
                if (shouldplace) // place decoration tile and mark which tile it occupies
                {
                    for (int p = 0; p <width; p++)
                        {
                        for (int q = 0; q < height; q++)
                        {
                            map[x + p, y + q] = 1;
                        }
                    }
                    decoration.SetTile(new Vector3Int(x, y, 0), tile.tile);
                }
            }
        }
    }

    public void generate_forests(ref int[,] map)
    {
        int x, y;

        int forestSize = Random.Range(15, 31); // minimum fores size of 15 forest size max size of 30

        //create an indicator
        x = Random.Range(0, mapWidth);
        y = Random.Range(0, mapHeight);

        // until placed as many trees as forestSize
        for (int i = 0; i < forestSize; i++)
        {
            TileBase tree = treesTiles[(Random.Range(0, treesTiles.Length))];
            // get the size of a tree tile
            Sprite sprite = (tree as Tile).sprite;
            float width = sprite.bounds.size.x; // divided by 16 as one cell has 16 pixels
            float height = sprite.bounds.size.y; // divided by 16 as one cell has 16 pixels

            bool shouldplace = true;
            // for every cell that tree will occupy check if it isn;t already occupied
            for (int p = 0; p < width; p++)
            {
                for (int q = 0; q < height; q++)
                {
                    if (isInMap(x + p, y + q)){ //check if still in the map
                        if (map[x + p, y + q] != 1)
                        {
                            continue;
                        }
                        else
                        {
                            shouldplace = false; // one of the cells is occupied
                        }
                    }
                    else shouldplace = false; // if outside of map don;t place

                }
            }
            if (shouldplace) // place tree and mark which tile it occupies
            {
                Debug.Log("Placed a tree");
                for (int p = 0; p < width; p++)
                {
                    for (int q = 0; q < height; q++)
                    {
                        map[x + p, y + q] = 1;
                    }
                }
                decoration.SetTile(new Vector3Int(x, y, 0), tree);
            }
            else
            {
                i--;
            }

            // move the indicator (0 for moveing up, 1 for moveing right, 2 for down, 3 for left)
            int randomMovement = Random.Range(0, 4);
            switch (randomMovement){
                case 0:
                    if (isInMap(x, y + (int)(height))) y += (int)(height);
                    break;
                case 1:
                    if (isInMap(x + (int)(width), y)) x += (int)(width);
                    break;
                case 2:
                    if (isInMap(x, y - (int)(height))) y -= (int)(height);
                    break;
                case 3:
                    if (isInMap(x - (int)(width), y)) x -= (int)(width);
                    break;
            }
        }
    }
}
