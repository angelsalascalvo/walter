using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestionPlantas : MonoBehaviour
{
    //Referencias publicas
    public GameObject prefabPlanta;
    public GestionInterfaz gestorInterfaz;
    public PhotonView pView;

    //Variables privadas
    private GameObject []generadores;
    private GameObject planta;
    public bool estoyCercaPlanta, plantaOcupada;

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start(){
        //Crear la planta desactivada
        planta = Instantiate(prefabPlanta, new Vector3(0, 0, 0), Quaternion.identity);
        planta.SetActive(false);
        planta.name = "Plantaa";
        //Obtener Generadores de plantas
        generadores = GameObject.FindGameObjectsWithTag("generadorPlanta");
        estoyCercaPlanta = false;
        plantaOcupada = false;
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ACTIVAR UNA DE LAS DIFERENTES PLANTAS DE FORMA ALEATORIA
     */
    public void activacion() {
        //Obtener una de forma aleatoria
        int num = Random.Range(0, generadores.Length);
        //Obtener el generador donde crear la planta
        GameObject generadorActivo = generadores[num].gameObject;
        //Obtener la posicion donde se genera la planta
        Vector3 posicionPlanta = generadorActivo.gameObject.transform.GetChild(0).gameObject.transform.position;
        //Activar generador 
        pView.RPC("activarGenerador_RPC", Photon.Pun.RpcTarget.AllBuffered, generadorActivo.name);
        //Activar la planta en la posicion del generador seleccionado
        pView.RPC("mostrarPlanta_RPC", Photon.Pun.RpcTarget.AllBuffered, posicionPlanta);
    }

    [Photon.Pun.PunRPC]
    public void activarGenerador_RPC(string nombreGenerador) {
        //Establecer la nueva posicion de la planta
        GameObject generadorActivo = GameObject.Find(nombreGenerador);
        //Activar generador
        generadorActivo.GetComponent<Animator>().SetTrigger("Activar");
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA SOLTAR LA PLANTA DESDE EL PERSONAJE
     */
    public void soltarPlanta(Vector3 posicion) {
        //Llamada al RPC que cambia de posicion la planta en todos los clientes
        pView.RPC("mostrarPlanta_RPC", Photon.Pun.RpcTarget.AllBuffered, posicion);
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR PLANTA EN LA POSICION ADECUADA
     */
    [Photon.Pun.PunRPC]
    public void mostrarPlanta_RPC(Vector3 posicion) {

        //Establecer la nueva posicion de la planta
        planta.transform.position = posicion;
        //Activar planta
        planta.SetActive(true);
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA SOLTAR LA PLANTA DESDE EL PERSONAJE
     */
    public void cogerPlanta() {
        //Llamada al RPC que cambia de posicion la planta en todos los clientes
        pView.RPC("ocultarPlanta_RPC", Photon.Pun.RpcTarget.AllBuffered);
    }

    //---------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OCULTAR PLANTA EN TODOS LOS CLIENTES
    */
    [Photon.Pun.PunRPC]
    public void ocultarPlanta_RPC() {
        //Desactivar planta
        planta.SetActive(false);
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MARCAR UNA PLANTA COMO OCUPADA (TIENE UN PERSONAJE CERCA)
     */
    public void ocuparPlanta(bool estado) {
        estoyCercaPlanta = estado;
        //Llamada al RPC para que se ejecute en todos los clientes meno en el que lo lanza
        pView.RPC("ocuparPlanta_RPC", Photon.Pun.RpcTarget.OthersBuffered, estado);
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA MARCAR UNA PLANTA COMO OCUPADA EN TODOS LOS CLIENTES
     */
    [Photon.Pun.PunRPC]
    public void ocuparPlanta_RPC(bool estado) {
        plantaOcupada = estado;
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA SABER SI LA PLANTA SE ENCUENTRA OCUPADA
     */
    public bool getPlantaOcupada() {
        return plantaOcupada;
    }

    //---------------------------------------------------------------------------------------------------

    /*
     * METODO PARA SABER SI ES MI PERSONAJE EL QUE ESTA OCUPANDO LA PLANTA
     */
    public bool getEstoyCercaPlanta() {
        return estoyCercaPlanta;
    }
}