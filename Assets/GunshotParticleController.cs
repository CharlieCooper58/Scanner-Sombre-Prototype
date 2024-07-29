using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunshotParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem focusedParticles;
    [SerializeField] ParticleSystem spreadParticles;

    public void FireGunParticles(float distance)
    {
        var main = focusedParticles.main;
        main.startSpeed = distance / (0.1f*main.startLifetime.constant);

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
