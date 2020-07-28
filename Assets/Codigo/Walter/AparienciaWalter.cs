using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AparienciaWalter : MonoBehaviour
{
    //Referencias publicas
    [Header("SOBREESCRITURAS DE ANIMACIONES")]
    public AnimatorOverrideController bros;
    public AnimatorOverrideController smoking;

    [Header("")]
    public PhotonView pView;
    public Animator animator;

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO PARA ESTABLECER LAS ANIMACIONES ASOCIADAS A LA APARIENCIA ACTIVA DEL JUGADOR
    */
    public void establecerApariencia(string apariencia) {

        //Cambiar apariencia para mi
        switch (apariencia) {
            case "bros":
                animator.runtimeAnimatorController = bros;
                break;
            case "smoking":
                animator.runtimeAnimatorController = smoking;
                break;
        }

        //Cambiar mi apariencia en el juego del oponente
        if (pView.IsMine) {
            pView.RPC("establecerApariencia_RPC", Photon.Pun.RpcTarget.OthersBuffered, apariencia);
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO RPC PARA ESTABLECER LA APARIENCIA DE MI PERSONAJE EN EL CONTRINCANTE
     */
    [Photon.Pun.PunRPC]
    public void establecerApariencia_RPC(string apariencia) {

        //Metodo que se ejecuta en el dispositivo del oponente
        //Actuamos si NO es el personaje del oponente, es decir, si es el enemigo
        if (pView.IsMine == false) {
            if (animator == null) {
                animator = gameObject.GetComponent<Animator>();
            }

            //Cambiar animaciones en funcion del nombre
            switch (apariencia) {
                case "bros":
                    animator.runtimeAnimatorController = bros;
                    break;
                case "smoking":
                    animator.runtimeAnimatorController = smoking;
                    break;
            }
        }
    }
}