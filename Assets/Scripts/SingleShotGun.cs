using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam; // Reference to the camera for shooting direction

    PhotonView PV; // PhotonView component for network synchronization

    void Awake()
    {
        PV = GetComponent<PhotonView>(); // Get the PhotonView component attached to this GameObject
    }

    public override void Use()
    {
        Shoot(); // Call the Shoot method when the gun is used
    }

    void Shoot()
    {
        // Create a ray from the center of the camera's viewport
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position; // Set the ray's origin to the camera's position

        // Perform a raycast to detect if it hits any colliders
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If the hit object is damageable, apply damage
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            // Call the RPC to synchronize the shoot effect across the network
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        // Check for colliders in the vicinity of the hit position
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            // Instantiate the bullet impact effect at the hit position
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 3f); // Destroy the impact object after 3 seconds
            bulletImpactObj.transform.SetParent(colliders[0].transform); // Set the impact object as a child of the hit collider
        }
    }
}
