using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(r, out hit) && hit.collider.tag == "Interactive")
            {
                hit.collider.gameObject.GetComponent<InputReactions>().ClickReaction();
            }
        }
    }
}
