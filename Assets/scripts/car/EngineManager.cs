using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CarStateMachine))]
public class Controller : MonoBehaviour {
    private CarStateMachine stateMachine;

    [Header("Gear Settings")]
    [Range(600, 12000)] public float minRPM = 1000f, maxRPM = 7000f;
    [Range(0, 20)] public float engineSmoothDamp = 9f;
    [Range(1, 7)] public float finalDrive = 3.6f;
    [Range(0f, 7f)] public float[] gears;
    public AnimationCurve enginePower;

    public int gearNum = 1;
    public bool reverse = false;
    public float engineRPM;
    private float totalPower;
    private float torquePerWheel;

    [Header("shifter settings")]
    [Tooltip("this will remove rpm from rpm instantly to make the shifting a little more realistic and nice to look at !")]
    [Range(100, 3000)] public float upShiftRpmBounce = 550;

    [Header("Braking")]
    [Range(800, 200000)] public float brakePower = 1000f;
    [Range(100, 200000)] public float handBrakePower = 2000f;
    public float handBrakeFrictionMultiplier = 2f;
    private float handBrakeTorque;
    private List<float> absWheelRPMs = new();  // absolute to only store positive values
    private float wheelsAvgRPM = 0;
    public float throttleInput, brakeInput, velocity;

    //tmp
    public float breakPowerWhenOverMaxRPM = 1000;

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        //  initialize variables
        if (absWheelRPMs.Count != stateMachine.wheelColliders.Length) absWheelRPMs = new List<float>(new float[stateMachine.wheelColliders.Length]);

    }

    void FixedUpdate() {
        automaticShifter();
    }

    private void Update() {
        // Separate throttle and brake inputs more clearly
        throttleInput = Mathf.Clamp01(stateMachine.moveInput.y); // Positive input for throttle
        brakeInput = Math.Abs(Mathf.Clamp(stateMachine.moveInput.y, -1f, 0f));   // Negative input for brake

        CalculateEnginePower();
        MoveVehicle();

        if (Input.GetKeyDown(KeyCode.F)) UpShift();
        if (Input.GetKeyDown(KeyCode.Q)) DownShift();
    }

    private void CalculateEnginePower() {
        WheelRPM();
        wheelsAvgRPM = Mathf.Lerp(wheelsAvgRPM, absWheelRPMs.Average(), Time.deltaTime * 3.5f); // damped average , so we dont get wiggly engine RPM ps. adjust the 3.5f if needed to smooth it even more !
        engineRPM = Mathf.SmoothDamp(engineRPM, gearNum < 1 ? (throttleInput * maxRPM) : (wheelsAvgRPM * finalDrive * gears[gearNum]), ref velocity, Time.deltaTime * (throttleInput > 0 ? engineSmoothDamp : engineSmoothDamp * 10));
        totalPower = finalDrive * (gearNum > 0 ? enginePower.Evaluate(engineRPM) * throttleInput : 0);

    }

    private void WheelRPM() {
        for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
            absWheelRPMs[i] = Mathf.Abs(stateMachine.wheelColliders[i].rpm);
        }

    }

    private void MoveVehicle() {
        // Calculate torque based on drive mode
        int drivenWheels = stateMachine.CarStats.driveMode == driveMode.allWheelDrive ? 4 : 2;
        torquePerWheel = (totalPower / drivenWheels) + stateMachine.boostNm;

        // Apply regular brakes with lerp
        //brakeAmount = Mathf.Lerp(brakeAmount, brakeInput * brakePower, brakeLerpSpeed * Time.deltaTime);

        // Apply handbrake (only to rear wheels)
        handBrakeTorque = Input.GetKey(KeyCode.Space) ? handBrakePower * handBrakeFrictionMultiplier : 0f;

        // Apply torque based on drive mode
        switch (stateMachine.CarStats.driveMode) {
            case driveMode.frontWheelDrive:
                stateMachine.wheelColliders[0].motorTorque = torquePerWheel;
                stateMachine.wheelColliders[1].motorTorque = torquePerWheel;
                stateMachine.wheelColliders[2].motorTorque = 0f;
                stateMachine.wheelColliders[3].motorTorque = 0f;
                break;
            case driveMode.rearWheelDrive:
                stateMachine.wheelColliders[0].motorTorque = 0f;
                stateMachine.wheelColliders[1].motorTorque = 0f;
                stateMachine.wheelColliders[2].motorTorque = torquePerWheel;
                stateMachine.wheelColliders[3].motorTorque = torquePerWheel;
                break;
            case driveMode.allWheelDrive:
                for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
                    stateMachine.wheelColliders[i].motorTorque = engineRPM > maxRPM ? 0 : torquePerWheel;
                    stateMachine.wheelColliders[i].brakeTorque = engineRPM > (maxRPM - 1000) ? (engineRPM - maxRPM - 1000) * 20000 : 0;  // this 2200 should be changed acoardingly , this will stop the car from reving over the max RPM
                }
                break;
        }

        // Apply brakes to all wheels and handbrake to rear wheels
        for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
            // Set brake torque directly for all wheels
            stateMachine.wheelColliders[i].brakeTorque = brakeInput * brakePower;
        }

        // Apply handbrake only to rear wheels
        stateMachine.wheelColliders[2].brakeTorque = handBrakeTorque;
        stateMachine.wheelColliders[3].brakeTorque = handBrakeTorque;
    }

    #region shifter

    void automaticShifter() {
        if (gearNum > 0) {
            // moving >
            if (engineRPM > maxRPM - 1000 && gearNum < gears.Length - 1) UpShift();
            if (engineRPM < minRPM + 2000 && gearNum > (throttleInput == 0 ? 0 : 1)) DownShift();
        } else if (throttleInput > 0) UpShift();
    }

    bool upShiftTimeout = false;
    async void DownShift() {


        gearNum = Mathf.Max(gearNum - 1, 0);
        if (gearNum > 1)
            engineRPM += upShiftRpmBounce;

    }

    async void UpShift() {
        if (upShiftTimeout) {
            //print("not yet available");

            return;
        }
        upShiftTimeout = true;

        gearNum = Mathf.Min(gearNum + 1, gears.Length - 1);
        if (gearNum > 1)
            engineRPM -= upShiftRpmBounce;


        await Task.Delay(500);
        upShiftTimeout = false;
    }

    #endregion

    #region GUI
    [Header("GUI")]
    public float GuiXPos = 0;
    public float GuiYPos = 0;
    public float GuiYSpace = 20;
    public GUIStyle customStyle = new();
    public float GuiCellWidth = 200;
    public float GuiCellHeight = 20;

    void OnGUI() {
        float pos = GuiYPos;
        GUI.HorizontalSlider(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), engineRPM, 0, maxRPM + 1000);
        pos += GuiYSpace;
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), $"Gear: {gearNum} ({gears[gearNum]:F2})", customStyle);
        pos += GuiYSpace;
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), $"RPM: {engineRPM:F0}", customStyle);
        pos += GuiYSpace;
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), $"Total horsepower: {totalPower:F2}", customStyle);
        pos += GuiYSpace;
    }
    #endregion
}