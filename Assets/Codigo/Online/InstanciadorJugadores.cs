using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanciadorJugadores : MonoBehaviour {

    //Referencias publicas
    public GameObject jugador;
    public Transform PortalJug1;
    public Transform PortalJug2;
    public PhotonView pView;
    public InfoPersistente infoPersistente;

    //Variables privadas
    private GameObject miPortal;
    private GameObject portalEnemigo;
    private GameObject jugCreado;
    private bool abierto=false, unicaEjecucion=true;
    private float tiempoParaCerrar = 2, tiempoAux;


    //-------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        tiempoAux = tiempoParaCerrar;

        //instanciar el jugador en el portal adecuado y remplazar los nombres de los objetos
        if (PhotonNetwork.IsMasterClient) {
            Photon.Pun.PhotonNetwork.Instantiate(jugador.name, PortalJug1.position, Quaternion.identity, 0);
            miPortal = GameObject.Find("PortalJug1");
            portalEnemigo = GameObject.Find("PortalJug2");
        } else {
            Photon.Pun.PhotonNetwork.Instantiate(jugador.name, PortalJug2.position, Quaternion.identity, 0);
            miPortal = GameObject.Find("PortalJug2");
            portalEnemigo = GameObject.Find("PortalJug1");
        }
    }

    //-------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA REGENERAR UN PERSONAJE CUANDO ESTE MUERE
     */
    public void regenerar(int balas, int energia) {

        //En funcion del cliente que sea lo instanciaremos en un portal u otro
        if (PhotonNetwork.IsMasterClient) {
            jugCreado = Photon.Pun.PhotonNetwork.Instantiate(jugador.name, PortalJug1.position, Quaternion.identity, 0);
        } else {
            jugCreado = Photon.Pun.PhotonNetwork.Instantiate(jugador.name, PortalJug2.position, Quaternion.identity, 0);
        }

        //Establecer el estado inicial del jugador
        jugCreado.GetComponent<EstadoJugador>().setContEnergia(energia);
        jugCreado.GetComponent<EstadoJugador>().setNumBalas(balas);
        jugCreado.GetComponent<EstadoJugador>().setEstado(0);

        //Establecer ajustes
        jugCreado.GetComponent<ControlesPantalla>().setModo(infoPersistente.getModoControl());
        jugCreado.GetComponent<ControlesPantalla>().setSensibilidad(infoPersistente.getSenApunt() / 2);
        jugCreado.GetComponent<Movimiento>().setSensibilidadMovi((infoPersistente.getSenMov() * 25) / 8);

        //Establecer Apariencia
        jugCreado.GetComponent <AparienciaWalter>().establecerApariencia(infoPersistente.getAparienciaActiva());
        //Ejecutar animacion de abrir el portal
        miPortal.GetComponent<Animator>().SetTrigger("Abrir");
        //Ejecutar animacion en el oponente
        pView.RPC("anim_RPC", Photon.Pun.RpcTarget.OthersBuffered, "Abrir");
        abierto = true;
    }


    //---------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ENVIAR EL PORTAL DELANTE DEL PERSONAJE EN EL MOMENTO ADECUADO
    */
    public void enviarDelate() {
        miPortal.GetComponent<Animator>().SetTrigger("EnviarDelante");
        //Ejecutar animacion en el oponente
        pView.RPC("anim_RPC", Photon.Pun.RpcTarget.OthersBuffered, "EnviarDelante");
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO RPC PARA EJECUTAR LAS ANIMACIONES SOBRE EL PORTAL EN EL DISPOSITIVO DEL OPONENTE
    */
    [Photon.Pun.PunRPC]
    public void anim_RPC(string trigger) {
        //Ejecutar animación mediante el trigger
        portalEnemigo.GetComponent<Animator>().SetTrigger(trigger);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void FixedUpdate() {
        if (abierto) {
            //Comprobar cuando ha terminado de abrirse para volver a cerrar
            if (miPortal.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) {
                //Esperar el tiempo predefinido hantes de cerrar 
                if (tiempoAux > 0) {
                    //Activar el arma en el jugador al abrir el portal
                    if (unicaEjecucion) {
                        unicaEjecucion = false;
                        jugCreado.GetComponent<EstadoJugador>().setEstado(2);
                    }

                    //Restar tiempo
                    tiempoAux -= Time.deltaTime;

                //Cerrar el portar tras pasar el tiempo definido
                } else {
                    //Cambiar estado de las variables
                    unicaEjecucion = true;
                    abierto = false;
                    tiempoAux = tiempoParaCerrar;
                    //Ejecutar animacion
                    miPortal.GetComponent<Animator>().SetTrigger("Cerrar");
                    //Ejecutar animacion en el dispositivo oponente
                    pView.RPC("anim_RPC", Photon.Pun.RpcTarget.OthersBuffered, "Cerrar");
                }
            }
        }
    }
}