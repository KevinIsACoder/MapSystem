using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Emapcelltype
{
    Round = 0,  //地块
    Grass = 1, //草
    House = 2 //房子
}

public class MapCell
{
    public Vector3 cellPos { get; private set; }
    public Emapcelltype mapType; //地块类型
    
}
