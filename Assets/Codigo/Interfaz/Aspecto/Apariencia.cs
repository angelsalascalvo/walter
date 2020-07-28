using System.Collections;
using UnityEngine;

public class Apariencia : MonoBehaviour {

    //Ajustes publicos
    public string nombreAspecto;
    public int precioApariencia;
    public GameObject panelBloqueado;
    public GameObject borde;
    public Animator animatorImagen1, animatorImagen2;

    //Variables privadas
    private InfoPersistente infoPersistente;
    private ReferenciaBaseDatos refBD;
    private GameObject[] apariencias;
    private GameObject ventanaEmergente;
    private bool bloqueado;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        apariencias = GameObject.FindGameObjectsWithTag("apariencia");
        ventanaEmergente = GameObject.Find("pApariencia").GetComponent<GestionApariencias>().getCanvasEmergente();

        //Inicializar estado
        panelBloqueado.gameObject.SetActive(true);
        bloqueado = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA CUANDO SE PULSA SOBRE UN ASPECTO
    */
    public void bApariencia() {
        //Comprobar si la apariencia esta bloqueada
        if (bloqueado) {
            if (infoPersistente.getEnergia() >= precioApariencia) {
                //Iniciamos proceso de compra
                solicitarComprar();
            } else {
                //Mostrar mensaje saldo insuficiente
                string texto = "No tienes suficiente energia para comprar esta apariencia";
                ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "Aceptar");
            }

        } else {
            //Guardar la interfaz activa
            refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("aparienciaActiva").SetValueAsync(nombreAspecto);
            infoPersistente.setAparienciaActiva(nombreAspecto);
            //Establecer nueva apariencia en la imagen
            animatorImagen1.SetTrigger(nombreAspecto);
            animatorImagen2.SetTrigger(nombreAspecto);

            //Desmarcar todos los objetos
            for (int i = 0; i < apariencias.Length; i++) {
                apariencias[i].GetComponent<Apariencia>().desmarcar();
            }
            //Marcar objeto como activo
            marcar();
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL PULSAR SOBRE UN ASPECTO QUE TENEMOS BLOQUEADO
    */
    public void solicitarComprar() {
        string texto = "¿Confirmas la compra?";
        //Llamada al metodo que muestra la ventana de confirmación
        VentanaConfirmacion ventana = ventanaEmergente.GetComponent<VentanaConfirmacion>().crearConfirmacion(texto, "Comprar", "Cancelar");
        //Lanzar evento en segundo plano (hilo aparte) que comprobará cuando se pulse una de las 2 opciones del cuadro de confirmacion
        StartCoroutine(ConfirmarCompra(ventana));
    }

    //===============>

    /*
    * METODO QUE SE EJECUTA EN UN HILO DIFERENTE PARA ESPERAR LA CONFIRMCION DEL MENSAJE
    */
    IEnumerator ConfirmarCompra(VentanaConfirmacion ventana) {
        //Esperar mientras no se seleccione ninguna opcion
        do {
            yield return null;
        } while (ventana.getAceptado() == 0);

        //Solo ejecutaremos la accion de borrado si se acepta la confirmación (aceptado = 1)
        if (ventana.getAceptado() == 1) {
            comprar();
        }
    }

    //===============>

    /*
    * METODO QUE SE EJECUTA CUANDO SE CONFIRMA EL MENSAJE
    */
    public void comprar() {
        //Agregar Apariencia al nodo de la base de datos para que este disponible
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("apariencias").Child(nombreAspecto).SetValueAsync("disponible");
        //Restar el precio a la energia de la base de datos
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("energia").SetValueAsync("" + (infoPersistente.getEnergia() - precioApariencia));
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA DESBLOQUEAR UNA APARIENCIA QUE TENEMOS COMPRADA
    */
    public void desbloquearApariencia() {
        //Ocultar el panel de bloqueo
        panelBloqueado.gameObject.SetActive(false);
        bloqueado = false;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA BLOQUEAR UNA APARIENCIA QUE NO TENEMOS COMPRADA
    */
    public void bloquearApariencia() {
        //Mostrar el panel de bloqueo
        panelBloqueado.gameObject.SetActive(true);
        bloqueado = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO OCULTA EL BORDE INDICADOR DE SELECCION DE UNA APARIENCIA
     */
    public void desmarcar() {
        borde.gameObject.SetActive(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL PULSAR SOBRE UNA APARIENCIA QUE TENEMOS DISPONIBLE, MUESTRA EL BORDE INDICADOR DE SELECCION
     */
    public void marcar() {
        borde.gameObject.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA OBTENER EL NOMBRE DE LA APARIENCIA
    */
    public string getNombre() {
        return nombreAspecto;
    }
}