using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeapon : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform playerTransform;
    public PaintSystem paintSystem;
    public PlayerMovement playerMovement;

    [Header("Configuration")]
    public WeaponProfile startingWeaponProfile;
    public Camera mainCamera;

    [Header("Paint")]
    public BrushProfile playerBrushProfile;

    [Header("Bullet Pool")]
    public GameObject bulletPrefab;
    public bool preventDuplicateReturns = false;
    public bool cullExcessBullets = false;

    #endregion

    // PROPERTIES
    public float currentPaint { get; private set; }
    public WeaponProfile currentProfile { get; private set; }

    // PRIVATE
    private LinkedList<Bullet> availableBullets;
    private List<Bullet> allBullets;

    private bool primaryFireInputHeld = false;
    private bool secondaryFireInputHeld = false;
    private Vector2 fireDirection = Vector2.up;
    private Vector2? firePosition = Vector2.zero;

    // Start is called before the first frame update
    private void Start()
    {
        BulletPoolStart();

        PaintStart();

        SetWeaponProfile(startingWeaponProfile);
    }

    // Update is called once per frame
    private void Update()
    {
        InputUpdate(Time.deltaTime);

        PaintUpdate(Time.deltaTime);

        FiringUpdate(Time.deltaTime);
    }

    #region Public Methods

    public void SetWeaponProfile(WeaponProfile newProfile)
    {
        if (newProfile == currentProfile) return;
        currentProfile = newProfile;

        // Update all bullets in the pool, and adjust the pool size if necessary
        ReconfigureBulletPool(currentProfile);
    }

    #endregion
}
