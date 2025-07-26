using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;



public class CarController : MonoBehaviour {

    public Playertype playertype;
    public Camera camera;   
    public Rigidbody rigidbody;
    public CarStats stats;
    public Wheel[] wheels;  //todo will need a reliable way of knowing how many weels are for steering , and how many wheels are for handbrake
    public float maxSteer = 30, wheelbase = 2.5f, trackwidth = 1.5f;
    public float steerModifier = 1;

    public CamraController camraController;
    private int cameraindexPos = 0;

    #region common

    //inputs 
    private Vector2 moveInput;
    private bool isSpacebarPressed;
    public float wheelTurnLerpSpeed = 1;
    public bool isShiftPressed;


    #endregion

    #region debug
    [Range(0,10)]public float fowDisplaceAmount = 0.1f;
    [Range(0,10)]public float fowDisplaceLerpSpeed = 1;
    public int inistalFow = 80;


    #endregion


    [Header("debug")]
    [Range(0.05f, 1f)] public float steerReducingMultiplier = .3f; // use this to steer less when going quicker , prevent crashing

    void Start() {
        stats = GetComponent<CarStats>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value) {
        if (playertype == Playertype.player) {
            moveInput = value.Get<Vector2>();
        }
    }

    public void OnJump(InputValue input) {
        if (playertype == Playertype.player) {
            isSpacebarPressed = input.Get<float>() == 1; // this will set the spaccebar pressed bool to true when input is 1
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab) && camraController) {
            cameraindexPos = cameraindexPos < 2 ? cameraindexPos + 1 : 0;
            camraController.switchPositions(stats.camraPositions[cameraindexPos]);
        }

    }

    void FixedUpdate() {
        if (!stats) return;

        if (playertype == Playertype.player) {
            isSpacebarPressed = Input.GetKey(KeyCode.Space); 
            isShiftPressed = Input.GetKey(KeyCode.LeftShift);
        }

        handleSteering();
        handleEeffects();
        translatePwertoWheels();

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

        maxSteer = stats.MaxSteerAngle + Mathf.Clamp(steerModifier, 0, 10);

        if (moveInput.x > 0) {
            wheels[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
            wheels[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
        } else if (moveInput.x < 0) {
            wheels[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
            wheels[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
        } else {
            wheels[0].collider.steerAngle = 0;
            wheels[1].collider.steerAngle = 0;
        }




        steerModifier = rigidbody.linearVelocity.magnitude * steerReducingMultiplier;


    }

    // this is where we have setters
    public void setMoveinput(Vector2 _moveInput) {
        moveInput = _moveInput;
    }

    public void setSpacebarPressed(bool _moveInput) {
        isSpacebarPressed = _moveInput;
    }


    public void translatePwertoWheels() {

        switch (stats.driveMode) {
            case driveMode.frontWheelDrive:
                wheels[0].collider.motorTorque = moveInput.y * stats.MaxPowerNM;
                wheels[1].collider.motorTorque = moveInput.y * stats.MaxPowerNM;
                break;
            case driveMode.rearWheelDrive:
                wheels[2].collider.motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                wheels[3].collider.motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                break;
            case driveMode.allWheelDrive:
                for (int i = 0; i < wheels.Length; i++) {
                    if (i > 1) {
                        // rear wheel
                        wheels[i].collider.motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                        //wheels[i].collider.brakeTorque = !isSpacebarPressed ? 0 : 1000;
                    } else {
                        wheels[i].collider.motorTorque = moveInput.y * stats.MaxPowerNM;

                        //fron wheels for now
                    }
                }
                break;
        }

        wheels[2].collider.brakeTorque = !isSpacebarPressed ? 0 : 1000;
        wheels[3].collider.brakeTorque = !isSpacebarPressed ? 0 : 1000;


    }

    public void handleEeffects() {
        if (isShiftPressed) {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inistalFow + fowDisplaceAmount, fowDisplaceLerpSpeed * Time.deltaTime);
        } else {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inistalFow, fowDisplaceLerpSpeed * Time.deltaTime);
        }

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

[System.Serializable]
public enum Playertype {
    player, npc
}