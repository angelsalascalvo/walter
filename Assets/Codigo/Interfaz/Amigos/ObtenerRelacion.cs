using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtenerRelacion : MonoBehaviour { 
    //*** Script compartido para obtener elementos del listado de amigo y el de peticiones

    //Ajustes publicos
    public string queUsuariosObtener;

    //Referencias Publicas
    public GameObject panelSinAmigos;
    public GameObject prefabElemento;
    public Transform contenedorLista;
    public GameObject ventanaEmergente;
    public SolicitarBatalla solicitarBatalla;

    //Variables privadas
    private ReferenciaBaseDatos refBD;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();

        //Ocultar el panel no tienes amigos
        panelSinAmigos.gameObject.SetActive(false);

        //Poner a la escucha el nodo para obtener amigos o solicitudes
        refBD.getBaseDatos()
        .GetReference("usuarios/" + refBD.getUsuario().UserId+"/relaciones")
        .ValueChanged += HandleValueChanged; 
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE PERMANECE CONSTANTEMENTE A LA ESCUCHA DE CAMBIOS SOBRE EL NODO DE RELACIONES DEL USUARIO
    */
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        //Lista donde almacenar los codigo de usuario de los amigos
        List<string> uidAmigos = new List<string>();

        //Limpiar lista de usuarios
        limpiarLista();

        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        IEnumerable<DataSnapshot> posLeida = args.Snapshot.Children;
        //Recorrer los resultado leidos de relaciones
        foreach (var linea in posLeida) {
            //Buscar las relaciones de amistad/peticiones y almacenar en la lista los uid
            if (linea.Value.ToString().ToLower().Equals(queUsuariosObtener.ToLower()))
                uidAmigos.Add(linea.Key);
        }
        
        //Comprobar si se ha obtenido algun amigo
        if (uidAmigos.Count == 0) {
            //Si no se ha obtenido ningun amigo, mostramos el panel que lo indica
            panelSinAmigos.gameObject.SetActive(true);

        //Si hemos obtenido algun amigo
        } else {
            //Mostrar el panel de amigos
            panelSinAmigos.gameObject.SetActive(false);
            //Recorrer la lista de uid y obtener la informacion del usuario correspondiente
            foreach (var leido in uidAmigos) {
                //Obtener los datos del usuario indicado
                _ = ObtenerUsuAsync(leido);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER LOS DATOS DEL USUARIO INDICADO POR PARAMETRO DE LA BASE DE DATOS
    */
    public async System.Threading.Tasks.Task ObtenerUsuAsync(string uid) {
        //Iniciar variables
        string nombreUsu="", foto="";

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

        //Crear un objeto de la clase amigo con los datos leidos
        Amigo amigoLeido = new Amigo { nombre = nombreUsu, urlFoto = foto, codigoUid = uid };
        //Crear el objeto en el contenedor de la lista
        GameObject elementoLista = Instantiate(prefabElemento, contenedorLista);

        //Llamada al metodo que le asigna los datos al elemento del listado, indicando que tipo de elemento es (solicitud, amigo...)
        //Es un amigo
        if(queUsuariosObtener.Equals("amigo"))
            elementoLista.GetComponent<ElementoLista>().crearMiAmigo(amigoLeido, ventanaEmergente, solicitarBatalla);
        //Es una solicitud
        else
            elementoLista.GetComponent<ElementoLista>().crear(amigoLeido, queUsuariosObtener, ventanaEmergente);
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
}