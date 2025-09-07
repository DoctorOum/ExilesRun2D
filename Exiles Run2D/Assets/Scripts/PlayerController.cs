using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;

    private GameObject cameraObj;

    private Vector3 cameraPos;

    void Update()
    {
        if (!IsOwner)
        {

            return;
        }
        else
        {
            if (moveInput.x != 0 || moveInput.y != 0)
            {
                Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;

                MovePlayerServerRpc(moveDirection);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (cameraObj == null)
            {
                cameraObj = GetComponentInChildren<Camera>().gameObject;
            }
        }
        else //if not owner
        {
            #region TurnOffOthersPlayersComponents
            //Don't need other audio listener or Camera acive
            AudioListener listener = GetComponentInChildren<AudioListener>();
            Camera othersCamera = GetComponentInChildren<Camera>();
            PlayerInput otherPlayerInput = GetComponent<PlayerInput>();

            if (listener.enabled)
            {
                listener.enabled = false;
                //Reset listener for when more join
                listener = null;
            }

            if (othersCamera.enabled)
            {
                othersCamera.enabled = false;
                //Reset camera obj to allow others to be shut off when joined
                othersCamera = null;
            }

            if (otherPlayerInput.enabled)
            {
                otherPlayerInput.enabled = false;
            }
            #endregion
        }
    }

    [ServerRpc]
    public void MovePlayerServerRpc(Vector3 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (context.performed)
        {
            
            StartCoroutine(CameraMoveDelay());

            cameraObj.transform.localPosition = cameraPos.normalized;
        }

        if (context.canceled) 
        {
            StartCoroutine(CameraReset());
        }
    }

    IEnumerator CameraMoveDelay()
    {
        float delay = 1f;

        cameraPos = new Vector3(Mathf.Lerp(cameraObj.transform.position.x, moveInput.x * 2, delay), 
                Mathf.Lerp(cameraObj.transform.position.y, moveInput.y * 2, delay), 
                -10);
        yield return new WaitForSeconds(delay);
    }

    IEnumerator CameraReset()
    {
        float delay = 1f;

        cameraObj.transform.localPosition = new Vector3(
            Mathf.Lerp(cameraObj.transform.localPosition.x, 0, delay),
            Mathf.Lerp(cameraObj.transform.localPosition.y, 0, delay),
            -10);

        yield return new WaitForSeconds(delay);
    }
}
