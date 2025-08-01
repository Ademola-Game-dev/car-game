using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;



public class CarController : MonoBehaviour {

    private CarStateMachine stateMachine;

    public Playertype playertype;
    public Camera camera;
    public Rigidbody rigidbody;
    public CarStats stats;
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
    [Range(0.1f,1f)] public float gizmosSphereDiameter = .4f;
    [Range(0, 20)] public float fowDisplaceAmount = 0.1f;
    [Range(0, 20)] public float fowDisplaceLerpSpeed = 1;
    public int inistalFow = 80;
    public bool drawOnlyOnSelected = false;


    #endregion


    [Header("debug")]
    [Range(0.05f, 1f)] public float steerReducingMultiplier = .3f; // use this to steer less when going quicker , prevent crashing

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
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



    }

    void handleSteering() {

        maxSteer = stats.MaxSteerAngle + Mathf.Clamp(steerModifier, 0, 10);

        if (moveInput.x > 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
        } else if (moveInput.x < 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer + (trackwidth / 2))) * moveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (maxSteer - (trackwidth / 2))) * moveInput.x;
        } else {
            stateMachine.wheelColliders[0].steerAngle = 0;
            stateMachine.wheelColliders[1].steerAngle = 0;
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
                stateMachine.wheelColliders[0].motorTorque = moveInput.y * stats.MaxPowerNM;
                stateMachine.wheelColliders[1].motorTorque = moveInput.y * stats.MaxPowerNM;
                break;
            case driveMode.rearWheelDrive:
                stateMachine.wheelColliders[2].motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                stateMachine.wheelColliders[3].motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                break;
            case driveMode.allWheelDrive:
                for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
                    if (i > 1) {
                        // rear wheel
                        stateMachine.wheelColliders[i].motorTorque = isSpacebarPressed ? 0 : moveInput.y * stats.MaxPowerNM;
                        //wheels[i].collider.brakeTorque = !isSpacebarPressed ? 0 : 1000;
                    } else {
                        stateMachine.wheelColliders[i].motorTorque = moveInput.y * stats.MaxPowerNM;

                        //fron wheels for now
                    }
                }
                break;
        }

        stateMachine.wheelColliders[2].brakeTorque = !isSpacebarPressed ? 0 : 1000;
        stateMachine.wheelColliders[3].brakeTorque = !isSpacebarPressed ? 0 : 1000;


    }

    public void handleEeffects() {
        if (isShiftPressed) {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inistalFow + fowDisplaceAmount, fowDisplaceLerpSpeed * Time.deltaTime);
        } else {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, inistalFow, fowDisplaceLerpSpeed * Time.deltaTime);
        }

    }


    // gizmos
    private void OnDrawGizmosSelected() {
        if (drawOnlyOnSelected) gizmosLogic();
    }

    private void OnDrawGizmos() {
        if (!drawOnlyOnSelected) gizmosLogic();

    } 


        void gizmosLogic() {

        //Gizmos.DrawWireSphere(transform.position + stats.lookAtPoint, gizmosSphereDiameter);


    }


}


[System.Serializable]
public enum WheelType {
    front, rear
}

[System.Serializable]
public enum Playertype {
    player, npc
}