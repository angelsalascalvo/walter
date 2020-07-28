using UnityEngine;
using UnityEngine.UI;

public class Batalla : MonoBehaviour
{
    //Ajustes publicos
    public float tiempoMaxCarga = 14;

    //Referencias publicas
    public GameObject panelPrincipal; //Panel que engloba todo el mensaje de la solicitud de una batalla
    public Text mensaje;
    public GestionPartidas gestionPartidas;
    public GameObject ventanaEmergente;
    public DetectorBatallas detectorBatallas;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private InfoPersistente infoPersistente;
    private string uidOponente;
    private bool empezarPartida, primeraEjecucion;
    private float tiempoAux;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();

        //Iniciar variables
        empezarPartida = false;
        primeraEjecucion = true;
        tiempoAux = tiempoMaxCarga;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    public void Update() {
        if (empezarPartida) {
            //Ejecutar solo la primera vez
            if (primeraEjecucion) {
                primeraEjecucion = false;
                //Llamada al metodo para acceder a la salas de un amigo
                gestionPartidas.accederSalaAmigo(uidOponente);
                //Mostrar pantalla de carga
                ventanaEmergente.GetComponent<VentanaEspera>().mostrar("Cargando batalla...");
            }

            //Reducir el tiempo
            tiempoAux -= Time.deltaTime;

            if (tiempoAux <= 0) {
                //Ocultar pantalla de carga y 
                ventanaEmergente.GetComponent<VentanaEspera>().ocultar();
                //Reiniciar variables a estado final
                empezarPartida = false;
                primeraEjecucion = true;
                tiempoAux = tiempoMaxCarga;

                //Reiniciar la lectura de solicitudes de batallas
                panelPrincipal.gameObject.SetActive(false);
                detectorBatallas.reiniciarLecturaBatallas();

                //Mostrar mensaje info
                string texto = "Imposible comenzar la batalla, intentelo mas tarde";
                ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "Aceptar");
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA MOSTRAR EL MENSAJE DE SOLICITUD DE UNA BATALLA
    */
    public void crear(string nombre, string uid) {
        uidOponente = uid;
        mensaje.text = "Solicitud de batalla de " + nombre;
        panelPrincipal.gameObject.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OVULTAR EL MENSAJE DE SOLICITUD DE UNA BATALLA
     */
    public void ocultar() {
        panelPrincipal.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA CUANDO SE PULSA EL BOTON DE ACEPTAR LA BATALLA
    */
    public void bAceptar() {
        //Ocultar panel
        panelPrincipal.gameObject.SetActive(false);
        //Indicar aceptar solicitud de batalla al contrincante
        refBD.getBaseDatos().GetReference("usuarios").Child(uidOponente).Child("batalla/respuesta").SetValueAsync("true");
        //Indicar que esta en batalla para no recibir mas solicitudes
        infoPersistente.setEnBatalla(true);

        //Marcar la variable para empezar la partida desde el metodo update
        empezarPartida = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    public void bCancelar() {
        //Indicar rechazo de solicitud de batalla
        refBD.getBaseDatos().GetReference("usuarios").Child(uidOponente).Child("batalla/respuesta").SetValueAsync("false");
        //Reiniciar peticion
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
        panelPrincipal.gameObject.SetActive(false);
        //Reiniciar la lectura de solicitudes de batallas
        detectorBatallas.reiniciarLecturaBatallas();
    }    
}