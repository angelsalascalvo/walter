using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class GestionServidorPhoton : MonoBehaviourPunCallbacks {

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Establecer los ajustes de la conexion con photon
        PhotonNetwork.AutomaticallySyncScene = true; //Sincronizar escena automáticamente
        PhotonNetwork.GameVersion = "v1"; //Version del juego
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER CONEXIÓN CON EL SERVIDOR DE PHOTON
     */
    public void conectarConServidor() {
        PhotonNetwork.ConnectUsingSettings();
    }
}
