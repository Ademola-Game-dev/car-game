using UnityEngine;

public class CarStateMachine : MonoBehaviour {



    public WheelCollider[] wheelColliders;
    public Transform[] wheelTransforms;


    void Start() {
        findValues();
    }


    public void findValues() {
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
