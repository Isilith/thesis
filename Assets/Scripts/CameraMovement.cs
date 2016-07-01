using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	
	//Inputs variables
	private float moveSpeed = 10f;
	private float rotateSpeed = 70f;
	private Vector3 startPosition = -Vector3.one;
	private Vector3 endPosition = -Vector3.one;
	public static bool rotating = false;
	public static bool reset = false;

	void Update () {
		Inputs ();
	}

	private void Inputs() {
		//+z
		if (Input.GetKey(KeyCode.Keypad8)) {
			transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
		}
		//-z
		if (Input.GetKey(KeyCode.Keypad2)) {
			transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);
		}
		//-x
		if (Input.GetKey(KeyCode.Keypad4)) {
			transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
		}
		//+x
		if (Input.GetKey(KeyCode.Keypad6)) {
			transform.Translate(-Vector3.left * moveSpeed * Time.deltaTime);
		}
		//+y
		if (Input.GetKey(KeyCode.PageUp)) {
			transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
		}
		//-y
		if (Input.GetKey(KeyCode.PageDown)) {
			transform.Translate(-Vector3.up * moveSpeed * Time.deltaTime);
		}
		//rotate left (ccw??)
		if (Input.GetKey(KeyCode.Keypad7)) {
			transform.Rotate(-Vector3.up, rotateSpeed * Time.deltaTime);
		}
		//rotate right (cw??)
		if (Input.GetKey(KeyCode.Keypad9)) {
			transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
		}
		//rotate up
		if (Input.GetKey(KeyCode.Home)) {
			transform.Rotate(Vector3.right, -rotateSpeed * Time.deltaTime);
		}
		//rotate down
		if (Input.GetKey(KeyCode.End)) {
			transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
		}
		//reset position and rotation
		if (Input.GetKey(KeyCode.Backspace)) {
			transform.rotation = Quaternion.identity;
			transform.position = new Vector3(0f,1f,0f);
		}

		if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl)) {
			startPosition = Input.mousePosition;
		}
		if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl)) {
			rotating = true;
			endPosition = Input.mousePosition;
			Vector3 delta = endPosition - startPosition;
			float temp = delta.x;
			delta.x = -delta.y;
			delta.y = temp;
			Transform target = null;
			if (UIScript.select != null)
				target = UIScript.select.root;
			if (target != null) 
				target.RotateAround(target.position, delta, Mathf.Sqrt(delta.magnitude));
			startPosition = endPosition;
		}
		if (!Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl)) {
			rotating = false;
		}
		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			reset = false;
		}
		if (Input.GetKey(KeyCode.LeftShift)) {
			reset = true;
		}
	}
}
