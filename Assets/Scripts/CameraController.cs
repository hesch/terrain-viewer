using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  float dragSpeed = 2.0f;
  float rotationSpeed = 2.0f;
  private Vector3 origin;
  private Vector3 oldLocalPosition;
  private Quaternion oldLocalRotation;
  private Camera controlledCamera;
    // Start is called before the first frame update
    void Start()
    {
      controlledCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
      handleZoom();
      handlePanning();
      handleRotation();
    }

    private void handlePanning() {
      if (Input.GetMouseButtonDown(1))
      {
	origin = Input.mousePosition;
	return;
      }

      if (!Input.GetMouseButton(1)) return;

      Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - origin);
      Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

      transform.Translate(move, Space.World);  
    }

    private void handleRotation() {
      if (Input.GetMouseButtonDown(0))
      {
	origin = Input.mousePosition;
	oldLocalPosition = transform.localPosition;
	oldLocalRotation = transform.localRotation;
	return;
      }

      if (!Input.GetMouseButton(0)) return;

      Vector3 pivot = controlledCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 100));
      transform.localPosition = oldLocalPosition;
      transform.localRotation = oldLocalRotation;
      transform.RotateAround(pivot, Vector3.Cross(pivot-transform.position, Input.mousePosition - origin), (Input.mousePosition - origin).magnitude);
    }

    private void handleZoom() {
      if (Input.GetAxis("Mouse ScrollWheel") < 0) {
	controlledCamera.fieldOfView++;
      } else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
	controlledCamera.fieldOfView--;
      }
    }

}
