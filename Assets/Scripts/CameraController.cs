using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  float dragSpeed = 2.0f;
  float rotationSpeed = 0.5f;
  float zoomSpeed = 2.0f;
  private Vector3 origin;
  private Vector3 oldLocalPosition;
  private Quaternion oldLocalRotation;
  public Camera controlledCamera;
    // Start is called before the first frame update
    void Start()
    {
      if (!controlledCamera) {
	controlledCamera = GetComponent<Camera>();
      }
    }

    // Update is called once per frame
    void Update()
    {
      if (!controlledCamera.pixelRect.Contains(Input.mousePosition))
	return;

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

      controlledCamera.transform.Translate(move, Space.World);  
    }

    private void handleRotation() {
      if (Input.GetMouseButtonDown(0))
      {
	origin = Input.mousePosition;
	oldLocalPosition = controlledCamera.transform.localPosition;
	oldLocalRotation = controlledCamera.transform.localRotation;
	return;
      }

      if (!Input.GetMouseButton(0)) return;

      controlledCamera.transform.localPosition = oldLocalPosition;
      controlledCamera.transform.localRotation = oldLocalRotation;
      Vector3 pivot = controlledCamera.transform.position + controlledCamera.transform.forward * 75;
      controlledCamera.transform.RotateAround(pivot, Vector3.Cross(pivot-controlledCamera.transform.position, Input.mousePosition - origin), (Input.mousePosition - origin).magnitude*rotationSpeed);
    }

    private void handleZoom() {
      if (Input.GetAxis("Mouse ScrollWheel") < 0) {
	controlledCamera.transform.Translate(0, 0, -zoomSpeed);
      } else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
	controlledCamera.transform.Translate(0, 0, zoomSpeed);
      }
    }

}
