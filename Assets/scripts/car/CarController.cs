using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CarStats))]
[RequireComponent(typeof(CarStateMachine))]
public class CarController : MonoBehaviour {

    private CarStateMachine stateMachine;

    public CarStats stats;
    public float maxSteer = 30, wheelbase = 2.5f, trackwidth = 1.5f;

    #region common
    private Vector2 moveInput;
    private bool isSpacebarPressed;
    public bool isShiftPressed;
    private Vector2 lerpedmoveInput;
    #endregion

    private Vector2 Velocity = new Vector2(0, 0);

    #region debug
    [Range(0.1f, 1f)] public float gizmosSphereDiameter = .4f;
    [Range(0, 20)] public float fowDisplaceAmount = 0.1f;
    [Range(0, 20)] public float fowDisplaceLerpSpeed = 1;
    public int inistalFow = 80;
    public bool drawOnlyOnSelected = false;
    #endregion


    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        stats = GetComponent<CarStats>();
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
            
    public void OnJump(InputValue input) => isSpacebarPressed = input.Get<float>() == 1; // this will set the spaccebar pressed bool to true when input is 1

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) stateMachine.cameraindexPos = stateMachine.cameraindexPos < 2 ? stateMachine.cameraindexPos + 1 : 0;
        
    }

    void FixedUpdate() {
        if (!stats) return;

        isSpacebarPressed = Input.GetKey(KeyCode.Space);
        isShiftPressed = Input.GetKey(KeyCode.LeftShift);
        

        HandleSteering();
        HandleEeffects();
        TranslatePwertoWheels();

        stateMachine.KPH = stateMachine.rigidbody.linearVelocity.magnitude * 3.6f;



    }

    void HandleSteering() {

        //concventional input forwading to the wheel turn logic !
        lerpedmoveInput =  Vector2.SmoothDamp(lerpedmoveInput, moveInput , ref Velocity, Time.deltaTime * stats.steerDampSpeed) ;

        if (lerpedmoveInput.x > 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((stats.MaxSteerAngle + stateMachine.steerModifier) + (trackwidth / 2))) * lerpedmoveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((stats.MaxSteerAngle + stateMachine.steerModifier) - (trackwidth / 2))) * lerpedmoveInput.x;
        } else if (lerpedmoveInput.x < 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((stats.MaxSteerAngle + stateMachine.steerModifier) - (trackwidth / 2))) * lerpedmoveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((stats.MaxSteerAngle + stateMachine.steerModifier) + (trackwidth / 2))) * lerpedmoveInput.x;
        } else {
            stateMachine.wheelColliders[0].steerAngle = 0;
            stateMachine.wheelColliders[1].steerAngle = 0;
        }


    }

    // this is where we have setters
    public void SetMoveinput(Vector2 _moveInput) {
        moveInput = _moveInput;
    }

    public void SetSpacebarPressed(bool _moveInput) {
        isSpacebarPressed = _moveInput;
    }

    public void TranslatePwertoWheels() {

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

    public void HandleEeffects() {
        if (isShiftPressed) {

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow + fowDisplaceAmount, fowDisplaceLerpSpeed * Time.deltaTime);
        } else {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow, fowDisplaceLerpSpeed * Time.deltaTime);
        }

    }

    // gizmos
    private void OnDrawGizmosSelected() {
        if (drawOnlyOnSelected) GizmosLogic();
    }

    private void OnDrawGizmos() {
        if (!drawOnlyOnSelected) GizmosLogic();

    }

    void GizmosLogic() {

        //Gizmos.DrawWireSphere(transform.position + stats.lookAtPoint, gizmosSphereDiameter);


    }

#region gui
    [Header("gui")]
    public float GuiXPos = 0;
    public float GuiYPos = 0;
    public float GuiYSpace = 1;
    public float GuiCellWidth = 200;
    public float GuiCellHeight = 20;
    void OnGUI() {
        float pos = GuiYPos;
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), "KPH: " + stateMachine.KPH.ToString("0"));
        pos += 25f;
    }
#endregion

}


[System.Serializable]
public enum WheelType {
    front, rear
}
