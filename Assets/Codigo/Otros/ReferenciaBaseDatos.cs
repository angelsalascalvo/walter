using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;

public class ReferenciaBaseDatos : MonoBehaviour {

    //Referencias publicas
    public GameObject infoPersistente;
    private FirebaseDatabase baseDatos;
    private FirebaseUser usuario;

    //Variables privadadas
    private bool destruir;

    //------------------------------------------------------------------------------------------------------------------
    
    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN 
     */
    void Awake() {
        //Hacer un objeto persistente entre las diferentes escenas (evitar destruccion)
        DontDestroyOnLoad(gameObject);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        // Establecer la conexion con la base de datos
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://walter-rutillastoby.firebaseio.com/");
        // Obtener objeto que hace referencia a la base de datos
        baseDatos = FirebaseDatabase.DefaultInstance;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA REFERENCIA A LA BASE DE DATOS
     */
    public FirebaseDatabase getBaseDatos() {
        return baseDatos;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER EL USUARIO  LOGUEADO
    */
    public FirebaseUser getUsuario() {
        //Obtener usuario logueado
        return usuario;
    }

    //------------------------------------------------------------------------------------------------------------------
    
    /*
    * METODO PARA ESTABLECER EL USUARIO LOGUEADO
    */
    public void setUsuario(FirebaseUser usuario) {
        this.usuario = usuario;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA CERRAR LA SESION DEL USUARIO Y CERRAR LA APLICACION
     */
    public void cerrarSesion() {
        FirebaseAuth.DefaultInstance.SignOut();
        Application.Quit();
    }
}