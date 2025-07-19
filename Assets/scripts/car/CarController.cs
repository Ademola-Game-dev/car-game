using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;



public class CarController : MonoBehaviour {

    public Rigidbody rigidbody;
    public CarStats carStats;
    public Wheel[] wheels;  //todo will need a reliable way of knowing how many weels are for steering , and how many wheels are for handbrake
    public float maxSteer = 30, wheelbase = 2.5f, trackwidth = 1.5f;
    public float steerModifier = 1;
    //cam

    public CamraController camraController;
    private int cameraindexPos = 0;

    #region common

    private Vector2 moveInput;
    public float wheelTurnLerpSpeed = 1;
    public bool SpacebarPressed;

    #endregion


    void Start() {
        carStats = GetComponent<CarStats>();
        rigidbody = GetComponent<Rigidbody>();
    }


    public void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue input) {
        SpacebarPressed = input.Get<float>() == 1; // this will set the spaccebar pressed bool to true when input is 1
    }

    public void OnInteractJump(InputValue input) {
        print("interacting");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab) && camraController) {
            cameraindexPos = cameraindexPos < 2 ? cameraindexPos + 1 : 0;

            camraController.switchPositions(carStats.camraPositions[cameraindexPos]);
        }

    }

    void FixedUpdate() {

        SpacebarPressed = Input.GetKey(KeyCode.Space); // this will set the spacebar bool to true depending on keypress space

        handleSteering();


        if (!carStats) return;

        //ToDo: add checks if wheels are present

        for (int i = 0; i < wheels.Length; i++) {
            if (i > 1) {
                // rear wheel
                wheels[i].collider.motorTorque = SpacebarPressed ? 0 : moveInput.y * carStats.MaxPowerNM;
                wheels[i].collider.brakeTorque = !SpacebarPressed ? 0 : 1000;
            } else {
                wheels[i].collider.motorTorque = moveInput.y * carStats.MaxPowerNM;

                //fron wheels for now
            }
        }

        for (int i = 0; i < wheels.Length; i++) {
            Quaternion Rot;
            Vector3 Pos;
            wheels[i].collider.GetWorldPose(out Pos, out Rot);

            Transform[] ChildTranforms = new Transform[wheels[i].collider.transform.childCount];
            int index = 0;
            foreach (var item in ChildTranforms) {
                wheels[i].collider.transform.GetChild(index).position = Pos;
                wheels[i].collider.transform.GetChild(index).rotation = Rot;
                index++;
            }

            //ToDo: this can be used to rotate the brake calipers
            //wheels[i].collider.transform.localRotation = Quaternion.Euler(0 , wheels[i].collider.transform.rotation.eulerAngles.y, 0);
        }

    }


    void handleSteering() {


        if (moveInput.x > 0 ) {
            wheels[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
            wheels[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
        } else if (moveInput.x < 0 ) {                                                          
            wheels[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
            wheels[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
        } else {
            wheels[0].collider.steerAngle =0;
            wheels[1].collider.steerAngle =0;
        }
    



        //velocity = rigidbody.linearVelocity.magnitude;


    }

}

[System.Serializable]
public class Wheel {
    public WheelCollider collider;
    public WheelType wheelType;
}


[System.Serializable]
public enum WheelType {
    front, rear
}