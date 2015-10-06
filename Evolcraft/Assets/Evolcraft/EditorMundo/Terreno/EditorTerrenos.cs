using UnityEngine;
using System.Collections;
using Cubiquity;
using Cubiquity.Impl;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

public class EditorTerrenos : MonoBehaviour {

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

	enum ESTADO
	{
		CREAR_BALDOSAS,
		MODIFICAR_BALDOSAS
	};

	enum HERRAMIENTA
	{
		ESCULPIR,
		ALISAR,
		PINTAR,
		CAVAR
	};

	enum FORMA_CAVAR
	{
		CUBO,
		ESFERA
	};

	//Estado Editor
	static ESTADO EstadoEditor = ESTADO.CREAR_BALDOSAS;

    //Tamaño del voxel
    int Profundidad  = 100;
    int Altura       = 32;
    int Ancho        = 100;

    //Prefab
    public GameObject VoxelPrefab;
	//Este voxel prefab debe tener dos hijos: (1) Los componentes de un Voxel (2)Un cubo


    //Variables Mapa
    static string DirectorioLocal = Application.dataPath+"/Evolcraft/EditorMundo/Mapas/";
    static string NombreMapa="MapaDemo"; //Nombre de la carpeta del mapa

    
    //Voxels
    Dictionary<string, GameObject>  MapaVoxels = new Dictionary<string, GameObject>();
    GameObject                      VoxelActual;
    Vector3                         CentroActual;
    string                          ClaveActual;
    static DIRECCIONES              Lado = DIRECCIONES.ADELANTE;

	//Modificar baldosas
	HERRAMIENTA	HerramientaActual = HERRAMIENTA.ESCULPIR;
	const int NroBrochas = 5;
	int BrochaSeleccionada = 0; //De 0 a NroBrochas, influira en FactorEscalaInternoBrocha 
    float RadioExternoBrocha = 5.0f; //Determinara el tamaño del area
    float RadioInternoBrocha; //Determinara que tan "cilindrico" 
    float FactorEscalaInternoBrocha; //Depende de la brocha seleccionada, determinará el radio interno
    float OpacidadBrocha = 1.0f; //Se puede entender como "intensidad"

	int TexturaSeleccionada = 2;//Esto deberia ser un enum...?

	FORMA_CAVAR FormaCavar = FORMA_CAVAR.CUBO;

	Vector3 PosicionPreviaMouse;

    void Start ()
    {
		//AssetDatabase.CreateFolder("Assets/Evolcraft/EditorMundo/Mapas",NombreMapa);

        GameObject VoxelBase = Instantiate(VoxelPrefab) as GameObject;//Voxel prefab debe estar inicializado.
        VoxelBase.name = "0#0#0";
        VoxelBase.transform.GetChild(0).GetComponent<TerrainVolume>().data=VolumeData.CreateFromVoxelDatabase<TerrainVolumeData>(@DirectorioLocal + NombreMapa +  "/" + VoxelBase.name + ".vdb",VolumeData.WritePermissions.ReadWrite); 
        VoxelBase.transform.GetChild(1).Translate(Ancho/2, (float)(0.1*Altura), Profundidad/2); 

        VoxelBase.transform.position = new Vector3(0,0,0);

	    MapaVoxels.Add(VoxelBase.name, VoxelBase);
	    VoxelActual = VoxelBase;
	    ClaveActual = VoxelBase.name;
        CentroActual = VoxelBase.transform.position;

		//Modificar baldosas
		PosicionPreviaMouse = new Vector3 (0, 0, 0);
		FactorEscalaInternoBrocha = (float)BrochaSeleccionada / ((float)(NroBrochas - 1));
		RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;//Esto debe actualizarse al cambiar el radioexterno
	}
	
