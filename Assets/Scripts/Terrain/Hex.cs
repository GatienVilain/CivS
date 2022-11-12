﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Assertions.Must;
/// <summary>
/// The Hex class defines the grid position, world space position, size,
/// neighbours, etc ... of a Hex Tile. However, it does NOT interact with
/// Unity directly in any way.
/// </summary>
public class Hex
{
    //Q + R + S = 0
    // S = -(Q+R)

    public readonly int Q; //Column
    public readonly int R; //Row
    public readonly int S; //Size

    //Data for map generation and maybe in-game effects
    public float Elevation;
    public float Moisture;
    public bool iswalkable = false;

    //TODO : Need some kind of property to track hex type (plains, grasslands, etc...)
    //TODO : Beed property to track hex detail (forest, mine, farm, etc...)

    float radius = 1f;
    public readonly HexMap hexMap;
    
    HashSet<Unit> units;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    public Hex(HexMap hexMap, int q, int r)
    {
        this.Q = q;
        this.R = r;
        this.S = -(q+r);
        this.hexMap = hexMap;
    }

    /// <summary>
    /// Returns the world-space position of this hex
    /// </summary>
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + this.R / 2f),
            0,
            HexVerticalSpacing() * this.R
        );

        //return new Vector3(HexHorizontalSpacing() * (this.Q + this.R/2f), 0, HexVerticalSpacing() * this.R);
    }

    public Vector3 PositionFromCamera()
    {
        return hexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float numberRows, float numberColumns)
    {
        float mapHeight = numberRows * HexVerticalSpacing();
        float mapWidth = numberColumns * HexHorizontalSpacing();

        Vector3 position = Position();
        if(hexMap.allowWrapEastWest)
        {
            float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;

            //We want howManyWidthsFromCamera to be between -0.5 to 0.5
            /*if(Mathf.Abs(howManyWidthsFromCamera) <= 0.5f)
            {
                return position;
            }*/

            if (howManyWidthsFromCamera > 0)
            {
                howManyWidthsFromCamera += 0.5f;
            }
            else
            {
                howManyWidthsFromCamera -= 0.5f;
            }

            int howManyWidthToFix = (int)howManyWidthsFromCamera;
            position.x -= howManyWidthToFix * mapWidth;
        }

        if(hexMap.allowWrapNorthSouth)
        {
            float howManyWidthsFromCamera = (position.z - cameraPosition.z) / mapHeight;

            //We want howManyWidthsFromCamera to be between -0.5 to 0.5
            /*if(Mathf.Abs(howManyWidthsFromCamera) <= 0.5f)
            {
                return position;
            }*/

            if (howManyWidthsFromCamera > 0)
            {
                howManyWidthsFromCamera += 0.5f;
            }
            else
            {
                howManyWidthsFromCamera -= 0.5f;
            }

            int howManyWidthToFix = (int)howManyWidthsFromCamera;
            position.z -= howManyWidthToFix * mapHeight;
        }
        
        return position;

    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public static float Distance(Hex a, Hex b)
    {
        int dQ = Mathf.Abs(a.Q - b.Q);
        if(dQ > a.hexMap.numberRows/2)
        {
            dQ = a.hexMap.numberColumns - dQ;
        }
        return
            Mathf.Max(
            dQ,
            Mathf.Abs(a.R - b.R),
            Mathf.Abs(a.S - b.S)
            );
    }

    public void AddUnit(Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();

        }
        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        if(units != null)
        {
            units.Remove(unit);
        }
        
    }

    public Unit[] getUnits()
    {
        return units.ToArray();
    }

    public int BaseMovementCost()
    {
        //TODO : Factor in terrain type & features
        return 1;
    }
}
