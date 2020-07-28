using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VentanaConfirmacion : MonoBehaviour {

    //Referencias publicas
    public Text textoDescrip;
    public Button bAceptar;
    public Button bCancelar;
    public GameObject ventanaConfirmacion;
    public GameObject ventanaInformacion;

    //Variables Privadas
    private int aceptado;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTAR UNA VENTANA EMERGENTE CON UNA CONFIRMACION
     */
    public VentanaConfirmacion crearConfirmacion(string texto, string botonAceptar, string botonCancelar) {
        //Inicializacion
        aceptado = 0;
        //Establecer texto a los botones
        bAceptar.transform.GetChild(0).GetComponent<Text>().text = botonAceptar;
        bCancelar.transform.GetChild(0).GetComponent<Text>().text = botonCancelar;
        textoDescrip.text = texto;
        //Mostrar la ventana adecuada
        ventanaConfirmacion.gameObject.SetActive(true);
        ventanaInformacion.gameObject.SetActive(false);
        gameObject.SetActive(true);
        return this;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE ACEPTAR DE LA VENTA, ESTABLECIENDO EL VALOR DEL BOTON Y OCULTANDO LA PROPIA VENTANA
     */
    public void aceptar() {
        aceptado = 1;
        ventanaConfirmacion.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE CANCELAR DE LA VENTA, ESTABLECIENDO EL VALOR DEL BOTON Y OCULTANDO LA PROPIA VENTANA
     */
    public void cancelado() {
        aceptado = 2;
        ventanaConfirmacion.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER EL VALOR DE ACEPTADO QUE CORRESPONDE CON EL BOTON PULSADO
    */
    public int getAceptado() {
        return aceptado;
    }
}