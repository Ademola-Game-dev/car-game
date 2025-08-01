using UnityEngine;

//[RequireComponent(typeof(carController))]
public class wheelsManager : MonoBehaviour {

    private CarStateMachine stateMachine;
    
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    //carController controller;

    [Range(.8f, 1.8f)] public float tireGrip = 1;
    [Range(.5f, 3)] public float forwardValue = 1;
    [Range(.5f, 3)] public float sidewaysValue = 2;
    [Range(.2f, 1f)] public float clampMinSlip = .35f;


    private float[] forwardSlip;
    private float[] sidewaysSlip;
    private float[] overallSlip;
    private float[] newStiffnessForward;
    private float[] newStiffnessSideways;

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        setUpWheels();
    }

    void setUpWheels() {
        forwardSlip = new float[4];
        sidewaysSlip = new float[4];
        overallSlip = new float[4];
        newStiffnessForward = new float[4];
        newStiffnessSideways = new float[4];
        for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {

            forwardFriction = stateMachine.wheelColliders[i].forwardFriction;

            forwardFriction.asymptoteValue = 1;
            forwardFriction.extremumSlip = 0.065f;
            forwardFriction.asymptoteSlip = 0.8f;
            //curve.stiffness = (inputM.vertical < 0)? ForwardFriction * 2 :ForwardFriction ;
            stateMachine.wheelColliders[i].forwardFriction = forwardFriction;

            sidewaysFriction = stateMachine.wheelColliders[i].sidewaysFriction;

            sidewaysFriction.asymptoteValue = 1;
            sidewaysFriction.extremumSlip = 0.065f;
            sidewaysFriction.asymptoteSlip = 0.8f;
            //curve.stiffness = (inputM.vertical < 0)? SidewaysFriction * 2 :SidewaysFriction ;
            stateMachine.wheelColliders[i].sidewaysFriction = sidewaysFriction;

        }
    }

    void Update() {
        manageFriction();
    }

    public float radiusModifier;
    public float radiusStart;
    public float radiusSlipModifier;

    void manageFriction() {

        WheelHit hit;
        for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
            if (stateMachine.wheelColliders[i].GetGroundHit(out hit)) {
                overallSlip[i] = (Mathf.Abs(hit.forwardSlip) + Mathf.Abs(hit.sidewaysSlip));

                forwardFriction = stateMachine.wheelColliders[i].forwardFriction;
                newStiffnessForward[i] = Mathf.Clamp(tireGrip - (overallSlip[i] / 2) / forwardValue, clampMinSlip, 2);
                forwardFriction.stiffness = newStiffnessForward[i];
                stateMachine.wheelColliders[i].forwardFriction = forwardFriction;

                sidewaysFriction = stateMachine.wheelColliders[i].sidewaysFriction;
                newStiffnessSideways[i] = Mathf.Clamp(tireGrip - (overallSlip[i] / 2) / sidewaysValue, clampMinSlip, 2);
                sidewaysFriction.stiffness = newStiffnessSideways[i];
                stateMachine.wheelColliders[i].sidewaysFriction = sidewaysFriction;

                forwardSlip[i] = hit.forwardSlip;
                sidewaysSlip[i] = hit.sidewaysSlip;
            }
        }

        //controller.radius = radiusStart + (((Mathf.Abs(overallSlip[2]) + Mathf.Abs(overallSlip[3])) / 2) * -radiusSlipModifier) + (controller.KPH * radiusModifier) ;

    }

    void OnGUI() {
        float pos = 50;
        GUI.Label(new Rect(300, pos, 200, 20), "forward: " + " " + forwardSlip[0].ToString("0.0") + " " + forwardSlip[1].ToString("0.0") + " " + forwardSlip[2].ToString("0.0") + " " + forwardSlip[3].ToString("0.0"));
        pos += 25f;
        GUI.Label(new Rect(300, pos, 200, 20), "sideways: " + " " + sidewaysSlip[0].ToString("0.0") + " " + sidewaysSlip[1].ToString("0.0") + " " + sidewaysSlip[2].ToString("0.0") + " " + sidewaysSlip[3].ToString("0.0"));
        pos += 25f;
        GUI.Label(new Rect(300, pos, 200, 20), "slip: " + " " + overallSlip[0].ToString("0.0") + " " + overallSlip[1].ToString("0.0") + " " + overallSlip[2].ToString("0.0") + " " + overallSlip[3].ToString("0.0"));
        pos += 25f;
        GUI.Label(new Rect(300, pos, 200, 20), "stiffnes Forward: " + " " + newStiffnessForward[0].ToString("0.0") + " " + newStiffnessForward[1].ToString("0.0") + " " + newStiffnessForward[2].ToString("0.0") + " " + newStiffnessForward[3].ToString("0.0"));
        pos += 25f;
        GUI.Label(new Rect(300, pos, 200, 20), "stiffnes Sideways: " + " " + newStiffnessSideways[0].ToString("0.0") + " " + newStiffnessSideways[1].ToString("0.0") + " " + newStiffnessSideways[2].ToString("0.0") + " " + newStiffnessSideways[3].ToString("0.0"));
        pos += 25f;
    }

}
