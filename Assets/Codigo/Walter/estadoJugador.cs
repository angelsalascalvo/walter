using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class EstadoJugador : MonoBehaviour {

    //Ajustes publicos
    public int vida = 100;
    public int energiaPorRecarga = 2; //cantidad de energia que se obtiene en cada recarga
    public float tiempoRecarga = 6f; //El tiempo de una recarga de energia

    //Referencias publicas
    private PhotonView pView;
    public ControlAnimaciones controlAnimaciones;

    //Variables privadas
    private Text txtVida, txtMunicion, txtEnergia;
    private GameObject iconoEnergia;
    private int vidaAux;
    private int contEnergia; //Marcador de energia
    private float tiempoEnergia = 0;
    private int numBalas = 0;
    private bool iniciarCargaEnergia;
    private int estado; //Variable para marcar el estado en el que se encuentra el personaje (0.Inicio | 1.Previo sin arma | 2.Disparar | 3.Con Planta | 4.Reaparecer)
    private ControlJuego controlJuego;
    private GestionPlantas gestionPlantas;
    private Vector2 posPortalInstanciacion; //Posicion del portal de instanciacion del personaje

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    private void Awake() {
        //Iniciar variables
        contEnergia = 0;
        vidaAux = vida;
        iniciarCargaEnergia = true;

        //Obtener referencias
        pView = GetComponent<PhotonView>();
        controlJuego = GameObject.Find("ControladorJuego").GetComponent<ControlJuego>();
        //controlJuego.establecerMarcadores();

        gestionPlantas = GameObject.Find("ControladorJuego").GetComponent<GestionPlantas>();

        //Obtener referencia elemento interfaz
        txtEnergia = controlJuego.getEnergia();
        txtVida = controlJuego.getVida();
        txtMunicion = controlJuego.getMunicion();
        iconoEnergia = controlJuego.getIconoEnergia();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void Update() {
        //Actuamos si el script pertenece a nuestro jugador
        if (pView.IsMine) {
            //Actualizar vida
            if (txtVida != null) {
                txtVida.text = "" + Mathf.Clamp(vidaAux, 0, vida);
            }

            ///////////////////////////////// MUERTE //////////////////////////////////
            //Si la vida es igual a 0 llamamos al metodo para morir
            if (vidaAux <= 0) {
                gestionPlantas.ocuparPlanta(false);
                pView.RPC("morir_RPC", Photon.Pun.RpcTarget.AllBuffered);
            }

            //////////////////////////// COGER LA PLANTA /////////////////////////////
            //Actuar mientras tengamos la planta
            if (estado == 3) {
                //Primera ejecucion con la planta
                if (iniciarCargaEnergia) {
                    //Ejecutar una sola vez
                    iniciarCargaEnergia = false;
                    //Establecer como cargando
                    iconoEnergia.GetComponent<Animator>().SetBool("Cargando", true);
                    //Reproducir sonido
                    pView.RPC("sonidoPlanta_RPC", Photon.Pun.RpcTarget.AllBuffered, true);
                    
                }

                //Sumar el tiempo con la planta
                tiempoEnergia += Time.deltaTime;

                //Comprobar si se alcanza una recarga 
                if (tiempoEnergia >= tiempoRecarga) {
                    iconoEnergia.GetComponent<Animator>().SetBool("Cargando", false);
                    tiempoEnergia = 0; //Resetear el tiempo de energia
                    contEnergia += energiaPorRecarga; //Sumar la energia de la recarga
                    controlJuego.setMiEnergia(contEnergia); //Establecer valor en el contador del control de juego
                    txtEnergia.text = contEnergia.ToString("f0"); //Actualizar marcador
                    iniciarCargaEnergia = true;
                }

            //Si no tenemos la planta 
            } else {
                //Detener animacion cargando
                iconoEnergia.GetComponent<Animator>().SetBool("Cargando", false);
                //Detener sonido
                pView.RPC("sonidoPlanta_RPC", Photon.Pun.RpcTarget.AllBuffered, false);
                //Establecer variables
                tiempoEnergia = 0;
                iniciarCargaEnergia = true;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO RPC PARA REPRODUCIR O DETENER EN FUNCION DEL PARAMETRO EL SONIDO DE LA PLANTA TANTO EN EL JUGADOR ANFITRION COMO EN EL CONTRINCANTE
    */
    [Photon.Pun.PunRPC]
    public void sonidoPlanta_RPC(bool reproducir) {
        if (reproducir)
            GetComponent<AudioSource>().Play();
        else
            GetComponent<AudioSource>().Stop();

    }

    //------------------------------------------------------------------------------------------------------------------

    //////////////////////////////////////////////////// VIDA //////////////////////////////////////////////////////////
    
    /*
     * METODO PARA OBTENER LA CANTIDAD DE VIDA DEL PERSONAJE
     */
    public int getVida() {
        return vidaAux;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA REDUCIR VIDA AL PERSONAJE
     */
    public void reducirVida(int cantVida) {
        //Actuamos si el script pertenece a nuestro jugador y si aun tenemos vida
        if (pView.IsMine) {
            if (vidaAux > 0) {
                vidaAux -= cantVida;
                Handheld.Vibrate();
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA HACER MORIR AL PERSONAJE
     */
    [Photon.Pun.PunRPC]
    public void morir_RPC() {

        //Si somos nosotros mismos los que hemos muerto
        if (pView.IsMine) {

            //Si tenemos la planta la soltamos
            if (estado == 3) {
                setEstado(2); //Establecer estado de disparar
                //Metodo que suelta la planta en el punto indicado
                gestionPlantas.soltarPlanta(GetComponent<ControlesPantalla>().getPuntoSoltarPlanta().position);
            }

            //Bloquear todos los controles del personaje
            GetComponent<ControlesPantalla>().setBloqueadoBrazo(true);
            GetComponent<ControlesPantalla>().setBloqueadoSalto(true);
            GetComponent<Movimiento>().setBloqueado(true);
        }

        //Mover los brazos hasta la posicion 0
        gameObject.transform.GetChild(0).rotation = Quaternion.Lerp(gameObject.transform.GetChild(0).rotation, Quaternion.identity, Time.deltaTime * 30);

        //Comprobar que el personaje no esta en movimiento y que los brazos estan rectos
        if ((GetComponent<Rigidbody2D>().velocity.y == 0) && gameObject.transform.GetChild(0).rotation == Quaternion.identity) {
            //Si somos nosotros mismos los que hemos muerto
            if (pView.IsMine) {
                GetComponent<Movimiento>().setVivo(false); //Establecer que ha muerto el personaje
                GetComponent<ControlAnimaciones>().activarAnim("Muerte"); //Ejecutar animacion de muerte
                controlJuego.reaparecer();//Llamada al metodo para reaparecer
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    //////////////////////////////////////////////////// MUNICION //////////////////////////////////////////////////////   

    /*
     * METODO PARA AUMENTAR EL NUMERO DE BALAS DEL JUGADOR
     */
    public void aumentarBalas(int cantidad) {
        //Actualizamos si el script pertenece a nuestro jugador
        if (pView.IsMine) {
            numBalas += cantidad;
            //Actualizar Marcador
            txtMunicion.text = numBalas + "";
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA REDUCIR EL NUMERO DE BALAS DEL JUGADOR EN UNA
    */
    public void reducirBalas(int cantidad) {
        //Actualizamos si el script pertenece a nuestro jugador
        if (pView.IsMine) {
            numBalas -= cantidad;
            //Actualizar Marcador
            txtMunicion.text = numBalas + "";
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL NUMERO DE BALAS DEL JUGADOR
     */
    public void setNumBalas(int balas) {
        numBalas = balas;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL NUMERO DE BALAS DEL JUGADOR
     */
    public int getNumBalas() {
        return numBalas;
    }

    //------------------------------------------------------------------------------------------------------------------

    //////////////////////////////////////////////////// ENERGIA //////////////////////////////////////////////////////   

    /*
     * METODO PARA ESTABLECER EL CONTADOR DE ENERGIA OBTENIDA DEL JUGADOR
     */
    public void setContEnergia(int energia) {
        contEnergia = energia;
        controlJuego.setMiEnergia(contEnergia);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL CONTADOR DE ENERGIA OBTENIDA DEL JUGADOR
     */
    public int getContEnergia() {
        return contEnergia;
    }

    //------------------------------------------------------------------------------------------------------------------

    //////////////////////////////////////////////////// ESTADOS //////////////////////////////////////////////////////   

    public void preparado() {
        //Si somos el cliente maestro indicamos que estamos preparados directamente
        if (pView.IsMine) {
            //Solo actuar por parte de mi jugador (evitar que se ejecute preparado 2 veces por cada maquina)
            if (PhotonNetwork.IsMasterClient)
                GameObject.Find("ControladorJuego").GetComponent<ControlJuego>().preparado();
            //Si no somos el cliente maestro llamamos al metodo RPC para que se ejecute en el cliente maestro
            else
                pView.RPC("preparado_RPC", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA INDICAR AL USUARIO MAESTRO (SERVIDOR) QUE UN CLIENTE ESTA PREPARADO
     */
    [Photon.Pun.PunRPC]
    public void preparado_RPC() {
        if (PhotonNetwork.IsMasterClient)
            GameObject.Find("ControladorJuego").GetComponent<ControlJuego>().preparado();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER EL ESTADO DE DEL JUGADOR
    */
    public int getEstado() {
        return estado;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ESTABLECER EL ESTADO DEL JUGADOR
    */
    public void setEstado(int estado) {
        //Actualizar variable
        this.estado = estado;
        // En funcion del estado establecido aplicaremos las acciones correspondientes en el personaje
        switch (estado) {

            case 0:
                //Desbloquear el desplazamiento y el salto
                GetComponent<Movimiento>().setBloqueado(true);
                GetComponent<ControlesPantalla>().setBloqueadoSalto(true);
                GetComponent<ControlesPantalla>().setBloqueadoBrazo(true);
                GetComponent<ControlAnimaciones>().activarAnim("Quieto");
                break;

            //Estado sin arma coger municion
            case 1:
                //Desbloquear el desplazamiento y el salto
                GetComponent<Movimiento>().setBloqueado(false);
                GetComponent<ControlesPantalla>().setBloqueadoSalto(false);
                controlAnimaciones.sinArma(); //Mostrar Brazos sin arma
                GetComponent<ControlAnimaciones>().desactivarAnim("Quieto");
                break;

            //Estado con arma para la batalla
            case 2:
                //iconoEnergia.GetComponent<Animator>().SetBool("Cargando", false); //Si es la primera ejecucion //Detener la ejecucion del icono de carga si es necesario
                GetComponent<Movimiento>().setBloqueado(false);
                GetComponent<ControlesPantalla>().setBloqueadoBrazo(false); //Desbloquear el movimiento del brazo
                GetComponent<ControlesPantalla>().setBloqueadoSalto(false);
                controlAnimaciones.conArma(); //Mostrar brazos con arma
                break;

            //Estado sin arma con la planta
            case 3:
                tiempoEnergia = 0; //Reiniciar el tiempo de energia para que al coger la planta se pierda el prograso anterior de recarga
                GetComponent<ControlesPantalla>().setBloqueadoBrazo(true); //Bloquear el movimiento del brazo
                GetComponent<ControlAnimaciones>().sinArma();//Mostrar Brazos sin arma
                GetComponent<ControlAnimaciones>().activarAnim("BrazosPlanta"); //Animacion con planta
                break;

            //Estado reaparecer
            case 4:
                //Desbloquear movimiento
                GetComponent<Movimiento>().setBloqueado(false);
                break;
        }
    }
}