using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Paint")]
    public BrushProfile playerBrushProfile;
    public float magazineCapacity;

    // PROPERTIES
    public float currentPaint { get; private set; } = 1f;

    private void PaintUpdate(float deltaTime)
    {
        SuctionPaint();
    }

    private void SuctionPaint()
    {
        // Suck up paint under the player
        paintSystem.EraseFromAll(playerTransform.position, playerBrushProfile);
    }

    // Called from PaintSystem event
    public void OnPaintErased(float paintAmount, Color paintColour)
    {
        // Called when paint is removed, by the PaintSystem
        AddPaint(paintAmount / magazineCapacity);
    }

    public float RemovePaint(float amount)
    {
        float originalPaint = currentPaint;

        // Remove from the current amount, not dropping below zero
        currentPaint -= Mathf.Max(amount, 0f);
        currentPaint = Mathf.Max(currentPaint, 0f);

        // Return how much we actually removed
        float paintRemoved = originalPaint - currentPaint;
        return paintRemoved;
    }

    public float AddPaint(float amount)
    {
        float originalPaint = currentPaint;

        // Add to the current amount, capping at 1
        currentPaint += Mathf.Max(amount, 0f);
        currentPaint = Mathf.Min(currentPaint, 1f);

        // Return how much was actually added
        float paintAdded = currentPaint - originalPaint;
        return paintAdded;
    }

    public void SetPaint(float amount)
    {
        currentPaint = Mathf.Clamp01(amount);
    }
}
