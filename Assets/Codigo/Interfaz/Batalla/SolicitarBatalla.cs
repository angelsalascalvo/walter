using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolicitarBatalla : MonoBehaviour
{
    //Ajustes publicos
    public float tiempoEspera = 20;

    //Referencias publicas
    public GameObject ventanaEmergente;
    public GestionPartidas gestionPartidas;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private InfoPersistente infoPersistente;
    private bool solicitado, buscando;
    private float tiempoAux;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start()
    {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        
        //Inicializar variables
        solicitado = false;
        tiempoAux = tiempoEspera;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update()
    {
        /////////////////////////////////////////////// RECIBIR RESPUESTAS A PETICIONES /////////////////////////////////////////////////

        if (solicitado) {
            if (tiempoAux > 0) {
                tiempoAux -= Time.deltaTime;
            } else {
                //Si ha finalizado el tiempo de vida de la solicitud sin respuesta
                //Ocultar ventana de espera
                ventanaEmergente.GetComponent<VentanaEspera>().ocultar();
                //Marcar como fin de solicitud
                solicitado = false;
                infoPersistente.setEnBatalla(false);
                
                //reiniciar tiempo
                tiempoAux = tiempoEspera;
                //Mostrar mensaje info
                string texto = "Tiempo de espera agotado";
                ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "Aceptar");

            }

        } else if (buscando) {
            if (tiempoAux > 0) {
                tiempoAux -= Time.deltaTime;
            } else {
                //Si ha finalizado el tiempo de vida de la busqueda sin respuesta
                //Ocultar ventana de espera
                ventanaEmergente.GetComponent<VentanaEspera>().ocultar();
                //Marcar como fin de solicitud
                buscando = false;
                infoPersistente.setEnBatalla(false);
                //Salir de la sala creada
                gestionPartidas.salirSala();
                //reiniciar tiempo
                tiempoAux = tiempoEspera;
                //Mostrar mensaje info
                string texto = "No hay ningun jugador en este momento, intentelo mas tarde";
                ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "Aceptar");

            }
        }


        if (infoPersistente.getRespuestaBatalla().Equals("true")) {
            //Crear Sala de juego con el nombre de mi uid
            gestionPartidas.crearSalaAmigo(refBD.getUsuario().UserId);
            //Reinicializar la respuesta
            refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");
            //Establecer a nulo la respuesta para evitar que se ejecute mas de una vez
            infoPersistente.setRespuestaBatalla("null");

        } else if (infoPersistente.getRespuestaBatalla().Equals("false")) {
            //Reinicializar la respuesta
            refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");
            ventanaEmergente.GetComponent<VentanaEspera>().ocultar();
            //Mostrar mensaje info
            string texto = "El oponente ha rechazado la batalla";
            ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "Aceptar");
            //Marcar como fin de solicitud
            solicitado = false;
            infoPersistente.setEnBatalla(false);
            infoPersistente.setRespuestaBatalla("null");

        }
    }

    //------------------------------------------------------------------------------------------------------------------

    public void solicitar(string uid) {
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("batalla/peticion").SetValueAsync(refBD.getUsuario().UserId);
        tiempoAux = tiempoEspera;
        //Mostrar ventana de espera
        ventanaEmergente.GetComponent<VentanaEspera>().mostrar("Esperando respuesta del jugador");
        //Indicar que estamos en solicitado batalla para no permitir solicitudes
        infoPersistente.setEnBatalla(true);
        //Marcar como solicitado
        solicitado = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    public void partidaAleatoria() {
        tiempoAux = tiempoEspera;
        //Mostrar ventana de espera
        ventanaEmergente.GetComponent<VentanaEspera>().mostrar("Buscando partida aleatoria...");
        //Indicar que estamos en solicitado batalla para no permitir solicitudes
        infoPersistente.setEnBatalla(true);
        //Marcar como buscando
        buscando = true;
        //Guardar nombre oponente
        infoPersistente.setNombreOponente("Aleatorio");
        //Acceder a la sala aleatoria
        gestionPartidas.accederSalaAleatoria();
    }
}