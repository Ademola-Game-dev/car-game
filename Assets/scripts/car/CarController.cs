using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CarStats))]
[RequireComponent(typeof(CarStateMachine))]
public class CarController : MonoBehaviour {

    #region variables , inspector
    private CarStateMachine stateMachine;
    private CarStats stats;

    public Transform centerOfMassTransform;

    [Range(0.1f, 1f)] public float gizmosSphereDiameter = .4f;
    [Range(0, 20)] public float fowDisplaceAmount = 0.1f;
    [Range(0, 20)] public float fowDisplaceLerpSpeed = 1;
    public int inistalFow = 80;
    public bool drawOnlyOnSelected = false;

    //private
    private float wheelbase = 2.5f, trackwidth = 1.5f;
    private Vector2 moveInput;
    private Vector2 lerpedmoveInput;
    private Vector2 Velocity = new(0, 0);
    private bool isSpacebarPressed;
    private bool isShiftPressed;
    private bool isEKeyPressed = false;
    private bool isFKeyPressed = false;
    // powerup variables
    private bool usingNitrus = false;
    //public KeyCode E;
    #endregion

    #region main
    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        stats = GetComponent<CarStats>();

        wheelbase = Vector3.Distance(stateMachine.wheelTransforms[0].position, stateMachine.wheelTransforms[2].position);
        trackwidth = Vector3.Distance(stateMachine.wheelTransforms[0].position, stateMachine.wheelTransforms[1].position);

        stateMachine.rigidbody.centerOfMass = centerOfMassTransform.localPosition;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) stateMachine.cameraindexPos = stateMachine.cameraindexPos < 2 ? stateMachine.cameraindexPos + 1 : 0;

        isSpacebarPressed = Input.GetKey(KeyCode.Space);
        isShiftPressed = Input.GetKey(KeyCode.LeftShift);
        isEKeyPressed = Input.GetKeyUp(KeyCode.E);
        isFKeyPressed = Input.GetKeyUp(KeyCode.F);
        TranslatePwertoWheels();

    }

    void FixedUpdate() {

        HandleSteering();
        HandleEeffects();

        stateMachine.KPH = stateMachine.rigidbody.linearVelocity.magnitude * 3.6f;

    }

    public void TranslatePwertoWheels() {

        switch (stats.driveMode) {
            case driveMode.frontWheelDrive:
                stateMachine.wheelColliders[0].motorTorque = moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);
                stateMachine.wheelColliders[1].motorTorque = moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);
                break;
            case driveMode.rearWheelDrive:
                stateMachine.wheelColliders[2].motorTorque = isSpacebarPressed ? 0 : moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);
                stateMachine.wheelColliders[3].motorTorque = isSpacebarPressed ? 0 : moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);
                break;
            case driveMode.allWheelDrive:
                for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
                    if (i > 1) {
                        // rear wheel
                        stateMachine.wheelColliders[i].motorTorque = isSpacebarPressed ? 0 : moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);
                        //wheels[i].collider.brakeTorque = !isSpacebarPressed ? 0 : 1000;
                    } else {
                        stateMachine.wheelColliders[i].motorTorque = moveInput.y * (stats.MaxPowerNM + stateMachine.boostNm);

                        //fron wheels for now
                    }
                }
                break;
        }

        stateMachine.wheelColliders[2].brakeTorque = !isSpacebarPressed ? 0 : 1000;
        stateMachine.wheelColliders[3].brakeTorque = !isSpacebarPressed ? 0 : 1000;

    }

    void HandleSteering() {
        //concventional input forwading to the wheel turn logic !
        lerpedmoveInput = Vector2.SmoothDamp(lerpedmoveInput, moveInput, ref Velocity, Time.deltaTime * stats.steerDampSpeed);
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

    // camera effects on shift press
    public void HandleEeffects() {

        if (isEKeyPressed) CyclePowerupIndex();
        if (isFKeyPressed) UsePowerUp();

        if (isShiftPressed || usingNitrus) {
            stateMachine.boostNm = stats.boostPowerNM;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow + fowDisplaceAmount, fowDisplaceLerpSpeed * Time.deltaTime);
            SetExhaust(true);
        } else {
            stateMachine.boostNm = 0;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow, fowDisplaceLerpSpeed * Time.deltaTime);
            SetExhaust(false);
        }
    }

    void CyclePowerupIndex() {
        if (stateMachine.powerups.Count > 0) {
            if (stateMachine.selectedPowerupIndex < stateMachine.powerups.Count - 1) {
                stateMachine.selectedPowerupIndex += 1;
            } else {
                stateMachine.selectedPowerupIndex = 0;
            }
        }
    }

    void UsePowerUp() {
        //print("using powerup" + stateMachine.powerups[stateMachine.selectedPowerupIndex].type);
        switch (stateMachine.powerups[stateMachine.selectedPowerupIndex].type) {
            case PowerupType.nitrus: UseNitrus(); break;
            case PowerupType.rocket: UseNitrus(); break;

        }
        stateMachine.powerups.RemoveAt(stateMachine.selectedPowerupIndex);

    }

    // setters , for inputs
    public void SetMoveinput(Vector2 _moveInput) {
        moveInput = _moveInput;
    }

    public void SetSpacebarPressed(bool _moveInput) {
        isSpacebarPressed = _moveInput;
    }

    // input
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnJump(InputValue input) => isSpacebarPressed = input.Get<float>() == 1; // this will set the spaccebar pressed bool to true when input is 1
    #endregion

    #region powerups
    private async Task UseNitrus() {
        usingNitrus = true;
        await Task.Delay(1000 * 5); // this is going to wait for 5 seconds 
        usingNitrus = false;
    }

    private void UseRocket() {
        print("using rocket powerup");
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected() {
        if (drawOnlyOnSelected) GizmosLogic();
    }

    private void OnDrawGizmos() {
        if (!drawOnlyOnSelected) GizmosLogic();

    }

    void GizmosLogic() {

        //Gizmos.DrawWireSphere(transform.position + stats.lookAtPoint, gizmosSphereDiameter);


    }
    #endregion

    #region bus
    public void SetExhaust(bool _exhaust) {
        if (_exhaust) {
            foreach (var item in stateMachine.nitrus) {
                if (!item.isPlaying) {
                    item.Play();
                }
            }
        } else {
            foreach (var item in stateMachine.nitrus) {
                item.Stop();
            }
        }
    }
    #endregion

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
        pos += GuiYSpace;
        //GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), "selected powerup: " + stateMachine.powerups[stateMachine.selectedPowerupIndex].ToString());
        //pos += GuiYSpace;
    }
    #endregion



}

[System.Serializable]
public enum WheelType {
    front, rear
}
