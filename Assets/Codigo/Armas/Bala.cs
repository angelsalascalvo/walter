using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Bala : MonoBehaviour
{
    //Ajustes publicos
    public int danno=10;  

    //Variables privadas
    private float tiempoVida=0;
    private float tiempoMaxVida=5;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
    */
    public void Update() {
        //Destruir las balas si no colisionan con ningun objeto en un periodo de tiempo
        tiempoVida += Time.deltaTime;
        if (tiempoVida >= tiempoMaxVida) {
            Destroy(gameObject);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA DETECTAR LAS COLISIONES DE ESTE OBJETO
     */
    void OnCollisionEnter2D(Collision2D collision){
        //Comprobar si se ha colisionado con el jugador
        if (collision.gameObject.tag == "Player"){
            //Reducir la vida del jugador
            collision.gameObject.GetComponent<EstadoJugador>().reducirVida(danno);
        }
        
        //Destruir la bala al colisionar con cualquier objeto
        Destroy(gameObject);
    }
}