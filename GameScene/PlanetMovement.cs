using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMovement : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        this.transform.Rotate(Vector3.down, 0.025f);
	}
}
