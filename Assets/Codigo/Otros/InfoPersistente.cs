using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InfoPersistente : MonoBehaviour {
    //Referencias publicas
    public ReferenciaBaseDatos refBD;

    //Variables privadas
    private float senMov = 0, senApunt = 0;
    private string modoControl = "";
    private List<string> aparienciasDisponibles = new List<string>();
    private string aparienciaActiva="", peticionBatalla="", respuestaBatalla="";
    private bool obtSenMov, obtSenApunt, obtModoControl, obtAparienciaActiva, obtAparienciasDisponibles, obtEnergia;
    private bool todoObtenido, enBatalla;
    private bool jugando;
    private int energia;
    private string miNombre, nombreOponente;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        //Hacer un objeto persistente entre las diferentes escenas (evitar destruccion)
        DontDestroyOnLoad(gameObject);

        //Iniciar variables
        todoObtenido = false;
        enBatalla = false;
        jugando = false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LOS METODO DE ESCUCHA CONTINUAOS DE LOS NODOS CON INFORMACION NECESARIA DE LA BASE DE DATOS
     */
    public void obtenerDatosContinuos() {
        //Poner a la escucha el campo correspondiente para obtener la informacion continua
        refBD.getBaseDatos()
        .GetReference("usuarios/" + refBD.getUsuario().UserId)
        .ValueChanged += HandleValueChanged;        
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE PERMANECE CONSTANTEMENTE A LA ESCUCHA DE CAMBIOS SOBRE EL NODO PRINCIPAL DEL USUARIO DE LA BASE DE DATOS
     */
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        //Reiniciar listado de apariencias
        aparienciasDisponibles = new List<string>();

        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        IEnumerable<DataSnapshot> posLeida = args.Snapshot.Children;
        //Recorrer los resultado leidos
        foreach (var linea in posLeida) {

            // LEIDO NODO ENERGIA
            if (linea.Key.ToString().ToLower().Equals("energia")) {
                energia = Int32.Parse(linea.Value.ToString());
                obtEnergia = true;
                Debug.Log("Energia" + energia);

            // LEIDO NODO APARIENCIAS
            } else if (linea.Key.ToString().ToLower().Equals("apariencias")) {
                IEnumerable<DataSnapshot> listApariencias = linea.Children;
                foreach (var apariencia in listApariencias) {
                    aparienciasDisponibles.Add(apariencia.Key.ToString().ToLower());
                    Debug.Log("Apariencia" + apariencia.Key);
                }
                obtAparienciasDisponibles = true;

            // LEIDO NODO APARIENCIA ACTIVA
            } else if (linea.Key.ToString().Equals("aparienciaActiva")) {
                aparienciaActiva = linea.Value.ToString().ToLower();
                obtAparienciaActiva = true;

            // LEIDO NODO BATALLA
            } else if (linea.Key.ToString().Equals("batalla")) {
                //Detener lectura de peticiones si estamos en una batalla
                if (jugando == false) {
                    IEnumerable<DataSnapshot> batalla = linea.Children;
                    foreach (var nodoBatalla in batalla) {
                        //Comprobar si es una peticion de batalla 
                        if (nodoBatalla.Key.ToString().Equals("peticion"))
                            peticionBatalla = nodoBatalla.Value.ToString();
                        //Comprobar si es una respuesta a una peticion
                        else if (nodoBatalla.Key.ToString().Equals("respuesta"))
                            respuestaBatalla = nodoBatalla.Value.ToString();
                    }
                }

            // LEIDO NODO AJUSTES
            } else if (linea.Key.ToString().Equals("ajustes")) {
                IEnumerable<DataSnapshot> ajustes = linea.Children;

                //Recorrer los resultado leidos del usuario
                foreach (var nodoAjuste in ajustes) {
                    //En funcion del ajuste leido, se guardará en la variable correspondiente
                    if (nodoAjuste.Key.ToString().Equals("senMov")) {
                        senMov = float.Parse(nodoAjuste.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        obtSenMov = true;
                    } else if (nodoAjuste.Key.ToString().Equals("senApunt")) {
                        senApunt = float.Parse(nodoAjuste.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        obtSenApunt = true;
                    } else if (nodoAjuste.Key.ToString().Equals("modoControl")) {
                        modoControl = nodoAjuste.Value.ToString().ToLower();
                        obtModoControl = true;
                    }
                }
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INDICAR SI TODA LA INFORMACION SE HA OBTENIDO
     */
    public void marcarInfoObtenida() {
        //Comprobar si se ha obtenido cada una de la informacion
        Debug.Log(""+obtSenMov + obtSenApunt + obtModoControl + obtAparienciaActiva + obtAparienciasDisponibles + obtEnergia);
        if (obtSenMov && obtSenApunt && obtModoControl && obtAparienciaActiva && obtAparienciasDisponibles && obtEnergia)
            todoObtenido = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA SENSIBILIDAD DE MOVIMIENTO
     */
    public float getSenMov() {
        return senMov;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA SENSIBILIDAD DE APUNTAR
     */
    public float getSenApunt() {
        return senApunt;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA SENSIBILIDAD DE MOVIMIENTO
     */
    public void setSenMov(float ajuste) {
        senMov = ajuste;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA SENSIBILIDAD DE APUNTAR
     */
    public void setSenApunt(float ajuste) {
        senApunt = ajuste;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL MODO DE CONTROL DE LOS AJUSTES
     */
    public bool getModoControl() {
        if (modoControl.Equals("a"))
            return true;

        return false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL MODO DE CONTROL DE LOS AJUSTES
     */
    public void setModoControl(string ajuste) {
        modoControl = ajuste;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL LISTADO DE APARIENCIAS ACTIVAS
     */
    public List<string> getAparienciasDisponibles(){
        return aparienciasDisponibles;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA APARIENCIA ACTIVA 
     */
    public string getAparienciaActiva() {
        return aparienciaActiva;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA APARIENCIA ACTIVA
     */
    public void setAparienciaActiva(string apariencia) {
        aparienciaActiva = apariencia;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI SE HA OBTENIDO TODA LA INFORMACION DEL USUARIO
     */
    public bool isObtenidoTodo() {
        marcarInfoObtenida();
        return todoObtenido;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA ENERGIA
     */
    public int getEnergia() {
        return energia;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA PETICION DE UNA BATALLA
     */
    public string getPeticionBatalla() {
        return peticionBatalla;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA PETICION DE UNA BATALLA
     */
    public void setPeticionBatalla(string p) {
        peticionBatalla = p;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA RESPUESTA DE UNA BATALLA
     */
    public string getRespuestaBatalla() {
        return respuestaBatalla;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA RESPUESTA A UNA SOLICITUD DE UNA BATALLA
     */
    public void setRespuestaBatalla(string respuesta) {
        respuestaBatalla=respuesta;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL NOMBRE DE USUARIO DEL PERSONAJE
     */
    public string getMiNombre() {
        return miNombre;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER MI NOMBRE DE USUARIO
     */
    public void setMiNombre(string nombre) {
        miNombre = nombre;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER EL NOMBRE DEL OPONENTE DE UNA BATALLA
     */
    public string getNombreOponente() {
        return nombreOponente;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL NOMBRE DEL OPONENTE
     */
    public void setNombreOponente(string nombre) {
        nombreOponente = nombre;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA COMPROBAR SI ESTAMOS EN UNA BATALLA
     */
    public bool isEnBatalla() {
        return enBatalla;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER SI ESTAMOS EN UNA BATALLA
     */
    public void setEnBatalla(bool estado) {
        enBatalla = estado;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER SI ESTAMOS JUGANDO
     */
    public void setJugando(bool j) {
        jugando = j;
    }
}