using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorBatallas : MonoBehaviour
{
    //Ajustes publicos
    public float tiempoVisualizacion = 20;

    //Referencias Publicas
    public GameObject ventanaBatalla;
    public GameObject panelEspera;

    //Variables privadas
    private ReferenciaBaseDatos refBD;
    private InfoPersistente infoPersistente;
    private float tiempoAux;
    private bool contarTiempo=false, crearMensaje=true;

    //------------------------------------------------------------------------------------------------------------------
    
    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void Start(){
        infoPersistente.setJugando(false);
        //Iniciar Variables
        reiniciarLecturaBatallas();
    }

    //--------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    void FixedUpdate(){
        ///////////////////////////////////////////   ESCUCHAR PETICIONES  ////////////////////////////////////////////////////////
        //Actuamos si la batalla es diferente de null
        if (!infoPersistente.getPeticionBatalla().Equals("null")) {

            if (contarTiempo) {
                //Si no se ha cumplido el tiempo de mostrar el mensaje seguimos contabilizando el tiempo
                if (tiempoAux >= 0)
                    tiempoAux -= Time.deltaTime;

                //Al cumplirse el tiempo reiniciamos los valores
                else {
                    reiniciarLecturaBatallas();
                }

            } else {
                //Comprobar que no estemos en una batalla
                if (!infoPersistente.isEnBatalla()) {
                    //Ejecutar solo la primera vez al ser una tarea asIncrona
                    if (crearMensaje) {
                        crearMensaje = false;
                        //Llamada al metodo que mostrara el mensaje de solicitud de una nueva batalla
                        _ = crearBatalla();
                    }
                }
            }
            
        } else {
            ventanaBatalla.GetComponent<Batalla>().ocultar();
            contarTiempo = false;
        }
    }

    //--------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR MENSAJE DE SOLICITUD DE BATALLA
     */
    public async System.Threading.Tasks.Task crearBatalla() {
        string nombre="";

        await refBD.getBaseDatos().GetReference("jugadores")
        .GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
            } else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                IEnumerable<DataSnapshot> posLeida = snapshot.Children;

                //Recorrer los resultado leidos del usuario
                foreach (var linea in posLeida) {
                    //Buscar el nombre del jugador con el id de batalla
                    if (linea.Key.ToString().Equals(infoPersistente.getPeticionBatalla())) {
                        nombre = linea.Value.ToString();
                    }
                }
            }
        });

        //Indicar que estamos en batalla
        infoPersistente.setEnBatalla(true);

        //Guardar nombre oponente
        infoPersistente.setNombreOponente(nombre);

        //Mostrar el mensaje emergente de batalla
        ventanaBatalla.GetComponent<Batalla>().crear(nombre, infoPersistente.getPeticionBatalla());
        //A partir de la creacion del mensaje contar tiempo
        contarTiempo = true;
    }

    //-----------------------------------------------------------------------------------------------------

    /*
     * METODO PARA VOLVER A ESCUCHAR PETICIONES DE BATALLA
     */
    public void reiniciarLecturaBatallas() {
        tiempoAux = tiempoVisualizacion;

        //Ocultar ventana de notificacion de batalla
        ventanaBatalla.GetComponent<Batalla>().ocultar();
        //restablecer valor en el nodo de peticiones
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
        //Indicar que ya no hay mensaje creado
        contarTiempo = false;
        crearMensaje = true;
        //Indicar que no estamos en batalla
        infoPersistente.setEnBatalla(false);
    }
}