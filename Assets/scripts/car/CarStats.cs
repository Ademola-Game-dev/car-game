
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(CarStateMachine))]
public class CarStats : MonoBehaviour {

    #region variables , inspector
    public CarStateMachine carStateMachine;
    public bool drawOnlyOnSelected = false;

    [Header("camera stats")]
    public List<Vector2> camraPositions = new(3);
    public Vector2 lookAtPoint = new(.42f, 1.5f);

    [Header("car stats")]
    public driveMode driveMode = driveMode.allWheelDrive;
    [Range(120, 2500)] public int MaxPowerNM = 1200;  //  use NM as newton meters , thats how unity car collider works
    [Tooltip("this will be used to add as a boost , this will only add power to the overall power of the car !")]
    [Range(0, 1000)] public int boostPowerNM = 100;

    [Header("forces")]
    public AnimationCurve downforceCurve;
    [Tooltip("this will increase the sideways velocity while on high speed , more stability when turning!")]
    [Range(0, 0.003f)] public float angularVelocityMultiplier = 0.0002f;
    [Range(0, 0.001f)] public float linearVelocityMultiplier = 0.0002f;

    [Header("Gizmos")]
    [Range(0, .3f)] public float wireSphereRadius = .2f;

    // holder variables
    private Vector2 initialLookAtPoint;

    // temp variables
    [Header("temp , for the cam lerping !")]
    public float lerpVal = 0;

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
    #endregion

    #region main
    void Awake() {
        initialLookAtPoint = lookAtPoint;
    }

    void Start() {
        carStateMachine = GetComponent<CarStateMachine>();
    }

    void FixedUpdate() {
        lookAtPoint = Vector2.Lerp(lookAtPoint, new Vector2(initialLookAtPoint.x, Mathf.Clamp(carStateMachine.KPH / 60, 0, 1) + initialLookAtPoint.y), Time.deltaTime * lerpVal);

    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected() { if (drawOnlyOnSelected) GizmosLogic(); }

    private void OnDrawGizmos() { if (!drawOnlyOnSelected) GizmosLogic(); }

    void GizmosLogic() {
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
    #endregion

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