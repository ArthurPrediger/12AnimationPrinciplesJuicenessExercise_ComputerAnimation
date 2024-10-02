using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    bool clickUpdateOn = false;

    private List<Vector3> bezierPoints = new List<Vector3>();
    const int numBezierPoints = 100;
    private double bezierCurveLength;
    private float tParam = 1.0f;
    private float speed = 0.25f;
    private float speedBackward = 4.0f;
    bool dragUpdateOn = false;
    Vector3 initPos = Vector3.zero;

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
        DeformingEaseJump();
        CurveEaseMove();
    }
    private void OnCollisionEnter(Collision collision)
    {
        handleCollisionScale = true;
    }

    public void ClickReaction()
    {
        if (clickUpdateOn) return;
        clickUpdateOn = true;
        handleCollisionScale = false;
        GetComponent<InputReactions>().enabled = true;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void DeformingEaseJump()
    {
        if (!clickUpdateOn) return;

        if (!handleCollisionScale)
        {
            float velMag = rb.velocity.magnitude;
            deformValue = velMag / jumpForce;
            gameObject.transform.localScale = originalScale + 1.2f * EaseOutExpo(deformValue) * Vector3.up;
        }
        else
        {
            const float scaleDecreaseStep = 0.01f;
            gameObject.transform.localScale -= scaleDecreaseStep * Vector3.up;
            gameObject.transform.position -= (scaleDecreaseStep * halfScaleDelta) * Vector3.up;
            if (gameObject.transform.localScale.y <= 1.0f)
            {
                gameObject.transform.localScale = originalScale;
                gameObject.transform.position += (scaleDecreaseStep * halfScaleDelta) * Vector3.up;
                handleCollisionScale = false;
                clickUpdateOn = false;
            }
        }
    }

    public float EaseOutExpo(float x)
    {
        // Clamp x to the range [0, 1]
        x = Mathf.Clamp01(x);

        // Calculate the function value
        float result = (float)(Mathf.Exp(-5 * x) / (x + 0.1));

        // Clamp the result to the range [0, 1]
        result = Mathf.Clamp01(x);

        return result;
    }

    public void DragReaction(List<Vector3> curveControlPoints)
    {
        if (dragUpdateOn) return;
        dragUpdateOn = true;
        bezierPoints.Clear();
        tParam = -1.0f;
        initPos = curveControlPoints[0];
        curveControlPoints[0] -= 0.2f * (curveControlPoints.Last() - curveControlPoints.First());
        float t = 0.0f;
        for (int i = 0; i < numBezierPoints; i++)
        {
            bezierPoints.Add(CalculateBezierPoint(t, 
                curveControlPoints.ElementAt(0),
                curveControlPoints.ElementAt(1),
                curveControlPoints.ElementAt(2),
                curveControlPoints.ElementAt(3)));

            t += (float)(1) / (float)(numBezierPoints - 1);
        }

        bezierCurveLength = 0.0f;
        for (int i = 0; i < bezierPoints.Count - 1; i++)
        {
            bezierCurveLength += (bezierPoints[i + 1] - bezierPoints[i]).magnitude;
        }

        GetComponent<InputReactions>().enabled = true;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        // Parametric form of cubic Bezier
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * P0; // (1 - t)^3 * P0
        point += 3 * uu * t * P1; // 3 * (1 - t)^2 * t * P1
        point += 3 * u * tt * P2; // 3 * (1 - t) * t^2 * P2
        point += ttt * P3;        // t^3 * P3

        return point;
    }

    public class PathInfo
    {
        public Vector3 pos;
        public Vector3 dir;
    }

    public PathInfo GetPathInfoAt(float t)
    {
        double distFromStart = bezierCurveLength * t;

        int i = 0;
        for (i = 0; i < bezierPoints.Count - 1 && distFromStart > 0.0f; i++)
        {
            distFromStart -= (bezierPoints[i + 1] - bezierPoints[i]).magnitude;
        }

        PathInfo pathInfo = new PathInfo();
        Vector3 lastDir = gameObject.transform.forward;
        if (i > 0)
        {
            lastDir = (bezierPoints[i] - bezierPoints[i - 1]);
            pathInfo.dir = lastDir.normalized;
        }
        pathInfo.pos = bezierPoints[i] + ((float)(distFromStart / lastDir.magnitude) * lastDir);
        return pathInfo;
    }

    private void CurveEaseMove()
    {
        if (!dragUpdateOn) return;

        if(tParam < 0.0f)
        {
            tParam += Time.deltaTime * speedBackward;
            tParam = Mathf.Min(tParam, 0.0f);
            gameObject.transform.position = Vector3.Lerp(bezierPoints[0], initPos, EaseInCubic(Mathf.Abs(tParam)));
        }
        else
        {
            tParam += Time.deltaTime * speed;
            tParam = Mathf.Clamp01(tParam);

            float easeT = EaseOutQuart(tParam);

            PathInfo pathInfo = GetPathInfoAt(easeT);
            gameObject.transform.position = pathInfo.pos;

            if (tParam >= 1.0f) dragUpdateOn = false;
        }
    }

    private float EaseOutQuart(float x)
    {
        return 1.0f - (float)Math.Pow(1.0f - x, 4.0f);
    }

    public float EaseInCubic(float x)
    {
        return x * x * x;
    }
}
