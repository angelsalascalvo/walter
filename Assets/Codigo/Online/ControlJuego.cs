using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ControlJuego : MonoBehaviour
{
    //Ajustes publicos
    public float tiempoCuentaAtras = 3f; //El tiempo de la cuenta atras
    public float tiempoFase1 = 10f; //El tiempo de la fase de inicio
    public float tiempoFase2 = 15f;
    public float tiempoReaparecer;
    public Text txtVida;
    public Text txtMunicion;
    public Text txtEnergia;

    //Referencias publicas
    public PhotonView pView;
    public GestionMunicion generadorMunicion;
    public GestionPlantas gestionPlantas;
    public InstanciadorJugadores instanciadorJugadores;
    public GameObject pantallaMarcadores;
    public GameObject iconoEnergia;
    public FinalBatalla finalBatalla;
    public Animator animatorPortal1;
    public Animator animatorPortal2;
    public GameObject cartelFase1, cartelFase2;
    public Text txtTiempo;
    public Text txtRegeneradndo;
    public GameObject panelCuentaAtras;
    public GameObject panelReaparecer;
    public Text txtCuentaAtras;

    //Variables privadas
    private bool fase1, fase2, cuentaAtras, reapareciendo, finBatalla;
    private GameObject miPersonaje;
    private int jugadoresPreparados=0;
    private GameObject []cajasMunicion;
    private float tiempoReaparecerAux;
    private bool unicaEjecucion;
    private int balasJugador, energiaJugador; //Estado del jugador previo a morir
    private int miEnergia, energiaEnemigo;
    private InfoPersistente infoPersistente;
    private ReferenciaBaseDatos refBD;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();

        //Resetear los nodos de juego de la base de datos
        infoPersistente.setJugando(true);
        infoPersistente.setPeticionBatalla("null");
        infoPersistente.setRespuestaBatalla("null");        
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");

        //Inicializacion
        txtCuentaAtras.text = tiempoCuentaAtras.ToString("f0");
        panelCuentaAtras.gameObject.SetActive(false);
        txtCuentaAtras.text = tiempoReaparecer.ToString("f0");
        panelReaparecer.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start()
    {
        //Indicar estado inicial de las variables
        cuentaAtras = false;
        fase1 = false;
        fase2 = false;
        reapareciendo = false;
        finBatalla = false;
        unicaEjecucion = true;
        miEnergia = 0;
        energiaEnemigo =0;

        //Inicializacion
        miPersonaje = GameObject.Find("miPersonaje");
        tiempoReaparecerAux = tiempoReaparecer;
        pantallaMarcadores.gameObject.SetActive(false);

        //Comprobar si este cliente es el anfitrion (Master) para no generar la municion 2 veces
        if (PhotonNetwork.IsMasterClient) {
            generadorMunicion.generar();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update()
    {
        //Empezar cuando los 2 jugadores esten preparados 
        if (jugadoresPreparados == 2) {
            if (PhotonNetwork.IsMasterClient)
                pView.RPC("empezarCuentaAtras_RPC", Photon.Pun.RpcTarget.AllBuffered); //Comenzar cuenta atrás a la vez en los 2 jugadores
        }

        ////////////////   0. CUENTA ATRAS /////////////////////
        if (cuentaAtras) {
            //Restar el tiempo transcurrido
            tiempoCuentaAtras -= Time.deltaTime;
            if (tiempoCuentaAtras > 0)
                txtCuentaAtras.text = "" + tiempoCuentaAtras.ToString("f0");

            //Fin cuenta atras
            if (tiempoCuentaAtras <= 0) {
                //Comprobar si este cliente es el anfitrion (Master)
                if (PhotonNetwork.IsMasterClient)
                    //Ejecutar el RPC (En todos los clientes)
                    pView.RPC("empezarFase1_RPC", Photon.Pun.RpcTarget.AllBuffered);
                    
            }

        ////////////////   1. FASE 1  /////////////////////
        } else if (fase1) {
            //Restar el tiempo transcurrido
            tiempoFase1 -= Time.deltaTime;
            if (tiempoFase1 > 0) 
                txtTiempo.text = "" + tiempoFase1.ToString("f0");

            //Fin de la primera fase
            else if (tiempoFase1 <= 0) {
                if (PhotonNetwork.IsMasterClient)
                    //Ejecutar el RPC (En todos los clientes)
                    pView.RPC("empezarFase2_RPC", Photon.Pun.RpcTarget.AllBuffered);
            }

        ////////////////   2. FASE 2  /////////////////////
        } else if (fase2) {
            //Restar el tiempo transcurrido
            tiempoFase2 -= Time.deltaTime;

            if (tiempoFase2 > 0)
                txtTiempo.text = "" + tiempoFase2.ToString("f0");

            //Fin de la partida
            else if (tiempoFase2 <= 0) {
                if (PhotonNetwork.IsMasterClient)
                    //Ejecutar el RPC (En todos los clientes)
                    pView.RPC("finPartida_RPC", Photon.Pun.RpcTarget.AllBuffered);
            }
        }

        //////////////////////// REAPARICION /////////////////////////
        //Si estamos reapareciendo
        if (reapareciendo) {
            tiempoReaparecerAux -= Time.deltaTime;
            txtRegeneradndo.text = "" + tiempoReaparecerAux.ToString("f0");

            //Cuando el contador de reaparicion llegue a la mitad eliminamos el personaje si no lo hemos hecho ya
            if (tiempoReaparecerAux <= tiempoReaparecer / 2) {
                //Obtener el personaje
                miPersonaje = GameObject.Find("miPersonaje");

                //Comprobar si el personaje aun no se ha eliminado
                if (miPersonaje!=null) {
                    instanciadorJugadores.enviarDelate(); //Enviar portal delante
                    balasJugador = miPersonaje.GetComponent<EstadoJugador>().getNumBalas();
                    energiaJugador = miPersonaje.GetComponent<EstadoJugador>().getContEnergia();

                    //Eliminar el personaje
                    PhotonNetwork.Destroy(miPersonaje);
                }
            }

            //Comprobar si se ha completado el tiempo para reaparecer
            if (tiempoReaparecerAux <= 0) {
                //Regenerar el personaje
                instanciadorJugadores.regenerar(balasJugador, energiaJugador);

                //Restaurar estado variables
                reapareciendo = false;
                panelReaparecer.gameObject.SetActive(false);
                tiempoReaparecerAux = tiempoReaparecer;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA INICIAR LA FASE DE CUENTA ATRAS PARA TODOS LOS JUGADORES 
     */
    [Photon.Pun.PunRPC]
    public void empezarCuentaAtras_RPC() {
        //Cambiar estado variables
        panelCuentaAtras.gameObject.SetActive(true);
        cuentaAtras = true;
        jugadoresPreparados = -1;

        //Obtener mi personaje si no lo he obtenido previamente
        if (miPersonaje == null)
            miPersonaje = GameObject.Find("miPersonaje");

        //Establecer el estado inmovil del personaje
        miPersonaje.GetComponent<EstadoJugador>().setEstado(0);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO RPC QUE DARÁ INICIO A LA FASE 2 DEL JUEGO PARA TODOS LOS JUGADORES 
    */
    [Photon.Pun.PunRPC]
    public void empezarFase1_RPC() {
        //Cerramos portales
        animatorPortal1.SetTrigger("Cerrar");
        animatorPortal2.SetTrigger("Cerrar");

        //Mostrar marcadores del jugador
        pantallaMarcadores.gameObject.SetActive(true);
        cartelFase1.gameObject.SetActive(true);
        cartelFase2.gameObject.SetActive(false);

        //Mostrar numeros de las cajas de municion
        //Obtener todas las cajas de municion y ocultar los numeros
        cajasMunicion = GameObject.FindGameObjectsWithTag("municion");

        foreach (var caja in cajasMunicion) {
            caja.GetComponent<Municion>().estadoNumeros(true);
        }

        //Obtener mi personaje si no lo he obtenido previamente
        if (miPersonaje == null)
            miPersonaje = GameObject.Find("miPersonaje");

        //Cambiar estado variables que se ejecutan en el update
        fase1 = true;
        cuentaAtras = false;
        panelCuentaAtras.SetActive(false);

        //Establecer el estado inicial del personaje (sin arma)
        miPersonaje.GetComponent<EstadoJugador>().setEstado(1);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE DARÁ INICIO A LA FASE 2 DEL JUEGO PARA TODOS LOS JUGADORES 
    */
    [Photon.Pun.PunRPC]
    public void empezarFase2_RPC() {

        fase1 = false;
        fase2 = true;

        //Mostrar indicacion
        cartelFase1.gameObject.SetActive(false);
        cartelFase2.gameObject.SetActive(true);

        //Obtener mi personaje si no lo he obtenido previamente
        if (miPersonaje == null)
            miPersonaje = GameObject.Find("miPersonaje");

        //Establecer el estado del personaje (con arma)
        miPersonaje.GetComponent<EstadoJugador>().setEstado(2);

        //Activar la planta si somos el cliente maestro de la partida
        if (PhotonNetwork.IsMasterClient) {
            gestionPlantas.activacion();
        }
    }

    //-----------------------------------------------------------------------------------

    /*
    * METODO QUE DARÁ INICIO A LA FASE 2 DEL JUEGO PARA TODOS LOS JUGADORES 
    */
    [Photon.Pun.PunRPC]
    public void finPartida_RPC() {
        fase2 = false;
        fase1 = false;
        finBatalla = true;

        //Ocultar numeros de las cajas de municion
        //Obtener todas las cajas de municion y ocultar los numeros
        cajasMunicion = GameObject.FindGameObjectsWithTag("municion");
        foreach (var caja in cajasMunicion) {
            caja.GetComponent<Municion>().estadoNumeros(false);
        }

        //Comprobar que jugador es el ganador en funcion de la energia recolectada
        if (miEnergia > energiaEnemigo) {
            _ = finalBatalla.mostrarAsync("ganado", miEnergia);
        } else if (miEnergia < energiaEnemigo) {
            _ = finalBatalla.mostrarAsync("perdido", miEnergia);
        } else {
            _ = finalBatalla.mostrarAsync("empatado", miEnergia);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void FixedUpdate() {
        //Comprobar en todo momento si el oponente se desconecta siempre que no haya terminado la batalla
        if (PhotonNetwork.CurrentRoom != null) {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2 && finBatalla == false) {
                if (miPersonaje == null)
                    miPersonaje = GameObject.Find("miPersonaje");
                if (unicaEjecucion) {
                    unicaEjecucion = false;
                    //Ejecutar final de la batalla por desconexion del oponente
                    _ = finalBatalla.mostrarAsync("desconectado", miPersonaje.GetComponent<EstadoJugador>().getContEnergia());
                }
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INDICAR QUE EL JUGADOR ESTA LISTO PARA COMENZAR LA PARTIDA
     */
    public void preparado() {
        jugadoresPreparados++;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA HACER REAPARECER AL JUGADOR TRAS MORIR
     */
    public void reaparecer() {
        reapareciendo = true; //Activar la reaparicion
        panelReaparecer.gameObject.SetActive(true); //Mostrar cuenta de reaparicion
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA CAMBIAR MI CONTADOR DE ENERGIA Y CAMBIARLO EN EL DISPOSITIVO DEL CONTRINCANTE 
    */
    public void setMiEnergia(int energia) {
        //Mientras no estemos en el final de la partida
        if (!finBatalla) {
            miEnergia = energia;
            //Lamada al metodo para establecer la energia en el dispositivo del oponente (solo se ejecuta en contrincante)
            pView.RPC("establecerEnergia_RPC", Photon.Pun.RpcTarget.OthersBuffered, miEnergia);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA CAMBIAR EL VALOR DE LA ENERGIA DEL ENEMIGO
     */
    [Photon.Pun.PunRPC]
    public void establecerEnergia_RPC(int energia) {
        //Cambiar la cantidad de energia del enemigo
        energiaEnemigo = energia;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER LA CANTIDAD DE VIDA
    */
    public Text getVida() {
        return txtVida;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER LA CANTIDAD DE MUNICION
    */
    public Text getMunicion() {
        return txtMunicion;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER LA CANTIDAD DE ENERGIA
    */
    public Text getEnergia() {
        return txtEnergia;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER EL OBJETO CORRESPONDIENTE CON EL ICONO DE ENERGIA
    */
    public GameObject getIconoEnergia() {
        return iconoEnergia;
    }
}