using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // (opcional, si luego quieres cargar otro nivel)

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Nivel completado!");
            // Efecto visual
            GetComponent<MeshRenderer>().material.color = Color.green;

            // Esperar un segundo antes de cambiar de escena
            StartCoroutine(NextLevel());
        }

        IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("Level2");
        }
    }
}
