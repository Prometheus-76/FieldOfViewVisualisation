using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    #region Inspector

    [Header("Components")]
    public Transform playerTransform;
    public PaintSystem paintSystem;
    public PlayerMovement playerMovement;

    [Header("Starting State")]
    [Range(0f, 1f)]
    public float startingPaint;
    public WeaponProfile startingWeaponProfile;

    [Header("Configuration")]
    public BrushProfile playerBrushProfile;
    [Min(1f)]
    public float maxPaint;

    #endregion

    // PROPERTIES
    public float currentPaint { get; private set; } = 0f;
    public WeaponProfile currentProfile { get; private set; }

    // PRIVATE
    private LinkedList<Bullet> availableAmmo;
    private List<Bullet> allAmmo;

    // Start is called before the first frame update
    void Start()
    {
        availableAmmo = new LinkedList<Bullet>();
        allAmmo = new List<Bullet>();

        currentPaint = Mathf.Clamp(maxPaint * startingPaint, 0f, maxPaint);
        currentProfile = startingWeaponProfile;
    }

    // Update is called once per frame
    void Update()
    {
        PaintUpdate(Time.deltaTime);

        WeaponUpdate(Time.deltaTime);
    }

    #region Public Methods

    /// <summary>
    /// Returns paint to the player's reserves
    /// </summary>
    /// <param name="paintAmount">The amount of paint to add</param>
    /// <param name="paintColour">The colour of the added paint</param>
    public void AddPaint(float paintAmount, Color paintColour)
    {
        // Play sound/animation?
    }

    /// <summary>
    /// Take some paint from the player's reserve
    /// </summary>
    /// <param name="paintAmount">The amount we want to remove</param>
    /// <returns>The amount we actually removed</returns>
    public float RemovePaint(float paintAmount)
    {
        float originalPaint = currentPaint;

        // Remove from the current amount, not dropping below zero
        currentPaint -= paintAmount;
        currentPaint = Mathf.Max(currentPaint, 0f);

        // Return how much we actually removed
        float paintRemoved = originalPaint - currentPaint;
        return paintRemoved;
    }

    #endregion

    private void PaintUpdate(float deltaTime)
    {
        paintSystem.EraseFromAll(playerTransform.position, playerBrushProfile);
    }

    private void WeaponUpdate(float deltaTime)
    {

    }
}
