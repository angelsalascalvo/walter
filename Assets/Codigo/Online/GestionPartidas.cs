using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GestionPartidas : MonoBehaviourPunCallbacks
{
    //Referencias publicas
    public GameObject ventanaEmergente;
    public GameObject panelSinConexion;

    //Variables privadas
    private bool acceder;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        PhotonNetwork.LeaveRoom();
        acceder = false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * SOBRESCRITURA DEL METODO ONDISCONNECTED
     * Se ejecuta si el usuario se desconecta del servidor
     */
    public override void OnDisconnected(DisconnectCause cause) {
        base.OnDisconnected(cause);
        //mOSTRAR MENSAJE DE DESCONEXION
        panelSinConexion.gameObject.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA CREAR UNA BATALLA ALEATORIA
     */
    public void accederSalaAleatoria()
    {
        //Comprobar si estoy conectado
        if (PhotonNetwork.IsConnected) {
            //Acceder a una sala aleatoria con el metodo propio de photon
            PhotonNetwork.JoinRandomRoom();
            acceder = true;
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * SOBRESCRITURA DEL METODO OnJoinRandomFailed
     * Este se ejecuta cuando no encuentra ninguna room aleatoria a la que conectarse
     */
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        //Si no hay una sala aleatria libre, la creamos con 2 jugadores como máximo
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        acceder = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA SALIR DE LA SALA DE JUEGO EN LA QUE ME ENCUENTRO
    */
    public void salirSala() {
        //Comprobar si estoy en una sala
        if(PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA CREAR UNA SALA PARA JUGAR CON UN AMIGO EMPLEANDO UN NOMBRE DE SALA UNICO
    */
    public void crearSalaAmigo(string nombre) {
        //Comprobar si estoy conectado
        if (PhotonNetwork.IsConnected) {
            //Mostrar info al usuario
            ventanaEmergente.GetComponent<VentanaEspera>().mostrar("Batalla aceptada. Creando todo...");
            //Crear la sala con un nombre especifico
            PhotonNetwork.CreateRoom(nombre, new RoomOptions { MaxPlayers = 2 });
            acceder = true;
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ACCEDER A UNA SALA DE UN AMIGO A TRAVES DE UN NOMBRE DE SALA UNICO
    */
    public void accederSalaAmigo(string nombre) {
        //Comprobar si estoy conectado
        if (PhotonNetwork.IsConnected) {
            //Llamar al metodo para acceder
            StartCoroutine(accederSalaAmigoHilo(nombre));
            acceder = true;
        }
    }

    //======>

    /*
     * METODO QUE SE EJECUTA EN UN HILO NUEVO PARA INTENTAR CONECTARNOS REPETIDAMENTE A LA SALA EN CASO DE QUE AUN NO ESTE CREADA
     * Y NO LA ENCONTREMOS
     */
    IEnumerator accederSalaAmigoHilo(string nombre) {
        //Intentar conectarse mientras no estemos en en la sala
        do {
            PhotonNetwork.JoinRoom(nombre);
            yield return null;
        } while (!PhotonNetwork.InRoom);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void FixedUpdate()
    {
        //Comprobar si estamos en alguna room
        if (PhotonNetwork.CurrentRoom != null && acceder)
        {
            //Si estamos en una room que ha llegado a su numero maximo de jugadores, cargamos la escena de juego para empezar la batalla
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2){
                SceneManager.LoadScene("Pruebas Basicas");
            }
        }
    }
}