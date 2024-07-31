
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    

    //Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(Weapon.ShotRecoilResults recoil)
    {
        targetRotation += new Vector3(recoil.rotationRecoil.x, Random.Range(-recoil.rotationRecoil.y, recoil.rotationRecoil.y), Random.Range(-recoil.rotationRecoil.z, recoil.rotationRecoil.z));
    }
}
