using System;
using UnityEngine;

[Serializable]
public struct BeetleParametor
{
   //{"color": {"r": 136, "g": 144, "b": 164}, "body_len": 55.18,
   //"weight": 112.85, "corner_ratio": 22, "leg_pairs": 9, "is_jagged": 1, "has_roll": 0, "roll_direction": 0}

   public Vector3 color;
   public float BodyLen;
   public float weight;
   public float CornerRatio;
   public float LegPairs;
   public bool isJagged;
   public bool hasRoll;
   public bool roolDirection;
}
