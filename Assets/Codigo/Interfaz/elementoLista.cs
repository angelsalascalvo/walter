using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ElementoLista : MonoBehaviour
{
    //Referencias Publicas
    public RawImage imgFoto;
    public Text nombre;
    public GameObject botonesAgregar;
    public GameObject botonesMisAmigos;
    public GameObject botonesSolicitud;
    public GameObject botonesPeticion;

    //Variables Privadas
    private string uid;
    private GameObject ventanaEmergente;
    private ReferenciaBaseDatos refBD;
    private InfoPersistente infoPersistente;
    private SolicitarBatalla solicitarBatalla;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INICIALIZAR LOS DATOS DEL ELEMENTO CON EL OBJETO USUARIO PASADO POR PARAMETRO
     */
    public void crear(Amigo amigo, string tipoElemento, GameObject ventana) {
        nombre.text = amigo.nombre;
        uid = amigo.codigoUid;
        ventanaEmergente = ventana;
        //Llamada al metodo para cargar la imagen del usuario a partir de la url
        StartCoroutine(cargarImagen(amigo.urlFoto));

        //En función del tipo de elemento que deseamos mostrar ocultaremos o mostraremos unos paneles
        switch (tipoElemento) {
            case "solicitud":
                botonesAgregar.gameObject.SetActive(false);
                botonesMisAmigos.gameObject.SetActive(false);
                botonesSolicitud.gameObject.SetActive(true);
                botonesPeticion.gameObject.SetActive(false);
                break;
            case "agregar":
                botonesAgregar.gameObject.SetActive(true);
                botonesMisAmigos.gameObject.SetActive(false);
                botonesSolicitud.gameObject.SetActive(false);
                botonesPeticion.gameObject.SetActive(false);
                break;
            case "peticion":
                botonesAgregar.gameObject.SetActive(false);
                botonesMisAmigos.gameObject.SetActive(false);
                botonesSolicitud.gameObject.SetActive(false);
                botonesPeticion.gameObject.SetActive(true);
                break;
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA INICIALIZAR LOS DATOS DEL ELEMENTO CON EL OBJETO USUARIO PASADO POR PARAMETRO
    */
    public void crearMiAmigo(Amigo amigo, GameObject ventana, SolicitarBatalla solicitar) {
        nombre.text = amigo.nombre;
        uid = amigo.codigoUid;
        ventanaEmergente = ventana;
        solicitarBatalla = solicitar;

        //Llamada al metodo para cargar la imagen del usuario a partir de la url
        StartCoroutine(cargarImagen(amigo.urlFoto));

        //Mostrar y ocultar los paneles
        botonesAgregar.gameObject.SetActive(false);
        botonesMisAmigos.gameObject.SetActive(true);
        botonesSolicitud.gameObject.SetActive(false);
        botonesPeticion.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA CARGAR UNA IMAGEN DESDE UNA URL
    */
    IEnumerator cargarImagen(string url) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            imgFoto.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //    METODOS PARA EL BOTON DE BORRADO DE UN AMIGO, SOLICITANDO CONFIRMACIÓN PARA REALIZAR LA ACCION     //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
     * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE BORRAR, MUESTRA DIALOGO DE CONFIRMACIÓN
     */
    public void botonBorrarAmigo() {
        string parte1 = "¿Estas seguro de que quieres dejar de ser amigo de ", parte2 = "?";
        //Llamada al metodo que muestra la ventana de confirmación
        VentanaConfirmacion ventana = ventanaEmergente.GetComponent<VentanaConfirmacion>().crearConfirmacion(parte1+nombre.text+parte2, "Aceptar", "Cancelar");
        //Lanzar evento en segundo plano (hilo aparte) que comprobará cuando se pulse una de las 2 opciones del cuadro de confirmacion
        StartCoroutine(ConfirmarBorrarAmigo(ventana));
    }

    //========>

    /*
     * METODO CO-ROUTINE QUE COMPROBARÁ EN TODO MOMENTO SI SE HA SELECCIONADO ALGUNA OPCION DEL CUADRO DE CONFIRMACIÓN
     * SE EJECUTARÁ EN UNA TAREA/HILO DIFERENTE PARA NO DETENER TODA LA EJECUCION
     * ACTUARÁ EN FUNCION DE LA OPCIÓN QUE SE SELECCIONE
     */
    IEnumerator ConfirmarBorrarAmigo(VentanaConfirmacion ventana) {
        //Esperar mientras no se seleccione ninguna opcion
        do { 
            yield return null;
        } while (ventana.getAceptado() == 0);
        
        //Solo ejecutaremos la accion de borrado si se acepta la confirmación (aceptado = 1)
        if (ventana.getAceptado() == 1) {
            borrarAmigo();
        }
    }

    //========>

    /*
     * METODO PARA EFECTUAR LA PROPIA ELIMINACIÓN DE LA AMISTAD EN AMBOS USUARIOS AL PULSAR EN ACEPTAR DEL MENSAJE EMERGENTE
     */
    public void borrarAmigo() {
        //Eliminar la entrada de la relacion para los 2 usuarios
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("relaciones").Child(uid).RemoveValueAsync();
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("relaciones").Child(refBD.getUsuario().UserId).RemoveValueAsync();
    }

    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                               METODOS PARA EL BOTON DE AGREGAR UN AMIGO                               //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
     * METODO QUE AGREGARA LA SOLICITUD AL NODO DEL USUARIO EN CUESTION Y LA PETICION A NUESTRO NODO
     */
    public void botonAgregar() {
        string texto = "Se ha enviado una peticion de amistad al usuario ";
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("relaciones").Child(uid).SetValueAsync("peticion");
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("relaciones").Child(refBD.getUsuario().UserId).SetValueAsync("solicitud");
        ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto+nombre.text, "Aceptar");
    }
    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                        METODOS PARA EL BOTON DE CANCELAR UNA PETICION DE AMISTAD                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
    * METODO QUE CANCELA UNA SOLICITUD ENVIADA A UN NODO USUARIO Y LA PETICION DE NUESTRO NODO
    */
    public void botonCancelarPeticion() {
        string texto = "Se ha retirado la peticion de amistad enviada al usuario ";
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("relaciones").Child(uid).RemoveValueAsync();
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("relaciones").Child(refBD.getUsuario().UserId).RemoveValueAsync();
        ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto + nombre.text, "Aceptar");
    }
    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                        METODOS PARA EL BOTON DE ACEPTAR UNA SOLICITUD DE AMISTAD                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
    * METODO QUE ESTABLECE COMO AMIGOS A DOS USUARIOS A TRAVES DEL NODO DE RELACION DE LOS DIFERENTES USUARIOS
    */
    public void botonAceptarSolicitud() {
        string texto = "Ahora eres amigo de ";
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("relaciones").Child(uid).SetValueAsync("amigo");
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("relaciones").Child(refBD.getUsuario().UserId).SetValueAsync("amigo");
        ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto + nombre.text, "Aceptar");
    }

    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                        METODOS PARA EL BOTON DE RECHAZAR UNA SOLICITUD DE AMISTAD                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
    * METODO QUE SE ELIMINA EL NODO DE RELACION DE DE AMBOS USUARIOS
    */
    public void botonRechazarSolicitud() {
        string texto = "Se ha descartado la solicitud de ";
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("relaciones").Child(uid).RemoveValueAsync();
        refBD.getBaseDatos().GetReference("usuarios").Child(uid).Child("relaciones").Child(refBD.getUsuario().UserId).RemoveValueAsync();
        ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto + nombre.text, "Aceptar");
    }

    //------------------------------------------------------------------------------------------------------------------

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                        METODOS PARA EL BOTON DE RETAR A UNA BATALLA A UN AMIGO                        //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    /*
    * METODO QUE SE EJECUTA AL PULSAR EL BOTON DE BATALLA PARA RETAR A UNA BATALLA A UN AMIGO
    */
    public void botonBatalla() {
        solicitarBatalla.solicitar(uid);
        //Guardar nombre oponente para mostrarlo mas adelante
        infoPersistente.setNombreOponente(nombre.text);
    }
}