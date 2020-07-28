using UnityEngine;
using UnityEngine.EventSystems;

public class ControlesPantalla : MonoBehaviour
{
    //Ajustes publicos
    public bool modo=true; //True (Derecha Salto - Izquierda Brazo) | False (Derecha Brazo - Izquierda Salto)
    public float sensibilidad=2.0f; //Sensibilidad del movimiento de brazo, (1.0 Nada Sensible | 4.0 Muy Sensible)
    public float rotBrazoLimit = 50f; //Rotacion limite del brazo en positivo y negativo

    //Referencias publicas
    public Movimiento mov;
    public Disparar arma;
    public GameObject brazo;
    public Transform PuntoSoltarPlanta;

    //Variables privadas
    private float puntoPartida, rotacionInicial, pxDesplazados, equivalencia, tamControl, gradosMover;
    private int valorY;
    private int dedoControl=-1;
    private bool inicioInvert=false, bloqueadoSalto=true, bloqueadoBrazo=true;
    private bool soltadoSalto; //Variable para bloquear el salto multiple al dejar pulsada la pantalla
    private EstadoJugador estadoJugador;
    private GestionPlantas gestionPlantas;

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    void Awake() {
        estadoJugador = GetComponent<EstadoJugador>();
        gestionPlantas = GameObject.Find("ControladorJuego").GetComponent<GestionPlantas>();
        //Al inicio el bloqueado brazo y salto
        bloqueadoSalto = true;
        bloqueadoBrazo = true;
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    private void Start(){
        //Obtener el tamaño del control en funcion de la sensibilidad indicada
        tamControl = Screen.height / sensibilidad;
        //Obtener el numero de pixeles que se necesitan desplazar para rotar un grado
        equivalencia = tamControl / (rotBrazoLimit * 2);
       
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update()
    {
        foreach (Touch tactil in Input.touches)
        {
            //Si al pulsar el jugador acaba de reaparecer (estado 4) se desactiva
            if (GetComponent<EstadoJugador>().getEstado() == 4)
                GetComponent<EstadoJugador>().setEstado(2); // Establecer el estado con arma

            //Si se pulsa una parte de la UI no hacemos nada(interfaz grafica)
            if (EventSystem.current.IsPointerOverGameObject() ||
                EventSystem.current.currentSelectedGameObject != null) {
                return;
            }            

            //Comprobar la parte de la pantalla que se toca para ejecutar una acción
            //Teniendo en cuenta el modo de control de juego seleccionado
            if ((tactil.position.x <= (Screen.width / 2) && modo) || (tactil.position.x > (Screen.width / 2) && !modo)) {

                //Si el brazo esta bloqueado no hacemos nada
                if (bloqueadoBrazo && estadoJugador.getEstado()!=3)
                    return;
                //Comproobar si el brazo esta bloqueado porque llevamos la planta
                else if (bloqueadoBrazo && estadoJugador.getEstado() == 3) {
                    //============================== Soltar Planta ===============================//
                    estadoJugador.setEstado(2); //Establecer estado de disparar
                    //Llamada al metodo para soltar la planta al escenario
                    gestionPlantas.soltarPlanta(PuntoSoltarPlanta.position);
                    return;
                }

                //==========================  Movimiento Brazo ========================//

                //Cuando se toca la pantalla (BEGAN)
                if (tactil.phase == TouchPhase.Began) {
                    if (dedoControl == -1) {
                        inicioInvert = mov.getInvertido();//Marcar si en el inicio esta invertido el personaje
                        dedoControl = tactil.fingerId;
                        puntoPartida = tactil.position.y; //Obtener la posicion del primer toque en la pantalla
                        rotacionInicial = brazo.GetComponent<Transform>().eulerAngles.z; //Obtener la rotacion inicial del brazo
                    }
                }

                //Si aun NO hemos soltado el dedo
                if (tactil.phase != TouchPhase.Ended) {
                    if (tactil.fingerId == dedoControl) {
                        pxDesplazados = tactil.position.y - puntoPartida; //Calculamos la distancia en pixeles recorrida entre punto actual e inicial
                        gradosMover = pxDesplazados / equivalencia; //Calculamos en función de los pixeles recorridos los grados de rotación a los que equivale

                        //Obtener la rotacion en negativo en lugar de en grados mayores a 180
                        if (rotacionInicial > 180)
                            rotacionInicial = rotacionInicial - 360;

                        /*
                        * ROTAR EL BRAZO
                        * Efectuar el movimiento del barzo en funcion de como este orientado el personaje 
                        * y como estaba en el inicio del toque
                        */
                        if (mov.getInvertido() && !inicioInvert) {
                            //Personaje invertido y no se ha iniciado invertido
                            //Invertimos la suma de la rotacion inicial mas los grados a mover
                            brazo.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp((rotacionInicial + gradosMover) * -1, -rotBrazoLimit, rotBrazoLimit));
                        } else if (mov.getInvertido() && inicioInvert) {
                            //Personaje invertido y se ha iniciado invertido
                            //Rotacion inicial menos los grados a mover
                            brazo.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(rotacionInicial - gradosMover, -rotBrazoLimit, rotBrazoLimit));
                        } else if (!mov.getInvertido() && inicioInvert) {
                            //Personaje no esta invertido y se ha iniciado invertido
                            //Invertimos la resta de la rotacion inicial menos los grados a mover
                            brazo.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp((rotacionInicial - gradosMover) * -1, -rotBrazoLimit, rotBrazoLimit));
                        } else {
                            //Personaje no esta invertido y no se ha iniciado invertido
                            //Rotacion inicial mas los grados a mover
                            brazo.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(rotacionInicial + gradosMover, -rotBrazoLimit, rotBrazoLimit));
                        }
                    }
                } else {
                    //==========================  Disparo  ========================
                    if (tactil.fingerId == dedoControl) {
                        dedoControl = -1;
                        arma.disparo(mov.getInvertido());
                    }
                }
            }
            //Si se toca la otra parte de la pantalla se ejecuta el salto
            else {
                //==========================  Salto  ========================
                //Si el salto esta bloqueado no hacemos nada
                if (bloqueadoSalto)
                    return;

                mov.Saltar(); //Llamada al metodo para que salte el personaje
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL ESTADO DEL MOVIMIENTO DEL BRAZO
     */
    public void setBloqueadoBrazo(bool b) {
        bloqueadoBrazo = b;
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL ESTADO DEL SALTO
     */
    public void setBloqueadoSalto(bool b) {
        bloqueadoSalto = b;
    }

    //----------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA OBTENER LA POSICION EN LA QUE SE DEBE DE SOLTAR LA PLANTA EN POSESION
     */
    public Transform getPuntoSoltarPlanta() {
        return PuntoSoltarPlanta;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL MODO DE CONTROL
     */
    public void setModo(bool m) {
        modo = m;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA SENSIBILIDAD DEL MOVIMIENTO DEL BRAZO
     */
    public void setSensibilidad (float s){
          sensibilidad = s;
    }
}