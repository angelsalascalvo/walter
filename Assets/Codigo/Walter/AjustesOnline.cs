using Photon.Pun;

public class AjustesOnline : MonoBehaviourPun {

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        //Comprobar si el personaje corresponde con el que yo he instanciado yo para controlarlo o no
        if (!photonView.IsMine) {
            //Establecer nombre al prefab enemigo
            gameObject.name = "personajeEnemigo";

            //Destruir los script con la funcionalidad adecuada
            if (GetComponent<Movimiento>() != null)
                Destroy(GetComponent<Movimiento>());

            if (GetComponent<ControlesPantalla>() != null)
                Destroy(GetComponent<ControlesPantalla>());

            if (GetComponent<DetectorColisiones>() != null)
                Destroy(GetComponent<DetectorColisiones>());

        } else {
            //Si soy yo establezco el nombre al objeto
            gameObject.name = "miPersonaje";
        }
    }
}