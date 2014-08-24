using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S))
		{
			Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int direction = ((Input.GetMouseButtonDown(0) && touchPosition.x > 0) || Input.GetKeyDown(KeyCode.S)) ? 1: -1;
			rigidbody2D.velocity = new Vector2(2 * direction,5);
			rigidbody2D.angularVelocity = 1000* direction;
			rigidbody2D.angularDrag = 4;
		}

		Camera.main.transform.position = new Vector3(0, transform.position.y,-10);
	}


}
