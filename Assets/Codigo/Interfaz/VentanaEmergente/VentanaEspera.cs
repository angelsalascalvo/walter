using UnityEngine;
using UnityEngine.UI;

public class VentanaEspera : MonoBehaviour
{
    //Referencias publicas
    public Text texto;
    public GameObject panelEspera;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA MOSTRAR LA VENTANA DE INFORMACION
    */
    public void mostrar(string txt) {
        //Mostrar ventana
        panelEspera.gameObject.SetActive(true);
        //Establecer el texto de la ventana.
        texto.text = txt;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OCULTAR LA PROPIA VENTANA QUE SE EJECUTARLA AL PULSAR EL BOTON DE ACEPTAR
    */
    public void ocultar() {
        panelEspera.gameObject.SetActive(false);
    }
}