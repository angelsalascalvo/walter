using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Municion : MonoBehaviour
{
    //Referencias Publicas
    public Text txtNumBalas;
    public Transform numBalasPos;
    public Canvas canvasNumeros;

    //Variables privadas
    private int numBalas; //Balas que tiene la Municion
    private bool abierta;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Inicializar variables
        abierta = false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update() {
        //Situar el numero de balas, en la posicion correspondiente respecto a la camara
        Vector3 posicion = Camera.main.WorldToScreenPoint(numBalasPos.position);
        txtNumBalas.transform.position = posicion;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTNER LAS PROPIEDADES DE LA CAJA DE MUNICION
     */
    public int obtenerPropiedades() {
        return numBalas;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LAS PROPIEDADES DE LA CAJA
     */
    public void establecerPropiedades(int propiedades) {
        numBalas = propiedades;
        txtNumBalas.text = numBalas + "";
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA CAJA COMO ABIERTA
     */
    public void abrirCaja() {
        abierta = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI LA CAJA ESTA ABIERTA
     */
    public bool isAbierta() {
        return abierta;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ACTIVAR O DESACTIVAR EL NUMEORO DE MUNICION EN FUNCION DEL PARAMETRO
    */
    public void estadoNumeros(bool estado) {
        canvasNumeros.gameObject.SetActive(estado);
    }
}