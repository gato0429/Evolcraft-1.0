using UnityEngine;
using System.Collections;
using Cubiquity;
using Cubiquity.Impl;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

public class EditorTerrenos : MonoBehaviour {

    //aun falta la serializacion
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
		DEFORMAR_BALDOSAS
	};

	enum HERRAMIENTA
	{
		ESCULPIR,
		ALISAR,
		PINTAR
	};

	//Estado Editor
	static ESTADO EstadoEditor = ESTADO.CREAR_BALDOSAS;

    //Tamaño del voxel
    int Profundidad  = 100;
    int Altura       = 32;
    int Ancho        = 100;
    //Prefab
    public GameObject VoxelPrefab;
	//Este voxel prefab debe tener dos hijos:
	//El primero con TerrainVolume...
	//El segundo es un cubo


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
	int BrochaSeleccionada = 0;
    float RadioExternoBrocha = 5.0f;
    float OpacidadBrocha = 1.0f;

	float FactorEscalaInternoBrocha;
	float RadioInternoBrocha;
	int TexturaSeleccionada = 2;

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
		RadioInternoBrocha = RadioExternoBrocha * FactorEscalaInternoBrocha;//Cuando se cambie alguno...
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
		if (Input.GetMouseButtonDown(0))
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				if(hit.collider != null){
					if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
						if(hit.collider.gameObject.name == "Centro")
						{
							ActualizarVoxelActual(hit);
							AgregarVoxel();
						}
					}else if(EstadoEditor == ESTADO.DEFORMAR_BALDOSAS)
					{
                        TerrainVolume VolumenVoxelActual = VoxelActual.transform.GetChild(0).GetComponent<TerrainVolume>();

						if(hit.collider.gameObject.name == "Centro")
						{
							VolumenVoxelActual.data.CommitChanges();
							ActualizarVoxelActual(hit);
						}
					}
				}
			}
		}else if(Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && (Input.mousePosition != PosicionPreviaMouse))){
			if(EstadoEditor == ESTADO.DEFORMAR_BALDOSAS)
			{
				TerrainVolume VolumenVoxelActual = VoxelActual.transform.GetChild(0).GetComponent<TerrainVolume>();
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				PickSurfaceResult DatosRaycast;
				bool Choco = Picking.PickSurface(VolumenVoxelActual, ray.origin, ray.direction, 1000.0f, out DatosRaycast);
				if(Choco)
				{
					PosicionPreviaMouse = Input.mousePosition;
					ModificarVoxel(VolumenVoxelActual,DatosRaycast);
				}

			}
		}
		else if(Input.GetKeyDown(KeyCode.Alpha1)){ //Todos estos deberian funcionar en determinado estado
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
				Lado = DIRECCIONES.ADELANTE;
			}else if(EstadoEditor == ESTADO.DEFORMAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.ESCULPIR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha2)){
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
				Lado = DIRECCIONES.ATRAS;
			}else if(EstadoEditor == ESTADO.DEFORMAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.ALISAR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha3)){
			if(EstadoEditor == ESTADO.CREAR_BALDOSAS){
	            Lado = DIRECCIONES.ARRIBA;
			}else if(EstadoEditor == ESTADO.DEFORMAR_BALDOSAS){
				HerramientaActual = HERRAMIENTA.PINTAR;
			}
		}else if(Input.GetKeyDown(KeyCode.Alpha4)){
            Lado = DIRECCIONES.ABAJO;
		}else if(Input.GetKeyDown(KeyCode.Alpha5)){
            Lado = DIRECCIONES.IZQUIERDA;
		}else if(Input.GetKeyDown(KeyCode.Alpha6)){
            Lado = DIRECCIONES.DERECHA;
		}else if(Input.GetKeyDown(KeyCode.F1)){
			EstadoEditor = ESTADO.CREAR_BALDOSAS;
		}else if(Input.GetKeyDown(KeyCode.F2)){
			EstadoEditor = ESTADO.DEFORMAR_BALDOSAS;
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

}


