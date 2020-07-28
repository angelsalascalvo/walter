using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlInterfazAjustes : MonoBehaviour
{
    //Referencias publicas
    public Slider sSenMov; //Sensibibilad movimiento
    public Slider sSenApunt; //Sensibilidad apuntar
    public GameObject controlesModoA; //Derecha salto | Izquierda Apuntar
    public GameObject controlesModoB; //Derecha Apuntar | Izquierda Salto
    public GameObject ventanaEmergente;

    //Variables privadas
    private InfoPersistente infoPersistente;
    private ReferenciaBaseDatos refBD;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
    */
    void Start() {
        //Obtener referencias
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();

        //Llamada al metodo para obtener lo ajustes actuales del usuario
        obtenerAjustes();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA INTERFAZ CON LOS PARAMETROS ALMACENADOS EN FIREBASE
     */
    public void obtenerAjustes() {
        if (infoPersistente == null) {
            infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();
        }

        //Establecer el valor de los slider
        sSenMov.value = infoPersistente.getSenMov();
        sSenApunt.value = infoPersistente.getSenApunt();

        //Establecer el boton de control adecuado
        if (infoPersistente.getModoControl()) {
            controlesModoB.transform.GetChild(0).gameObject.SetActive(false);
            controlesModoA.transform.GetChild(0).gameObject.SetActive(true);

        } else {
            controlesModoB.transform.GetChild(0).gameObject.SetActive(true);
            controlesModoA.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL CAMBIAR LA SENSIBILIDAD DEL MOVIMIENTO
     */
    public void cambiadoSenMov() {
        //Obtener valor seleccionado
        int ajuste =(int) sSenMov.value;
        //Guardar el nuevo ajuste en el nodo de firebase
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("ajustes").Child("senMov").SetValueAsync(ajuste+"");
        //Cambiar directamente el ajuste en el objeto de informacion persistente
        infoPersistente.setSenMov(ajuste);
        
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL CAMBIAR LA SENSIBILIDAD DE APUNTAR
    */
    public void cambiadoSenApunt() {
        //Obtener valor seleccionado
        int ajuste = (int)sSenApunt.value;
        //Guardar el nuevo ajuste en el nodo de firebase
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("ajustes").Child("senApunt").SetValueAsync(ajuste+"");
        //Cambiar directamente el ajuste en el objeto de informacion persistente
        infoPersistente.setSenApunt(ajuste);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL PULSAR EL BOTON CORRESPONDIENTE CON EL CONTROL EN MODO A (DERECHA SALTAR | IZQUIERDA DISPARAR)
    */
    public void cambiadoModoControlA() {
        //Guardar el nuevo ajuste en el nodo de firebase
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("ajustes").Child("modoControl").SetValueAsync("a");
        //Cambiar directamente el ajuste en el objeto de informacion persistente
        infoPersistente.setModoControl("a");
        //Marcar el borde correspondiente
        controlesModoB.transform.GetChild(0).gameObject.SetActive(false);
        controlesModoA.transform.GetChild(0).gameObject.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA AL PULSAR EL BOTON CORRESPONDIENTE CON EL CONTROL EN MODO A (DERECHA DISPARAR | IZQUIERDA SALTAR)
    */
    public void cambiadoModoControlB() {
        //Guardar el nuevo ajuste en el nodo de firebase
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("ajustes").Child("modoControl").SetValueAsync("b");
        //Cambiar directamente el ajuste en el objeto de informacion persistente
        infoPersistente.setModoControl("b");
        //Marcar el borde correspondiente
        controlesModoB.transform.GetChild(0).gameObject.SetActive(true);
        controlesModoA.transform.GetChild(0).gameObject.SetActive(false);
    }  

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR INFORMACION SOBRE EL DESARROLLADOR DE LA APLICACION AL PULSAR EL BOTON "ACERCA DE"
     */
    public void botonAcercaDe() {
        string texto = "Aplicación desarrollada por Ángel Salas Calvo bajo la marca personal Rutillas Toby";
        ventanaEmergente.GetComponent<VentanaInformacion>().mostrarInfo(texto, "CERRAR");
    }
}