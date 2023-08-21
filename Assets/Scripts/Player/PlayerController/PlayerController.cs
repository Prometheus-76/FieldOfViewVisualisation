using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Transform playerTransform;
    public CircleCollider2D playerCollider;
    public Rigidbody2D playerRigidbody;

    public PaintSystem paintSystem;
    public Camera mainCamera;
    public RumbleProfile testProfile;
    public RumbleSystem rumbleSystem;
    public float fireInterval;
    public float fireTimer;

    private Coroutine firingRumble = null;

    // Start is called before the first frame update
    private void Start()
    {
        CombatStart();
    }

    IEnumerator FiringRumble()
    {
        while (true)
        {
            rumbleSystem.AddRumbleEvent(testProfile);

            yield return new WaitForSeconds(fireInterval);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        InputUpdate(Time.deltaTime);

        GameplayUpdate(Time.deltaTime);

        CombatUpdate(Time.deltaTime);

        if (fireInputHeld)
        {
            if (firingRumble == null)
            {
                firingRumble = StartCoroutine(FiringRumble());
            }
        }
        else
        {
            if (firingRumble != null)
            {
                StopCoroutine(firingRumble);
                firingRumble = null;
            }
        }
    }

    // FixedUpdate is called once per physics iteration
    private void FixedUpdate()
    {
        MovementFixedUpdate(Time.fixedDeltaTime);
    }
}
