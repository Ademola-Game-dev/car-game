using UnityEngine;

public class WheelAnimator : MonoBehaviour {

    private CarStateMachine stateMachine;


    private Vector3 wheelPosition;
    private Quaternion wheelRotation;

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
    }

    void FixedUpdate() {
        updateWheels();
    }


    
    void updateWheels() {
        for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
            stateMachine.wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            stateMachine.wheelTransforms[i].transform.localRotation = Quaternion.Euler(0, stateMachine.wheelColliders[i].steerAngle, 0);                      //steer rotation
            if (i % 2 != 0) {
                stateMachine.wheelTransforms[i].transform.GetChild(0).transform.Rotate(stateMachine.wheelColliders[i].rpm * -6.6f * Time.deltaTime, 0, 0, Space.Self);   //engine rotation
            } else {
                stateMachine.wheelTransforms[i].transform.GetChild(0).transform.Rotate(stateMachine.wheelColliders[i].rpm * 6.6f * Time.deltaTime, 0, 0, Space.Self);   //engine rotation
            }
            stateMachine.wheelTransforms[i].transform.position = wheelPosition;
        }
    }

}
