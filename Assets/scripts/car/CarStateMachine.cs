using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CarStats))]
[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(SteerManager))]
[RequireComponent(typeof(Rigidbody))]
public class CarStateMachine : MonoBehaviour {

    #region StateMachine
    [HideInInspector] public CarStats CarStats;

    [Header("Car Parts")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;
    public ParticleSystem[] nitrus;
    [HideInInspector] public new Rigidbody rigidbody;

    //camera
    [HideInInspector] public int cameraindexPos = 0;

    //car
    public float KPH = 0;
    [HideInInspector] public int boostNm = 0; // this will add , not multiply to the overall power of the vehicle

    [Header("effects")]
    public int selectedPowerupIndex = 0;
    public List<Powerup> powerups;


    [Header("ui elements")]
    public GameObject powerupsUiContainer;
    public GameObject reusablePowerupsUiContainer;
    [HideInInspector] public List<RectTransform> reusablePowerupsUiObjects;

    [Header("outside variables")]
    [HideInInspector] public float overallSlip = 0;
    [HideInInspector] public float overallSidewaysSlip = 0;
    [HideInInspector] public float overallForwardSlip = 0;

    [Header("inputs")]
    [HideInInspector] public Vector2 moveInput;
    #endregion

    #region initializer
    void Start() => FindValues();

    public void FindValues() {
        rigidbody = GetComponent<Rigidbody>();
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

    #region getters and setters
    public bool AddPowerup(Powerup p) {
        if (powerups.Count < 3) {
            // this is using the select to basicly map thru the array , the hashset is just a collection of elemets unique . 
            var existingIndices = powerups.Select(pu => pu.index).ToHashSet();
            int nextIndex = 0;
            while (existingIndices.Contains(nextIndex)) {
                nextIndex++;
            }
            Powerup _powerUp = p;
            _powerUp.index = nextIndex;
            powerups.Add(_powerUp);
            return true;
        } else {
            return false;
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