using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerWeapon
{
    #region Public Methods

    public void OnPaintErased(float paintAmount, Color paintColour)
    {
        // Called when paint is removed, by the PaintSystem
        RestorePaint(paintAmount / currentProfile.magazinePaintCapacity);
    }

    #endregion

    private void PaintStart()
    {
        currentPaint = 1f;
    }

    private void PaintUpdate(float deltaTime)
    {
        paintSystem.EraseFromAll(playerTransform.position, playerBrushProfile);
    }

    private void RestorePaint(float paintAmount)
    {
        currentPaint += paintAmount;
        currentPaint = Mathf.Min(currentPaint, 1f);
    }

    private float ConsumePaint(float paintAmount)
    {
        float originalPaint = currentPaint;

        // Remove from the current amount, not dropping below zero
        currentPaint -= paintAmount;
        currentPaint = Mathf.Max(currentPaint, 0f);

        // Return how much we actually removed
        float paintRemoved = originalPaint - currentPaint;
        return paintRemoved;
    }
}
