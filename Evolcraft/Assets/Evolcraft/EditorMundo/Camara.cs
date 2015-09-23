using UnityEngine;
using System.Collections;

public class Camara : MonoBehaviour {

	private float speed=20f;
	private bool rotarderecha=false;
	private bool rotarizquierda=false;
	private int anguloRotacion=30;
	public int contadorRotacion=0;
	public float anguloRotx=30;
	public int avanceZ=0;
	// Use this for initialization
	//void Start () {	}

	void Start(){
		transform.Rotate (30, 0, 0);
	}


	// Update is called once per frame
	void Update () {

		Zoom ();
		Rotacion ();
		Traslacion();
	}

	void Rotacion(){

		if(Input.GetKeyDown(KeyCode.LeftArrow) & rotarderecha==false && rotarizquierda==false){
			rotarizquierda=true;
			avanceZ-=1;
			avanceZ=modulo(avanceZ,4);
		}

		if(Input.GetKeyDown(KeyCode.RightArrow) && rotarderecha==false && rotarizquierda==false){
			rotarderecha=true;
			avanceZ+=1;
			avanceZ=modulo(avanceZ,4);
		}

		if(rotarderecha || rotarizquierda){
	
			if(rotarderecha){
				transform.Rotate (0,anguloRotacion,0,Space.World);
			}
			else{
				transform.Rotate (0,-1*anguloRotacion,0,Space.World);
			}

			contadorRotacion+=1;

			if(contadorRotacion>=90/anguloRotacion){
				rotarderecha=false;
				rotarizquierda=false;
				contadorRotacion=0;
			}
		}
	}

	void Zoom(){
		//REVISAR EL MOTIVO POR EL CUAL SE DETIENE EL AVANCE
		if(Input.GetKey(KeyCode.L)){
			Camera.current.fieldOfView-=2;
		}

		if(Input.GetKey(KeyCode.M)){
			Camera.current.fieldOfView+=2;
			//Camera.fieldOfView+=2;
		}
	}

	void Traslacion(){

		if (Input.GetKey (KeyCode.UpArrow)) {
			transform.Translate (0, Time.deltaTime * speed, 0, Space.World);
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			transform.Translate (0, -Time.deltaTime * speed, 0, Space.World);
		}
		/*
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
			// Get movement of the finger since last frame
			var touchDeltaPosition: Vector2 = Input.GetTouch(0).deltaPosition;
			// Move object across XY plane
			transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
		}*/
	
		if (Input.mousePosition.x >= Screen.width - 1) {

			transform.Translate (Time.deltaTime * speed, 0, 0);
		}
		if (Input.mousePosition.x < 1) {
			
			transform.Translate (-Time.deltaTime * speed, 0, 0);
		}

		switch (avanceZ) {

		case 0:
			if(Input.mousePosition.y>=Screen.height-1){	
				//transform.Translate(0,Time.deltaTime*speed*Mathf.Sin(anguloRotx)*Mathf.Cos(anguloRotx),Time.deltaTime*speed*Mathf.Cos(anguloRotx)*Mathf.Cos(anguloRotx)); //Time.deltaTime*speed*Mathf.Cos(anguloRotx)
				transform.Translate(0,0,Time.deltaTime*speed,Space.World);
			}
			if(Input.mousePosition.y<1){
				//transform.Translate(0,-Time.deltaTime*speed*Mathf.Sin(anguloRotx),-Time.deltaTime*speed*Mathf.Cos(anguloRotx));
				transform.Translate(0,0,-Time.deltaTime*speed,Space.World);
			}
			break;
		case 1:
			if(Input.mousePosition.y>=Screen.height-1){	
				//transform.Translate(0,Time.deltaTime*speed*Mathf.Sin(anguloRotx)*Mathf.Cos(anguloRotx),Time.deltaTime*speed*Mathf.Cos(anguloRotx)*Mathf.Cos(anguloRotx)); //Time.deltaTime*speed*Mathf.Cos(anguloRotx)
				transform.Translate(Time.deltaTime*speed,0,0,Space.World);
			}
			if(Input.mousePosition.y<1){
				//transform.Translate(0,-Time.deltaTime*speed*Mathf.Sin(anguloRotx),-Time.deltaTime*speed*Mathf.Cos(anguloRotx));
				transform.Translate(-Time.deltaTime*speed,0,0,Space.World);
			}
			break;
		case 2:
			if(Input.mousePosition.y>=Screen.height-1){	
				//transform.Translate(0,Time.deltaTime*speed*Mathf.Sin(anguloRotx)*Mathf.Cos(anguloRotx),Time.deltaTime*speed*Mathf.Cos(anguloRotx)*Mathf.Cos(anguloRotx)); //Time.deltaTime*speed*Mathf.Cos(anguloRotx)
				transform.Translate(0,0,-Time.deltaTime*speed,Space.World);
			}
			if(Input.mousePosition.y<1){
				//transform.Translate(0,-Time.deltaTime*speed*Mathf.Sin(anguloRotx),-Time.deltaTime*speed*Mathf.Cos(anguloRotx));
				transform.Translate(0,0,Time.deltaTime*speed,Space.World);
			}
			break;
		case 3:
			if(Input.mousePosition.y>=Screen.height-1){	
				//transform.Translate(0,Time.deltaTime*speed*Mathf.Sin(anguloRotx)*Mathf.Cos(anguloRotx),Time.deltaTime*speed*Mathf.Cos(anguloRotx)*Mathf.Cos(anguloRotx)); //Time.deltaTime*speed*Mathf.Cos(anguloRotx)
				transform.Translate(-Time.deltaTime*speed,0,0,Space.World);
			}
			if(Input.mousePosition.y<1){
				//transform.Translate(0,-Time.deltaTime*speed*Mathf.Sin(anguloRotx),-Time.deltaTime*speed*Mathf.Cos(anguloRotx));
				transform.Translate(Time.deltaTime*speed,0,0,Space.World);
			}
			break;
		}


	}

	int modulo(int a,int b){
	
		int q, r;

		q = a / b;
		if (a < 0) {
			q-=1;
		}
		r = a - b * q;

		return r;
	}
}

