using UnityEngine;
using System;
using System.Collections;
using Cubiquity;
using Cubiquity.Impl;
public class EditorTerreno : MonoBehaviour {

	int numObjects = 1;
	public GameObject BaldosaPF;
	public GameObject CubePF;	
	int depth = 100;
	int height = 10;
	int width = 100;

	enum DIRECCIONES
	{
		ADELANTE,
		ATRAS,
		IZQUIERDA,
		DERECHA,
		ARRIBA,
		ABAJO,
		NUM_DIRECCIONES
	};

	public int estado = (int)DIRECCIONES.ADELANTE;

	void Start () {
		GameObject b1 = Instantiate (BaldosaPF) as GameObject;
		b1.name="0#0#0";
		b1.GetComponent<TerrainVolume>().data = VolumeData.CreateFromVoxelDatabase<TerrainVolumeData>(@"C:\Cat\0#0#0.vdb",VolumeData.WritePermissions.ReadWrite);
		GameObject cont1 = new GameObject();
		GameObject c1 = Instantiate (CubePF) as GameObject;
		c1.name = "cubo";
		b1.transform.parent = cont1.transform;
		c1.transform.parent = cont1.transform;
		c1.transform.localScale = new Vector3 (10, 10, 10);

		Vector3i esqInferior = b1.GetComponent<TerrainVolume> ().data.enclosingRegion.lowerCorner;
		Vector3i esqSuperior = b1.GetComponent<TerrainVolume> ().data.enclosingRegion.upperCorner;

		Vector3 baldosPos = b1.transform.position;
		Vector3 newPos = new Vector3 ( baldosPos.x+(esqSuperior.x-esqInferior.x)/2, baldosPos.y+(esqSuperior.y-esqInferior.y),baldosPos.z+(esqSuperior.z-esqInferior.z)/2 );

		c1.transform.position = newPos;
	}
	
	TerrainVolumeData crearVolumenData(string direccion)
	{
		TerrainVolumeData volumeData = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1),@"C:\CS-UCSP\Evolcraft\"+ direccion+".vdb");
		numObjects++;
		//Si no recibe un path, crea solo un temporal. Caso contrario, evalua si es un path relativo o no...
		MaterialSet materialSet = new MaterialSet();

		for (int z = 0; z < depth; z++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					materialSet.weights[0] = (byte)255;  
					volumeData.SetVoxel(x, y, z, materialSet);
				}
			}
		}
		volumeData.CommitChanges ();

		return volumeData;
	}

	int[] codigo(string name){
		int[] codes = new int[3];
		int ind1, ind2;
		ind1 = 0;
		ind2 = name.IndexOf("#");
		Debug.Log ("ind2: "+ind2);
		for(int i=0;i<2;i++){
			codes[i] = Int32.Parse(name.Substring(ind1,ind2-ind1));
			ind1 = ind2+1;
			ind2 = name.IndexOf("#",ind1);
		}
		codes[2] = Int32.Parse(name.Substring(ind1,name.Length-ind1));
		return codes;
	}


	void Update () {

		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) 
			{
//				Debug.Log (hit.collider.gameObject.name);
				if(hit.collider.gameObject.name == "cubo"){

					string name = hit.collider.gameObject.transform.parent.GetChild(0).name;
					Debug.Log("nombre: "+name);
					GameObject chocado = GameObject.Find(name);
					Vector3 posChocado = chocado.transform.position;

					int[] codes = codigo(name);

					GameObject cont1 = new GameObject();

					Vector3 newBaldosaPos = new Vector3();

					switch(estado)
					{
					case (int)DIRECCIONES.ADELANTE: 	newBaldosaPos = new Vector3(posChocado.x,posChocado.y,posChocado.z+depth);
												codes[2]++;
							break;
					case (int)DIRECCIONES.ATRAS: newBaldosaPos = new Vector3(posChocado.x,posChocado.y,posChocado.z-depth);
												codes[2]--;
							break;
					case (int)DIRECCIONES.ARRIBA: newBaldosaPos = new Vector3(posChocado.x,posChocado.y+height,posChocado.z);
												codes[1]++;
							break;
					case (int)DIRECCIONES.ABAJO: newBaldosaPos = new Vector3(posChocado.x,posChocado.y-height,posChocado.z+depth);
												codes[1]--;				
							break;
					case (int)DIRECCIONES.DERECHA: newBaldosaPos = new Vector3(posChocado.x+width,posChocado.y,posChocado.z);
												codes[0]++;
							break;
					case (int)DIRECCIONES.IZQUIERDA: newBaldosaPos = new Vector3(posChocado.x-width,posChocado.y,posChocado.z+depth);
												codes[0]--;
							break;
					}
					string codepath="";
					codepath += codes[0].ToString()+"#"+codes[1].ToString()+"#"+codes[2].ToString();


					GameObject b1 = Instantiate (BaldosaPF) as GameObject;
					b1.name = codepath;
					b1.GetComponent<TerrainVolume>().data = crearVolumenData(codepath);
					GameObject c1 = Instantiate (CubePF) as GameObject;
					c1.name="cubo";

					b1.transform.position = newBaldosaPos;

					b1.transform.parent = cont1.transform;
					c1.transform.parent = cont1.transform;

					c1.transform.localScale = new Vector3 (10, 10, 10);
					
					Vector3i esqInferior = b1.GetComponent<TerrainVolume> ().data.enclosingRegion.lowerCorner;
					Vector3i esqSuperior = b1.GetComponent<TerrainVolume> ().data.enclosingRegion.upperCorner;
					
					Vector3 baldosPos = b1.transform.position;
					Vector3 newPos = new Vector3 ( baldosPos.x+(esqSuperior.x-esqInferior.x)/2, baldosPos.y+(esqSuperior.y-esqInferior.y),baldosPos.z+(esqSuperior.z-esqInferior.z)/2 );
					
					c1.transform.position = newPos;
				}
			}

		}else if(Input.GetKeyDown(KeyCode.Alpha1)){
				estado = (int)DIRECCIONES.ADELANTE;
		}else if(Input.GetKeyDown(KeyCode.Alpha2)){
			estado = (int)DIRECCIONES.ATRAS;
		}else if(Input.GetKeyDown(KeyCode.Alpha3)){
			estado = (int)DIRECCIONES.ARRIBA;
		}else if(Input.GetKeyDown(KeyCode.Alpha4)){
			estado = (int)DIRECCIONES.ABAJO;
		}else if(Input.GetKeyDown(KeyCode.Alpha5)){
			estado = (int)DIRECCIONES.IZQUIERDA;
		}else if(Input.GetKeyDown(KeyCode.Alpha6)){
			estado = (int)DIRECCIONES.DERECHA;
		}  
		
	}
}
