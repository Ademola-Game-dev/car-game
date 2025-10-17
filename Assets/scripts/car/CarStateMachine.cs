using System.Collections.Generic;
using UnityEngine;

public class CarStateMachine : MonoBehaviour {

    #region StateMachine

    [Header("Car Parts")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;
    public Transform[] exhaustTransforms;
    public ParticleSystem[] nitrus;

    public CarStats CarStats;
    public new Rigidbody rigidbody;

    //camera
    public int cameraindexPos = 0;

    //car
    public float KPH = 0;
    [Tooltip("this will add , not multiply to the overall power of the vehicle")]
    public int boostNm = 0;

    [Header("effects")]
    public int selectedPowerupIndex = 0;
    public List<Powerup> powerups;

    // modifiers are used to modify car properties from helper scripts
    public float steerModifier = 0;
    #endregion

    #region initializer
    void Start() => FindValues();

    public void FindValues() {
        foreach (Transform i in gameObject.transform) {
            if (i.transform.name == "carColliders") {
                wheelColliders = new WheelCollider[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++) {
                    wheelColliders[q] = i.transform.GetChild(q).GetComponent<WheelCollider>();
                }
            }
            if (i.transform.name == "carWheels") {
                wheelTransforms = new Transform[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++) {
                    wheelTransforms[q] = i.transform.GetChild(q);
                }
            }
        }
    }
    #endregion

}

[System.Serializable]
public enum PowerupType {
    nitrus, rocket, shield
}

[System.Serializable]
public struct Powerup {
    public PowerupType type;
    public int index;
}