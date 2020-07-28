using Firebase.Database;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    //Referencias publicas
    public GameObject panelLogin;
    public GameObject panelDirecto;
    public ReferenciaBaseDatos refBD;
    public GameObject panelVersion;
    public GestionServidorPhoton gestionServidorPhoton;
    public ControladorAutenticacion controladorAutenticacion;

    [Header("LOGIN")]
    public InputField ifCorreo;
    public InputField ifContrasena;
    public Text errorLogin;

    [Header("RECUPERAR")]
    public InputField ifCorreoRecuperar;
    public GameObject ventanaEmergente;
    
    //Variables privadas
    private Firebase.Auth.FirebaseAuth auth;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    public void Start() {
        //Inicio de variables
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        errorLogin.gameObject.SetActive(false);

        //Comprobar version
        _ = detectar();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA COMPROBAR SI LA VERSION QUE SE ESTA EJECUTANDO ES LA ULTIMA DISPONIBLE
    */
    public async System.Threading.Tasks.Task detectar() {
        string version = "";

        //Leer el nodo de relaciones del usuario
        await refBD.getBaseDatos().GetReference("version")
        .GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {

            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                version = snapshot.Value.ToString();
            }
        });

        //Si la version de la aplicacion no se corresponde con la ultima, mostramos el mensaje de desconexion para evitar su uso
        if (!version.Equals("3"))
            panelVersion.gameObject.SetActive(true);
        //Si la version es la ultima
        else {
            //Comprobar si se esta logueado
            comprobarLogueo();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI UN USUARIO ESTA YA LOGUEADO DE UNA SESION PREVIA
     */
    public void comprobarLogueo() {
        //Comprobar si se ha obtenido un usuario (si esta logueado)
        if (auth.CurrentUser != null) {
            //Mostrar panel directo
            panelDirecto.gameObject.SetActive(true);
            panelLogin.gameObject.SetActive(false);

        //En caso contrario mostrar el panel de login
        } else {
            panelLogin.gameObject.SetActive(true);
            panelDirecto.gameObject.SetActive(false);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA INICIAR SESION CON LOS DATOS INTRODUCIDOS
    */
    public void iniciarSesion() {
        //Comprobar que se han introducido datos
        if (!ifCorreo.text.Equals("") && !ifContrasena.text.Equals("")) {
            //Llamada al metodo para iniciar sesion
            _ = iniciarSesionAsync(ifCorreo.text, ifContrasena.text);

        //Si no se han introducido mostramos un mensaje de error
        } else {
            errorLogin.text = "Debes introducir un correo y contraseña";
            errorLogin.gameObject.SetActive(true);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INICIAR SESION CON LA INFORMACION PASADA POR LOS PARAMETROS
     */
    public async System.Threading.Tasks.Task iniciarSesionAsync(string usu, string pwd)
    {
        //Mostrar la pantalla de carga
        controladorAutenticacion.estadoPantCarga(true);
        errorLogin.gameObject.SetActive(false);
        bool error = false;

        //Ejecutar el metodo propio de firebase para el inicio de sesión
        await auth.SignInWithEmailAndPasswordAsync(usu, pwd).ContinueWith(task => {
            if (task.IsCanceled) {
                return;
            }
            if (task.IsFaulted) {
                error = true;           
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });

        //Si se ha producido un error en los datos, los mostramos
        if (error) {
            errorLogin.text = "Usuario o contraseña incorrectos";
            errorLogin.gameObject.SetActive(true);
            //Ocultar la pantalla de carga
            controladorAutenticacion.estadoPantCarga(false);
        }
    }

    //------------------------------------------------------------------------------------------------------------------


    /*
     * METODO PARA EJECUTAR EL METODO QUE ENVIA LA INFORMACION AL CORREO PARA RECUPERAR LA INFORMACION
     */
    public void recuperarContrasena() {
        //Comprobar que se han introducido datos
        if (!ifCorreoRecuperar.text.Equals("")) {
            _ = recuperarContrasenaAsync();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ENVIAR LA INFORMACION DE RECUPERACION DE CONTRASEÑA AL CORREO INDICADO
    */
    public async System.Threading.Tasks.Task recuperarContrasenaAsync() {
        string correo = ifCorreoRecuperar.text;
        bool error = false;
        //Mostrar pantalla de carga 
        controladorAutenticacion.estadoPantCarga(true);

        //Ejecutar el metodo propio de firebase authentication para enviar instrucciones de recuperacion
        await auth.SendPasswordResetEmailAsync(ifCorreoRecuperar.text).ContinueWith(task => {
            if (task.IsCanceled) {
                error = true;
                return;
            }
            if (task.IsFaulted) {
                error = true;
                return;
            }
        });

        //Borrar contenido del campo de texto
        ifCorreoRecuperar.text = "";
        //Ocultar pantalla de carga
        controladorAutenticacion.estadoPantCarga(false);
        //Volver a la pantalla de login
        controladorAutenticacion.volver();

        //Si se ha producido un error en los datos lo indicaremos
        if (error) {
            ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo("Imposible enviar indicaciones al email " + correo, "Aceptar");
        } else {
            ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo("Se han enviado las indicaciones para restablecer tu contraseña al email " + correo, "Aceptar");
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void Update() {
        //En el momento en el que se loguea, se conecta con el servidor photon
        if (auth.CurrentUser != null) {
            if (PhotonNetwork.IsConnected == false) {
                //Conectar con el servidor de photon
                gestionServidorPhoton.conectarConServidor();
            }
        }
    }
}