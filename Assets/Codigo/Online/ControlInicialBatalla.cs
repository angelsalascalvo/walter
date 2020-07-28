using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlInicialBatalla : MonoBehaviour
{
    //Variables privadas
    private InfoPersistente infoPersistente;
    private ControlesPantalla controlesPantalla;
    private AparienciaWalter aparienciaWalter;
    private Movimiento movimiento;
    private GameObject miPersonaje = null;
    private bool primeraEjecucion = true;

    //Referencias Publicas
    public Text txtNombreMiJugador;
    public Text txtNombreOponente;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();

        //Establecer nombre de jugadores
        txtNombreMiJugador.text = infoPersistente.getMiNombre();
        txtNombreOponente.text = infoPersistente.getNombreOponente();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update() {
        if (miPersonaje == null) {
            miPersonaje = GameObject.Find("miPersonaje");

        //Si obtengo el personaje y es la primera vez que lo ejecuto, efectuo las acciones
        }else if (primeraEjecucion) {
            //Ejecutar una sola vez
            primeraEjecucion = false;

            //Obtener referencias
            movimiento = miPersonaje.GetComponent<Movimiento>();
            controlesPantalla = miPersonaje.GetComponent<ControlesPantalla>();
            aparienciaWalter = miPersonaje.GetComponent<AparienciaWalter>();

            //Establecer ajustes
            controlesPantalla.setModo(infoPersistente.getModoControl());
            controlesPantalla.setSensibilidad(infoPersistente.getSenApunt() / 2);
            movimiento.setSensibilidadMovi((infoPersistente.getSenMov() * 25) / 8);

            //Establecer Apariencia
            aparienciaWalter.establecerApariencia(infoPersistente.getAparienciaActiva());
        }
    }
}