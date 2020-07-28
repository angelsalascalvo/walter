using UnityEngine;

public class Movimiento : MonoBehaviour
{
    //Ajustes publicos
    public float sensibilidadMovi = 18f; //Sensibilidad de movimiento respecto a la inclinacion del dispositivo
    public float velocidadMaxMovi = 5f; //Velocidad máxima a la que el personaje puede moverse
    public float velocidadMaxEspalda = 2.8f; //Velocidad a partir de la cual se gira el jugador en el sentido de la inclinación
    public float fuerzaSalto = 10f;

    //Variables privadas
    private bool enSuelo, cercaSuelo;
    private bool vivo, bloqueado;
    private bool invertido; //Indica orientación horizontal del personaje 
    private float desplazamiento; //Desplazamiento en el eje X que tendra Walter

    //Referencias publicas
    public Transform comprobadorSuelo;
    public LayerMask maskSuelo;
    public ControlAnimaciones controlAnimaciones;


    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIO DE LA EJECUCIÓN PARA REALIZAR REFERENCIAS
     */
    private void Awake(){
        vivo = true;
        bloqueado = true;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA REPETIDAMENTE CADA FOTOGRAMA DE LA EJECUCION
     */
    void Update(){
        //Si el movimiento esta bloqueado salimos
        if (bloqueado) {
            return;
        } 

        //En función de la inclinacion del dispositivo aumentará o disminuirá la velocidad de desplazamiento limitando al maximo establecido
        desplazamiento = Mathf.Clamp(Input.acceleration.x * sensibilidadMovi, -velocidadMaxMovi, velocidadMaxMovi); 

        //Controlar la direccion del sprite (derecha o izquierda) (Teniendo en cuenta la velocidad a partir de la cual se gira para no caminar de espaldas)
        if (desplazamiento >= velocidadMaxEspalda)
        {
            transform.localScale = new Vector3(1, 1, 1);
            invertido = false;
        }
        else if (desplazamiento < -velocidadMaxEspalda )
        {
            transform.localScale = new Vector3(-1, 1, 1);
            invertido = true;
        }

        //Animaciones movimiento en funcion de la velocidad del personaje
        float despAbs = Mathf.Abs(desplazamiento); //Las animaciones serán independientes a la horientacion (derecha o izuierda)

        //Para ejecutar la animaciones comprobamos que este vivo y en el suelo
        if (enSuelo && vivo)
        {
            if (despAbs > 0.4 && despAbs < 3.5)
            {

                controlAnimaciones.activarAnim("Caminar");
                
            }
            else if (despAbs <= 0.4)
            {

                controlAnimaciones.activarAnim("QuietoPerfil");

            }
            else if (despAbs >= 3.5)
            {
                controlAnimaciones.activarAnim("Correr");
                
            }
        }

        //Hacer que no sea muy sensible en el punto de detenido (cuando se pone recto el dispositivo)
        if (despAbs >= 0.5) {
            GetComponent<Rigidbody2D>().velocity = new Vector2(desplazamiento, GetComponent<Rigidbody2D>().velocity.y); //Aplicar velocidad en el eje x


        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA CADA CIERTO TIEMPO FIJO
     */
    public void FixedUpdate()
    {
        //Comprobar si estamos tocando el suelo
        enSuelo = Physics2D.OverlapCircle(comprobadorSuelo.position, 0.2f, maskSuelo);

        if (enSuelo == false) {
            controlAnimaciones.desactivarAnim("Correr");
            controlAnimaciones.desactivarAnim("Caminar");
            controlAnimaciones.desactivarAnim("Quieto");
            controlAnimaciones.desactivarAnim("QuietoPerfil");
            
        }

        //Comprobar si estamos llegando al suelo
        cercaSuelo = Physics2D.OverlapArea(new Vector2(comprobadorSuelo.position.x, GetComponent<Transform>().position.y), 
                                            new Vector2(comprobadorSuelo.position.x, comprobadorSuelo.position.y-3), maskSuelo);

        //Si estamos bajando y estamos cerca del suelo ejecutamos la animacion de final del salto
        if (GetComponent<Rigidbody2D>().velocity.y < 0 && cercaSuelo) {
            controlAnimaciones.desactivarAnim("SaltoInicio");
            controlAnimaciones.activarAnim("SaltoFinal");
        } else {
            controlAnimaciones.desactivarAnim("SaltoFinal");
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA HACER QUE EL PERSONAJE SALTE
     */
    public void Saltar(){
        //Comprobar si esta en el suelo para poder saltar
        if (enSuelo)
        {
            

            //Activar animacion de salto
            controlAnimaciones.activarAnim("SaltoInicio");

            //Aplicar la velocidad de salto
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, fuerzaSalto); //Aplicar velocidad en el eje y
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE DEVUELVE SI EL PERSONAJE ESTA INVERTIDO O NO
     * (Si esta mirando a derecha o izquierda)
     */
    public bool getInvertido() {
        return invertido;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA INCDICAR SI EL PERSONAJE ESTA VIVO O NO
     */
    public void setVivo(bool v) {
        vivo = v;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER EL BLOQUEO DE MOVIMIENTO DEL PERSONAJE
     */
    public void setBloqueado(bool b){
        bloqueado = b;
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA ESTABLECER LA SENSIBILIDAD DE MOVIMIENTO
     */
    public void setSensibilidadMovi(float sens) {
        sensibilidadMovi = sens;
    }
}