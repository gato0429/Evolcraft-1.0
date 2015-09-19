using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {

	public Transform posicionActor;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate () {

		transform.position=new Vector3(posicionActor.position.x,posicionActor.position.y+50,posicionActor.position.z);
	}
}
