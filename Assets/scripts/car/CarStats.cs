
using System.Collections.Generic;
using UnityEngine;

public class CarStats : MonoBehaviour {


    [Header("camera stats")]
    public List<Vector2> camraPositions = new List<Vector2>(3);

    [Header("car stats")]
    public int MaxPowerNM = 1200;  //  use NM as newton meters , thats how unity car collider works
    [Range(1, 5)] public float MaxSteerAngle = 4;
    public driveMode driveMode = driveMode.allWheelDrive;

}



[System.Serializable]
public enum driveMode {
    frontWheelDrive,
    rearWheelDrive,
    allWheelDrive
}
