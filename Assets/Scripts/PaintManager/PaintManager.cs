using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject paintPrefab;
    public List<Material> paintMaterials;
    public List<Color> paintColours;

    [Header("Performance / Stability")]
    [Range(0, 100)]
    public int initialPaintReserve = 0;
    public bool preventDuplicateReturns = false;

    // PRIVATE
    private LinkedList<MaskablePaint> availablePaint;
    private List<MaskablePaint> allPaint;

    private CommandBuffer commandBuffer = null;
    private float totalRemoved = 0f;

    private void Start()
    {
        Initialise();

        MaskablePaint paint = ExtractFromPool();
        paint.gameObject.SetActive(true);
        paint.transform.parent = null;
        paint.Splatter();
    }

    private void Update()
    {
        Vector2 cursorWorldPos = Camera.main.ScreenToWorldPoint(InputManager.GetAimPosition().Value);
        EraseFromAll(cursorWorldPos, 0.5f, 0.2f, 0.5f);

        totalRemoved += allPaint[0].ComputeRemovalDelta();
    }

    // Setup the paint manager
    public void Initialise()
    {
        availablePaint = new LinkedList<MaskablePaint>();
        allPaint = new List<MaskablePaint>();

        commandBuffer = new CommandBuffer();

        // Create initial pool size
        for (int i = 0; i < initialPaintReserve; i++)
        {
            ReturnToPool(CreatePaint());
        }
    }

    // Erase at a position in the world from all paint objects below the brush
    public void EraseFromAll(Vector2 brushPosition, float brushRadius, float brushHardness, float brushStrength)
    {
        // Broad -> narrow phase to find all applicable paint objects, erase them as we go
        int eraseCount = 0;
        for (int i = 0; i < allPaint.Count; i++)
        {
            // If active and splattered
            if (allPaint[i].gameObject.activeSelf && allPaint[i].isReset == false)
            {
                // If the distance to the bounding box is less than the brush radius...
                if (true)
                {
                    // Check if brush radius overlaps with shape perimeter as well?
                    if (true)
                    {
                        allPaint[i].SubmitEraseCommand(commandBuffer, brushPosition, brushRadius, brushHardness, brushStrength);
                        eraseCount += 1;
                    }
                }
            }
        }

        if (eraseCount > 0)
        {
            // Then execute the commandbuffer and clear it
            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
        }
    }

    // Get a paint instance from the open pool, creating one if necessary
    public MaskablePaint ExtractFromPool()
    {
        // If there is paint available
        if (availablePaint.Count > 0)
        {
            // Remove and return an item from the open pool
            MaskablePaint paintInstance = availablePaint.First.Value;
            availablePaint.RemoveFirst();

            return paintInstance;
        }

        // Create a new instance and return that instead
        return CreatePaint();
    }

    // Returns a paint instance to the open pool
    public void ReturnToPool(MaskablePaint paintToRelease)
    {
        if (preventDuplicateReturns && availablePaint.Contains(paintToRelease)) return;

        // Return it to the pool
        availablePaint.AddLast(paintToRelease);

        // Store as inactive child
        paintToRelease.transform.parent = transform;
        paintToRelease.gameObject.SetActive(false);
    }

    // Creates a new paint object and return it
    private MaskablePaint CreatePaint()
    {
        // Create the paint object
        GameObject paintObject = Instantiate(paintPrefab);
        MaskablePaint paintComponent = paintObject.GetComponent<MaskablePaint>();

        // Add to complete pool
        allPaint.Add(paintComponent);

        // Store as inactive child
        paintObject.transform.parent = transform;
        paintObject.SetActive(false);

        // Initialise this paint
        paintComponent.Initialise(paintMaterials[0], "_MainTex", "_Color");

        return paintComponent;
    }
}
