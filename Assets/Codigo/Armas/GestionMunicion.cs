using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestionMunicion : MonoBehaviour
{
    //Ajustes publicos
    public float minCajasMunicion = 1;//Numero minimo de municion que puede aparecer
    public float maxCajasMunicion = 3;//Numero minimo de municion que puede aparecer
    public int minBalasPistola, maxBalasPistola; //Intervalo de balas que puede tener la municion

    //Referencias publicas
    public PhotonView pView;
    public GameObject objMunicion;

    //Variables privadas
    private int idCaja=0; //Identificador para cada una de las cajas de municion

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INICIAR LA GENERACION DE MUNICION
     */
    public void generar() {
        //Iniciar array con las posiciones en las que puede aparecer la municion
        Vector3[] posicionesMunicion = new[] {new Vector3(-26.475f,-9.488f,0),
                                                new Vector3(43.67f, -12.71f, 0),
                                                new Vector3(-14.02694f, -7.356795f, 0),
                                                new Vector3(-41.62f, 0.66f, 0),
                                                new Vector3(-36.03f, 0.67f, 0),
                                                new Vector3(-57.62627f, -2.822383f, 0),
                                                new Vector3(-81.154f, 0.121f, 0),
                                                new Vector3(-84.71f, 4.73f, 0),
                                                new Vector3(-1.2f, 1.16f, 0),
                                                new Vector3(25.56004f, -0.7677503f, 0),
                                                new Vector3(57.02794f, 3.154418f, 0),
                                                new Vector3(27.06319f, -3.202271f, 0),
                                                new Vector3(-77.14986f, -5.497224f, 0),
                                                new Vector3(51, 2.97f, 0),
                                                new Vector3(-6.476f, -1.046f, 0)};
        //Llamada al metodo que genera la municion
        generarCajas(posicionesMunicion);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER POSICIONES Y CANTIDAD DE CAJAS DE MUNICION DE FORMA ALEATORIA
     */
    public void generarCajas(Vector3[] posicionesMunicion) {
        //Listado con las posiciones del array utilizadas
        List<int> posicionesUsadas = new List<int>();

        //Numero de municiones que vamos a crear de forma aleatoria entre el rango
        int contMuniciones = (int)Random.Range(minCajasMunicion, maxCajasMunicion + 1);

        //Generar el numero de municiones establecido
        for (int i = 0; i < contMuniciones; i++) {
            bool valido;
            int posicion;//Posicion del array donde generar municion

            //Repetir hasta obtener posicion libre
            do {
                valido = true;
                posicion = Random.Range(0, posicionesMunicion.Length);
                //Comprobar si se ha generado municion en esa posicion
                for (int j = 0; j < posicionesUsadas.Count; j++) {
                    if (posicionesUsadas[j] == posicion)
                        valido = false;
                }
            } while (!valido);

            //Generar Objeto en la posicion determinada 
            generarMunicion(posicionesMunicion[posicion]);
            //Annadir la posicion utilizada
            posicionesUsadas.Add(posicion);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA GENERAR UNA CAJA DE MUNICION EN LA POSICION INDICADA CON UN NUMERO DE BALAS ALEATORIO
     */
    public void generarMunicion(Vector3 posicion) {
        //Establecer de forma aleatoria el numero de balas de la caja de municion
        int numBalas = Random.Range(minBalasPistola, maxBalasPistola + 1);
        //Crear la caja con el RPC(Para crearlas en todos los dispositivos)
        pView.RPC("instanciarCaja_RPC", Photon.Pun.RpcTarget.AllBuffered, posicion, numBalas);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA INSTANCIAR UNA CAJA EN TODOS LOS CLIENTE CON LAS MISMAS PROPIEDADES
     */
    [Photon.Pun.PunRPC]
    public void instanciarCaja_RPC(Vector3 posicion, int numBalas) {
        GameObject objCreado = Instantiate(objMunicion, posicion, Quaternion.identity);
        //crear objeto con el id indicado por nombre
        objCreado.name = "caja" + idCaja;
        //Aumentar el identificador de caja en 1;
        idCaja++;
        //Establecer el numero de balas a la caja
        objCreado.GetComponent<Municion>().establecerPropiedades(numBalas);
        //Ocultar por defecto los numeros
        objCreado.GetComponent<Municion>().estadoNumeros(false);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA DESACTIVAR UNA CAJA DE MUNICION
     */
    public void desactivarCaja(string id) {
        //Llamada al metodo RPC para desactivar la caja (realizar la acción en todos los clientes)
        pView.RPC("desactivarCaja_RPC", Photon.Pun.RpcTarget.AllBuffered, id);
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA LA DESACTIVACION DE LA CAJA DE MUNICION
     */
    [Photon.Pun.PunRPC]
    public void desactivarCaja_RPC(string id) {
        //Buscar mediante el ID la caja de municion creada
        GameObject cajaMunicion = GameObject.Find(id);
        //Cambiar apariencia de la misma
        cajaMunicion.GetComponent<Municion>().txtNumBalas.text = "X";
        cajaMunicion.GetComponent<Animator>().SetTrigger("Abrir");
        //Deshabilitar el collider
        cajaMunicion.GetComponent<Collider2D>().enabled = false;
    }
}