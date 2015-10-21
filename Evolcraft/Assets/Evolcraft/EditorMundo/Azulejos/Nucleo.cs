using UnityEngine;
using System.Collections;

public class Nucleo : MonoBehaviour
{

    protected   string      Nombre; //identificador
    protected   Elemento    ElementoBasico;
    public      GameObject  Modelo3d;  //Objeto3d que se cargara

    public string GetNombre()
    {
        return Nombre;
    }

    public Elemento GetElemento()
    {
        return ElementoBasico;
    }


}
