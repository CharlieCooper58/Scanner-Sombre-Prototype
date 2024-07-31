using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshotParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem focusedParticles;
    [SerializeField] ParticleSystem spreadParticles;

    public void FireGunParticles(Vector3 start, Vector3 end)
    {
        var main = focusedParticles.main;
        float distance = Vector3.Distance(start, end);

        main.startSpeed = distance / (0.1f*main.startLifetime.constant);
        transform.rotation = Quaternion.LookRotation(end - start);
        focusedParticles.Play();
        spreadParticles.Play();
        StartCoroutine("DestroyThis");
    }
    private IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
