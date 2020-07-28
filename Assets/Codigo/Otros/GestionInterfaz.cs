using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GestionInterfaz : MonoBehaviour {

    //Referencias publicas
    public Button objBotonPlanta;
    public GestionPlantas gestionPlantas;
    public PhotonView pView;

    //--------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR U OCULTAR EL BOTON DE COGER PLANTA
     */
    public void estadoBotonPlanta(bool estado) {
        objBotonPlanta.gameObject.SetActive(estado);
    }

    //--------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE COGER PLANTA
     */
    public void botonPlanta() {
        //Obtener el objeto del personaje que ha cogido la planta
        GameObject personaje = GameObject.Find("miPersonaje");

        //Indicar que el personaje ha cogido la planta
        personaje.GetComponent<EstadoJugador>().setEstado(3);

        //Destruir la planta
        gestionPlantas.cogerPlanta();
    } 
}