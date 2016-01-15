using UnityEngine;
using System.Collections;
using Sjabloon;

public class Barrel : MonoBehaviour
{
    [SerializeField]
    private float m_TurnSpeed = 10.0f;

    [SerializeField]
    private Gun m_Gun;

    [SerializeField]
    private int m_PlayerID;

    private void Start()
    {
        InitializeControls();
    }

    private void OnDestroy()
    {
    }

    private void InitializeControls()
    {
        //Movement
        InputManager inputManager = InputManager.Instance;
        inputManager.BindAxis("HorizontalBarrelAxis_" + m_PlayerID, m_PlayerID, ControllerAxisCode.RightStickX);
        inputManager.BindAxis("VerticalBarrelAxis_" + m_PlayerID, m_PlayerID, ControllerAxisCode.RightStickY);

        //Shooting
        inputManager.BindAxis("FireTrigger_" + m_PlayerID, m_PlayerID, ControllerAxisCode.RightTrigger);
    }

    private void Update()
    {
        //Rotate the barrel
        float horizInput = -InputManager.Instance.GetAxis("HorizontalBarrelAxis_" + m_PlayerID);
        float vertInput = InputManager.Instance.GetAxis("VerticalBarrelAxis_" + m_PlayerID);

        float currentAngle = transform.rotation.eulerAngles.z;
        float desired = Mathf.Atan2(horizInput, vertInput) * Mathf.Rad2Deg;

        if (horizInput == 0.0f && vertInput == 0.0f && desired == 0.0f) return;

        float tweenAngle = Mathf.LerpAngle(currentAngle, desired, Time.deltaTime * m_TurnSpeed);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, tweenAngle));

        //Fire if required
        float fireTrigger = InputManager.Instance.GetAxis("FireTrigger_" + m_PlayerID);
        if (fireTrigger > 0.1f)
        {
            m_Gun.Fire();
        }
    }
}
