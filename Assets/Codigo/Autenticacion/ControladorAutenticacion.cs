using Firebase.Auth;
using Firebase.Database;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorAutenticacion : MonoBehaviour
{
    //Variables privadas
    private bool escenaPrincipal; //Variable para determinar cuando pasar a la escena principal
    private bool obtenerinfo, primeraEjecucion, primeraEjecucion2; 

    //Referencia publica
    public GestionServidorPhoton gestionServidorPhoton;
    public Canvas pantallaCarga;
    public GameObject panelRegistro;
    public GameObject panelRecuperar;
    public GameObject panelLogin;
    public ReferenciaBaseDatos refBD;
    public InfoPersistente infoPersistente;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Iniciar variables
        escenaPrincipal = false;
        obtenerinfo = true;
        primeraEjecucion = true;
        primeraEjecucion2 = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void Update()
    {
        //Obtener autenticacion
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        
        //Comprobar si esta conectado al servidor photon y logueado para cargar la escena principal
        if (PhotonNetwork.IsConnected && auth!=null) {
            //Ejecutar solo la primera vez
            if (primeraEjecucion) {
                primeraEjecucion = false;
                refBD.setUsuario(auth.CurrentUser);
                cargarEscenaPrincipal();
            }
        }

        //Si se activa la variable de escena principal
        if (escenaPrincipal) {
            if (obtenerinfo && primeraEjecucion2) {
                primeraEjecucion2 = false;
                obtenerinfo = false;
                //Cargar informacion inicial
                infoPersistente.obtenerDatosContinuos();
                //Iniciar Informacion
                refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
                refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");
            }

            //No cambiar de escena mientras no se recupere toda la informacion inicial
            if (infoPersistente.isObtenidoTodo()) {
                //Cargar escena principal
                SceneManager.LoadScene("Principal");
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA CAMBIAR ESTADO DE LA PANTALLA DE CARGA (CANVAS)
     */
    public void estadoPantCarga(bool estado) {
        pantallaCarga.gameObject.SetActive(estado);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ACTIVAR LAS VARIABLES QUE CARGARAN LA ESCENA PRINCIPAL DESDE EL UPDATE
    */
    public void cargarEscenaPrincipal() {
        escenaPrincipal = true;
        obtenerinfo = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE MOSTRARA U OCULTARA LA SECCION DE REGISTRO
    */
    public void mostrarRegistro(bool estado) {
        panelRegistro.gameObject.SetActive(estado);
        panelLogin.gameObject.SetActive(!estado);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE MOSTRARA U OCULTARA LA SECCION DE RECUPERAR LA CONTRASEÑA
     */
    public void mostrarRecuperar(bool estado) {
        panelRecuperar.gameObject.SetActive(estado);
        panelLogin.gameObject.SetActive(!estado);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE MOSTRARA U OCULTARA LA SECCION DE LOGIN
     */
    public void mostrarLogin(bool estado) {
        panelLogin.gameObject.SetActive(estado);
        panelRegistro.gameObject.SetActive(!estado);
        panelRecuperar.gameObject.SetActive(!estado);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTARÁ AL PULSAR EL BOTON DE VOLVER PARA MOSTRAR LA SECCION DE LOGIN
     */
    public void volver() {
        panelRecuperar.gameObject.SetActive(false);
        panelRegistro.gameObject.SetActive(false);
        panelLogin.gameObject.SetActive(true);
    }
}