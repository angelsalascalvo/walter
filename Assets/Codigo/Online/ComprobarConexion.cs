using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComprobarConexion : MonoBehaviourPunCallbacks {

    //Referencias publicas
    public GameObject panelSinConexion;
    public Canvas canvasPantalla;
    public GameObject pConexion;
    public Text txtPing;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        pConexion.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * SOBRESCRITURA DEL METODO ONDISCONNECTED
     * Se ejecuta si el usuario se desconecta del servidor
     */
    public override void OnDisconnected(DisconnectCause cause) {
        base.OnDisconnected(cause);
        //mostrar panel para notificar al usuario
        panelSinConexion.gameObject.SetActive(true);
        canvasPantalla.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void FixedUpdate() {
        //Comprobar el ping para indicar mala conexion si es necesario
        if (PhotonNetwork.GetPing() >= 200) {
            txtPing.text = "Ping: " + PhotonNetwork.GetPing();
            pConexion.SetActive(true);
        } else {
            pConexion.SetActive(false);
        }
    }
}