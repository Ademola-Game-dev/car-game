
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(CarStateMachine))]
public class CarStats : MonoBehaviour {

    public CarStateMachine carStateMachine;
    public bool drawOnlyOnSelected = false;

    [Header("camera stats")]
    public List<Vector2> camraPositions = new List<Vector2>(3);
    public Vector2 lookAtPoint= new Vector2(.42f, 1.5f);

    [Header("car stats")]
    public driveMode driveMode = driveMode.allWheelDrive;
    [Range(120, 2500)]
    public int MaxPowerNM = 1200;  //  use NM as newton meters , thats how unity car collider works
    [Tooltip("lower = more steer")]
    [Range(2, 5)] public float MaxSteerAngle = 4;
    [Tooltip("less = steers quicker , not very controllable when too fast , suggested 14")]
    [Range(1,20)] public float initialSteerDampSpeed = 1;
    [Tooltip("this will simply scale based on the car speed , slower turns at high speed !  , suggested 0.2f")]
    [Range(0, .5f)] public float steerDampMultiplier = 1.5f;
    [HideInInspector]public float steerDampSpeed = 0;


    [Header("gui")]
    [Range(0, .3f)] public float wireSphereRadius = .2f;


    // local variables

    [ContextMenu("Set Camera Position")]
    public void SetCameraPosition() {
        if (carStateMachine == null) {
            try {
                carStateMachine = GetComponent<CarStateMachine>();
            } catch {
                throw new System.Exception("no state machine attached to car");
            }
        }
        if (Camera.main != null) {
            Camera.main.transform.position = new Vector3(transform.position.x + 0, transform.position.y + camraPositions[carStateMachine.cameraindexPos].y, transform.position.z + camraPositions[carStateMachine.cameraindexPos].x);
            Debug.Log("Camera position set!");
        } else {
            Debug.LogError("Camera or target position not found!");
        }
    }

    //init valu
    private Vector2 initialLookAtPoint;

    void Awake() {
        initialLookAtPoint = lookAtPoint;
    }

    void Start() {
        carStateMachine = GetComponent<CarStateMachine>();
    }

    // temp variables
    [Header("temp , for the cam lerping !")]
    public float lerpVal = 0;

    void FixedUpdate() {
        lookAtPoint = Vector2.Lerp(lookAtPoint , new Vector2(initialLookAtPoint.x , Mathf.Clamp(carStateMachine.KPH / 60, 0 , 1) +  initialLookAtPoint.y ), Time.deltaTime * lerpVal);

    }



    // gizmos
    private void OnDrawGizmosSelected() {
        if (drawOnlyOnSelected) gizmosLogic();
    }

    private void OnDrawGizmos() {
        if (!drawOnlyOnSelected) gizmosLogic();

    }

    void gizmosLogic() {
        if (carStateMachine == null) {
            try {
                carStateMachine = GetComponent<CarStateMachine>();
            } catch {
                throw new System.Exception("no state machine attached to car");
            }
        }
        foreach (var item in camraPositions) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(0, item.y, item.x)), wireSphereRadius);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(new Vector3(0, lookAtPoint.y, lookAtPoint.x)), wireSphereRadius);


    }


}



[System.Serializable]
public enum driveMode {
    frontWheelDrive,
    rearWheelDrive,
    allWheelDrive
}


#if UNITY_EDITOR

[CustomEditor(typeof(CarStats))]
public class CameraPositionSetterEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CarStats script = (CarStats)target;
        if (GUILayout.Button("Set Camera Position")) {
            script.SetCameraPosition();
        }
    }
}
#endif