    bool CrearVoxel(float px, float py, float pz, string pnombre)
    {
        
        //Creacion del voxel la db
		TerrainVolumeData volumeData = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, Ancho - 1, Altura - 1, Profundidad - 1), @DirectorioLocal + NombreMapa + "/" + pnombre + ".vdb");
		MaterialSet materialSet = new MaterialSet();

        double DifAltura = Altura - Altura * 0.9;
        for (int z = 0; z < Profundidad; z++)
        {
            for (int y = 0; y < (int)DifAltura; y++)
            {
                for (int x = 0; x < Ancho; x++)
                {
                    materialSet.weights[0] = (byte)255;
                    volumeData.SetVoxel(x, y, z, materialSet);
                }
            }
        }
        volumeData.CommitChanges();
        //asignacion del prefab

        GameObject VoxelTemporal = Instantiate(VoxelPrefab) as GameObject;//Voxel prefab debe estar inicializado.
        VoxelTemporal.name = pnombre;
        VoxelTemporal.transform.GetChild(0).GetComponent<TerrainVolume>().data=volumeData; 
        VoxelTemporal.transform.GetChild(1).Translate(Ancho/2, (float)DifAltura, Profundidad/2); 

        VoxelTemporal.transform.position = new Vector3(px,py,pz);
        
        MapaVoxels.Add(pnombre, VoxelTemporal);        
        return false;
    }

    string GenerarNombre()
    {
        string[] Pos = ClaveActual.Split('#');

        int nx= Int32.Parse(Pos[0]); 
        int ny= Int32.Parse(Pos[1]);
        int nz= Int32.Parse(Pos[2]);

        string NuevoNombre;
        switch(Lado)
        {
            case DIRECCIONES.ATRAS:
                nz--;
                break;
            case DIRECCIONES.ADELANTE:
                nz++;
                break;
            case DIRECCIONES.IZQUIERDA:
                nx--;
                break;
            case DIRECCIONES.DERECHA:
                nx++;
                break;
            case DIRECCIONES.ARRIBA:
                ny--;
                break;
            case DIRECCIONES.ABAJO:
                ny++;
                break;
        }
        NuevoNombre = nx.ToString() + "#" + ny.ToString() + "#" + nz.ToString();
        return NuevoNombre;
    }
    void Update()
    {
        TerrainVolume VolumenVoxelActual = VoxelActual.transform.GetChild(0).GetComponent<TerrainVolume>();
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        PickSurfaceResult DatosRaycast;

        bool Choco = Physics.Raycast(ray, out hit);
        bool ChocoVoxel = Picking.PickSurface(VolumenVoxelActual, ray.origin, ray.direction, 1000.0f, out DatosRaycast);

		if (Input.GetMouseButtonDown (0)) {
			if (EstadoEditor == ESTADO.CREAR_BALDOSAS) {
				if (Choco) {
					if (hit.collider.gameObject.name == "Centro") {
						ActualizarVoxelActual (hit);
						AgregarVoxel ();
					}
				}
			} else if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS) {
				if (Choco) {
					if (hit.collider.gameObject.name == "Centro") {
						VolumenVoxelActual.data.CommitChanges ();
						ActualizarVoxelActual (hit);
					} else if (ChocoVoxel) {
						if (HerramientaActual == HERRAMIENTA.CAVAR) {
							CavarVoxel (VolumenVoxelActual, (int)DatosRaycast.volumeSpacePos.x, (int)DatosRaycast.volumeSpacePos.y, (int)DatosRaycast.volumeSpacePos.z, (int)RadioExternoBrocha); //Este radioexterno deberia ser otra variable...
						}
					}
				}
			}
		} else if (Input.GetMouseButtonDown (0) || (Input.GetMouseButton (0) && (Input.mousePosition != PosicionPreviaMouse))) {
			if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS) {
				if (ChocoVoxel) {
					PosicionPreviaMouse = Input.mousePosition;
					ModificarVoxel (VolumenVoxelActual, DatosRaycast);
				}

			}
		} else if (Input.GetKeyDown (KeyCode.Q)) {
			if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS && (HerramientaActual != HERRAMIENTA.CAVAR )) {
				if(BrochaSeleccionada > 0){
                    BrochaSeleccionada--;
                    Debug.Log("Brocha: "+BrochaSeleccionada);
                    FactorEscalaInternoBrocha = (float)BrochaSeleccionada / ((float)(NroBrochas - 1));
                    RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;
				}
			}
		} else if (Input.GetKeyDown (KeyCode.W)) {
			if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS && (HerramientaActual != HERRAMIENTA.CAVAR )) {
				if(BrochaSeleccionada < NroBrochas-1){
                    BrochaSeleccionada++;
                    Debug.Log("Brocha: "+BrochaSeleccionada);
                    FactorEscalaInternoBrocha = (float)BrochaSeleccionada / ((float)(NroBrochas - 1));
                    RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;
				}
			}
		} else if (Input.GetKeyDown (KeyCode.A)) {
			if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS && (HerramientaActual != HERRAMIENTA.CAVAR ) ) {
				if(OpacidadBrocha > 0){
					OpacidadBrocha--;
				}
			}
		} else if (Input.GetKeyDown (KeyCode.S)) {
			if (EstadoEditor == ESTADO.MODIFICAR_BALDOSAS && (HerramientaActual != HERRAMIENTA.CAVAR )) {
				OpacidadBrocha++; //No estoy seguro si hay algun limite
			}
		}else if(Input.GetKeyDown(KeyCode.Z)){
			if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				if(RadioExternoBrocha >0){
					RadioExternoBrocha--;
                    RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;
				}
			}
		}else if(Input.GetKeyDown(KeyCode.X)){
			if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				RadioExternoBrocha++;
                RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;
			}
		}else if(Input.GetKeyDown(KeyCode.C)){
			if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				if(HerramientaActual == HERRAMIENTA.CAVAR){
					FormaCavar = FORMA_CAVAR.CUBO;
				}
			}
		}else if(Input.GetKeyDown(KeyCode.E)){
			if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				if(HerramientaActual == HERRAMIENTA.CAVAR){
					FormaCavar = FORMA_CAVAR.ESFERA;
				}
			}
		}
		else if(Input.GetKeyDown(KeyCode.Alpha1)){ //Todos estos deberian funcionar en determinado estado
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
				Lado = DIRECCIONES.ADELANTE;
			}else if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.ESCULPIR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha2)){
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
				Lado = DIRECCIONES.ATRAS;
			}else if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.ALISAR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha3)){
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
	            Lado = DIRECCIONES.ARRIBA;
			}else if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.PINTAR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha4)){
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
				Lado = DIRECCIONES.ABAJO;
			}else if(EstadoEditor == ESTADO.MODIFICAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.CAVAR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha5)){
            Lado = DIRECCIONES.IZQUIERDA;
		}else if(Input.GetKeyDown(KeyCode.Alpha6)){
            Lado = DIRECCIONES.DERECHA;
		}else if(Input.GetKeyDown(KeyCode.F1)){
			EstadoEditor = ESTADO.CREAR_BALDOSAS;
		}else if(Input.GetKeyDown(KeyCode.F2)){
			EstadoEditor = ESTADO.MODIFICAR_BALDOSAS;
		}
    }

	void ActualizarVoxelActual(RaycastHit hit)
	{
		ClaveActual = hit.collider.gameObject.transform.parent.name;
		Debug.Log("Clave actual: "+ClaveActual);
		VoxelActual = MapaVoxels[ClaveActual];
		CentroActual = hit.collider.gameObject.transform.parent.position;
	}

	void AgregarVoxel()
	{
		Vector3 NuevaPosicion=CentroActual;
		
		switch (Lado)
		{
			case DIRECCIONES.ATRAS:
				NuevaPosicion.z -= Profundidad ;
				break;
			case DIRECCIONES.ADELANTE:
				NuevaPosicion.z += Profundidad;
				break;
			case DIRECCIONES.IZQUIERDA:
				NuevaPosicion.x -= Ancho ;
				break;
			case DIRECCIONES.DERECHA:
				NuevaPosicion.x += Ancho ;
				break;
			case DIRECCIONES.ARRIBA:
				NuevaPosicion.y += Altura ;
				break;
			case DIRECCIONES.ABAJO:
				NuevaPosicion.y -= Altura ;
				break;
		}
		if (!MapaVoxels.ContainsKey(GenerarNombre()))
		{
			CrearVoxel(NuevaPosicion.x, NuevaPosicion.y, NuevaPosicion.z, GenerarNombre());
		}
	}

	void ModificarVoxel(TerrainVolume VolumenVoxelActual, PickSurfaceResult DatosRaycast)
	{
		//Es condicional que haya un renderer
		Material MaterialVolumen = VoxelActual.transform.GetChild(0).GetComponent<TerrainVolumeRenderer>().material;

		if(MaterialVolumen != null){
			MaterialVolumen.SetVector("BrushCenter", DatosRaycast.volumeSpacePos);				
			MaterialVolumen.SetVector("BrushSettings", new Vector4(RadioInternoBrocha, RadioExternoBrocha, OpacidadBrocha, 0.0f));
			MaterialVolumen.SetVector("BrushColor", new Vector4(0.0f, 0.5f, 1.0f, 1.0f));
		}
		
		switch(HerramientaActual)
		{
			case HERRAMIENTA.ESCULPIR:
				TerrainVolumeEditor.SculptTerrainVolume(VolumenVoxelActual, DatosRaycast.volumeSpacePos.x, DatosRaycast.volumeSpacePos.y, DatosRaycast.volumeSpacePos.z, RadioInternoBrocha, RadioExternoBrocha, OpacidadBrocha);
				break;
			case HERRAMIENTA.ALISAR:
				TerrainVolumeEditor.BlurTerrainVolume(VolumenVoxelActual, DatosRaycast.volumeSpacePos.x, DatosRaycast.volumeSpacePos.y, DatosRaycast.volumeSpacePos.z, RadioInternoBrocha, RadioExternoBrocha, OpacidadBrocha);
				break;
			case HERRAMIENTA.PINTAR:
				TerrainVolumeEditor.PaintTerrainVolume(VolumenVoxelActual, DatosRaycast.volumeSpacePos.x, DatosRaycast.volumeSpacePos.y, DatosRaycast.volumeSpacePos.z, RadioInternoBrocha, RadioExternoBrocha, OpacidadBrocha,(uint)TexturaSeleccionada);						
				break;
		}
		VolumenVoxelActual.ForceUpdate();
	}

	void CavarVoxel(TerrainVolume VolumenTerreno, int xPos, int yPos, int zPos, int rango)
	{
		// Initialise outside the loop, but we'll use it later.
		int rangoCuadrado = rango * rango;
		MaterialSet MaterialSetVacio = new MaterialSet();
		
		// Iterage over every voxel in a cubic region defined by the received position (the center) and
		// the range. It is quite possible that this will be hundreds or even thousands of voxels.
		for(int z = zPos - rango; z < zPos + rango; z++) 
		{
			for(int y = yPos - rango; y < yPos + rango; y++)
			{
				for(int x = xPos - rango; x < xPos + rango; x++)
				{			
					// Compute the distance from the current voxel to the center of our explosion.
					int xDistancia = x - xPos;
					int yDistancia = y - yPos;
					int zDistancia = z - zPos;
					
					// Working with squared distancias avoids costly square root operations.
					int distSquared = xDistancia * xDistancia + yDistancia * yDistancia + zDistancia * zDistancia;
					
					if(FormaCavar == FORMA_CAVAR.CUBO){
						VolumenTerreno.data.SetVoxel(x, y, z, MaterialSetVacio);
					}else if(FormaCavar == FORMA_CAVAR.ESFERA){
						if(distSquared < rangoCuadrado)
						{	
							VolumenTerreno.data.SetVoxel(x, y, z, MaterialSetVacio);
						}
					}
				}
			}
		}

		TerrainVolumeEditor.BlurTerrainVolume(VolumenTerreno, new Region(xPos - rango, yPos - rango, zPos - rango, xPos + rango, yPos + rango, zPos + rango));
	}
}


