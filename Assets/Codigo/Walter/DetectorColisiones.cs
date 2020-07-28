using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorColisiones : MonoBehaviour {

    //Referencias publicas
    public EstadoJugador estadoJugador;
    public Transform puntoA;
    public Transform puntoB;
    public LayerMask mascaraPlanta;

    //Variables privadas
    private GestionMunicion generadorMunicion;
    private GestionInterfaz gestorInterfaz;
    private GestionPlantas gestionPlantas;


    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        generadorMunicion = GameObject.Find("ControladorJuego").gameObject.GetComponent<GestionMunicion>();
        gestorInterfaz = GameObject.Find("ControladorJuego").gameObject.GetComponent<GestionInterfaz>();
        gestionPlantas = GameObject.Find("ControladorJuego").gameObject.GetComponent<GestionPlantas>();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA CADA CIERTO TIEMPO FIJO EN LUGAR DE CADA FOTOGRAMA (OPTIMO PARA LA GESTION DE FISICAS)
     */
    public void FixedUpdate() {
        //Comprobar si estamos cerca de la planta y estamos vivos
        if (Physics2D.OverlapArea(puntoA.position, puntoB.position, mascaraPlanta) && estadoJugador.getVida() > 0) {

            //Es la primera vez que entramos en la planta 
            if (gestionPlantas.getEstoyCercaPlanta() == false) {
                //Marcamos la planta como ocupada
                gestionPlantas.ocuparPlanta(true);
            }
           
        } else {
            //Es la primera vez que salimos de la planta
            if (gestionPlantas.getEstoyCercaPlanta()) {
                //Marcamos la planta como no ocupada
                gestionPlantas.ocuparPlanta(false);
            }
        }

        //Si estoy cerca de la planta
        if (gestionPlantas.getEstoyCercaPlanta()) 
            //Dependiendo de si la planta esta ocupada mostramos el boton o no
            gestorInterfaz.estadoBotonPlanta(!gestionPlantas.getPlantaOcupada());
        else
            //Si no estoy cerca de la planta directamente no se muestra en boton
            gestorInterfaz.estadoBotonPlanta(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTARÁ AL PRODUCIRSE UNA COLISION CON UN COLLIDER ONTRIGGER
     */
    void OnTriggerEnter2D(Collider2D collision) {
        //////////////////////// MUNICION //////////////////////////
        if (collision.gameObject.tag == "municion") {
            GameObject cajaMunicion = collision.gameObject;

            //Obtener referencia si no se ha obtenido
            if (generadorMunicion == null)
                generadorMunicion = GameObject.Find("ControladorJuego").gameObject.GetComponent<GestionMunicion>();

            //Desactivar los collider de la caja
            generadorMunicion.desactivarCaja(cajaMunicion.GetComponent<Municion>().name);

            //Si la caja no esta abierta
            if (!cajaMunicion.GetComponent<Municion>().isAbierta()) {
                //Aumentar las balas
                estadoJugador.aumentarBalas(cajaMunicion.GetComponent<Municion>().obtenerPropiedades());
                //Marcar como abierta
                cajaMunicion.GetComponent<Municion>().abrirCaja();
            }
        }
    }
}