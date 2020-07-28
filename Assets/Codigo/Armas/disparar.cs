using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disparar : MonoBehaviourPun
{
    //Ajustes publicos
    public float fuerzaDisparo=2f;
    public float tiempoDisparoMax = 0.5f;

    //Referencias publicas
    public GameObject bala;
    public GameObject brazo;
    public Movimiento mov;
    public ControlAnimaciones controlAnimaciones;

    //Variables privadas
    private float inclinacion, porcentX, porcentY;
    private float tiempoDisparo;
    private PhotonView pView;
    private EstadoJugador estadoJugador;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    private void Start()
    {
        estadoJugador = transform.GetComponentInParent<EstadoJugador>();
        pView = GetComponent<PhotonView>();
        //Establecer el tiempo que se tendra que cumplir entre disparo y disparo
        tiempoDisparo = tiempoDisparoMax;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update()
    {
        //Restar tiempo
        tiempoDisparo -= Time.deltaTime;
    }

    //-------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE PERMITIRA DISPARAR UNA BALA EN LA DIRECCION 
     */
    public void disparo(bool invertido) {
        //Disparamos si se ha cumplido el tiempo de repeticion entre disparos y si tenemos balas
        if (tiempoDisparo <= 0 && estadoJugador.getNumBalas()>0)
        {
            //Establecer el tiempo que se tendra que cumplir entre disparo y disparo
            tiempoDisparo = tiempoDisparoMax;

            //Obtener la inclinacion a la que debemos aplicar la fuerza de la bala comprendida entre 90º y -90º
            if (brazo.transform.eulerAngles.z >= 90)
                inclinacion = brazo.transform.eulerAngles.z - 360;
            else
                inclinacion = brazo.transform.eulerAngles.z;

            //Calcular velocidades necesaria para aplicar fuerza en el eje X e Y para que la bala siga la misma inclinacion
            if (inclinacion < 0)
                porcentX = -1 * ((-90 - inclinacion) / 90);
            else
                porcentX = (90 - inclinacion) / 90;

            porcentY = inclinacion / 90;

            //Si el personaje esta inverido, invertimos la velocidad del eje X e Y
            if (mov.getInvertido())
            {
                porcentX = porcentX * -1;
                porcentY = porcentY * -1;
            }

            //Calcular velocidad del elemento multiplicandola por la fuerza del disparo
            Vector2 velocidad = new Vector2(porcentX * fuerzaDisparo, porcentY * fuerzaDisparo);

            //Llamada al RPC que ejecuta el disparo en todas las maquinas
            pView.RPC("disparo_RPC", Photon.Pun.RpcTarget.AllBuffered, transform.position, brazo.GetComponent<Transform>().rotation, velocidad, invertido);

            //Ejecutar anaimacion en la pistola
            controlAnimaciones.animacionDisparar();

            //Reducir el numero de balas del jugador en 1
            estadoJugador.reducirBalas(1);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
    * METODO RPC PARA CREAR Y DAR MOVIMIENTO A LA BALA EN TODOS LOS CLIENTES
    */
    [Photon.Pun.PunRPC]
    public void disparo_RPC(Vector3 posicion, Quaternion rotacion, Vector2 velocidadMov, bool invertido) {
        //Crear Bala
        GameObject balaCreada = Instantiate(bala, posicion, rotacion);
        if (invertido)
            balaCreada.transform.localScale= new Vector3(-0.89f, 1, 1);

        //Aplicar Velocidad
        balaCreada.GetComponent<Rigidbody2D>().velocity = velocidadMov;

        //Reproducir sonido disparo
        GetComponent<AudioSource>().Play();
    }
}