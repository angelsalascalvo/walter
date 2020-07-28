using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgregarAmigos : MonoBehaviour {
     
    //Referencias Publicas
    public InputField ifUsuarioBuscar;
    public GameObject panelSinResultados;
    public GameObject panelInicio;
    public GameObject prefabElemento;
    public Transform contenedorLista;
    public GameObject ventanaEmergente;
    public SolicitarBatalla solicitarBatalla;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private List<string> uidAmigos = new List<string>();
    private List<string> uidPeticiones = new List<string>();
    private List<string> uidSolicitudes = new List<string>();

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();

        //Añadir listener que llamara un metodo cuando los valores del campo de texto cambien
        ifUsuarioBuscar.onEndEdit.AddListener(delegate { buscarUsuarioAux(); });

        //Inicializar componentes
        panelInicio.gameObject.SetActive(true);
        panelSinResultados.gameObject.SetActive(false);

        //Poner a la escucha el nodo para obtener amigos
        refBD.getBaseDatos()
        .GetReference("usuarios/" + refBD.getUsuario().UserId + "/relaciones")
        .ValueChanged += HandleValueChanged;
    }

    //------------------------------------------------------------------------------------------------------------------

    
    /*
     * SOBRESCRITURA DEL METODO ONENABLE
     * Se ejecuta cuando se activa el objeto que tiene el script
     */
    void OnEnable() {
        //Al activar de nuevo el objeto limpiamos la lista y borramos contenido input field
        ifUsuarioBuscar.text = "";
        limpiarLista();
        panelInicio.gameObject.SetActive(true);
    }

    //-----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO AUXILIAR PARA LLAMAR A UN METODO ASINCRONO CON ESPERA
     */
    public void buscarUsuarioAux() {
        limpiarLista(); //Eliminar todo el contenido de la lista
        _ = buscarUsuarioAsync(); //Llamada al metodo para realizar la busqueda
    }

    //-----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA BUSCAR UN USUARIO POR EL NOMBRE
     */
    public async System.Threading.Tasks.Task buscarUsuarioAsync() {
        List<string> listEncontrados = new List<string>();

        //Comprobar que se ha introducido algun texto de busqueda 
        if (ifUsuarioBuscar.text.Equals("")) {
            panelInicio.gameObject.SetActive(true);
            panelSinResultados.gameObject.SetActive(false);
            return;
        }

        //Obtener datos de firebase
        await refBD.getBaseDatos().GetReference("jugadores")
        .GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {

            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                IEnumerable<DataSnapshot> posLeida = snapshot.Children;

                //Recorrer los resultado leidos sobre todos los jugadores
                foreach (var linea in posLeida) {

                    //Buscar jugadores que en su nombre contienen el texto introducido
                    if (linea.Value.ToString().ToLower().Contains(ifUsuarioBuscar.text.ToLower())) {
                        if (linea.Value.ToString().ToLower().Equals(ifUsuarioBuscar.text.ToLower())) {
                            //Si se encuentra uno en el que coinciden todas las letras, lo situamos el primero de la lista
                            listEncontrados.Insert(0, linea.Key.ToString());
                        } else {
                            //Si solo contiene la palabra, lo incluimos como otro mas
                            listEncontrados.Add(linea.Key.ToString());
                        }
                    }
                }
            }
        });

        //Tras realizar busqueda comprobar el numero de resultados
        if (listEncontrados.Count > 0) {
            //Si es mayor que 0, llamamos al metodo para obtener los datos de los usuario 
            _ = ObtenerDatosPorUIDAsync(listEncontrados);
            panelSinResultados.gameObject.SetActive(false);
            panelInicio.gameObject.SetActive(false);

        } else {
            //En otro caso, indicamos que no se han encontrado resultados de la busqueda, mostramos el panel adecuado
            panelSinResultados.gameObject.SetActive(true);
            panelInicio.gameObject.SetActive(false);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LOS DATOS DEL LISTADO DE UID DE USUARIOS PASADO POR PARAMETRO, AGREGANDOLOS A LA LISTVIEW
     */
    public async System.Threading.Tasks.Task ObtenerDatosPorUIDAsync(List<string> listEncontrados) {
        string nombreUsu = "", foto = "";

        //Recorrer lista de UID correspondientes con usuarios encontrados
        foreach (var uid in listEncontrados) {
            bool yaEsAmigo = false, yaEstaPedido = false, esUnaSolicitud = false;

            //Comprobar si el usuario encontrado ya es amigo nuestro
            foreach (var uidAmigo in uidAmigos) {
                if (uidAmigo.Equals(uid))
                    yaEsAmigo = true;
            }
            //Comprobar si ya le hemos mandado una solicitud al usuario encontrado
            foreach (var uidAmigo in uidPeticiones) {
                if (uidAmigo.Equals(uid))
                    yaEstaPedido = true;
            }
            //Comprobar si nos ha mandado una solicitud
            foreach (var uidAmigo in uidSolicitudes) {
                if (uidAmigo.Equals(uid))
                    esUnaSolicitud = true;
            }

            //Leer el nodo de relaciones del usuario
            await refBD.getBaseDatos().GetReference("usuarios/" + uid)
            .GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted) {

                } else if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;

                    IEnumerable<DataSnapshot> posLeida = snapshot.Children;

                    //Recorrer los resultado leidos del usuario
                    foreach (var linea in posLeida) {
                        //En funcion del dato leido lo alamacenaremos en la variable adecuada
                        if (linea.Key.ToString().Equals("nombreUsu"))
                            nombreUsu = linea.Value.ToString();
                        if (linea.Key.ToString().Equals("foto"))
                            foto = linea.Value.ToString();
                    }
                }
            });

            //Añadir encontrado
            Amigo usuario = new Amigo { nombre = nombreUsu, urlFoto = foto, codigoUid = uid };

            //Comprobar que no somos nosotros mismos
            if (uid != refBD.getUsuario().UserId) {
                //Crear el objeto en el contenedor de la lista
                GameObject elementoLista = Instantiate(prefabElemento, contenedorLista);

                //Si ya somos amigos lo indicaremos
                if (yaEsAmigo) {
                    //Añadir elemento con los botones de un amigo activados
                    elementoLista.GetComponent<ElementoLista>().crearMiAmigo(usuario, ventanaEmergente, solicitarBatalla);
                } else if (yaEstaPedido) {
                    //Llamada al metodo que le asigna los datos al elemento del listado
                    elementoLista.GetComponent<ElementoLista>().crear(usuario, "peticion", ventanaEmergente);
                } else if (esUnaSolicitud) {
                    //Llamada al metodo que le asigna los datos al elemento del listado
                    elementoLista.GetComponent<ElementoLista>().crear(usuario, "solicitud", ventanaEmergente);
                } else {
                    //Llamada al metodo que le asigna los datos al elemento del listado
                    elementoLista.GetComponent<ElementoLista>().crear(usuario, "agregar", ventanaEmergente);
                }
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ELIMINAR TODOS LOS COMPONENTES DE LA LISTA
     */
    public void limpiarLista() {
        //Obtener todos los hijos de la lista
        for (int i = 0; i < contenedorLista.transform.childCount; i++) {
            //Obtener cada elemento
            GameObject elemento = contenedorLista.transform.GetChild(i).gameObject;
            //Destruir el elemento
            Destroy(elemento);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA MANTENER EN ESCUCHA DE CAMBIOS EL NODO DE RELACIONES DEL USUARIO
    */
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {

        //Limpiar lista de usuarios amigos
        uidAmigos = new List<string>();
        //Limpiar lista de usuarios a los que le hemos mandado un peticion de amistad
        uidPeticiones = new List<string>();
        //Limpiar lista de usuarios que nos han mandado una solicitud de amistad
        uidSolicitudes = new List<string>();

        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        IEnumerable<DataSnapshot> posLeida = args.Snapshot.Children;
        //Recorrer los resultado leidos de relaciones
        foreach (var linea in posLeida) {
            //Buscar las relaciones de amistad/peticiones y almacenar en la lista los uid
            if (linea.Value.ToString().ToLower().Equals("amigo"))
                uidAmigos.Add(linea.Key);
            else if (linea.Value.ToString().ToLower().Equals("peticion"))
                uidPeticiones.Add(linea.Key);
            else if (linea.Value.ToString().ToLower().Equals("solicitud"))
                uidSolicitudes.Add(linea.Key);
        }

        //En el momento en el que cambia algun nodo de relaciones, refrescamos el listado
        buscarUsuarioAux();
    }
}