using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestionApariencias : MonoBehaviour {

    //Referencias publicas
    public GameObject canvasEmergente;
    public Animator animatorImagen, animatorImagen2;
    
    //Variables privadas
    private InfoPersistente infoPersistente;
    private GameObject[] apariencias;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        apariencias = GameObject.FindGameObjectsWithTag("apariencia");
        //Llamada al metodo para activar la apariencia activa
        aparienciaActiva();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void Update() {
        comprobarApariencias();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR LAS APARIENCIAS DISPONIBLES
     */
    public void comprobarApariencias() {
        //Al inicio bloquear todas las apariencias
        for (int i = 0; i < apariencias.Length; i++) {
            apariencias[i].GetComponent<Apariencia>().bloquearApariencia();
        }

        //Recorrer todas las apariencias disponibles
        foreach (var leido in infoPersistente.getAparienciasDisponibles()) {
            for (int i = 0; i < apariencias.Length; i++) {
                //Si la apariencia esta disponible, la desbloqueamos
                if (apariencias[i].GetComponent<Apariencia>().nombreAspecto.Equals(leido)) {
                    apariencias[i].GetComponent<Apariencia>().desbloquearApariencia();
                } 
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ACTIVAR LA APARIENCIA EN USO
     */
    public void aparienciaActiva() {
        //Marcar la apariencia activa
        for (int i = 0; i < apariencias.Length; i++) {
            //Si la apariencia activa corresponde con la recorrida, la marcamos.
            if (infoPersistente.getAparienciaActiva().Equals(apariencias[i].GetComponent<Apariencia>().nombreAspecto)) {
                apariencias[i].GetComponent<Apariencia>().marcar();
                //Ejecutar animacion
                animatorImagen.SetTrigger(apariencias[i].GetComponent<Apariencia>().getNombre());
                animatorImagen2.SetTrigger(apariencias[i].GetComponent<Apariencia>().getNombre());
                //En caso contrario la desmarcamos
            } else
                apariencias[i].GetComponent<Apariencia>().desmarcar();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER LA REFERENCIA AL CANVAS EMERGENTE DE LA ESCENA PRINCIPAL
    */
    public GameObject getCanvasEmergente() {
        return canvasEmergente;
    }
}