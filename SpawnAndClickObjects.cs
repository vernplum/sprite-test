using UnityEngine;
using System.Collections;

public class SpawnAndClickObjects : MonoBehaviour
{
    
    public GameObject objectPrefab; // Prefab of the object to be spawned
    public GameObject dotPrefab; // Prefab for the dot
    public GameObject linePrefab; // Prefab for the line
    public int numberOfObjects = 4; // Number of objects to spawn
    public float spacing = 1.5f; // Spacing between objects
    private GameObject[] spawnedObjects;
    private Vector3[] objectPositions; // Array to store the positions of the objects
    private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

    void Start()
    {
        spawnedObjects = new GameObject[numberOfObjects];
        objectPositions = new Vector3[numberOfObjects]; // Initialize the positions array

        for (int i = 0; i < numberOfObjects; i++)
        {
            float xPos = ((i + 0.5f) - numberOfObjects / 2f) * spacing;
            Vector3 position = new Vector3(xPos, 0, 0);
            GameObject newObject = Instantiate(objectPrefab, position, Quaternion.identity);
            newObject.name = "Object" + (i + 1);
            spawnedObjects[i] = newObject;
            objectPositions[i] = position; // Store the position in the array

            // Set the color of the sprite on the child object
            SpriteRenderer sr = newObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = colors[i % colors.Length]; // Dynamically assign colors
            }

            // Add a BoxCollider if not already present
            if (newObject.GetComponent<Collider>() == null && newObject.GetComponent<Collider2D>() == null)
            {
                newObject.AddComponent<BoxCollider>();
            }

            // Add the ObjectClickHandler component and set the array index and positions array
            ObjectClickHandler clickHandler = newObject.AddComponent<ObjectClickHandler>();
            clickHandler.arrayIndex = i;
            clickHandler.objectPositions = objectPositions; // Pass the positions array to the handler
        }
    }
}


public class ObjectClickHandler : MonoBehaviour
{
    public int arrayIndex;
    public Vector3[] objectPositions; // Array to store the positions of the objects
    private static bool isAnyObjectInteracting = false; // Static flag to track if any object is currently being interacted with
    private static bool isAnyObjectRotating = false; // Static flag to track if any object is currently rotating
    private Vector3 lastMousePosition;
    private Vector3 lastPosition; // Track the last position of the sprite
    private bool isDragging = false;
    private float clickStartTime;
    private const float clickThresholdTime = 0.2f; // Time threshold to distinguish between click and drag
    private const float dragThresholdDistance = 0.1f; // Distance threshold to distinguish between click and drag
    private const int left = 0;
    private const int right = 1;
    private int direction = left;
    private int lastDirection = left + 10;
    private string positionMessage = ""; // Variable to store the position message
    private string lastPositionMessage = ""; // Variable to store the last position message
    private int positionLorR = right; // Variable to store the position relative to the starting position
    private int nextSpriteIndex = -1; // Variable to store the index of the next sprite

    void OnMouseDown()
    {
        if (isAnyObjectInteracting || isAnyObjectRotating)
        {
            // If any object is already being interacted with or rotating, do nothing
            return;
        }

        // Start dragging
        isAnyObjectInteracting = true;
        isDragging = true;
        lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition.z = 0; // Ensure the z-position is maintained
        lastPosition = transform.position; // Store the initial position
        clickStartTime = Time.time; // Record the time when the mouse button is pressed
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePosition.z = 0; // Maintain the same z position

            // Calculate the offset
            Vector3 offset = currentMousePosition - lastMousePosition;

            // Calculate the new position with the offset
            Vector3 newPosition = transform.position + new Vector3(offset.x, 0, 0);

            // Clamp the new position to be within the neighbors' starting positions
            if (arrayIndex > 0) // Not the first object
            {
                newPosition.x = Mathf.Max(newPosition.x, objectPositions[arrayIndex - 1].x);
            }
            if (arrayIndex < objectPositions.Length - 1) // Not the last object
            {
                newPosition.x = Mathf.Min(newPosition.x, objectPositions[arrayIndex + 1].x);
            }

            // Get the screen bounds
            float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

            // Get the sprite's width
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                float spriteWidth = sr.bounds.size.x / 2;

                // Clamp the new position to be within the screen bounds
                newPosition.x = Mathf.Clamp(newPosition.x, screenLeft + spriteWidth, screenRight - spriteWidth);
            }

