using System;
using UnityEngine;

[System.Serializable]
public class ScanResultData
{
   public string is_succsess; // "OK" または "NO"
    
   // ※以下は、スキャンツールが実際に返してくるパラメーター名に合わせて自由に変更・追加してください
   public ColorRGB color;
   public float body_len;
   public float weight;
   public int corner_ratio;
   public int leg_pairs;
   public bool is_jagged;
   public bool has_roll;
   public bool roll_direction;
}

[System.Serializable]
public class ColorRGB
{
   public int r, g, b;
}
