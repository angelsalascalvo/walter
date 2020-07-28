using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlAnimaciones : MonoBehaviour
{
    //Variables privadas
    private Animator animator; //Animador del personaje

    //Referencias publicas
    public EstadoJugador estadoJugador;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        animator = GetComponent<Animator>();
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ACTIVAR UNA ANIMACION Y DESACTIVAR EL RESTO
    */
    public void activarAnim(string animacion) {
        //Activar la animación
        animator.SetTrigger(animacion);

        //Si se encuentra en el estado 1 (sin arma) mostramos los brazos de movimiento en funcion de la animacion principal
        if (estadoJugador.getEstado() == 1) { 
            switch (animacion) {
                case "QuietoPerfil":
                    animator.SetTrigger("BrazosQuietoPerfil");
                    break;
                case "Caminar":
                    animator.SetTrigger("BrazosCaminar");
                    break;
                case "Correr":
                    animator.SetTrigger("BrazosCorrer");
                    break;
                case "SaltoInicio":
                    animator.SetTrigger("BrazosInicioSalto");
                    break;
                case "SaltoFinal":
                    animator.SetTrigger("BrazosFinalSalto");
                    break;
            }
        }
        //Si se encuentra en el estado 3 (sin arma) mostramos los brazos de movimiento en funcion de la animacion principal
        else if (estadoJugador.getEstado() == 3) {
            animator.SetTrigger("BrazosPlanta");
        } 
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA DESACTIVAR EL TRIGGER DE UNA ANIMACION PASADA POR PARAMETRO
    */
    public void desactivarAnim(string animacion) {
        animator.ResetTrigger(animacion);
    }

    //-----------------------------------------------------------------------

    /*
    * METODO PARA OCULTAR LOS BRAZOS CON ARMA Y MOSTRAR LOS DE MOVIMIENTO
    */
    public void sinArma() {
        animator.SetTrigger("QuietoPerfil");
        //Activar la capa de los brazos
        animator.SetLayerWeight(1, 1);
    }

    //--------------------------------------------------------------------------------

    /*
     * METODO PARA MOSTRAR LOS BRAZOS CON ARMA Y OCULTAR LOS DE MOVIMIENTO
     */
    public void conArma() {
        //Activar la capa de los brazos
        animator.SetLayerWeight(1, 0);

        //Desactivar animaciones de brazos
        animator.ResetTrigger("BrazosCorrer");
        animator.ResetTrigger("BrazosCaminar");
        animator.ResetTrigger("BrazosQuietoPerfil");
        animator.ResetTrigger("BrazosPlanta");
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ACTIVAR LA ANIMACION DE DISPARAR
    */
    public void animacionDisparar() {
        animator.SetTrigger("Disparar");
    }
}