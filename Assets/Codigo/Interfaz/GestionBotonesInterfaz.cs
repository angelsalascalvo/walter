using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestionBotonesInterfaz : MonoBehaviour{

    //Referencias Publicas
    [Header("AMIGOS")]
    public GameObject panelMisAmigos;
    public GameObject panelSolicitudes;
    public GameObject panelAgregarAmigos;

    [Header("GENERALES")]
    public GameObject panelGeneralAmigos;
    public GameObject panelGeneralAjustes;
    public GameObject panelGeneralApariencia;
    public GameObject panelMenu;
    public GameObject panelGenerico;
    public Animator animatorBotonBatalla;

    [Header("OTROS")]
    public GameObject ventanaEmergente;
    public SolicitarBatalla solicitarBatalla;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private bool mostrando = false, pausa=false;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update() {
        if (Application.platform == RuntimePlatform.Android) {
            if (Input.GetKey(KeyCode.Escape)) {
                if (pausa == false) {
                    pausa = true;
                    //Si el panel de menu esta desactivado al pulsar el boton de volver se intentará cerrar la aplicacion
                    if (panelMenu.gameObject.activeSelf == false) { 
                        bSalir();
                    } else {
                        //Volver
                        bAtras();
                    }
                    pausa = false;
                }
                return;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    ////////////////////////////// AMIGOS ///////////////////////////////////////////////

    /*
    * METODO PARA MOSTRAR EL PANEL DE MIS AMIGOS
    */
    public void bMisAmigos() {
        panelMisAmigos.gameObject.SetActive(true);
        panelSolicitudes.gameObject.SetActive(false);
        panelAgregarAmigos.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA MOSTRAR EL PANEL DE SOLUCITUDES
    */
    public void bSolicitudes() {
        panelMisAmigos.gameObject.SetActive(false);
        panelSolicitudes.gameObject.SetActive(true);
        panelAgregarAmigos.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR EL PANEL DE BUSCAR AMIGOS
     */
    public void bAgregarAmigos() {
        panelMisAmigos.gameObject.SetActive(false);
        panelSolicitudes.gameObject.SetActive(false);
        panelAgregarAmigos.gameObject.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------------------------

    ////////////////////////////////// GENERAL ////////////////////////////////////////////

    /*
    * METODO PARA MOSTRAR LOS BOTONES DE BATALLA U OCULTARLOS SEGUN EL ESTADO
    */
    public void botonBatalla() {
        //Si se pulsa y esta activado, lo desactivamos
        if (mostrando) {
            mostrando = false;
        } else {
            mostrando = true;
        }
        animatorBotonBatalla.SetBool("Mostrar", mostrando);
        //Reproducir sonido
        GetComponent<AudioSource>().Play();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA VOLVER A MOSTRAR LA PANTALLA PRINCIPAL
     */
    public void bAtras() {
        //Ocultar el panel generico que engloba todos los demás
        panelGenerico.gameObject.SetActive(false);
        panelMenu.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR LA OPCION DE AMIGOS DEL MENU
     */
    public void bMenuAmigos() {
        //Ocultar ajustes y mostrar apartado amigos del menu con la seccion de mis amigos
        panelGeneralApariencia.gameObject.SetActive(false);
        panelGeneralAjustes.gameObject.SetActive(false);
        panelGeneralAmigos.gameObject.SetActive(true);
        bMisAmigos();
        //Mostrar el panel generico que engloba todos los subapartados
        panelGenerico.gameObject.SetActive(true);
        //Reproducir sonido
        GetComponent<AudioSource>().Play();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR LA OPCION DE AJUSTES DEL MENU
     */
    public void bMenuAjustes() {
        //Ocultar amigos y mostrar ajustes
        panelGeneralAmigos.gameObject.SetActive(false);
        panelGeneralApariencia.gameObject.SetActive(false);
        panelGeneralAjustes.gameObject.SetActive(true);
        //Mostrar el panel generico que engloba todos los subapartados
        panelGenerico.gameObject.SetActive(true);
        //Reproducir sonido
        GetComponent<AudioSource>().Play();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR LA OPCION DE APARIENCIAS DEL MENU
     */
    public void bMenuApariencia() {
        //Ocultar amigos y mostrar ajustes
        panelGeneralAmigos.gameObject.SetActive(false);
        panelGeneralAjustes.gameObject.SetActive(false);
        panelGeneralApariencia.gameObject.SetActive(true);
        //Mostrar el panel generico que engloba todos los subapartados
        panelGenerico.gameObject.SetActive(true);
        //Reproducir sonido
        GetComponent<AudioSource>().Play();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA MOSTRAR EL MENU PRINCIPAL
    */
    public void bMenu() {
        //Ocultar amigos y mostrar ajustes
        panelMenu.gameObject.SetActive(true);
        //Si esta el menu de batalla desplegado lo ocultamos
        if(mostrando)
            botonBatalla();
        //Reproducir sonido
        GetComponent<AudioSource>().Play();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE SALIR DE LA APLICACION
     */
    public void bSalir() {
        string texto = "¿Quieres salir?";
        //Llamada al metodo que muestra la ventana de confirmación antes de salir
        VentanaConfirmacion ventana = ventanaEmergente.GetComponent<VentanaConfirmacion>().crearConfirmacion(texto, "Si", "No");
        //Lanzar evento en segundo plano (hilo aparte) que comprobará cuando se pulse una de las 2 opciones del cuadro de confirmacion
        StartCoroutine(ConfirmarSalir(ventana));
    }

    //========>

    /*
     * METODO QUE SE EJECUTA EN SEGUNDO PLANO PARA COMPROBAR SI SE HA PULSADO ALGUN BOTON DE LA VENTANA EMERGENTE
     */
    IEnumerator ConfirmarSalir(VentanaConfirmacion ventana) {
        //Esperar mientras no se seleccione ninguna opcion
        do {
            yield return null;
        } while (ventana.getAceptado() == 0);

        //Solo ejecutaremos la accion de borrado si se acepta la confirmación (aceptado = 1)
        if (ventana.getAceptado() == 1) {
            salir();
        }
    }

    //========>

    /*
     * METODO PARA CERRAR LA APLICACION QUE SE EJECUTARÁ AL CONFIRMAR LA ACCION
     */
    public void salir() {
        //Salir de la aplicacion
        Application.Quit();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE CERRAR SESION Y SALIR
     */
    public void bCerrarSesion() {
        string texto = "¿Quieres cerrar tu sesión y salir?";
        //Llamada al metodo que muestra la ventana de confirmación
        VentanaConfirmacion ventana = ventanaEmergente.GetComponent<VentanaConfirmacion>().crearConfirmacion(texto, "Si", "No");
        //Lanzar evento en segundo plano (hilo aparte) que comprobará cuando se pulse una de las 2 opciones del cuadro de confirmacion
        StartCoroutine(ConfirmarCerrarSesion(ventana));
    }

    //========>

    /*
     * METODO QUE SE EJECUTA EN SEGUNDO PLANO PARA COMPROBAR SI SE HA PULSADO ALGUN BOTON DE LA VENTANA EMERGENTE
     */
    IEnumerator ConfirmarCerrarSesion(VentanaConfirmacion ventana) {
        //Esperar mientras no se seleccione ninguna opcion
        do {
            yield return null;
        } while (ventana.getAceptado() == 0);

        //Solo ejecutaremos la accion de borrado si se acepta la confirmación (aceptado = 1)
        if (ventana.getAceptado() == 1) {
            cerrarSesion();
        }
    }

    //========>

    /*
     * METODO PARA CERRAR LA SESION DE LA APLICACION Y SALIR, SE EJECUTA AL CONFIRMAR LA ACCIÓN EN LA VENTANA EMERGENTE
     */
    public void cerrarSesion() {
        //Cerrar sesion
        refBD.cerrarSesion();

    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR EL PANEL DE AMIGOS AL PULSAR EL BOTON DE BATALLA CON UN AMIGO
     */
    public void bJugarAmigos() {
        //Mostrar menu
        bMenu();
        bMenuAmigos();
        //Mostrar ventana de mis amigos
        bMisAmigos();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA EMPEZAR UNA PARTIDA ALEATORIA AL PULSAR EL BOTON DE BATALLA ALEATORIA
    */
    public void bJugarAleatorio() {
        solicitarBatalla.partidaAleatoria();
    }
}