            // Update the position
            transform.position = newPosition;

            // Update the last mouse position and last position
            lastMousePosition = currentMousePosition;

            if (transform.position.x < lastPosition.x)
            {
                direction = left;

                if (direction != lastDirection)
                {
                    lastDirection = left;
                    positionMessage = "Direction changed to left";
                    if (positionMessage != lastPositionMessage)
                    {
                        Debug.Log(positionMessage);
                        lastPositionMessage = positionMessage;
                    }
                }
            }
            else if (transform.position.x > lastPosition.x)
            {
                direction = right;

                if (direction != lastDirection)
                {
                    lastDirection = right;
                    positionMessage = "Direction changed to right";
                    if (positionMessage != lastPositionMessage)
                    {
                        Debug.Log(positionMessage);
                        lastPositionMessage = positionMessage;
                    }
                }
            }

            // Check if the sprite is left or right of its starting position
            int newPositionLorR = transform.position.x < objectPositions[arrayIndex].x ? left : right;
            if (newPositionLorR != positionLorR)
            {
                positionLorR = newPositionLorR;
                Debug.Log("Position relative to starting position: " + (positionLorR == left ? "left" : "right"));
            }

            // Determine the next sprite index based on direction and positionLorR
            int newNextSpriteIndex = -1;
            if (direction == left && positionLorR == left && arrayIndex > 0)
            {
                newNextSpriteIndex = arrayIndex - 1;
            }
            else if (direction == right && positionLorR == right && arrayIndex < objectPositions.Length - 1)
            {
                newNextSpriteIndex = arrayIndex + 1;
            }

            // Print the next sprite index if it changes
            if (newNextSpriteIndex != nextSpriteIndex)
            {
                nextSpriteIndex = newNextSpriteIndex;
                if (nextSpriteIndex != -1)
                {
                    Debug.Log("Next sprite index: " + nextSpriteIndex);
                }
            }

            lastPosition = newPosition;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Calculate the total drag distance
        float dragDistance = Vector3.Distance(objectPositions[arrayIndex], transform.position);

        // Check if the action is a click based on time and distance thresholds
        if (Time.time - clickStartTime <= clickThresholdTime && dragDistance <= dragThresholdDistance)
        {
            // Output the array index and the x coordinate of the object's position to the console
            Debug.Log("Clicked on sprite with array index: " + arrayIndex + ", x position: " + objectPositions[arrayIndex].x);

            // Determine the click position relative to the sprite
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0; // Ensure the z-position is maintained

            // Get the sprite's position
            Vector3 spritePosition = transform.position;

            // Determine if the click was on the left or right side of the sprite
            if (clickPosition.x < spritePosition.x)
            {
                // Clicked on the left side, rotate 180 degrees anti-clockwise
                Debug.Log("Clicked on the left side");
                StartCoroutine(RotateSprite(180));
            }
            else
            {
                // Clicked on the right side, rotate 180 degrees clockwise
                Debug.Log("Clicked on the right side");
                StartCoroutine(RotateSprite(-180));
            }
        }
        else
        {
            Debug.Log("Drag detected, no rotation");
            StartCoroutine(SmoothReturn());
        }

        isAnyObjectInteracting = false;
    }

    IEnumerator RotateSprite(float angle)
    {
        isAnyObjectRotating = true; // Set the static flag to indicate an object is rotating
        float duration = 0.5f; // Duration for the rotation
        float elapsedTime = 0.0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isAnyObjectRotating = false; // Reset the static flag to indicate the rotation has finished
    }

    IEnumerator SmoothReturn()
    {
        float elapsedTime = 0.0f;
        float duration = 0.5f; // Duration for the return animation
        Vector3 endPosition = objectPositions[arrayIndex]; // Get the original position from the array

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        isAnyObjectInteracting = false; // Reset the static flag to indicate the interaction has finished
    }
}