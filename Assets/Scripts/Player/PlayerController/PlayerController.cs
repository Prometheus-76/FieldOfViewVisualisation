using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform playerTransform;
    public CircleCollider2D playerCollider;
    public Rigidbody2D playerRigidbody;

    public PaintSystem paintSystem;
    public Camera mainCamera;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        InputUpdate(Time.deltaTime);

        GameplayUpdate(Time.deltaTime);

        CombatUpdate(Time.deltaTime);
    }

    // FixedUpdate is called once per physics iteration
    private void FixedUpdate()
    {
        MovementFixedUpdate(Time.fixedDeltaTime);
    }
}
