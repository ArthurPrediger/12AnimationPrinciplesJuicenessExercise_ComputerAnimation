using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    float scaleStep = -6f;

    private List<Vector3> bezierPoints = new List<Vector3>();
    const int numBezierPoints = 100;
    private double bezierCurveLength;
    private float tParam = 1.0f;
    private float speed = 0.25f;
    private float speedBackward = 4.0f;
    bool dragUpdateOn = false;
    Vector3 initPos = Vector3.zero;
    Vector3 curVel = Vector3.zero;

    [SerializeField]
    private ParticleSystem collisionEffect;

    private TMPro.TextMeshProUGUI principlesText;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        originalScale = gameObject.transform.localScale;
        Vector3 minBefore = GetComponent<Renderer>().bounds.min;
        gameObject.transform.localScale = gameObject.transform.localScale + 2.0f * Vector3.up;
        Vector3 minAfter = GetComponent<Renderer>().bounds.min;
        halfScaleDelta = Mathf.Abs((minAfter.y - minBefore.y) / 2.0f);
        gameObject.transform.localScale = originalScale;

        principlesText = FindObjectOfType<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        DeformingEaseJump();
        CurveEaseMove();

        if(gameObject.transform.position.y < -100.0f)
        {
            gameObject.transform.position = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (clickUpdateOn)
        {
            handleCollisionScale = true;
        }

        if (dragUpdateOn && collision.gameObject.CompareTag("Interactive"))
        {
            rb.AddForce(curVel * 200.0f, ForceMode.VelocityChange);
            dragUpdateOn = false;
        }

        List<ContactPoint> contacts = new();
        int numContacts = collision.GetContacts(contacts);
        ParticleSystem partSys = new();

        if (collision.collider.CompareTag("Interactive"))
        {
            if(numContacts > 0)
            {
                partSys = Instantiate(collisionEffect, contacts.First().point, Quaternion.identity);
                var main = partSys.main;
                main.startColor = (gameObject.GetComponent<MeshRenderer>().material.color + collision.gameObject.GetComponent<MeshRenderer>().material.color) * 0.5f;
                partSys.gameObject.transform.up = contacts.First().normal;
            }
        }
        else
        {
            partSys = Instantiate(collisionEffect, contacts.First().point, Quaternion.identity);
            var main = partSys.main;
            main.startColor = gameObject.GetComponent<MeshRenderer>().material.color;
            partSys.gameObject.transform.up = contacts.First().normal;
        }

        principlesText.GetComponent<PrinciplesTextManager>().AddPrinciple("Encenação", partSys.main.startColor.color);
    }

    public void ClickReaction()
    {
        if (clickUpdateOn || dragUpdateOn) return;
        clickUpdateOn = true;
        handleCollisionScale = false;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        principlesText.GetComponent<PrinciplesTextManager>().AddPrinciple("Compressão e Estiramento", GetComponent<MeshRenderer>().material.color);
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
        else if (scaleStep < 0.0f)
        {
            gameObject.transform.localScale += scaleStep * Time.deltaTime * Vector3.up;
            gameObject.transform.position += (scaleStep * halfScaleDelta) * Time.deltaTime * Vector3.up;
            if (gameObject.transform.localScale.y <= 0.4f)
            {
                scaleStep *= -1.0f;
            }
        }
        else
        {
            gameObject.transform.localScale += scaleStep * Time.deltaTime * Vector3.up;
            gameObject.transform.position += (scaleStep * halfScaleDelta) * Time.deltaTime * Vector3.up;
            if (gameObject.transform.localScale.y >= 1.0f)
            {
                gameObject.transform.localScale = originalScale;
                gameObject.transform.position += (scaleStep * halfScaleDelta) * Time.deltaTime * Vector3.up;
                handleCollisionScale = false;
                clickUpdateOn = false;
                scaleStep *= -1.0f;
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
        if (dragUpdateOn || clickUpdateOn) return;
        dragUpdateOn = true;
        bezierPoints.Clear();
        tParam = -1.0f;
        initPos = curveControlPoints[0];
        curveControlPoints[0] -= 0.25f * (curveControlPoints.Last() - curveControlPoints.First());
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

        principlesText.GetComponent<PrinciplesTextManager>().AddPrinciple("Antecipação", GetComponent<MeshRenderer>().material.color);
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
            curVel = pathInfo.pos - gameObject.transform.position;
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
