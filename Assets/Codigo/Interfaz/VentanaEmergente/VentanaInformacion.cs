using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VentanaInformacion : MonoBehaviour
{

    //Referencias publicas
    public Text textoDescrip;
    public Button bAceptar;
    public GameObject ventanaConfirmacion;
    public GameObject ventanaInformacion;

    /*
     * METODO PARA MOSTAR UNA VENTANA EMERGENTE CON UNA INFORMACION
     */
    public void mostrarInfo(string texto, string botonAceptar) {
        Debug.Log("mostrarInfo");
        bAceptar.transform.GetChild(0).GetComponent<Text>().text = botonAceptar;
        textoDescrip.text = texto;
        //Mostrar la ventana adecuada
        ventanaConfirmacion.gameObject.SetActive(false);
        ventanaInformacion.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void aceptar() {
        ventanaInformacion.gameObject.SetActive(false);
    }
}
