//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject middlePointPrefab;
    public GameObject endPointPrefab;
    private List<Vector3> curveControlPoints = new List<Vector3>();
    private GameObject interactedObj = null;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 4; i++)
            curveControlPoints.Add(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out RaycastHit hit) && hit.collider.CompareTag("Interactive"))
            {
                hit.collider.gameObject.GetComponent<InputReactions>().ClickReaction();
            }
        }
        else if(Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out RaycastHit hit) && hit.collider.CompareTag("Interactive"))
            {
                interactedObj = hit.collider.gameObject;
                curveControlPoints[0] = interactedObj.transform.position;
            }
        }
        else if (Input.GetMouseButton((int)MouseButton.Right) && interactedObj)
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out RaycastHit hit))
            {
                curveControlPoints[^1] = hit.point + interactedObj.GetComponent<Renderer>().bounds.size.y * 0.5f * Vector3.up;
            }
        }
        else if(Input.GetMouseButtonUp((int)MouseButton.Right) && interactedObj)
        {
            Vector3 startPoint = curveControlPoints.First();
            Vector3 endPoint = curveControlPoints.Last();
            float maxUpDist = (endPoint - startPoint).magnitude / 2.0f;
            Vector3 maxPos = Vector3.Max(startPoint, endPoint);
            maxPos.y += maxUpDist;
            Vector3 minPos = Vector3.Min(startPoint, endPoint);
            for(int i = 1; i < curveControlPoints.Count - 1; i++)  //Vector3 controlPoint in curveControlPoints.GetRange(1, curveControlPoints.Count-1))
            {
                float x = Random.Range(minPos.x, maxPos.x);
                float y = Random.Range(minPos.y, maxPos.y);
                float z = Random.Range(minPos.z, maxPos.z);
                curveControlPoints[i] = new Vector3(x, y, z);

                if (i > 1)
                {
                    curveControlPoints[i] = Vector3.Max(curveControlPoints[i-1], curveControlPoints[i]);
                }
            }

            interactedObj.GetComponent<InputReactions>().DragReaction(curveControlPoints);
            interactedObj = null;
        }
    }
}
