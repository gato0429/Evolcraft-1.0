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


    //Tamaño del voxel
    int Profundidad   = 100;
    int Altura       = 32;
    int Ancho        = 100;
    //Prefab
    public GameObject VoxelPrefab;

    //Variables Mapa
    static string DirectorioLocal = Application.dataPath+"/Evolcraft/EditorMundo/Mapas/";
    static string NombreMapa="MapaDemo"; //Nombre de la carpeta del mapa

    
    //Voxels
    Dictionary<string, GameObject>  MapaVoxels= new Dictionary<string, GameObject>();
    GameObject                      VoxelActual;
    Vector3                         CentroActual;
    string                          ClaveActual;
    static DIRECCIONES Lado =       DIRECCIONES.ADELANTE;


    void Start ()
    {
         AssetDatabase.CreateFolder("Assets/Evolcraft/EditorMundo/Mapas", NombreMapa);
        CrearVoxel(0,0,0,"0#0#0");
    }

    bool CrearVoxel(float px, float py, float pz, string pnombre)
    {
        
        //Creacion del voxel la db
        TerrainVolumeData volumeData = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(new Region(0, 0, 0, Ancho - 1, Altura - 1, Profundidad - 1), @DirectorioLocal + NombreMapa+"/"+pnombre + ".vdb");
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

        GameObject VoxelTemporal = Instantiate(VoxelPrefab) as GameObject;
        VoxelTemporal.name = pnombre;
        VoxelTemporal.transform.GetChild(0).GetComponent<TerrainVolume>().data=volumeData;
       VoxelTemporal.transform.GetChild(1).Translate(Ancho/2, (float)DifAltura, Profundidad/2);

        VoxelTemporal.transform.position = new Vector3(px,py,pz);
        
        MapaVoxels.Add(pnombre, VoxelTemporal);
            VoxelActual = VoxelTemporal;
            ClaveActual = pnombre;
        
        return false;
    }

    // Update is called once per frame
    string PrepararNombre()
    {
        string[] Pos = ClaveActual.Split('#');

        int nx= Int32.Parse(Pos[0]); 
        int ny= Int32.Parse(Pos[1]);
        int nz= Int32.Parse(Pos[2]);

        string NuevoNombre;
        switch(Lado)
        {
            case DIRECCIONES.ATRAS:
                nz = nz - 1;
                break;
            case DIRECCIONES.ADELANTE:
                nz = nz + 1;
                break;
            case DIRECCIONES.IZQUIERDA:
                nx = nx - 1;
                break;
            case DIRECCIONES.DERECHA:
                nx = nx + 1;
                break;
            case DIRECCIONES.ARRIBA:
                ny = ny - 1;
                break;
            case DIRECCIONES.ABAJO:
                ny = ny + 1;
                break;
        }
        NuevoNombre = nx.ToString() + "#" + ny.ToString() + "#" + nz.ToString();
        return NuevoNombre;
    }
    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.name == "Centro")
            {
                Vector3 PosTemp = hit.collider.gameObject.transform.position;
                ClaveActual = hit.collider.gameObject.transform.parent.transform.name;
                CentroActual = hit.collider.gameObject.transform.parent.position;
            }

        }
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == "Centro")
        {
            Vector3 NuevaPosicion=CentroActual;


            switch (Lado)
            {
                case DIRECCIONES.ATRAS:
                    NuevaPosicion.z =NuevaPosicion.z - Profundidad ;
                    break;
                case DIRECCIONES.ADELANTE:
                    NuevaPosicion.z = NuevaPosicion.z + Profundidad;
                    break;
                case DIRECCIONES.IZQUIERDA:
                    NuevaPosicion.x = NuevaPosicion.x - Ancho ;
                    break;
                case DIRECCIONES.DERECHA:
                    NuevaPosicion.x = NuevaPosicion.x + Ancho ;
                    break;
                case DIRECCIONES.ARRIBA:
                    NuevaPosicion.y = NuevaPosicion.y + Altura ;
                    break;
                case DIRECCIONES.ABAJO:
                    NuevaPosicion.y = NuevaPosicion.y - Altura ;
                    break;
            }
            if (!MapaVoxels.ContainsKey(PrepararNombre()))
            {
                CrearVoxel(NuevaPosicion.x, NuevaPosicion.y, NuevaPosicion.z, PrepararNombre());
            }
        }

        else if(Input.GetKeyDown(KeyCode.Keypad2)){
				Lado =DIRECCIONES.ADELANTE;
		}else if(Input.GetKeyDown(KeyCode.Keypad8)){
            Lado = DIRECCIONES.ATRAS;
		}else if(Input.GetKeyDown(KeyCode.Keypad5)){
            Lado = DIRECCIONES.ARRIBA;
		}else if(Input.GetKeyDown(KeyCode.Keypad0)){
            Lado = DIRECCIONES.ABAJO;
		}else if(Input.GetKeyDown(KeyCode.Keypad4)){
            Lado = DIRECCIONES.IZQUIERDA;
		}else if(Input.GetKeyDown(KeyCode.Keypad6)){
            Lado = DIRECCIONES.DERECHA;
		}  
    }
}
