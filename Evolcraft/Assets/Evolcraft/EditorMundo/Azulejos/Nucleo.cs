using UnityEngine;
using System.Collections;

public class Nucleo : MonoBehaviour
{

    protected   string      Nombre; //identificador
    protected   Elemento    ElementoBasico;
    public      GameObject  Modelo3d;  //Objeto3d que se cargara
    string GetNombre()
    {
        return Nombre;
    }

    Elemento GetElemento()
    {
        return ElementoBasico;
    }


}
