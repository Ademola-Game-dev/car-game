using UnityEngine;

public class CarStateMachine : MonoBehaviour {



    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;

    public CarStats CarStats;
    public Rigidbody rigidbody;

    //camera
    public int cameraindexPos = 0;


    //car
    public float KPH = 0;

    // modifires
    // modifiers are used to modify car properties from helper scripts
    public float steerModifier = 0;

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
     


}
