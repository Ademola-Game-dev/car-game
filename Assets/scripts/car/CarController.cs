using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CarStateMachine))]
public class CarController : MonoBehaviour {

    #region variables , inspector
    private CarStateMachine stateMachine;

    public Transform centerOfMassTransform;

    [Range(0.1f, 1f)] public float gizmosSphereDiameter = .4f;
    [Range(0, 20)] public float fowDisplaceAmount = 0.1f;
    [Range(0, 20)] public float fowDisplaceLerpSpeed = 1;
    public int inistalFow = 80;
    public bool drawOnlyOnSelected = false;


    //private

    private bool isSpacebarPressed;
    private bool isShiftPressed;
    private bool usingNitrus = false;
    #endregion

    #region main
    void Start() {
        stateMachine = GetComponent<CarStateMachine>();
        stateMachine.rigidbody.centerOfMass = centerOfMassTransform.localPosition;
        InitializePowerupsUi();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) stateMachine.cameraindexPos = stateMachine.cameraindexPos < 2 ? stateMachine.cameraindexPos + 1 : 0;

        isSpacebarPressed = Input.GetKey(KeyCode.Space);
        isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyUp(KeyCode.E)) CyclePowerupIndex();
        if (Input.GetKeyUp(KeyCode.F)) UsePowerUp();

        TranslatePwertoWheels();

    }

    void FixedUpdate() {
        HandleEeffects();
        stateMachine.KPH = stateMachine.rigidbody.linearVelocity.magnitude * 3.6f;
    }

    public void TranslatePwertoWheels() {
        return;
        switch (stateMachine.CarStats.driveMode) {
            case driveMode.frontWheelDrive:
                stateMachine.wheelColliders[0].motorTorque = stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                stateMachine.wheelColliders[1].motorTorque = stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                break;
            case driveMode.rearWheelDrive:
                stateMachine.wheelColliders[2].motorTorque = isSpacebarPressed ? 0 : stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                stateMachine.wheelColliders[3].motorTorque = isSpacebarPressed ? 0 : stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                break;
            case driveMode.allWheelDrive:
                for (int i = 0; i < stateMachine.wheelColliders.Length; i++) {
                    if (i > 1) {
                        stateMachine.wheelColliders[i].motorTorque = isSpacebarPressed ? 0 : stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                    } else {
                        stateMachine.wheelColliders[i].motorTorque = stateMachine.moveInput.y * (stateMachine.CarStats.MaxPowerNM + stateMachine.boostNm);
                    }
                    stateMachine.wheelColliders[i].brakeTorque = stateMachine.moveInput.y < 0.0 ? Mathf.Abs(stateMachine.moveInput.y) * 100000 : 0;
                }
                break;
        }

        stateMachine.wheelColliders[2].brakeTorque = !isSpacebarPressed ? 0 : 200000;
        stateMachine.wheelColliders[3].brakeTorque = !isSpacebarPressed ? 0 : 200000;

        //this is world axis
        //stateMachine.rigidbody.AddForce(transform.up * (downforceMultiplier * stateMachine.KPH));

        //this should be local axis
        stateMachine.rigidbody.AddForce(-stateMachine.rigidbody.transform.up * stateMachine.CarStats.downforceCurve.Evaluate(stateMachine.KPH), ForceMode.Force);
        stateMachine.rigidbody.angularDamping = Mathf.Lerp(stateMachine.rigidbody.angularDamping, stateMachine.KPH * stateMachine.CarStats.angularVelocityMultiplier, Time.deltaTime * 3);
        stateMachine.rigidbody.linearDamping = Mathf.Lerp(stateMachine.rigidbody.linearDamping, stateMachine.KPH * stateMachine.CarStats.linearVelocityMultiplier, Time.deltaTime * 3);

    }


    // camera effects on shift press
    public void HandleEeffects() {
        if (Camera.main == null) return;

        if (isShiftPressed || usingNitrus) {
            stateMachine.boostNm = stateMachine.CarStats.boostPowerNM;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow + fowDisplaceAmount, fowDisplaceLerpSpeed * Time.deltaTime);
            SetExhaust(true);
        } else {
            stateMachine.boostNm = 0;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, inistalFow, fowDisplaceLerpSpeed * Time.deltaTime);
            SetExhaust(false);
        }
    }

    void CyclePowerupIndex() {
        if (stateMachine.selectedPowerupIndex < 2) stateMachine.selectedPowerupIndex += 1;
        else stateMachine.selectedPowerupIndex = 0;
        UpdatePowerupsUi();
    }

    void UsePowerUp() {
        for (int i = 0; i < stateMachine.powerups.Count; i++) {
            if (stateMachine.powerups[i].index == stateMachine.selectedPowerupIndex) {
                //Powerup _powerUp = stateMachine.powerups[i];
                switch (stateMachine.powerups[i].type) {
                    case PowerupType.nitrus: UseNitrus(); break;
                    case PowerupType.rocket: UseRocket(); break;
                    case PowerupType.shield: UseShield(); break;
                }
                stateMachine.powerups.RemoveAt(i);
                break;
            }
        }
        UpdatePowerupsUi();
    }

    // input
    public void OnMove(InputValue value) {
        if (gameObject.CompareTag("Player"))
            stateMachine.moveInput = value.Get<Vector2>();
    }
    public void OnJump(InputValue input) {
        if (gameObject.CompareTag("Player"))
            isSpacebarPressed = input.Get<float>() == 1; // this will set the spaccebar pressed bool to true when input is 1
    }

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

    private void UseShield() {
        print("using shield powerup");
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

    #region ui

    public void InitializePowerupsUi() {
        // this will just initialise the place holders for the ui components for the powerups nothings else .
        // not using this count cur players will always have a set amount of powerupds to hold ,
        //var PowerupTypeCount = Enum.GetNames(typeof(PowerupType)).Length;

        float spaceBetween = 120;

        for (int i = 0; i < 3; i++) {
            GameObject newObj = Instantiate(stateMachine.reusablePowerupsUiContainer, stateMachine.powerupsUiContainer.transform);
            RectTransform newRect = newObj.GetComponent<RectTransform>();
            newRect.localPosition = new Vector3((i * spaceBetween) - spaceBetween, newRect.localPosition.y, newRect.localPosition.z);
            stateMachine.reusablePowerupsUiObjects.Add(newRect);
        }

        UpdatePowerupsUi();
    }

    void UpdatePowerupsUi() {

        for (int i = 0; i < stateMachine.reusablePowerupsUiObjects.Count; i++) {

            // this will overlay a ui image to indicate the selected powerup
            stateMachine.reusablePowerupsUiObjects[i].GetComponent<Image>().color = new Color(255, 255, 255, i == stateMachine.selectedPowerupIndex ? 100 : 0);

            for (int j = 0; j < stateMachine.powerups.Count; j++) {
                if (stateMachine.powerups[j].index == i) {
                    Powerup _powerUp = stateMachine.powerups[j];

                    // main enable powerup ui image function !
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(0).transform.localScale = _powerUp.type == PowerupType.rocket ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(1).transform.localScale = _powerUp.type == PowerupType.nitrus ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(2).transform.localScale = _powerUp.type == PowerupType.shield ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);

                    break;
                } else {
                    // setting for when the powerup is used , so it disable the used one !
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(0).transform.localScale = new Vector3(0, 0, 0);
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(1).transform.localScale = new Vector3(0, 0, 0);
                    stateMachine.reusablePowerupsUiObjects[i].GetChild(2).transform.localScale = new Vector3(0, 0, 0);
                }
            }

            // setting for cases when there is no powerups !
            if (stateMachine.powerups.Count == 0) {
                stateMachine.reusablePowerupsUiObjects[i].GetChild(0).transform.localScale = new Vector3(0, 0, 0);
                stateMachine.reusablePowerupsUiObjects[i].GetChild(1).transform.localScale = new Vector3(0, 0, 0);
                stateMachine.reusablePowerupsUiObjects[i].GetChild(2).transform.localScale = new Vector3(0, 0, 0);
            }

        }
    }

    #endregion

    #region gui
    [Header("gui")]
    public float GuiXPos = 0;
    public float GuiYPos = 0;
    public float GuiYSpace = 1;
    public GUIStyle customStyle = new();
    public float GuiCellWidth = 200;
    public float GuiCellHeight = 20;

    void OnGUI() {
        float pos = GuiYPos;
        GUI.Label(new Rect(GuiXPos, pos, GuiCellWidth, GuiCellHeight), "KPH: " + stateMachine.KPH.ToString("0"), customStyle);
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
