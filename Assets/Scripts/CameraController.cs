using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  float speed = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
       float translation = speed * Input.GetAxis("Vertical");
       float translationX = speed * Input.GetAxis("Horizontal"); 

       translation *= Time.deltaTime;
        translationX *= Time.deltaTime;

	transform.Translate(translationX, 0, translation);

	float h = Input.GetAxis("RotH");
        float v = Input.GetAxis("RotV");

        transform.Rotate(v, h, 0);
    }
}
