using System;
using UnityEngine;

[RequireComponent(typeof(CarStateMachine))]
public class SteerManager : MonoBehaviour {

    private CarStateMachine stateMachine;

    [Tooltip("lower = more steer")]
    [Range(2, 5)] public float MaxSteerAngle = 4;
    [Tooltip("more = less steer at high speed !")]
    [Range(0, 0.5f)] public float steeringRadiusModifier;
    [Tooltip("this will add counter steer mod , making it possible to hold the drift and also be able to correct the steer ! , prefered to be set about 10-15 , lowe = less mod = harder to hold the drift")]
    [Range(0, 10)] public float slipSteerRadiusMultiplier = 1;

    [Tooltip("less = steers quicker , not very controllable when too fast , suggested 14")]
    [Range(1, 20)] public float initialSteerDampSpeed = 1;
    [Tooltip("this will simply scale based on the car speed , slower turns at high speed !  , suggested 0.2f")]
    [Range(0, .5f)] public float steerDampMultiplier = 1.5f;

    [Header("visualizers")]
    public float modifiedRadius;
    private float wheelbase = 2.5f, trackwidth = 1.5f;
    public float slipRadiusModifier = 1;
    public float smoothedSidewaysSplipSim;
    private Vector2 lerpedmoveInput;
    private Vector2 Velocity = new(0, 0);
    public float steerDampSpeed;
    public float steerModifier = 0;

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        wheelbase = Vector3.Distance(stateMachine.wheelTransforms[0].position, stateMachine.wheelTransforms[2].position);
        trackwidth = Vector3.Distance(stateMachine.wheelTransforms[0].position, stateMachine.wheelTransforms[1].position);
    }

    void FixedUpdate() {

        steerDampSpeed = initialSteerDampSpeed + (stateMachine.KPH * steerDampMultiplier);
        smoothedSidewaysSplipSim = Mathf.Lerp(smoothedSidewaysSplipSim, stateMachine.overallSidewaysSlip, Time.deltaTime * 4);
        modifiedRadius = Mathf.Lerp(modifiedRadius, stateMachine.KPH * steeringRadiusModifier, Time.deltaTime * 2);
        slipRadiusModifier = Math.Clamp(smoothedSidewaysSplipSim * slipSteerRadiusMultiplier, 0, modifiedRadius);

        steerModifier = modifiedRadius - slipRadiusModifier;


        //concventional input forwading to the wheel turn logic !
        lerpedmoveInput = Vector2.SmoothDamp(lerpedmoveInput, stateMachine.moveInput, ref Velocity, Time.deltaTime * (stateMachine.moveInput.x != 0 ? steerDampSpeed : steerDampSpeed / 3));
        if (lerpedmoveInput.x > 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((MaxSteerAngle + steerModifier) + (trackwidth / 2))) * lerpedmoveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((MaxSteerAngle + steerModifier) - (trackwidth / 2))) * lerpedmoveInput.x;
        } else if (lerpedmoveInput.x < 0) {
            stateMachine.wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((MaxSteerAngle + steerModifier) - (trackwidth / 2))) * lerpedmoveInput.x;
            stateMachine.wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / ((MaxSteerAngle + steerModifier) + (trackwidth / 2))) * lerpedmoveInput.x;
        } else {
            stateMachine.wheelColliders[0].steerAngle = 0;
            stateMachine.wheelColliders[1].steerAngle = 0;
        }



    }

}
