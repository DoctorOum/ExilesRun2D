using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private GameObject cameraObj;

    private Vector3 cameraPos;

    public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (cameraObj == null )
        {
            cameraObj = GetComponentInChildren<Camera>().gameObject;
        }
    }

    void Update()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            MoveRpc();
            //rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener.enabled )
            {
                listener.enabled = false;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void MoveRpc(RpcParams rpcParams = default)
    {
        rb.linearVelocity = moveInput * moveSpeed;
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
