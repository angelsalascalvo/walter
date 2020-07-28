using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ControladorNuevoUsu : MonoBehaviour {

    //Referencias publicas
    public InputField ifNomUsu;
    public InputField ifCorreo;
    public InputField ifContrasena;
    public Text txtErrores;
    public ReferenciaBaseDatos refBD;
    public ControladorAutenticacion controladorAutenticacion;
    public Login login;
    public Image indicador;
    public Sprite imgCorrecto, imgError;

    //Variables privadas
    private List<String> listUsuarios = null; //Lista con los nombres de los usuarios existentes
    private bool nomDisponible;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Inicializar variables
        nomDisponible = true;

        //Llamada al metodo para comprobar la disponibilidad del nombre
        ifNomUsu.onEndEdit.AddListener(delegate { comprobarDisponibilidad(); });

        //Poner a la escucha el campo correspondiente para obtener todos los nombres de usuario ocupados de la base de datos
        refBD.getBaseDatos()
        .GetReference("jugadores")
        .ValueChanged += HandleValueChanged;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE PERMANECE A LA ESCUCHA SOBRE EL NODO DE JUGADORES PARA ALMACENAR EL USUARIO DE LOS NOMBRES UTILIZADOS
    */
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            return;
        }

        //Almacenar los datos leidos en el listado
        List<DataSnapshot> leidos = args.Snapshot.Children.ToList();
        listUsuarios = new List<string>();
        foreach (var linea in leidos) {
            listUsuarios.Add(linea.Value.ToString().ToLower());
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI EL NOMBRE DE USUARIO INTRODUCIDO ESTA DISPONIBLE
     */
    public void comprobarDisponibilidad() {
        //inicializar la variables
        nomDisponible = true;
        txtErrores.gameObject.SetActive(false);
        //Comprobar validez sin mostrar mensaje error
        validoDisponible(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI UN NOMBRE DE USUARIO ES VALIDO Y DISPONIBLE
     * Se indica si se desea mostrar mensajes de error o no
     */
    public bool validoDisponible(bool mostrarTxt) {
        //Iniciar variables
        nomDisponible = true;

        /////////////// VALIDEZ /////////////////
        //Comprobar si se ha introducido un nombre valido
        if (ifNomUsu.text.Length >= 5 && ifNomUsu.text.Length <= 12) {
            if (Regex.IsMatch(ifNomUsu.text, "^[a-zA-Z0-9]+[_]*[a-zA-Z0-9]*$")) {

                /////////////// DISPONIBILIDAD /////////////////
                //Comprobar disponibilidad del nombre
                if (ifNomUsu.text != "" && listUsuarios != null) {

                    //Recorrer listado de usuarios ocupados
                    foreach (var usuario in listUsuarios) {
                        //Si coincide no esta disponible
                        if (usuario.Equals(ifNomUsu.text.ToLower())) {
                            nomDisponible = false;
                        }
                    }
                }

                //Si el nombre esta disponible devolvemos verdadero
                if (nomDisponible) {
                    //Cambiar image
                    indicador.overrideSprite = imgCorrecto;
                    indicador.color = new Color32(32, 147, 16, 255);
                    indicador.gameObject.SetActive(true);
                    return true;
                } else {
                    //Mostrar error si es necesario
                    if (mostrarTxt) {
                        txtErrores.text = "Vaya! Este nombre ya esta utilizado";
                        txtErrores.gameObject.SetActive(true);
                    }
                }
            } else {
                //Mostrar error si es necesario
                if (mostrarTxt) {
                    txtErrores.text = "Un nombre demasiado dificil ¿No te parece?\nRecuerda solo numeros o letras";
                    txtErrores.gameObject.SetActive(true);
                }
            }
        } else {
            //Mostrar error si es necesario
            if (mostrarTxt) {
                txtErrores.text = "Tu nombre debe tener entre 5 y 12 caracteres";
                txtErrores.gameObject.SetActive(true);
            }
        }

        //Cambiar imagen si no se ha escrito nada
        if (ifNomUsu.text == "") {
            indicador.gameObject.SetActive(false);
            //Mostrar error si es necesario
            if (mostrarTxt) {
                txtErrores.text = "Indica tu nombre de usuario antes de continuar";
                txtErrores.gameObject.SetActive(true);
            }
        }//Si se ha escrito algo y estamos en este lugar no es valido
        else {
            indicador.overrideSprite = imgError;
            indicador.color = new Color32(190, 55, 6, 255);
            indicador.gameObject.SetActive(true);
        }

        return false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTARÁ AL PULSAR EL BOTON DE ACEPTAR
     */
    public void bAceptar() {
        //Comprobar validez mostrando mensaje error
        if (validoDisponible(true)) {
            //Comprobar si se han rellenado todos los campos
            if (!ifCorreo.text.Equals("") && !ifContrasena.text.Equals("")) {
                //Ocultar errores
                txtErrores.gameObject.SetActive(false);
                //Llamada al metodo asincrono que registra al usuario en el sistema, es necesario _= ya que es un metodo asincrono
                _ = registrarUsuarioAsync();

            } else {
                txtErrores.text = "Debes rellenar todos los campos";
                txtErrores.gameObject.SetActive(true);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO ASINCRONO PARA EFECTUAR EL REGISTRO DEL USUARIO EN FIREBASE AUTHENTICATION Y EN FIREBASE DATABASE
     * MEDIANTE "await" SE DETIENE LA EJECUCIÓN HASTA COMPLETARSE LA ESCRITURA
     */
    public async System.Threading.Tasks.Task registrarUsuarioAsync() {
        //Iniciar variables
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        bool usuarioCreado = true;
        string msmError = "Imposible crear el usuario con los datos introducidos";

        //Activar la pantalla de carga
        controladorAutenticacion.estadoPantCarga(true);

        //Metodo para crear el usuario en el sistema de autenticacion de firebase
        await auth.CreateUserWithEmailAndPasswordAsync(ifCorreo.text, ifContrasena.text).ContinueWith(task => {
            if (task.IsFaulted) {
                //Comprobar errores en el registro
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
                    string authErrorCode = "";
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null) {
                        authErrorCode = String.Format("AuthError.{0}: ",
                          ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                    }
                    string code = ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString();

                    //Comprobar el error producido
                    if (code.Equals("InvalidEmail")) {
                        msmError = "El correo introducido no es valido";

                    } else if (code.Equals("WeakPassword")) {
                        msmError = "Contraseña demasiado debil";

                    } else if (code.Equals("EmailAlreadyInUse")) {
                        msmError = "Ya existe un usuario con ese correo";
                    }
                }

                usuarioCreado = false;
                return;
            }
        });

        //Obtener la autenticacion del nuevo usuario creado
        auth = FirebaseAuth.DefaultInstance;

        if (usuarioCreado) {
            //Guardar en el listado de jugadores
            await refBD.getBaseDatos().GetReference("jugadores").Child(auth.CurrentUser.UserId).SetValueAsync(ifNomUsu.text);
            //Crear nodo con los datos del usuario
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("nombreUsu").SetValueAsync(ifNomUsu.text);
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("foto").SetValueAsync("http://drive.google.com/uc?export=view&id=1D_FIPVEEspgXz_DjCWGEgu2aBZWf8Wut");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("energia").SetValueAsync("0");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("batalla/peticion").SetValueAsync("null");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("batalla/respuesta").SetValueAsync("null");

            //Guardar los ajustes iniciales y establecerlos a las variables del objeto de informacion
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("ajustes").Child("senMov").SetValueAsync("5");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("ajustes").Child("senApunt").SetValueAsync("5");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("ajustes").Child("modoControl").SetValueAsync("a");

            //Guardar la apariencias disponibles inicialmente
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("apariencias").Child("normal").SetValueAsync("disponible");
            await refBD.getBaseDatos().GetReference("usuarios").Child(auth.CurrentUser.UserId).Child("aparienciaActiva").SetValueAsync("normal");

            //Cargar escena principal
            _ = login.iniciarSesionAsync(ifCorreo.text, ifContrasena.text);
        } else {
            controladorAutenticacion.estadoPantCarga(false);
            txtErrores.text = msmError;
            txtErrores.gameObject.SetActive(true);
        }
    }
}