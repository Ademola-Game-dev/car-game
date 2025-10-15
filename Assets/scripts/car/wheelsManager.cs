using System;
using UnityEngine;

//[RequireComponent(typeof(carController))]
public class wheelsManager : MonoBehaviour {

    private CarStateMachine stateMachine;

    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    //carController controller;
    [Header("friction")]
    [Range(.8f, 1.8f)] public float tireGrip = 1;
    [Range(.5f, 3)] public float forwardValue = 1;
    [Range(.5f, 3)] public float sidewaysValue = 2;
    [Range(.2f, 1f)] public float clampMinSlip = .35f;

    [Header("steering")]
    [Tooltip("more = less steer at high speed !")]
    [Range(0, 0.5f)] public float radiusModifier;
    [Tooltip("this will simplty multiply the radiusModifier by this value , steeper curve ")]
    [Range(1, 2)] public float radiusMultiplier = 1.1f;
    [Tooltip("this will add counter steer mod , making it possible to hold the drift and also be able to correct the steer ! , prefered to be set about 10-15 , lowe = less mod = harder to hold the drift")]
    [Range(0,20)]public float slipMultiplier = 1;
    


    private float[] forwardSlip;
    private float[] sidewaysSlip;
    private float[] overallSlip;
    private float[] newStiffnessForward;
    private float[] newStiffnessSideways;
    private float sidewaysSplipSim = 0;

    [Header("visualizers")]
    private float modifiedRadius;
    private float slipRadiusModifier = 1;

    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        SetUpWheels();
    }

    void SetUpWheels() {
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
        ManageFriction();
    }



    void ManageFriction() {

        sidewaysSplipSim = 0;
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
                if(i > 1 ) sidewaysSplipSim += Mathf.Abs(hit.sidewaysSlip); // getting the slip only for the rear wheels , when sideways sliping !
            }
        }
    

        slipRadiusModifier = Mathf.Clamp(Mathf.Abs(sidewaysSplipSim)* slipMultiplier , 0 , modifiedRadius - stateMachine.CarStats.MaxSteerAngle);
        
        modifiedRadius = Mathf.Lerp(modifiedRadius, stateMachine.KPH * (radiusModifier * radiusMultiplier ), Time.deltaTime * 2);

        stateMachine.steerModifier = modifiedRadius - slipRadiusModifier;
        
        stateMachine.CarStats.steerDampSpeed = stateMachine.CarStats.initialSteerDampSpeed + (stateMachine.KPH * stateMachine.CarStats.steerDampMultiplier);

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

        // forwardSlip
        string forwardSlipString = "";
        foreach (float slipValue in forwardSlip) forwardSlipString += slipValue.ToString("0.0") + " ";
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), forwardSlipString.TrimEnd() + " forward");
        pos += GuiYSpace;

        // sidewaysSlip
        string sidewaysSlipString = "";
        foreach (float slipValue in sidewaysSlip) sidewaysSlipString += slipValue.ToString("0.0") + " ";
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), sidewaysSlipString.TrimEnd() + " sideways");
        pos += GuiYSpace;

        // overallSlip
        string overallSlipString = "";
        foreach (float slipValue in overallSlip) overallSlipString += slipValue.ToString("0.0") + " ";
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), overallSlipString.TrimEnd() + " slip");
        pos += GuiYSpace;

        // newStiffnessForward
        string stiffnessForwardString = "";
        foreach (float slipValue in newStiffnessForward) stiffnessForwardString += slipValue.ToString("0.0") + " ";
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), stiffnessForwardString.TrimEnd() + " stiffnes Forward");
        pos += GuiYSpace;

        // newStiffnessSideways
        string stiffnessSidewaysString = "";
        foreach (float slipValue in newStiffnessSideways) stiffnessSidewaysString += slipValue.ToString("0.0") + " ";
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), stiffnessSidewaysString.TrimEnd() + " stiffnes Sideways");
        pos += GuiYSpace; // No increment needed after the last item
        
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), sidewaysSplipSim + " sum of slip");



    }
#endregion

}
