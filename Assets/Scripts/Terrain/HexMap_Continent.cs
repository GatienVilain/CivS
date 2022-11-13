using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexMap_Continent : HexMap
{
    public override void GenerateMap()
    {
        //First call the base version to make all the hexes we need
        base.GenerateMap();

        int numContinents = 3;
        int continentSpacing = numberColumns / numContinents;
        for(int c = 0; c < numContinents; c++)
        {
            //Make some kind of raised area
            Random.InitState(DateTime.Now.Second + DateTime.Now.Millisecond);
            int numSplats = Random.Range(4, 8);
            for (int i = 0; i < numSplats; i++)
            {
                int range = Random.Range(5, 8);
                int y = Random.Range(range, numberRows - range);
                int x = Random.Range(0, 10) - y / 2 + (c*continentSpacing);

                elevateArea(x, y, range);
            }
        }

        //elevateArea(21, 15, 6);
        //elevateArea(16, 21, 6);
        //elevateArea(24, 5, 6);

        //Add lumpiness Perlin NOise?
        float noiseResolution = 0.01f;
        Vector2 noiseOffset = new Vector2(Random.Range(0f,1f), Random.Range(0f, 1f));
        float noiseScale = 1.5f;
        for (int column = 0; column < numberColumns; column++)
        {
            for (int row = 0; row < numberRows; row++)
            {
                Hex h = getHexeAt(column, row);
                float n = Mathf.PerlinNoise(((float)column / Mathf.Max(numberColumns, numberRows) / noiseResolution) + noiseOffset.x, 
                    ((float)row / Mathf.Max(numberColumns, numberRows) / noiseResolution) + noiseOffset.y) 
                    - 0.5f;
                h.Elevation += n * noiseScale;
            }

        }

        //Set mesh to mounter/hill/flat/water based on height

        //Simulate rainfall/moisture (probably just Perlin it for now) and set plain/granssslands + forest

        noiseResolution = 0.05f;
        noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        noiseScale = 2f;
        for (int column = 0; column < numberColumns; column++)
        {
            for (int row = 0; row < numberRows; row++)
            {
                Hex h = getHexeAt(column, row);
                float n = Mathf.PerlinNoise(((float)column / Mathf.Max(numberColumns, numberRows) / noiseResolution) + noiseOffset.x,
                    ((float)row / Mathf.Max(numberColumns, numberRows) / noiseResolution) + noiseOffset.y)
                    - 0.5f;
                h.Moisture = n * noiseScale;
            }

        }



        //Met-�-jour les Visuels (voir HexMap.cs)
        UpdateHexVisuals();

        GeneratePathfindingGraph(); //G�n�re le graphe des voisins pour le pathfinding (voir HexMap.cs)

        SpawnUnitAt(0, worker, 35, 16);
    }

    private void elevateArea(int q, int r, int range, float centerHeight = 0.8f)
    {
        Hex centerHex = getHexeAt(q, r);

        //centerHex.Elevation = 0.5f;

        Hex[] areaHexes = getHexesWithinRangeOf(centerHex, range);
        foreach(Hex h in areaHexes)
        {
            /*if(h.Elevation < 0)
            {
                h.Elevation = 0;
            }*/
            h.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(Hex.Distance(centerHex, h)/range,2f));
        }
    }
}
