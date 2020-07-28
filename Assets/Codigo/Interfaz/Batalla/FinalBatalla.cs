using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalBatalla : MonoBehaviour
{
    //Referencias publicas
    public Text txtTitulo;
    public Text txtEnergiaGanada;
    public GameObject panelBonus;
    public GameObject panelContenido;
    public Canvas canvasPantalla;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private InfoPersistente infoPersistente;
    private GameObject miPersonaje;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR EL PANEL FINAL DE LA PARTIDA
     */
    public async System.Threading.Tasks.Task mostrarAsync(string estado, int energiaGanada) {
        
        //Desconectarnos de la sala de juego creada
        if(PhotonNetwork.CurrentRoom!=null)
            PhotonNetwork.LeaveRoom();

        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();

        //Obtener mi personaje si no lo he obtenido previamente
        if (miPersonaje == null)
            miPersonaje = GameObject.Find("miPersonaje");

        //Bloquear el personaje para no poder interactuar mas con el
        if(miPersonaje != null)
            miPersonaje.GetComponent<EstadoJugador>().setEstado(0);

        //Establecer la energia y titulo en funcion de si hemos ganado, perdido, empatado o se ha desconectado el oponente
        int energia=0;
        switch (estado) {
            case "ganado":
                energia = energiaGanada + 30; //Sumar el bonus
                panelBonus.gameObject.SetActive(true);
                txtTitulo.text = "Has ganado";
                break;
            case "perdido":
                energia = energiaGanada;
                panelBonus.gameObject.SetActive(false);
                txtTitulo.text = "Has perdido";
                break;
            case "empatado":
                energia = energiaGanada;
                panelBonus.gameObject.SetActive(false);
                txtTitulo.text = "Empate";
                break;
            case "desconectado":
                panelBonus.gameObject.SetActive(true);
                energia = energiaGanada + 30; //Sumar el bonus
                txtTitulo.text = "El oponente ha abandonado la partida";
                break;
        }
        
        //Mostrar la energia obtenida
        txtEnergiaGanada.text = ("+" + energiaGanada);
        //Sumar energia al nodo de firebase
        await refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("energia").SetValueAsync(""+(infoPersistente.getEnergia()+energia));
        //Establecer los datos a null 
        await refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
        await refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");

        //Cuando sumamos la energia a la base de datos mostramos el propio panel informativo
        panelContenido.gameObject.SetActive(true);
        canvasPantalla.gameObject.SetActive(false);

    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE ACEPTAR DE LA VENTANA
     */
    public void botonAceptar() {
        //Si no  hemos salido de la sala creada para la partida lo hacemos
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
        //Llamada al metodo que sale de la partida
        StartCoroutine(volverPrincipal());
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA EN UN HILO DIFERENTE PARA SALIR EN EL MOMENTO EN EL QUE NO ESTAMOS EN LA SALA Y EVITAR PROBLEMAS
    */
    IEnumerator volverPrincipal() {
        //Esperar mientras estamos en la habitacion
        do {
            yield return null;
        } while (PhotonNetwork.InRoom);

        //Cuando no estemos en una habitacion volvemos a la pantalla principal
        SceneManager.LoadScene("Principal");
    }
}