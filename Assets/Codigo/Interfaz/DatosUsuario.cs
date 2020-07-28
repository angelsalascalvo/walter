using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DatosUsuario : MonoBehaviour {
    //Referencias publicas
    public Text txtNombreUsu, txtEnergia, txtEnergia1, txtEnergia2;
    public RawImage imgFoto;
    public Animator animatorImagenWalter;

    //Variables privadas
    private FirebaseUser usuario;
    private InfoPersistente infoPersistente;
    private ReferenciaBaseDatos refBD;
    private string nombreUsuario, foto, energia;

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO QUE SE EJECUTA AL INICIAR EL SCRIPT
     */
    void Start() {
        //Obtener referencias
        refBD = GameObject.Find("ReferenciaBD").GetComponent<ReferenciaBaseDatos>();
        infoPersistente = GameObject.Find("InfoPersistente").GetComponent<InfoPersistente>();

        //Establecer los datos a null para comenzar
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/peticion").SetValueAsync("null");
        refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("batalla/respuesta").SetValueAsync("null");

        //Poner a la escucha el campo correspondiente para obtener todos los nombres de usuario ocupados de la base de datos
        refBD.getBaseDatos()
           .GetReference("usuarios/" + refBD.getUsuario().UserId)
           .ValueChanged += HandleValueChanged;

        //Llamada al metodo de actualizar foto por si ha cambiado respecto a la ultima conexion
        //actualizarFotoPerfil();

        //Establecer animacion del personaje
        animatorImagenWalter.SetTrigger(infoPersistente.getAparienciaActiva());
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
    * METODO QUE PERMANECE A LA ESCUCHA SOBRE CAMBIOS EN EL NODO DE LA INFORMACION DEL USUARIO
    */
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        //Leer base de datos
        IEnumerable<DataSnapshot> posLeida = args.Snapshot.Children;

        //Recorrer los resultado leidos y almacenar en las variables
        foreach (var linea in posLeida) {
            switch (linea.Key) {
                case "nombreUsu":
                    nombreUsuario = linea.Value.ToString();
                    txtNombreUsu.text = nombreUsuario;
                    infoPersistente.setMiNombre(nombreUsuario);
                    break;
                case "foto":
                    foto = linea.Value.ToString();
                    //Llamada al metodo para cargar la imagen desde la URL
                    StartCoroutine(cargarImagen(foto));
                    break;
                case "energia":
                    energia = linea.Value.ToString();
                    txtEnergia.text = energia;
                    txtEnergia1.text = energia;
                    txtEnergia2.text = energia;
                    break;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------

    /*
     * METODO PARA CARGAR UNA IMAGEN DESDE UNA URL
     */
    IEnumerator cargarImagen(string url) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            imgFoto.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    //------------------------------------------------------------------------------------------------------------------

    //public void actualizarFotoPerfil() {
    //    //Provisional
    //    refBD.getBaseDatos().GetReference("usuarios").Child(refBD.getUsuario().UserId).Child("foto").SetValueAsync("URL AQUI");
    //}
}