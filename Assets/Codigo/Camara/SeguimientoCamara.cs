using UnityEngine;

public class SeguimientoCamara : MonoBehaviour {

    //Ajustes Publicos
    public Vector2 posicionMin, posicionMax; //Posiciones extremas hasta las que puede llegar la camara
    public float suavidadMovimiento;
    public float margenCamVert;
    public Camera camara;
    public GameObject panelInicioBatalla;
    public InstanciadorJugadores instanciadorJugadores;
    public Animator animatorPortal1;
    public Animator animatorPortal2;

    //Variables privadas
    private GameObject miPersonaje = null;
    private Vector2 aux; //Variable necesaria para calculos internos del metodo SmoothDamp que permite suavizar el movimiento de camara
    private float posX, posY;
    private bool cinematicaIniCompletada, unicaEjecucion, unicaEjecucion2;
    private float tiempAnimacionIni, tiempEsperaAbrirPortal;
    private float t = 0f;
    private float velocidadCam = 0.3f;
    private Vector3 posCamaraIni, posCamaraFin;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        panelInicioBatalla.gameObject.SetActive(true);
        tiempAnimacionIni = 5;
        tiempEsperaAbrirPortal = 3;
        cinematicaIniCompletada = false;
        unicaEjecucion = true;
        unicaEjecucion2 = true;
        posCamaraIni = camara.gameObject.transform.position;
        posCamaraFin = new Vector3(0, 0, 0);
    }


    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void FixedUpdate() {

        //Comprobar si se ha cumplido el tiempo previo al movimiento establecido
        if (tiempAnimacionIni >= 0) {
            tiempAnimacionIni -= Time.deltaTime;

        //Comprobar si se ha obtenido el personaje para empezar el movimiento de la camara
        } else if (miPersonaje != null) {

            //Ocultar el panel de batalla
            panelInicioBatalla.gameObject.SetActive(false);

            //Obtener la posicion final de la camara si no la tenemos ya
            if (posCamaraFin == new Vector3(0, 0, 0)) {
                //La posicion de la camara final será la del personaje teniendo en cuenta el margen de Y, y los limites de posicion de la camara
                posCamaraFin = new Vector3(
                    Mathf.Clamp(miPersonaje.transform.position.x, posicionMin.x, posicionMax.x),
                    Mathf.Clamp(miPersonaje.transform.position.y + margenCamVert, posicionMin.y, posicionMax.y),
                    gameObject.transform.position.z
                );
            }

            //Efectuar movimiento suave de camara
            if (t < 1) {
                t += Time.deltaTime * velocidadCam;
                camara.orthographicSize = Mathf.Lerp(20f, 5f, t);
                camara.gameObject.transform.position = Vector3.Lerp(posCamaraIni, posCamaraFin, t);
            }

            //Controlar cuando finaliza el desplazamiento de la camara
            if (camara.gameObject.transform.position == posCamaraFin) {

                //Completar tiempo espera para abrir el portal
                if (tiempEsperaAbrirPortal >= 0) {
                    tiempEsperaAbrirPortal -= Time.deltaTime;

                } else {
                    if (unicaEjecucion) {
                        unicaEjecucion = false;
                        animatorPortal1.SetTrigger("Abrir");
                        animatorPortal2.SetTrigger("Abrir");
                    }
                    //Comprobar si ha terminado la animacion para comenzar con la cuenta atras
                    else if (animatorPortal1.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
                        && animatorPortal2.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) {

                        if (unicaEjecucion2) {
                            unicaEjecucion2 = false;
                            //Indicar que el personaje esta preparado
                            miPersonaje.GetComponent<EstadoJugador>().preparado();
                            cinematicaIniCompletada = true;
                        }
                    }
                }
            }
        }

        //Si no se ha obtenido el personaje se busca su referencia
        if (miPersonaje == null) {
            miPersonaje = GameObject.Find("miPersonaje");
        }

        //Si se ha obtenido el personaje y la cinematica inicial de movimiento de la camara se ha completado, comenzaremos a seguir al personaje
        if (miPersonaje != null && cinematicaIniCompletada) {
            //Obtener la posicion del personaje de forma suavizada
            posX = Mathf.SmoothDamp(gameObject.transform.position.x, miPersonaje.transform.position.x, ref aux.x, suavidadMovimiento);
            posY = Mathf.SmoothDamp(gameObject.transform.position.y, miPersonaje.transform.position.y + margenCamVert, ref aux.y, suavidadMovimiento);

            //Cambiar la posicion de la camara en funcion de la del personaje y limitando a los margenes de la misma
            gameObject.transform.position = new Vector3(
                Mathf.Clamp(posX, posicionMin.x, posicionMax.x),
                Mathf.Clamp(posY, posicionMin.y, posicionMax.y),
                gameObject.transform.position.z
            );
        }
    }
}