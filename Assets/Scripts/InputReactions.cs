using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputReactions : MonoBehaviour
{
    private Rigidbody rb;
    private float jumpForce = 10.0f;
    private float deformValue = 0.0f;
    private bool handleCollisionScale = false;
    private float halfScaleDelta = 0.0f;
    private Vector3 originalScale;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GetComponent<InputReactions>().enabled = false;

        originalScale = gameObject.transform.localScale;
        Vector3 minBefore = GetComponent<Renderer>().bounds.min;
        gameObject.transform.localScale = gameObject.transform.localScale + 2.0f * Vector3.up;
        Vector3 minAfter = GetComponent<Renderer>().bounds.min;
        halfScaleDelta = Mathf.Abs((minAfter.y - minBefore.y) / 2.0f);
        gameObject.transform.localScale = originalScale;
    }

    // Update is called once per frame
    void Update()
    {
        float velMag = rb.velocity.magnitude;

        if ((velMag > 0.0001f) && !handleCollisionScale)
        {
            deformValue = velMag / jumpForce;
            gameObject.transform.localScale = originalScale + 1.2f * Calculate(deformValue) * Vector3.up;
        }
        else if (handleCollisionScale)
        {
            const float scaleDecreaseStep = 0.01f;
            gameObject.transform.localScale -= scaleDecreaseStep * Vector3.up;
            gameObject.transform.position -= (scaleDecreaseStep * halfScaleDelta) * Vector3.up;
            if (gameObject.transform.localScale.y <= 1.0f)
            {
                gameObject.transform.localScale = originalScale;
                gameObject.transform.position += (scaleDecreaseStep * halfScaleDelta) * Vector3.up;
                handleCollisionScale = false;
                GetComponent<InputReactions>().enabled = false;
            }
        }
    }

    public void ClickReaction()
    {
        handleCollisionScale = false;
        GetComponent<InputReactions>().enabled = true;

        if (rb.velocity.sqrMagnitude > 0.001f) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        handleCollisionScale = true;
    }

    public float Calculate(float x)
    {
        // Clamp x to the range [0, 1]
        x = Mathf.Clamp01(x);

        // Calculate the function value
        float result = (float)(Mathf.Exp(-5 * x) / (x + 0.1));

        // Clamp the result to the range [0, 1]
        result = Mathf.Clamp01(x);

        return result;
    }
}
